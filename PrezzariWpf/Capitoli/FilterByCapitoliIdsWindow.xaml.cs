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
    public partial class FilterByCapitoliIdsWindow : Window
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
        /// Fuoco su Prezzo all'apertura del dialogo
        /// </summary>
        public Guid CurrentCapitoliId { get; set; } = Guid.Empty;

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


        public FilterByCapitoliIdsWindow()
        {
            InitializeComponent();

            base.Closed += FilterByCapitoliIdsWindow_Closed;
        }

        private void FilterByCapitoliIdsWindow_Closed(object sender, EventArgs e)
        {
            CapitoliView.ItemsView.AllowNoSelection = false;
        }

        public void Init()
        {
            CapitoliCtrl.Init();

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
            CapitoliView.ItemsView.IsImportItemsEnabled = false;
            //CapitoliView.ItemsView.SetNoSelectionChecked(true);

            CapitoliView.ItemsView.SelectEntityById(CurrentCapitoliId);
           
        }

        private void FindButton_Click(object sender, RoutedEventArgs e)
        {
            //prendo solo gli elementi con prezzo (foglie)
            IEnumerable<Guid> selectedItemsId = CapitoliView.ItemsView.CheckedEntitiesId.Where(item => !CapitoliView.ItemsView.HasChildren(item));

            UpdateViewSettings();

            CapitoliItemSelectedIds = selectedItemsId.ToList<Guid>();
            if (AllowNoSelection && CapitoliView.ItemsView.IsNoSelectionChecked)
                CapitoliItemSelectedIds.Add(Guid.Empty);

            IsFilter = false;
            DialogResult = true;


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
