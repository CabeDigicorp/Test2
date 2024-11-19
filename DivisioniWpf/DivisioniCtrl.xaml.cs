using Commons;
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

namespace DivisioniWpf
{
    /// <summary>
    /// Interaction logic for DivisioniCtrl.xaml
    /// </summary>
    public partial class DivisioniCtrl : UserControl
    {

        public DivisioniView View { get => DataContext as DivisioniView; }

        public DivisioniCtrl()
        {
            InitializeComponent();
            View.CurrentDivisioneView = currentDivisioneCtrl.DivisioneView;

        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                (sender as TextBox).GetBindingExpression(TextBox.TextProperty).UpdateSource();
        }


        private void TextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            textBox.IsReadOnly = false;
            textBox.SelectAll();
            textBox.Background = Brushes.White;
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            DivisioniNameCtrl.SelectedItem = textBox.DataContext;
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            textBox.IsReadOnly = true;
            textBox.Background = Brushes.Transparent;
        }

    }
}
