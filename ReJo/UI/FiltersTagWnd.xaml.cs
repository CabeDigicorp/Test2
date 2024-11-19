using Autodesk.Revit.DB;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;
using Syncfusion.UI.Xaml.ScrollAxis;
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

namespace ReJo.UI
{
    /// <summary>
    /// Interaction logic for FiltersTagWnd.xaml
    /// </summary>
    public partial class FiltersTagWnd : Window
    {
        public FiltersTagView View { get => DataContext as FiltersTagView; }
        public FiltersTagWnd()
        {
            InitializeComponent();

            Loaded += FiltersTagWnd_Loaded;
        }

        private void FiltersTagWnd_Loaded(object sender, RoutedEventArgs e)
        {
            View.Load();

            if (FilterList.Items.Count > 0)
                FilterList.SelectedIndex = 0;

            FilterList.Focus();
        }

        private void FilterList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            View.SelectedFilterIds.Clear();

            if (FilterList.SelectedItems.Count == 0)
                View.UpdateFiltersTagCheck();
            else
            {
                foreach (TagFilterItemView filter in FilterList.SelectedItems)
                {
                    View.SelectedFilterIds.Add(filter.UniqueId);
                    View.UpdateFiltersTagCheck();
                }
            }
        }

        private void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            View.OnOk();
            DialogResult = true;
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void AddTag_Click(object sender, RoutedEventArgs e)
        {
            View.AddTag();

            var selectedItem = View.CurrentTagItem;
            var rowindex = TagsDataGrid.ResolveToRowIndex(selectedItem);
            var columnindex = TagsDataGrid.ResolveToStartColumnIndex();
            //Make the row in to available on the view. 
            TagsDataGrid.ScrollInView(new RowColumnIndex(rowindex, columnindex));
            //to set the found row as current row 
            TagsDataGrid.View.MoveCurrentTo(selectedItem);



        }
    }
}
