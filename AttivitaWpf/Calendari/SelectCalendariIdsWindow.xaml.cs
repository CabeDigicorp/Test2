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
using System.Windows.Shapes;

namespace AttivitaWpf
{
    /// <summary>
    /// Interaction logic for SelectCalendariIdsWindow.xaml
    /// </summary>
    public partial class SelectCalendariIdsWindow : Window
    {
        CalendariView CalendariView { get { return CalendariCtrl.DataContext as CalendariView; } }

        public ClientDataService DataService { get; set; } = null;//ref
        public ModelActionsStack ModelActionsStack { get; set; } = null;//ref
        public IEntityWindowService WindowService { get; set; } = null;//ref
        public IMainOperation MainOperation { get; set; } = null;//ref

        /// <summary>
        /// Items selezionati (checkati) alla chiusura o apertura del dialogo
        /// </summary>
        public List<Guid> CalendariItemSelectedIds { get; set; } = new List<Guid>();

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

        public SelectCalendariIdsWindow()
        {
            InitializeComponent();

            DataContext = new CalendariView();

            base.Closed += SelectContattiIdsWindow_Closed;
        }

        private void SelectContattiIdsWindow_Closed(object sender, EventArgs e)
        {
            CalendariView.ItemsView.AllowNoSelection = false;
        }

        public void Init()
        {
            CalendariCtrl.Init();

            CalendariView.CalculatorFunctions.Add(NoteCalculatorFunction);
            CalendariView.CalculatorFunctions.Add(EPCalculatorFunction);


            CalendariView.DataService = DataService;
            CalendariView.WindowService = WindowService;
            CalendariView.ModelActionsStack = ModelActionsStack;
            CalendariView.MainOperation = MainOperation;

            CalendariView.Init(ViewSettings);

            CalendariView.ItemsView.RightPanesView.ClosePanes();

            HashSet<Guid> calendariItemSelectedIds = new HashSet<Guid>(CalendariItemSelectedIds);
            if (calendariItemSelectedIds.Contains(Guid.Empty))
            {
                CalendariView.ItemsView.SetNoSelectionChecked(true);
                calendariItemSelectedIds.Remove(Guid.Empty);
            }
            else
                CalendariView.ItemsView.SetNoSelectionChecked(false);

            CalendariView.ItemsView.CheckedEntitiesId = new HashSet<Guid>(calendariItemSelectedIds);
            CalendariView.ItemsView.AllowNoSelection = AllowNoSelection;
            CalendariView.ItemsView.IsSingleSelection = IsSingleSelection;
            CalendariView.ItemsView.IsImportItemsEnabled = false;

            Guid currentContattiId = CalendariItemSelectedIds.FirstOrDefault();
            CalendariView.ItemsView.SelectEntityById(currentContattiId);

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
        private void CalendariCtrl_EntityViewMouseDoubleClick(object sender, EventArgs e)
        {
            CalendariView.ItemsView.CheckedEntitiesId = new HashSet<Guid>() { CalendariView.ItemsView.SelectedEntityId };
            AcceptSelection();
        }


        private void AcceptSelection()
        {
            //N.B. non so bene perche alla chiusura della finestra questo evento a volte parta 2 volte
            if (DialogResult != null)
                return;

            //prendo solo gli elementi foglie
            IEnumerable<Guid> selectedItemsId = CalendariView.ItemsView.CheckedEntitiesId;//.Where(item => !ContattiView.ItemsView.HasChildren(item));

            UpdateViewSettings();

            if (IsSingleSelection)
            {

                if (selectedItemsId.Count() == 1)
                {
                    CalendariItemSelectedIds = selectedItemsId.ToList<Guid>();
                    DialogResult = true;
                }
                else if (selectedItemsId.Count() > 1)
                {
                    CalendariView.ItemsView.MainOperation.ShowMessageBarView(LocalizationProvider.GetString("NonPossibileSelezionarePiuDiUnaVoce"));
                    DialogResult = false;
                }
                else
                {
                    if (AllowNoSelection && CalendariView.ItemsView.IsNoSelectionChecked)
                    {
                        CalendariItemSelectedIds = new List<Guid>() { Guid.Empty };
                        DialogResult = true;
                    }
                    else
                    {
                        MainOperation.ShowMessageBarView(LocalizationProvider.GetString("OccorreSelezionareUnCalendario"));
                        DialogResult = false;
                    }
                }


            }
            else
            {
                CalendariItemSelectedIds = selectedItemsId.ToList<Guid>();
                DialogResult = true;
            }
        }

        private void UpdateViewSettings()
        {


            //if (CalendariView.ItemsView.RightPanesView.GroupView.Data != null && CalendariView.ItemsView.RightPanesView.GroupView.Data.Items.Any())
            //    CurrentViewSettings.Groups = CalendariView.ItemsView.RightPanesView.GroupView.Data.Items.Select(item => item.Clone()).ToList();

            //if (CalendariView.ItemsView.RightPanesView.SortView.Data != null && CalendariView.ItemsView.RightPanesView.SortView.Data.Items.Any())
            //    CurrentViewSettings.Sorts = CalendariView.ItemsView.RightPanesView.SortView.Data.Items.Select(item => item.Clone()).ToList();

            //if (CalendariView.ItemsView.RightPanesView.FilterView.Data != null && CalendariView.ItemsView.RightPanesView.FilterView.Data.Items.Any())
            //    CurrentViewSettings.Filters = CalendariView.ItemsView.RightPanesView.FilterView.Data.Items.Select(item => item.Clone()).ToList();

           CalendariView.ItemsView.RightPanesView.UpdateViewSettings(CurrentViewSettings);

        }
    }
}