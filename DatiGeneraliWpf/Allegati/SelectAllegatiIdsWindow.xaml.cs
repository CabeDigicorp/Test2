using _3DModelExchange;
using DatiGeneraliWpf.View;
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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DatiGeneraliWpf
{
    /// <summary>
    /// Interaction logic for SelectAllegatiIdsWindow.xaml
    /// </summary>
    public partial class SelectAllegatiIdsWindow : Window
    {
        AllegatiView AllegatiView { get { return AllegatiCtrl.DataContext as AllegatiView; } }

        public ClientDataService DataService { get; set; } = null;//ref
        public ModelActionsStack ModelActionsStack { get; set; } = null;//ref
        public IEntityWindowService WindowService { get; set; } = null;//ref
        public IMainOperation MainOperation { get; set; } = null;//ref

        /// <summary>
        /// Items selezionati (checkati) alla chiusura o apertura del dialogo
        /// </summary>
        public List<Guid> AllegatiItemSelectedIds { get; set; } = new List<Guid>();

        /// <summary>
        /// POssibilità di selezionare solo un elemento
        /// </summary>
        public bool IsSingleSelection { get; set; } = false;

        /// <summary>
        /// Possibilità di assegnare nessun articolo
        /// </summary>
        public bool AllowNoSelection { get; set; } = false;

        /// <summary>
        /// Filtro attivo all'avvio del dialogo
        /// </summary>
        public EntityTypeViewSettings ViewSettings = null;

        /// <summary>
        /// Impostazioni di Filtro, Ordine e Raggruppamento all'avvio del dialogo 
        /// </summary>
        public EntityTypeViewSettings CurrentViewSettings { get; set; } = new EntityTypeViewSettings();

        public NoteCalculatorFunction NoteCalculatorFunction { get; set; } = null;
        public EPCalculatorFunction EPCalculatorFunction { get; set; } = null;

        public SelectAllegatiIdsWindow()
        {
            InitializeComponent();

            DataContext = new AllegatiView();

            base.Closed += SelectContattiIdsWindow_Closed;
        }

        private void SelectContattiIdsWindow_Closed(object sender, EventArgs e)
        {
            AllegatiView.ItemsView.AllowNoSelection = false;
        }

        public void Init()
        {
            AllegatiCtrl.Init();

            AllegatiView.CalculatorFunctions.Add(NoteCalculatorFunction);
            AllegatiView.CalculatorFunctions.Add(EPCalculatorFunction);


            AllegatiView.DataService = DataService;
            AllegatiView.WindowService = WindowService;
            AllegatiView.ModelActionsStack = ModelActionsStack;
            AllegatiView.MainOperation = MainOperation;

            AllegatiView.Init(ViewSettings);

            AllegatiView.ItemsView.RightPanesView.ClosePanes();

            HashSet<Guid> elencoAttivitaItemSelectedIds = new HashSet<Guid>(AllegatiItemSelectedIds);
            if (elencoAttivitaItemSelectedIds.Contains(Guid.Empty))
            {
                AllegatiView.ItemsView.SetNoSelectionChecked(true);
                elencoAttivitaItemSelectedIds.Remove(Guid.Empty);
            }
            else
                AllegatiView.ItemsView.SetNoSelectionChecked(false);

            AllegatiView.ItemsView.CheckedEntitiesId = new HashSet<Guid>(elencoAttivitaItemSelectedIds);
            AllegatiView.ItemsView.AllowNoSelection = AllowNoSelection;
            AllegatiView.ItemsView.IsSingleSelection = IsSingleSelection;
            AllegatiView.ItemsView.IsImportItemsEnabled = false;

            Guid currentContattiId = AllegatiItemSelectedIds.FirstOrDefault();
            AllegatiView.ItemsView.SelectEntityById(currentContattiId);

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
        private void AllegatiCtrl_EntityViewMouseDoubleClick(object sender, EventArgs e)
        {
            AllegatiView.ItemsView.CheckedEntitiesId = new HashSet<Guid>() { AllegatiView.ItemsView.SelectedEntityId };
            AcceptSelection();
        }


        private void AcceptSelection()
        {
            //N.B. non so bene perche alla chiusura della finestra questo evento a volte parta 2 volte
            if (DialogResult != null)
                return;

            //prendo solo gli elementi foglie
            IEnumerable<Guid> selectedItemsId = AllegatiView.ItemsView.CheckedEntitiesId;//.Where(item => !ContattiView.ItemsView.HasChildren(item));

            UpdateViewSettings();

            if (IsSingleSelection)
            {

                if (selectedItemsId.Count() == 1)
                {
                    AllegatiItemSelectedIds = selectedItemsId.ToList<Guid>();
                    DialogResult = true;
                }
                else if (selectedItemsId.Count() > 1)
                {
                    AllegatiView.ItemsView.MainOperation.ShowMessageBarView(LocalizationProvider.GetString("NonPossibileSelezionarePiuDiUnaVoce"));
                    DialogResult = false;
                }
                else
                {
                    if (AllowNoSelection && AllegatiView.ItemsView.IsNoSelectionChecked)
                    {
                        AllegatiItemSelectedIds = new List<Guid>() { Guid.Empty };
                        DialogResult = true;
                    }
                    else
                    {
                        MainOperation.ShowMessageBarView(LocalizationProvider.GetString("OccorreSelezionareUnAttivita"));
                        DialogResult = false;
                    }
                }


            }
            else
            {
                AllegatiItemSelectedIds = selectedItemsId.ToList<Guid>();
                DialogResult = true;
            }
        }

        private void UpdateViewSettings()
        {


            //if (AllegatiView.ItemsView.RightPanesView.GroupView.Data != null && AllegatiView.ItemsView.RightPanesView.GroupView.Data.Items.Any())
            //    CurrentViewSettings.Groups = AllegatiView.ItemsView.RightPanesView.GroupView.Data.Items.Select(item => item.Clone()).ToList();

            //if (AllegatiView.ItemsView.RightPanesView.SortView.Data != null && AllegatiView.ItemsView.RightPanesView.SortView.Data.Items.Any())
            //    CurrentViewSettings.Sorts = AllegatiView.ItemsView.RightPanesView.SortView.Data.Items.Select(item => item.Clone()).ToList();

            //if (AllegatiView.ItemsView.RightPanesView.FilterView.Data != null && AllegatiView.ItemsView.RightPanesView.FilterView.Data.Items.Any())
            //    CurrentViewSettings.Filters = AllegatiView.ItemsView.RightPanesView.FilterView.Data.Items.Select(item => item.Clone()).ToList();

            AllegatiView.ItemsView.RightPanesView.UpdateViewSettings(CurrentViewSettings);

        }
    }
}
