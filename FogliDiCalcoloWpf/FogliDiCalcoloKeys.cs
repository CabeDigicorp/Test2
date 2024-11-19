using CommonResources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FogliDiCalcoloWpf
{
    public class FogliDiCalcoloKeys
    {
        //public static string LocalizeDelta { get { return LocalizationProvider.GetString("Delta"); } }
        //public static string LocalizeCumulato { get { return LocalizationProvider.GetString("Cumulato"); } }
        //public static string LocalizeProduttivitaH { get { return LocalizationProvider.GetString("ProduttivitaH"); } }
        public static string ConstSAL { get { return "SAL"; } }
        public static string ConstProg { get { return "Prog"; } }
        public static string ConstSched { get { return "Sched"; } }

        public static string GanttDataSheetBaseName { get => LocalizationProvider.GetString("ProduttivitaGantt"); }
        public static string GanttSALDataSheetBaseName { get => LocalizationProvider.GetString("ProgrammazioneSALData"); }
        public static string GanttProgSALDataSheetBaseName { get => LocalizationProvider.GetString("SAL"); }
        public static string GanttSchedDataSheetBaseName { get => LocalizationProvider.GetString("ProgrammaLavori"); }
    }
}
