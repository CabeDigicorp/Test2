using CommonResources;
using Commons;
using System;
using System.Collections.Generic;
//using System.Deployment.Application;
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
using System.Windows.Shapes;

namespace MainApp
{
    /// <summary>
    /// Interaction logic for LoginWnd.xaml
    /// </summary>
    public partial class LoginWnd : Window
    {
        bool _isNewLicenseValid = false;
        public bool IsNewLicenseValid { get => _isNewLicenseValid; }
        
        public LoginWnd()
        {
            InitializeComponent();
            Load();
        }

        private void Load()
        {

            AppVersionText.Text = string.Empty;// App.ShortDeploymentVersion;
            
            LanguageItem lanItem = LanguageHelper.LanguagesItems.FirstOrDefault(item => item.Code == LanguageHelper.CurrentLanguageCode);
            LanguageCombo.ItemsSource = LanguageHelper.LanguagesItems;
            LanguageCombo.SelectedItem = lanItem;

            Translate();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }


        private void ValidateButton_Click(object sender, RoutedEventArgs e)
        {
            Cursor previousCursor = Cursor;
            Cursor = Cursors.Wait;

            _isNewLicenseValid = LicenseHelper.ValidateLicense(LicenseCodeText.Text);

            if (_isNewLicenseValid)
            {
                ValidationFailedText.Text = LicenseHelper.GetLicenseInfo();
                TrialTextBox.Visibility = Visibility.Collapsed;
            }
            else
                ValidationFailedText.Text = LicenseHelper.LastLicenseStatus;

            Cursor = previousCursor;
        }

        private void Translate()
        {

            BenvenutoInJoinText.Text = LocalizationProvider.GetString("BenvenutoInJoin");
            InsertLicenseText.Text = LocalizationProvider.GetString("InserireLicenza");
            ValidateButton.Content = LocalizationProvider.GetString("AttivaLicenza");
            ExitButton.Content = LocalizationProvider.GetString("Esci");
            GoText.Text = LocalizationProvider.GetString("Avvia");
            TrialTextBox.Text = LocalizationProvider.GetString("versioneDimostrativa");

            ValidationFailedText.Text = LicenseHelper.GetLicenseInfo();
        }

        private void LanguageCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LanguageItem item = LanguageCombo.SelectedItem as LanguageItem;
            LanguageHelper.SetApplicationLanguage(item.Code);
            Translate();
        }
    }
}
