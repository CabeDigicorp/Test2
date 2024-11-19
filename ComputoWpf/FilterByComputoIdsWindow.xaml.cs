using Commons;
using MasterDetailModel;
using MasterDetailView;
using Model;
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

namespace ComputoWpf
{
    /// <summary>
    /// Interaction logic for SelectArticoliWindow.xaml
    /// </summary>
    public partial class FilterByComputoIdsWindow : Window
    {
        ComputoView ComputoView { get { return ComputoCtrl.DataContext as ComputoView; } }

        public ClientDataService DataService { get; set; } = null;//ref
        public ModelActionsStack ModelActionsStack { get; set; } = null;//ref
        public IEntityWindowService WindowService { get; set; } = null;//ref
        public IMainOperation MainOperation { get; set; } = null;//ref

        /// <summary>
        /// Items selezionati (checkati) alla chiusura o apertura del dialogo
        /// </summary>
        public List<Guid> ComputoItemSelectedIds { get; set; } = new List<Guid>();

        /// <summary>
        /// Fuoco su Prezzo all'apertura del dialogo
        /// </summary>
        public Guid CurrentElementoId { get; set; } = Guid.Empty;

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


        public FilterByComputoIdsWindow()
        {
            InitializeComponent();

            ComputoCtrl.DataContext = new ComputoView();

            base.Closed += FilterByPrezzarioIdsWindow_Closed;
        }

        private void FilterByPrezzarioIdsWindow_Closed(object sender, EventArgs e)
        {
            ComputoView.ComputoItemsView.AllowNoSelection = false;
        }

        public void Init()
        {
            ComputoView.DataService = DataService;
            ComputoView.WindowService = WindowService;
            ComputoView.ModelActionsStack = ModelActionsStack;
            ComputoView.MainOperation = MainOperation;

            //ComputoView.Init(CurrentViewSettings);
            ComputoView.Init(null);

            HashSet<Guid> computoItemSelectedIds = new HashSet<Guid>(ComputoItemSelectedIds);
            if (ComputoItemSelectedIds.Contains(Guid.Empty))
            {
                ComputoView.ComputoItemsView.SetNoSelectionChecked(true);
                computoItemSelectedIds.Remove(Guid.Empty);
            }
            else
                ComputoView.ComputoItemsView.SetNoSelectionChecked(false);

            ComputoView.ComputoItemsView.RightPanesView.ClosePanes();
            ComputoView.ComputoItemsView.CheckedEntitiesId = new HashSet<Guid>(ComputoItemSelectedIds);
            ComputoView.ComputoItemsView.AllowNoSelection = AllowNoSelection;
            ComputoView.ItemsView.IsImportItemsEnabled = false;
            //ComputoView.ComputoItemsView.SetNoSelectionChecked(true);



            ComputoView.ComputoItemsView.SelectEntityById(CurrentElementoId);
           
        }

        private void FindButton_Click(object sender, RoutedEventArgs e)
        {
            ComputoView.UpdateViewSettings(CurrentViewSettings);

            //prendo solo gli Computo con prezzo (foglie)
            //IEnumerable<Guid> selectedItemsId = ComputoView.ComputoItemsView.CheckedEntitiesId.Where(item => !ComputoView.ComputoItemsView.HasChildren(item));
            IEnumerable<Guid> selectedItemsId = ComputoView.ComputoItemsView.CheckedEntitiesId;

            ComputoItemSelectedIds = selectedItemsId.ToList<Guid>();
            if (AllowNoSelection && ComputoView.ComputoItemsView.IsNoSelectionChecked)
                ComputoItemSelectedIds.Add(Guid.Empty);

            IsFilter = false;
            DialogResult = true;


        }

        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            ComputoView.UpdateViewSettings(CurrentViewSettings);

            //prendo solo gli Computo con prezzo (foglie)
            //IEnumerable<Guid> selectedItemsId = ComputoView.ComputoItemsView.CheckedEntitiesId.Where(item => !ComputoView.ComputoItemsView.HasChildren(item));
            IEnumerable<Guid> selectedItemsId = ComputoView.ComputoItemsView.CheckedEntitiesId;

            ComputoItemSelectedIds = selectedItemsId.ToList<Guid>();
            if (AllowNoSelection && ComputoView.ComputoItemsView.IsNoSelectionChecked)
                ComputoItemSelectedIds.Add(Guid.Empty);

            IsFilter = true;
            DialogResult = true;

        }
    }
}
