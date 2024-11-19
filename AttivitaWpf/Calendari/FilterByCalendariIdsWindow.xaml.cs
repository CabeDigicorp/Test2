using AttivitaWpf.View;
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
    /// Interaction logic for FilterByCalendariIdsWindow.xaml
    /// </summary>
    public partial class FilterByCalendariIdsWindow : Window
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
        /// Fuoco su Contatto all'apertura del dialogo
        /// </summary>
        public Guid CurrentCalendariId { get; set; } = Guid.Empty;

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


        public FilterByCalendariIdsWindow()
        {
            InitializeComponent();

            base.Closed += FilterByCalendariIdsWindow_Closed;
        }

        private void FilterByCalendariIdsWindow_Closed(object sender, EventArgs e)
        {
            CalendariView.ItemsView.AllowNoSelection = false;
        }

        public void Init()
        {
            CalendariCtrl.Init();

            CalendariView.DataService = DataService;
            CalendariView.WindowService = WindowService;
            CalendariView.ModelActionsStack = ModelActionsStack;
            CalendariView.MainOperation = MainOperation;
            //ContattiView.Init(CurrentViewSettings);
            CalendariView.Init(null);//per ora non imposto filtri e ordinamento all'apertura del dialogo

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
            CalendariView.ItemsView.IsImportItemsEnabled = false;

            CalendariView.ItemsView.SelectEntityById(CurrentCalendariId);

        }

        private void FindButton_Click(object sender, RoutedEventArgs e)
        {
            //prendo solo gli elementi foglie
            IEnumerable<Guid> selectedItemsId = CalendariView.ItemsView.CheckedEntitiesId;//.Where(item => !ContattiView.ItemsView.HasChildren(item));

            UpdateViewSettings();

            CalendariItemSelectedIds = selectedItemsId.ToList<Guid>();
            if (AllowNoSelection && CalendariView.ItemsView.IsNoSelectionChecked)
                CalendariItemSelectedIds.Add(Guid.Empty);

            IsFilter = false;
            DialogResult = true;


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