using AttivitaWpf.View;
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

namespace AttivitaWpf
{
    /// <summary>
    /// Interaction logic for PreviewGanttScala.xaml
    /// </summary>
    public partial class PreviewGanttScalaWnd : Window
    {
        private PreviewGanttScalaView view { get { return DataContext as PreviewGanttScalaView; } }
        public PreviewGanttScalaWnd()
        {
            InitializeComponent();
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            view.AcceptButton();
            this.Close();
        }
    }
}
