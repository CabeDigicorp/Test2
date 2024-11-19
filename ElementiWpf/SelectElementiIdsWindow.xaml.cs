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
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ElementiWpf
{
    /// <summary>
    /// Interaction logic for SelectArticoliWindow.xaml
    /// </summary>
    public partial class SelectElementiIdsWindow : Window
    {
        ElementiView ElementiView { get { return ElementiCtrl.DataContext as ElementiView; } }

        public ClientDataService DataService { get; set; } = null;//ref
        public ModelActionsStack ModelActionsStack { get; set; } = null;//ref
        public IEntityWindowService WindowService { get; set; } = null;//ref
        public IMainOperation MainOperation { get; set; } = null;//ref

        /// <summary>
        /// Items selezionati (checkati) alla chiusura o apertura del dialogo
        /// </summary>
        public List<Guid> ElementiItemSelectedIds { get; set; } = new List<Guid>();

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
        /// Impostazioni di Filtro, Ordine e Raggruppamento all'avvio del dialogo 
        /// </summary>
        public EntityTypeViewSettings CurrentViewSettings { get; set; } = new EntityTypeViewSettings();

        public SelectElementiIdsWindow()
        {
            InitializeComponent();

            ElementiCtrl.DataContext = new ElementiView();

            base.Closed += SelectPrezzarioIdsWindow_Closed;
        }


        private void SelectPrezzarioIdsWindow_Closed(object sender, EventArgs e)
        {
            ElementiView.ElementiItemsView.AllowNoSelection = false;
        }

        public void Init()
        {
            ElementiView.DataService = DataService;
            ElementiView.WindowService = WindowService;
            ElementiView.ModelActionsStack = ModelActionsStack;
            ElementiView.MainOperation = MainOperation;

            //ElementiView.Init(CurrentViewSettings);
            ElementiView.Init(null);

            HashSet<Guid> elementiItemSelectedIds = new HashSet<Guid>(ElementiItemSelectedIds);
            if (elementiItemSelectedIds.Contains(Guid.Empty))
            {
                ElementiView.ElementiItemsView.SetNoSelectionChecked(true);
                elementiItemSelectedIds.Remove(Guid.Empty);
            }
            else
                ElementiView.ElementiItemsView.SetNoSelectionChecked(false);

            ElementiView.ElementiItemsView.CheckedEntitiesId = elementiItemSelectedIds;
            ElementiView.ElementiItemsView.AllowNoSelection = AllowNoSelection;
            ElementiView.ElementiItemsView.IsSingleSelection = IsSingleSelection;
            ElementiView.ItemsView.IsImportItemsEnabled = false;

            Guid currentElementoId = ElementiItemSelectedIds.FirstOrDefault();
            ElementiView.ElementiItemsView.SelectEntityById(currentElementoId);
           
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            if (DialogResult != null)
                return;

            //prendo solo gli elementi con prezzo (foglie)
            //IEnumerable<Guid> selectedItemsId = ElementiView.ElementiItemsView.CheckedEntitiesId.Where(item => !ElementiView.ElementiItemsView.HasChildren(item));
            IEnumerable<Guid> selectedItemsId = ElementiView.ElementiItemsView.CheckedEntitiesId;

            UpdateViewSettings();

            if (IsSingleSelection)
            {

                if (selectedItemsId.Count() == 1)
                {
                    ElementiItemSelectedIds = selectedItemsId.ToList<Guid>();
                    DialogResult = true;
                }
                else if (selectedItemsId.Count() > 1)
                {
                    MainOperation.ShowMessageBarView(LocalizationProvider.GetString("NonPossibileSelezionarePiuDiUnaVoce"));
                    DialogResult = false;
                }
                else
                {
                    if (AllowNoSelection && ElementiView.ElementiItemsView.IsNoSelectionChecked)
                    {
                        ElementiItemSelectedIds = new List<Guid>() { Guid.Empty };
                        DialogResult = true;
                    }
                    else
                    {
                        //MessageBox.Show(LocalizationProvider.GetString("OccorreSelezionareUnaVoce"), LocalizationProvider.GetString("MainApp"));
                        MainOperation.ShowMessageBarView(LocalizationProvider.GetString("OccorreSelezionareUnaVoce"));
                        DialogResult = false;
                    }
                }


            }
            else
            {
                ElementiItemSelectedIds = selectedItemsId.ToList<Guid>();
                DialogResult = true;
            }


        }

        private void UpdateViewSettings()
        {
            //if (ElementiView.ElementiItemsView.RightPanesView.GroupView.Data != null && ElementiView.ElementiItemsView.RightPanesView.GroupView.Data.Items.Any())
            //    CurrentViewSettings.Groups = ElementiView.ElementiItemsView.RightPanesView.GroupView.Data.Items.Select(item => item.Clone()).ToList();

            //if (ElementiView.ElementiItemsView.RightPanesView.SortView.Data != null && ElementiView.ElementiItemsView.RightPanesView.SortView.Data.Items.Any())
            //    CurrentViewSettings.Sorts = ElementiView.ElementiItemsView.RightPanesView.SortView.Data.Items.Select(item => item.Clone()).ToList();

            //if (ElementiView.ElementiItemsView.RightPanesView.FilterView.Data != null && ElementiView.ElementiItemsView.RightPanesView.FilterView.Data.Items.Any())
            //    CurrentViewSettings.Filters = ElementiView.ElementiItemsView.RightPanesView.FilterView.Data.Items.Select(item => item.Clone()).ToList();

            ElementiView.ItemsView.RightPanesView.UpdateViewSettings(CurrentViewSettings);

        }


    }
}
