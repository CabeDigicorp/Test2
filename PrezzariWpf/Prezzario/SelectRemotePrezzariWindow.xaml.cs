using Model.JoinService;
using PrezzariWpf.View;
using Syncfusion.Data;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Cells;
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

namespace PrezzariWpf.Prezzario
{
    /// <summary>
    /// Interaction logic for SelectRemotePrezzari.xaml
    /// </summary>
    public partial class SelectRemotePrezzariWindow : Window
    {
        public SelectRemotePrezzariView View { get => DataContext as SelectRemotePrezzariView; }

        IEnumerable<string> _filenames = null;
        public IEnumerable<string> FileNamesReturned { get => _filenames; }

        public SelectRemotePrezzariWindow()
        {
            InitializeComponent();

            //Default CaptionSummaryCellRenderer CellRenderer is removed.
            PrezzariGrid.CellRenderers.Remove("CaptionSummary");
            //Customized CaptionSummaryCellRenderer is added.
            PrezzariGrid.CellRenderers.Add("CaptionSummary", new CustomCaptionSummaryCellRenderer());
        }

        public void Init(Dictionary<string, PrezzarioInfo> prezzariInfoDownloaded)
        {
            View.Load(prezzariInfoDownloaded);
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            _filenames = PrezzariGrid.SelectedItems.Select(item => (item as PrezzarioInfoView).FileName);
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        
    }

    /// <summary>
    /// Scopo: rendere grigi i prezzari della griglia che non sono scaricabili (per via che sono di una versione successiva)
    /// </summary>
    public class CustomStyleSelector : StyleSelector
    {
        Style _disabledRowStyle = null;

        public CustomStyleSelector()
        {
            _disabledRowStyle = new Style(typeof(VirtualizingCellsControl));
            _disabledRowStyle.Setters.Add(new Setter(TextBlock.ForegroundProperty, Brushes.Gray));
        }

        public override Style SelectStyle(object item, DependencyObject container)
        {
            var row = item as DataRowBase;
            var rowView = row.RowData as PrezzarioInfoView;

            if (!rowView.IsPrezzarioDownloadable)
                return _disabledRowStyle;

            return base.SelectStyle(item, container);
        }



    }

    /// <summary>
    /// scopo: Togliere Gruppo dalla riga quando raggruppata la griglia per Gruppo
    /// </summary>
    public class CustomCaptionSummaryCellRenderer : GridCaptionSummaryCellRenderer
    {
        /// <summary>
        /// Method to initialize the CaptionSummaryCell.
        /// </summary>
        public override void OnInitializeEditElement(DataColumnBase dataColumn, GridCaptionSummaryCell uiElement, object dataContext)
        {
            if (dataContext is Group)
            {
                var groupRecord = dataContext as Group;
                if (this.DataGrid.CaptionSummaryRow == null)
                {
                    // get the column which is grouped
                    var groupedColumn = this.GetGroupedColumn(groupRecord);
                    //set the captionsummarycell text as customized.
                    uiElement.Content = GetCustomizedCaptionText(groupedColumn.HeaderText, groupRecord.Key,
                        groupRecord.ItemsCount);
                }
                else if (this.DataGrid.CaptionSummaryRow.ShowSummaryInRow)
                {
                    uiElement.Content = SummaryCreator.GetSummaryDisplayTextForRow(groupRecord.SummaryDetails,
                        this.DataGrid.View);
                }
                else
                    uiElement.Content = SummaryCreator.GetSummaryDisplayText(groupRecord.SummaryDetails,
                        dataColumn.GridColumn.MappingName, this.DataGrid.View);
            }
        }

        /// <summary>
        /// Method to update the CaptionSummaryCell.
        /// </summary>
        public override void OnUpdateEditBinding(DataColumnBase dataColumn, GridCaptionSummaryCell element, object dataContext)
        {
            if (element.DataContext is Group && this.DataGrid.View.GroupDescriptions.Count > 0)
            {
                var groupRecord = element.DataContext as Group;
                //get the column which is grouped.
                var groupedColumn = this.GetGroupedColumn(groupRecord);
                if (this.DataGrid.CaptionSummaryRow == null)
                {
                    if (this.DataGrid.View.GroupDescriptions.Count < groupRecord.Level)
                        return;
                    //set the captionsummary text as customized.
                    element.Content = GetCustomizedCaptionText(groupedColumn.HeaderText, groupRecord.Key,
                        groupRecord.ItemsCount);
                }
                else if (this.DataGrid.CaptionSummaryRow.ShowSummaryInRow)
                {
                    element.Content = SummaryCreator.GetSummaryDisplayTextForRow(groupRecord.SummaryDetails,
                        this.DataGrid.View, groupedColumn.HeaderText);
                }
                else
                    element.Content = SummaryCreator.GetSummaryDisplayText(groupRecord.SummaryDetails,
                        dataColumn.GridColumn.MappingName, this.DataGrid.View);
            }
        }

        /// <summary>
        /// Method to get the Column that is grouped.
        /// </summary>
        private GridColumn GetGroupedColumn(Group group)
        {
            var groupDesc = this.DataGrid.View.GroupDescriptions[group.Level - 1] as PropertyGroupDescription;
            foreach (var column in this.DataGrid.Columns)
            {
                if (column.MappingName == groupDesc.PropertyName)
                {
                    return column;
                }
            }

            return null;
        }

        /// <summary>
        /// Method to Customize the CaptionSummaryCell Text.
        /// </summary>
        private string GetCustomizedCaptionText(string columnName, object groupName, int itemsCount)
        {
            return string.Format("{0}", groupName);
        }

    }
}
