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

namespace StampeWpf
{
    /// <summary>
    /// Interaction logic for ReportWizardSettingFogliDiCalcoloWnd.xaml
    /// </summary>
    public partial class ReportWizardSettingFogliDiCalcoloWnd : Window
    {
        private ReportWizardSettingFogliDiCalcoloView view { get { return DataContext as ReportWizardSettingFogliDiCalcoloView; } }
        public ReportWizardSettingFogliDiCalcoloWnd()
        {
            InitializeComponent();
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            view.AcceptButton();
            this.Close();
        }

        private void NavigationButton_Click(object sender, RoutedEventArgs e)
        {
            DocumentPreviewControl.ShowNavigationPane = true;
        }

        private void ComboBoxEdit_EditValueChanged(object sender, DevExpress.Xpf.Editors.EditValueChangedEventArgs e)
        {
            if (!view.IsLocked)
                view.Preview((int)e.NewValue);
        }
    }
}
