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
    /// Interaction logic for EntityHighlightersWnd.xaml
    /// </summary>
    public partial class EntityHighlightersWnd : Window
    {
        EntityHighlightersView View { get => DataContext as EntityHighlightersView; }
        public EntityHighlightersWnd()
        {
            InitializeComponent();
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            View.Accept();
            DialogResult = true;
        }
    }
}
