using CommonResources;
using Commons;
using MasterDetailModel;
using MasterDetailView;
using Model;
using StampeWpf.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;


namespace StampeWpf
{
    /// <summary>
    /// Interaction logic for SelectArticoliWindow.xaml
    /// </summary>
    public partial class SelectDocumentiIdsWindow : Window
    {
        DocumentiView DocumentiView { get { return DocumentiCtrl.DataContext as DocumentiView; } }

        public ClientDataService DataService { get; set; } = null;//ref
        public ModelActionsStack ModelActionsStack { get; set; } = null;//ref
        public IEntityWindowService WindowService { get; set; } = null;//ref
        public IMainOperation MainOperation { get; set; } = null;//ref

        /// <summary>
        /// Items selezionati (checkati) alla chiusura o apertura del dialogo
        /// </summary>
        public List<Guid> DocumentiItemSelectedIds { get; set; } = new List<Guid>();


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

        //public NoteCalculatorFunction NoteCalculatorFunction { get; set; } = null;
        //public EPCalculatorFunction EPCalculatorFunction { get; set; } = null;

        public SelectDocumentiIdsWindow()
        {
            InitializeComponent();
            base.Closed += SelectDocumentiIdsWindow_Closed;
        }

        private void SelectDocumentiIdsWindow_Closed(object sender, EventArgs e)
        {
            DocumentiView.ItemsView.AllowNoSelection = false;
        }

        public void Init()
        {
            DocumentiCtrl.Init();
            
            //DocumentiView.CalculatorFunctions.Add(NoteCalculatorFunction);
            //DocumentiView.CalculatorFunctions.Add(EPCalculatorFunction);


            DocumentiView.DataService = DataService;
            DocumentiView.WindowService = WindowService;
            DocumentiView.ModelActionsStack = ModelActionsStack;
            DocumentiView.MainOperation = MainOperation;
            //DocumentiView.Init(CurrentViewSettings);
            
            DocumentiView.Init(null);//per ora non imposto filtri e ordinamento all'apertura del dialogo

            DocumentiView.ItemsView.RightPanesView.ClosePanes();

            HashSet<Guid> documentiItemSelectedIds = new HashSet<Guid>(DocumentiItemSelectedIds);
            if (documentiItemSelectedIds.Contains(Guid.Empty))
            {
                DocumentiView.ItemsView.SetNoSelectionChecked(true);
                documentiItemSelectedIds.Remove(Guid.Empty);
            }
            else
                DocumentiView.ItemsView.SetNoSelectionChecked(false);

            DocumentiView.ItemsView.CheckedEntitiesId = new HashSet<Guid>(documentiItemSelectedIds);

            DocumentiView.ItemsView.AllowNoSelection = AllowNoSelection;
            //DocumentiView.ItemsView.SetNoSelectionChecked(true);
            DocumentiView.ItemsView.IsSingleSelection = IsSingleSelection;
            DocumentiView.ItemsView.IsImportItemsEnabled = false;

            Guid currentDocumentiId = DocumentiItemSelectedIds.FirstOrDefault();

            DocumentiView.ItemsView.SelectEntityById(currentDocumentiId);
           
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
        private void DocumentiCtrl_EntityViewMouseDoubleClick(object sender, EventArgs e)
        {
            DocumentiView.ItemsView.CheckedEntitiesId = new HashSet<Guid>() { DocumentiView.ItemsView.SelectedEntityId };
            AcceptSelection();
        }


        private void AcceptSelection()
        {
            //N.B. non so bene perche alla chiusura della finestra questo evento a volte parta 2 volte
            if (DialogResult != null)
                return;

            //prendo solo gli elementi con prezzo (foglie)
            IEnumerable<Guid> selectedItemsId = DocumentiView.ItemsView.CheckedEntitiesId.Where(item => !DocumentiView.ItemsView.HasChildren(item));

            UpdateViewSettings();

            if (IsSingleSelection)
            {

                if (selectedItemsId.Count() == 1)
                {
                    DocumentiItemSelectedIds = selectedItemsId.ToList<Guid>();
                    DialogResult = true;
                }
                else if (selectedItemsId.Count() > 1)
                {
                    DocumentiView.ItemsView.MainOperation.ShowMessageBarView(LocalizationProvider.GetString("NonPossibileSelezionarePiuDiUnaVoce"));
                    DialogResult = false;
                }
                else
                {
                    if (AllowNoSelection && DocumentiView.ItemsView.IsNoSelectionChecked)
                    {
                        DocumentiItemSelectedIds = new List<Guid>() { Guid.Empty };
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
                DocumentiItemSelectedIds = selectedItemsId.ToList<Guid>();
                DialogResult = true;
            }
        }

        private void UpdateViewSettings()
        {


            //if (DocumentiView.ItemsView.RightPanesView.GroupView.Data != null && DocumentiView.ItemsView.RightPanesView.GroupView.Data.Items.Any())
            //    CurrentViewSettings.Groups = DocumentiView.ItemsView.RightPanesView.GroupView.Data.Items.Select(item => item.Clone()).ToList();

            //if (DocumentiView.ItemsView.RightPanesView.SortView.Data != null && DocumentiView.ItemsView.RightPanesView.SortView.Data.Items.Any())
            //    CurrentViewSettings.Sorts = DocumentiView.ItemsView.RightPanesView.SortView.Data.Items.Select(item => item.Clone()).ToList();

            //if (DocumentiView.ItemsView.RightPanesView.FilterView.Data != null && DocumentiView.ItemsView.RightPanesView.FilterView.Data.Items.Any())
            //    CurrentViewSettings.Filters = DocumentiView.ItemsView.RightPanesView.FilterView.Data.Items.Select(item => item.Clone()).ToList();

            DocumentiView.ItemsView.RightPanesView.UpdateViewSettings(CurrentViewSettings);

        }


    }
}
