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

namespace MasterDetailWpf
{
    /// <summary>
    /// Interaction logic for CodiceAttributoWindow.xaml
    /// </summary>
    public partial class CodiceAttributoWindow : Window
    {
        public string CodiceAttributo { get; set; } = string.Empty;

        public CodiceAttributoWindow()
        {
            InitializeComponent();

            Loaded += CodiceAttributoWindow_Loaded;
        }

        private void CodiceAttributoWindow_Loaded(object sender, RoutedEventArgs e)
        {
            CodiceEditBox.Text = CodiceAttributo;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            CodiceAttributo = CodiceEditBox.Text;
            DialogResult = true;
        }
    }
}
