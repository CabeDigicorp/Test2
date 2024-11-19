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
    public partial class FilterByPrezzarioIdsWindow : Window
    {
        PrezzarioView PrezzarioView { get { return PrezzarioCtrl.DataContext as PrezzarioView; } }

        public ClientDataService DataService { get; set; } = null;//ref
        public ModelActionsStack ModelActionsStack { get; set; } = null;//ref
        public IEntityWindowService WindowService { get; set; } = null;//ref
        public IMainOperation MainOperation { get; set; } = null;//ref

        /// <summary>
        /// Items selezionati (checkati) alla chiusura o apertura del dialogo
        /// </summary>
        public List<Guid> PrezzarioItemSelectedIds { get; set; } = new List<Guid>();

        /// <summary>
        /// Fuoco su Prezzo all'apertura del dialogo
        /// </summary>
        public Guid CurrentPrezzarioId { get; set; } = Guid.Empty;

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


        public FilterByPrezzarioIdsWindow()
        {
            InitializeComponent();

            base.Closed += FilterByPrezzarioIdsWindow_Closed;
        }

        private void FilterByPrezzarioIdsWindow_Closed(object sender, EventArgs e)
        {
            PrezzarioView.ItemsView.AllowNoSelection = false;
        }

        public void Init()
        {
            PrezzarioCtrl.Init();

            PrezzarioView.DataService = DataService;
            PrezzarioView.WindowService = WindowService;
            PrezzarioView.ModelActionsStack = ModelActionsStack;
            PrezzarioView.MainOperation = MainOperation;
            //PrezzarioView.Init(CurrentViewSettings);
            PrezzarioView.Init(null);//per ora non imposto filtri e ordinamento all'apertura del dialogo

            PrezzarioView.ItemsView.RightPanesView.ClosePanes();


            HashSet<Guid> prezzarioItemSelectedIds = new HashSet<Guid>(PrezzarioItemSelectedIds);
            if (prezzarioItemSelectedIds.Contains(Guid.Empty))
            {
                PrezzarioView.ItemsView.SetNoSelectionChecked(true);
                prezzarioItemSelectedIds.Remove(Guid.Empty);
            }
            else
                PrezzarioView.ItemsView.SetNoSelectionChecked(false);
            PrezzarioView.ItemsView.CheckedEntitiesId = new HashSet<Guid>(prezzarioItemSelectedIds);
            PrezzarioView.ItemsView.AllowNoSelection = AllowNoSelection;
            PrezzarioView.ItemsView.IsImportItemsEnabled = false;
            //PrezzarioView.ItemsView.SetNoSelectionChecked(true);

            PrezzarioView.ItemsView.SelectEntityById(CurrentPrezzarioId);
           
        }

        private void FindButton_Click(object sender, RoutedEventArgs e)
        {
            //prendo solo gli elementi con prezzo (foglie)
            IEnumerable<Guid> selectedItemsId = PrezzarioView.ItemsView.CheckedEntitiesId.Where(item => !PrezzarioView.ItemsView.HasChildren(item));

            UpdateViewSettings();

            PrezzarioItemSelectedIds = selectedItemsId.ToList<Guid>();
            if (AllowNoSelection && PrezzarioView.ItemsView.IsNoSelectionChecked)
                PrezzarioItemSelectedIds.Add(Guid.Empty);

            IsFilter = false;
            DialogResult = true;


        }

        //private void FilterButton_Click(object sender, RoutedEventArgs e)
        //{
        //    //prendo solo gli elementi con prezzo (foglie)
        //    IEnumerable<Guid> selectedItemsId = PrezzarioView.ItemsView.CheckedEntitiesId.Where(item => !PrezzarioView.ItemsView.HasChildren(item));

        //    UpdateViewSettings();

        //    PrezzarioItemSelectedIds = selectedItemsId.ToList<Guid>();
        //    if (AllowNoSelection && PrezzarioView.ItemsView.IsNoSelectionChecked)
        //        PrezzarioItemSelectedIds.Add(Guid.Empty);

        //    IsFilter = true;
        //    DialogResult = true;

        //}

        private void UpdateViewSettings()
        {


            //if (PrezzarioView.ItemsView.RightPanesView.GroupView.Data != null && PrezzarioView.ItemsView.RightPanesView.GroupView.Data.Items.Any())
            //    CurrentViewSettings.Groups = PrezzarioView.ItemsView.RightPanesView.GroupView.Data.Items.Select(item => item.Clone()).ToList();

            //if (PrezzarioView.ItemsView.RightPanesView.SortView.Data != null && PrezzarioView.ItemsView.RightPanesView.SortView.Data.Items.Any())
            //    CurrentViewSettings.Sorts = PrezzarioView.ItemsView.RightPanesView.SortView.Data.Items.Select(item => item.Clone()).ToList();

            //if (PrezzarioView.ItemsView.RightPanesView.FilterView.Data != null && PrezzarioView.ItemsView.RightPanesView.FilterView.Data.Items.Any())
            //    CurrentViewSettings.Filters = PrezzarioView.ItemsView.RightPanesView.FilterView.Data.Items.Select(item => item.Clone()).ToList();

            PrezzarioView.ItemsView.RightPanesView.UpdateViewSettings(CurrentViewSettings);

        }
    }
}
