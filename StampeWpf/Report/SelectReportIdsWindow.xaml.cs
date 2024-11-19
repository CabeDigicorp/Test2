using CommonResources;
using Commons;
using MasterDetailModel;
using MasterDetailView;
using Model;
using StampeWpf.View;
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

namespace StampeWpf.Report
{
    /// <summary>
    /// Interaction logic for SelectReportIdsWindow.xaml
    /// </summary>
    public partial class SelectReportIdsWindow : Window
    {
        ReportView ReportView { get { return ReportCtrl.DataContext as ReportView; } }
        public ClientDataService DataService { get; set; } = null;//ref
        public ModelActionsStack ModelActionsStack { get; set; } = null;//ref
        public IEntityWindowService WindowService { get; set; } = null;//ref
        public IMainOperation MainOperation { get; set; } = null;//ref

        /// <summary>
        /// Items selezionati (checkati) alla chiusura o apertura del dialogo
        /// </summary>
        public List<Guid> ReportItemSelectedIds { get; set; } = new List<Guid>();


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

        public SelectReportIdsWindow()
        {
            InitializeComponent();

            ReportCtrl.DataContext = new ReportView();

            base.Closed += SelectReportIdsWindow_Closed;

        }

        private void SelectReportIdsWindow_Closed(object sender, EventArgs e)
        {
            ReportView.ItemsView.AllowNoSelection = false;

            ReportCtrl.MasterDetailGrid.UnsubscribeViewEvents();
        }

        public void Init()
        {
            //ReportCtrl.MasterDetailGrid.Init();

            ReportView.DataService = DataService;
            ReportView.WindowService = WindowService;
            ReportView.ModelActionsStack = ModelActionsStack;
            ReportView.MainOperation = MainOperation;
            //CapitoliView.Init(CurrentViewSettings);
            ReportView.Init(null);//per ora non imposto filtri e ordinamento all'apertura del dialogo

            ReportView.ItemsView.RightPanesView.ClosePanes();

            HashSet<Guid> reportItemSelectedIds = new HashSet<Guid>(ReportItemSelectedIds);
            if (reportItemSelectedIds.Contains(Guid.Empty))
            {
                ReportView.ItemsView.SetNoSelectionChecked(true);
                reportItemSelectedIds.Remove(Guid.Empty);
            }
            else
                ReportView.ItemsView.SetNoSelectionChecked(false);

            ReportView.ItemsView.CheckedEntitiesId = new HashSet<Guid>(reportItemSelectedIds);

            ReportView.ItemsView.AllowNoSelection = AllowNoSelection;
            //CapitoliView.ItemsView.SetNoSelectionChecked(true);
            ReportView.ItemsView.IsSingleSelection = IsSingleSelection;
            ReportView.ItemsView.IsImportItemsEnabled = false;

            Guid currentReportId = ReportItemSelectedIds.FirstOrDefault();
            ReportView.ItemsView.SelectEntityById(currentReportId);

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
        private void ReportCtrl_EntityViewMouseDoubleClick(object sender, EventArgs e)
        {
            ReportView.ItemsView.CheckedEntitiesId = new HashSet<Guid>() { ReportView.ItemsView.SelectedEntityId };
            AcceptSelection();
        }


        private void AcceptSelection()
        {
            //N.B. non so bene perche alla chiusura della finestra questo evento a volte parta 2 volte
            if (DialogResult != null)
                return;

            // DA CONCLUDERE 
            //prendo solo gli elementi con prezzo (foglie)
            IEnumerable<Guid> selectedItemsId = ReportView.ItemsView.CheckedEntitiesId.ToList();

            UpdateViewSettings();

            if (IsSingleSelection)
            {

                if (selectedItemsId.Count() == 1)
                {
                    ReportItemSelectedIds = selectedItemsId.ToList<Guid>();
                    DialogResult = true;
                }
                else if (selectedItemsId.Count() > 1)
                {
                    ReportView.ItemsView.MainOperation.ShowMessageBarView(LocalizationProvider.GetString("NonPossibileSelezionarePiuDiUnaVoce"));
                    DialogResult = false;
                }
                else
                {
                    if (AllowNoSelection && ReportView.ItemsView.IsNoSelectionChecked)
                    {
                        ReportItemSelectedIds = new List<Guid>() { Guid.Empty };
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
                ReportItemSelectedIds = selectedItemsId.ToList<Guid>();
                DialogResult = true;
            }
        }

        private void UpdateViewSettings()
        {


            //if (ReportView.ItemsView.RightPanesView.GroupView.Data != null && ReportView.ItemsView.RightPanesView.GroupView.Data.Items.Any())
            //    CurrentViewSettings.Groups = ReportView.ItemsView.RightPanesView.GroupView.Data.Items.Select(item => item.Clone()).ToList();

            //if (ReportView.ItemsView.RightPanesView.SortView.Data != null && ReportView.ItemsView.RightPanesView.SortView.Data.Items.Any())
            //    CurrentViewSettings.Sorts = ReportView.ItemsView.RightPanesView.SortView.Data.Items.Select(item => item.Clone()).ToList();

            //if (ReportView.ItemsView.RightPanesView.FilterView.Data != null && ReportView.ItemsView.RightPanesView.FilterView.Data.Items.Any())
            //    CurrentViewSettings.Filters = ReportView.ItemsView.RightPanesView.FilterView.Data.Items.Select(item => item.Clone()).ToList();

            ReportView.ItemsView.RightPanesView.UpdateViewSettings(CurrentViewSettings);

        }


    }
}
