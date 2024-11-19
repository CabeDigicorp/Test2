using CommonResources;
using MasterDetailModel;
using MasterDetailView;
using MasterDetailWpf;
using Model;
using StampeWpf.View;
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

namespace StampeWpf
{
    /// <summary>
    /// Interaction logic for ReportCtrl.xaml
    /// </summary>
    public partial class ReportCtrl : UserControl
    {
        public ReportView View { get { return DataContext as ReportView; } }

        public MasterDetailGrid MasterDetailGrid { get; set; } = new MasterDetailGrid();

        bool _isInitialized = false;

        public ReportCtrl()
        {
            InitializeComponent();

            DataContextChanged += ReportCtrl_DataContextChanged;
            //DataContext = new ReportView();
            
        }


        private void ReportCtrl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!_isInitialized)
                Init();
        }

        public void Init()
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

            MasterDataGrid.SelectionChanged += MasterDataGrid_SelectionChanged;

            _isInitialized = true;
        }

        private void MasterDataGrid_SelectionChanged(object sender, Syncfusion.UI.Xaml.Grid.GridSelectionChangedEventArgs e)
        {
            if (View == null) { return; }

            DevZest.Windows.DataVirtualization.VirtualListItem<MasterDetailView.EntityView> SelectedItem = (DevZest.Windows.DataVirtualization.VirtualListItem<MasterDetailView.EntityView>)MasterDataGrid.SelectedItem;

            if (SelectedItem != null)
            {
                View.SelectedItemGuid = SelectedItem.Data.Entity.EntityId.ToString();
                //View.AddEntityFromViewInteraction(SelectedItem);
            }
            else
            {
                View.SelectedItemGuid = null;
            }
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

        private void GridSplitter_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            MasterDetailGrid.GridSplitter_DragCompleted(sender, e);
        }

        private void StarWizard_Click(object sender, RoutedEventArgs e)
        {
            EntitiesHelper entsHelper = new EntitiesHelper(View.ItemsView.DataService);
            Entity entReport = View.ItemsView.DataService.GetEntityById(BuiltInCodes.EntityType.Report, View.ItemsView.SelectedEntityId);

            if (entReport != null)
            {
                View.OpenWizardOfSelectedReport(entReport.EntityId);
            }
            else
            {
                MessageBox.Show(LocalizationProvider.GetString("SelezionareUnReportPrimaDiProseugireAltrimentiCreareUnoNuovo"));
            }
            
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
