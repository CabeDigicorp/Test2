using Commons;
using ContattiWpf.View;
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

namespace ContattiWpf
{
    /// <summary>
    /// Interaction logic for FilterByContattiIdsWindow.xaml
    /// </summary>
    public partial class FilterByContattiIdsWindow : Window
    {
        ContattiView ContattiView { get { return ContattiCtrl.DataContext as ContattiView; } }

        public ClientDataService DataService { get; set; } = null;//ref
        public ModelActionsStack ModelActionsStack { get; set; } = null;//ref
        public IEntityWindowService WindowService { get; set; } = null;//ref
        public IMainOperation MainOperation { get; set; } = null;//ref

        /// <summary>
        /// Items selezionati (checkati) alla chiusura o apertura del dialogo
        /// </summary>
        public List<Guid> ContattiItemSelectedIds { get; set; } = new List<Guid>();

        /// <summary>
        /// Fuoco su Contatto all'apertura del dialogo
        /// </summary>
        public Guid CurrentContattiId { get; set; } = Guid.Empty;

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


        public FilterByContattiIdsWindow()
        {
            InitializeComponent();

            base.Closed += FilterByContattiIdsWindow_Closed;
        }

        private void FilterByContattiIdsWindow_Closed(object sender, EventArgs e)
        {
            ContattiView.ItemsView.AllowNoSelection = false;
        }

        public void Init()
        {
            ContattiCtrl.Init();

            ContattiView.DataService = DataService;
            ContattiView.WindowService = WindowService;
            ContattiView.ModelActionsStack = ModelActionsStack;
            ContattiView.MainOperation = MainOperation;
            //ContattiView.Init(CurrentViewSettings);
            ContattiView.Init(null);//per ora non imposto filtri e ordinamento all'apertura del dialogo

            ContattiView.ItemsView.RightPanesView.ClosePanes();

            HashSet<Guid> contattiItemSelectedIds = new HashSet<Guid>(ContattiItemSelectedIds);
            if (contattiItemSelectedIds.Contains(Guid.Empty))
            {
                ContattiView.ItemsView.SetNoSelectionChecked(true);
                contattiItemSelectedIds.Remove(Guid.Empty);
            }
            else
                ContattiView.ItemsView.SetNoSelectionChecked(false);

            ContattiView.ItemsView.CheckedEntitiesId = new HashSet<Guid>(contattiItemSelectedIds);
            ContattiView.ItemsView.AllowNoSelection = AllowNoSelection;
            ContattiView.ItemsView.IsImportItemsEnabled = false;

            ContattiView.ItemsView.SelectEntityById(CurrentContattiId);

        }

        private void FindButton_Click(object sender, RoutedEventArgs e)
        {
            //prendo solo gli elementi foglie
            IEnumerable<Guid> selectedItemsId = ContattiView.ItemsView.CheckedEntitiesId;//.Where(item => !ContattiView.ItemsView.HasChildren(item));

            UpdateViewSettings();

            ContattiItemSelectedIds = selectedItemsId.ToList<Guid>();
            if (AllowNoSelection && ContattiView.ItemsView.IsNoSelectionChecked)
                ContattiItemSelectedIds.Add(Guid.Empty);

            IsFilter = false;
            DialogResult = true;


        }


        private void UpdateViewSettings()
        {


            //if (ContattiView.ItemsView.RightPanesView.GroupView.Data != null && ContattiView.ItemsView.RightPanesView.GroupView.Data.Items.Any())
            //    CurrentViewSettings.Groups = ContattiView.ItemsView.RightPanesView.GroupView.Data.Items.Select(item => item.Clone()).ToList();

            //if (ContattiView.ItemsView.RightPanesView.SortView.Data != null && ContattiView.ItemsView.RightPanesView.SortView.Data.Items.Any())
            //    CurrentViewSettings.Sorts = ContattiView.ItemsView.RightPanesView.SortView.Data.Items.Select(item => item.Clone()).ToList();

            //if (ContattiView.ItemsView.RightPanesView.FilterView.Data != null && ContattiView.ItemsView.RightPanesView.FilterView.Data.Items.Any())
            //    CurrentViewSettings.Filters = ContattiView.ItemsView.RightPanesView.FilterView.Data.Items.Select(item => item.Clone()).ToList();
            
            ContattiView.ItemsView.RightPanesView.UpdateViewSettings(CurrentViewSettings);

        }
    }
}
