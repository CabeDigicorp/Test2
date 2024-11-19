using CommonResources;
using DevExpress.Mvvm.UI.Interactivity;
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

namespace FogliDiCalcoloWpf
{
    /// <summary>
    /// Interaction logic for FoglioDiCalcoloDataWnd.xaml
    /// </summary>
    public partial class FoglioDiCalcoloDataWnd : Window
    {
        public FoglioDiCalcoloDataView View { get => DataContext as FoglioDiCalcoloDataView; }
        public FoglioDiCalcoloDataWnd()
        {
            InitializeComponent();
            TextBoxFilter.LostFocus += TextBoxFilter_LostFocus;
            TextBoxFilter.GotFocus += TextBoxFilter_GotFocus;
            TextBoxFilter.Foreground = Brushes.DarkGray;
            FormulasGrid.QueryRowHeight += FormulasGrid_QueryRowHeight;
            gridRowResizingOptions.ExcludeColumns.Add("Etichetta");
            gridRowResizingOptions.ExcludeColumns.Add("Note");
        }
        GridRowSizingOptions gridRowResizingOptions = new GridRowSizingOptions();

        double autoHeight = double.NaN;
        void FormulasGrid_QueryRowHeight(object sender, QueryRowHeightEventArgs e)
        {
            if (FormulasGrid.GridColumnSizer.GetAutoRowHeight(e.RowIndex, gridRowResizingOptions, out autoHeight))
            {
                autoHeight += 10;
                if (autoHeight > 24)
                {
                    e.Height = autoHeight;
                    e.Handled = true;
                }
            }
        }

        private void TextBoxFilter_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TextBoxFilter.Text == LocalizationProvider.GetString("Filtra"))
            {
                View.TextSearched = "";
                TextBoxFilter.Foreground = Brushes.Black;
            }
        }

        private void TextBoxFilter_LostFocus(object sender, RoutedEventArgs e)
        {
            if (TextBoxFilter.Text == "")
            {
                View.TextSearched = LocalizationProvider.GetString("Filtra");
                TextBoxFilter.Foreground = Brushes.DarkGray;
            }
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            if (View.Accept())
                DialogResult = true;
        }

        public string AttributeCopied { get; set; }
        private void CopyAttribute_Click(object sender, RoutedEventArgs e)
        {
            View.GetCopiedText = "[" + View.FiltratoSelezionato.Etichetta + "]";
            //AttributeCopied = "[" + View.FiltratoSelezionato.Etichetta + "]";
        }
        //private void CheckBoxSelectAll_Checked(object sender, RoutedEventArgs e)
        //{
        //    if (!LockActionOnAllCheckbox)
        //    {
        //        LockActionOnCheckbox = true;
        //        View.SetCheckForEachAttribute(true);
        //        LockActionOnCheckbox = false;
        //    }
        //}

        //private void CheckBoxSelectAll_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    if (!LockActionOnAllCheckbox)
        //    {
        //        LockActionOnCheckbox = true;
        //        View.SetCheckForEachAttribute(false);
        //        LockActionOnCheckbox = false;
        //    }
        //}

        //private bool _LockActionOnAllCheckbox;
        //public bool LockActionOnAllCheckbox
        //{
        //    get
        //    {

        //        return _LockActionOnAllCheckbox;
        //    }
        //    set
        //    {
        //        _LockActionOnAllCheckbox= value;
        //    }
        //}
        //private void CheckBoxSelectAll_Indeterminate(object sender, RoutedEventArgs e)
        //{
        //    if (!LockActionOnAllCheckbox)
        //        View.IsAllChecked = false;
        //}


        //private bool _LockActionOnCheckbox;
        //public bool LockActionOnCheckbox
        //{
        //    get
        //    {

        //        return _LockActionOnCheckbox;
        //    }
        //    set
        //    {
        //        _LockActionOnCheckbox = value;
        //    }
        //}
        //private void CheckBox_Checked(object sender, RoutedEventArgs e)
        //{
        //    if (View.IsIndeterminateState() && !LockActionOnCheckbox)
        //    {
        //        LockActionOnAllCheckbox = true;
        //        View.IsAllChecked = null;
        //        LockActionOnAllCheckbox = false;
        //    }
        //}

        //private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    if (View.IsIndeterminateState() && !LockActionOnCheckbox)
        //    {
        //        LockActionOnAllCheckbox = true;
        //        View.IsAllChecked = null;
        //        LockActionOnAllCheckbox = false;
        //    }
        //}
    }
}
