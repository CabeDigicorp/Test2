using ComputoWpf;
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

namespace MainApp
{
    /// <summary>
    /// Interaction logic for BIMViewerWindow.xaml
    /// </summary>
    public partial class BIMViewerWindow : Window
    {
        public BIMViewerWindow(UserControl userControl)
        {
            InitializeComponent();

            myStack.Children.Add(userControl);
            SizeToContent = SizeToContent.WidthAndHeight;
        }
    }
}
