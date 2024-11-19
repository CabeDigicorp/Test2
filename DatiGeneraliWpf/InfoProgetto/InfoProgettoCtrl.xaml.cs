using CommonResources;
using Commons;
using DatiGeneraliWpf.View;
using MasterDetailModel;
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

namespace DatiGeneraliWpf
{
    /// <summary>
    /// Interaction logic for InfoProgettoCtrl.xaml
    /// </summary>
    public partial class InfoProgettoCtrl : UserControl
    {
        InfoProgettoView View { get => DataContext as InfoProgettoView; }
        MasterDetailList MasterDetailList { get; set; } = new MasterDetailList();

        bool _isInitialized = false;

        public InfoProgettoCtrl()
        {
            InitializeComponent();

            Loaded += InfoProgettoCtrl_Loaded1;
            //Init();
        }

        private void InfoProgettoCtrl_Loaded1(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized)
                Init();
        }

        public event EventHandler EntityViewMouseDoubleClick;
        protected virtual void OnEntityViewMouseDoubleClick(EventArgs e)
        {
            EntityViewMouseDoubleClick?.Invoke(this, e);
        }

        public void Init()
        {
            View.ItemsView.RefreshView += ItemsView_RefreshView;
            Loaded += InfoProgettoCtrl_Loaded;
            //ContattiItemList.Loaded += ContattiItemList_Loaded;

            MasterDetailList = new MasterDetailList();
            //MasterDetailList.MasterListView = InfoProgettoItemList;
            MasterDetailList.DetailListView = DetailListView;
            MasterDetailList.MasterDetailListCtrl = this;
            MasterDetailList.MasterDetailView = View;
            MasterDetailList.MasterDetailScale = MasterDetailScale;

            _isInitialized = true;
        }

        private async void InfoProgettoCtrl_Loaded(object sender, RoutedEventArgs e)
        {
            await View.ItemsView.UpdateCache(true);
            //throw new NotImplementedException();
        }

        //private void ContattiItemList_Loaded(object sender, RoutedEventArgs e)
        //{
        //    MasterDetailList.ContattiItemList_Loaded(sender, e);
        //}

        private void ItemsView_RefreshView(object sender, EventArgs e)
        {
            //MasterDetailList.ItemsView_RefreshView(sender, e);
            if (View != null)
                View.Refresh(0);

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

            InfoProgettoAttributiSettingsWnd setAttributiWnd = new InfoProgettoAttributiSettingsWnd();
            setAttributiWnd.SourceInitialized += (x, y) => setAttributiWnd.HideMinimizeAndMaximizeButtons();
            setAttributiWnd.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            setAttributiWnd.Init(clientDataService, View.ItemsView.MainOperation, View.ItemsView.WindowService, entityTypeKey, codiceAtt);

            if (setAttributiWnd.ShowDialog() == true)
            {
                View.UpdateEntityType();

                List<string> detendentEntityTypes = View.ItemsView.GetDependentEntityTypesKey();
                View.MainOperation.UpdateEntityTypesView(detendentEntityTypes);

                //View.MainOperation.UpdateEntityTypes(new List<string>()
                //{   BuiltInCodes.EntityType.Prezzario,
                //    BuiltInCodes.EntityType.Elementi,
                //    BuiltInCodes.EntityType.Computo });
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

        //private void listViewDetail_DragEnter(object sender, DragEventArgs e)
        //{
        //    MasterDetailList.listViewDetail_DragEnter(sender, e);
        //}

        //private void RightSplitPaneControl_DragOver(object sender, DragEventArgs e)
        //{
        //    MasterDetailList.RightSplitPaneControl_DragOver(sender, e);
        //}

        //private void RightSplitPaneControl_Drop(object sender, DragEventArgs e)
        //{
        //   MasterDetailList.RightSplitPaneControl_Drop(sender, e);
        //}

        //private void FilterToggleButton_Drop(object sender, DragEventArgs e)
        //{
        //    MasterDetailList.FilterToggleButton_Drop(sender, e);
        //}

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

        //private void SortToggleButton_Drop(object sender, DragEventArgs e)
        //{
        //    //MasterDetailList.SortToggleButton_Drop(sender, e);
        //}

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            MasterDetailList.TextBox_PreviewKeyDown(sender, e);
        }

        //private void ListaFiltri_MouseEnter(object sender, MouseEventArgs e)
        //{
        //    //MasterDetailList.ListaFiltri_MouseEnter(sender, e);
        //}

        //private void ListaFiltri_MouseLeave(object sender, MouseEventArgs e)
        //{
        //    //MasterDetailList.ListaSort_MouseLeave(sender, e);
        //}

        //private void ListaSort_MouseEnter(object sender, MouseEventArgs e)
        //{
        //    //MasterDetailList.ListaSort_MouseEnter(sender, e);
        //}

        //private void ListaSort_MouseLeave(object sender, MouseEventArgs e)
        //{
        //    //MasterDetailList.ListaSort_MouseLeave(sender, e);
        //}

        private void GridSplitter_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
           MasterDetailList.GridSplitter_DragCompleted(sender, e);
        }

        //private void ListViewItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    //MasterDetailList.ListViewItem_MouseLeftButtonDown(sender, e);
        //}

        //private void ListaFiltri_HandleDoubleClick(object sender, MouseButtonEventArgs e)
        //{
        //    //MasterDetailList.ListaFiltri_HandleDoubleClick(sender, e);
        //}
    }


}
