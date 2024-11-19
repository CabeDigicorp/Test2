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

namespace ComputoWpf
{
    /// <summary>
    /// Interaction logic for ApplyComputoRulesWindow.xaml
    /// </summary>
    public partial class ApplyComputoRulesWindow : Window
    {
        public ApplyComputoRulesView View { get => DataContext as ApplyComputoRulesView; }

        public ApplyComputoRulesWindow()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
