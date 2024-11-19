
using CommonResources;
using LogicNP.CryptoLicensing;
using System;
using System.Collections.Generic;
//using System.Deployment.Application;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using static DevExpress.Pdf.Native.BouncyCastle.Asn1.X509.Target;


//Register digicorp.it

//Server: iisrc8504.dadapro.com
//Username: remoteadm@digicorp.it
//PW: Pier@ntonella
//Nome del sito: digicorp.it
//URL di destinazione: http://digicorp.it


//.../JoinLicense/install/install.aspx
//http://joinlicense.digicorp.it/activationconsole/listactivations.aspx
//username: programmer
//pw:Pier@ntonella


////////////////////////////////////////////////////////////////
///21/02/2020 Licenza developer (installato in macchina PC-ALE)
//CryptoLicense license code
//Type of license : 1-Developer License
//Number of Licenses : 1

//Support And Upgrade Subscription Purchased : No
//Support And Upgrade Subscription Ends on : 21 March 2020

//CryptoLicensing For.Net 2018 Enterprise Edition License String: 
//
//  lgCAAP / oFJsJ6NUBgwBOYW1lPUdpYW4gUGllcm8gVHJ1Y2NvbG8jQ29tcGFueT1EaWdpIE
//  NvcnAgcy5yLmwuI0VtYWlsPXByb2dyYW1tZXJAZGlnaWNvcnAuaXQjU3Vic2NyaXB0aW9u
//  PUZhbHNlI1N1YnNjcmlwdGlvblZhbGlkVGlsbD0yMS1NYXItMjAyMAEHYihOHJ7uzR+gEh
//  /F1gBC4aw44qu7lxI9VVYZMrqMXknx4DkX91pgV0XIf+QKpFVX


//LICENSE CODE TO USE WITH LICENSE SERVICE(required only if you are using the license service) :  
//this.Generator.SetLicenseCode("lgCAABWtGZsJ6NUBgwBOYW1lPUdpYW4gUGllcm8gVHJ1Y2NvbG8jQ29tcGFueT1EaWdpIENvcnAgcy5yLmwuI0VtYWlsPXByb2dyYW1tZXJAZGlnaWNvcnAuaXQjU3Vic2NyaXB0aW9uPUZhbHNlI1N1YnNjcmlwdGlvblZhbGlkVGlsbD0yMS1NYXItMjAyMAEHeedNGSqrtfsEtu6chPGMHekWm5N05LMo7N+6S+nvXm34urqzeE74HDnPgUipgjFt");


//Che sia la licenza sales questa qui sotto?

//Type of license : 1-Developer License
//Number of Licenses : 1

//Support And Upgrade Subscription Purchased : No
//Support And Upgrade Subscription Ends on : 21 March 2020

//CryptoLicensing For.Net 2018 Enterprise Sales Edition License String:

//  lgCBABYrSLZz6NUBAQCDAE5hbWU9R2lhbiBQaWVybyBUcnVjY29sbyNDb21wYW55PURpZ2
//  kgQ29ycCBzLnIubC4jRW1haWw9cHJvZ3JhbW1lckBkaWdpY29ycC5pdCNTdWJzY3JpcHRp
//  b249RmFsc2UjU3Vic2NyaXB0aW9uVmFsaWRUaWxsPTIxLU1hci0yMDIwAQ8LDM0XAs8Nnh
//  eRPTp5zITzyW+EsBG9mQuDfTOxg+yTYcRW/+grlJgYZ7/C4GOqO1U=

/////////////////////////////////////////////////////////////////////////////////////




namespace Commons
{
    public class LicenseHelper
    {
        static CryptoLicense _license = null;
        static public string LastLicenseStatus { get; protected set; } = string.Empty;
        static public string LicenseFilePath {get; set;}
        //static public string LicenseServiceURL { get; set; }
        private static string _encriptionStringkey = "b14ca5898a4e4142aace2ea2143a2410";


        static public string ValidationKey 
        {
            ////JoinV2.netlicproj(v100) publish 1.0.0
            get => "AMAAMADPjuXaJ3vGoS3qQRz9KlMVAityeeZFlC4TPqouZckbpMNzxRhLPgPYKN5DPkOEFEkDAAEAAQ==";
        }


