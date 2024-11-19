using Commons;
using MasterDetailView;
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

namespace MainApp.Windows
{
    /// <summary>
    /// Interaction logic for SelectNumericFormatsWnd.xaml
    /// </summary>
    public partial class SelectNumericFormatsWnd : Window
    {
        public NumericFormatView FormatNumeroView { get => NumberFormatCtrl.DataContext as NumericFormatView; }

        public SelectNumericFormatsWnd()
        {
            InitializeComponent();
            FormatNumeroView.SelectionChanged += FormatNumeroView_SelectionChanged;


        }

        private void FormatNumeroView_SelectionChanged(object sender, EventArgs e)
        {
            AcceptButton.IsEnabled = FormatNumeroView.SelectedNumberFormats.Any();
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
