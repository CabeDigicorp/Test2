using Commons.View;
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

namespace Commons
{
    /// <summary>
    /// Interaction logic for StampeListView.xaml
    /// </summary>
    public partial class StampeListCtrl : UserControl
    {
        public StampeListView StampeListView { get => DataContext as StampeListView; }

        private Point startPoint = new Point();
        private int startIndex = -1;
        Dictionary<int, WorkItem> ItemToDragAndDrop = new Dictionary<int, WorkItem>();
        public StampeListCtrl()
        {
            InitializeComponent();
        }

        public void StampeListView_CollectionChanged(object sender, EventArgs e)
        {
            foreach (var column in GridViewColumn.Columns)
            {
                // If this is an "auto width" column...
                if (double.IsNaN(column.Width))
                {
                    // Set its Width back to NaN to auto-size again
                    column.Width = 0;
                    column.Width = double.NaN;
                }
            }
        }
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            WorkItem ItemToRemove = this.LstViewColumn.SelectedItem as WorkItem;
            StampeListView.ItemColumn.Remove(ItemToRemove);
        }

        private static T FindAnchestor<T>(DependencyObject current)
         where T : DependencyObject
        {
            do
            {
                if (current is T)
                {
                    return (T)current;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            while (current != null);
            return null;
        }

        private void LstViewColumn_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Get current mouse position
            startPoint = e.GetPosition(null);
        }

        private void LstViewColumn_DragEnter(object sender, DragEventArgs e)
        {
            var ListView = ((ListView)sender);
            if (ListView != null)
            {
                if (ListView.SelectedItems.Count != 0)
                {
                    try
                    {
                        int result;
                        Int32.TryParse(ListView.Name.LastOrDefault().ToString(), out result);
                        StampeListView.RaiseDragEvent((WorkItem)ListView.SelectedItems[0], "Drag");
                    }
                    catch (ArgumentException ex)
                    {
                    }
                }
            }

            if (!e.Data.GetDataPresent("WorkItem") || sender != e.Source)
            {

                e.Effects = DragDropEffects.None;
            }
        }

        private void LstViewColumn_MouseMove(object sender, MouseEventArgs e)
        {
            // Get the current mouse position
            Point mousePos = e.GetPosition(null);
            Vector diff = startPoint - mousePos;

            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                       Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                // Get the dragged ListViewItem
                ListView listView = sender as ListView;
                ListViewItem listViewItem = FindAnchestor<ListViewItem>((DependencyObject)e.OriginalSource);
                if (listViewItem == null) return;           // Abort
                                                            // Find the data behind the ListViewItem
                WorkItem item = (WorkItem)listView.ItemContainerGenerator.ItemFromContainer(listViewItem);
                if (item == null) return;                   // Abort
                                                            // Initialize the drag & drop operation
                startIndex = listView.SelectedIndex;
                DataObject dragData = new DataObject("WorkItem", item);
                DragDrop.DoDragDrop(listViewItem, dragData, DragDropEffects.Copy | DragDropEffects.Move);
            }
        }

        private void LstViewColumn_Drop(object sender, DragEventArgs e)
        {
            int index = -1;

            if (e.Data.GetDataPresent("WorkItem") && sender == e.Source)
            {
                // Get the drop ListViewItem destination
                ListView listView = sender as ListView;
                ListViewItem listViewItem = FindAnchestor<ListViewItem>((DependencyObject)e.OriginalSource);
                StampeListView.RaiseDropEvent();
                if (listViewItem == null)
                {
                    // Abort
                    e.Effects = DragDropEffects.None;
                    return;
                }
                // Find the data behind the ListViewItem
                WorkItem item = (WorkItem)listView.ItemContainerGenerator.ItemFromContainer(listViewItem);
                // Move item into observable collection 
                // (this will be automatically reflected to lstView.ItemsSource)
                e.Effects = DragDropEffects.Move;
                index = StampeListView.ItemColumn.IndexOf(item);
                
                if (startIndex >= 0 && index >= 0)
                {
                    StampeListView.ItemColumn.Move(startIndex, index);
                }
                startIndex = -1;        // Done!
            }
        }
    }
}