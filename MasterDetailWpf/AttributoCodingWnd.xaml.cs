using MasterDetailModel;
using MasterDetailView;
using Model;
using Syncfusion.Data;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.TreeGrid;
using System;
using System.Collections.Generic;
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

namespace MasterDetailWpf
{
    /// <summary>
    /// Interaction logic for AttributoCodingWnd.xaml
    /// </summary>
    public partial class AttributoCodingWnd : Window
    {
        public AttributoCodingView View { get => DataContext as AttributoCodingView; }

        public AttributoCodingWnd()
        {
            InitializeComponent();
            RicodificaCodiciGrid.CurrentCellBeginEdit += RicodificaCodiciGrid_CurrentCellBeginEdit;
            RicodificaCodiciGrid.CurrentCellValueChanged += RicodificaCodiciGrid_CurrentCellValueChanged;
        }

        private void RicodificaCodiciGrid_CurrentCellValueChanged(object sender, CurrentCellValueChangedEventArgs e)
        {
            int columnindex = RicodificaCodiciGrid.ResolveToGridVisibleColumnIndex(e.RowColumnIndex.ColumnIndex);
            var column = RicodificaCodiciGrid.Columns[columnindex];
            if (column is GridCheckBoxColumn)
            {
                var rowIndex = this.RicodificaCodiciGrid.ResolveToRecordIndex(e.RowColumnIndex.RowIndex);

                RecordEntry record = null;
                if (this.RicodificaCodiciGrid.GroupColumnDescriptions.Count == 0)
                {
                    record = this.RicodificaCodiciGrid.View.Records[rowIndex] as RecordEntry;
                    if (!AttributoCodingView.SelectedLevels.Contains(rowIndex))
                    {
                        if (columnindex == 6)
                        {
                            ((AttributoCodingSetting)record.Data).AggiungiCodiceSuperiore = false;
                        }
                        else
                        {
                            if (AttributoCodingView.SelectedAttributeCodice == BuiltInCodes.Attributo.Codice)
                                ((AttributoCodingSetting)record.Data).Codifica = false;
                        }
                    }
                    if (columnindex == 6 && rowIndex == 0)
                        ((AttributoCodingSetting)record.Data).AggiungiCodiceSuperiore = false;

                }
                else
                {
                    record = (this.RicodificaCodiciGrid.View.TopLevelGroup.DisplayElements[rowIndex] as RecordEntry);
                }
                ////Checkbox property changed value is stored here.
                //var value = (record.Data as Model).Review;
            }
        }

        private void RicodificaCodiciGrid_CurrentCellBeginEdit(object sender, Syncfusion.UI.Xaml.Grid.CurrentCellBeginEditEventArgs e)
        {
            var recordindex = e.RowColumnIndex.RowIndex;
            if (!AttributoCodingView.SelectedLevels.Contains(recordindex - 1 ))
                e.Cancel = true;

            if (AttributoCodingView.SelectedAttributeCodice != BuiltInCodes.Attributo.Codice)
                e.Cancel = false;
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            if (View.AcceptLocal())
                DialogResult = true;
        }

        private void ComboBoxAttributiSelezionati_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            View.SelectionAttributeChange(e.RemovedItems, e.AddedItems);
        }
    }
    public class ColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //get the GridCell proeprty
            var cell = value as GridCell;

            //get the rowIndex value 
            var rowIndex = (cell.ColumnBase as Syncfusion.UI.Xaml.Grid.DataColumn).RowIndex;

            //Specific Column style achievd by Checking condition as particular mappingName 
            //if (cell.ColumnBase.GridColumn.MappingName == "AggiungiCodiceSuperiore")
            //{
            //    if (rowIndex != 1)
            //        return new SolidColorBrush(Colors.Green);
            //}

            //apply the row style for particualrrow
            if (AttributoCodingView.SelectedLevels.Contains(rowIndex - 1) || AttributoCodingView.SelectedAttributeCodice != BuiltInCodes.Attributo.Codice)
                return DependencyProperty.UnsetValue;
            else
                return new SolidColorBrush(Colors.LightGray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //get the GridCell proeprty
            var cell = value as GridCell;

            if (cell == null)
                return true;

            //get the rowIndex value 
            var rowIndex = (cell.ColumnBase as Syncfusion.UI.Xaml.Grid.DataColumn).RowIndex;

            //Specific Column style achievd by Checking condition as particular mappingName 
            //if (cell.ColumnBase.GridColumn.MappingName == "AggiungiCodiceSuperiore")
            //{
            //    if (rowIndex != 1)
            //        return new SolidColorBrush(Colors.Green);
            //}

            //apply the row style for particualrrow
            //if (cell.ColumnBase.GridColumn.MappingName == "AggiungiCodiceSuperiore")
            //{
            //    if (rowIndex == 1)
            //        return Visibility.Visible;
            //    else
            //        return Visibility.Visible;
            //}

            if (AttributoCodingView.SelectedAttributeCodice != BuiltInCodes.Attributo.Codice)
                return false;
            else
                return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
