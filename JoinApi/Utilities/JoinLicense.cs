using LogicNP.CryptoLicensing;
using ModelData.Utilities;
using ModelData.Dto;
using System.ComponentModel;
using Humanizer.Localisation;
using Microsoft.AspNetCore.Http;

namespace JoinApi.Utilities
{
    public class JoinLicense
    {

        #region Francesco

        CryptoLicense _license = null;

        public string Message { get; protected set; } = string.Empty;

        //JoinV2.netlicproj(v100) publish 1.0.0
        static string ValidationKey { get => "AMAAMADPjuXaJ3vGoS3qQRz9KlMVAityeeZFlC4TPqouZckbpMNzxRhLPgPYKN5DPkOEFEkDAAEAAQ=="; }

        public bool ValidateLicense(string licenseCode)
        {
            if (string.IsNullOrEmpty(licenseCode))
                return false;

            try
            {
                CryptoLicense license = new CryptoLicense(ValidationKey);
                license.StorageMode = LicenseStorageMode.None;
                license.LicenseServiceURL = "http://joinlicense.digicorp.it/Service.asmx";//register
                license.LicenseCode = licenseCode;

                if (!license.ValidateSignature())
                {
                    Message = "ValidateSignature failed";
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
                        Message = string.Format("{0} - {1}", ls.ToString(), ex.Message);
                    }
                    else if ((ls & LicenseStatus.ServiceNotificationFailed) != 0) // Could not activate
                    {
                        Exception ex = license.GetStatusException(LicenseStatus.ServiceNotificationFailed);
                        Message = string.Format("{0} - {1}", ls.ToString(), ex.Message);
                    }
                    else if ((ls & LicenseStatus.GenericFailure) != 0) // Could not activate
                    {
                        Exception ex = license.GetStatusException(LicenseStatus.GenericFailure);
                        Message = string.Format("{0} - {1}", ls.ToString(), ex.Message);
                    }
                    else
                    {
                        Message = ls.ToString();
                    }
                    return false;

                }
                else
                {

                    if (IsFeaturePresent(license, LicenseFeature.Feature_Web))
                    {

                        //good
                        _license = license;

                        return true;
                    }
                    else
                    {
                        Message = "Feature Web non presente";
                    }
                    return false;
                }


            }
            catch (Exception ex)
            {

            }

            return false;
        }

        private void License_FloatingHeartBeat(object sender, FloatingHeartBeatEventArgs e)
        {
            if (e.Exception != null)
            {
                string msg = string.Format("{0} - {1}", "Floating heart beat failed", e.Exception.Message);
                Message = msg;
            }
        }

        public bool DeactivateLicense()
        {
            if (_license == null)
                return false;

            try
            {
                _license.Dispose();
            }
            catch (Exception exc)
            {

            }
            return false;
        }


        public bool IsFeaturePresent(LicenseFeature feature)
        {
            if (_license == null)
                return false;

            if (_license.IsFeaturePresentEx((int)feature))
                return true;

            return false;
        }

        static bool IsFeaturePresent(CryptoLicense license, LicenseFeature feature)
        {
            if (license == null)
                return false;

            if (license.IsFeaturePresentEx((int)feature))
                return true;

            return false;
        }

        #endregion


        public static LicenzaDto DecodeLicense(string licenseKey)
        {
            return DecodeLicense(licenseKey, null, false);
        }

