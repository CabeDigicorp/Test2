using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttivitaWpf
{
    public class CustomProviderTimeScale : IFormatProvider, ICustomFormatter
    {
        string Formato;
        int Key;
        int RulerType;
        public CustomProviderTimeScale(string formato, int key, int rulerType)
        {
            Formato = formato;
            Key = key;
            RulerType = rulerType;
        }
        public object GetFormat(Type formatType)
        {
            if (typeof(ICustomFormatter) == formatType)
                return this;
            else
                return null;
        }
        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (RulerType == 0)
            {
                var range = arg as DateTimeRange?;
                if (Key == 5)
                {
                    //args.CurrentCell.CellToolTip = args.CurrentCell.CellDate.ToString(Formato) + "-" + args.CurrentCell.CellDate.AddDays(7).ToString(Formato);
                    //return range.HasValue ? range.Value.Start.ToString(Formato) + "-" + range.Value.End.ToString(Formato) : null;
                    return range.HasValue ? range.Value.Start.ToString(Formato) : null;
                }
                else
                {
                    //args.CurrentCell.CellToolTip = args.CurrentCell.CellDate.ToString(Formato);
                    try
                    {
                        return range.HasValue ? range.Value.Start.ToString(Formato) : null;
                    }
                    catch (Exception)
                    {
                        return "";
                    }
                }
            }
            else
            {
                var range = arg as DateTimeRange?;
                //if (String.IsNullOrEmpty(Formato))
                //Formato = "N0";

                if (RulerType == 1)
                {
                    if (GanttSource.ScalaNumericaAnonima.ContainsKey(range.Value.Start))
                        if (GanttSource.ScalaNumericaAnonima.ContainsKey(range.Value.Start))
                            switch (Key)
                            {
                                case 0:
                                    return GanttSource.ScalaNumericaAnonima[range.Value.Start].ProgressivoNumericoAnnoAnonima.ToString(Formato);
                                    break;
                                case 1:
                                    // half year
                                    //return GanttSource.ScalaNumericaAnonima2024[range.Value.Start].ProgressivoNumericoMinutoAnonima.ToString(Formato);
                                    break;
                                case 2:
                                    //Quarter
                                    //return GanttSource.ScalaNumericaAnonima2024[range.Value.Start].ProgressivoNumericoMinutoAnonima.ToString(Formato);
                                    break;
                                case 3:
                                    return GanttSource.ScalaNumericaAnonima[range.Value.Start].ProgressivoNumericoMeseAnonima.ToString(Formato);
                                    break;
                                case 4:
                                    return GanttSource.ScalaNumericaAnonima[range.Value.Start].ProgressivoNumericoSettimanaAnonima.ToString(Formato);
                                    //DECADI
                                    break;
                                case 5:
                                    return GanttSource.ScalaNumericaAnonima[range.Value.Start].ProgressivoNumericoSettimanaAnonima.ToString(Formato);
                                    break;
                                case 6:
                                    return GanttSource.ScalaNumericaAnonima[range.Value.Start].ProgressivoNumericoGiornoAnonima.ToString(Formato);
                                    break;
                                case 7:
                                    return GanttSource.ScalaNumericaAnonima[range.Value.Start].ProgressivoNumericoOraAnonima.ToString(Formato);
                                    break;
                                case 8:
                                    return GanttSource.ScalaNumericaAnonima[range.Value.Start].ProgressivoNumericoMinutoAnonima.ToString(Formato);
                                    break;
                                default:
                                    break;
                            }
                        else
                            return "";
                }

                if (RulerType == 2)
                {
                    if (GanttSource.ScalaNumericaFeriale.ContainsKey(range.Value.Start))
                        switch (Key)
                        {
                            case 0:
                                return GanttSource.ScalaNumericaFeriale[range.Value.Start].ProgressivoNumericoAnno.ToString(Formato);
                                break;
                            case 1:
                                // half year
                                //return GanttSource.ScalaNumericaFeriale[range.Value.Start].ProgressivoNumericoMinuto.ToString(Formato);
                                break;
                            case 2:
                                //Quarter
                                //return GanttSource.ScalaNumericaFeriale[range.Value.Start].ProgressivoNumericoMinuto.ToString(Formato);
                                break;
                            case 3:
                                return GanttSource.ScalaNumericaFeriale[range.Value.Start].ProgressivoNumericoMese.ToString(Formato);
                                break;
                            case 4:
                                return GanttSource.ScalaNumericaFeriale[range.Value.Start].ProgressivoNumericoSettimana.ToString(Formato);
                                //DECADI
                                break;
                            case 5:
                                return GanttSource.ScalaNumericaFeriale[range.Value.Start].ProgressivoNumericoSettimana.ToString(Formato);
                                break;
                            case 6:
                                return GanttSource.ScalaNumericaFeriale[range.Value.Start].ProgressivoNumericoGiorno.ToString(Formato);
                                break;
                            case 7:
                                return GanttSource.ScalaNumericaFeriale[range.Value.Start].ProgressivoNumericoOra.ToString(Formato);
                                break;
                            case 8:
                                return GanttSource.ScalaNumericaFeriale[range.Value.Start].ProgressivoNumericoMinuto.ToString(Formato);
                                break;
                            default:
                                break;
                        }
                    else
                        return "";
                }

                return "";
            }
        }
    }
}
