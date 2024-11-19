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

namespace StampeWpf.Wizard
{
    /// <summary>
    /// Interaction logic for GroupSettingWnd.xaml
    /// </summary>
    public partial class GroupSettingWnd : Window
    {
        public GroupSettingWnd()
        {
            InitializeComponent();
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
