using Commons;
using StampeWpf.View;
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

namespace StampeWpf
{
    /// <summary>
    /// Interaction logic for StampeCtrl.xaml
    /// </summary>
    public partial class StampeCtrl : UserControl
    {
        public StampeView View { get => DataContext as StampeView; }

        public StampeCtrl()
        {
            InitializeComponent();

            View.ViewUpdated += View_ViewUpdated;
            View.DocumentiView = DocumentiCtrl.DataContext as DocumentiView;
            View.ReportView = ReportCtrl.DataContext as ReportView;
        }

        /// <summary>
        /// N.B. L'aggiornamento di SelectedItem di HierarchyNavigator non si riesce a fare via binding
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void View_ViewUpdated(object sender, EventArgs e)
        {
            StampeHierarchicalItemsSource itemsSource = StampeNavigator.ItemsSource as StampeHierarchicalItemsSource;

            SectionItemView currSectionItem = View.CurrentSectionItem as SectionItemView;
            SectionItemView hierarchyItem = itemsSource[0].SectionItems.FirstOrDefault(item => item.Id == currSectionItem.Id);

            if (StampeNavigator.SelectedItem != hierarchyItem)
                StampeNavigator.SelectedItem = hierarchyItem;
        }

        private void SectionButton_Click(object sender, RoutedEventArgs e)
        {
            View.CurrentSectionItem = (sender as FrameworkElement).DataContext;
        }
    }
}
