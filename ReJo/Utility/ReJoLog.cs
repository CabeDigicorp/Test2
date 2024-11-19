using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace ReJo.Utility
{
    public class ReJoLog
    {
        //call example
        //MainAppLog.Error(MethodBase.GetCurrentMethod(), string.Format("ProjectSave: {0}", exc.Message));
        static string RepositoryName { get => "ReJo"; }

        static ReJoLog()
        {
            SetLog4NetConfiguration();
        }

        private static void SetLog4NetConfiguration()
        {
            try
            {
                var repo = LogManager.CreateRepository(RepositoryName);

                var ass = Assembly.GetExecutingAssembly();
                string logPath = Path.GetDirectoryName(ass.Location);
                string fileConfigPath = String.Format("{0}\\Resources\\log4net.config", logPath);
                string logFilePath = Path.Combine(logPath, "MainApp.log");

                GlobalContext.Properties["LogFileName"] = logFilePath;
                var fileInfo = new FileInfo(fileConfigPath);
                log4net.Config.XmlConfigurator.Configure(repo, fileInfo);


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            } 

        }

        static ILog GetLog(System.Reflection.MethodBase method)
        {

            ILog Log = LogManager.GetLogger(RepositoryName, string.Format("{0}::{1}", method.DeclaringType, method.Name));
            return Log;
        }


        public static void Info(System.Reflection.MethodBase method, string str = null)
        {
            if (str == null)
                GetLog(method).Info("");
            else
                GetLog(method).Info(str);
        }

        public static void Error(System.Reflection.MethodBase method, string str = null, Exception e = null)
        {
            

            if (str == null)
                GetLog(method).Error("");
            else
            {
                if (e != null)
                {
                    string str1 = string.Format("{0} - StackTrace: \"{1}\"", str, e.StackTrace);
                    GetLog(method).Error(str1);
                }
                else
                    GetLog(method).Error(str);
            }
        }

        public static void Show()
        {
            try
            {

                var ass = Assembly.GetExecutingAssembly();
                string logPath = Path.GetDirectoryName(ass.Location);

                //Apertura del file di log
                var process = new Process();
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.FileName = String.Format("{0}\\{1}.log", logPath, RepositoryName);
                process.Start();


                //System.Diagnostics.Process.Start(String.Format("{0}.log", RepositoryName));
            }
            catch
            {

            }




        }
    }
}
