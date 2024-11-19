using CommonResources;
using Commons;
using MasterDetailModel;
using MasterDetailView;
using MasterDetailWpf;
using Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ElementiWpf
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ElementiCtrl : UserControl
    {
        public ElementiView View { get { return DataContext as ElementiView; } }
        MasterDetailGrid MasterDetailGrid { get; set; } = new MasterDetailGrid();

        public ElementiCtrl()
        {
            InitializeComponent();

            DataContextChanged += ElementiCtrl_DataContextChanged;

            //Init();

            //DataContext = new ElementiView();

            //MasterDetailGrid = new MasterDetailGrid();
            //MasterDetailGrid.MasterDataGrid = MasterDataGrid;
            //MasterDetailGrid.DetailListView = DetailListView;
            //MasterDetailGrid.MasterDetailGridCtrl = this;
            //MasterDetailGrid.MasterDetailGridView = View;
            //MasterDetailGrid.MasterDetailScale = MasterDetailScale;

            //MouseMove += ElementiCtrl_MouseMove;

            //MasterDetailGrid.Init();

        }

        private void ElementiCtrl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Init();
        }

        private void Init()
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

        //private void ElementiCtrl_MouseMove(object sender, MouseEventArgs e)
        //{

        //    //Debug.WriteLine(e.OriginalSource.ToString());
        //}



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

        private void DetailListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MasterDetailGrid.DetailListView_PreviewMouseLeftButtonDown(sender, e);
        }

        private void DetailListView_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            MasterDetailGrid.DetailLIstView_PreviewMouseMove(sender, e);
        }

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

            AttributoRiferimento attRif = View.ItemsView.EntityType.Attributi[valView.Tag as string] as AttributoRiferimento;
            if (attRif == null)
                return;

            if (attRif.ReferenceEntityTypeKey.StartsWith(BuiltInCodes.EntityType.Divisione))
            {
                View.ReplaceCurrentItemDivisioneGuid(attRif);
            }
            else
            {
                View.ReplaceCurrentItemAttributoGuid(attRif, null);
            }
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

        /// <summary>
        /// Scopo: separatore decimale nel tasterino numerico secondo le impostazioni di windows
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

            View.ElementiItemsView.IsMultipleModify = false;
            View.ElementiItemsView.ReadyToModifyEntitiesId.Clear();
            View.ElementiItemsView.UpdateUI();

            string entityTypeKey = View.ElementiItemsView.EntityType.GetKey();
            ClientDataService clientDataService = View.ElementiItemsView.DataService;

            ElementiAttributiSettingsWnd setAttributiWnd = new ElementiAttributiSettingsWnd();
            setAttributiWnd.SourceInitialized += (x, y) => setAttributiWnd.HideMinimizeAndMaximizeButtons();
            setAttributiWnd.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            setAttributiWnd.Init(clientDataService, View.ElementiItemsView.MainOperation, View.ElementiItemsView.WindowService, entityTypeKey, codiceAtt);

            if (setAttributiWnd.ShowDialog() == true)
            {
                View.UpdateEntityType();

                List<string> detendentEntityTypes = View.ItemsView.GetDependentEntityTypesKey();
                View.MainOperation.UpdateEntityTypesView(detendentEntityTypes);
            }
        }

        private void GridSplitter_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            MasterDetailGrid.GridSplitter_DragCompleted(sender, e);
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
