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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AttivitaWpf
{
    /// <summary>
    /// Interaction logic for SelectElencoAttivitaIdsWindow.xaml
    /// </summary>
    public partial class SelectElencoAttivitaIdsWindow : Window
    {
        ElencoAttivitaView ElencoAttivitaView { get { return ElencoAttivitaCtrl.DataContext as ElencoAttivitaView; } }

        public ClientDataService DataService { get; set; } = null;//ref
        public ModelActionsStack ModelActionsStack { get; set; } = null;//ref
        public IEntityWindowService WindowService { get; set; } = null;//ref
        public IMainOperation MainOperation { get; set; } = null;//ref

        /// <summary>
        /// Items selezionati (checkati) alla chiusura o apertura del dialogo
        /// </summary>
        public List<Guid> ElencoAttivitaItemSelectedIds { get; set; } = new List<Guid>();

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

        public SelectElencoAttivitaIdsWindow()
        {
            InitializeComponent();

            DataContext = new ElencoAttivitaView();

            base.Closed += SelectContattiIdsWindow_Closed;
        }

        private void SelectContattiIdsWindow_Closed(object sender, EventArgs e)
        {
            ElencoAttivitaView.ItemsView.AllowNoSelection = false;
        }

        public void Init()
        {
            ElencoAttivitaCtrl.Init();

            ElencoAttivitaView.CalculatorFunctions.Add(NoteCalculatorFunction);
            ElencoAttivitaView.CalculatorFunctions.Add(EPCalculatorFunction);


            ElencoAttivitaView.DataService = DataService;
            ElencoAttivitaView.WindowService = WindowService;
            ElencoAttivitaView.ModelActionsStack = ModelActionsStack;
            ElencoAttivitaView.MainOperation = MainOperation;

            ElencoAttivitaView.Init(ViewSettings);

            ElencoAttivitaView.ItemsView.RightPanesView.ClosePanes();

            HashSet<Guid> elencoAttivitaItemSelectedIds = new HashSet<Guid>(ElencoAttivitaItemSelectedIds);
            if (elencoAttivitaItemSelectedIds.Contains(Guid.Empty))
            {
                ElencoAttivitaView.ItemsView.SetNoSelectionChecked(true);
                elencoAttivitaItemSelectedIds.Remove(Guid.Empty);
            }
            else
                ElencoAttivitaView.ItemsView.SetNoSelectionChecked(false);

            ElencoAttivitaView.ItemsView.CheckedEntitiesId = new HashSet<Guid>(elencoAttivitaItemSelectedIds);
            ElencoAttivitaView.ItemsView.AllowNoSelection = AllowNoSelection;
            ElencoAttivitaView.ItemsView.IsSingleSelection = IsSingleSelection;
            ElencoAttivitaView.ItemsView.IsImportItemsEnabled = false;

            Guid currentContattiId = ElencoAttivitaItemSelectedIds.FirstOrDefault();
            ElencoAttivitaView.ItemsView.SelectEntityById(currentContattiId);

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
        private void ElencoAttivitaCtrl_EntityViewMouseDoubleClick(object sender, EventArgs e)
        {
            ElencoAttivitaView.ItemsView.CheckedEntitiesId = new HashSet<Guid>() { ElencoAttivitaView.ItemsView.SelectedEntityId };
            AcceptSelection();
        }


        private void AcceptSelection()
        {
            //N.B. non so bene perche alla chiusura della finestra questo evento a volte parta 2 volte
            if (DialogResult != null)
                return;

            //prendo solo gli elementi foglie
            IEnumerable<Guid> selectedItemsId = ElencoAttivitaView.ItemsView.CheckedEntitiesId;//.Where(item => !ContattiView.ItemsView.HasChildren(item));

            UpdateViewSettings();

            if (IsSingleSelection)
            {

                if (selectedItemsId.Count() == 1)
                {
                    ElencoAttivitaItemSelectedIds = selectedItemsId.ToList<Guid>();
                    DialogResult = true;
                }
                else if (selectedItemsId.Count() > 1)
                {
                    ElencoAttivitaView.ItemsView.MainOperation.ShowMessageBarView(LocalizationProvider.GetString("NonPossibileSelezionarePiuDiUnaVoce"));
                    DialogResult = false;
                }
                else
                {
                    if (AllowNoSelection && ElencoAttivitaView.ItemsView.IsNoSelectionChecked)
                    {
                        ElencoAttivitaItemSelectedIds = new List<Guid>() { Guid.Empty };
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
                ElencoAttivitaItemSelectedIds = selectedItemsId.ToList<Guid>();
                DialogResult = true;
            }
        }

        private void UpdateViewSettings()
        {


            //if (ElencoAttivitaView.ItemsView.RightPanesView.GroupView.Data != null && ElencoAttivitaView.ItemsView.RightPanesView.GroupView.Data.Items.Any())
            //    CurrentViewSettings.Groups = ElencoAttivitaView.ItemsView.RightPanesView.GroupView.Data.Items.Select(item => item.Clone()).ToList();

            //if (ElencoAttivitaView.ItemsView.RightPanesView.SortView.Data != null && ElencoAttivitaView.ItemsView.RightPanesView.SortView.Data.Items.Any())
            //    CurrentViewSettings.Sorts = ElencoAttivitaView.ItemsView.RightPanesView.SortView.Data.Items.Select(item => item.Clone()).ToList();

            //if (ElencoAttivitaView.ItemsView.RightPanesView.FilterView.Data != null && ElencoAttivitaView.ItemsView.RightPanesView.FilterView.Data.Items.Any())
            //    CurrentViewSettings.Filters = ElencoAttivitaView.ItemsView.RightPanesView.FilterView.Data.Items.Select(item => item.Clone()).ToList();

            ElencoAttivitaView.ItemsView.RightPanesView.UpdateViewSettings(CurrentViewSettings);

        }
    }
}
