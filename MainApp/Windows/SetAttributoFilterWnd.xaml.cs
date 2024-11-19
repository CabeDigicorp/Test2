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

namespace MainApp
{
    /// <summary>
    /// Interaction logic for SelectAttributoFilterWnd.xaml
    /// </summary>
    public partial class SetAttributoFilterWnd : Window
    {
        public SetAttributoFilterView View { get => DataContext as SetAttributoFilterView; }

        public SetAttributoFilterWnd()
        {
            InitializeComponent();
            View.ValoreConditionsGroupView = RootConditionsCtrl.DataContext as ValoreConditionsGroupView;
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            if (View.Accept())
                DialogResult = true;
        }


    }
}
