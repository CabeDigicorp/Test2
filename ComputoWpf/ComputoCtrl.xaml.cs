using Commons;
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
using Syncfusion.UI.Xaml.Grid.Helpers;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.Data;
using MasterDetailModel;
using MasterDetailView;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Model;
using MasterDetailWpf;
using CommonResources;
using System.Windows.Controls.Primitives;
using System.Windows.Forms.Design.Behavior;
using System.Windows.Interop;
using ModelData.Dto;

namespace ComputoWpf
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ComputoCtrl : UserControl
    {
        public ComputoView View { get { return DataContext as ComputoView; } }
        MasterDetailGrid MasterDetailGrid { get; set; } = new MasterDetailGrid();

        public ComputoCtrl()
        {
            InitializeComponent();

            DataContextChanged += ComputoCtrl_DataContextChanged;

        }

        private void ComputoCtrl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (View == null)
                return;

            MasterDetailGrid = new MasterDetailGrid();
            MasterDetailGrid.MasterDataGrid = MasterDataGrid;
            MasterDetailGrid.DetailListView = DetailListView;
            MasterDetailGrid.MasterDetailGridCtrl = this;
            MasterDetailGrid.MasterDetailGridView = View;
            MasterDetailGrid.MasterDetailScale = MasterDetailScale;

            MasterDetailGrid.Init();
        }



        #region Layout scale
        private void Grid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            base.OnPreviewMouseWheel(e);

            MasterDetailGrid.Grid_PreviewMouseWheel(sender, e);
        }

        private void Grid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);

            MasterDetailGrid.Grid_PreviewMouseDown(sender, e);
        }

        #endregion  Layout scale

        private void ColumnChooser_Click(object sender, RoutedEventArgs e)
        {
            MasterDetailGrid.ColumnChooser_Click(sender, e);
        }

        private void MasterDataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            MasterDetailGrid.MasterDataGrid_Loaded(sender, e);
        }

        //private void Copy_Click(object sender, RoutedEventArgs e)
        //{
        //    MasterDetailGrid.Copy_Click(sender, e);
        //}

        //private void Paste_Click(object sender, RoutedEventArgs e)
        //{
        //    MasterDetailGrid.Paste_Click(sender, e);
        //}

        //private void CopyText_Click(object sender, RoutedEventArgs e)
        //{
        //    MasterDetailGrid.CopyText_Click(sender, e);
        //}

        private void DetailListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MasterDetailGrid.DetailListView_PreviewMouseLeftButtonDown(sender, e);
        }

        private void DetailListView_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            MasterDetailGrid.DetailLIstView_PreviewMouseMove(sender, e);
        }

        #region Drag Drop

        private void DetailListView_DragEnter(object sender, DragEventArgs e)
        {
            MasterDetailGrid.DetailListView_DragEnter(sender, e);
        }

        private void RightSplitPaneControl_Drop(object sender, DragEventArgs e)
        {
            MasterDetailGrid.RightSplitPaneControl_Drop(sender, e);
        }

        private void FilterToggleButton_Drop(object sender, DragEventArgs e)
        {
            MasterDetailGrid.FilterToggleButton_Drop(sender, e);
        }

        private void SortToggleButton_Drop(object sender, DragEventArgs e)
        {
            MasterDetailGrid.SortToggleButton_Drop(sender, e);
        }

        private void GroupToggleButton_Drop(object sender, DragEventArgs e)
        {
            MasterDetailGrid.GroupToggleButton_Drop(sender, e);

        }
        #endregion Drag Drop

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            MasterDetailGrid.TextBox_KeyDown(sender, e);
        }

        /// <summary>
        /// Gestisce l'evento di doppio click sul valore di un attributo di tipo AttributoRiferimento
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ValoreAttributo_HandleMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (View.DataService == null || View.DataService.IsReadOnly)
                return;

            FrameworkElement frameworkElement = sender as FrameworkElement;
            if (frameworkElement == null)
                return;

            ValoreView valView = frameworkElement.DataContext as ValoreView;
            if (valView == null)
                return;

            View.ItemsView.ReplaceValore(valView);

        }

        private void ListaFiltri_MouseEnter(object sender, MouseEventArgs e)
        {
            MasterDetailGrid.ListaFiltri_MouseEnter(sender, e);
        }

        private void ListaFiltri_MouseLeave(object sender, MouseEventArgs e)
        {
            MasterDetailGrid.ListaFiltri_MouseLeave(sender, e);
        }

        private void ListaFiltri_HandleDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MasterDetailGrid.ListaFiltri_HandleDoubleClick(sender, e);
        }

        private void ListaSort_MouseEnter(object sender, MouseEventArgs e)
        {
            MasterDetailGrid.ListaSort_MouseEnter(sender, e);
        }

        private void ListaSort_MouseLeave(object sender, MouseEventArgs e)
        {
            MasterDetailGrid.ListaSort_MouseLeave(sender, e);
        }

        private void ListaGroup_MouseEnter(object sender, MouseEventArgs e)
        {
            MasterDetailGrid.ListaGroup_MouseEnter(sender, e);
        }

        private void ListaGroup_MouseLeave(object sender, MouseEventArgs e)
        {
            MasterDetailGrid.ListaGroup_MouseLeave(sender, e);
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            MasterDetailGrid.TextBox_PreviewKeyDown(sender, e);
        }

        private void SetAttributi_Click(object sender, RoutedEventArgs e)
        {
            //ricavo l'attributo corrente
            DetailAttributoView currAttView = DetailListView.SelectedItem as DetailAttributoView;
            string codiceAtt = string.Empty;
            if (currAttView != null)
                codiceAtt = currAttView.CodiceAttributo;


            View.ComputoItemsView.IsMultipleModify = false;
            View.ComputoItemsView.ReadyToModifyEntitiesId.Clear();
            View.ComputoItemsView.UpdateUI();

            string entityTypeKey = View.ComputoItemsView.EntityType.GetKey();
            ClientDataService clientDataService = View.ComputoItemsView.DataService;

            ComputoAttributiSettingsWnd setAttributiWnd = new ComputoAttributiSettingsWnd();
            setAttributiWnd.SourceInitialized += (x, y) => setAttributiWnd.HideMinimizeAndMaximizeButtons();
            setAttributiWnd.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            setAttributiWnd.Init(clientDataService, View.ComputoItemsView.MainOperation, View.ComputoItemsView.WindowService, entityTypeKey, codiceAtt);

            if (setAttributiWnd.ShowDialog() == true)
            {
                View.UpdateEntityType();

                List<string> dependentEntityTypes = View.ItemsView.GetDependentEntityTypesKey();
                View.MainOperation.UpdateEntityTypesView(dependentEntityTypes);
            }


        }

        private void GridSplitter_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            MasterDetailGrid.GridSplitter_DragCompleted(sender, e);

        }

        private void ExportExcel_Click(object sender, RoutedEventArgs e)
        {
            MasterDetailGrid.ExportExcel_Click(sender, e);
        }

        private void RightSplitPaneControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MasterDetailGrid.RightSplitPaneControl_PreviewMouseLeftButtonDown(sender, e);
        }

        private void RightSplitPaneControl_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            MasterDetailGrid.RightSplitPaneControl_PreviewMouseMove(sender, e);
        }
    }
}




