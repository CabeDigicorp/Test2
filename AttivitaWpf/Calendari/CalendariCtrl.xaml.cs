using AttivitaWpf.View;
using CommonResources;
using Commons;
using MasterDetailView;
using MasterDetailWpf;
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

namespace AttivitaWpf
{
    /// <summary>
    /// Interaction logic for CalendariCtrl.xaml
    /// </summary>
    public partial class CalendariCtrl : UserControl
    {
        public CalendariView View { get { return DataContext as CalendariView; } }
        MasterDetailList MasterDetailList { get; set; } = new MasterDetailList();

        bool _isInitialized = false;

        public event EventHandler EntityViewMouseDoubleClick;
        protected virtual void OnEntityViewMouseDoubleClick(object sender, EventArgs e)
        {
            EntityViewMouseDoubleClick?.Invoke(sender, e);
        }

        public CalendariCtrl()
        {
            InitializeComponent();

            //DataContextChanged += CalendariCtrl_DataContextChanged;

            Loaded += CalendariCtrl_Loaded;

            //Init();

        }

        private void CalendariCtrl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //if (!_isInitialized)
            //    Init();
        }

        private void CalendariCtrl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized)
                Init();
        }

        public void Init()
        {
            //DataContext = new CalendariView();

            //System.Diagnostics.PresentationTraceSources.SetTraceLevel(this, System.Diagnostics.PresentationTraceLevel.High);

            if (View == null)
                return;

            View.ItemsView.RefreshView += ItemsView_RefreshView;
            View.ItemsView.ItemsLoaded += ItemsView_ItemsLoaded;
            CalendariItemList.Loaded += CalendariItemList_Loaded;

            MasterDetailList = new MasterDetailList();
            MasterDetailList.MasterListView = CalendariItemList;
            MasterDetailList.DetailListView = DetailListView;
            MasterDetailList.MasterDetailListCtrl = this;
            MasterDetailList.MasterDetailView = View;
            MasterDetailList.MasterDetailScale = MasterDetailScale;
            //
            MasterDetailList.EntityViewMouseDoubleClick += MasterDetailList_EntityViewMouseDoubleClick;

            View.Init(null);
            _isInitialized = true;
        }

        private void ItemsView_ItemsLoaded(object sender, EventArgs e)
        {
            MasterDetailList.ItemsView_ItemsLoaded(sender, e);
        }

        private void MasterDetailList_EntityViewMouseDoubleClick(object sender, EventArgs e)
        {
            OnEntityViewMouseDoubleClick(sender, e);
        }

        private void CalendariItemList_Loaded(object sender, RoutedEventArgs e)
        {
            MasterDetailList.ItemList_Loaded(sender, e);
        }

        private void ItemsView_RefreshView(object sender, EventArgs e)
        {
            MasterDetailList.ItemsView_RefreshView(sender, e);
        }

        private void SetAttributi_Click(object sender, RoutedEventArgs e)
        {
            //ricavo l'attributo corrente
            DetailAttributoView currAttView = DetailListView.SelectedItem as DetailAttributoView;
            string codiceAtt = string.Empty;
            if (currAttView != null)
                codiceAtt = currAttView.CodiceAttributo;


            View.ItemsView.IsMultipleModify = false;
            View.ItemsView.ReadyToModifyEntitiesId.Clear();
            View.ItemsView.UpdateUI();

            string entityTypeKey = View.ItemsView.EntityType.GetKey();
            ClientDataService clientDataService = View.ItemsView.DataService;

            CalendariAttributiSettingsWnd setAttributiWnd = new CalendariAttributiSettingsWnd();
            setAttributiWnd.SourceInitialized += (x, y) => setAttributiWnd.HideMinimizeAndMaximizeButtons();
            setAttributiWnd.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            setAttributiWnd.Init(clientDataService, View.ItemsView.MainOperation, View.ItemsView.WindowService, entityTypeKey, codiceAtt);

            if (setAttributiWnd.ShowDialog() == true)
            {
                View.UpdateEntityType();

                List<string> detendentEntityTypes = View.ItemsView.GetDependentEntityTypesKey();
                View.MainOperation.UpdateEntityTypesView(detendentEntityTypes);
            }
        }

        private void listViewDetail_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MasterDetailList.listViewDetail_PreviewMouseLeftButtonDown(sender, e);
        }

        private void listViewDetail_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            MasterDetailList.listViewDetail_PreviewMouseMove(sender, e);
        }

        private void listViewDetail_DragEnter(object sender, DragEventArgs e)
        {
            MasterDetailList.listViewDetail_DragEnter(sender, e);
        }

        private void RightSplitPaneControl_DragOver(object sender, DragEventArgs e)
        {
            MasterDetailList.RightSplitPaneControl_DragOver(sender, e);
        }

        private void RightSplitPaneControl_Drop(object sender, DragEventArgs e)
        {
            MasterDetailList.RightSplitPaneControl_Drop(sender, e);
        }

        private void FilterToggleButton_Drop(object sender, DragEventArgs e)
        {
            MasterDetailList.FilterToggleButton_Drop(sender, e);
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            MasterDetailList.TextBox_KeyDown(sender, e);
        }

        private void ValoreAttributo_HandleMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MasterDetailList.ValoreAttributo_HandleMouseDoubleClick(sender, e);
        }

        private void Grid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            base.OnPreviewMouseWheel(e);
            MasterDetailList.Grid_PreviewMouseWheel(sender, e);
        }

        private void Grid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
            MasterDetailList.Grid_PreviewMouseDown(sender, e);
        }

        private void SortToggleButton_Drop(object sender, DragEventArgs e)
        {
            MasterDetailList.SortToggleButton_Drop(sender, e);
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            MasterDetailList.TextBox_PreviewKeyDown(sender, e);
        }

        private void ListaFiltri_MouseEnter(object sender, MouseEventArgs e)
        {
            MasterDetailList.ListaFiltri_MouseEnter(sender, e);
        }

        private void ListaFiltri_MouseLeave(object sender, MouseEventArgs e)
        {
            MasterDetailList.ListaSort_MouseLeave(sender, e);
        }

        private void ListaSort_MouseEnter(object sender, MouseEventArgs e)
        {
            MasterDetailList.ListaSort_MouseEnter(sender, e);
        }

        private void ListaSort_MouseLeave(object sender, MouseEventArgs e)
        {
            MasterDetailList.ListaSort_MouseLeave(sender, e);
        }

        private void GridSplitter_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            MasterDetailList.GridSplitter_DragCompleted(sender, e);
        }

        private void ListViewItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MasterDetailList.ListViewItem_MouseLeftButtonDown(sender, e);
        }

        private void ListaFiltri_HandleDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MasterDetailList.ListaFiltri_HandleDoubleClick(sender, e);
        }
    }
}