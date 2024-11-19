using CommonResources;
using Commons;
using MainApp.Resources;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telerik.Windows.Controls;
using WPFLocalizeExtension.Engine;


/// <summary>
/// **************************************************************************************
/// xaml find and replace per inserire lex:Loc (Notepad++)
/// 
/// Find: Text="(\b.+?)"
/// Replace: Text="{lex:Loc (\1)}"
/// 
/// Find: Description="(\b.+?)"
/// Replace: Description="{lex:Loc (\1)}"
/// 
/// Find: Header="(\b.+?)"
/// Replace: Header="{lex:Loc (\1)}"
/// 
/// Find: Title="(\b.+?)"
/// Replace: Title="{lex:Loc (\1)}"
/// 
/// /// Find: Tooltip="(\b.+?)"
/// Replace: Tooltip="{lex:Loc (\1)}"
/// 
/// *****************************************************************************************
/// Per estrarre le chiavi per la traduzione xaml
/// 
/// Find: ({lex:Loc (\b.+?)})
/// Replace: \r\n\1\r\n
/// 
/// Mark All con bookmark line
/// Remove Unbookmark line
/// 
/// Extract strings
/// Find: ({lex:Loc (\b.+?)})
/// Replace: \2
/// 
/// *****************************************************************************************
/// Per estrarre le chiavi per la traduzione c#
/// 
/// Find: (LocalizationProvider.GetString\("(\b.+?)"\))
/// Replace: \r\n\1\r\n
/// 
/// Mark All con bookmark line
/// Remove Unbookmark line
/// 
/// Extract strings
/// Find: (LocalizationProvider.GetString\("(\b.+?)"\))
/// Replace: \2
/// 
/// *****************************************************************************************
/// Per estrarre solo le frasi univoche
/// 
/// Edit-> Line Operations -> Remove Duplicate Lines
/// 




namespace MainApp
{
    public static class LanguageHelper
    {

        static List<LanguageItem> _languageItems = new List<LanguageItem>();
        public static List<LanguageItem> LanguagesItems { get => _languageItems; }

        static public LanguageEnum CurrentLanguage { get; private set; } = LanguageEnum.English_US;


        static public string CurrentLanguageCode
        {
            get
            {
                switch (CurrentLanguage)
                {
                    case LanguageEnum.English_US:
                        return "en-US";
                    case LanguageEnum.Italian_IT:
                        return "it-IT";
                    case LanguageEnum.German_DE:
                        return "de-DE";
                    default:
                        return "en-US";
                }
            }
            set
            {
                switch (value)
                {
                    case "en-US":
                        CurrentLanguage = LanguageEnum.English_US;
                        break;
                    case "it-IT":
                        CurrentLanguage = LanguageEnum.Italian_IT;
                        break;
                    case "de-DE":
                        CurrentLanguage = LanguageEnum.German_DE;
                        break;
                    default:
                        CurrentLanguage = LanguageEnum.English_US;
                        break;
                }

            }
        }
 

        static public void Load()
        {

            try
            {
                AddLanguages();

                //var versionInfo = FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetEntryAssembly().Location);
                //string appSettingsPath = string.Format("{0}\\{1}\\{2}\\",
                //        MainMenuView.GetApplicationDataFolder(),
                //        versionInfo.CompanyName,//Digi Corp
                //        versionInfo.ProductName);//Join

                string appSettingsPath = MainMenuView.GetApplicationDataFolder();

                //Carico il file AppSettings.ini
                AppSettings appSettings = null;
                string json = null;

                if (appSettingsPath != null && appSettingsPath.Any())
                {

                    string appSettingsFullFileName = string.Format("{0}{1}", appSettingsPath, AppSettings.AppSettingsFileName);

                    if (File.Exists(appSettingsFullFileName))
                    {

                        json = System.IO.File.ReadAllText(appSettingsFullFileName);
                    }
                }

                if (json != null)
                    JsonSerializer.JsonDeserialize<AppSettings>(json, out appSettings, typeof(AppSettings));

                string currentLanguagerCode = CurrentLanguageCode;
                if (appSettings != null)
                    currentLanguagerCode = appSettings.LanguageCode;

                SetApplicationLanguage(currentLanguagerCode);
            }
            catch (Exception exc)
            {
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), "LanguageHelper load error: " + exc.Message);
            }

            

        }

        public static void SetApplicationLanguage(string languageCode)
        {
            CurrentLanguageCode = languageCode;

            LocalizeDictionary.Instance.SetCurrentThreadCulture = true;
            LocalizeDictionary.Instance.Culture = new CultureInfo(CurrentLanguageCode);

            CultureInfo.CurrentCulture = new CultureInfo(CurrentLanguageCode, true);
            CultureInfo.CurrentUICulture = new CultureInfo(CurrentLanguageCode, true);

            if (CurrentLanguage != LanguageEnum.English_US)
            {
                JReport.JReportHelper.LoadFastReportLocatizationFile(CurrentLanguageCode);
            }
        }

        private static void AddLanguages()
        {
            _languageItems.Clear();

            //_languageItems.Add(new LanguageItem()
            //{
            //    Code = "",
            //    Language = LanguageEnum.Nothing,
            //    DisplayName = LocalizationProvider.GetString("Nessuno"),
            //});
            _languageItems.Add(new LanguageItem()
            {
                Code = "de-DE",
                Language = LanguageEnum.German_DE,
                DisplayName = "Deutsche (DE)",
            });

            _languageItems.Add(new LanguageItem()
            {
                Code = "en-US",
                Language = LanguageEnum.English_US,
                DisplayName = "English (US)",
            });

            _languageItems.Add(new LanguageItem()
            {
                Code = "it-IT",
                Language = LanguageEnum.Italian_IT,
                DisplayName = "Italiano (IT)",
            });


        }
    }

    public enum LanguageEnum
    {
        Nothing = 0,
        English_US,
        Italian_IT,
        German_DE,
    }

    public class LanguageItem
    {
        public string Code { get; set; } = string.Empty;
        public LanguageEnum Language { get; set; }
        public string DisplayName { get; set; } = string.Empty;
    }
}
