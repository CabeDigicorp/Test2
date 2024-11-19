using CommonResources;
using Commons;
using DatiGeneraliWpf.View;
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

namespace DatiGeneraliWpf.Stili
{
    /// <summary>
    /// Interaction logic for SelectStiliIdsWindow.xaml
    /// </summary>
    public partial class SelectStiliIdsWindow : Window
    {
        StiliProgettoView StiliProgettoView { get { return StiliProgettoCtrl.DataContext as StiliProgettoView; } }
        public ClientDataService DataService { get; set; } = null;//ref
        public ModelActionsStack ModelActionsStack { get; set; } = null;//ref
        public IEntityWindowService WindowService { get; set; } = null;//ref
        public IMainOperation MainOperation { get; set; } = null;//ref

        /// <summary>
        /// Items selezionati (checkati) alla chiusura o apertura del dialogo
        /// </summary>
        public List<Guid> StiliItemSelectedIds { get; set; } = new List<Guid>();


        /// <summary>
        /// POssibilità di selezionare solo un elemento
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
        public SelectStiliIdsWindow()
        {
            InitializeComponent();

            StiliProgettoCtrl.DataContext = new StiliProgettoView();

            base.Closed += SelectStiletIdsWindow_Closed;
        }

        private void SelectStiletIdsWindow_Closed(object sender, EventArgs e)
        {
            StiliProgettoView.ItemsView.AllowNoSelection = false;
        }

        public void Init()
        {
            //StiletCtrl.MasterDetailGrid.Init();

            StiliProgettoView.DataService = DataService;
            StiliProgettoView.WindowService = WindowService;
            StiliProgettoView.ModelActionsStack = ModelActionsStack;
            StiliProgettoView.MainOperation = MainOperation;
            //CapitoliView.Init(CurrentViewSettings);
            StiliProgettoView.Init(null);//per ora non imposto filtri e ordinamento all'apertura del dialogo

            StiliProgettoView.ItemsView.RightPanesView.ClosePanes();

            HashSet<Guid> stileItemSelectedIds = new HashSet<Guid>(StiliItemSelectedIds);
            if (stileItemSelectedIds.Contains(Guid.Empty))
            {
                StiliProgettoView.ItemsView.SetNoSelectionChecked(true);
                stileItemSelectedIds.Remove(Guid.Empty);
            }
            else
                StiliProgettoView.ItemsView.SetNoSelectionChecked(false);

            StiliProgettoView.ItemsView.CheckedEntitiesId = new HashSet<Guid>(stileItemSelectedIds);

            StiliProgettoView.ItemsView.AllowNoSelection = AllowNoSelection;
            //CapitoliView.ItemsView.SetNoSelectionChecked(true);
            StiliProgettoView.ItemsView.IsSingleSelection = IsSingleSelection;
            StiliProgettoView.ItemsView.IsImportItemsEnabled = false;

            Guid currentStileId = StiliItemSelectedIds.FirstOrDefault();
            StiliProgettoView.ItemsView.SelectEntityById(currentStileId);

        }

        /// <summary>
        /// Uscita dal dialogo con pulsante con spunta
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            AcceptSelection();

        }

        /// <summary>
        /// Uscita dal dialogo con doppio click su un'entità
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StileCtrl_EntityViewMouseDoubleClick(object sender, EventArgs e)
        {
            StiliProgettoView.ItemsView.CheckedEntitiesId = new HashSet<Guid>() { StiliProgettoView.ItemsView.SelectedEntityId };
            AcceptSelection();
        }


        private void AcceptSelection()
        {
            //N.B. non so bene perche alla chiusura della finestra questo evento a volte parta 2 volte
            if (DialogResult != null)
                return;

            // DA CONCLUDERE 
            //prendo solo gli elementi con prezzo (foglie)
            IEnumerable<Guid> selectedItemsId = StiliProgettoView.ItemsView.CheckedEntitiesId.ToList();

            UpdateViewSettings();

            if (IsSingleSelection)
            {

                if (selectedItemsId.Count() == 1)
                {
                    StiliItemSelectedIds = selectedItemsId.ToList<Guid>();
                    DialogResult = true;
                }
                else if (selectedItemsId.Count() > 1)
                {
                    StiliProgettoView.ItemsView.MainOperation.ShowMessageBarView(LocalizationProvider.GetString("NonPossibileSelezionarePiuDiUnaVoce"));
                    DialogResult = false;
                }
                else
                {
                    if (AllowNoSelection && StiliProgettoView.ItemsView.IsNoSelectionChecked)
                    {
                        StiliItemSelectedIds = new List<Guid>() { Guid.Empty };
                        DialogResult = true;
                    }
                    else
                    {
                        MainOperation.ShowMessageBarView(LocalizationProvider.GetString("OccorreSelezionareUnArticolo"));
                        DialogResult = false;
                    }
                }


            }
            else
            {
                StiliItemSelectedIds = selectedItemsId.ToList<Guid>();
                DialogResult = true;
            }
        }

        private void UpdateViewSettings()
        {


            //if (StiliProgettoView.ItemsView.RightPanesView.GroupView.Data != null && StiliProgettoView.ItemsView.RightPanesView.GroupView.Data.Items.Any())
            //    CurrentViewSettings.Groups = StiliProgettoView.ItemsView.RightPanesView.GroupView.Data.Items.Select(item => item.Clone()).ToList();

            //if (StiliProgettoView.ItemsView.RightPanesView.SortView.Data != null && StiliProgettoView.ItemsView.RightPanesView.SortView.Data.Items.Any())
            //    CurrentViewSettings.Sorts = StiliProgettoView.ItemsView.RightPanesView.SortView.Data.Items.Select(item => item.Clone()).ToList();

            //if (StiliProgettoView.ItemsView.RightPanesView.FilterView.Data != null && StiliProgettoView.ItemsView.RightPanesView.FilterView.Data.Items.Any())
            //    CurrentViewSettings.Filters = StiliProgettoView.ItemsView.RightPanesView.FilterView.Data.Items.Select(item => item.Clone()).ToList();

            StiliProgettoView.ItemsView.RightPanesView.UpdateViewSettings(CurrentViewSettings);

        }
    }
}
