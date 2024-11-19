using _3DModelExchange;
using CommonResources;
using Commons;
using MasterDetailModel;
using MasterDetailView;
using Model;
using PrezzariWpf.View;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PrezzariWpf
{
    /// <summary>
    /// Interaction logic for SelectArticoliWindow.xaml
    /// </summary>
    public partial class SelectCapitoliIdsWindow : Window
    {
        CapitoliView CapitoliView { get { return CapitoliCtrl.DataContext as CapitoliView; } }

        public ClientDataService DataService { get; set; } = null;//ref
        public ModelActionsStack ModelActionsStack { get; set; } = null;//ref
        public IEntityWindowService WindowService { get; set; } = null;//ref
        public IMainOperation MainOperation { get; set; } = null;//ref

        /// <summary>
        /// Items selezionati (checkati) alla chiusura o apertura del dialogo
        /// </summary>
        public List<Guid> CapitoliItemSelectedIds { get; set; } = new List<Guid>();


        /// <summary>
        /// POssibilità di selezionare solo un elemento
        /// </summary>
        public bool IsSingleSelection { get; set; } = false;

        /// <summary>
        /// Possibilità di assegnare nessun articolo
        /// </summary>
        public bool AllowNoSelection { get; set; } = false;

        /// <summary>
        /// Impostazioni di Filtro, Ordine e Raggruppamento all'avvio del dialogo 
        /// </summary>
        public EntityTypeViewSettings CurrentViewSettings { get; set; } = new EntityTypeViewSettings();

        public NoteCalculatorFunction NoteCalculatorFunction { get; set; } = null;
        public EPCalculatorFunction EPCalculatorFunction { get; set; } = null;

        public SelectCapitoliIdsWindow()
        {
            InitializeComponent();
            base.Closed += SelectCapitoliIdsWindow_Closed;
        }

        private void SelectCapitoliIdsWindow_Closed(object sender, EventArgs e)
        {
            CapitoliView.ItemsView.AllowNoSelection = false;
        }

        public void Init()
        {
            CapitoliCtrl.Init();
            
            CapitoliView.CalculatorFunctions.Add(NoteCalculatorFunction);
            CapitoliView.CalculatorFunctions.Add(EPCalculatorFunction);


            CapitoliView.DataService = DataService;
            CapitoliView.WindowService = WindowService;
            CapitoliView.ModelActionsStack = ModelActionsStack;
            CapitoliView.MainOperation = MainOperation;
            //CapitoliView.Init(CurrentViewSettings);
            
            CapitoliView.Init(null);//per ora non imposto filtri e ordinamento all'apertura del dialogo

            CapitoliView.ItemsView.RightPanesView.ClosePanes();

            HashSet<Guid> capitoliItemSelectedIds = new HashSet<Guid>(CapitoliItemSelectedIds);
            if (capitoliItemSelectedIds.Contains(Guid.Empty))
            {
                CapitoliView.ItemsView.SetNoSelectionChecked(true);
                capitoliItemSelectedIds.Remove(Guid.Empty);
            }
            else
                CapitoliView.ItemsView.SetNoSelectionChecked(false);

            CapitoliView.ItemsView.CheckedEntitiesId = new HashSet<Guid>(capitoliItemSelectedIds);

            CapitoliView.ItemsView.AllowNoSelection = AllowNoSelection;
            //CapitoliView.ItemsView.SetNoSelectionChecked(true);
            CapitoliView.ItemsView.IsSingleSelection = IsSingleSelection;
            CapitoliView.ItemsView.IsImportItemsEnabled = false;

            Guid currentCapitoliId = CapitoliItemSelectedIds.FirstOrDefault();

            CapitoliView.ItemsView.SelectEntityById(currentCapitoliId);
           
        }

        /// <summary>
        /// Uscita dal dialogo con pulsante con spunta
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            AcceptSelection();

        }

        /// <summary>
        /// Uscita dal dialogo con doppio click su un'entità
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CapitoliCtrl_EntityViewMouseDoubleClick(object sender, EventArgs e)
        {
            CapitoliView.ItemsView.CheckedEntitiesId = new HashSet<Guid>() { CapitoliView.ItemsView.SelectedEntityId };
            AcceptSelection();
        }


        private void AcceptSelection()
        {
            //N.B. non so bene perche alla chiusura della finestra questo evento a volte parta 2 volte
            if (DialogResult != null)
                return;

            //prendo solo gli elementi con prezzo (foglie)
            IEnumerable<Guid> selectedItemsId = CapitoliView.ItemsView.CheckedEntitiesId.Where(item => !CapitoliView.ItemsView.HasChildren(item));

            UpdateViewSettings();

            if (IsSingleSelection)
            {

                if (selectedItemsId.Count() == 1)
                {
                    CapitoliItemSelectedIds = selectedItemsId.ToList<Guid>();
                    DialogResult = true;
                }
                else if (selectedItemsId.Count() > 1)
                {
                    CapitoliView.ItemsView.MainOperation.ShowMessageBarView(LocalizationProvider.GetString("NonPossibileSelezionarePiuDiUnaVoce"));
                    DialogResult = false;
                }
                else
                {
                    if (AllowNoSelection && CapitoliView.ItemsView.IsNoSelectionChecked)
                    {
                        CapitoliItemSelectedIds = new List<Guid>() { Guid.Empty };
                        DialogResult = true;
                    }
                    else
                    {
                        MainOperation.ShowMessageBarView(LocalizationProvider.GetString("OccorreSelezionareUnArticolo"));
                        DialogResult = false;
                    }
                }


            }
            else
            {
                CapitoliItemSelectedIds = selectedItemsId.ToList<Guid>();
                DialogResult = true;
            }
        }

        private void UpdateViewSettings()
        {


            //if (CapitoliView.ItemsView.RightPanesView.GroupView.Data != null && CapitoliView.ItemsView.RightPanesView.GroupView.Data.Items.Any())
            //    CurrentViewSettings.Groups = CapitoliView.ItemsView.RightPanesView.GroupView.Data.Items.Select(item => item.Clone()).ToList();

            //if (CapitoliView.ItemsView.RightPanesView.SortView.Data != null && CapitoliView.ItemsView.RightPanesView.SortView.Data.Items.Any())
            //    CurrentViewSettings.Sorts = CapitoliView.ItemsView.RightPanesView.SortView.Data.Items.Select(item => item.Clone()).ToList();

            //if (CapitoliView.ItemsView.RightPanesView.FilterView.Data != null && CapitoliView.ItemsView.RightPanesView.FilterView.Data.Items.Any())
            //    CurrentViewSettings.Filters = CapitoliView.ItemsView.RightPanesView.FilterView.Data.Items.Select(item => item.Clone()).ToList();

            CapitoliView.ItemsView.RightPanesView.UpdateViewSettings(CurrentViewSettings);

        }


    }
}
