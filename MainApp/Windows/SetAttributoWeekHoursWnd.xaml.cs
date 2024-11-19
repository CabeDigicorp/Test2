using MasterDetailView;
using Syncfusion.Data;
using Syncfusion.UI.Xaml.Grid;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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
    /// Interaction logic for SetAttributoWeekHoursWnd.xaml
    /// </summary>
    public partial class SetAttributoWeekHoursWnd : Window
    {
        public SetAttributoWeekHoursView View { get => DataContext as SetAttributoWeekHoursView; }

        public SetAttributoWeekHoursWnd()
        {
            InitializeComponent();
            WeekHoursGrid.PasteGridCellContent += dataGrid_PasteGridCellContent;
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            if (View.Accept())
                DialogResult = true;
        }
        void dataGrid_PasteGridCellContent(object sender, GridCopyPasteCellEventArgs e)
        {
            if (e.Column.MappingName == "Giorno" || e.Column.MappingName == "Ore")
                e.Handled = true;
        }
    }
}