        public static LicenzaDto DecodeLicense(string licenseKey, string? codiceCliente, bool outputKey)
        {
            LicenzaDto decoded = new LicenzaDto();

            if (string.IsNullOrEmpty(licenseKey))
            {
                decoded.IsValid = false;
                decoded.AdditionalInfo = "Chiave di licenza mancante";
            }
            else
            {
                if (outputKey) decoded.ChiaveLicenza = licenseKey;

                try
                {
                    CryptoLicense license = new CryptoLicense(ValidationKey);
                    license.StorageMode = LicenseStorageMode.None;
                    license.LicenseServiceURL = "http://joinlicense.digicorp.it/Service.asmx";//register
                    license.LicenseCode = licenseKey;

                    if (!license.ValidateSignature())
                    {
                        decoded.IsValid = false;
                        decoded.AdditionalInfo = "Verifica della firma non riuscita";
                    }
                    else
                    {
                        decoded.AdditionalInfo = string.Empty;
                        const string SEMICOLONSPACER = "; ";

                        decoded.IsValid = license.Status == LicenseStatus.Valid;

                        if (!decoded.IsValid)
                        {
                            foreach (var value in Enum.GetValues<LicenseStatus>())
                            {
                                if ((license.Status & value) != 0)
                                {
                                    switch (value)
                                    {
                                        case LicenseStatus.Valid:
                                            //Non ci dovrei mai arrivare
                                            break;
                                        case LicenseStatus.Deactivated:
                                            decoded.IsDisabled = true;
                                            decoded.AdditionalInfo = JoinStringsWithSpacer(decoded.AdditionalInfo, "Licenza disattivata", SEMICOLONSPACER);
                                            break;
                                        case LicenseStatus.Expired:
                                            decoded.IsExpired = true;
                                            decoded.AdditionalInfo = JoinStringsWithSpacer(decoded.AdditionalInfo, "Licenza scaduta", SEMICOLONSPACER);
                                            break;
                                        default:
                                            string firstPart = Enum.GetName<LicenseStatus>(value) ?? string.Empty;
                                            string secondPart = license.GetStatusException(value)?.ToString() ?? string.Empty;
                                            string joined = JoinStringsWithSpacer(firstPart, secondPart, " - ");
                                            decoded.AdditionalInfo = JoinStringsWithSpacer(decoded.AdditionalInfo, joined, SEMICOLONSPACER);
                                            break;
                                    }
                                }
                            }
                        }

                        decoded.CodiceCliente = GetCodiceCliente(license);
                        if (string.IsNullOrWhiteSpace(decoded.CodiceCliente))
                        {
                            decoded.AdditionalInfo = JoinStringsWithSpacer(decoded.AdditionalInfo,  "Codice cliente vuoto", SEMICOLONSPACER);
                        }
                        else if (!string.IsNullOrWhiteSpace(codiceCliente) && decoded.CodiceCliente.Trim().ToLower() != codiceCliente.Trim().ToLower())
                        {
                            //verifico la corrispondenza del codice cliente della licenza con quello _eventualmente_ passato in input
                            decoded.IsValid = false;
                            decoded.AdditionalInfo = JoinStringsWithSpacer(decoded.AdditionalInfo,  "Codice cliente non corrispondente", SEMICOLONSPACER);
                        }

                        decoded.ExpirationDate = license.HasDateExpires ? DateOnly.FromDateTime(license.DateExpires) : DateOnly.MaxValue;

                        decoded.Activations = license.MaxActivations;
                        if (decoded.Activations <= 0)
                        {
                            decoded.IsValid = false;
                            decoded.AdditionalInfo = JoinStringsWithSpacer(decoded.AdditionalInfo,  "Non contiene attivazioni", SEMICOLONSPACER);
                        }

                        foreach (var f in Enum.GetValues<LicenseFeature>())
                        {
                            if (IsFeaturePresent(license, f)) decoded.LicenseFeatures.Add(f);
                        }
                        if (decoded.LicenseFeatures.Count() <= 0)
                        {
                            decoded.IsValid = false;
                            decoded.AdditionalInfo = JoinStringsWithSpacer(decoded.AdditionalInfo,  "Non contiene features", SEMICOLONSPACER);
                        }


                    }
                    
                }
                catch (Exception ex)
                {
                    decoded.AdditionalInfo += (decoded.AdditionalInfo.Length > 0 ? "; " : "") + ex.Message;
                }
            }

            return decoded;
        }

        private static string JoinStringsWithSpacer(string? first, string? second, string spacer)
        {
            first = (first ?? string.Empty).Trim();
            second = (second ?? string.Empty).Trim();

            if (first.Length > 0 && second.Length > 0)
                return first + spacer + second;
            else if (first.Length > 0 && second.Length == 0)
                return first;
            else if (first.Length == 0 && second.Length > 0)
                return second;
            else
                return string.Empty;
        }

        public static string GetCodiceCliente(CryptoLicense license)
        {
            if (license == null)
                return string.Empty;

            if (license.HasUserData)
            {
                string userCode = license.GetUserDataFieldValue("UserCode", "#");
                return userCode;
            }
            return string.Empty;
        }

        public static string GetNomeCliente(CryptoLicense license)
        {
            if (license == null)
                return string.Empty;

            if (license.HasUserData)
            {
                string userName = license.GetUserDataFieldValue("UserName", "#");
                return userName;
            }
            return string.Empty;
        }

    }

    


}
