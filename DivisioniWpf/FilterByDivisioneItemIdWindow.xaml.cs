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

namespace DivisioniWpf
{
    /// <summary>
    /// Interaction logic for SelectDivisioneItemIdWindow.xaml
    /// </summary>
    public partial class FilterByDivisioneItemIdWindow : Window
    {
        public ClientDataService DataService { get; set; } = null;//ref
        public IEntityWindowService WindowService { get; set; } = null;//ref
        public ModelActionsStack ModelActionsStack { get; set; } = null;//ref
        public IMainOperation MainOperation { get; set; } = null;//ref

        DivisioneView DivisioneView { get { return DivisioneCtrl.DataContext as DivisioneView; } }

        /// <summary>
        /// Items selezionati (checkati) alla chiusura del dialogo
        /// </summary>
        public List<Guid> DivisioneItemSelectedIds { get; set; } = new List<Guid>();

        /// <summary>
        /// Filtro impostato all'apertura del dialogo
        /// </summary>
        public FilterData FilterData { get; set; } = null;

        /// <summary>
        /// Fuoco su Prezzo all'apertura del dialogo
        /// </summary>
        public Guid CurrentDivisioneItemId { get; set; } = Guid.Empty;

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

        public FilterByDivisioneItemIdWindow()
        {
            InitializeComponent();

            base.Closed += FilterByDivisioneItemIdWindow_Closed;
        }

        private void FilterByDivisioneItemIdWindow_Closed(object sender, EventArgs e)
        {
            DivisioneView.ItemsView.AllowNoSelection = false;
        }

        public void Init(Guid divId)
        {
            DivisioneCtrl.Init();

            DivisioneView.DataService = DataService;
            DivisioneView.WindowService = WindowService;
            DivisioneView.ModelActionsStack = ModelActionsStack;
            DivisioneView.MainOperation = MainOperation;

            //DivisioneView.Init(divId, CurrentViewSettings);
            DivisioneView.Init(divId, null);

            HashSet<Guid> divisioneItemSelectedIds = new HashSet<Guid>(DivisioneItemSelectedIds);
            if (divisioneItemSelectedIds.Contains(Guid.Empty))
            {
                DivisioneView.ItemsView.SetNoSelectionChecked(true);
                divisioneItemSelectedIds.Remove(Guid.Empty);
            }
            else
                DivisioneView.ItemsView.SetNoSelectionChecked(false);

            DivisioneView.ItemsView.CheckedEntitiesId = new HashSet<Guid>(divisioneItemSelectedIds);
            DivisioneView.ItemsView.RightPanesView.ClosePanes();

            DivisioneView.ItemsView.AllowNoSelection = AllowNoSelection;
            NomeDivisione.Text = DivisioneView.ItemsView.EntityType.Name;
            DivisioneView.ItemsView.SelectEntityById(CurrentDivisioneItemId);
        }

        private void FindButton_Click(object sender, RoutedEventArgs e)
        {
            //prendo solo gli elementi con prezzo (foglie)
            IEnumerable<Guid> selectedItemsId = DivisioneView.ItemsView.CheckedEntitiesId.Where(item => !DivisioneView.ItemsView.HasChildren(item));

            UpdateViewSettings();

            DivisioneItemSelectedIds = selectedItemsId.ToList<Guid>();
            if (AllowNoSelection && DivisioneView.ItemsView.IsNoSelectionChecked)
                DivisioneItemSelectedIds.Add(Guid.Empty);

            IsFilter = false;
            DialogResult = true;

        }

        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            //prendo solo gli elementi con prezzo (foglie)
            IEnumerable<Guid> selectedItemsId = DivisioneView.ItemsView.CheckedEntitiesId.Where(item => !DivisioneView.ItemsView.HasChildren(item));

            UpdateViewSettings();

            DivisioneItemSelectedIds = selectedItemsId.ToList<Guid>();
            if (AllowNoSelection && DivisioneView.ItemsView.IsNoSelectionChecked)
                DivisioneItemSelectedIds.Add(Guid.Empty);

            IsFilter = true;
            DialogResult = true;

        }


        private void UpdateViewSettings()
        {


            //if (DivisioneView.ItemsView.RightPanesView.GroupView.Data != null && DivisioneView.ItemsView.RightPanesView.GroupView.Data.Items.Any())
            //    CurrentViewSettings.Groups = DivisioneView.ItemsView.RightPanesView.GroupView.Data.Items.Select(item => item.Clone()).ToList();

            //if (DivisioneView.ItemsView.RightPanesView.SortView.Data != null && DivisioneView.ItemsView.RightPanesView.SortView.Data.Items.Any())
            //    CurrentViewSettings.Sorts = DivisioneView.ItemsView.RightPanesView.SortView.Data.Items.Select(item => item.Clone()).ToList();

            //if (DivisioneView.ItemsView.RightPanesView.FilterView.Data != null && DivisioneView.ItemsView.RightPanesView.FilterView.Data.Items.Any())
            //    CurrentViewSettings.Filters = DivisioneView.ItemsView.RightPanesView.FilterView.Data.Items.Select(item => item.Clone()).ToList();

            DivisioneView.ItemsView.RightPanesView.UpdateViewSettings(CurrentViewSettings);

        }
    }
}
