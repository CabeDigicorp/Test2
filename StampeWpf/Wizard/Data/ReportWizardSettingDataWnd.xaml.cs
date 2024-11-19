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
    /// Interaction logic for ReportWizardSettingData.xaml
    /// </summary>
    public partial class ReportWizardSettingDataWnd : Window
    {
        private ReportWizardSettingDataView view { get { return DataContext as ReportWizardSettingDataView; } }
        public ReportWizardSettingDataWnd()
        {
            InitializeComponent();

            this.Deactivated += ReportWizardSettingDataWnd_Deactivated;
            this.MouseLeftButtonUp += ReportWizardSettingDataWnd_MouseLeftButtonUp;
        }

        private void ReportWizardSettingDataWnd_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (view != null)
            {
                if (view.DocumentoCorpoView != null || view.ItemsRaggruppamenti != null)
                    view.ForceCloseOfPopUps();
            }
        }

        private void ReportWizardSettingDataWnd_Deactivated(object sender, EventArgs e)
        {
            view.ForceCloseOfPopUps();
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            view.AcceptButton();
            this.Close();
        }
    }
}
