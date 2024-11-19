using Commons;
using ContattiWpf.View;
using DatiGeneraliWpf.View;
using Syncfusion.Windows.Tools.Controls;
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
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace DatiGeneraliWpf
{
    /// <summary>
    /// Interaction logic for DatiGeneralilCtrl.xaml
    /// </summary>
    public partial class DatiGeneraliCtrl : UserControl
    {
        public DatiGeneraliView View { get => DataContext as DatiGeneraliView; }

        public DatiGeneraliCtrl()
        {
            InitializeComponent();
            View.ViewUpdated += View_ViewUpdated;

            View.ContattiView = ContattiCtrl.DataContext as ContattiView;
            View.InfoProgettoView = InfoProgettoCtrl.DataContext as InfoProgettoView;
            View.StiliProgettoView = StiliCtrl.DataContext as StiliProgettoView;
            View.UnitaMisuraView = UnitaMisuraCtrl.DataContext as UnitaMisuraView;
            View.VariabiliView = VariabiliCtrl.DataContext as VariabiliView;
            View.AllegatiView = AllegatiCtrl.DataContext as AllegatiView;
            View.TagView = TagCtrl.DataContext as TagView;

        }

        /// <summary>
        /// N.B. L'aggiornamento di SelectedItem di HierarchyNavigator non si riesce a fare via binding
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void View_ViewUpdated(object sender, EventArgs e)
        {
            DatiGeneraliHierarchicalItemsSource itemsSource = DatiGeneraliNavigator.ItemsSource as DatiGeneraliHierarchicalItemsSource;

            SectionItemView currSectionItem = View.CurrentSectionItem as SectionItemView;
            SectionItemView hierarchyItem = itemsSource[0].SectionItems.FirstOrDefault(item => item.Id == currSectionItem.Id);
            
            if (DatiGeneraliNavigator.SelectedItem != hierarchyItem)
                DatiGeneraliNavigator.SelectedItem = hierarchyItem;

        }


        private void SectionButton_Click(object sender, RoutedEventArgs e)
        {
            View.CurrentSectionItem = (sender as FrameworkElement).DataContext;
        }
    }
}
