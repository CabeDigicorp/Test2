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
using JReport;
using MasterDetailModel;
using CommonResources;

namespace StampeWpf
{
    /// <summary>
    /// Interaction logic for DocumentiCtrl.xaml
    /// </summary>
    public partial class DocumentiCtrl : UserControl
    {
        public DocumentiView View { get { return DataContext as DocumentiView; } }
        private string DefaultId { get { return "00000000-0000-0000-0000-000000000000"; } }
        MasterDetailList MasterDetailList { get; set; } = new MasterDetailList();

        bool _isInitialized = false;

        public event EventHandler EntityViewMouseDoubleClick;
        protected virtual void OnEntityViewMouseDoubleClick(object sender, EventArgs e)
        {
            EntityViewMouseDoubleClick?.Invoke(sender, e);
        }
        public DocumentiCtrl()
        {
            InitializeComponent();

            //Init();
            Loaded += DocumentiCtrl_Loaded;

        }
        private void DocumentiCtrl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized)
                Init();

        }

        public void Init()
        {
            View.ItemsView.RefreshView += ItemsView_RefreshView;
            View.ItemsView.ItemsLoaded += ItemsView_ItemsLoaded;
            DocumentiItemTree.Loaded += ContattiItemList_Loaded;
            DocumentiItemTree.ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged;

            MasterDetailList = new MasterDetailList();
            MasterDetailList.MasterListView = DocumentiItemTree;
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

        private void ContattiItemList_Loaded(object sender, RoutedEventArgs e)
        {
            MasterDetailList.ItemList_Loaded(sender, e);
        }

        private void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
        {
            MasterDetailList.ItemContainerGenerator_StatusChanged(sender, e);
        }

        private void ItemsView_RefreshView(object sender, EventArgs e)
        {
            MasterDetailList.ItemsView_RefreshView(sender, e);
        }

        private void ReportPreview_Click(object sender, RoutedEventArgs e)
        {
            var SelectedItem = View.ItemsView.SelectedTreeEntityView;
            if (SelectedItem != null)
            {
                //return;
                View.ReportPreview(SelectedItem);
            }
            else
            {
                MessageBox.Show(LocalizationProvider.GetString("Selezionare documento prima di proseugire con la preview"));
            }
        }

        private void StarWizard_Click(object sender, RoutedEventArgs e)
        {
            var SelectedItem = View.ItemsView.SelectedTreeEntityView;
            EntitiesHelper entsHelper = new EntitiesHelper(View.ItemsView.DataService);
            Entity DocumentoSelezionato = View.ItemsView.DataService.GetEntityById(BuiltInCodes.EntityType.Documenti, View.ItemsView.SelectedEntityId);
            if (DocumentoSelezionato != null)
            {
                ValoreGuid validReport = (ValoreGuid)entsHelper.GetValoreAttributo(DocumentoSelezionato, BuiltInCodes.Attributo.ReportItem_Guid, true, true);
                if (validReport != null)
                {
                    Entity entReport = View.ItemsView.DataService.GetEntityById(BuiltInCodes.EntityType.Report, validReport.V);

                    if (entReport != null)
                    {
                        View.OpenWizardOfSelectedReport(entReport.EntityId);

                    }
                    else
                    {
                        MessageBox.Show(LocalizationProvider.GetString("Nessun report assegnato per il documento selezionato"));
                    }
                }
            }
            else
            {
                MessageBox.Show(LocalizationProvider.GetString("Seleziona un documento collegato ad un report prima di procedere all'editazione dello stesso"));
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

        private void ListaFiltri_HandleDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MasterDetailList.ListaFiltri_HandleDoubleClick(sender, e);
        }

        private void TreeViewItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MasterDetailList.ListViewItem_MouseLeftButtonDown(sender, e);
        }

    }
}
