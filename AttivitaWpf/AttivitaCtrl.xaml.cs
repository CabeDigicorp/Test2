using AttivitaWpf.View;
using Commons;
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

namespace AttivitaWpf
{
    /// <summary>
    /// Interaction logic for AttivitaCtrl.xaml
    /// </summary>
    public partial class AttivitaCtrl : UserControl
    {
        public AttivitaView View { get => DataContext as AttivitaView; }
        public AttivitaCtrl()
        {
            InitializeComponent();

            View.ViewUpdated += View_ViewUpdated;
            View.ElencoAttivitaView = ElencoAttivitaCtrl.DataContext as ElencoAttivitaView;
            View.GanttView = new GanttView();// GanttCtrl.DataContext as GanttView;
            View.WBSView = WBSCtrl.DataContext as WBSView;
            View.CalendariView = CalendariCtrl.DataContext as CalendariView;
        }

        /// <summary>
        /// N.B. L'aggiornamento di SelectedItem di HierarchyNavigator non si riesce a fare via binding
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void View_ViewUpdated(object sender, EventArgs e)
        {
            AttivitaHierarchicalItemsSource itemsSource = AttivitaNavigator.ItemsSource as AttivitaHierarchicalItemsSource;
            if (!itemsSource.Any())
                return;

            SectionItemView currSectionItem = View.CurrentSectionItem as SectionItemView;
            SectionItemView hierarchyItem = itemsSource[0].SectionItems.FirstOrDefault(item => item.Id == currSectionItem.Id);

            if (AttivitaNavigator.SelectedItem != hierarchyItem)
                AttivitaNavigator.SelectedItem = hierarchyItem;

        }

        private void SectionButton_Click(object sender, RoutedEventArgs e)
        {
            View.CurrentSectionItem = (sender as FrameworkElement).DataContext;
        }
    }
}
