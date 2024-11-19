using Syncfusion.UI.Xaml.Grid;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Commons
{

    public class SfDataGridExt : SfDataGrid
    {

        public static void SetSelectedItems(DependencyObject element, object value)
        {
            if (element is SfDataGrid)
                element.SetValue(SelectedItemsProperty, value);
            else
                throw new NotSupportedException("This property can be used only with SfDataGrid");
        }
        public static object GetSelectedItems(DependencyObject element)
        {
            return (object)element.GetValue(SelectedItemsProperty);
        }
        public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.RegisterAttached(
        "SelectedItems", typeof(ObservableCollection<Object>), typeof(SfDataGridExt), new PropertyMetadata(new ObservableCollection<Object>(), OnSelectedItemsChanged));
        private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            var sfDataGrid = d as SfDataGrid;
            if (sfDataGrid == null)
                return;
            //SfDataGridHelper.SelectedItems property updated based on SfDataGrid.SelectedItems Collectionchanged event.
            sfDataGrid.SelectedItems.CollectionChanged += (sender, e) =>
            {
                SfDataGridExt.SetSelectedItems(sfDataGrid, sfDataGrid.SelectedItems);
            };
        }
        public SfDataGridExt()
        {
            Loaded += SfDataGridExt_Loaded;
        }

        private void SfDataGridExt_Loaded(object sender, RoutedEventArgs e)
        {
            if (RowDragDropController != null)
                RowDragDropController.Drop += RowDragDropController_Drop;

            if (RowDragDropController != null)
                RowDragDropController.DragOver += RowDragDropController_DragOver;

        }

        private void RowDragDropController_DragOver(object sender, GridRowDragOverEventArgs e)
        {
            e.ShowDragUI = false;
        }

        /// <summary>
        /// In questo modo risponde la collectionChanged di ObservableCollection<object>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RowDragDropController_Drop(object sender, GridRowDropEventArgs e)
        {
            IList items = ItemsSource as IList;

            if (e.DraggingRecords == null)
                return;

            object obj = e.DraggingRecords[0];
            items.Remove(obj);
            items.Insert((int)e.TargetRecord, obj);
        }




    }
}
