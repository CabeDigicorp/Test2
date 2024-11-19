using Model.JoinService;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;
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
using System.Windows.Shapes;

namespace MainApp
{
    /// <summary>
    /// Interaction logic for SelectRemoteProjectModelsWnd.xaml
    /// </summary>
    public partial class SelectRemoteProjectModelsWnd : Window
    {
        public SelectRemoteProjectModelsView View { get => DataContext as SelectRemoteProjectModelsView; }

        public SelectRemoteProjectModelsWnd()
        {
            InitializeComponent();
        }

        public void Init(Dictionary<string, ModelloInfo> modelliInfoDownloaded)
        {
            View.Load(modelliInfoDownloaded);
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            
            DialogResult = true;
        }

        private void ModelliList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            View.SelectedModelli = ModelliList.SelectedItems
                .Cast<ModelloInfoView>()
                .ToList();

            AcceptButton.IsEnabled = View.SelectedModelli.Any();
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
    }
}
