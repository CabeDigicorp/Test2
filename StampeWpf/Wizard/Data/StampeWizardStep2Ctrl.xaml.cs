using Commons;
using Commons.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
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
    /// Interaction logic for WizardStep2Ctrl.xaml
    /// </summary>
    public partial class StampeWizardStep2Ctrl : UserControl
    {
        private ReportWizardSettingDataView view { get { return DataContext as ReportWizardSettingDataView; } }

        public StampeWizardStep2Ctrl()
        {
            InitializeComponent();
        }
        private void PreviewButton_Click(object sender, RoutedEventArgs e)
        {
            view.PreviewButton();
        }

    }
}
