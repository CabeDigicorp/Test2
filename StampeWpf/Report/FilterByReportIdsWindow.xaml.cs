using Commons;
using MasterDetailModel;
using MasterDetailView;
using Model;
using StampeWpf.View;
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

namespace StampeWpf
{
    /// <summary>
    /// Interaction logic for SelectArticoliWindow.xaml
    /// </summary>
    public partial class FilterByReportIdsWindow : Window
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
        /// Fuoco su Prezzo all'apertura del dialogo
        /// </summary>
        public Guid CurrentReportId { get; set; } = Guid.Empty;

        /// <summary>
        /// true: si esce filtrando, false: si esce trovando
        /// </summary>
        public bool IsFilter { get; protected set; }

        /// <summary>
        /// Possibilità di assegnare nessun articolo
        /// </summary>
        public bool AllowNoSelection { get; set; } = false;

        /// <summary>
        /// Impostazioni di Filtro, Ordine e Raggruppamento all'avvio del dialogo 
        /// </summary>
        public EntityTypeViewSettings CurrentViewSettings { get; set; } = new EntityTypeViewSettings();


        public FilterByReportIdsWindow()
        {
            InitializeComponent();

            base.Closed += FilterByReportIdsWindow_Closed;
        }

        private void FilterByReportIdsWindow_Closed(object sender, EventArgs e)
        {
            ReportView.ItemsView.AllowNoSelection = false;
        }

        public void Init()
        {
            ReportCtrl.Init();

            ReportView.DataService = DataService;
            ReportView.WindowService = WindowService;
            ReportView.ModelActionsStack = ModelActionsStack;
            ReportView.MainOperation = MainOperation;
            //ReportView.Init(CurrentViewSettings);
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
            ReportView.ItemsView.IsImportItemsEnabled = false;
            //ReportView.ItemsView.SetNoSelectionChecked(true);

            ReportView.ItemsView.SelectEntityById(CurrentReportId);
           
        }

        private void FindButton_Click(object sender, RoutedEventArgs e)
        {
            
            IEnumerable<Guid> selectedItemsId = ReportView.ItemsView.CheckedEntitiesId;

            UpdateViewSettings();

            ReportItemSelectedIds = selectedItemsId.ToList<Guid>();
            if (AllowNoSelection && ReportView.ItemsView.IsNoSelectionChecked)
                ReportItemSelectedIds.Add(Guid.Empty);

            IsFilter = false;
            DialogResult = true;


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
