using CommonResources;
using Commons;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;
using Syncfusion.UI.Xaml.ScrollAxis;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace MainApp
{
    /// <summary>
    /// Interaction logic for ProjectModelCtrl.xaml
    /// </summary>
    public partial class ProjectModelCtrl : UserControl
    {
        ProjectModelView View { get => DataContext as ProjectModelView; }


        public ProjectModelCtrl()
        {
            InitializeComponent();
            ModelliList.SelectionChanged += ModelliList_SelectionChanged;

            TagsGrid.SelectionChanged += TagsGrid_SelectionChanged;
            TagsGrid.MouseMove += TagsGrid_MouseMove;
            TagsGrid.MouseLeave += TagsGrid_MouseLeave;
            TagsGrid.RowDragDropController = new GridRowDragDropControllerTags();
        }



        private void ModelliList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            
            View.SelectedModelli = ModelliList.SelectedItems
                .Cast<ClientModelloInfoView>()
                .ToList();
        }

        private void TagsGrid_SelectionChanged(object sender, GridSelectionChangedEventArgs e)
        {
            View.SelectedTags = TagsGrid.SelectedItems
                .Cast<ClientTagView>()
                .ToList();
        }

        private void SelectAllBtn_Click(object sender, RoutedEventArgs e)
        {
            TagsGrid.SelectAll();
            View.SelectedTags = (TagsGrid.ItemsSource as ObservableCollection<ClientTagView>).ToList();
        }

        private void TextBoxDoubleClick_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            View.CurrentTag = tb.DataContext as ClientTagView;
        }



        private void ModelliListItem_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("TagViewFormat"))
            {
                FrameworkElement fwElement = sender as FrameworkElement;
                ClientModelloInfoView modelloInfoView = fwElement.DataContext as ClientModelloInfoView;

                if (modelloInfoView == null)
                    return;    

                string pacchetto = e.Data.GetData("TagViewFormat") as string;

                List<string> tags = pacchetto.Split(ProjectModelView.TagSeparator).ToList();

                modelloInfoView.AddTags(tags);

                e.Handled = true;
            }
        }


        #region Filter ToggleButton Visibility
        private void TagsGrid_MouseMove(object sender, MouseEventArgs e)
        {
            //With the help of VisualContainer ,you can get the row and column index based on the mouse move pointer position
            var visualcontainer = this.TagsGrid.GetVisualContainer();
            // Gets the exact position where the mouse pointer is moved 
            var point = e.GetPosition(visualcontainer);
            //Here you can get the row and column index based on the pointer position by using PointToCellRowColumnIndex() method
            var rowColumnIndex = visualcontainer.PointToCellRowColumnIndex(point);
            var recordIndex = this.TagsGrid.ResolveToRecordIndex(rowColumnIndex.RowIndex);

            ResetFilterVisibility();

            if (recordIndex < 0)
                return;

            View.TagsView[recordIndex].IsFilterVisible = true;

            
        }

        private void TagsGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            ResetFilterVisibility();
        }

        private void ResetFilterVisibility()
        {
            

            foreach (var tagView in View.TagsView)
            {
                if (!tagView.IsFiltered)
                    tagView.IsFilterVisible = false;
            }
        }
        #endregion

        private void DownloadModelsBtn_Click(object sender, RoutedEventArgs e)
        {

            SelectRemoteProjectModelsWnd wnd = new SelectRemoteProjectModelsWnd();
            wnd.SourceInitialized += (x, y) => wnd.HideMinimizeAndMaximizeButtons();
            wnd.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            wnd.View.MainOperation = View.MainOperation;
            wnd.View.WindowService = View.WindowService;


            wnd.Init(View.ModelliInfoDownloaded);

            if (wnd.ShowDialog() == true)
            {
                List<string> modelsFileName = wnd.View.GetSelectedModelsFileName();
                View.DownloadModelli(modelsFileName);
            }



        }
    }

    public class GridRowDragDropControllerTags : GridRowDragDropController
    {
        ObservableCollection<object> _draggingRecords = new ObservableCollection<object>();


        /// <summary>
        /// Occurs when the input system reports an underlying dragover event with this element as the potential drop target.
        /// </summary>
        /// <param name="args">An <see cref="T:Windows.UI.Xaml.DragEventArgs">DragEventArgs</see> that contains the event data.</param>
        /// <param name="rowColumnIndex">Specifies the row column index based on the mouse point.</param>
        protected override void ProcessOnDragOver(DragEventArgs args, RowColumnIndex rowColumnIndex)
        {
            _draggingRecords = args.Data.GetData("Records") as ObservableCollection<object>;

            if (_draggingRecords == null)
                return;


            string pacchetto = string.Join(ProjectModelView.TagSeparator.ToString(), _draggingRecords.Select(item => (item as ClientTagView).Name));

            //DataObject data = new DataObject("TagViewFormat", pacchetto);
            args.Data.SetData("TagViewFormat", pacchetto);

            //DragDropEffects de = DragDrop.DoDragDrop(this.DataGrid, data, DragDropEffects.Link);

            //To get the dropping position of the record
            var dropPosition = GetDropPosition(args, rowColumnIndex, _draggingRecords);

            args.Handled = true;
        }





    }
}
