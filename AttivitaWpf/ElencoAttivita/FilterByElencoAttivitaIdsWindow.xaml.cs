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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AttivitaWpf
{
    /// <summary>
    /// Interaction logic for FilterByElencoAttivitaIdsWindow.xaml
    /// </summary>
    public partial class FilterByElencoAttivitaIdsWindow : Window
    {
        ElencoAttivitaView ElencoAttivitaView { get { return ElencoAttivitaCtrl.DataContext as ElencoAttivitaView; } }

        public ClientDataService DataService { get; set; } = null;//ref
        public ModelActionsStack ModelActionsStack { get; set; } = null;//ref
        public IEntityWindowService WindowService { get; set; } = null;//ref
        public IMainOperation MainOperation { get; set; } = null;//ref

        /// <summary>
        /// Items selezionati (checkati) alla chiusura o apertura del dialogo
        /// </summary>
        public List<Guid> ElencoAttivaItemSelectedIds { get; set; } = new List<Guid>();

        /// <summary>
        /// Fuoco su Contatto all'apertura del dialogo
        /// </summary>
        public Guid CurrentElencoAttivitaId { get; set; } = Guid.Empty;

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


        public FilterByElencoAttivitaIdsWindow()
        {
            InitializeComponent();

            base.Closed += FilterByElencoAttivitaIdsWindow_Closed;
        }

        private void FilterByElencoAttivitaIdsWindow_Closed(object sender, EventArgs e)
        {
            ElencoAttivitaView.ItemsView.AllowNoSelection = false;
        }

        public void Init()
        {
            ElencoAttivitaCtrl.Init();

            ElencoAttivitaView.DataService = DataService;
            ElencoAttivitaView.WindowService = WindowService;
            ElencoAttivitaView.ModelActionsStack = ModelActionsStack;
            ElencoAttivitaView.MainOperation = MainOperation;
            //ContattiView.Init(CurrentViewSettings);
            ElencoAttivitaView.Init(null);//per ora non imposto filtri e ordinamento all'apertura del dialogo

            ElencoAttivitaView.ItemsView.RightPanesView.ClosePanes();

            HashSet<Guid> elencoAttivitaItemSelectedIds = new HashSet<Guid>(ElencoAttivaItemSelectedIds);
            if (elencoAttivitaItemSelectedIds.Contains(Guid.Empty))
            {
                ElencoAttivitaView.ItemsView.SetNoSelectionChecked(true);
                elencoAttivitaItemSelectedIds.Remove(Guid.Empty);
            }
            else
                ElencoAttivitaView.ItemsView.SetNoSelectionChecked(false);

            ElencoAttivitaView.ItemsView.CheckedEntitiesId = new HashSet<Guid>(elencoAttivitaItemSelectedIds);
            ElencoAttivitaView.ItemsView.AllowNoSelection = AllowNoSelection;
            ElencoAttivitaView.ItemsView.IsImportItemsEnabled = false;

            ElencoAttivitaView.ItemsView.SelectEntityById(CurrentElencoAttivitaId);

        }

        private void FindButton_Click(object sender, RoutedEventArgs e)
        {
            //prendo solo gli elementi foglie
            IEnumerable<Guid> selectedItemsId = ElencoAttivitaView.ItemsView.CheckedEntitiesId;//.Where(item => !ContattiView.ItemsView.HasChildren(item));

            UpdateViewSettings();

            ElencoAttivaItemSelectedIds = selectedItemsId.ToList<Guid>();
            if (AllowNoSelection && ElencoAttivitaView.ItemsView.IsNoSelectionChecked)
                ElencoAttivaItemSelectedIds.Add(Guid.Empty);

            IsFilter = false;
            DialogResult = true;


        }


        private void UpdateViewSettings()
        {


            //if (ElencoAttivitaView.ItemsView.RightPanesView.GroupView.Data != null && ElencoAttivitaView.ItemsView.RightPanesView.GroupView.Data.Items.Any())
            //    CurrentViewSettings.Groups = ElencoAttivitaView.ItemsView.RightPanesView.GroupView.Data.Items.Select(item => item.Clone()).ToList();

            //if (ElencoAttivitaView.ItemsView.RightPanesView.SortView.Data != null && ElencoAttivitaView.ItemsView.RightPanesView.SortView.Data.Items.Any())
            //    CurrentViewSettings.Sorts = ElencoAttivitaView.ItemsView.RightPanesView.SortView.Data.Items.Select(item => item.Clone()).ToList();

            //if (ElencoAttivitaView.ItemsView.RightPanesView.FilterView.Data != null && ElencoAttivitaView.ItemsView.RightPanesView.FilterView.Data.Items.Any())
            //    CurrentViewSettings.Filters = ElencoAttivitaView.ItemsView.RightPanesView.FilterView.Data.Items.Select(item => item.Clone()).ToList();

            ElencoAttivitaView.ItemsView.RightPanesView.UpdateViewSettings(CurrentViewSettings);

        }
    }
}
