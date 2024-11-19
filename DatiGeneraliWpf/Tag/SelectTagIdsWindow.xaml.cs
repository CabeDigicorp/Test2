using _3DModelExchange;
using DatiGeneraliWpf.View;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DatiGeneraliWpf
{
    /// <summary>
    /// Interaction logic for SelectTagIdsWindow.xaml
    /// </summary>
    public partial class SelectTagIdsWindow : Window
    {
        TagView TagView { get { return TagCtrl.DataContext as TagView; } }

        public ClientDataService DataService { get; set; } = null;//ref
        public ModelActionsStack ModelActionsStack { get; set; } = null;//ref
        public IEntityWindowService WindowService { get; set; } = null;//ref
        public IMainOperation MainOperation { get; set; } = null;//ref

        /// <summary>
        /// Items selezionati (checkati) alla chiusura o apertura del dialogo
        /// </summary>
        public List<Guid> TagItemSelectedIds { get; set; } = new List<Guid>();

        /// <summary>
        /// POssibilità di selezionare solo un elemento
        /// </summary>
        public bool IsSingleSelection { get; set; } = false;

        /// <summary>
        /// Possibilità di assegnare nessun articolo
        /// </summary>
        public bool AllowNoSelection { get; set; } = false;

        /// <summary>
        /// Filtro attivo all'avvio del dialogo
        /// </summary>
        public EntityTypeViewSettings ViewSettings = null;

        /// <summary>
        /// Impostazioni di Filtro, Ordine e Raggruppamento all'avvio del dialogo 
        /// </summary>
        public EntityTypeViewSettings CurrentViewSettings { get; set; } = new EntityTypeViewSettings();

        public NoteCalculatorFunction NoteCalculatorFunction { get; set; } = null;
        public EPCalculatorFunction EPCalculatorFunction { get; set; } = null;

        public SelectTagIdsWindow()
        {
            InitializeComponent();

            DataContext = new TagView();

            base.Closed += SelectContattiIdsWindow_Closed;
        }

        private void SelectContattiIdsWindow_Closed(object sender, EventArgs e)
        {
            TagView.ItemsView.AllowNoSelection = false;
        }

        public void Init()
        {
            TagCtrl.Init();

            TagView.CalculatorFunctions.Add(NoteCalculatorFunction);
            TagView.CalculatorFunctions.Add(EPCalculatorFunction);


            TagView.DataService = DataService;
            TagView.WindowService = WindowService;
            TagView.ModelActionsStack = ModelActionsStack;
            TagView.MainOperation = MainOperation;

            TagView.Init(ViewSettings);

            TagView.ItemsView.RightPanesView.ClosePanes();

            HashSet<Guid> elencoAttivitaItemSelectedIds = new HashSet<Guid>(TagItemSelectedIds);
            if (elencoAttivitaItemSelectedIds.Contains(Guid.Empty))
            {
                TagView.ItemsView.SetNoSelectionChecked(true);
                elencoAttivitaItemSelectedIds.Remove(Guid.Empty);
            }
            else
                TagView.ItemsView.SetNoSelectionChecked(false);

            TagView.ItemsView.CheckedEntitiesId = new HashSet<Guid>(elencoAttivitaItemSelectedIds);
            TagView.ItemsView.AllowNoSelection = AllowNoSelection;
            TagView.ItemsView.IsSingleSelection = IsSingleSelection;
            TagView.ItemsView.IsImportItemsEnabled = false;

            Guid currentContattiId = TagItemSelectedIds.FirstOrDefault();
            TagView.ItemsView.SelectEntityById(currentContattiId);

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
        private void TagCtrl_EntityViewMouseDoubleClick(object sender, EventArgs e)
        {
            TagView.ItemsView.CheckedEntitiesId = new HashSet<Guid>() { TagView.ItemsView.SelectedEntityId };
            AcceptSelection();
        }


        private void AcceptSelection()
        {
            //N.B. non so bene perche alla chiusura della finestra questo evento a volte parta 2 volte
            if (DialogResult != null)
                return;

            //prendo solo gli elementi foglie
            IEnumerable<Guid> selectedItemsId = TagView.ItemsView.CheckedEntitiesId;//.Where(item => !ContattiView.ItemsView.HasChildren(item));

            UpdateViewSettings();

            if (IsSingleSelection)
            {

                if (selectedItemsId.Count() == 1)
                {
                    TagItemSelectedIds = selectedItemsId.ToList<Guid>();
                    DialogResult = true;
                }
                else if (selectedItemsId.Count() > 1)
                {
                    TagView.ItemsView.MainOperation.ShowMessageBarView(LocalizationProvider.GetString("NonPossibileSelezionarePiuDiUnaVoce"));
                    DialogResult = false;
                }
                else
                {
                    if (AllowNoSelection && TagView.ItemsView.IsNoSelectionChecked)
                    {
                        TagItemSelectedIds = new List<Guid>() { Guid.Empty };
                        DialogResult = true;
                    }
                    else
                    {
                        MainOperation.ShowMessageBarView(LocalizationProvider.GetString("OccorreSelezionareUnAttivita"));
                        DialogResult = false;
                    }
                }


            }
            else
            {
                TagItemSelectedIds = selectedItemsId.ToList<Guid>();
                DialogResult = true;
            }
        }

        private void UpdateViewSettings()
        {

            TagView.ItemsView.RightPanesView.UpdateViewSettings(CurrentViewSettings);

        }
    }
}
