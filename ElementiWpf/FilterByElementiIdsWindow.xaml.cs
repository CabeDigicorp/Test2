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

namespace ElementiWpf
{
    /// <summary>
    /// Interaction logic for SelectArticoliWindow.xaml
    /// </summary>
    public partial class FilterByElementiIdsWindow : Window
    {
        ElementiView ElementiView { get { return ElementiCtrl.DataContext as ElementiView; } }

        public ClientDataService DataService { get; set; } = null;//ref
        public ModelActionsStack ModelActionsStack { get; set; } = null;//ref
        public IEntityWindowService WindowService { get; set; } = null;//ref
        public IMainOperation MainOperation { get; set; } = null;//ref

        /// <summary>
        /// Items selezionati (checkati) alla chiusura o apertura del dialogo
        /// </summary>
        public List<Guid> ElementiItemSelectedIds { get; set; } = new List<Guid>();

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


        public FilterByElementiIdsWindow()
        {
            InitializeComponent();

            ElementiCtrl.DataContext = new ElementiView();

            base.Closed += FilterByPrezzarioIdsWindow_Closed;
        }

        private void FilterByPrezzarioIdsWindow_Closed(object sender, EventArgs e)
        {
            ElementiView.ElementiItemsView.AllowNoSelection = false;
        }

        public void Init()
        {
            ElementiView.DataService = DataService;
            ElementiView.WindowService = WindowService;
            ElementiView.ModelActionsStack = ModelActionsStack;
            ElementiView.MainOperation = MainOperation;

            //ElementiView.Init(CurrentViewSettings);
            ElementiView.Init(null);

            HashSet<Guid> elementiItemSelectedIds = new HashSet<Guid>(ElementiItemSelectedIds);
            if (elementiItemSelectedIds.Contains(Guid.Empty))
            {
                ElementiView.ElementiItemsView.SetNoSelectionChecked(true);
                elementiItemSelectedIds.Remove(Guid.Empty);
            }
            else
                ElementiView.ElementiItemsView.SetNoSelectionChecked(false);

            ElementiView.ElementiItemsView.RightPanesView.ClosePanes();
            ElementiView.ElementiItemsView.CheckedEntitiesId = new HashSet<Guid>(elementiItemSelectedIds);
            ElementiView.ElementiItemsView.AllowNoSelection = AllowNoSelection;
            ElementiView.ItemsView.IsImportItemsEnabled = false;
            //ElementiView.ElementiItemsView.SetNoSelectionChecked(true);



            ElementiView.ElementiItemsView.SelectEntityById(CurrentElementoId);
           
        }

        private void FindButton_Click(object sender, RoutedEventArgs e)
        {
            ElementiView.UpdateViewSettings(CurrentViewSettings);

            //prendo solo gli elementi con prezzo (foglie)
            //IEnumerable<Guid> selectedItemsId = ElementiView.ElementiItemsView.CheckedEntitiesId.Where(item => !ElementiView.ElementiItemsView.HasChildren(item));
            IEnumerable<Guid> selectedItemsId = ElementiView.ElementiItemsView.CheckedEntitiesId;

            ElementiItemSelectedIds = selectedItemsId.ToList<Guid>();
            if (AllowNoSelection && ElementiView.ElementiItemsView.IsNoSelectionChecked)
                ElementiItemSelectedIds.Add(Guid.Empty);

            IsFilter = false;
            DialogResult = true;


        }

        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            ElementiView.UpdateViewSettings(CurrentViewSettings);

            //prendo solo gli elementi con prezzo (foglie)
            //IEnumerable<Guid> selectedItemsId = ElementiView.ElementiItemsView.CheckedEntitiesId.Where(item => !ElementiView.ElementiItemsView.HasChildren(item));
            IEnumerable<Guid> selectedItemsId = ElementiView.ElementiItemsView.CheckedEntitiesId;

            ElementiItemSelectedIds = selectedItemsId.ToList<Guid>();
            if (AllowNoSelection && ElementiView.ElementiItemsView.IsNoSelectionChecked)
                ElementiItemSelectedIds.Add(Guid.Empty);

            IsFilter = true;
            DialogResult = true;

        }
    }
}
