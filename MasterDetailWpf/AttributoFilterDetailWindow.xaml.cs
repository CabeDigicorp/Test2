using MasterDetailModel;
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

namespace MasterDetailWpf
{
    /// <summary>
    /// Interaction logic for AttributoFilterWindow.xaml
    /// </summary>
    public partial class AttributoFilterDetailWindow : Window
    {
        public AttributoFilterDetailView View { get => DataContext as AttributoFilterDetailView; }

        public AttributoFilterDetailWindow(EntitiesListMasterDetailView master, AttributoFilterData data)
        {
            DataContext = new AttributoFilterDetailView(master, data);
            InitializeComponent();
            View.ValoreConditionsGroupView = RootConditionsCtrl.DataContext as ValoreConditionsGroupView;
            View.Load();

            View.TextSearchedEnter += View_TextSearchedEnter;
        }

        private void Find_Click(object sender, RoutedEventArgs e)
        {
            //View.Find();
            View.Accept();
            DialogResult = true;
        }

        private void View_TextSearchedEnter(object sender, EventArgs e)
        {
            DialogResult = true;
        }


        //private void Filter_Click(object sender, RoutedEventArgs e)
        //{
        //    View.Filter();
        //    DialogResult = true;
        //}
    }
}
