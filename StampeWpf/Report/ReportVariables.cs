using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StampeWpf
{
    public class RiportiVariables
    {
        // Riporto
        public static string PrimaColonnaRiportoString { get { return "PrimaColonnaRiporto"; } }
        public static string SecondaColonnaRiportoString { get { return "SecondaColonnaRiporto"; } }
        public static string TerzaColonnaRiportoString { get { return "TerzaColonnaRiporto"; } }
        public static string QuartaColonnaRiportoString { get { return "QuartaColonnaRiporto"; } }
        public static string QuintaColonnaRiportoString { get { return "QuintaColonnaRiporto"; } }
        public static string SestaColonnaRiportoString { get { return "SestaColonnaRiporto"; } }
        public static string SettimaColonnaRiportoString { get { return "SettimaColonnaRiporto"; } }
        public static string OttavaColonnaRiportoString { get { return "OttavaColonnaRiporto"; } }
        public static string NonaColonnaRiportoString { get { return "NonaColonnaRiporto"; } }
        public static string DecimaColonnaRiportoString { get { return "DecimaColonnaRiporto"; } }
        public static string UndicesimaColonnaRiportoString { get { return "UndicesimaColonnaRiporto"; } }
        public static string DodicesimaColonnaRiportoString { get { return "DodicesimaColonnaRiporto"; } }
        public static string TredicesimaColonnaRiportoString { get { return "TredicesimaColonnaRiporto"; } }
        public static string QuattordicesimaColonnaRiportoString { get { return "QattordicesimaColonnaRiporto"; } }
        public static string QuindicesimaColonnaRiportoString { get { return "QuindicesimaColonnaRiporto"; } }
        public static string SedicesimaColonnaRiportoString { get { return "SedicesimaColonnaRiporto"; } }
        public static string DiciassettesimaColonnaRiportoString { get { return "DiciassettesimaColonnaRiporto"; } }
        public static string DiciottesimaColonnaRiportoString { get { return "DiciottesimaColonnaRiporto"; } }
        public static string DiciannovesimaColonnaRiportoString { get { return "DiciannovesimaColonnaRiporto"; } }
        public static string VentesimaColonnaRiportoString { get { return "VentesimaColonnaRiporto"; } }

        //Riportare
        public static string PrimaColonnaRiportareString { get { return "PrimaColonnaRiportare"; } }
        public static string SecondaColonnaRiportareString { get { return "SecondaColonnaRiportare"; } }
        public static string TerzaColonnaRiportareString { get { return "TerzaColonnaRiportare"; } }
        public static string QuartaColonnaRiportareString { get { return "QuartaColonnaRiportare"; } }
        public static string QuintaColonnaRiportareString { get { return "QuintaColonnaRiportare"; } }
        public static string SestaColonnaRiportareString { get { return "SestaColonnaRiportare"; } }
        public static string SettimaColonnaRiportareString { get { return "SettimaColonnaRiportare"; } }
        public static string OttavaColonnaRiportareString { get { return "OttavaColonnaRiportare"; } }
        public static string NonaColonnaRiportareString { get { return "NonaColonnaRiportare"; } }
        public static string DecimaColonnaRiportareString { get { return "DecimaColonnaRiportare"; } }
        public static string UndicesimaColonnaRiportareString { get { return "UndicesimaColonnaRiportare"; } }
        public static string DodicesimaColonnaRiportareString { get { return "DodicesimaColonnaRiportare"; } }
        public static string TredicesimaColonnaRiportareString { get { return "TredicesimaColonnaRiportare"; } }
        public static string QuattordicesimaColonnaRiportareString { get { return "QattordicesimaColonnaRiportare"; } }
        public static string QuindicesimaColonnaRiportareString { get { return "QuindicesimaColonnaRiportare"; } }
        public static string SedicesimaColonnaRiportareString { get { return "SedicesimaColonnaRiportare"; } }
        public static string DiciassettesimaColonnaRiportareString { get { return "DiciassettesimaColonnaRiportare"; } }
        public static string DiciottesimaColonnaRiportareString { get { return "DiciottesimaColonnaRiportare"; } }
        public static string DiciannovesimaColonnaRiportareString { get { return "DiciannovesimaColonnaRiportare"; } }
        public static string VentesimaColonnaRiportareString { get { return "VentesimaColonnaRiportare"; } }

    }

    public class RiportoTotalProperties
    {
        public int ColumnIndex { get; set; }
        public string TotalName { get; set; }
        public string TextObjectTotalName { get; set; }
        public string EntityAttributo { get; set; }
        public string BandName { get; set; }
        public int BandIndex { get; set; }
    }
    public class ReportParameter
    {
        public int ColumnIndex { get; set; }
        public double RiportoValue { get; set; }
        public double RiportareValue { get; set; }
        public bool IsStampaRiportoRiportareEnable { get; set; } = false;

        public ReportParameter(int columnIndex)
        {
            ColumnIndex = columnIndex;
        }

        public string RetrieveParameterString(bool IsRiporto)
        {
            string parameterstring = null;

            if (ColumnIndex == 0)
            {
                if (IsRiporto)
                {
                    return RiportiVariables.PrimaColonnaRiportoString;
                }
                else
                {
                    return RiportiVariables.PrimaColonnaRiportareString;
                }
            }
            if (ColumnIndex == 1)
            {
                if (IsRiporto)
                {
                    return RiportiVariables.SecondaColonnaRiportoString;
                }
                else
                {
                    return RiportiVariables.SecondaColonnaRiportareString;
                }
            }
            if (ColumnIndex == 2)
            {
                if (IsRiporto)
                {
                    return RiportiVariables.TerzaColonnaRiportoString;
                }
                else
                {
                    return RiportiVariables.TerzaColonnaRiportareString;
                }
            }
            if (ColumnIndex == 3)
            {
                if (IsRiporto)
                {
                    return RiportiVariables.QuartaColonnaRiportoString;
                }
                else
                {
                    return RiportiVariables.QuartaColonnaRiportareString;
                }
            }
            if (ColumnIndex == 4)
            {
                if (IsRiporto)
                {
                    return RiportiVariables.QuintaColonnaRiportoString;
                }
                else
                {
                    return RiportiVariables.QuintaColonnaRiportareString;
                }
            }
            if (ColumnIndex == 5)
            {
                if (IsRiporto)
                {
                    return RiportiVariables.SestaColonnaRiportoString;
                }
                else
                {
                    return RiportiVariables.SestaColonnaRiportareString;
                }
            }
            if (ColumnIndex == 6)
            {
                if (IsRiporto)
                {
                    return RiportiVariables.SettimaColonnaRiportoString;
                }
                else
                {
                    return RiportiVariables.SettimaColonnaRiportareString;
                }
            }
            if (ColumnIndex == 7)
            {
                if (IsRiporto)
                {
                    return RiportiVariables.OttavaColonnaRiportoString;
                }
                else
                {
                    return RiportiVariables.OttavaColonnaRiportareString;
                }
            }
            if (ColumnIndex == 8)
            {
                if (IsRiporto)
                {
                    return RiportiVariables.NonaColonnaRiportoString;
                }
                else
                {
                    return RiportiVariables.NonaColonnaRiportareString;
                }
            }
            if (ColumnIndex == 9)
            {
                if (IsRiporto)
                {
                    return RiportiVariables.DecimaColonnaRiportoString;
                }
                else
                {
                    return RiportiVariables.DecimaColonnaRiportareString;
                }
            }
            if (ColumnIndex == 10)
            {
                if (IsRiporto)
                {
                    return RiportiVariables.UndicesimaColonnaRiportoString;
                }
                else
                {
                    return RiportiVariables.UndicesimaColonnaRiportareString;
                }
            }
            if (ColumnIndex == 11)
            {
                if (IsRiporto)
                {
                    return RiportiVariables.DodicesimaColonnaRiportoString;
                }
                else
                {
                    return RiportiVariables.DodicesimaColonnaRiportareString;
                }
            }
            if (ColumnIndex == 12)
            {
                if (IsRiporto)
                {
                    return RiportiVariables.TredicesimaColonnaRiportoString;
                }
                else
                {
                    return RiportiVariables.TredicesimaColonnaRiportareString;
                }
            }
            if (ColumnIndex == 13)
            {
                if (IsRiporto)
                {
                    return RiportiVariables.QuattordicesimaColonnaRiportoString;
                }
                else
                {
                    return RiportiVariables.QuattordicesimaColonnaRiportareString;
                }
            }
            if (ColumnIndex == 14)
            {
                if (IsRiporto)
                {
                    return RiportiVariables.QuindicesimaColonnaRiportoString;
                }
                else
                {
                    return RiportiVariables.QuindicesimaColonnaRiportareString;
                }
            }
            if (ColumnIndex == 15)
            {
                if (IsRiporto)
                {
                    return RiportiVariables.SedicesimaColonnaRiportoString;
                }
                else
                {
                    return RiportiVariables.SedicesimaColonnaRiportareString;
                }
            }
            if (ColumnIndex == 16)
            {
                if (IsRiporto)
                {
                    return RiportiVariables.DiciassettesimaColonnaRiportoString;
                }
                else
                {
                    return RiportiVariables.DiciassettesimaColonnaRiportareString;
                }
            }
            if (ColumnIndex == 17)
            {
                if (IsRiporto)
                {
                    return RiportiVariables.DiciottesimaColonnaRiportoString;
                }
                else
                {
                    return RiportiVariables.DiciottesimaColonnaRiportareString;
                }
            }
            if (ColumnIndex == 18)
            {
                if (IsRiporto)
                {
                    return RiportiVariables.DiciannovesimaColonnaRiportoString;
                }
                else
                {
                    return RiportiVariables.DiciannovesimaColonnaRiportareString;
                }
            }
            if (ColumnIndex == 19)
            {
                if (IsRiporto)
                {
                    return RiportiVariables.VentesimaColonnaRiportoString;
                }
                else
                {
                    return RiportiVariables.VentesimaColonnaRiportareString;
                }
            }

            return parameterstring;
        }

    }
}
