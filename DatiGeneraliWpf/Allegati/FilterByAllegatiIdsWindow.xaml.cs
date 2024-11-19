using DatiGeneraliWpf.View;
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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DatiGeneraliWpf
{
    /// <summary>
    /// Interaction logic for FilterByAllegatiIdsWindow.xaml
    /// </summary>
    public partial class FilterByAllegatiIdsWindow : Window
    {
        AllegatiView AllegatiView { get { return AllegatiCtrl.DataContext as AllegatiView; } }

        public ClientDataService DataService { get; set; } = null;//ref
        public ModelActionsStack ModelActionsStack { get; set; } = null;//ref
        public IEntityWindowService WindowService { get; set; } = null;//ref
        public IMainOperation MainOperation { get; set; } = null;//ref

        /// <summary>
        /// Items selezionati (checkati) alla chiusura o apertura del dialogo
        /// </summary>
        public List<Guid> AllegatiItemSelectedIds { get; set; } = new List<Guid>();

        /// <summary>
        /// Fuoco su Contatto all'apertura del dialogo
        /// </summary>
        public Guid CurrentAllegatiId { get; set; } = Guid.Empty;

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


        public FilterByAllegatiIdsWindow()
        {
            InitializeComponent();

            base.Closed += FilterByAllegatiIdsWindow_Closed;
        }

        private void FilterByAllegatiIdsWindow_Closed(object sender, EventArgs e)
        {
            AllegatiView.ItemsView.AllowNoSelection = false;
        }

        public void Init()
        {
            AllegatiCtrl.Init();

            AllegatiView.DataService = DataService;
            AllegatiView.WindowService = WindowService;
            AllegatiView.ModelActionsStack = ModelActionsStack;
            AllegatiView.MainOperation = MainOperation;
            //ContattiView.Init(CurrentViewSettings);
            AllegatiView.Init(null);//per ora non imposto filtri e ordinamento all'apertura del dialogo

            AllegatiView.ItemsView.RightPanesView.ClosePanes();

            HashSet<Guid> allegatiItemSelectedIds = new HashSet<Guid>(AllegatiItemSelectedIds);
            if (allegatiItemSelectedIds.Contains(Guid.Empty))
            {
                AllegatiView.ItemsView.SetNoSelectionChecked(true);
                allegatiItemSelectedIds.Remove(Guid.Empty);
            }
            else
                AllegatiView.ItemsView.SetNoSelectionChecked(false);

            AllegatiView.ItemsView.CheckedEntitiesId = new HashSet<Guid>(allegatiItemSelectedIds);
            AllegatiView.ItemsView.AllowNoSelection = AllowNoSelection;
            AllegatiView.ItemsView.IsImportItemsEnabled = false;

            AllegatiView.ItemsView.SelectEntityById(CurrentAllegatiId);

        }

        private void FindButton_Click(object sender, RoutedEventArgs e)
        {
            //prendo solo gli elementi foglie
            IEnumerable<Guid> selectedItemsId = AllegatiView.ItemsView.CheckedEntitiesId;//.Where(item => !ContattiView.ItemsView.HasChildren(item));

            UpdateViewSettings();

            AllegatiItemSelectedIds = selectedItemsId.ToList<Guid>();
            if (AllowNoSelection && AllegatiView.ItemsView.IsNoSelectionChecked)
                AllegatiItemSelectedIds.Add(Guid.Empty);

            IsFilter = false;
            DialogResult = true;


        }


        private void UpdateViewSettings()
        {


            //if (AllegatiView.ItemsView.RightPanesView.GroupView.Data != null && AllegatiView.ItemsView.RightPanesView.GroupView.Data.Items.Any())
            //    CurrentViewSettings.Groups = AllegatiView.ItemsView.RightPanesView.GroupView.Data.Items.Select(item => item.Clone()).ToList();

            //if (AllegatiView.ItemsView.RightPanesView.SortView.Data != null && AllegatiView.ItemsView.RightPanesView.SortView.Data.Items.Any())
            //    CurrentViewSettings.Sorts = AllegatiView.ItemsView.RightPanesView.SortView.Data.Items.Select(item => item.Clone()).ToList();

            //if (AllegatiView.ItemsView.RightPanesView.FilterView.Data != null && AllegatiView.ItemsView.RightPanesView.FilterView.Data.Items.Any())
            //    CurrentViewSettings.Filters = AllegatiView.ItemsView.RightPanesView.FilterView.Data.Items.Select(item => item.Clone()).ToList();

            AllegatiView.ItemsView.RightPanesView.UpdateViewSettings(CurrentViewSettings);

        }
    }
}
