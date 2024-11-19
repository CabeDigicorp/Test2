using DevExpress.Mvvm.UI.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FogliDiCalcoloWpf
{
    public class SfDataGridBehavior : Behavior<Syncfusion.UI.Xaml.Grid.SfDataGrid>
    {

        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.GridContextMenuOpening += OnGridContextMenuOpening;
        }

        private void OnGridContextMenuOpening(object sender, Syncfusion.UI.Xaml.Grid.GridContextMenuEventArgs e)
        {
            //here customized based on your scenario
            var dataGrid = sender as Syncfusion.UI.Xaml.Grid.SfDataGrid;
            //here context menu only showed for EmployeeName Column
            if (dataGrid.Columns[e.RowColumnIndex.ColumnIndex].MappingName != "Formula")
                e.Handled = true;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            this.AssociatedObject.GridContextMenuOpening -= OnGridContextMenuOpening;
        }
    }
}
