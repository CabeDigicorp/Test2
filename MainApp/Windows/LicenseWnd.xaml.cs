using CommonResources;
using Commons;
using System;
using System.Collections.Generic;
using System.IO;
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


namespace MainApp
{
    /// <summary>
    /// Interaction logic for LicenseWnd.xaml
    /// </summary>
    public partial class LicenseWnd : Window
    {
        
        //public bool IsNewLicenseValid { get; }

        public LicenseWnd()
        {
            InitializeComponent();

            RiavvioStackPanel.Visibility = Visibility.Collapsed;
            Loaded += LicenseWnd_Loaded;
        }

        private void LicenseWnd_Loaded(object sender, RoutedEventArgs e)
        {

            ValidationFailedText.Text = string.Empty;
            //if (LicenseHelper.IsLicenseValid())
            //    ValidationFailedText.Text = LicenseHelper.GetLicenseInfo();
            //else
            //    ValidationFailedText.Text = LicenseHelper.LastLicenseStatus;
            
        }

        private void ValidateButton_Click(object sender, RoutedEventArgs e)
        {
            Cursor previousCursor = Cursor;
            Cursor = Cursors.Wait;

            ValidationFailedText.Text = string.Empty;

            var isNewLicenseValid = LicenseHelper.ValidateLicense(LicenseCode.Text, true);

            if (isNewLicenseValid)
            {
                ValidationFailedText.Text = LicenseHelper.GetLicenseInfo(LicenseCode.Text);
                RiavvioStackPanel.Visibility = Visibility.Visible;
            }
            else
            {
                ValidationFailedText.Text = string.Format("\n{0}", LicenseHelper.LastLicenseStatus);
                RiavvioStackPanel.Visibility = Visibility.Collapsed;
            }

            Cursor = previousCursor;
        }

        private void RiavvioBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string location = Application.ResourceAssembly.Location;

                string path = Path.GetDirectoryName(location);
                string fileName = Path.GetFileNameWithoutExtension(location);
                location = string.Format("{0}\\{1}.exe", path, fileName);

                System.Diagnostics.Process.Start(location);
                Application.Current.Shutdown();

            }
            catch (Exception exc)
            {
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), exc.Message);
            }
        }
    }
}
