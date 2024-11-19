using _3DModelExchange;
using AttivitaWpf.View;
using CommonResources;
using Commons;
using MasterDetailModel;
using MasterDetailView;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AttivitaWpf
{
    /// <summary>
    /// Interaction logic for SelectWBSIdsWindow.xaml
    /// </summary>
    public partial class SelectWBSIdsWindow : Window
    {
        WBSView WBSView { get { return WBSCtrl.DataContext as WBSView; } }

        public ClientDataService DataService { get; set; } = null;//ref
        public ModelActionsStack ModelActionsStack { get; set; } = null;//ref
        public IEntityWindowService WindowService { get; set; } = null;//ref
        public IMainOperation MainOperation { get; set; } = null;//ref

        /// <summary>
        /// Items selezionati (checkati) alla chiusura o apertura del dialogo
        /// </summary>
        public List<Guid> WBSItemSelectedIds { get; set; } = new List<Guid>();


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

        public SelectWBSIdsWindow()
        {
            InitializeComponent();
            base.Closed += SelectWBSIdsWindow_Closed;
        }

        private void SelectWBSIdsWindow_Closed(object sender, EventArgs e)
        {
            WBSView.ItemsView.AllowNoSelection = false;
        }

        public void Init()
        {
            WBSCtrl.Init();

            WBSView.CalculatorFunctions.Add(NoteCalculatorFunction);
            WBSView.CalculatorFunctions.Add(EPCalculatorFunction);


            WBSView.DataService = DataService;
            WBSView.WindowService = WindowService;
            WBSView.ModelActionsStack = ModelActionsStack;
            WBSView.MainOperation = MainOperation;
            //WBSView.Init(CurrentViewSettings);

            WBSView.Init(null);//per ora non imposto filtri e ordinamento all'apertura del dialogo

            WBSView.ItemsView.RightPanesView.ClosePanes();

            HashSet<Guid> wbsItemSelectedIds = new HashSet<Guid>(WBSItemSelectedIds);
            if (wbsItemSelectedIds.Contains(Guid.Empty))
            {
                WBSView.ItemsView.SetNoSelectionChecked(true);
                WBSItemSelectedIds.Remove(Guid.Empty);
            }
            else
                WBSView.ItemsView.SetNoSelectionChecked(false);

            WBSView.ItemsView.CheckedEntitiesId = new HashSet<Guid>(wbsItemSelectedIds);

            WBSView.ItemsView.AllowNoSelection = AllowNoSelection;
            //WBSView.ItemsView.SetNoSelectionChecked(true);
            WBSView.ItemsView.IsSingleSelection = IsSingleSelection;
            WBSView.ItemsView.IsImportItemsEnabled = false;

            Guid currentWBSId = WBSItemSelectedIds.FirstOrDefault();

            WBSView.ItemsView.SelectEntityById(currentWBSId);

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
        private void WBSCtrl_EntityViewMouseDoubleClick(object sender, EventArgs e)
        {
            WBSView.ItemsView.CheckedEntitiesId = new HashSet<Guid>() { WBSView.ItemsView.SelectedEntityId };
            AcceptSelection();
        }


        private void AcceptSelection()
        {
            //N.B. non so bene perche alla chiusura della finestra questo evento a volte parta 2 volte
            if (DialogResult != null)
                return;

            //prendo solo gli elementi con prezzo (foglie)
            IEnumerable<Guid> selectedItemsId = WBSView.ItemsView.CheckedEntitiesId.Where(item => !WBSView.ItemsView.HasChildren(item));

            UpdateViewSettings();

            if (IsSingleSelection)
            {

                if (selectedItemsId.Count() == 1)
                {
                    WBSItemSelectedIds = selectedItemsId.ToList<Guid>();
                    DialogResult = true;
                }
                else if (selectedItemsId.Count() > 1)
                {
                    WBSView.ItemsView.MainOperation.ShowMessageBarView(LocalizationProvider.GetString("NonPossibileSelezionarePiuDiUnaVoce"));
                    DialogResult = false;
                }
                else
                {
                    if (AllowNoSelection && WBSView.ItemsView.IsNoSelectionChecked)
                    {
                        WBSItemSelectedIds = new List<Guid>() { Guid.Empty };
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
                WBSItemSelectedIds = selectedItemsId.ToList<Guid>();
                DialogResult = true;
            }
        }

        private void UpdateViewSettings()
        {


            //if (WBSView.ItemsView.RightPanesView.GroupView.Data != null && WBSView.ItemsView.RightPanesView.GroupView.Data.Items.Any())
            //    CurrentViewSettings.Groups = WBSView.ItemsView.RightPanesView.GroupView.Data.Items.Select(item => item.Clone()).ToList();

            //if (WBSView.ItemsView.RightPanesView.SortView.Data != null && WBSView.ItemsView.RightPanesView.SortView.Data.Items.Any())
            //    CurrentViewSettings.Sorts = WBSView.ItemsView.RightPanesView.SortView.Data.Items.Select(item => item.Clone()).ToList();

            //if (WBSView.ItemsView.RightPanesView.FilterView.Data != null && WBSView.ItemsView.RightPanesView.FilterView.Data.Items.Any())
            //    CurrentViewSettings.Filters = WBSView.ItemsView.RightPanesView.FilterView.Data.Items.Select(item => item.Clone()).ToList();

            WBSView.ItemsView.RightPanesView.UpdateViewSettings(CurrentViewSettings);

        }
    }
}
