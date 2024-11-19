using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static System.Net.WebRequestMethods;

namespace WebServiceClient
{
    public static class ServerAddress
    {


        public static ServerName CurrentServer { get; set; }

        public static string ApiLocal { get => "https://localhost:5100"; }
        public static string ApiPcCompile4 { get => "https://192.168.0.95:5100"; }// https://pc-compile4:5100"
        public static string ApiRegister { get => "http://joinservice.digicorp.it"; }
        //public static string ApiAzure { get => "https://joinweb.westeurope.cloudapp.azure.com:5110"; }
        public static string ApiAzure { get => "https://joinweb.digicorp.it:5110"; }

        public static string WebUILocal { get => "https://localhost:5101"; }
        public static string WebUIPcCompile4 { get => "https://192.168.0.95:5101"; }// https://pc-compile4:5101"
        //public static string WebUIAzure { get => "https://joinweb.westeurope.cloudapp.azure.com"; }
        public static string WebUIAzure { get => "https://joinweb.digicorp.it"; }


        static ServerAddress()
        {
            CurrentServer = ServerName.Azure;
        }

        public static void SetCurrentServer(int serverIndex)
        {
            ServerName serverName = (ServerName)serverIndex;
            if (serverName != ServerName.Local && serverName != ServerName.PcCompile4 && serverName != ServerName.Azure)
            {
                MessageBox.Show("ServerIndex error in AppSettings.ini \n  0: Local\n  1: PcCompile4\n  2: Azure", "Join");
            }
            else
            {
                CurrentServer = serverName;
            }


        }

        public static string ApiCurrent
        {
            get
            {
                switch (CurrentServer)
                {
                    case ServerName.Local:
                        return ApiLocal;
                    case ServerName.PcCompile4:
                        return ApiPcCompile4;
                    case ServerName.Register:
                        return ApiRegister;
                    case ServerName.Azure:
                        return ApiAzure;

                }
                return string.Empty;
            }
        }

        public static string WebUICurrent
        {
            get
            {
                switch (CurrentServer)
                {
                    case ServerName.Local:
                        return WebUILocal;
                    case ServerName.PcCompile4:
                        return WebUIPcCompile4;
                    case ServerName.Azure:
                        return WebUIAzure;
                }
                return string.Empty;
            }
        }

        public enum ServerName
        {
            Local = 0,
            PcCompile4 = 1,
            Azure = 2,
            Register = 3,
        }

    }



}
