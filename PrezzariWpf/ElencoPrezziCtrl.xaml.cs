using Commons;
using PrezzariWpf.View;
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

namespace PrezzariWpf
{
    /// <summary>
    /// Interaction logic for ElencoPrezziCtrl.xaml
    /// </summary>
    public partial class ElencoPrezziCtrl : UserControl
    {
        public ElencoPrezziView View { get => DataContext as ElencoPrezziView; }

        public ElencoPrezziCtrl()
        {
            InitializeComponent();

            View.ViewUpdated += View_ViewUpdated;
            View.PrezzarioView = PrezzarioCtrl.DataContext as PrezzarioView;
            View.CapitoliView = CapitoliCtrl.DataContext as CapitoliView;

        }

        /// <summary>
        /// N.B. L'aggiornamento di SelectedItem di HierarchyNavigator non si riesce a fare via binding
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void View_ViewUpdated(object sender, EventArgs e)
        {
            ElencoPrezziHierarchicalItemsSource itemsSource = ElencoPrezziNavigator.ItemsSource as ElencoPrezziHierarchicalItemsSource;

            SectionItemView currSectionItem = View.CurrentSectionItem as SectionItemView;
            SectionItemView hierarchyItem = itemsSource[0].SectionItems.FirstOrDefault(item => item.Id == currSectionItem.Id);

            if (ElencoPrezziNavigator.SelectedItem != hierarchyItem)
                ElencoPrezziNavigator.SelectedItem = hierarchyItem;
        }

        private void SectionButton_Click(object sender, RoutedEventArgs e)
        {
            View.CurrentSectionItem = (sender as FrameworkElement).DataContext;
        }
    }
}