        static public string TrialLicenseCode
        {
            //JoinV2.netlicproj, Limit usage days to 20day,notifyService,verify local time, Feature_4D, UserCode = 0, UserName = Trial Version
            get => "lhKiA/1Nq38K+dgBFAAhAFVzZXJDb2RlPTAjVXNlck5hbWU9VHJpYWwgVmVyc2lvbgEBFRGyVvhkiExJfwQLvVhWx6R4c5jdosYeWRKlCCJM1H/TX6M3YNstKl8UCoYWp/Px";
        }

        static public string Join360LicenseCode
        {
            //Licenza per Join360 (abilitata solo per la macchina SRVXA01)
            //creata con JoinV2.netlicproj
            //Codice cliente: 859
            //Info: Digi Corp s.r.l.
            //concorrenti: 1
            //scadenza: 01/04/2026 00:00:00
            //features: 4D
            //Codice macchina: d9xZ9i28l7YnHHyK5/1KPg==

            get => "9hGiIwPJk79IddoBgHL+W9HB3AEQAHfcWfYtvJe2Jxx8iuf9Sj4BACUAVXNlckNvZGU9ODU5I1VzZXJOYW1lPURpZ2ljb3JwIHMuci5sLgEBBQAAAHoNIUh4Vz44vYAID+/oo43nQKSj982oLt97PKOQ5Xn03F+ThWCLbzq+MCTAPwTohw==";
        }

