using Commons;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JReport
{
    public class JReportHelper
    {
        public static void LoadFastReportLocatizationFile(string currentLanguageCode)
        {
            try
            {

                string fileName = null;
                string ApplicationPath = AppDomain.CurrentDomain.BaseDirectory;
                ApplicationPath = System.IO.Path.Combine(ApplicationPath, @"Resources\FastReport");

                switch (currentLanguageCode)
                {
                    case "it-IT":
                        //FastReport.Utils.Res.LoadLocale(System.IO.Path.Combine(ApplicationPath, "Italian.frl"));
                        //var resourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames().Where(name => name.Contains("Italian"));
                        //fileName = @"JReport.LocalizationFiles.Italian.frl";
                        fileName = @"Italian.frl";
                        break;

                    case "de-DE":
                        fileName = @"German.frl";
                        break;

                    default:
                        break;
                }

                //var assembly = Assembly.GetExecutingAssembly();
                //var stream = assembly.GetManifestResourceStream(fileName);
                FastReport.Utils.Res.LoadLocale(System.IO.Path.Combine(ApplicationPath, fileName));

                FastReport.EnvironmentSettings e = new FastReport.EnvironmentSettings();
                e.ReportSettings.ShowProgress = false;

            }
            catch (Exception ex)
            {
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex.Message);
            }


        }

        public static string ReplaceSymbolNotAllowInReport(string Stringa)
        {
            if (string.IsNullOrEmpty(Stringa)){return "";}
            Stringa = Stringa.Replace(" ", "_");
            Stringa = Stringa.Replace("-", "_");
            Stringa = Stringa.Replace(".", "_");
            Stringa = Stringa.Replace("+", "piu");
            Stringa = Stringa.Replace("*", "per");
            Stringa = Stringa.Replace(":", "div");
            return Stringa;
        }
    }
}
