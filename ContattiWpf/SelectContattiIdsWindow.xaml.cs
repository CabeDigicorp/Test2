using _3DModelExchange;
using CommonResources;
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
    /// Interaction logic for SelectContattiIdsWindow.xaml
    /// </summary>
    public partial class SelectContattiIdsWindow : Window
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

        public NoteCalculatorFunction NoteCalculatorFunction { get; set; } = null;
        public EPCalculatorFunction EPCalculatorFunction { get; set; } = null;

        public SelectContattiIdsWindow()
        {
            InitializeComponent();

            DataContext = new ContattiView();

            base.Closed += SelectContattiIdsWindow_Closed;
        }

        private void SelectContattiIdsWindow_Closed(object sender, EventArgs e)
        {
            ContattiView.ItemsView.AllowNoSelection = false;
        }

        public void Init()
        {
            ContattiCtrl.Init();

            ContattiView.CalculatorFunctions.Add(NoteCalculatorFunction);
            ContattiView.CalculatorFunctions.Add(EPCalculatorFunction);


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
            //ContattiView.ItemsView.SetNoSelectionChecked(true);
            ContattiView.ItemsView.IsSingleSelection = IsSingleSelection;
            ContattiView.ItemsView.IsImportItemsEnabled = false;

            Guid currentContattiId = ContattiItemSelectedIds.FirstOrDefault();
            ContattiView.ItemsView.SelectEntityById(currentContattiId);

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
        private void ContattiCtrl_EntityViewMouseDoubleClick(object sender, EventArgs e)
        {
            ContattiView.ItemsView.CheckedEntitiesId = new HashSet<Guid>() { ContattiView.ItemsView.SelectedEntityId };
            AcceptSelection();
        }


        private void AcceptSelection()
        {
            //N.B. non so bene perche alla chiusura della finestra questo evento a volte parta 2 volte
            if (DialogResult != null)
                return;

            //prendo solo gli elementi foglie
            IEnumerable<Guid> selectedItemsId = ContattiView.ItemsView.CheckedEntitiesId;//.Where(item => !ContattiView.ItemsView.HasChildren(item));

            UpdateViewSettings();

            if (IsSingleSelection)
            {

                if (selectedItemsId.Count() == 1)
                {
                    ContattiItemSelectedIds = selectedItemsId.ToList<Guid>();
                    DialogResult = true;
                }
                else if (selectedItemsId.Count() > 1)
                {
                    ContattiView.ItemsView.MainOperation.ShowMessageBarView(LocalizationProvider.GetString("NonPossibileSelezionarePiuDiUnaVoce"));
                    DialogResult = false;
                }
                else
                {
                    if (AllowNoSelection && ContattiView.ItemsView.IsNoSelectionChecked)
                    {
                        ContattiItemSelectedIds = new List<Guid>() { Guid.Empty };
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
                ContattiItemSelectedIds = selectedItemsId.ToList<Guid>();
                DialogResult = true;
            }
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
