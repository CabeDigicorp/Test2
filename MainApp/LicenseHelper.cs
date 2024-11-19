using LicenseSpring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MainApp
{
    public class LicenseHelper
    {
        #region License
        private static LicenseSpringConfiguration configuration = null;

        internal static void InitLicense()
        {

            try
            {
                configuration = Config.CreateLicenseSpringConfiguration();

                var licenseManager = LicenseSpring.LicenseManager.GetInstance();

                licenseManager.Initialize(configuration);

                var productInfo = licenseManager.GetProductDetails();

                bool isInizialized = licenseManager.IsInitialized();

                //var license = licenseManager.CurrentLicense();

                //if (null != license)
                //    license.LocalCheck();
            }
            catch (Exception ex)
            {
                catchException(ex);
            }
        }

        internal static bool ActivateLicense(string licenseKey)
        {

            try
            {
                var licenseManager = LicenseSpring.LicenseManager.GetInstance();

                ILicense license = licenseManager.CurrentLicense();


                if (license == null || !license.IsActive())
                {
                    license = licenseManager.ActivateLicense(licenseKey);
                    bool isActive = license.IsActive();

                }

            }
            catch (Exception ex)
            {
                catchException(ex);
            }
            return true;

        }

        internal static bool ActivateLicense(string userId, string password)
        {
            try
            {
                var licenseManager = LicenseSpring.LicenseManager.GetInstance();

                ILicense license = licenseManager.CurrentLicense();


                if (license == null || !license.IsActive())
                {
                    license = licenseManager.ActivateLicense(userId, password);
                    bool isActive = license.IsActive();
                }

            }
            catch (Exception ex)
            {
                catchException(ex);
            }
            return true;
        }

        internal static bool DeactivateLicense(string licenseKey)
        {
            try
            {
                var licenseManager = LicenseSpring.LicenseManager.GetInstance();

                ILicense license = licenseManager.CurrentLicense();
                if (license == null || !license.IsActive())
                    return false;

                bool ret = license.Deactivate();

                return ret;
            }
            catch (Exception ex)
            {
                catchException(ex);
            }
            return false;
        }

        private static void catchException(Exception ex)
        {
            //if (ex is LicenseSpringException)
            //    MessageBox.Show(ex.Message, "LicenseSpring exception occurred:");
            //else
            //    MessageBox.Show(ex.Message, "Exception occured:");
        }

        internal static bool CheckLicense()
        {
            if (!isValidLicense())
                return false;
            if (!localCheck())
                return false;
            try
            {
                var licenseManager = LicenseSpring.LicenseManager.GetInstance();
                var license = licenseManager.CurrentLicense();
                license.Check();
                //showInfoScreen();
            }
            catch (Exception ex)
            {
                catchException(ex);
                //showInfoScreen();
                return false;
            }
            return true;
        }

        private static bool isValidLicense()
        {
            try
            {
                var licenseManager = LicenseSpring.LicenseManager.GetInstance();
                var license = licenseManager.CurrentLicense();

                if (null == licenseManager || null == license)
                    return false;
                return license.IsActive() && license.IsEnabled() && !license.IsExpired();
            }
            catch (Exception ex)
            {
                catchException(ex);
                return false;
            }
        }

        private static bool localCheck()
        {
            try
            {
                var licenseManager = LicenseSpring.LicenseManager.GetInstance();
                var license = licenseManager.CurrentLicense();
                license.LocalCheck();
            }
            catch (Exception ex)
            {
                if (ex is LicenseExpiredException)
                {
                    string msg = "Your subscription has now expired.\n" +
                    "Please contact your product owner representative as soon as possible to reactivate your subscription.";
                    MessageBox.Show(msg);
                    return false;
                }
                catchException(ex);
                return false;
            }
            return true;
        }

        #endregion



    }
}
