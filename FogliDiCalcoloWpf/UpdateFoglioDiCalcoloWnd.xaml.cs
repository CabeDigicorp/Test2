using FogliDiCalcoloWpf.View;
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

namespace FogliDiCalcoloWpf
{
    /// <summary>
    /// Interaction logic for UpdateFoglioDiCalcoloWnd.xaml
    /// </summary>
    public partial class UpdateFoglioDiCalcoloWnd : Window
    {
        public UpdateFoglioDiCalcoloView View { get => DataContext as UpdateFoglioDiCalcoloView; }
        public UpdateFoglioDiCalcoloWnd()
        {
            InitializeComponent();
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            if (View.Accept())
                DialogResult = true;
        }
    }
}
