using CommonResources;
using Commons;
using MasterDetailModel;
using MasterDetailView;
using Model;
using Syncfusion.UI.Xaml.Grid;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


namespace ComputoWpf
{
    /// <summary>
    /// Interaction logic for SelectArticoliWindow.xaml
    /// </summary>
    public partial class SelectComputoIdsWindow : Window
    {
        ComputoView ComputoView { get { return ComputoCtrl.DataContext as ComputoView; } }

        public ClientDataService DataService { get; set; } = null;//ref
        public ModelActionsStack ModelActionsStack { get; set; } = null;//ref
        public IEntityWindowService WindowService { get; set; } = null;//ref
        public IMainOperation MainOperation { get; set; } = null;//ref

        /// <summary>
        /// Items selezionati (checkati) alla chiusura o apertura del dialogo
        /// </summary>
        public List<Guid> ComputoItemSelectedIds { get; set; } = new List<Guid>();

        ///// <summary>
        ///// Fuoco su Prezzo all'apertura del dialogo
        ///// </summary>
        //public Guid CurrentElementoId { get; set; } = Guid.Empty;

        /// <summary>
        /// POssibilità di selezionare solo un elemento
        /// </summary>
        public bool IsSingleSelection { get; set; } = false;

        /// <summary>
        /// Possibilità di assegnare nessun articolo
        /// </summary>
        public bool AllowNoSelection { get; set; } = false;

        /// <summary>
        /// Possibilità di disabilitare il pulsante di OK 
        /// </summary>
        public bool AllowAcceptSelection { get; set; } = true;

        /// <summary>
        /// Impostazioni di Filtro, Ordine e Raggruppamento all'avvio del dialogo 
        /// </summary>
        public EntityTypeViewSettings CurrentViewSettings { get; set; } = new EntityTypeViewSettings();

        public SelectComputoIdsWindow()
        {
            InitializeComponent();

            ComputoCtrl.DataContext = new ComputoView();
            
            base.Closed += SelectPrezzarioIdsWindow_Closed;
        }


        private void SelectPrezzarioIdsWindow_Closed(object sender, EventArgs e)
        {
            ComputoView.ComputoItemsView.AllowNoSelection = false;
        }

        public void Init()
        {
            ComputoView.DataService = DataService;
            ComputoView.WindowService = WindowService;
            ComputoView.ModelActionsStack = ModelActionsStack;
            ComputoView.MainOperation = MainOperation;

            //ComputoView.Init(CurrentViewSettings);
            ComputoView.Init(null);

            HashSet<Guid> computoItemSelectedIds = new HashSet<Guid>(ComputoItemSelectedIds);
            if (computoItemSelectedIds.Contains(Guid.Empty))
            {
                ComputoView.ComputoItemsView.SetNoSelectionChecked(true);
                computoItemSelectedIds.Remove(Guid.Empty);
            }
            else
                ComputoView.ComputoItemsView.SetNoSelectionChecked(false);

            ComputoView.ComputoItemsView.CheckedEntitiesId = computoItemSelectedIds;
            ComputoView.ComputoItemsView.AllowNoSelection = AllowNoSelection;
            ComputoView.ComputoItemsView.IsSingleSelection = IsSingleSelection;
            ComputoView.ItemsView.IsImportItemsEnabled = false;

            Guid currentElementoId = ComputoItemSelectedIds.FirstOrDefault();
            ComputoView.ComputoItemsView.SelectEntityById(currentElementoId);


            AcceptButton.IsEnabled = AllowAcceptSelection;
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            if (DialogResult != null)
                return;

            //prendo solo gli elementi con prezzo (foglie)
            //IEnumerable<Guid> selectedItemsId = ComputoView.ComputoItemsView.CheckedEntitiesId.Where(item => !ComputoView.ComputoItemsView.HasChildren(item));
            IEnumerable<Guid> selectedItemsId = ComputoView.ComputoItemsView.CheckedEntitiesId;

            UpdateViewSettings();

            if (IsSingleSelection)
            {

                if (selectedItemsId.Count() == 1)
                {
                    ComputoItemSelectedIds = selectedItemsId.ToList<Guid>();
                    DialogResult = true;
                }
                else if (selectedItemsId.Count() > 1)
                {
                    MainOperation.ShowMessageBarView(LocalizationProvider.GetString("NonPossibileSelezionarePiuDiUnaVoce"));
                    DialogResult = false;
                }
                else
                {
                    if (AllowNoSelection/* && ComputoView.ComputoItemsView.IsNoSelectionChecked*/)
                    {
                        ComputoItemSelectedIds = new List<Guid>() { Guid.Empty };
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
                ComputoItemSelectedIds = selectedItemsId.ToList<Guid>();
                DialogResult = true;
            }


        }

        private void UpdateViewSettings()
        {
            //if (ComputoView.ComputoItemsView.RightPanesView.GroupView.Data != null && ComputoView.ComputoItemsView.RightPanesView.GroupView.Data.Items.Any())
            //    CurrentViewSettings.Groups = ComputoView.ComputoItemsView.RightPanesView.GroupView.Data.Items.Select(item => item.Clone()).ToList();

            //if (ComputoView.ComputoItemsView.RightPanesView.SortView.Data != null && ComputoView.ComputoItemsView.RightPanesView.SortView.Data.Items.Any())
            //    CurrentViewSettings.Sorts = ComputoView.ComputoItemsView.RightPanesView.SortView.Data.Items.Select(item => item.Clone()).ToList();

            //if (ComputoView.ComputoItemsView.RightPanesView.FilterView.Data != null && ComputoView.ComputoItemsView.RightPanesView.FilterView.Data.Items.Any())
            //    CurrentViewSettings.Filters = ComputoView.ComputoItemsView.RightPanesView.FilterView.Data.Items.Select(item => item.Clone()).ToList();

            ComputoView.ComputoItemsView.RightPanesView.UpdateViewSettings(CurrentViewSettings);

        }


    }
}
