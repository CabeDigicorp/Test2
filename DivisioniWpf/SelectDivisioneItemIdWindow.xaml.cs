using CommonResources;
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
    public partial class SelectDivisioneItemIdWindow : Window
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
        /// Possibile selezionare solo un elemento
        /// </summary>
        public bool IsSingleSelection { get; set; } = false;

        /// <summary>
        /// Possibilità di assegnare nessun articolo
        /// </summary>
        public bool AllowNoSelection { get; set; } = false;

        /// <summary>
        /// Impostazioni di Filtro, Ordine e Raggruppamento all'avvio del dialogo 
        /// </summary>
        public EntityTypeViewSettings CurrentViewSettings { get; set; } = new EntityTypeViewSettings();

        /// <summary>
        /// Attributo che chiede di selezionare l'item della divisione (doppio click su un attributo riferimento del computo)
        /// </summary>
        public AttributoRiferimento SenderAttRif { get; set; } = null;

        /// <summary>
        /// true se gli items dovranno essere summarize rispetto a  SenderAttRif
        /// </summary>
        public bool IsItemsSummarized { get; set; } = false;

        public SelectDivisioneItemIdWindow()
        {
            InitializeComponent();

            base.Closed += SelectDivisioneItemIdWindow_Closed;
        }

        private void SelectDivisioneItemIdWindow_Closed(object sender, EventArgs e)
        {
            DivisioneView.ItemsView.AllowNoSelection = false;
        }

        public void Init(ClientDataService dataService, IEntityWindowService windowService, Guid divId, ModelActionsStack modelActionsStack)
        {
            DivisioneCtrl.Init();

            DataService = dataService;

            DivisioneView.DataService = DataService;
            DivisioneView.WindowService = WindowService;
            DivisioneView.ModelActionsStack = ModelActionsStack;
            DivisioneView.MainOperation = MainOperation;

            DivisioneView.IsItemsSummarized = IsItemsSummarized;
            DivisioneView.SenderAttRif = SenderAttRif;
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

            DivisioneView.ItemsView.AllowNoSelection = AllowNoSelection;
            DivisioneView.ItemsView.RightPanesView.ClosePanes();

            NomeDivisione.Text = DivisioneView.ItemsView.EntityType.Name;

            Guid currentDivisioneItemId = DivisioneItemSelectedIds.FirstOrDefault();
            DivisioneView.ItemsView.SelectEntityById(currentDivisioneItemId);
        }

        /// <summary>
        /// Accetto la selezione tramite pulsante con spunta
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            AcceptSelection();

        }

        /// <summary>
        /// Accetto la selezione tramite doppio click su un'entità
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DivisioneCtrl_EntityViewMouseDoubleClick(object sender, EventArgs e)
        {
            DivisioneView.ItemsView.CheckedEntitiesId = new HashSet<Guid>() { DivisioneView.ItemsView.SelectedEntityId };
            AcceptSelection();
        }

        private void AcceptSelection()
        {
            //N.B. non so bene perche alla chiusura della finestra questo evento a volte parta 2 volte
            if (DialogResult != null)
                return;

            //prendo solo gli elementi con prezzo (foglie)
            IEnumerable<Guid> selectedItemsId = DivisioneView.ItemsView.CheckedEntitiesId.Where(item => !DivisioneView.ItemsView.HasChildren(item));

            UpdateViewSettings();

            if (IsSingleSelection)
            {

                if (selectedItemsId.Count() == 1)
                {
                    DivisioneItemSelectedIds = selectedItemsId.ToList<Guid>();
                    DialogResult = true;
                }
                else if (selectedItemsId.Count() > 1)
                {
                    MainOperation.ShowMessageBarView(LocalizationProvider.GetString("NonPossibileSelezionarePiuDiUnaVoce"));
                    DialogResult = false;
                }
                else
                {
                    if (AllowNoSelection && DivisioneView.ItemsView.IsNoSelectionChecked)
                    {
                        DivisioneItemSelectedIds = new List<Guid>() { Guid.Empty };
                        DialogResult = true;
                    }
                    else
                    {
                        MainOperation.ShowMessageBarView(LocalizationProvider.GetString("OccorreSelezionareUnaVoce"));
                        DialogResult = false;
                    }
                }


            }
            else
            {
                DivisioneItemSelectedIds = selectedItemsId.ToList<Guid>();
                DialogResult = true;
            }
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
