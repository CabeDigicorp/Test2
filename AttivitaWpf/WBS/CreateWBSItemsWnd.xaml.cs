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
    /// Interaction logic for CreateWBSItemsWnd.xaml
    /// </summary>
    public partial class CreateWBSItemsWnd : Window
    {
        public CreateWBSItemsView View { get => DataContext as CreateWBSItemsView; }

        public CreateWBSItemsWnd()
        {
            InitializeComponent();
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            View.Accept();
            DialogResult = true;
        }
    }
}
