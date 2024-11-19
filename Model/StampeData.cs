using CommonResources;
using Commons;
using MasterDetailModel;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Model
{
    //I dati solo di tipi base (string, int, double, decimal, bool, etc..) e List<>

//    public class StampeTemp
//    {
//#if DEBUG
//        //è stata implementata per la lentezza nel caso di tanti raggruppamenti nella stampa ed errata costuzione della db di stampa
//        public static bool IsNewStampaAle { get => true; }
//#else
//        public static bool IsNewStampaAle { get => false; }
//#endif
//    }


    //[ProtoContract]
    public class StampeData
    {
        public List<string> AttributiDaEstrarreInPerDatasource { get; set; }
        public List<AttributiUtilizzati> AttributiDaEstrarrePerDatasource { get; set; }
        public OrdinamentoCorpo OrdinamentoCorpo { get; set; }
        public bool IsNew { get; set; }
        public List<Raggruppamento> RaggruppamentiDatasource { get; set; }
        public List<CellaTabellaBase> OrdinamentiDatasource { get; set; }
        public List<Intestazione> IntestazioniDocumento { get; set; }
        public List<Intestazione> PiePaginaDocumento { get; set; }
        public List<Intestazione> Intestazioni { get; set; }
        public List<Raggruppamenti> Teste { get; set; }
        public List<Raggruppamenti> Code { get; set; }
        public List<CorpiDocumento> CorpiDocumento { get; set; }
        public bool IsBandDocumentoEmpty
        {
            get
            {
                bool IsEmpty = true;
                if (CorpiDocumento != null)
                {
                    foreach (CorpiDocumento Colonna in CorpiDocumento)
                    {
                        foreach (CorpoDocumento Cella in Colonna.CorpoColonna)
                        {
                            if (Cella.Nascondi == false && !string.IsNullOrEmpty(Cella.Attributo))
                            {
                                IsEmpty = false;
                            }
                        }
                    }
                }
                return IsEmpty;
            }
        }

        public int PositionVerticalTreeDocumento
        {
            get
            {
                //non si capisce cosa serve PositionVerticalTreeDocumento.
                //Il fatto sta che se non è -1 i padri vengono stampati più volte

                return -1;
                //return GetPostionOfVerticalTree(CorpiDocumento);//Rem by Ale 26/06/2023
            }
        }

        private int GetPostionOfVerticalTree(List<CorpiDocumento> CorpiDocumento)
        {
            int _PositionVerticalTree = -1;
            if (CorpiDocumento == null)
                return 0;
            int ContatoreRiga = CorpiDocumento.FirstOrDefault().CorpoColonna.Count();
            int ContatoreColonna = CorpiDocumento.Count();

            int ContatoreConsecutivita = 0;

            for (int R = 0; R < ContatoreRiga; R++)
            {
                if (R > 0)
                    continue;

                for (int C = 0; C < ContatoreColonna; C++)
                {
                    if (CorpiDocumento[C].CorpoColonna.FirstOrDefault().AttributoCodice == BuiltInCodes.Attributo.Codice ||
                    CorpiDocumento[C].CorpoColonna.FirstOrDefault().AttributoCodice == BuiltInCodes.Attributo.DescrizioneRTF ||
                    CorpiDocumento[C].CorpoColonna.FirstOrDefault().AttributoCodice == BuiltInCodes.Attributo.Nome)
                        ContatoreConsecutivita++;
                    else
                        ContatoreConsecutivita = 0;
                    if (ContatoreConsecutivita == 2)
                        return _PositionVerticalTree;
                }
                ContatoreRiga++;
            }

            ContatoreRiga = 0;
            ContatoreColonna = 0;
            if (ContatoreConsecutivita != 2)
                _PositionVerticalTree = -1;

            if (CorpiDocumento != null)
            {
                foreach (CorpiDocumento Colonna in CorpiDocumento)
                {
                    foreach (CorpoDocumento Cella in Colonna.CorpoColonna)
                    {
                        if (ContatoreRiga == 0)
                            if (Cella.AttributoCodice == BuiltInCodes.Attributo.Codice || Cella.AttributoCodice == BuiltInCodes.Attributo.DescrizioneRTF || Cella.AttributoCodice == BuiltInCodes.Attributo.Nome)
                                _PositionVerticalTree = ContatoreColonna;

                        if (ContatoreRiga == 1)
                            if (Cella.AttributoCodice == BuiltInCodes.Attributo.Codice || Cella.AttributoCodice == BuiltInCodes.Attributo.DescrizioneRTF || Cella.AttributoCodice == BuiltInCodes.Attributo.Nome)
                                return _PositionVerticalTree;

                        if (ContatoreRiga == 1)
                            if (Cella.AttributoCodice != BuiltInCodes.Attributo.Codice || Cella.AttributoCodice != BuiltInCodes.Attributo.DescrizioneRTF || Cella.AttributoCodice != BuiltInCodes.Attributo.Nome)
                                _PositionVerticalTree = -1;

                        ContatoreRiga++;
                    }
                    ContatoreColonna++;
                    ContatoreRiga = 0;
                }
            }
            return _PositionVerticalTree;

        }
        public List<CorpiDocumento> FineDocumento { get; set; }
        public bool IsBandFineDocumentoEmpty
        {
            get
            {
                bool IsEmpty = true;
                if (CorpiDocumento != null)
                {
                    foreach (CorpiDocumento Colonna in FineDocumento)
                    {
                        foreach (CorpoDocumento Cella in Colonna.CorpoColonna)
                        {
                            if (Cella.Nascondi == false && !string.IsNullOrEmpty(Cella.Attributo))
                            {
                                IsEmpty = false;
                            }
                        }
                    }
                }
                return IsEmpty;
            }
        }
        public string Codice { get; set; }
        public string TipologiaReport { get; set; }
        public int GuidSezione { get; set; }
        public string Sezione { get; set; }
        public string SezioneKey { get; set; }
        public string DescrizioneReport { get; set; }
        public string Orientamento { get; set; }
        public string NumeroColonne { get; set; }
        public float AltezzaPagina { get; set; }
        public float LarghezzaPagina { get; set; }
        public float MargineSuperiore { get; set; }
        public float MargineInferiore { get; set; }
        public float MargineSinistro { get; set; }
        public float MargineDestro { get; set; }
        public int IniziaDaPag { get; set; }
        public int ContatorePaginaAggiunte { get; set; }
        public string Intestazione { get; set; }
        public string Firme { get; set; }
        public string Corpo { get; set; }
        public string PiePagina { get; set; }
        public bool IsIntestazioneEmpty { get; set; }
        public bool IsFirmeEmpty { get; set; }
        public bool IsCorpoEmpty { get; set; }
        public bool IsPiePaginaEmpty { get; set; }
        public bool IsAllFieldRtfFormat { get; set; }
        public bool IsTreeMaster { get; set; }
        //public List<string> ImageForPage { get; set; }
        public List<ImageForPage> ImagesForPage { get; set; }
        public GanttSetting GanttSetting { get; set; }
        public FoglioDiCalcoloSetting FoglioDiCalcoloSetting { get; set; }


        public object this[string propertyName]
        {
            get { return this.GetType().GetProperty(propertyName).GetValue(this, null); }
        }
        public void AttributiDaEstrarrePerDataSource(string PropertyType, string Attributo, string EntityType, string DivisioneCodice, bool IsRtf, bool DescrfBreve, bool StampaFormula, string AttributoOrigine = null, string AttributoCodicePath = null)
        {
            if (!String.IsNullOrEmpty(Attributo) && !String.IsNullOrEmpty(EntityType))
            {
                if (!string.IsNullOrEmpty(Attributo) || Attributo != "Somma" || Attributo != "Conta")
                {
                    if (AttributiDaEstrarrePerDatasource == null)
                    {
                        AttributiDaEstrarrePerDatasource = new List<AttributiUtilizzati>();
                    }

                    if (AttributiDaEstrarrePerDatasource.FirstOrDefault(a => a.EntityType == EntityType) == null)
                    {
                        AttributiDaEstrarrePerDatasource.Add(new AttributiUtilizzati()
                        {
                            EntityType = EntityType,
                            DivisioneCodice = DivisioneCodice,
                            AttributiPerEntityType = new List<string>(),
                            AttributiOriginePerEntityType = new List<string>(),
                            PropertyType = new List<string>(),
                            Rtf = new List<bool>(),
                            DescrfBreve = new List<bool>(),
                            StampaFormula = new List<bool>(),
                            AttributiCodicePathPerEntityType = new List<string>()
                        });
                    }

                    //var varAttributiGiaInseriti = AttributiDaEstrarrePerDatasource.Where(a => a.EntityType == EntityType).FirstOrDefault().AttributiPerEntityType;
                    var varAttributiGiaInseriti = AttributiDaEstrarrePerDatasource.FirstOrDefault(a => a.EntityType == EntityType).AttributiCodicePathPerEntityType;

                    if (varAttributiGiaInseriti.Where(a => a == Attributo).FirstOrDefault() == null || PropertyType == BuiltInCodes.DefinizioneAttributo.Reale || PropertyType == BuiltInCodes.DefinizioneAttributo.Contabilita)
                    {
                        AttributiDaEstrarrePerDatasource.Where(a => a.EntityType == EntityType).FirstOrDefault().AttributiPerEntityType.Add(Attributo);
                        AttributiDaEstrarrePerDatasource.Where(a => a.EntityType == EntityType).FirstOrDefault().AttributiOriginePerEntityType.Add(AttributoOrigine);
                        AttributiDaEstrarrePerDatasource.Where(a => a.EntityType == EntityType).FirstOrDefault().AttributiCodicePathPerEntityType.Add(AttributoCodicePath);
                        AttributiDaEstrarrePerDatasource.Where(a => a.EntityType == EntityType).FirstOrDefault().Rtf.Add(IsRtf);
                        AttributiDaEstrarrePerDatasource.Where(a => a.EntityType == EntityType).FirstOrDefault().DescrfBreve.Add(DescrfBreve);
                        AttributiDaEstrarrePerDatasource.Where(a => a.EntityType == EntityType).FirstOrDefault().StampaFormula.Add(StampaFormula);
                        AttributiDaEstrarrePerDatasource.Where(a => a.EntityType == EntityType).FirstOrDefault().PropertyType.Add(PropertyType);
                    }
                }
            }
        }
    }

    public class Intestazione
    {
        public string Etichetta { get; set; }
        public string Attributo { get; set; }
        public decimal Size { get; set; }
        public ProprietaCarattere StileCarattere { get; set; }
    }

    public class Raggruppamento : CellaTabellaBase
    {
        public decimal Indent { get; set; }
        public bool IsCheckedRiepilogo { get; set; }
        public bool IsCheckedTotale { get; set; }
        public bool IsCheckedNuovapagina { get; set; }
        public bool IsCheckedDescrBreve { get; set; }

        public OpzioniDiStampa OpzioniDiStampa { get; set; }
        public bool IsOrdineCrescente { get; set; }
        public CellaTabellaBase EntitaAttributoOrdinamento { get; set; }
        public bool IsOrdineDecrescente { get; set; }
    }

    public class CellaTabellaBase
    {
        public string Attributo { get; set; }
        public string AttributoCodice
        {
            get;
            set;
        }
        public string AttributoCodiceOrigine { get; set; }
        public string AttributoCodicePath { get; set; }
        public string EntityType { get; set; }
        public string PropertyType { get; set; }
        public string DivisioneCodice { get; set; }


        public string AttributoPerReport
        {
            get
            {
                if (DeveloperVariables.IsNewStampa)
                {
                    if (string.IsNullOrEmpty(EntityType) && string.IsNullOrEmpty(AttributoCodicePath))
                    {
                        return null;
                    }
                    else
                    {
                        return EntityType?.Replace(" ", "_") + AttributoCodicePath?.Replace(" ", "_");
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(EntityType) && string.IsNullOrEmpty(AttributoCodice))
                    {
                        return null;
                    }
                    else
                    {
                        return EntityType?.Replace(" ", "_") + AttributoCodice?.Replace(" ", "_");
                    }
                }
            }
        }

        //public string AttributoPerReportOld
        //{
        //    get
        //    {
        //        if (string.IsNullOrEmpty(EntityType) && string.IsNullOrEmpty(AttributoCodice))
        //        {
        //            return null;
        //        }
        //        else
        //        {
        //            return EntityType?.Replace(" ", "_") + AttributoCodice?.Replace(" ", "_");
        //        }
        //    }
        //}

    }


    public class OpzioniDiStampa
    {
        public bool IsCheckedNuovaPagina { get; set; }
        public bool IsCheckedDescrizioneBreve { get; set; }
        public bool IsCheckedRiepilogo { get; set; }
    }

    public class Raggruppamenti
    {
        public string Attributo { get; set; }
        public string EntityType { get; set; }

        public bool IsBandRaggrupamentoEmpty
        {
            get
            {
                bool IsEmpty = true;
                if (RaggruppamentiDocumento != null)
                {
                    foreach (RaggruppamentiDocumento Colonna in RaggruppamentiDocumento)
                    {
                        foreach (RaggruppamentiValori Cella in Colonna.RaggruppamentiValori)
                        {
                            if (!string.IsNullOrEmpty(Cella.Attributo))
                            {
                                IsEmpty = false;
                            }
                        }
                    }
                }
                return IsEmpty;
            }
        }

        public int PositionVerticalTreeRaggruppamento
        {
            get
            {
                //non si capisce cosa serve PositionVerticalTreeRaggruppamento.
                //Il fatto sta che se non è -1 i padri vengono stampati più volte

                return -1;//AU
                //return GetPostionOfVerticalTree(RaggruppamentiDocumento);//Rem by Ale 26/06/2023
            }
        }

        private int GetPostionOfVerticalTree(List<RaggruppamentiDocumento> raggruppamentiDocumento)
        {
            int _PositionVerticalTree = -1;
            int ContatoreRiga = RaggruppamentiDocumento.FirstOrDefault().RaggruppamentiValori.Count();
            int ContatoreColonna = RaggruppamentiDocumento.Count();

            int ContatoreConsecutivita = 0;

            for (int R = 0; R < ContatoreRiga; R++)
            {
                if (R > 0)
                    continue;

                for (int C = 0; C < ContatoreColonna; C++)
                {
                    if (RaggruppamentiDocumento[C].RaggruppamentiValori.FirstOrDefault().AttributoCodice == BuiltInCodes.Attributo.Codice ||
                    RaggruppamentiDocumento[C].RaggruppamentiValori.FirstOrDefault().AttributoCodice == BuiltInCodes.Attributo.DescrizioneRTF ||
                    RaggruppamentiDocumento[C].RaggruppamentiValori.FirstOrDefault().AttributoCodice == BuiltInCodes.Attributo.Nome)
                        ContatoreConsecutivita++;
                    else
                        ContatoreConsecutivita = 0;
                    if (ContatoreConsecutivita == 2)
                        return _PositionVerticalTree;
                }
                ContatoreRiga++;
            }

            ContatoreRiga = 0;
            ContatoreColonna = 0;
            if (ContatoreConsecutivita != 2)
                _PositionVerticalTree = -1;

            if (RaggruppamentiDocumento != null)
            {
                foreach (RaggruppamentiDocumento Colonna in RaggruppamentiDocumento)
                {
                    foreach (RaggruppamentiValori Cella in Colonna.RaggruppamentiValori)
                    {
                        if (ContatoreRiga == 0)
                            if (Cella.AttributoCodice == BuiltInCodes.Attributo.Codice || Cella.AttributoCodice == BuiltInCodes.Attributo.DescrizioneRTF || Cella.AttributoCodice == BuiltInCodes.Attributo.Nome)
                                _PositionVerticalTree = ContatoreColonna;

                        if (ContatoreRiga == 1)
                            if (Cella.AttributoCodice == BuiltInCodes.Attributo.Codice || Cella.AttributoCodice == BuiltInCodes.Attributo.DescrizioneRTF || Cella.AttributoCodice == BuiltInCodes.Attributo.Nome)
                                return _PositionVerticalTree;

                        if (ContatoreRiga == 1)
                            if (Cella.AttributoCodice != BuiltInCodes.Attributo.Codice || Cella.AttributoCodice != BuiltInCodes.Attributo.DescrizioneRTF || Cella.AttributoCodice != BuiltInCodes.Attributo.Nome)
                                _PositionVerticalTree = -1;

                        ContatoreRiga++;
                    }
                    ContatoreColonna++;
                    ContatoreRiga = 0;
                }
            }
            return _PositionVerticalTree;

        }

        public List<RaggruppamentiDocumento> RaggruppamentiDocumento { get; set; }
    }

    public class RaggruppamentiDocumento
    {
        public decimal Size { get; set; }
        public List<RaggruppamentiValori> RaggruppamentiValori { get; set; }
    }

    public class RaggruppamentiValori : CellaTabella
    {

    }

    public class CorpiDocumento
    {
        public decimal Size { get; set; }
        public List<CorpoDocumento> CorpoColonna { get; set; }
    }
    public class CorpoDocumento : CellaTabella
    {

    }

    public class CellaTabella : CellaTabellaBase
    {
        public string Etichetta { get; set; }
        public ProprietaCarattere StileCarattere { get; set; }
        public bool Rtf { get; set; }
        public bool DescrBreve { get; set; }
        public bool StampaFormula { get; set; }
        public bool ConcatenaEtichettaEValore { get; set; }
        public string CodiceDigicorp { get; set; }
        public bool Nascondi { get; set; }
        public bool RiportoPagina { get; set; }
        public bool RiportoRaggruppamento { get; set; }
        public Visibility IsNascondiVisible { get; set; }
    }

    public class ProprietaCarattere
    {
        public ColorInfo ColorCharacther { get; set; }
        public ColorInfo ColorBackground { get; set; }
        public string Size { get; set; }
        public bool IsGrassetto { get; set; }
        public bool IsCorsivo { get; set; }
        public bool IsBarrato { get; set; }
        public bool IsSottolineato { get; set; }
        public string FontFamily { get; set; }
        public string Stile { get; set; }
        public string TextAlignement { get; set; }
        public string TextVerticalAlignement { get; set; }
        public int TextAlignementCode { get; set; }
        public int TextVerticalAlignementCode { get; set; }
    }

    public class AttributiUtilizzati
    {
        public string EntityType { get; set; }
        public string DivisioneCodice { get; set; }
        public List<string> AttributiPerEntityType { get; set; }
        public List<string> AttributiOriginePerEntityType { get; set; }
        public List<string> PropertyType { get; set; }
        public List<bool> Rtf { get; set; }
        public List<bool> DescrfBreve { get; set; }
        public List<bool> StampaFormula { get; set; }
        public List<string> AttributiCodicePathPerEntityType { get; set; }
    }
    public class OrdinamentoCorpo : CellaTabellaBase
    {
        public bool IsOrdinamentoCrescente { get; set; }
        public bool IsOrdinamentoDecrescente { get; set; }
    }

    public class GanttSetting
    {
        public DateTime GanttDateFrom { get; set; }
        public DateTime GanttDateTo { get; set; }
        public TimeSpan GanttZoom { get; set; }
        public GuidToOpenOrClose GuidToOpenOrClose { get; set; }
        public List<int> ColumnWidth { get; set; }
        public int AdjustToPage { get; set; }
        public double ZoomFactor { get; set; }
    }
    public class FoglioDiCalcoloSetting
    {
        public List<string> SheetNameToPrint { get; set; }
        public int FitToPageKey { get; set; } = -1;
    }

    public struct ImageForPage
    {
        public string Image { get; set; }
        public SizeF Size { get; set; }
    }
}
