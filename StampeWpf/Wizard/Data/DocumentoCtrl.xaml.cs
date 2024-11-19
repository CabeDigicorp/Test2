using StampeWpf.Wizard;
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
    /// Interaction logic for DocumentoCtrl.xaml
    /// </summary>
    public partial class DocumentoCtrl : UserControl
    {
        private DocumentoView view { get { return DataContext as DocumentoView; } }
        public DocumentoCtrl()
        {
            InitializeComponent();
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (view != null)
            {
                if (e.HorizontalChange != 0)
                    view.ForceCloseOfPopUpsMethod();
            }
        }

        private void Grid_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            e.Handled = true;
        }
    }
}
