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
    /// Interaction logic for SetValoreConditionsWnd.xaml
    /// </summary>
    public partial class SetValoreConditionsWnd : Window
    {
        public SetValoreConditionsWnd()
        {
            InitializeComponent();
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            ValoreConditionsGroupCtrl.View.UpdateData();
            DialogResult = true;
        }
    }
}
