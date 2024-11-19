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
    /// Interaction logic for GanttChartStyleSetting.xaml
    /// </summary>
    public partial class GanttChartStyleSettingWnd : Window
    {
        private GanttChartStyleSettingView View { get { return DataContext as GanttChartStyleSettingView; } }
        public GanttChartStyleSettingWnd()
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
