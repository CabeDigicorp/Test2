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
    /// Interaction logic for FilterByEntityIdsWnd.xaml
    /// </summary>
    public partial class FilterByEntityIdsWnd : Window
    {
        public FilterByEntityIdsView View { get => DataContext as FilterByEntityIdsView; }

        public FilterByEntityIdsWnd(EntitiesListMasterDetailView master, AttributoFilterData data)
        {
            DataContext = new FilterByEntityIdsView(master, data);
            InitializeComponent();
            //View.Init();

            //View.Refresh += View_Refresh;
        }

        //private void View_Refresh(object sender, EventArgs e)
        //{
        //}

        private void Find_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
