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
    /// Interaction logic for PreviewGanttSetting.xaml
    /// </summary>
    public partial class PreviewGanttSetting : Window
    {
        private PreviewGanttSettingView view { get { return DataContext as PreviewGanttSettingView; } }
        public PreviewGanttSetting()
        {
            InitializeComponent();
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            view.AcceptButton();
            DialogResult = true;
        }
    }
}
