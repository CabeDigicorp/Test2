using CommonResources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttivitaWpf
{
    public class GanttKeys
    {
        public static string LocalizeMenuScollega { get { return LocalizationProvider.GetString("Scollega"); } }
        public static string LocalizeMenuDeseleziona { get { return LocalizationProvider.GetString("Deseleziona"); } }
        public static string LocalizeMenuDeselezionaTutti { get { return LocalizationProvider.GetString("Deseleziona tutti"); } }
        public static string LocalizeMenuUnLivello{ get { return LocalizationProvider.GetString("Un livello (inferiore)"); } }
        public static string LocalizeMenuDueLivelli { get { return LocalizationProvider.GetString("Due livelli (intermedio, inferiore)"); } }
        public static string LocalizeMenuTreLivelli { get { return LocalizationProvider.GetString("Tre livelli (superiore, intermedio, inferiore)"); } }
        public static string LocalizeSinistra { get { return LocalizationProvider.GetString("Sinistra"); } }
        public static string LocalizeCentro { get { return LocalizationProvider.GetString("Centro"); } }
        public static string LocalizeDestra { get { return LocalizationProvider.GetString("Destra"); } }
        public static string LocalizeAnni { get { return LocalizationProvider.GetString("Anni"); } }
        public static string LocalizeSemestri { get { return LocalizationProvider.GetString("Semestri"); } }
        public static string LocalizeTrimestri { get { return LocalizationProvider.GetString("Trimestri"); } }
        public static string LocalizeMesi { get { return LocalizationProvider.GetString("Mesi"); } }
        public static string LocalizeDecadi { get { return LocalizationProvider.GetString("Decadi"); } }
        public static string LocalizeSettimane { get { return LocalizationProvider.GetString("Settimane"); } }
        public static string LocalizeGiorni { get { return LocalizationProvider.GetString("Giorni"); } }
        public static string LocalizeOre { get { return LocalizationProvider.GetString("Ore"); } }
        public static string LocalizeMinuti { get { return LocalizationProvider.GetString("Minuti"); } }
        public static string LocalizeDelta { get { return LocalizationProvider.GetString("Periodo"); } }
        public static string LocalizeCumulato { get { return LocalizationProvider.GetString("Progressivo"); } }
        public static string LocalizeProduttivitaH { get { return LocalizationProvider.GetString("ProduttivitaH"); } }
        public static string LocalizeValorePercentualeProgressiva { get { return " (%)"; } }
        public static string ColonnaAttributo { get { return "ColonnaAttributo"; } }
        public static string ConstSAL { get { return LocalizationProvider.GetString("Valore SAL"); } }
    }
}
