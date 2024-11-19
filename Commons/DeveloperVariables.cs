using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons
{
    public class DeveloperVariables
    {
        static string UserName { get; } = System.Security.Principal.WindowsIdentity.GetCurrent()?.Name;
        public static bool IsTesting { get => true; }

#if DEBUG
        public static bool IsUnderConstruction { get => IsAlessandroUlianaUser(); }
        public static bool IsNewStampa { get => true; }//è stata implementata per la lentezza nel caso di tanti raggruppamenti nella stampa ed errata costuzione della db di stampa
        public static bool IsUndoActive { get => true; }
        public static bool SalvaFrx { get => false; }//salvataggio del file .frx in fase di stampa fastreport
        public static bool IsAIEnabled { get => IsAlessandroUlianaUser(); }
        public static bool IsDebug { get => true; }
#else
        public static bool IsUnderConstruction { get => false; }
        public static bool IsNewStampa { get => true; }
        public static bool IsUndoActive { get => true; }
        public static bool SalvaFrx { get => false; }//salvataggio del file .frx in fase di stampa fastreport
        public static bool IsAIEnabled { get => false; }
        public static bool IsDebug { get => false; }
#endif

        private static bool IsAlessandroUlianaUser()
        {
            //string? userName = System.Security.Principal.WindowsIdentity.GetCurrent()?.Name;
            if (UserName != null && UserName.Contains("alessandro.uliana"))
                return true;

            return false;

        }

    }
}