        public static bool ValidateLicense(string licenseCode, bool onNextSession = false)
        {
             //Scopo: Far funzionare Join senza collegamento internet
//            return true;



            LastLicenseStatus = string.Empty;

            try
            {
                

                CryptoLicense license = new CryptoLicense(ValidationKey);
                license.StorageMode = LicenseStorageMode.ToFile;
                license.FileStoragePath = GetFileStoragePath(license);
                license.LicenseServiceURL = "http://joinlicense.digicorp.it/Service.asmx";//register
                //license.LicenseServiceURL = "http://mosaicosklicense.digicorp.it/Service.asmx";//register mosaicosk
                //license.LicenseServiceURL = "http://localhost:3030/Service.asmx";
                //license.LicenseServiceURL = "http://192.168.0.81/JoinLicense/Service.asmx";
                //license.LicenseServiceURL = "http://52.157.83.125/JoinLicense/Service.asmx";
                //license.LicenseServiceURL = "http://digicorpdata.it/JoinLicense/Service.asmx";
                //license.LicenseServiceURL = "http://www.digicorpdata.it/JoinLicense/Service.asmx";

                if (licenseCode != null)
                {
                    license.LicenseCode = licenseCode;

                    if (!license.ValidateSignature())
                    {
                        LastLicenseStatus = "ValidateSignature failed";
                        return false;
                    }

                }
                else if (license.Load() == false)
                {
                    //license.LicenseCode = TrialLicenseCode;
                    //LastLicenseStatus = "Load license failed";
                    return false;
                }
                license.FloatingHeartBeat += License_FloatingHeartBeat;

                LicenseStatus ls = license.Status;
                if (ls != LicenseStatus.Valid)
                {
                    //not good...
                    if ((ls & LicenseStatus.ActivationFailed) != 0) // Could not activate
                    {
                        Exception ex = license.GetStatusException(LicenseStatus.ActivationFailed);
                        LastLicenseStatus = string.Format("{0} - {1}", ls.ToString(), ex.Message);
                    }
                    else if ((ls & LicenseStatus.ServiceNotificationFailed) != 0) // Could not activate
                    {
                        Exception ex = license.GetStatusException(LicenseStatus.ServiceNotificationFailed);
                        LastLicenseStatus = string.Format("{0} - {1}", ls.ToString(), ex.Message);
                    }
                    else if ((ls & LicenseStatus.GenericFailure) != 0) // Could not activate
                    {
                        Exception ex = license.GetStatusException(LicenseStatus.GenericFailure);
                        LastLicenseStatus = string.Format("{0} - {1}", ls.ToString(), ex.Message);
                    }
                    else
                    {
                        LastLicenseStatus = ls.ToString();
                    }
                    return false;
                }
                else
                {
                    //good
                    if (!onNextSession)//rende effettiva il cambio chiave al prossimo avvio di join
                        _license = license;

                    if (licenseCode != null && licenseCode.Any())
                    {
                        if (!license.Save())
                            LastLicenseStatus = "Save license failed";
                    }

                    

                    return true;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return true;

        }

        private static void License_FloatingHeartBeat(object sender, FloatingHeartBeatEventArgs e)
        {
            if (e.Exception != null)
            {
                string msg = string.Format("{0} - {1}", "Floating heart beat failed", e.Exception.Message);
                MainAppLog.Info(System.Reflection.MethodBase.GetCurrentMethod(), msg);
            }
        }

        public static bool IsLicenseValid()
        {
            if (_license == null)
                return false;

            if (_license.Status == LicenseStatus.Valid)
                return true;

            return false;
        }

        public static string GetLicenseInfo(string licenseCode = null)
        {
            CryptoLicense license = null;

            if (licenseCode != null)
            {
                license = new CryptoLicense(ValidationKey);
                license.LicenseCode = licenseCode;
                license.LicenseServiceURL = "http://joinlicense.digicorp.it/Service.asmx";//register

                if (!license.ValidateSignature())
                {
                    return "ValidateSignature failed";
                }
                
            }
            else
            {
                license = _license;
            }

            if (license == null)
                return string.Empty;


            string str = string.Empty;
            LicenseStatus ls = license.Status;
            if (ls == LicenseStatus.Valid)
                str += string.Format("\n{0}: {1}", LocalizationProvider.GetString("LicenseStatus"), LocalizationProvider.GetString("Valid"));
            else
                str += string.Format("\n{0}: {1}", LocalizationProvider.GetString("LicenseStatus"), ls);


            if (ls == LicenseStatus.Valid)
            {
                if (license.HasUserData)
                {
                    string userCode = license.GetUserDataFieldValue("UserCode", "#");
                    str += string.Format("\n{0}: {1}", LocalizationProvider.GetString("CodiceCliente"), userCode);

                    string userName = license.GetUserDataFieldValue("UserName", "#");
                    str += string.Format("\n{0}: {1}", LocalizationProvider.GetString("Info"), userName);
                }

                if (license.HasMaxUniqueUsageDays)
                    str += string.Format("\n{0}: {1}", LocalizationProvider.GetString("RemainingUniqueUsageDays"), license.RemainingUniqueUsageDays);

                if (license.HasMaxUsageDays)
                    str += string.Format("\n{0}: {1}", LocalizationProvider.GetString("RemainingUsageDays"), license.RemainingUsageDays);

                if (license.HasMaxActivations)
                {
                    //str += string.Format("{0}: {1}\n", LocalizationProvider.GetString("ActivationsAreFloating"), _license.ActivationsAreFloating);
                    str += string.Format("\n{0}: {1}", LocalizationProvider.GetString("MaxActivations"), license.MaxActivations);
                    str += string.Format("\n{0}: {1}", LocalizationProvider.GetString("GetRemainingActivationCount"), license.GetRemainingActivationCount());
                }

                if (license.HasDateExpires)
                    str += string.Format("\n{0}: {1}", LocalizationProvider.GetString("DateExpires"), license.DateExpires);

                //str += string.Format("GetLocalMachineCodeAsString: {0}\n", _license.GetLocalMachineCodeAsString());

                if (IsFeaturePresent(license, LicenseFeature.Feature_4D))
                    str += string.Format("\n{0}: {1}", LocalizationProvider.GetString("Feature"), GetLicenseFeatureName(LicenseFeature.Feature_4D));

                if (IsFeaturePresent(license, LicenseFeature.Feature_Web))
                    str += string.Format("\n{0}: {1}", LocalizationProvider.GetString("Feature"), GetLicenseFeatureName(LicenseFeature.Feature_Web));

                if (IsFeaturePresent(license, LicenseFeature.Feature_ReJo))
                    str += string.Format("\n{0}: {1}", LocalizationProvider.GetString("Feature"), GetLicenseFeatureName(LicenseFeature.Feature_ReJo));

                //str += string.Format("\n{0}: {1}", "MachineCode", _license.GetLocalMachineCodeAsString());
            }

            return str;
        }

        public static string GetLicenseCode()
        {
            
            if (_license != null)
                return _license.LicenseCode;

            return string.Empty;
        }

        public static string GetFileStoragePath(CryptoLicense license)
        {
            ////AU for NET6
            //if (ApplicationDeployment.IsNetworkDeployed)//for ClickOnce
            //{
            //    FileInfo fileInfo = new FileInfo(license.FileStoragePath);
            //    string dataDir = ApplicationDeployment.CurrentDeployment.DataDirectory;
            //    string fullFileName = string.Format("{0}\\{1}", dataDir, fileInfo.Name);
            //    return fullFileName;
            //}
            //else
            //{
            //    return license.FileStoragePath;
            //}

            FileInfo fileInfo = new FileInfo(license.FileStoragePath);
            string dataDir = LicenseFilePath;
            string fullFileName = string.Format("{0}{1}", dataDir, fileInfo.Name);
            return fullFileName;

            //return license.FileStoragePath;

        }

        public static bool DeactivateLicense()
        {
            if (_license == null)
                return false;

            try
            {
                //string ret = _license.Deactivate();
                //if (ret == null)
                //    MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), "Deactivation failed");


                _license.Dispose();
            }
            catch (Exception exc)
            {
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), exc.Message);
            }
            return false;
        }

