using AttivitaWpf.View;
using CommonResources;
using Syncfusion.UI.Xaml.Grid;
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

namespace AttivitaWpf
{
    /// <summary>
    /// Interaction logic for ProgrammazioneSALWnd.xaml
    /// </summary>
    public partial class ProgrammazioneSALWnd : Window
    {
        private ProgrammazioneSALView view { get { return DataContext as ProgrammazioneSALView; } }

        public ProgrammazioneSALWnd()
        {
            InitializeComponent();
            TextBoxFilter.LostFocus += TextBoxFilter_LostFocus;
            TextBoxFilter.GotFocus += TextBoxFilter_GotFocus;
            TextBoxFilter.Foreground = Brushes.DarkGray;
            AddDefaultColumn();
            ProgrammazioneSALGrid.QueryRowHeight += ProgrammazioneSALGrid_QueryRowHeight;
            this.DataContextChanged += ProgrammazioneSALWnd_DataContextChanged;
            this.Closing += ProgrammazioneSALWnd_Closing;
        }

        private void ProgrammazioneSALGrid_QueryRowHeight(object sender, QueryRowHeightEventArgs e)
        {
            double height = 0.0;
            if (ProgrammazioneSALGrid.GetHeaderIndex() == e.RowIndex)
            {
                bool isAutoRowHeight = ProgrammazioneSALGrid.GridColumnSizer.GetAutoRowHeight(e.RowIndex, new GridRowSizingOptions(), out height);
                if (height > ProgrammazioneSALGrid.RowHeight)
                {
                    e.Height = height;
                    e.Handled = true;
                }
            }
        }

        private void ProgrammazioneSALWnd_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            view.UpdateColumn -= View_UpdateColumn;
        }

        private void ProgrammazioneSALWnd_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            view.UpdateColumn += View_UpdateColumn;
        }
        private void View_UpdateColumn(object sender, EventArgs e)
        {
            ProgrammazioneSALGrid.Columns.Clear();
            AddDefaultColumn();
            //AddCustomColumns(view.GetListaColonne());
            AddCustomColumns(view.GetListaColonneB());
        }

        private void AddDefaultColumn()
        {

            ProgrammazioneSALGrid.Columns.Add(new GridNumericColumn() { HeaderText = "", MappingName = "ContatoreSAL", ColumnSizer = GridLengthUnitType.Auto, NumberDecimalDigits = 0 }); 
            if (view != null)
                if ((bool)view?.GetCreateDataColumn())
                    ProgrammazioneSALGrid.Columns.Add(new GridDateTimeColumn() { HeaderText = LocalizationProvider.GetString("Data"), MappingName = "Data", CustomPattern = "dd/MM/yyyy HH:mm", Pattern = Syncfusion.Windows.Shared.DateTimePattern.CustomPattern, CellStyle = (Style)Application.Current.Resources["ProgSALDataGridCellStyle"], ColumnSizer = GridLengthUnitType.Auto });
            ProgrammazioneSALGrid.Columns.Add(new GridCheckBoxColumn() { HeaderText = LocalizationProvider.GetString("SAL"), MappingName = "IsSAL", AllowEditing = true, ColumnSizer = GridLengthUnitType.Auto });
            ProgrammazioneSALGrid.Columns.Add(new GridNumericColumn() { HeaderText = LocalizationProvider.GetString("GiorniPeriodo"), MappingName = "GiorniPeriodo", ColumnSizer = GridLengthUnitType.Auto, NumberDecimalDigits = 0 });
            ProgrammazioneSALGrid.Columns.Add(new GridNumericColumn() { HeaderText = LocalizationProvider.GetString("GiorniProg"), MappingName = "GiorniProgressivo", ColumnSizer = GridLengthUnitType.Auto, NumberDecimalDigits = 0 });
        }
        //private void AddCustomColumns(List<string> columnnames)
        private void AddCustomColumns(List<ColumnVisible> columnVisibles)
        {
            //int contatore = 1;

            //foreach (var columnname in columnnames)
            //{
            //    ProgrammazioneSALGrid.Columns.Add(new GridNumericColumn() { HeaderText = columnname, MappingName = GanttKeys.ColonnaAttributo + contatore, AllowEditing = false, IsReadOnly = true });
            //    contatore++;
            //}

            for (int i = 0; i < SALProgrammatoView.GetTotalColumnForCycle(); i++)
            {
                if (i < columnVisibles.Count())
                {
                    ProgrammazioneSALGrid.Columns.Add(new GridNumericColumn() { HeaderText = columnVisibles.ElementAt(i).Name, MappingName = GanttKeys.ColonnaAttributo + (i + 1), AllowEditing = false, IsReadOnly = true, IsHidden = !columnVisibles.ElementAt(i).IsVisible, ColumnSizer = GridLengthUnitType.SizeToHeader });
                }
                //, IsHidden = !columnVisibles.ElementAt(i - 1).IsVisible
            }
        }

        private void TextBoxFilter_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TextBoxFilter.Text == LocalizationProvider.GetString("Filtra"))
            {
                view.TextSearched = "";
                TextBoxFilter.Foreground = Brushes.Black;
            }
        }

        private void TextBoxFilter_LostFocus(object sender, RoutedEventArgs e)
        {
            if (TextBoxFilter.Text == "")
            {
                view.TextSearched = LocalizationProvider.GetString("Filtra");
                TextBoxFilter.Foreground = Brushes.DarkGray;
            }
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            view.AcceptButton();
            DialogResult = true;
        }

        private void ProgrammazioneSALGrid_CurrentCellValueChanged(object sender, CurrentCellValueChangedEventArgs e)
        {
            if (e.RowColumnIndex.ColumnIndex == 2)
               view.RicalculateSALCounter();
        }

        private void ProgrammazioneSALGrid_SelectionChanged(object sender, GridSelectionChangedEventArgs e)
        {
            //Syncfusion.UI.Xaml.Grid.GridRowInfo p = (Syncfusion.UI.Xaml.Grid.GridRowInfo)e.AddedItems.FirstOrDefault();
            //if (p != null)
            //    view.AddRemoveSALObject(true, p.RowData as SALProgrammatoView);
            //p = (Syncfusion.UI.Xaml.Grid.GridRowInfo)e.RemovedItems.FirstOrDefault();
            //if (p != null)
            //    view.AddRemoveSALObject(false, p.RowData as SALProgrammatoView);

        }

        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            ProgrammazioneSALGrid.SelectedItems.Clear();
            foreach (var item in view.GetAllItem())
            {
                ProgrammazioneSALGrid.SelectedItems.Add(item);
            }
        }
    }
}
