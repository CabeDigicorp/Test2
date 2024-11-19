

using CommonResources;
using Commons;
using DevExpress.Mvvm;
using DevExpress.Xpf.Core;
using log4net.Config;
using System;
//using System.Deployment.Application;
using System.Drawing;
using System.Globalization;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.SplashScreen;
using WPFLocalizeExtension.Engine;

namespace MainApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static Type[] types;
        /// <summary>
        /// FTP register:
        /// Publishing Folder location: ftp://join.digicorp.it/join.digicorp.it/Install/v0_0
        /// Installation Folder URL: http://join.digicorp.it/Install/v0_0
        /// user: GPTDIGICORP
        /// pw: Pier@ntonella
        /// 
        /// Server Locale:
        /// Publishing Folder location: C:\Join\JoinInstall\v0_0\
        /// Installation Folder URL: \\PC-COMPILE4\JoinInstall\v0_0\
        /// </summary>
        /// 

        public App()
        {

            //Register Syncfusion license
            //Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MTUwODYyQDMxMzcyZTMyMmUzMGdvZG16Ujdxb0dBeEFXS3R1QktUbmx1ZnBRUnA0YjZ4N0ZqVGxtVW9tQ1k9");//Version 17.2...
            //Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MTU5ODg1QDMxMzcyZTMzMmUzME1uUDVDNU5qM3paVTkySXdCQ1hJVXlYMzRlcXliQkJaRTNOK2haUGR0Ujg9");//Version 17.3.0.14 Essential Studio Enterprise Edition - Community license
            //Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mjg0MjE5QDMxMzcyZTMzMmUzMG80OXkxbEFNL0VLN21jano5R1ZjMGJqb2JSbFUyT1hOMnY1VFVPMkFGeVE9");//Version 17.3.0.14 Essential Studio WPF (10/07/2020)
            //Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MzEyMzYzQDMxMzgyZTMyMmUzMFZTVjBFYUxEcTJRRlVyc00xWXl3ZEZrY3dKYnFvNkpTVzJkVXdKTGhSM009");//Version 18.2.0.56 Essential Studio WPF (03/09/2020)
            //Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("NDI5ODc4QDMxMzkyZTMxMmUzMGswT2Fmc1ZpeUZZWGpvUU8rcmVDeHdRcGxUdzgwYjNlSnNWZ3FKR3NhMk09");//Version 19.1.0.56 Essential Studio WPF (14/04/2021)
            //Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("NDM4NTcyQDMxMzkyZTMxMmUzMEhRcS94T0tYVDJTZHBYUDJ6OG82ank5Szg1QzVqekZZdUNyUmlNUWgxMTQ9");//Version 19.1.0.56 Essential Studio WPF (29/04/2021)
            //Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("NDU4MTQ4QDMxMzkyZTMxMmUzMGg5RFBVR3pDZzJEZTM1TTVqQzBTYXZnRUxEN1hRQndpNTRQNEdSNUxEK3c9");//Version 19.1.0.67 Essential Studio WPF (08/06/2021)
            //Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("NDY5MTU1QDMxMzkyZTMyMmUzME84ZnJSZ01GQVBrY0ZjenZ6YUErcHg4MHoxNHdXa0E3eENVUGszVE45WVE9");//Version 19.2.0.46 Essential Studio WPF (06/07/2021)
            //Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("NTM5NzM0QDMxMzkyZTMzMmUzMEl2aGV2VGpVRnJBaDN2dHcwdmVCeXVUVGVwUVNZbkRKMkdYOWlyaTdEWnc9");//Version 19.2.0.55 Essential Studio WPF (26/11/2021)

            //Claim License Key  (licenza community) https://www.syncfusion.com/sales/communitylicense                                                                                                                                                              
            //Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mgo+DSMBaFt+QHJqVk1hXk5Hd0BLVGpAblJ3T2ZQdVt5ZDU7a15RRnVfR1xrSH9SdERlWnddeQ==;Mgo+DSMBPh8sVXJ1S0R+X1pFdEBBXHxAd1p/VWJYdVt5flBPcDwsT3RfQF5jTH9bd0RiW35YdHRUTw==;ORg4AjUWIQA/Gnt2VFhiQlJPd11dXmJWd1p/THNYflR1fV9DaUwxOX1dQl9gSXtSfkRiWH1fdnBXRmk=;MjEwMzAwMkAzMjMxMmUzMjJlMzNvM3gvRmNsRFozY2FXQlZWdWRvMzBlSk1Mb0Z5Y1RXMEtISE0xWS80a1VBPQ==;MjEwMzAwM0AzMjMxMmUzMjJlMzNHYXBJNTNTN203T0YvYjh3eHZHK2FpVW1IR3R5bWpxOEU2cUd2cGZZMTVrPQ==;NRAiBiAaIQQuGjN/V0d+Xk9HfV5AQmBIYVp/TGpJfl96cVxMZVVBJAtUQF1hSn5Wd01iWH5ccXJQR2BV;MjEwMzAwNUAzMjMxMmUzMjJlMzNvQ2U2VDdoUFljZmdoR0pRU1hHRCtZYWt2TmRucUs0d21ZK3JiNlZCOHhrPQ==;MjEwMzAwNkAzMjMxMmUzMjJlMzNJMTE5TzBwVW9VR1p4K3lHano3Z09Oc0xVOSt1dW15Z0tBL1NTZDhUZytNPQ==;Mgo+DSMBMAY9C3t2VFhiQlJPd11dXmJWd1p/THNYflR1fV9DaUwxOX1dQl9gSXtSfkRiWH1fdnJTQ2k=;MjEwMzAwOEAzMjMxMmUzMjJlMzNtVmV1cUMzVWhhalU4UlpVMllNYXpseXBjWk93UktpRHYwQlppVWYzcU9jPQ==;MjEwMzAwOUAzMjMxMmUzMjJlMzNhZHAwQzJCa0pUTnBYUFJDVnhoYnpqNlAyVVZ5Y0Q0dUU0WE9SVTAxV09BPQ==;MjEwMzAxMEAzMjMxMmUzMjJlMzNvQ2U2VDdoUFljZmdoR0pRU1hHRCtZYWt2TmRucUs0d21ZK3JiNlZCOHhrPQ==");
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1NDaF5cWWtCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdnWH9cc3RRRmRYUkd2W0A=");


            CompatibilitySettings.AllowThemePreload = true;

            //Eazfuscator license key 2020.4: 25M76BBA-6FE2UMRM-3R9P8C5S-FR7854QA-DFAMEH4Z-9LJC5LMS-FLSPNTE3-R2AFLZ7J


            //SplashScreenManager.Create(() => new SplashScreenWnd(), new DXSplashScreenViewModel
            //{
            //}).ShowOnStartup();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {

            //Mouse.OverrideCursor = Cursors.Wait;


            //Disable shutdown when the dialog closes
            Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            //XmlConfigurator.Configure();//AU for NET6
            MainAppLog.Info(System.Reflection.MethodBase.GetCurrentMethod(), "---------------------------------------------------------------------------------------------------------------------");

            LanguageHelper.Load();

            MainMenuView.InitAppSettingsPath();

            string licenseCode = null;
            if (Environment.MachineName == MainMenuView.Join360MachineName)
                licenseCode = LicenseHelper.Join360LicenseCode;


            if (!LicenseHelper.ValidateLicense(licenseCode))
            {
                LoginWnd loginWnd = new LoginWnd();
                loginWnd.SourceInitialized += (x, y) => loginWnd.HideMinimizeAndMaximizeButtons();
                if (loginWnd.ShowDialog() == true)
                {

                    if (loginWnd.IsNewLicenseValid)
                    {
                        Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
                    }
                    else
                    {
                        //viene lanciato in modalità trial
                        if (!LicenseHelper.ValidateLicense(LicenseHelper.TrialLicenseCode))
                        {
                            MessageBox.Show(LocalizationProvider.GetString("LicenzaTrialNonValida"), LocalizationProvider.GetString("AppName"), MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            Current.Shutdown(-1);
                        }
                        else
                            Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
                    }
                }
                else
                {
                    Current.Shutdown(-1);
                    return;
                }


                //return;
            }
            else
                Current.ShutdownMode = ShutdownMode.OnMainWindowClose;


            //Mouse.OverrideCursor = null;

            ShowSplashScreen();

            //////////////////////
            //DevExpress init
            ApplicationThemeHelper.ApplicationThemeName = DevExpress.Xpf.Core.Theme.NoneName;


            //ApplicationThemeHelper.SaveApplicationThemeName();
            //types = new Type[] { typeof(DevExpress.Xpf.Spreadsheet.SpreadsheetControl) };
            DevExpress.Xpf.Core.ThemeManager.PreloadThemeResource("Office2019Colorful");
            DevExpress.Xpf.Core.ThemeManager.PreloadThemeResource(DevExpress.Xpf.Core.Theme.Win11LightName);// "Win11Light");

            /////////////////////



        }

        protected override void OnStartup(StartupEventArgs e)
        {

            try
            {
                base.OnStartup(e);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        private static void ShowSplashScreen()
        {


            var dataContext = (SplashScreenDataContext)RadSplashScreenManager.SplashScreenDataContext;
            dataContext.Content = "Loading Application";
            dataContext.Footer = "This is the footer.";

            RadSplashScreenManager.Show<SplashScreenCtrl>();


        }

        protected override void OnExit(ExitEventArgs e)
        {

            bool ret = LicenseHelper.DeactivateLicense();

            base.OnExit(e);
        }

        //public static string ShortDeploymentVersion { get => "2.4"; }//ricordarsi di cambiare la versione incrementale anche nel publish

    }
}