        public static bool IsFeaturePresent(LicenseFeature feature)
        {
            return IsFeaturePresent(_license, feature);
        }

        static bool IsFeaturePresent(CryptoLicense license, LicenseFeature feature)
        {
            if (license == null)
                return false;

            if (license.IsFeaturePresentEx((int)feature))
                return true;

            return false;

        }


        public static bool IsAnyFeaturePresent(List<LicenseFeature> features, out string msg)
        {
            msg = string.Empty;
            foreach (LicenseFeature feature in features)
            {
                if (IsFeaturePresent(feature))
                    return true;

            }

            var featureList = features.Select(x => GetLicenseFeatureName(x));

            string featuresName = string.Join(", ", featureList);
            string str = string.Format("{0}: {1}", LocalizationProvider.GetString("LaFunzionalitaEDisponibileSoltantoNelleFeatures"), featuresName);
            msg = str;
            return false;
        }

        public static string GetLicenseFeatureName(LicenseFeature feature)
        {
            if (feature == LicenseFeature.Feature_4D)
                return "4D";
                //return LocalizationProvider.GetString("4D");

            if (feature == LicenseFeature.Feature_Web)
                return "Web";
                //return LocalizationProvider.GetString("Web");

            if (feature == LicenseFeature.Feature_ReJo)
                return "ReJo";
                //return LocalizationProvider.GetString("ReJo");

            return string.Empty;
        }

        public static string GetCodiceCliente()
        {
            if (_license == null)
                return string.Empty;


            string str = string.Empty;
            LicenseStatus ls = _license.Status;

            if (ls == LicenseStatus.Valid)
            {
                if (_license.HasUserData)
                {
                    string userCode = _license.GetUserDataFieldValue("UserCode", "#");
                    return userCode;
                }

            }
            return string.Empty;

        }

        #region String encription
        public static string EncryptString(string plainText)
        {
            byte[] iv = new byte[16];
            byte[] array;
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(_encriptionStringkey);
                aes.IV = iv;
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }
                        array = memoryStream.ToArray();
                    }
                }
            }
            return Convert.ToBase64String(array);
        }

        public static string DecryptString(string cipherText)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(_encriptionStringkey);
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }


        #endregion //string encryption

    }

    public enum LicenseFeature
    {
        Feature_4D = 1,
        Feature_Web = 2,
        Feature_ReJo = 3,
    }
}
