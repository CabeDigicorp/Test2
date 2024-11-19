using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JReport;
using FastReport;
using FastReport.Utils;
using Commons.View;
using System.Drawing;
using System.Runtime.Serialization;
using FastReport.Data;
using FastReport.Format;
using Microsoft.SqlServer.Server;
using System.Data;
using MasterDetailModel;
using FastReport.Editor;
using FastReport.DataVisualization.Charting;
using Commons;
using CommonResources;
using DevExpress.Utils.Text.Internal;
using Microsoft.VisualBasic.Logging;
using DevExpress.ClipboardSource.SpreadsheetML;
using DevExpress.Xpf.Printing.Native;
using DevExpress.DataAccess.Wizard.Native;

namespace StampeWpf.Report
{
    public class PageStructureGenerator
    {
        private Model.StampeData ReportSetting;

        private ClientDataService dataService;
        public ReportPage Page { get; }
        public SubreportObject Subreport { get; set; }
        private DataBand Data;

        private List<string> FormatoValuta = new List<string>() { MasterDetailModel.BuiltInCodes.DefinizioneAttributo.Contabilita };
        private List<string> FormatoNumero = new List<string>() { MasterDetailModel.BuiltInCodes.DefinizioneAttributo.Reale };

        
        //private float DefaultHeight = Units.Centimeters * 1 / 2;
        //private float DefaultHeightTextBox = Units.Centimeters * 1 / 2;
        //private float DefaultHeightBand = Units.Centimeters * 1;

        //Soluzione per non far uscire dei pixel bianchi nella linea verticale a destra della stampa Realzione CAM //by Ale 18/12/2023
        static float Scale { get; set; } = (float) 0.6;//(float)1.0;
        private float DefaultHeight = Units.Centimeters * (Scale / 2);
        private float DefaultHeightTextBox = Units.Centimeters * (Scale / 2);
        private float DefaultHeightBand = Units.Centimeters * Scale;

        //Scale riporti //AU 09/01/2023
        private float HeightTextBoxPageHeader = Units.Centimeters * ((float) 1.0 / 2);
        private float HeightTextBoxColumnFooter = Units.Centimeters * ((float)1.0 / 2);
        //

        private float MiddlePage;
        private float ThirdSpace;

        private Int64 ContatoreEtichetteSenzaAttributo = 1;
        private Int64 ContatoreTextObject = 1;
        private Int64 ContatoreRtfObject = 1;

        public JReport.Report ReportObject;

        public int IndexDb { get; set; }
        public string DbName { get; set; }
        public string Sezione { get; set; }

        private bool CompilazioneDatiTotalePadre;

        public List<ParenEntity> ListParentItem { get; set; }
        public bool IsTreeMaster { get; internal set; }
        private string PageNumberSyntax { get { return "PageNumber"; } }
        private string NumberPagesSyntax { get { return "NumberPages"; } }

        public bool Bordatura { get; internal set; }

        private EntitiesHelper entitiesHelper;
        private RtfEntityDataService RtfDataService;

        private float DimensionamentoRtf;
        private float TraslationXRtf;

        private List<CellaTabella> ListaCelleCorpo = new List<CellaTabella>();
        private List<CellaTabella> ListaCelleRappamento = new List<CellaTabella>();
        private List<RiportoTotalProperties> ListOggettiPerBanda;
        private List<ReportParameter> ReportParameters;

        public PageStructureGenerator(ClientDataService DataService, IMainOperation MainOperation, StampeData reportSetting, JReport.Report reportObject, long contatorePagina)
        {
            dataService = DataService;
            ListOggettiPerBanda = new List<RiportoTotalProperties>();
            ReportParameters = new List<ReportParameter>();
            for (int i = 0; i < 20; i++)
            {
                ReportParameter reportParameters = new ReportParameter(i);
                ReportParameters.Add(reportParameters);
            }
            ReportSetting = new StampeData();
            ReportSetting = reportSetting;
            Page = new ReportPage();
            ReportObject = reportObject;
            Page.Name = "Page" + contatorePagina.ToString();
            MiddlePage = Units.Centimeters * ((reportSetting.LarghezzaPagina - reportSetting.MargineSinistro - reportSetting.MargineDestro) / (2 * 10));
            ThirdSpace = Units.Centimeters * ((reportSetting.LarghezzaPagina - reportSetting.MargineSinistro - reportSetting.MargineDestro) / (3 * 10));
            entitiesHelper = new EntitiesHelper(dataService);
            RtfDataService = new RtfEntityDataService(MainOperation);
            DimensionamentoRtf = ThirdSpace * 3 - (2 * Units.Centimeters) / 64;
            TraslationXRtf = (1 * Units.Centimeters) / 64;

            if (!string.IsNullOrEmpty(ReportSetting.Intestazione))
            {
                if (ReportSetting.Intestazione.StartsWith("{\\rtf"))
                {
                    ReportSetting.Intestazione = entitiesHelper.GetRtfPreview(ReportSetting.Intestazione, RtfDataService);

                    //ReportSetting.Intestazione = ReportSetting.Intestazione.Replace(@"\u171?" + NumberPagesSyntax + @"\u187?", "[TotalPages#]");
                    //ReportSetting.Intestazione = ReportSetting.Intestazione.Replace(@"\u171?" + PageNumberSyntax + @"\u187?", "[Page#]");

                    //devExpress
                    ReportSetting.Intestazione = ReportSetting.Intestazione.Replace(@"\u171\'ab" + NumberPagesSyntax + @"\u187\'bb", "[TotalPages#]");
                    ReportSetting.Intestazione = ReportSetting.Intestazione.Replace(@"\u171\'ab" + PageNumberSyntax + @"\u187\'bb", "[Page#]");
                }
            }

            if (!string.IsNullOrEmpty(ReportSetting.PiePagina))
            {
                if (ReportSetting.PiePagina.StartsWith("{\\rtf"))
                {
                    ReportSetting.PiePagina = entitiesHelper.GetRtfPreview(ReportSetting.PiePagina, RtfDataService);

                    //ReportSetting.PiePagina = ReportSetting.PiePagina.Replace(@"\u171?" + NumberPagesSyntax + @"\u187?", "[TotalPages#]");
                    //ReportSetting.PiePagina = ReportSetting.PiePagina.Replace(@"\u171?" + PageNumberSyntax + @"\u187?", "[Page#]");

                    //devExpress
                    ReportSetting.PiePagina = ReportSetting.PiePagina.Replace(@"\u171\'ab" + NumberPagesSyntax + @"\u187\'bb", "[TotalPages#]");
                    ReportSetting.PiePagina = ReportSetting.PiePagina.Replace(@"\u171\'ab" + PageNumberSyntax + @"\u187\'bb", "[Page#]");
                }
            }

            bool Find = false;

            if (ReportSetting.CorpiDocumento != null)
            {
                foreach (var AttributiColonna in ReportSetting.CorpiDocumento)
                {
                    Find = false;
                    foreach (var Attributo in AttributiColonna.CorpoColonna)
                    {
                        if (Attributo.RiportoPagina || Attributo.RiportoRaggruppamento)
                        {
                            ListaCelleCorpo.Add(Attributo);
                            Find = true;
                            break;
                        }
                    }

                    if (Find == false)
                    {
                        ListaCelleCorpo.Add(new CellaTabella());
                    }
                }

                foreach (CellaTabella CellaCorpo in ListaCelleCorpo)
                {
                    if (ReportSetting.Code.Count() > 0)
                    {
                        foreach (RaggruppamentiDocumento Colonne in ReportSetting.Code.LastOrDefault().RaggruppamentiDocumento)
                        {
                            foreach (RaggruppamentiValori Riga in Colonne.RaggruppamentiValori)
                            {
                                if (Riga.AttributoCodice == CellaCorpo.Attributo && Riga.AttributoCodice == CellaCorpo.AttributoCodice &&
                                    Riga.AttributoPerReport == CellaCorpo.AttributoPerReport && Riga.EntityType == CellaCorpo.EntityType)
                                {
                                    ListaCelleRappamento.Add(Riga);
                                }
                            }
                        }
                    }
                }
            }

            InitializeScriptReportVariable(true);
        }

        public void CreateGanttPageStructure(ImageForPage Gantt, bool FromDocumento = false)
        {
            PageLayoutSetting();

            SetDefaultPageComponent();

            AddImageObjectToDataBand(Gantt);

            AddObjectToPageFooter(FromDocumento);

            AddObjectToPageHeader(FromDocumento, false);
        }
        public void CreateRtfPageStructure(bool FromDocumento = false)
        {
            PageLayoutSetting();

            SetDefaultPageComponent();

            AddRftObjectToDataBand();

            AddObjectToPageFooter(FromDocumento);

            CreateColumnFooter(FromDocumento, true);

            AddObjectToPageHeader(FromDocumento, false);
        }
        public void CreateDatiPageStructure(bool FromDocumento = false, bool isFirstReportPage = false)
        {
            if (ReportSetting.PositionVerticalTreeDocumento == -1)
                ModificaReportSettingSuSezioneSelezionata();

            PageLayoutSetting();

            SetDefaultPageComponent(FromDocumento);

            CreateGroupsBand();

            AddObjectToDataBand();

            AddObjectToPageFooter(FromDocumento);

            CreateColumnFooter(FromDocumento, false);

            AddObjectToPageHeader(FromDocumento, true);
        }
        private void ModificaReportSettingSuSezioneSelezionata()
        {
            VariabiliDiCicloBanda VDCB = new VariabiliDiCicloBanda(0, 0, 0, 0, 0, 0);
            bool creaRaggruppamentoPrimaRiga = false;
            if (IsTreeMaster)
            {
                foreach (var Colonna in ReportSetting.CorpiDocumento)
                {
                    VDCB.ContatoreRiga = 0;

                    if (VDCB.ContatoreRiga == 0)
                    {
                        foreach (var Cella in Colonna.CorpoColonna)
                        {
                            if (VDCB.ContatoreRiga == 0)
                            {
                                if (!creaRaggruppamentoPrimaRiga)
                                {
                                    ReportSetting.RaggruppamentiDatasource.Add(new Raggruppamento() { AttributoCodice = Sezione + StampeKeys.Const_Guid, EntityType = Sezione, Attributo = Sezione });
                                    ReportSetting.Teste.Add(new Raggruppamenti() { Attributo = Sezione });
                                    ReportSetting.Code.Add(new Raggruppamenti() { Attributo = Sezione });
                                    ReportSetting.Teste.Last().RaggruppamentiDocumento = new List<RaggruppamentiDocumento>();
                                    ReportSetting.Code.Last().RaggruppamentiDocumento = new List<RaggruppamentiDocumento>();
                                    creaRaggruppamentoPrimaRiga = true;
                                }

                                ReportSetting.Teste.Last().RaggruppamentiDocumento.Add(new RaggruppamentiDocumento());
                                ReportSetting.Teste.Last().RaggruppamentiDocumento.Last().Size = Colonna.Size;
                                ReportSetting.Teste.Last().RaggruppamentiDocumento.Last().RaggruppamentiValori = new List<RaggruppamentiValori>();

                                ReportSetting.Code.Last().RaggruppamentiDocumento.Add(new RaggruppamentiDocumento());
                                ReportSetting.Code.Last().RaggruppamentiDocumento.Last().Size = Colonna.Size;
                                ReportSetting.Code.Last().RaggruppamentiDocumento.Last().RaggruppamentiValori = new List<RaggruppamentiValori>();

                                //Rev by Ale 19/12/2023
                                //RaggruppamentiValori rag = new RaggruppamentiValori() { Attributo = Cella.Attributo, AttributoCodice = Cella.AttributoCodice, AttributoCodicePath = Cella.AttributoCodice, EntityType = Cella.EntityType, Etichetta = Cella.Etichetta, Rtf = Cella.Rtf, StileCarattere = Cella.StileCarattere, PropertyType = Cella.PropertyType, ConcatenaEtichettaEValore = Cella.ConcatenaEtichettaEValore, StampaFormula = Cella.StampaFormula };
                                RaggruppamentiValori rag = new RaggruppamentiValori() { Attributo = Cella.Attributo, AttributoCodice = Cella.AttributoCodice, AttributoCodicePath = Cella.AttributoCodicePath, EntityType = Cella.EntityType, Etichetta = Cella.Etichetta, Rtf = Cella.Rtf, StileCarattere = Cella.StileCarattere, PropertyType = Cella.PropertyType, ConcatenaEtichettaEValore = Cella.ConcatenaEtichettaEValore, StampaFormula = Cella.StampaFormula };
                               
                                
                                ReportSetting.Teste.Last().RaggruppamentiDocumento.Last().RaggruppamentiValori.Add(rag);
                                Cella.Attributo = null;
                                Cella.Etichetta = null;
                                Cella.AttributoCodice = null;
                                Cella.PropertyType = null;
                                VDCB.ContatoreRiga++;
                            }
                        }
                    }

                    VDCB.ContatoreColonna++;
                }

                int ContatoreColonne = ReportSetting.CorpiDocumento.Count();

                for (int i = 0; i < ContatoreColonne; i++)
                {
                    if (ReportSetting.CorpiDocumento[i].CorpoColonna.Count() > 0)
                    {
                        ReportSetting.CorpiDocumento[i].CorpoColonna.RemoveAt(0);
                    }
                }
            }
        }
        private void PageLayoutSetting()
        {
            if (string.IsNullOrEmpty(ReportSetting.Orientamento))
                ReportSetting.Orientamento = "0";

            if ((float)ReportSetting.AltezzaPagina != 0)
            {
                Page.PaperHeight = (float)ReportSetting.AltezzaPagina;
                Page.PaperWidth = (float)ReportSetting.LarghezzaPagina;
                Page.TopMargin = ReportSetting.MargineSuperiore;
                Page.BottomMargin = ReportSetting.MargineInferiore;
                Page.RightMargin = ReportSetting.MargineDestro;
                Page.LeftMargin = ReportSetting.MargineSinistro;
            }

            if (ReportSetting.Orientamento != "0")
            {
                Page.Landscape = true;
                MiddlePage = Units.Centimeters * ((ReportSetting.AltezzaPagina - ReportSetting.MargineSinistro - ReportSetting.MargineDestro) / (2 * 10));
                ThirdSpace = Units.Centimeters * ((ReportSetting.AltezzaPagina - ReportSetting.MargineSinistro - ReportSetting.MargineDestro) / (3 * 10));
                DimensionamentoRtf = ThirdSpace * 3 - (2 * Units.Centimeters) / 64;
            }
            else
                Page.Landscape = false;
        }
        private void SetDefaultPageComponent(bool FromDocumento = false)
        {
            Page.ReportTitle = new ReportTitleBand() { Name = "ReportTitleBand1" };

            Page.PageHeader = new PageHeaderBand();
            Page.PageHeader.Name = "PageHeader" + IndexDb;
            if (FromDocumento)
            {
                Page.PageHeader.Height = DefaultHeightBand * (float)2;
            }
            else
            {
                Page.PageHeader.Height = DefaultHeightBand * (float)1.5;
            }
            Page.PageHeader.CanGrow = true;
            Page.PageHeader.BeforePrint += PageHeader_BeforePrint;

            Page.PageFooter = new PageFooterBand();
            Page.PageFooter.Name = "PageFooter" + IndexDb;
            Page.PageFooter.Height = DefaultHeightBand * (float)1;
            Page.PageFooter.CanShrink = true;
            Page.PageFooter.CanGrow = true;

            Page.ColumnFooter = new ColumnFooterBand();
            Page.ColumnFooter.Name = "ColumnFooter" + IndexDb;
            Page.ColumnFooter.Height = DefaultHeightBand * (float)1;
            Page.ColumnFooter.CanShrink = true;
            Page.ColumnFooter.CanGrow = true;

            Page.ReportSummary = new ReportSummaryBand();
            Page.ReportSummary.Name = "ReportSummary" + IndexDb;
            Page.ReportSummary.Height = DefaultHeightBand;
            Page.ReportSummary.CanBreak = true;
            Page.ReportSummary.CanGrow = true;
            Page.ReportSummary.CanShrink = true;
            Page.ReportSummary.BeforePrint += ReportSummary_BeforePrint;

            Data = new DataBand();
            Data.CanGrow = true;
            //Data.CanBreak = true;
            Data.CanShrink = true;
            //Data.EvenStyle = "RigheAlternate";
            Data.Name = "Data";
            Data.Height = DefaultHeight;
            if (DbName != null)
                Data.DataSource = ReportObject.GetDataSource(DbName);
        }
        int CouinterPageHeaderPrint;
        private void PageHeader_BeforePrint(object sender, EventArgs e)
        {
            if (CouinterPageHeaderPrint == 0)
            {
                foreach (TextObject textObject in RiportiPrimaPaginaDaNascondere)
                {
                    textObject.Visible = false;
                }
            }
            else
            {
                foreach (TextObject textObject in RiportiPrimaPaginaDaNascondere)
                {
                    textObject.Visible = true;
                }
            }
            CouinterPageHeaderPrint++;
        }

        private void ReportSummary_BeforePrint(object sender, EventArgs e)
        {
            foreach (TextObject textObject in RiportiUltimaPaginaDaNascondere)
            {
                textObject.Text = "";
                //textObject.Visible = false;
            }
        }

        Dictionary<string, int> GroupsName = new Dictionary<string, int>();
        private void CreateGroupsBand()
        {
            GroupHeaderBand GruppoPadre = new GroupHeaderBand();

            //CREO GRUPPO PADRE PER GESTIRE I TOTALI COMPLESSIVI IN ATTESA DI RISOLUZIONE ULTIMA PAGINA PER COLUMN FOOTER
            CreateGroupHeaderSettingData(GruppoPadre, StampeKeys.ConstGruppoPadre, null);
            GruppoPadre.GroupFooter = CreateGroupFooterSettingData(StampeKeys.ConstGruppoPadre);
            AddObjectToFooterGroupPadre(GruppoPadre.GroupFooter);
            GroupsName.Add(GruppoPadre.GroupFooter.Name, 0);
            if (ReportSetting.RaggruppamentiDatasource.Count != 0)
                CreateGroupStructure(ReportSetting.RaggruppamentiDatasource, 0, GruppoPadre, GruppoPadre, GruppoPadre);
            else
                AssingDataSourceToGroup(GruppoPadre, GruppoPadre);
        }

        private void CreateGroupStructure(List<Raggruppamento> raggruppamentiDatasource, int contatoreGruppoLocal, GroupHeaderBand GruppoPrecedente, GroupHeaderBand PrimoGruppo = null, GroupHeaderBand UltimoGruppo = null)
        {

            if (raggruppamentiDatasource.Count() > 0)
            {
                if (raggruppamentiDatasource.Any(x => string.IsNullOrEmpty(x.Attributo)))
                {
                    AssingDataSourceToGroup(UltimoGruppo, UltimoGruppo);
                }
                else
                {
                    int IndexParent = 0;
                    string EntityType = raggruppamentiDatasource.ElementAt(contatoreGruppoLocal).EntityType;
                    string AttributoPerReport = raggruppamentiDatasource.ElementAt(contatoreGruppoLocal).AttributoPerReport;

                    GroupHeaderBand GruppoPadri = new GroupHeaderBand();
                    GruppoPadri.SortOrder = SortOrder.None;
                    GroupHeaderBand GruppoFoglia = new GroupHeaderBand();
                    GruppoFoglia.SortOrder = SortOrder.None;

                    bool ExistOne = false;
                    if (ListParentItem.Where(x => x.Attributo1.StartsWith(JReportHelper.ReplaceSymbolNotAllowInReport(EntityType))).Count() > 0 && ReportSetting.Teste[contatoreGruppoLocal].PositionVerticalTreeRaggruppamento == -1)
                    {
                        ExistOne = CreateParentForAttibute(contatoreGruppoLocal, EntityType, GruppoPadri, ListParentItem.Where(x => x.Attributo1.StartsWith(JReportHelper.ReplaceSymbolNotAllowInReport(EntityType))).ElementAt(IndexParent), IndexParent);
                    }


                    if (raggruppamentiDatasource.ElementAt(contatoreGruppoLocal).EntityType.StartsWith(BuiltInCodes.EntityType.Divisione))
                    {
                        if (raggruppamentiDatasource.ElementAt(contatoreGruppoLocal).PropertyType == BuiltInCodes.DefinizioneAttributo.Guid)
                        {
                            var rag = raggruppamentiDatasource.ElementAt(contatoreGruppoLocal);
                            if (DeveloperVariables.IsNewStampa)
                                AttributoPerReport = rag.AttributoPerReport;
                            else
                                AttributoPerReport = EntityType + EntityType + StampeKeys.Const_Guid;
                        }
                    }

                    CreateGroupHeaderSettingData(GruppoFoglia, AttributoPerReport, raggruppamentiDatasource.ElementAt(contatoreGruppoLocal).OpzioniDiStampa);
                    AddObjectToHeaderGroups(contatoreGruppoLocal, GruppoFoglia, GruppoPadri);
                    GruppoFoglia.GroupFooter = CreateGroupFooterSettingData(AttributoPerReport);
                    GruppoFoglia.BeforePrint += GruppoFoglia_BeforePrint;
                    GruppoFoglia.GroupFooter.AfterPrint += GruppoFoglia_AfterPrint;

                    AddObjectToFooterGroups(contatoreGruppoLocal, GruppoFoglia.GroupFooter);
                    GroupsName.Add(GruppoFoglia.GroupFooter.Name, contatoreGruppoLocal);

                    if (contatoreGruppoLocal == 0)
                    {
                        if (ExistOne) { PrimoGruppo.NestedGroup = GruppoPadri; } else { PrimoGruppo.NestedGroup = GruppoFoglia; }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(GruppoPadri.Name))
                        {
                            GruppoPrecedente.NestedGroup = GruppoPadri;
                        }
                        else
                        {
                            GruppoPrecedente.NestedGroup = GruppoFoglia;
                        }
                    }


                    if (contatoreGruppoLocal + 1 == ReportSetting.RaggruppamentiDatasource.Count())
                    {
                        AssingDataSourceToGroup(FindLastGroup(GruppoPrecedente), PrimoGruppo);
                        return;
                    }

                    CreateGroupStructure(raggruppamentiDatasource, contatoreGruppoLocal + 1, FindLastGroup(GruppoPrecedente), PrimoGruppo);
                }
            }
            else
            {
                AssingDataSourceToGroup(UltimoGruppo, UltimoGruppo);
            }
        }

        private void GroupFooter_BeforePrint(object sender, EventArgs e)
        {
            //gestione per non stampare i "Sommano" in caso di albero disomogeneo (livelli diversi per ramo)

            GroupFooterBand GroupHeaderFooterBand = sender as GroupFooterBand;

            string dataSourceColumnName = GroupHeaderFooterBand.Name.Substring(StampeKeys.ConstGroupFooter.Length);

            var column = Data.DataSource.Columns.FindByName(dataSourceColumnName);

            string value = string.Empty;

            if (column != null)
                value = Data.DataSource[column].ToString();

            if (string.IsNullOrEmpty(value))
            {
                foreach (var childObj in GroupHeaderFooterBand.ChildObjects)
                {
                    TextObject textObject = childObj as TextObject;
                    textObject.Visible = false;
                }
            }
            else
            {
                foreach (var childObj in GroupHeaderFooterBand.ChildObjects)
                {
                    TextObject textObject = childObj as TextObject;
                    textObject.Visible = true;
                }
            }
        }

        private void GruppoFoglia_BeforePrint(object sender, EventArgs e)
        {
            string GroupHeaderFooterBandName = (sender as GroupHeaderBand).Name;

            foreach (RiportoTotalProperties totalInGroupFooter in ListOggettiPerBanda)
            {
                int indiceGruppoCorrente = GroupsName[GroupHeaderFooterBandName.Replace(StampeKeys.ConstGroupHeader, StampeKeys.ConstGroupFooter)];
                int indiceGruppoTotale = GroupsName[totalInGroupFooter.BandName];

                if (indiceGruppoCorrente < indiceGruppoTotale)
                {
                    string complexName = ReportParameters.ElementAt(totalInGroupFooter.ColumnIndex).RetrieveParameterString(true);
                    ReportObject.SetParameterValue(complexName, "");

                    complexName = ReportParameters.ElementAt(totalInGroupFooter.ColumnIndex).RetrieveParameterString(true);
                    ReportObject.SetParameterValue(complexName, "");
                    ReportParameters.ElementAt(totalInGroupFooter.ColumnIndex).RiportoValue = 0;
                    ReportParameters.ElementAt(totalInGroupFooter.ColumnIndex).RiportareValue = 0;
                }
                else
                {
                    ReportObject.SetParameterValue(ReportParameters.ElementAt(totalInGroupFooter.ColumnIndex).RetrieveParameterString(true), RetieveFormattingValue(totalInGroupFooter.EntityAttributo, ReportParameters.ElementAt(totalInGroupFooter.ColumnIndex).RiportoValue));
                    ReportObject.SetParameterValue(ReportParameters.ElementAt(totalInGroupFooter.ColumnIndex).RetrieveParameterString(false), RetieveFormattingValue(totalInGroupFooter.EntityAttributo, ReportParameters.ElementAt(totalInGroupFooter.ColumnIndex).RiportareValue));
                }
            }
        }

        private void GruppoFoglia_AfterPrint(object sender, EventArgs e)
        {
            InitializeScriptReportVariable(false, (sender as HeaderFooterBandBase).Name);
        }

        bool enableSetValue = true;
        string raggruppamantoPrecedente = null;
        private void InitializeScriptReportVariable(bool SetDefalutValue, string GroupHeaderFooterBandName = null)
        {
            double ZeroValue = 0;

            if (SetDefalutValue)
            {
                for (int i = 0; i < 20; i++)
                {
                    ReportObject.SetParameterValue(ReportParameters.ElementAt(i).RetrieveParameterString(true), "");
                    ReportParameters.ElementAt(i).RiportoValue = ZeroValue;
                    ReportObject.SetParameterValue(ReportParameters.ElementAt(i).RetrieveParameterString(false), "");
                    ReportParameters.ElementAt(i).RiportoValue = ZeroValue;
                }
            }
            else
            {
                enableSetValue = true;
                if (GroupHeaderFooterBandName.Contains(StampeKeys.ConstGroupFooter))
                {
                    foreach (RiportoTotalProperties totalInGroupFooter in ListOggettiPerBanda)
                    {
                        int indiceGruppoCorrente = GroupsName[GroupHeaderFooterBandName];
                        int indiceGruppoTotale = GroupsName[totalInGroupFooter.BandName];

                        if (indiceGruppoCorrente < indiceGruppoTotale)
                        {
                            ReportObject.SetParameterValue(ReportParameters.ElementAt(totalInGroupFooter.ColumnIndex).RetrieveParameterString(true), "");
                            ReportObject.SetParameterValue(ReportParameters.ElementAt(totalInGroupFooter.ColumnIndex).RetrieveParameterString(false), "");
                            ReportParameters.ElementAt(totalInGroupFooter.ColumnIndex).RiportoValue = 0;
                            ReportParameters.ElementAt(totalInGroupFooter.ColumnIndex).RiportareValue = 0;
                        }
                        else
                        {
                            ReportObject.SetParameterValue(ReportParameters.ElementAt(totalInGroupFooter.ColumnIndex).RetrieveParameterString(true), RetieveFormattingValue(totalInGroupFooter.EntityAttributo, ReportParameters.ElementAt(totalInGroupFooter.ColumnIndex).RiportoValue));
                            ReportObject.SetParameterValue(ReportParameters.ElementAt(totalInGroupFooter.ColumnIndex).RetrieveParameterString(false), RetieveFormattingValue(totalInGroupFooter.EntityAttributo, ReportParameters.ElementAt(totalInGroupFooter.ColumnIndex).RiportareValue));
                        }
                    }
                }
                raggruppamantoPrecedente = GroupHeaderFooterBandName;
            }
        }

        private void AddObjectToFooterGroupPadre(GroupFooterBand groupFooter)
        {
            VariabiliDiCicloBanda VDCB = new VariabiliDiCicloBanda(0, 0, 0, 0, 0, 0);
            bool AddObjectToBand = true;

            if (ReportSetting.IsBandFineDocumentoEmpty)
                return;

            if (ReportSetting.FineDocumento != null)
            {
                if (ReportSetting.FineDocumento.Count != 0)
                {
                    foreach (var Attributi in ReportSetting.FineDocumento)
                    {
                        VDCB.Width = (Units.Centimeters * (float)Attributi.Size);

                        foreach (var Attributo in Attributi.CorpoColonna)
                        {
                            float RowHeight = Units.Centimeters * (float)1 / 2;

                            if (!string.IsNullOrEmpty(Attributo.Attributo))
                            {

                                TextObject TextObject = new TextObject();
                                TextObject.CanGrow = true;
                                TextObject.HorzAlign = HorzAlign.Left;

                                VDCB.WidthEtichetta = VDCB.Width / 2;
                                bool HasEtichetta = false;
                                string TextAttributo = null;

                                if (!string.IsNullOrEmpty(FormatEtichettaIfIsEmpty(Attributo.Etichetta)))
                                {
                                    TextObject Etichetta = CreaEtichetta(Attributo, VDCB.XTraslation, VDCB.YTraslation, VDCB.WidthEtichetta);
                                    Etichetta.HorzAlign = HorzAlign.Left;
                                    if (!Attributo.ConcatenaEtichettaEValore)
                                    {
                                        HasEtichetta = true;
                                        if (Bordatura)
                                        {
                                            CreaBordo(Etichetta, true, false, false, false);
                                            if (Attributo.StileCarattere.TextVerticalAlignementCode == 2)
                                                Etichetta.WordWrap = false;
                                            Etichetta.GrowToBottom = true;
                                        }
                                        groupFooter.Objects.Add(Etichetta);
                                    }
                                    else
                                    {
                                        TextAttributo = Etichetta.Text;
                                        TextAttributo = TextAttributo + " ";
                                    }
                                }

                                if (HasEtichetta == false)
                                    TextObject.Bounds = new RectangleF(VDCB.XTraslation, VDCB.YTraslation, VDCB.Width, DefaultHeightTextBox);
                                else
                                    TextObject.Bounds = new RectangleF(VDCB.XTraslation + VDCB.WidthEtichetta, VDCB.YTraslation, VDCB.Width - VDCB.WidthEtichetta, DefaultHeightTextBox);

                                SettingFontForTextObject(TextObject, Attributo.StileCarattere, Bordatura);

                                if (Attributo.CodiceDigicorp == StampeKeys.ConstSommaWizard || Attributo.CodiceDigicorp == StampeKeys.ConstContaWizard)
                                {
                                    Total Total = new Total();
                                    string TotalName = null;
                                    if (Attributo.CodiceDigicorp == StampeKeys.ConstSommaWizard) { TotalName = "TotalOf"; }
                                    else { TotalName = "ContaOf"; }
                                    TotalName = TotalName + Page.ReportSummary.Name + JReportHelper.ReplaceSymbolNotAllowInReport(Attributo.AttributoPerReport) + IndexDb;
                                    TotalName = TotalName + JReportHelper.ReplaceSymbolNotAllowInReport(Attributo.AttributoPerReport);
                                    if (ReportObject.Dictionary.Totals.FindByName(TotalName) == null && !string.IsNullOrEmpty(Attributo.Attributo))
                                    {
                                        if (Attributo.CodiceDigicorp == StampeKeys.ConstSommaWizard) { Total.Name = TotalName; Total.TotalType = TotalType.Sum; Total.Expression = "[" + DbName + "." + JReportHelper.ReplaceSymbolNotAllowInReport(Attributo.AttributoPerReport) + "]"; }
                                        if (Attributo.CodiceDigicorp == StampeKeys.ConstContaWizard) { Total.Name = TotalName; Total.TotalType = TotalType.Count; }
                                        Total.Evaluator = Data;
                                        Total.PrintOn = Page.ReportSummary;
                                        ReportObject.Dictionary.Totals.Add(Total);
                                    }
                                    else
                                    {
                                        AddObjectToBand = false;
                                    }

                                    TextObject.Name = StampeKeys.ConstGruppoPadre + "TextAttributoValoreTotal" + JReportHelper.ReplaceSymbolNotAllowInReport(Attributo.AttributoPerReport);
                                    TextObject.Text = "[" + TotalName + "]";
                                    TextObject.AfterData += Riporti_AfterData;

                                    if (FormatoNumero.Contains(Attributo.PropertyType) || FormatoValuta.Contains(Attributo.PropertyType))
                                        RetrieveFormatFromAttribute(JReportHelper.ReplaceSymbolNotAllowInReport(Attributo.AttributoPerReport), TextObject);
                                }
                                else
                                {
                                    TextObject.Name = "TextAttributoValore" + JReportHelper.ReplaceSymbolNotAllowInReport(Attributo.AttributoPerReport);
                                    TextObject.Text = "[" + DbName + "." + JReportHelper.ReplaceSymbolNotAllowInReport(Attributo.AttributoPerReport) + "]";

                                    if (FormatoNumero.Contains(Attributo.PropertyType) || FormatoValuta.Contains(Attributo.PropertyType))
                                        RetrieveFormatFromAttribute(JReportHelper.ReplaceSymbolNotAllowInReport(Attributo.AttributoPerReport), TextObject);
                                }

                                if (HasEtichetta)
                                    if (Bordatura)
                                        CreaBordo(TextObject, false, false, true, false);

                                TextObject.Text = TextAttributo + TextObject.Text;

                                if (!AddObjectToBand)
                                    TextObject.Text = "";

                                if (Attributo.StileCarattere != null)
                                    if (Attributo.StileCarattere.TextVerticalAlignementCode == 2)
                                        AggiungiEtichettaVuotaPerAncoraggioBasso(groupFooter, TextObject, HasEtichetta);

                                groupFooter.Objects.Add(TextObject);
                                AddObjectToBand = true;
                            }
                            else
                            {
                                TextObject Etichetta = CreaEtichetta(Attributo, VDCB.XTraslation, VDCB.YTraslation, VDCB.Width);
                                if (Bordatura)
                                {
                                    CreaBordo(Etichetta, true, false, true, false);
                                    Etichetta.GrowToBottom = true;
                                }
                                groupFooter.Objects.Add(Etichetta);
                            }

                            VDCB.YTraslation = VDCB.YTraslation + RowHeight;

                            VDCB.ContatoreRiga++;
                        }
                        VDCB.ContatoreColonna++;

                        if (VDCB.ContatoreColonna != 0)
                        { VDCB.XTraslation = VDCB.XTraslation + VDCB.Width; }
                        else { VDCB.XTraslation = 0; }
                        VDCB.YTraslation = 0;
                        VDCB.ContatoreRiga = 0;
                    }
                    VDCB.YTraslation = VDCB.YTraslation + DefaultHeightTextBox;
                }
            }
        }
        private bool CreateParentForAttibute(int ContatoreGruppo, string entityType, GroupHeaderBand Group, ParenEntity Attributo, int indexParent)
        {
            VariabiliDiCicloBanda VDCB = new VariabiliDiCicloBanda(0, 0, 0, 0, 0, 0);
            bool ExistOne = false;
            bool JustUseAttributo1 = false;

            if (ReportSetting.Teste.Count != 0)
            {
                foreach (var Attributi in ReportSetting.Teste.ElementAt(ContatoreGruppo).RaggruppamentiDocumento)
                {
                    VDCB.Width = (Units.Centimeters * (float)Attributi.Size);

                    foreach (var Corpo in Attributi.RaggruppamentiValori)
                    {
                        if (VDCB.ContatoreRiga == 1)
                        {
                            continue;
                        }

                        float RowHeight = (float)1 / 2;


                        //if (!String.IsNullOrEmpty(Corpo.Attributo) && Corpo.EntityType == entityType && !String.IsNullOrEmpty(Corpo.EntityType) && (Corpo.AttributoCodice == MasterDetailModel.BuiltInCodes.Attributo.Codice || Corpo.AttributoCodice == MasterDetailModel.BuiltInCodes.Attributo.DescrizioneRTF || Corpo.AttributoCodice == MasterDetailModel.BuiltInCodes.Attributo.Nome))
                        if (!String.IsNullOrEmpty(Corpo.Attributo) && Corpo.EntityType == entityType && !String.IsNullOrEmpty(Corpo.EntityType) && (entitiesHelper.IsAttributoDeep(entityType, Corpo.AttributoCodice)))
                        {
                            VDCB.WidthEtichetta = VDCB.Width / 2;
                            bool HasEtichetta = false;
                            string TextAttributo = null;

                            if (!string.IsNullOrEmpty(FormatEtichettaIfIsEmpty(Corpo.Etichetta)))
                            {
                                TextObject Etichetta = CreaEtichetta(Corpo, VDCB.XTraslation, VDCB.YTraslation, VDCB.WidthEtichetta);
                                Etichetta.HorzAlign = HorzAlign.Left;
                                if (!Corpo.ConcatenaEtichettaEValore)
                                {
                                    HasEtichetta = true;
                                    if (Bordatura)
                                    {
                                        CreaBordo(Etichetta, true, false, false, false);
                                        Etichetta.GrowToBottom = true;
                                        Etichetta.CanShrink = false;
                                    }
                                    Group.Objects.Add(Etichetta);
                                }
                                else
                                {
                                    TextAttributo = Etichetta.Text;
                                    TextAttributo = TextAttributo + " ";
                                }
                            }

                            //if (Corpo.AttributoCodice == MasterDetailModel.BuiltInCodes.Attributo.Codice || Corpo.AttributoCodice == MasterDetailModel.BuiltInCodes.Attributo.DescrizioneRTF || Corpo.AttributoCodice == MasterDetailModel.BuiltInCodes.Attributo.Nome)
                            if (entitiesHelper.IsAttributoDeep(entityType, Corpo.AttributoCodice))
                            {
                                if (Corpo.PropertyType == MasterDetailModel.BuiltInCodes.DefinizioneAttributo.TestoRTF)
                                {
                                    if (Corpo.Rtf || ReportSetting.IsAllFieldRtfFormat)
                                    {
                                        RichObject RichText = null;
                                        if (HasEtichetta)
                                            RichText = CreateRichObject("Text" + JReportHelper.ReplaceSymbolNotAllowInReport(Attributo.Attributo2), DbName + "." + JReportHelper.ReplaceSymbolNotAllowInReport(Attributo.Attributo2), VDCB.XTraslation + VDCB.WidthEtichetta, Units.Centimeters * VDCB.YTraslation, VDCB.WidthEtichetta, DefaultHeightTextBox);
                                        else
                                            RichText = CreateRichObject("Text" + JReportHelper.ReplaceSymbolNotAllowInReport(Attributo.Attributo2), DbName + "." + JReportHelper.ReplaceSymbolNotAllowInReport(Attributo.Attributo2), VDCB.XTraslation, Units.Centimeters * VDCB.YTraslation, VDCB.Width, DefaultHeightTextBox);
                                        RichText.GrowToBottom = false;
                                        RichText.CanShrink = true;
                                        if (Bordatura)
                                        {
                                            if (HasEtichetta)
                                                CreaBordo(RichText, false, false, true, false);
                                            else
                                                CreaBordo(RichText, true, false, true, false);
                                            
                                        }

                                        Group.Objects.Add(RichText);
                                        goto EndFormatting;

                                    }
                                }
                                ExistOne = true;


                                TextObject GroupText = null;
                                if (Corpo.PropertyType == MasterDetailModel.BuiltInCodes.DefinizioneAttributo.TestoRTF)
                                {
                                    GroupText = CreateTextObject("Text" + Attributo.Attributo2, TextAttributo + "[" + DbName + "." + Attributo.Attributo2 + "]", VDCB.XTraslation, Units.Centimeters * VDCB.YTraslation, VDCB.Width, DefaultHeightTextBox);
                                }
                                else
                                {
                                    if (Corpo.AttributoCodice == MasterDetailModel.BuiltInCodes.Attributo.Codice || Corpo.AttributoCodice == MasterDetailModel.BuiltInCodes.Attributo.Nome)
                                    {
                                        if (!JustUseAttributo1)
                                        {
                                            GroupText = CreateTextObject("Text" + Attributo.Attributo1, TextAttributo + "[" + DbName + "." + Attributo.Attributo1 + "]", VDCB.XTraslation, Units.Centimeters * VDCB.YTraslation, VDCB.Width, DefaultHeightTextBox);
                                            JustUseAttributo1 = true;
                                        }
                                        else
                                        {
                                            //by Ale : non capisco quando entri in questo codice
                                            GroupText = CreateTextObject("Text" + Attributo.Attributo2, TextAttributo + "[" + DbName + "." + Attributo.Attributo2 + "]", VDCB.XTraslation, Units.Centimeters * VDCB.YTraslation, VDCB.Width, DefaultHeightTextBox);
                                        }
                                    }
                                    else if (Corpo.AttributoCodice == MasterDetailModel.BuiltInCodes.Attributo.Descrizione)
                                    {
                                        GroupText = CreateTextObject("Text" + Attributo.Attributo3, TextAttributo + "[" + DbName + "." + Attributo.Attributo3 + "]", VDCB.XTraslation, Units.Centimeters * VDCB.YTraslation, VDCB.Width, DefaultHeightTextBox);
                                    }
                                    else
                                        GroupText = CreateTextObject("Text" + Corpo.AttributoPerReport, TextAttributo + "[" + DbName + "." + Corpo.AttributoPerReport + "]", VDCB.XTraslation, Units.Centimeters * VDCB.YTraslation, VDCB.Width, DefaultHeightTextBox);
                                }

                                SettingFontForTextObject(GroupText, Corpo.StileCarattere, Bordatura);

                                if (HasEtichetta == false)
                                {
                                    GroupText.Bounds = new RectangleF(VDCB.XTraslation, Units.Centimeters * VDCB.YTraslation, VDCB.Width, DefaultHeightTextBox);
                                }
                                else
                                {
                                    GroupText.Bounds = new RectangleF(VDCB.XTraslation + VDCB.WidthEtichetta, Units.Centimeters * VDCB.YTraslation, VDCB.Width - VDCB.WidthEtichetta, DefaultHeightTextBox);
                                    if (Bordatura)
                                        CreaBordo(GroupText, false, false, true, false);
                                }
                                GroupText.CanShrink = true;
                                Group.Objects.Add(GroupText);

                            EndFormatting:

                                VDCB.YTraslation = VDCB.YTraslation + RowHeight;

                                VDCB.ContatoreRiga++;
                            }
                            else
                            {
                                TextObject Etichetta = CreaEtichetta(null, VDCB.XTraslation, VDCB.YTraslation, VDCB.Width);
                                int OldCode = Corpo.StileCarattere.TextVerticalAlignementCode;
                                Corpo.StileCarattere.TextVerticalAlignementCode = 1;
                                SettingFontForTextObject(Etichetta, Corpo.StileCarattere, Bordatura);
                                Corpo.StileCarattere.TextVerticalAlignementCode = OldCode;
                                Etichetta.CanShrink = true;
                                Group.Objects.Add(Etichetta);
                            }
                        }
                        else
                        {
                            TextObject Etichetta = CreaEtichetta(null, VDCB.XTraslation, VDCB.YTraslation, VDCB.Width);
                            int OldCode = Corpo.StileCarattere.TextVerticalAlignementCode;
                            Corpo.StileCarattere.TextVerticalAlignementCode = 1;
                            SettingFontForTextObject(Etichetta, Corpo.StileCarattere, Bordatura);
                            Corpo.StileCarattere.TextVerticalAlignementCode = OldCode;
                            Etichetta.CanShrink = true;
                            Group.Objects.Add(Etichetta);
                        }

                        VDCB.ContatoreColonna++;

                    }
                    if (VDCB.ContatoreColonna != 0)
                    { VDCB.XTraslation = VDCB.XTraslation + (float)ReportSetting.RaggruppamentiDatasource.ElementAt(ContatoreGruppo).Indent + VDCB.Width; }
                    else { VDCB.XTraslation = (float)ReportSetting.RaggruppamentiDatasource.ElementAt(ContatoreGruppo).Indent; }
                    VDCB.YTraslation = 0;
                    VDCB.ContatoreRiga = 0;
                }

                if (ExistOne)
                {
                    Group.CanGrow = true;
                    Group.CanBreak = true;
                    Group.CanShrink = true;
                    string AttributoPerReport = string.Empty;

                    if (DeveloperVariables.IsNewStampa)
                        AttributoPerReport = Attributo.AttributoGuid;
                    else
                        AttributoPerReport = JReportHelper.ReplaceSymbolNotAllowInReport(entityType + entityType + StampeKeys.Const_Guid + Attributo.Attributo1.Substring(Attributo.Attributo1.Length - 1));

                    Group.Name = "GroupHeader" + AttributoPerReport;
                    Group.Condition = "[" + DbName + "." + AttributoPerReport + "]";
                    Group.SortOrder = SortOrder.None;
                    Group.GroupFooter = CreateGroupFooterSettingData(AttributoPerReport);
                    Group.GroupFooter.BeforePrint += GroupFooter_BeforePrint;


                    AddTotalsForParent(ContatoreGruppo, Group.GroupFooter, Attributo);

                    
                }

                GroupHeaderBand NestedGroupGerarchico = new GroupHeaderBand();

                if (indexParent + 1 < ListParentItem.Where(x => x.Attributo1.StartsWith(JReportHelper.ReplaceSymbolNotAllowInReport(entityType))).Count())
                {
                    CreateParentForAttibute(ContatoreGruppo, entityType, NestedGroupGerarchico, ListParentItem.Where(x => x.Attributo1.StartsWith(JReportHelper.ReplaceSymbolNotAllowInReport(entityType))).ElementAt(indexParent + 1), indexParent + 1);
                }


                if (!string.IsNullOrEmpty(NestedGroupGerarchico.Condition))
                {
                    Group.NestedGroup = NestedGroupGerarchico;
                }
            }
            return ExistOne;
        }

        private void AddTotalsForParent(int ContatoreGruppo, GroupFooterBand groupFooter, ParenEntity attributo)
        {
            VariabiliDiCicloBanda VDCB = new VariabiliDiCicloBanda(0, 0, 0, 0, 0, 0);
            bool AddObjectToBand = true;

            string Testo = null;
            bool VisibleAtLestOneSommaStruttura = false;

            if (ReportSetting.Code.Count != 0)
            {
                foreach (var Attributi in ReportSetting.Code.ElementAt(ContatoreGruppo).RaggruppamentiDocumento)
                {
                    VDCB.Width = (Units.Centimeters * (float)Attributi.Size);

                    foreach (var Attributo in Attributi.RaggruppamentiValori)
                    {
                        float RowHeight = (float)1 / 2;

                        VisibleAtLestOneSommaStruttura = false;
                        List<RaggruppamentiValori> RaggruppamentiValori = RicercaAttributoPerEspressioneFunzioneInRiga(ReportSetting.Code.ElementAt(ContatoreGruppo).RaggruppamentiDocumento, VDCB.ContatoreRiga);
                        if (RaggruppamentiValori.Any(c => c.CodiceDigicorp == StampeKeys.ConstSommaStrutturaWizard))
                        {
                            VisibleAtLestOneSommaStruttura = true;
                        }

                        if (!string.IsNullOrEmpty(Attributo.Attributo))
                        {
                            TextObject GroupText = new TextObject();
                            GroupText.CanGrow = true;
                            SettingFontForTextObject(GroupText, Attributo.StileCarattere, Bordatura);

                            VDCB.WidthEtichetta = VDCB.Width / 2;
                            bool HasEtichetta = false;
                            string TextAttributo = null;

                            if (!string.IsNullOrEmpty(FormatEtichettaIfIsEmpty(Attributo.Etichetta)))
                            {
                                HasEtichetta = true;
                                RaggruppamentiValori Val = new RaggruppamentiValori();
                                Val.Attributo = Attributo.Attributo + (attributo.Numero).ToString();
                                Val.AttributoCodice = Attributo.AttributoCodice + (attributo.Numero).ToString();
                                Val.EntityType = Attributo.EntityType;
                                Val.Etichetta = Attributo.Etichetta;
                                Val.PropertyType = Attributo.PropertyType;
                                Val.StileCarattere = Attributo.StileCarattere;

                                if (!string.IsNullOrEmpty(Val.Etichetta))
                                {
                                    TextObject Etichetta = CreaEtichetta(Val, VDCB.XTraslation, VDCB.YTraslation, VDCB.WidthEtichetta);
                                    if (!Attributo.ConcatenaEtichettaEValore)
                                    {
                                        if (Bordatura)
                                        {
                                            CreaBordo(Etichetta, true, false, false, false);
                                        }
                                        if (VisibleAtLestOneSommaStruttura)
                                        {
                                            groupFooter.Objects.Add(Etichetta);
                                        }
                                    }
                                    else
                                    {
                                        TextAttributo = Etichetta.Text + " ";
                                    }
                                }
                            }

                            if (HasEtichetta == false)
                            {
                                GroupText.Bounds = new RectangleF(VDCB.XTraslation, Units.Centimeters * VDCB.YTraslation, VDCB.Width, DefaultHeightTextBox);
                                if (Bordatura)
                                    CreaBordo(GroupText, true, false, true, false);
                            }
                            else
                            {
                                GroupText.Bounds = new RectangleF(VDCB.XTraslation + VDCB.WidthEtichetta, Units.Centimeters * VDCB.YTraslation, VDCB.Width - VDCB.WidthEtichetta, DefaultHeightTextBox);
                                if (!Attributo.ConcatenaEtichettaEValore)
                                {
                                    if (Bordatura)
                                        CreaBordo(GroupText, false, false, true, false);
                                }
                                else
                                    GroupText.Bounds = new RectangleF(VDCB.XTraslation, Units.Centimeters * VDCB.YTraslation, VDCB.Width, DefaultHeightTextBox);
                            }

                            if (Attributo.CodiceDigicorp == StampeKeys.ConstSommaWizard || Attributo.CodiceDigicorp == StampeKeys.ConstSommaStrutturaWizard || Attributo.CodiceDigicorp == StampeKeys.ConstContaWizard)
                            {
                                if (Attributo.PropertyType == MasterDetailModel.BuiltInCodes.DefinizioneAttributo.Reale)
                                    goto EndFormatting;

                                Total Total = new Total();
                                string TotalName = null;
                                if (Attributo.CodiceDigicorp == StampeKeys.ConstSommaWizard || Attributo.CodiceDigicorp == StampeKeys.ConstSommaStrutturaWizard) { TotalName = "TotalOf"; }
                                else { TotalName = "ContaOf"; }
                                TotalName = TotalName + groupFooter.Name + JReportHelper.ReplaceSymbolNotAllowInReport(Attributo.AttributoPerReport) + IndexDb;
                                TotalName = TotalName + JReportHelper.ReplaceSymbolNotAllowInReport(Attributo.AttributoPerReport);

                                if (ReportObject.Dictionary.Totals.FindByName(TotalName) == null && !string.IsNullOrEmpty(Attributo.Attributo))
                                {
                                    if (Attributo.CodiceDigicorp == StampeKeys.ConstSommaWizard || Attributo.CodiceDigicorp == StampeKeys.ConstSommaStrutturaWizard) { Total.Name = TotalName; Total.TotalType = TotalType.Sum; Total.Expression = "[" + DbName + "." + JReportHelper.ReplaceSymbolNotAllowInReport(Attributo.AttributoPerReport) + "]"; }
                                    if (Attributo.CodiceDigicorp == StampeKeys.ConstContaWizard) { Total.Name = TotalName; Total.TotalType = TotalType.Count; }
                                    Total.Evaluator = Data;
                                    Total.PrintOn = groupFooter;
                                    ReportObject.Dictionary.Totals.Add(Total);
                                }
                                else
                                {
                                    AddObjectToBand = false;
                                }

                                GroupText.Name = "TextAttributoValoreTotal" + Attributo.AttributoPerReport + (attributo.Numero).ToString();
                                GroupText.Text = "[" + TotalName + "]";

                                if (FormatoNumero.Contains(Attributo.PropertyType) || FormatoValuta.Contains(Attributo.PropertyType))
                                    RetrieveFormatFromAttribute(JReportHelper.ReplaceSymbolNotAllowInReport(Attributo.AttributoPerReport), GroupText);
                            }
                            else
                            {





                                //if (Attributo.AttributoCodice != MasterDetailModel.BuiltInCodes.Attributo.Codice && Attributo.AttributoCodice != MasterDetailModel.BuiltInCodes.Attributo.DescrizioneRTF)
                                if (!entitiesHelper.IsAttributoDeep(Attributo.EntityType, Attributo.AttributoCodice))
                                {
                                    if (Bordatura)
                                    {
                                        GroupText.CanShrink = true;
                                        groupFooter.Objects.Add(GroupText);
                                    }
                                    goto EndFormatting;
                                }

                                GroupText.Name = "TextAttributoValore" + JReportHelper.ReplaceSymbolNotAllowInReport(Attributo.AttributoPerReport) + (attributo.Numero).ToString();
                                GroupText.Text = "[" + DbName + "." + JReportHelper.ReplaceSymbolNotAllowInReport(Attributo.AttributoPerReport) + (attributo.Numero).ToString() + "]";

                                if (FormatoNumero.Contains(Attributo.PropertyType) || FormatoValuta.Contains(Attributo.PropertyType))
                                    RetrieveFormatFromAttribute(JReportHelper.ReplaceSymbolNotAllowInReport(Attributo.AttributoPerReport), GroupText);
                            }

                            GroupText.Text = TextAttributo + GroupText.Text;

                            if (VisibleAtLestOneSommaStruttura)
                            {
                                if (Attributo.CodiceDigicorp != StampeKeys.ConstSommaWizard)
                                    if (AddObjectToBand)
                                        groupFooter.Objects.Add(GroupText);
                            }
                            else
                            {
                                if (Bordatura)
                                {
                                    TextObject Etichetta = CreaEtichetta(new CellaTabella(), VDCB.XTraslation, VDCB.YTraslation, VDCB.Width);
                                    Etichetta.GrowToBottom = true;
                                    Etichetta.CanShrink = true;
                                    CreaBordo(Etichetta, true, false, true, false);
                                    groupFooter.Objects.Add(Etichetta);
                                }
                            }
                            AddObjectToBand = true;
                        }
                        else
                        {
                            TextObject Etichetta = CreaEtichetta(Attributo, VDCB.XTraslation, VDCB.YTraslation, VDCB.Width);
                            Etichetta.GrowToBottom = true;
                            if (Bordatura)
                            {
                                CreaBordo(Etichetta, true, false, true, false);
                                Etichetta.CanShrink = true;
                                groupFooter.Objects.Add(Etichetta);
                            }
                        }

                    EndFormatting:
                        VDCB.YTraslation = VDCB.YTraslation + RowHeight;

                        VDCB.ContatoreRiga++;

                        if (VDCB.ContatoreRiga == 1)
                            break;
                    }
                    VDCB.ContatoreColonna++;

                    if (VDCB.ContatoreColonna != 0)
                    { VDCB.XTraslation = VDCB.XTraslation + (float)ReportSetting.RaggruppamentiDatasource.ElementAt(ContatoreGruppo).Indent + VDCB.Width; }
                    else { VDCB.XTraslation = (float)ReportSetting.RaggruppamentiDatasource.ElementAt(ContatoreGruppo).Indent; }
                    VDCB.YTraslation = 0;
                    VDCB.ContatoreRiga = 0;
                }
            }
        }
        private void AddObjectToHeaderGroups(int IndiceGruppo, GroupHeaderBand GroupHeaderBand, GroupHeaderBand groupGerarchico1 = null)
        {
            VariabiliDiCicloBanda VDCB = new VariabiliDiCicloBanda(0, 0, 0, 0, 0, 0);

            if (ReportSetting.Teste != null)
            {
                if (ReportSetting.Teste.Count != 0)
                {
                    if (!ReportSetting.Teste.ElementAt(IndiceGruppo).IsBandRaggrupamentoEmpty)
                    {
                        if (GroupHeaderBand != null)
                        {
                            int PositionVerticalTreeRaggruppamento = ReportSetting.Teste.ElementAt(IndiceGruppo).PositionVerticalTreeRaggruppamento;
                            if (PositionVerticalTreeRaggruppamento != -1)
                                CreateParentInRaggruppamento(PositionVerticalTreeRaggruppamento, ReportSetting.Teste.ElementAt(IndiceGruppo).RaggruppamentiDocumento);
                        }

                        AddObjectToGroupsReWrite(ReportSetting.Teste.ElementAt(IndiceGruppo).RaggruppamentiDocumento, IndiceGruppo, GroupHeaderBand, null);
                    }
                }
            }

            if (!string.IsNullOrEmpty(groupGerarchico1.Name))
            {
                if (groupGerarchico1 != null)
                {
                    if (groupGerarchico1.NestedGroup == null)
                    {
                        groupGerarchico1.NestedGroup = GroupHeaderBand;
                    }
                    else
                    {
                        if (groupGerarchico1.NestedGroup.NestedGroup == null)
                        {
                            groupGerarchico1.NestedGroup.NestedGroup = GroupHeaderBand;
                        }
                        else
                        {
                            if (groupGerarchico1.NestedGroup.NestedGroup.NestedGroup == null)
                            {
                                groupGerarchico1.NestedGroup.NestedGroup.NestedGroup = GroupHeaderBand;
                            }
                            else
                            {
                                if (groupGerarchico1.NestedGroup.NestedGroup.NestedGroup.NestedGroup == null)
                                {
                                    groupGerarchico1.NestedGroup.NestedGroup.NestedGroup.NestedGroup = GroupHeaderBand;
                                }
                                else
                                {
                                    if (groupGerarchico1.NestedGroup.NestedGroup.NestedGroup.NestedGroup.NestedGroup == null)
                                    {
                                        groupGerarchico1.NestedGroup.NestedGroup.NestedGroup.NestedGroup.NestedGroup = GroupHeaderBand;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        private void AddObjectToFooterGroups(int IndiceGruppo, GroupFooterBand group)
        {
            VariabiliDiCicloBanda VDCB = new VariabiliDiCicloBanda(0, 0, 0, 0, 0, 0);

            if (ReportSetting.Code.Count != 0)
            {
                if (!ReportSetting.Code.ElementAt(IndiceGruppo).IsBandRaggrupamentoEmpty)
                {
                    AddObjectToGroupsReWrite(ReportSetting.Code.ElementAt(IndiceGruppo).RaggruppamentiDocumento, IndiceGruppo, null, group);
                }
            }
        }

        //they work only on the last group
        //private GroupFooterBand BandaUltimoRaggruppamentoRiporti;
        //private List<string> ListaRiportiTotaliBandaUltimoRaggruppamento = new List<string>();
        //they work on all group
        private Dictionary<int, GroupFooterBand> DictionaryBandaRaggruppamento = new Dictionary<int, GroupFooterBand>();
        private Dictionary<int, List<string>> DictionarySommePerRaggrupamento = new Dictionary<int, List<string>>();
        private void AddObjectToGroupsReWrite(List<RaggruppamentiDocumento> AttributiColonna, int IndiceGruppo, GroupHeaderBand GroupHeaderBand, GroupFooterBand GroupFooterBand)
        {
            if (GroupFooterBand != null)
            {
                DictionaryBandaRaggruppamento.Add(IndiceGruppo, GroupFooterBand);
                DictionarySommePerRaggrupamento.Add(IndiceGruppo, new List<string>());
            }


            //ListaRiportiTotaliBandaUltimoRaggruppamento = new List<string>();
            VariabiliDiCicloBanda VDCB = new VariabiliDiCicloBanda(0, 0, 0, 0, 0, 0);

            bool AddObjectToBand = true;
            int Colonne = ReportSetting.Intestazioni.Count();
            int Righe = AttributiColonna.FirstOrDefault().RaggruppamentiValori.Count();
            int ContatoreChild = 0;

            for (int R = 0; R < Righe; R++)
            {
                float RowHeight = (float)1 / 2;
                VDCB.ContatoreColonna = 0;
                ContatoreChild++;
                ChildBand childBand = new ChildBand();
                childBand.CanGrow = true;
                childBand.CanShrink = true;
                childBand.CanBreak = true;
                childBand.Height = DefaultHeightBand / 2;

                for (int C = 0; C < Colonne; C++)
                {
                    RaggruppamentiValori Attributo = AttributiColonna[C].RaggruppamentiValori[R];
                    VDCB.Width = (Units.Centimeters * (float)AttributiColonna[C].Size);

                    if (!string.IsNullOrEmpty(Attributo.Attributo))
                    {
                        TextObject GroupText = new TextObject();
                        GroupText.HorzAlign = HorzAlign.Left;
                        GroupText.CanGrow = true;
                        string TextAttributo = null;

                        VDCB.WidthEtichetta = VDCB.Width / 2;
                        bool HasEtichetta = false;
                        if (!string.IsNullOrEmpty(FormatEtichettaIfIsEmpty(Attributo.Etichetta)))
                        {
                            TextObject Etichetta = CreaEtichetta(Attributo, VDCB.XTraslation, VDCB.YTraslation, VDCB.WidthEtichetta);
                            Etichetta.HorzAlign = HorzAlign.Left;

                            if (!Attributo.ConcatenaEtichettaEValore)
                            {
                                HasEtichetta = true;
                                if (Bordatura)
                                {
                                    CreaBordo(Etichetta, true, false, false, false);
                                    if (Attributo.StileCarattere.TextVerticalAlignementCode == 2)
                                        Etichetta.WordWrap = false;
                                    Etichetta.GrowToBottom = true;
                                }
                                childBand.Objects.Add(Etichetta);
                            }
                            else
                            {
                                TextAttributo = Etichetta.Text;
                                TextAttributo = TextAttributo + " ";
                            }
                        }

                        if (Attributo.PropertyType == MasterDetailModel.BuiltInCodes.DefinizioneAttributo.TestoRTF)
                        {
                            if (Attributo.Rtf || ReportSetting.IsAllFieldRtfFormat)
                            {
                                RichObject RichText = null;
                                if (HasEtichetta)
                                    RichText = CreateRichObject("Text" + JReportHelper.ReplaceSymbolNotAllowInReport(Attributo.AttributoPerReport), DbName + "." + JReportHelper.ReplaceSymbolNotAllowInReport(Attributo.AttributoPerReport), VDCB.XTraslation + VDCB.WidthEtichetta, Units.Centimeters * VDCB.YTraslation, VDCB.WidthEtichetta, DefaultHeightTextBox);
                                else
                                    RichText = CreateRichObject("Text" + JReportHelper.ReplaceSymbolNotAllowInReport(Attributo.AttributoPerReport), DbName + "." + JReportHelper.ReplaceSymbolNotAllowInReport(Attributo.AttributoPerReport), VDCB.XTraslation, Units.Centimeters * VDCB.YTraslation, VDCB.Width, DefaultHeightTextBox);
                                
                                RichText.GrowToBottom = true;//AU 19/09/2023
                                RichText.CanShrink = true;
                                RichText.CanGrow = true;
                                if (Bordatura)
                                {
                                    if (HasEtichetta)
                                    {
                                        CreaBordo(RichText, false, false, true, false);
                                    }
                                    else
                                    {
                                        CreaBordo(RichText, true, false, true, false);
                                    }
                                }
                                childBand.Objects.Add(RichText);
                                VDCB.XTraslation = VDCB.XTraslation + VDCB.Width;
                                VDCB.ContatoreColonna++;
                                continue;
                            }
                        }

                        if (HasEtichetta == false)
                        {
                            GroupText.Bounds = new RectangleF(VDCB.XTraslation, Units.Centimeters * VDCB.YTraslation, VDCB.Width, DefaultHeightTextBox);
                        }
                        else
                        {
                            GroupText.Bounds = new RectangleF(VDCB.XTraslation + VDCB.WidthEtichetta, Units.Centimeters * VDCB.YTraslation, VDCB.Width - VDCB.WidthEtichetta, DefaultHeightTextBox);
                        }

                        SettingFontForTextObject(GroupText, Attributo.StileCarattere, Bordatura);

                        if (HasEtichetta)
                            if (Bordatura)
                                CreaBordo(GroupText, false, false, true, false);

                        if (Attributo.CodiceDigicorp == StampeKeys.ConstSommaWizard || Attributo.CodiceDigicorp == StampeKeys.ConstSommaStrutturaWizard || Attributo.CodiceDigicorp == StampeKeys.ConstContaWizard)
                        {
                            Total Total = new Total();
                            string TotalName = null;
                            string Prefisso = null;
                            if (Attributo.CodiceDigicorp == StampeKeys.ConstSommaWizard || Attributo.CodiceDigicorp == StampeKeys.ConstSommaStrutturaWizard) { Prefisso = "TotalOf"; }
                            else { Prefisso = "ContaOf"; }
                            TotalName = Prefisso + GroupHeaderBand?.Name + GroupFooterBand?.Name;
                            TotalName = TotalName + JReportHelper.ReplaceSymbolNotAllowInReport(Attributo.AttributoPerReport) + IndexDb;

                            //PER RIPORTI E A RIPORTARE
                            if (ReportObject.Dictionary.Totals.FindByName(TotalName) == null && !string.IsNullOrEmpty(Attributo.Attributo))
                            {
                                if (Attributo.CodiceDigicorp == StampeKeys.ConstSommaWizard || Attributo.CodiceDigicorp == "SommaStruttura") { Total.Name = TotalName; Total.TotalType = TotalType.Sum; Total.Expression = "[" + DbName + "." + JReportHelper.ReplaceSymbolNotAllowInReport(Attributo.AttributoPerReport) + "]"; }
                                if (Attributo.CodiceDigicorp == StampeKeys.ConstContaWizard) { Total.Name = TotalName; Total.TotalType = TotalType.Count; }
                                Total.Evaluator = Data;
                                if (GroupHeaderBand != null) { Total.PrintOn = GroupHeaderBand; } else { Total.PrintOn = GroupFooterBand; }
                                ReportObject.Dictionary.Totals.Add(Total);
                            }

                            TotalName = Prefisso + GroupHeaderBand?.Name.Replace(StampeKeys.ConstGroupHeader, "Child" + ContatoreChild) + GroupFooterBand?.Name.Replace(StampeKeys.ConstGroupFooter, "Child" + ContatoreChild);
                            TotalName = TotalName + JReportHelper.ReplaceSymbolNotAllowInReport(Attributo.AttributoPerReport) + IndexDb;
                            Total = new Total();

                            if (ReportObject.Dictionary.Totals.FindByName(TotalName) == null && !string.IsNullOrEmpty(Attributo.Attributo))
                            {
                                if (Attributo.CodiceDigicorp == StampeKeys.ConstSommaWizard || Attributo.CodiceDigicorp == "SommaStruttura") { Total.Name = TotalName; Total.TotalType = TotalType.Sum; Total.Expression = "[" + DbName + "." + JReportHelper.ReplaceSymbolNotAllowInReport(Attributo.AttributoPerReport) + "]"; }
                                if (Attributo.CodiceDigicorp == StampeKeys.ConstContaWizard) { Total.Name = TotalName; Total.TotalType = TotalType.Count; }
                                Total.Evaluator = Data;
                                Total.PrintOn = childBand;
                                ReportObject.Dictionary.Totals.Add(Total);
                            }
                            else
                            {
                                AddObjectToBand = false;
                            }

                            GroupText.Name = "Child" + ContatoreChild + "TextAttributoValoreTotal" + JReportHelper.ReplaceSymbolNotAllowInReport(Attributo.AttributoPerReport);
                            GroupText.Text = "[" + TotalName + "]";

                            if (FormatoNumero.Contains(Attributo.PropertyType) || FormatoValuta.Contains(Attributo.PropertyType))
                                RetrieveFormatFromAttribute(JReportHelper.ReplaceSymbolNotAllowInReport(Attributo.AttributoPerReport), GroupText);

                            //IDENTIFICA ULTIMO RAGGRUPPAMENTO

                            if (AddObjectToBand)
                            {
                                if (GroupFooterBand != null)
                                {
                                    DictionarySommePerRaggrupamento[IndiceGruppo].Add(TotalName);
                                    GroupText.AfterPrint += GroupTextTotal_AfterPrint;

                                    if (ListaCelleCorpo.ElementAt(C).Nascondi && ListaCelleCorpo.ElementAt(C).RiportoRaggruppamento)
                                    {
                                        if (ListOggettiPerBanda.Where(g => g.ColumnIndex == C).FirstOrDefault() == null)
                                        {
                                            if (ListaCelleCorpo.ElementAt(C).AttributoPerReport == Attributo.AttributoPerReport)
                                            {
                                                RiportoTotalProperties riportoTotalProperties = new RiportoTotalProperties();
                                                riportoTotalProperties.ColumnIndex = C;
                                                riportoTotalProperties.EntityAttributo = Attributo.AttributoPerReport;
                                                riportoTotalProperties.TotalName = TotalName;
                                                riportoTotalProperties.TextObjectTotalName = GroupText.Name;
                                                riportoTotalProperties.BandName = GroupFooterBand.Name;
                                                riportoTotalProperties.BandIndex = IndiceGruppo;
                                                ListOggettiPerBanda.Add(riportoTotalProperties);
                                            }
                                        }
                                        else
                                        {
                                            if (ListaCelleCorpo.ElementAt(C).AttributoPerReport == Attributo.AttributoPerReport)
                                            {
                                                ListOggettiPerBanda.Where(g => g.ColumnIndex == C).FirstOrDefault().EntityAttributo = Attributo.AttributoPerReport;
                                                ListOggettiPerBanda.Where(g => g.ColumnIndex == C).FirstOrDefault().TotalName = TotalName;
                                                ListOggettiPerBanda.Where(g => g.ColumnIndex == C).FirstOrDefault().TextObjectTotalName = GroupText.Name;
                                                ListOggettiPerBanda.Where(g => g.ColumnIndex == C).FirstOrDefault().BandName = GroupFooterBand.Name;
                                                ListOggettiPerBanda.Where(g => g.ColumnIndex == C).FirstOrDefault().BandIndex = IndiceGruppo;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (!Attributo.StampaFormula)
                            {
                                GroupText.Name = "Child" + ContatoreChild + "TextAttributoValore" + JReportHelper.ReplaceSymbolNotAllowInReport(Attributo.AttributoPerReport);
                                GroupText.Text = "[" + DbName + "." + JReportHelper.ReplaceSymbolNotAllowInReport(Attributo.AttributoPerReport) + "]";
                            }
                            else
                            {
                                GroupText.Name = "Child" + ContatoreChild + "TextAttributoValore" + JReportHelper.ReplaceSymbolNotAllowInReport(Attributo.AttributoPerReport + "Formula");
                                GroupText.Text = "[" + DbName + "." + JReportHelper.ReplaceSymbolNotAllowInReport(Attributo.AttributoPerReport + "Formula") + "]";
                            }

                            if (FormatoNumero.Contains(Attributo.PropertyType) || FormatoValuta.Contains(Attributo.PropertyType))
                            {
                                RetrieveFormatFromAttribute(JReportHelper.ReplaceSymbolNotAllowInReport(Attributo.AttributoPerReport), GroupText);
                            }
                        }

                        GroupText.CanShrink = true;

                        GroupText.Text = TextAttributo + GroupText.Text;

                        if (!AddObjectToBand)
                            GroupText.Text = "";

                        if (Attributo.StileCarattere != null)
                            if (Attributo.StileCarattere.TextVerticalAlignementCode == 2)
                                AggiungiEtichettaVuotaPerAncoraggioBasso(childBand, GroupText, HasEtichetta);

                        childBand.Objects.Add(GroupText);
                        AddObjectToBand = true;
                    }
                    else
                    {
                        TextObject Etichetta = CreaEtichetta(Attributo, VDCB.XTraslation, VDCB.YTraslation, VDCB.Width);
                        SettingFontForTextObject(Etichetta, Attributo.StileCarattere, Bordatura);
                        Etichetta.CanShrink = true;
                        childBand.Objects.Add(Etichetta);
                    }

                    VDCB.XTraslation = VDCB.XTraslation + VDCB.Width;
                    VDCB.ContatoreColonna++;
                }

                VDCB.XTraslation = 0;
                VDCB.YTraslation = 0;

                VDCB.ContatoreRiga++;

                if (GroupHeaderBand != null)
                {
                    if (GroupHeaderBand.Child == null)
                        GroupHeaderBand.Child = childBand;
                    else
                        FindLastChild(GroupHeaderBand.Child).AddChild(childBand);
                }
                else
                {
                    if (GroupFooterBand.Child == null)
                        GroupFooterBand.Child = childBand;
                    else
                        FindLastChild(GroupFooterBand.Child).AddChild(childBand);
                }
            }

        }

        private void GroupTextTotal_AfterPrint(object sender, EventArgs e)
        {
            string TotalName = (sender as TextObject).Name;
            RiportoTotalProperties riportoTotalPropertiesFound = null;
            int indexProperties = 0;

            foreach (RiportoTotalProperties riportoTotalProperties in ListOggettiPerBanda)
            {
                if (TotalName == riportoTotalProperties.TextObjectTotalName)
                {
                    riportoTotalPropertiesFound = riportoTotalProperties;
                    indexProperties++;
                    break;
                }
            }

            if (riportoTotalPropertiesFound == null)
                return;

            ReportParameters.ElementAt(riportoTotalPropertiesFound.ColumnIndex).RiportoValue = ReportParameters.ElementAt(riportoTotalPropertiesFound.ColumnIndex).RiportoValue + ReportObject.GetTotalValue(riportoTotalPropertiesFound.TotalName);
            ReportParameters.ElementAt(riportoTotalPropertiesFound.ColumnIndex).RiportareValue = ReportParameters.ElementAt(riportoTotalPropertiesFound.ColumnIndex).RiportareValue + ReportObject.GetTotalValue(riportoTotalPropertiesFound.TotalName);
            ReportObject.SetParameterValue(ReportParameters.ElementAt(riportoTotalPropertiesFound.ColumnIndex).RetrieveParameterString(true), RetieveFormattingValue(riportoTotalPropertiesFound.EntityAttributo, ReportParameters.ElementAt(riportoTotalPropertiesFound.ColumnIndex).RiportoValue));
            ReportObject.SetParameterValue(ReportParameters.ElementAt(riportoTotalPropertiesFound.ColumnIndex).RetrieveParameterString(false), RetieveFormattingValue(riportoTotalPropertiesFound.EntityAttributo, ReportParameters.ElementAt(riportoTotalPropertiesFound.ColumnIndex).RiportareValue));
        }

        public string RetieveFormattingValue(string entityAttributo, double value)
        {
            string entityAttributoWithNoSpecialCharachter = JReportHelper.ReplaceSymbolNotAllowInReport(entityAttributo);
            int Index = ((System.Data.DataRow)ReportObject.Dictionary.DataSources[IndexDb].CurrentRow).Table.Columns["V_DDC_CDlS_CGS" + entityAttributoWithNoSpecialCharachter].Ordinal;
            var formattazione = (string)(((System.Data.DataRow)ReportObject.Dictionary.DataSources[IndexDb].CurrentRow).ItemArray.ElementAt(Index));
            string valore = null;
            if (value != 0)
                valore = String.Format("{0:" + formattazione + @"}", value);
            else
                valore = "";
           
            return valore;
        }

        private void AggiungiEtichettaVuotaPerAncoraggioBasso(BandBase BandBase, TextObject TextObject, bool HasEtichetta)
        {
            TextObject Etichetta = CreaEtichetta(new CorpoDocumento(), TextObject.Bounds.X, TextObject.Bounds.Y, TextObject.Bounds.Width);
            if (Bordatura)
            {
                TextObject.GrowToBottom = false;
                if (HasEtichetta)
                    CreaBordo(Etichetta, false, false, true, false);
                else
                    CreaBordo(Etichetta, true, false, true, false);
                Etichetta.GrowToBottom = true;
                TextObject.WordWrap = false;
                BandBase.Objects.Add(Etichetta);
            }
        }

        private void CreateParentInRaggruppamento(int PositionVerticalTreeDocumento, List<RaggruppamentiDocumento> RaggruppamentiDocumento)
        {
            VariabiliDiCicloBanda VDCB = new VariabiliDiCicloBanda(0, 0, 0, 0, 0, 0);
            List<RaggruppamentiValori> ListaOrdinataNew = new List<RaggruppamentiValori>();
            int Indice = 1;
            AggiungiRicorsivamentePadriRaggruppamento(ListaOrdinataNew, Indice, PositionVerticalTreeDocumento, RaggruppamentiDocumento);
            VDCB.ContatoreColonna = 0;

            foreach (var Colonna in RaggruppamentiDocumento)
            {
                foreach (var Cella in ListaOrdinataNew)
                {
                    if (VDCB.ContatoreColonna == PositionVerticalTreeDocumento)
                    {
                        Colonna.RaggruppamentiValori.Insert(VDCB.ContatoreRiga, Cella);
                    }
                    else
                    {
                        RaggruppamentiValori raggruppamentiValori = new RaggruppamentiValori();
                        raggruppamentiValori.StileCarattere = new ProprietaCarattere();
                        raggruppamentiValori.StileCarattere.FontFamily = "Segoe UI";
                        raggruppamentiValori.StileCarattere.Size = "9";
                        Colonna.RaggruppamentiValori.Insert(VDCB.ContatoreRiga, raggruppamentiValori);
                    }
                    VDCB.ContatoreRiga++;
                }
                VDCB.ContatoreColonna++;
                VDCB.ContatoreRiga = 0;
            }
        }

        private void AggiungiRicorsivamentePadriRaggruppamento(List<RaggruppamentiValori> ListaOrdinataNew, int Indice, int PositionVerticalTreeDocumento, List<RaggruppamentiDocumento> RaggruppamentiDocumento)
        {
            int IndiceLocal = Indice;
            int ContatoreColonna = 0;
            int PositionVerticalTreeDocumentoLocal = PositionVerticalTreeDocumento;
            bool NonAggiungere = false;

            foreach (var Colonna in RaggruppamentiDocumento)
            {
                if (ContatoreColonna == PositionVerticalTreeDocumento)
                {
                    foreach (var Cella in Colonna.RaggruppamentiValori)
                    {
                        if (!NonAggiungere)
                        {
                            //if (Cella.AttributoCodice == MasterDetailModel.BuiltInCodes.Attributo.Codice || Cella.AttributoCodice == MasterDetailModel.BuiltInCodes.Attributo.DescrizioneRTF || Cella.AttributoCodice == MasterDetailModel.BuiltInCodes.Attributo.Nome)
                            if (entitiesHelper.IsAttributoDeep(Cella.EntityType, Cella.AttributoCodice))
                            {
                                string AttributoCodice = JReportHelper.ReplaceSymbolNotAllowInReport(Cella.EntityType + Cella.AttributoCodice + IndiceLocal);
                                if (ListParentItem.Where(x => x.Attributo1 == AttributoCodice || x.Attributo2 == AttributoCodice || x.Attributo3 == AttributoCodice).FirstOrDefault() == null)
                                {
                                    return;
                                }
                                RaggruppamentiValori raggruppamentiValori = new RaggruppamentiValori();
                                raggruppamentiValori.Attributo = Cella.Attributo;
                                raggruppamentiValori.AttributoCodice = Cella.AttributoCodice + IndiceLocal;
                                raggruppamentiValori.CodiceDigicorp = Cella.CodiceDigicorp;
                                raggruppamentiValori.ConcatenaEtichettaEValore = Cella.ConcatenaEtichettaEValore;
                                raggruppamentiValori.DivisioneCodice = Cella.DivisioneCodice;
                                raggruppamentiValori.EntityType = Cella.EntityType;
                                raggruppamentiValori.Etichetta = Cella.Etichetta;
                                raggruppamentiValori.PropertyType = Cella.PropertyType;
                                raggruppamentiValori.Rtf = Cella.Rtf;
                                raggruppamentiValori.StileCarattere = Cella.StileCarattere;
                                ListaOrdinataNew.Add(raggruppamentiValori);
                            }
                            else
                                NonAggiungere = true;
                        }
                    }
                }
                ContatoreColonna++;
            }

            IndiceLocal++;
            AggiungiRicorsivamentePadriRaggruppamento(ListaOrdinataNew, IndiceLocal, PositionVerticalTreeDocumentoLocal, RaggruppamentiDocumento);
        }


        private void AssingDataSourceToGroup(GroupHeaderBand LatsGroup, GroupHeaderBand FirstGroup)
        {
            if (LatsGroup == null)
            {
                FirstGroup.Data = Data;
                Page.Bands.Add(FirstGroup);
            }
            else
            {
                LatsGroup.Data = Data;
                Page.Bands.Add(FirstGroup);
            }
        }
        private void AddObjectToDataBand()
        {
            if (ReportSetting.IsBandDocumentoEmpty)
                return;

            int PositionVerticalTreeDocumento = ReportSetting.PositionVerticalTreeDocumento;
            if (PositionVerticalTreeDocumento != -1)
                CreateParentInDocumento(PositionVerticalTreeDocumento);

            VariabiliDiCicloBanda VDCB = new VariabiliDiCicloBanda(0, 0, 0, 0, 0, 0);
            int Colonne = ReportSetting.Intestazioni.Count();
            int Righe = ReportSetting.CorpiDocumento.FirstOrDefault().CorpoColonna.Count();
            int ContatoreChild = 0;

            for (int R = 0; R < Righe; R++)
            {
                float RowHeight = Units.Centimeters * (float)1 / 2;
                VDCB.ContatoreColonna = 0;
                ContatoreChild++;
                ChildBand childBand = new ChildBand();
                childBand.CanGrow = true;
                childBand.CanShrink = true;
                childBand.CanBreak = true;
                childBand.Height = DefaultHeightBand / 2;

                for (int C = 0; C < Colonne; C++)
                {
                    CorpoDocumento Attributo = ReportSetting.CorpiDocumento[C].CorpoColonna[R];
                    VDCB.Width = (Units.Centimeters * (float)ReportSetting.CorpiDocumento[C].Size);

                    if (Attributo.Nascondi)
                    {
                        TextObject Etichetta = CreaEtichetta(Attributo, VDCB.XTraslation, VDCB.YTraslation, VDCB.Width);
                        if (Bordatura)
                        {
                            CreaBordo(Etichetta, true, false, true, false);
                            Etichetta.GrowToBottom = true;
                        }
                        Etichetta.CanShrink = true;
                        //childBand.Objects.Add(Etichetta);
                        Data.Objects.Add(Etichetta);
                        goto EndFormatting;
                    }

                    if (!String.IsNullOrEmpty(Attributo.Attributo))
                    {
                        TextObject Text = null;

                        VDCB.WidthEtichetta = VDCB.Width / 2;
                        bool HasEtichetta = false;

                        string TextAttributo = null;

                        if (!string.IsNullOrEmpty(FormatEtichettaIfIsEmpty(Attributo.Etichetta)))
                        {
                            TextObject Etichetta = CreaEtichetta(Attributo, VDCB.XTraslation, VDCB.YTraslation, VDCB.WidthEtichetta);
                            Etichetta.HorzAlign = HorzAlign.Left;
                            if (!Attributo.ConcatenaEtichettaEValore)
                            {
                                HasEtichetta = true;
                                if (Bordatura)
                                {
                                    CreaBordo(Etichetta, true, false, false, false);
                                    if (Attributo.StileCarattere.TextVerticalAlignementCode == 2)
                                        Etichetta.WordWrap = false;
                                    Etichetta.GrowToBottom = true;
                                }
                                Etichetta.CanShrink = true;
                                //childBand.Objects.Add(Etichetta);
                                Data.Objects.Add(Etichetta);
                            }
                            else
                            {
                                TextAttributo = Etichetta.Text;
                                TextAttributo = TextAttributo + " ";
                            }
                        }

                        if (Attributo.PropertyType == MasterDetailModel.BuiltInCodes.DefinizioneAttributo.TestoRTF)
                        {
                            if (Attributo.Rtf || ReportSetting.IsAllFieldRtfFormat)
                            {
                                RichObject RichText = null;
                                if (HasEtichetta)
                                    RichText = CreateRichObject("Text" + JReportHelper.ReplaceSymbolNotAllowInReport(Attributo.AttributoPerReport), DbName + "." + JReportHelper.ReplaceSymbolNotAllowInReport(Attributo.AttributoPerReport), VDCB.XTraslation + VDCB.WidthEtichetta, VDCB.YTraslation, VDCB.WidthEtichetta, DefaultHeightTextBox);
                                else
                                    RichText = CreateRichObject("Text" + JReportHelper.ReplaceSymbolNotAllowInReport(Attributo.AttributoPerReport), DbName + "." + JReportHelper.ReplaceSymbolNotAllowInReport(Attributo.AttributoPerReport), VDCB.XTraslation, VDCB.YTraslation, VDCB.Width, DefaultHeightTextBox);
                                RichText.GrowToBottom = false;
                                if (Bordatura)
                                {
                                    if (HasEtichetta)
                                    {
                                        CreaBordo(RichText, false, false, true, false);
                                    }
                                    else
                                    {
                                        CreaBordo(RichText, true, false, true, false);
                                    }
                                }
                                RichText.CanShrink = true;

                                //childBand.Objects.Add(RichText);
                                Data.Objects.Add(RichText);
                                goto EndFormatting;
                            }
                        }

                        if (HasEtichetta == false)
                        {
                            if (Attributo.StampaFormula)
                                Text = CreateTextObject("TextAttributoValore" + JReportHelper.ReplaceSymbolNotAllowInReport(Attributo.AttributoPerReport + "Formula"), "[" + DbName + "." + JReportHelper.ReplaceSymbolNotAllowInReport(Attributo.AttributoPerReport + "Formula") + "]", VDCB.XTraslation, VDCB.YTraslation, VDCB.Width, DefaultHeightTextBox);
                            else
                                Text = CreateTextObject("TextAttributoValore" + JReportHelper.ReplaceSymbolNotAllowInReport(Attributo.AttributoPerReport), "[" + DbName + "." + JReportHelper.ReplaceSymbolNotAllowInReport(Attributo.AttributoPerReport) + "]", VDCB.XTraslation, VDCB.YTraslation, VDCB.Width, DefaultHeightTextBox);


                        }
                        else
                        {
                            if (Attributo.StampaFormula)
                                Text = CreateTextObject("TextAttributoValore" + JReportHelper.ReplaceSymbolNotAllowInReport(Attributo.AttributoPerReport + "Formula"), "[" + DbName + "." + JReportHelper.ReplaceSymbolNotAllowInReport(Attributo.AttributoPerReport + "Formula") + "]", VDCB.XTraslation + VDCB.WidthEtichetta, VDCB.YTraslation, VDCB.Width - VDCB.WidthEtichetta, DefaultHeightTextBox);
                            else
                                Text = CreateTextObject("TextAttributoValore" + JReportHelper.ReplaceSymbolNotAllowInReport(Attributo.AttributoPerReport), "[" + DbName + "." + JReportHelper.ReplaceSymbolNotAllowInReport(Attributo.AttributoPerReport) + "]", VDCB.XTraslation + VDCB.WidthEtichetta, VDCB.YTraslation, VDCB.Width - VDCB.WidthEtichetta, DefaultHeightTextBox);
                        }

                        SettingFontForTextObject(Text, Attributo.StileCarattere, Bordatura);

                        if (HasEtichetta)
                            if (Bordatura)
                                CreaBordo(Text, false, false, true, false);

                        if (FormatoNumero.Contains(Attributo.PropertyType) || FormatoValuta.Contains(Attributo.PropertyType))
                            RetrieveFormatFromAttribute(JReportHelper.ReplaceSymbolNotAllowInReport(Attributo.AttributoPerReport), Text);

                        if (Attributo.StileCarattere != null)
                            if (Attributo.StileCarattere.TextVerticalAlignementCode == 2)
                                AggiungiEtichettaVuotaPerAncoraggioBasso(Data, Text, HasEtichetta);

                        Text.Text = TextAttributo + Text.Text;
                        Text.CanShrink = true;
                        //Text.AfterData += CellaCorpoDocumento_AfterData;
                        //childBand.Objects.Add(Text);
                        Data.Objects.Add(Text);
                    }
                    else
                    {
                        TextObject Etichetta = CreaEtichetta(Attributo, VDCB.XTraslation, VDCB.YTraslation, VDCB.Width);
                        if (Bordatura)
                        {
                            CreaBordo(Etichetta, true, false, true, false);
                            Etichetta.GrowToBottom = true;
                        }
                        //GESTIONE DIFFERENTE TRA GRUPPI E DATABAND, NELLA DATABAND NON POSSO USARE I CHILD QUINDI SONO COSTRETTO A MANTENERE I VUOIT SE CI SONO
                        //Etichetta.CanShrink = true;
                        //childBand.Objects.Add(Etichetta);
                        Data.Objects.Add(Etichetta);
                    }

                EndFormatting:


                    VDCB.XTraslation = VDCB.XTraslation + VDCB.Width;
                    VDCB.ContatoreColonna++;
                }

                VDCB.XTraslation = 0;
                //VDCB.YTraslation = 0;
                VDCB.YTraslation = VDCB.YTraslation + RowHeight;

                VDCB.ContatoreRiga++;

                //if (Data.Child == null)
                //    Data.Child = childBand;
                //else
                //    FindLastChild(Data.Child).AddChild(childBand);
            }
        }

        private void CreateParentInDocumento(int PositionVerticalTreeDocumento)
        {
            VariabiliDiCicloBanda VDCB = new VariabiliDiCicloBanda(0, 0, 0, 0, 0, 0);
            List<CorpoDocumento> ListaOrdinataNew = new List<CorpoDocumento>();
            int Indice = 1;
            AggiungiRicorsivamentePadriDocumento(ListaOrdinataNew, Indice, PositionVerticalTreeDocumento);
            VDCB.ContatoreColonna = 0;

            foreach (var Colonna in ReportSetting.CorpiDocumento)
            {
                foreach (var Cella in ListaOrdinataNew)
                {
                    if (VDCB.ContatoreColonna == PositionVerticalTreeDocumento)
                    {
                        Colonna.CorpoColonna.Insert(VDCB.ContatoreRiga, Cella);
                    }
                    else
                    {
                        CorpoDocumento corpoDocumento = new CorpoDocumento();
                        corpoDocumento.StileCarattere = new ProprietaCarattere();
                        corpoDocumento.StileCarattere.FontFamily = "Segoe UI";
                        corpoDocumento.StileCarattere.Size = "9";
                        Colonna.CorpoColonna.Insert(VDCB.ContatoreRiga, corpoDocumento);
                    }
                    VDCB.ContatoreRiga++;
                }
                VDCB.ContatoreColonna++;
                VDCB.ContatoreRiga = 0;
            }
        }

        private void AggiungiRicorsivamentePadriDocumento(List<CorpoDocumento> ListaOrdinataNew, int Indice, int PositionVerticalTreeDocumento)
        {
            int IndiceLocal = Indice;
            int ContatoreColonna = 0;
            int PositionVerticalTreeDocumentoLocal = PositionVerticalTreeDocumento;
            bool NonAggiungere = false;

            foreach (var Colonna in ReportSetting.CorpiDocumento)
            {
                if (ContatoreColonna == PositionVerticalTreeDocumento)
                {
                    foreach (var Cella in Colonna.CorpoColonna)
                    {
                        if (!NonAggiungere)
                        {
                            //if (Cella.AttributoCodice == MasterDetailModel.BuiltInCodes.Attributo.Codice || Cella.AttributoCodice == MasterDetailModel.BuiltInCodes.Attributo.DescrizioneRTF || Cella.AttributoCodice == MasterDetailModel.BuiltInCodes.Attributo.Nome)
                            if (entitiesHelper.IsAttributoDeep(Cella.EntityType, Cella.AttributoCodice))
                            {
                                string AttributoCodice = Cella.EntityType + Cella.AttributoCodice + IndiceLocal;
                                if (ListParentItem.Where(x => x.Attributo1 == AttributoCodice || x.Attributo2 == AttributoCodice || x.Attributo3 == AttributoCodice).FirstOrDefault() == null)
                                {
                                    return;
                                }
                                CorpoDocumento CorpoDocumento = new CorpoDocumento();
                                CorpoDocumento.Attributo = Cella.Attributo;
                                CorpoDocumento.AttributoCodice = Cella.AttributoCodice + IndiceLocal;
                                CorpoDocumento.CodiceDigicorp = Cella.CodiceDigicorp;
                                CorpoDocumento.ConcatenaEtichettaEValore = Cella.ConcatenaEtichettaEValore;
                                CorpoDocumento.DivisioneCodice = Cella.DivisioneCodice;
                                CorpoDocumento.EntityType = Cella.EntityType;
                                CorpoDocumento.Etichetta = Cella.Etichetta;
                                CorpoDocumento.PropertyType = Cella.PropertyType;
                                CorpoDocumento.Rtf = Cella.Rtf;
                                CorpoDocumento.StileCarattere = Cella.StileCarattere;
                                ListaOrdinataNew.Add(CorpoDocumento);
                            }
                            else
                                NonAggiungere = true;
                        }
                    }
                }
                ContatoreColonna++;
            }

            IndiceLocal++;
            AggiungiRicorsivamentePadriDocumento(ListaOrdinataNew, IndiceLocal, PositionVerticalTreeDocumentoLocal);
        }

        private void AddImageObjectToDataBand(ImageForPage imageForPage)
        {
            System.IO.MemoryStream ImageStream = new System.IO.MemoryStream();
            var bytes = Convert.FromBase64String(imageForPage.Image);
            ImageStream = new System.IO.MemoryStream(bytes);
            PictureObject ImageCorpo = new PictureObject();
            ImageCorpo.Name = "ImageCorpo" + IndexDb;
            float LarghezzaGantt;
            float AltezzaGantt;


            if (imageForPage.Size.IsEmpty)
            {
                if (Page.Landscape)
                {
                    LarghezzaGantt = Units.Millimeters * (ReportSetting.AltezzaPagina - ReportSetting.MargineSuperiore - ReportSetting.MargineInferiore - 1);
                    AltezzaGantt = Units.Millimeters * (ReportSetting.LarghezzaPagina - ReportSetting.MargineSinistro - ReportSetting.MargineDestro - (10 * 3));
                }
                else
                {
                    LarghezzaGantt = Units.Millimeters * (ReportSetting.LarghezzaPagina - ReportSetting.MargineSinistro - ReportSetting.MargineDestro - 1);
                    AltezzaGantt = Units.Millimeters * (ReportSetting.AltezzaPagina - ReportSetting.MargineSuperiore - ReportSetting.MargineInferiore - (10 * 3));
                }
                ImageCorpo.Bounds = new RectangleF(0, 0, LarghezzaGantt, AltezzaGantt);
            }
            else //uso le dimensioni calcolate per l'immagine
            {
                ImageCorpo.Bounds = new RectangleF(0, 0, Units.Centimeters * imageForPage.Size.Width, Units.Centimeters * imageForPage.Size.Height);
            }
        
                

            //ImageCorpo.Bounds = new RectangleF(0, 0, LarghezzaGantt, (float) (Units.Centimeters * 6.09));
            //ImageCorpo.Bounds = new RectangleF(0, 0, (float)(Units.Centimeters * 18.1398335), (float)(Units.Centimeters * 6.09599926));
            //ImageCorpo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            ImageCorpo.CanGrow = true;
            ImageCorpo.CanShrink = true;
            ImageCorpo.GrowToBottom = true;
            ImageCorpo.ImageAlign = ImageAlign.Top_Center;
            ImageCorpo.Image = Image.FromStream(ImageStream);
            Data.Objects.Add(ImageCorpo);
            Page.Bands.Add(Data);
        }

        private void AddRftObjectToDataBand()
        {
            if (!string.IsNullOrEmpty(ReportSetting.Corpo))
            {
                ReportSetting.Corpo = entitiesHelper.GetRtfPreview(ReportSetting.Corpo, RtfDataService);
            }
            RichObject RichTextCorpo = CreateRichObject("TextCorpo", "", 0, (Units.Centimeters * 1 / 2), MiddlePage * 2, DefaultHeightTextBox, @"" + ReportSetting.Corpo);
            Data.Objects.Add(RichTextCorpo);
            Page.Bands.Add(Data);
        }
        private void AddObjectToPageFooter(bool FromDocumento)
        {
            VariabiliDiCicloBanda VDCB = new VariabiliDiCicloBanda(0, 0, 0, 0, 0, 0);

            if (ReportSetting != null)
            {
                RichObject RichTextINte = new RichObject();
                TextObject TextInt = new TextObject();

                if (FromDocumento)
                {
                    RichTextINte = CreateRichObject("Text" + StampeKeys.ConstPiePagina + DbName, "", TraslationXRtf, VDCB.YTraslation, DimensionamentoRtf, DefaultHeightTextBox * 2, @"" + ReportSetting.PiePagina);
                    RichTextINte.CanShrink = false;
                    RichTextINte.CanGrow = false;
                    RichTextINte.CanBreak = false;
                    VDCB.XTraslation = VDCB.XTraslation + ThirdSpace;
                    Page.PageFooter.Objects.Add(RichTextINte);
                }
            }
        }

        List<TextObject> RiportiUltimaPaginaDaNascondere = new List<TextObject>();
        private void CreateColumnFooter(bool fromDocumento, bool IsRrfDocument)
        {
            VariabiliDiCicloBanda VDCB = new VariabiliDiCicloBanda(0, 0, 0, 0, 0, 0);

            string BaseName = "RiportareRaggruppamentoCalcolato";
            string AttributoPerReport = null;

            int Contatore = 0;

            bool NessunaTipologiadiRiporto = true;

            if (ListaCelleCorpo.Any(r => r.RiportoRaggruppamento))
            {
                NessunaTipologiadiRiporto = false;
                string ConditionRiporto = "[IIf(";
                int CounterColonna = 0;

                foreach (CellaTabella Cella in ListaCelleCorpo)
                {
                    AttributoPerReport = JReportHelper.ReplaceSymbolNotAllowInReport(Cella.AttributoPerReport);
                    bool TextBoxCreated = false;
                    VDCB.Width = (Units.Centimeters * (float)ReportSetting.CorpiDocumento.ElementAt(Contatore).Size);

                    if ((FormatoNumero.Contains(Cella.PropertyType) || FormatoValuta.Contains(Cella.PropertyType)) && Contatore != 1)
                    {
                        string TotalName = null;
                        bool useConditionHideValue = false;

                        if (!Cella.Nascondi)
                        {
                            foreach (var SommaPerRaggruppamento in DictionarySommePerRaggrupamento.Reverse())
                            {
                                if (SommaPerRaggruppamento.Value.Where(t => t.Contains(AttributoPerReport)).FirstOrDefault() != null)
                                {
                                    TotalName = SommaPerRaggruppamento.Value.Where(t => t.Contains(AttributoPerReport)).FirstOrDefault();
                                    TotalName = "[" + TotalName + "]";
                                    break;
                                }
                            }
                        }
                        else
                        {
                            foreach (var SommaPerRaggruppamento in DictionarySommePerRaggrupamento.Reverse())
                            {
                                if (SommaPerRaggruppamento.Value.Where(t => t.Contains(AttributoPerReport)).FirstOrDefault() != null)
                                {
                                    if (CounterColonna == 0) TotalName = "[" + RiportiVariables.PrimaColonnaRiportareString + "]";
                                    if (CounterColonna == 1) TotalName = "[" + RiportiVariables.SecondaColonnaRiportareString + "]";
                                    if (CounterColonna == 2) TotalName = "[" + RiportiVariables.TerzaColonnaRiportareString + "]";
                                    if (CounterColonna == 3) TotalName = "[" + RiportiVariables.QuartaColonnaRiportareString + "]";
                                    if (CounterColonna == 4) TotalName = "[" + RiportiVariables.QuintaColonnaRiportareString + "]";
                                    if (CounterColonna == 5) TotalName = "[" + RiportiVariables.SestaColonnaRiportareString + "]";
                                    if (CounterColonna == 6) TotalName = "[" + RiportiVariables.SettimaColonnaRiportareString + "]";
                                    if (CounterColonna == 7) TotalName = "[" + RiportiVariables.OttavaColonnaRiportareString + "]";
                                    if (CounterColonna == 8) TotalName = "[" + RiportiVariables.NonaColonnaRiportareString + "]";
                                    if (CounterColonna == 9) TotalName = "[" + RiportiVariables.DecimaColonnaRiportareString + "]";
                                    if (CounterColonna == 10) TotalName = "[" + RiportiVariables.UndicesimaColonnaRiportareString + "]";
                                    if (CounterColonna == 11) TotalName = "[" + RiportiVariables.DodicesimaColonnaRiportareString + "]";
                                    if (CounterColonna == 12) TotalName = "[" + RiportiVariables.TredicesimaColonnaRiportareString + "]";
                                    if (CounterColonna == 13) TotalName = "[" + RiportiVariables.QuattordicesimaColonnaRiportareString + "]";
                                    if (CounterColonna == 14) TotalName = "[" + RiportiVariables.QuindicesimaColonnaRiportareString + "]";
                                    if (CounterColonna == 15) TotalName = "[" + RiportiVariables.SedicesimaColonnaRiportareString + "]";
                                    if (CounterColonna == 16) TotalName = "[" + RiportiVariables.DiciassettesimaColonnaRiportareString + "]";
                                    if (CounterColonna == 17) TotalName = "[" + RiportiVariables.DiciottesimaColonnaRiportareString + "]";
                                    if (CounterColonna == 18) TotalName = "[" + RiportiVariables.DiciannovesimaColonnaRiportareString + "]";
                                    if (CounterColonna == 19) TotalName = "[" + RiportiVariables.VentesimaColonnaRiportareString + "]";
                                    useConditionHideValue = true;
                                    break;
                                }
                            }
                        }

                        if (!String.IsNullOrEmpty(TotalName))
                        {
                            TextObject Text = CreateTextObject(BaseName + AttributoPerReport, TotalName, VDCB.XTraslation, VDCB.YTraslation, VDCB.Width, HeightTextBoxColumnFooter);
                            Text.CanGrow = false; // true;//22/12/2023
                            Text.CanShrink = false;
                            Text.HorzAlign = HorzAlign.Right;
                            Text.Font = new Font("Segoe UI", 9, FontStyle.Italic);
                            Text.TextColor = Color.SteelBlue;
                            if (!useConditionHideValue)
                            {
                                string quote = "\"";
                                ConditionRiporto = ConditionRiporto + Text.Text + " == 0 &&";
                                Text.Text = "IIf(" + Text.Text + " == 0 ," + quote + quote + "," + Text.Text + ")";
                            }
                            Text.AfterData += Riporti_AfterData;
                            RetrieveFormatFromAttribute(AttributoPerReport, Text); //era commentato (non veniva aggiunto il formato)
                            if (Bordatura)
                            {
                                CreaBordo(Text, true, true, true, true);
                                Text.Border.TopLine.Color = Color.LightGray;
                                Text.Border.TopLine.Width = (float)0.25;
                            }
                            RiportiUltimaPaginaDaNascondere.Add(Text);
                            Page.ColumnFooter.Objects.Add(Text);
                            TextBoxCreated = true;
                        }
                    }

                    if (!TextBoxCreated)
                    {
                        TextObject Etichetta = CreaEtichetta(new CellaTabella(), VDCB.XTraslation, VDCB.YTraslation, VDCB.Width);
                        Etichetta.CanGrow = false;
                        Etichetta.CanShrink = false;
                        Etichetta.HorzAlign = HorzAlign.Right;
                        Etichetta.Font = new Font("Segoe UI", 9, FontStyle.Italic);
                        Etichetta.TextColor = Color.SteelBlue;
                        Etichetta.Height = HeightTextBoxColumnFooter;
                        Etichetta.AfterData += Riporti_AfterData;
                        if (Bordatura)
                        {
                            CreaBordo(Etichetta, true, true, true, true);
                            Etichetta.Border.TopLine.Color = Color.LightGray;
                            Etichetta.Border.TopLine.Width = (float)0.25;
                        }
                        Page.ColumnFooter.Objects.Add(Etichetta);
                    }

                    VDCB.XTraslation = VDCB.XTraslation + VDCB.Width;
                    Contatore++;
                    CounterColonna++;
                }

                Contatore = 0;
                VDCB.XTraslation = 0;
                ConditionRiporto = ConditionRiporto.Substring(0, ConditionRiporto.Length - 2);

                foreach (CellaTabella Cella in ListaCelleCorpo)
                {
                    AttributoPerReport = JReportHelper.ReplaceSymbolNotAllowInReport(Cella.AttributoPerReport);
                    bool TextBoxCreated = false;
                    VDCB.Width = (Units.Centimeters * (float)ReportSetting.CorpiDocumento.ElementAt(Contatore).Size);

                    if (Contatore == 1)
                    {
                        string Testo = LocalizationProvider.GetString("RiportareRaggruppamento");
                        //string quote = "\"";
                        //ConditionRiporto = ConditionRiporto + "," + quote + quote + "," + quote + Testo + quote + ")]";
                        //if (!ConditionRiporto.StartsWith("[IIf("))
                        //{
                        //    ConditionRiporto = "";
                        //}
                        //Testo = ConditionRiporto;
                        TextObject Etichetta = CreateTextObject("TextRiportareRaggruppamento", Testo, VDCB.XTraslation, VDCB.YTraslation, VDCB.Width, HeightTextBoxColumnFooter);
                        Etichetta.Font = new Font("Segoe UI", 9, FontStyle.Italic);
                        Etichetta.TextColor = Color.SteelBlue;
                        Etichetta.CanGrow = false;//22/12/2023 true;
                        Etichetta.CanShrink = false;
                        Etichetta.HorzAlign = HorzAlign.Right;
                        Etichetta.AfterData += Riporti_AfterData;
                        if (Bordatura)
                        {
                            CreaBordo(Etichetta, true, true, true, true);
                            Etichetta.Border.TopLine.Color = Color.LightGray;
                            Etichetta.Border.TopLine.Width = (float)0.25;
                        }
                        Page.ColumnFooter.Objects.Add(Etichetta);
                        RiportiUltimaPaginaDaNascondere.Add(Etichetta);
                        TextBoxCreated = true;
                    }
                    VDCB.XTraslation = VDCB.XTraslation + VDCB.Width;
                    Contatore++;
                }

                VDCB.YTraslation = VDCB.YTraslation + HeightTextBoxColumnFooter;
                VDCB.XTraslation = 0;
            }

            if (NessunaTipologiadiRiporto)
            {
                CreateEmptyRow(false, VDCB.YTraslation);
                //VDCB.YTraslation = VDCB.YTraslation + HeightTextBoxColumnFooter;
                VDCB.YTraslation = VDCB.YTraslation + DefaultHeightTextBox;
            }

            BaseName = "RiportareCalcolato";

            Contatore = 0;
            if (ListaCelleCorpo.Any(r => r.RiportoPagina))
            {
                string ConditionRiporto = "[IIf(";
                foreach (CellaTabella Cella in ListaCelleCorpo)
                {
                    VDCB.Width = (Units.Centimeters * (float)ReportSetting.CorpiDocumento.ElementAt(Contatore).Size);
                    if (Cella.RiportoPagina)
                    {
                        AttributoPerReport = JReportHelper.ReplaceSymbolNotAllowInReport(Cella.AttributoPerReport);
                        bool TextBoxCreated = false;

                        if (FormatoNumero.Contains(Cella.PropertyType) || FormatoValuta.Contains(Cella.PropertyType))
                        {
                            TextObject Text = CreateTextObject(BaseName + AttributoPerReport, "", VDCB.XTraslation + VDCB.WidthEtichetta, VDCB.YTraslation, VDCB.Width, HeightTextBoxColumnFooter);
                            if (Cella.Nascondi)
                            {
                                string totalGroup = null;
                                int IndexGroup = 0;
                                foreach (var SommaPerRaggruppamento in DictionarySommePerRaggrupamento.Reverse())
                                {
                                    if (SommaPerRaggruppamento.Value.Where(t => t.Contains(AttributoPerReport)).FirstOrDefault() != null)
                                    {
                                        totalGroup = SommaPerRaggruppamento.Value.Where(t => t.Contains(AttributoPerReport)).FirstOrDefault();
                                        IndexGroup = SommaPerRaggruppamento.Key;
                                        break;
                                    }
                                }

                                if (!string.IsNullOrEmpty(totalGroup))
                                {
                                    //SOMME PROGRESSIVE ULTIMO RAGGRUPPAMENTO
                                    Total Total = new Total();
                                    string TotalName = "TotalOfRiportarePaginaRaggr";
                                    TotalName = TotalName + AttributoPerReport + IndexDb;
                                    Total.Name = TotalName;
                                    Total.TotalType = TotalType.Sum;
                                    Total.Expression = "[" + DbName + "." + AttributoPerReport + "]";
                                    Total.Evaluator = Data;
                                    Total.ResetAfterPrint = false;
                                    Total.PrintOn = DictionaryBandaRaggruppamento[IndexGroup];
                                    ReportObject.Dictionary.Totals.Add(Total);
                                    //string totalGroup = ListaRiportiTotaliBandaUltimoRaggruppamento.Where(t => t.Contains(BandaUltimoRaggruppamentoRiporti.Name.Replace("GroupFooter", "")) && t.Contains(AttributoPerReport)).FirstOrDefault();
                                    Text.Text = "[" + TotalName + "] - [" + totalGroup + "]";
                                }
                                else
                                {
                                    VDCB.XTraslation = VDCB.XTraslation + VDCB.Width;
                                    Contatore++;
                                    continue;
                                }

                            }
                            if (!Cella.Nascondi)
                            {
                                Total Total = new Total();
                                string TotalName = "TotalOfRiportarePagina";
                                TotalName = TotalName + AttributoPerReport + IndexDb;
                                Total.Name = TotalName;
                                Total.TotalType = TotalType.Sum;
                                Total.Expression = "[" + DbName + "." + AttributoPerReport + "]";
                                Total.Evaluator = Data;
                                Total.ResetAfterPrint = false;
                                Total.PrintOn = Page.PageHeader;
                                ReportObject.Dictionary.Totals.Add(Total);
                                Text.Text = "[" + TotalName + "]";
                            }

                            Text.CanGrow = false;//22/12/2023 true
                            Text.CanShrink = false;
                            Text.HorzAlign = HorzAlign.Right;
                            Text.Font = new Font("Segoe UI", 9, FontStyle.Italic);
                            string quote = "\"";
                            ConditionRiporto = ConditionRiporto + Text.Text + " == 0 &&";
                            Text.Text = "IIf(" + Text.Text + " == 0 ," + quote + quote + "," + Text.Text + ")";
                            if (Bordatura)
                                CreaBordo(Text, false, true, false, false);
                            Text.AfterData += Riporti_AfterData;
                            RetrieveFormatFromAttribute(AttributoPerReport, Text);
                            RiportiUltimaPaginaDaNascondere.Add(Text);
                            Page.ColumnFooter.Objects.Add(Text);
                            TextBoxCreated = true;
                        }

                        if (!TextBoxCreated)
                        {
                            TextObject Etichetta = CreaEtichetta(new CellaTabella(), VDCB.XTraslation, VDCB.YTraslation, VDCB.Width);
                            Etichetta.CanGrow = false;
                            Etichetta.CanShrink = false;
                            Etichetta.HorzAlign = HorzAlign.Right;
                            Etichetta.Font = new Font("Segoe UI", 9, FontStyle.Italic);
                            Etichetta.TextColor = Color.SteelBlue;
                            Etichetta.AfterData += Riporti_AfterData;
                            if (Bordatura)
                                CreaBordo(Etichetta, false, true, false, false);
                            Page.ColumnFooter.Objects.Add(Etichetta);
                        }
                    }

                    VDCB.XTraslation = VDCB.XTraslation + VDCB.Width;
                    Contatore++;
                }

                Contatore = 0;
                VDCB.XTraslation = 0;
                ConditionRiporto = ConditionRiporto.Substring(0, ConditionRiporto.Length - 2);


                foreach (CellaTabella Cella in ListaCelleCorpo)
                {
                    AttributoPerReport = JReportHelper.ReplaceSymbolNotAllowInReport(Cella.AttributoPerReport);
                    VDCB.Width = (Units.Centimeters * (float)ReportSetting.CorpiDocumento.ElementAt(Contatore).Size);

                    if (Contatore == 1)
                    {
                        string Testo = LocalizationProvider.GetString("RiportarePagina");
                        //string quote = "\"";
                        //ConditionRiporto = ConditionRiporto + "," + quote + quote + "," + quote + Testo + quote + ")]";
                        //if (!ConditionRiporto.StartsWith("[IIf("))
                        //{
                        //    ConditionRiporto = "";
                        //}
                        //Testo = ConditionRiporto;
                        TextObject Etichetta = CreateTextObject("TextRiportare", Testo, VDCB.XTraslation, VDCB.YTraslation, VDCB.Width, HeightTextBoxColumnFooter);
                        //Etichetta.Text = Etichetta.Text.Replace("[", "").Replace("]", "");
                        Etichetta.Font = new Font("Segoe UI", 9, FontStyle.Italic);
                        Etichetta.CanGrow = false;//22/12/2023 true;
                        Etichetta.CanShrink = false;
                        Etichetta.HorzAlign = HorzAlign.Right;
                        if (Bordatura)
                            CreaBordo(Etichetta, false, true, false, false);
                        RiportiUltimaPaginaDaNascondere.Add(Etichetta);
                        Page.ColumnFooter.Objects.Add(Etichetta);
                    }
                    VDCB.XTraslation = VDCB.XTraslation + VDCB.Width;
                    Contatore++;
                }

                VDCB.YTraslation = VDCB.YTraslation + HeightTextBoxColumnFooter;
                VDCB.XTraslation = 0;
            }

            if (!ReportSetting.IsFirmeEmpty)
            {
                if (!string.IsNullOrEmpty(ReportSetting.Firme))
                {
                    ReportSetting.Firme = entitiesHelper.GetRtfPreview(ReportSetting.Firme, RtfDataService);
                    RichObject RichTextFirme = CreateRichObject("Text" + StampeKeys.ConstFirme, "", TraslationXRtf, VDCB.YTraslation, DimensionamentoRtf, VDCB.YTraslation, @"" + ReportSetting.Firme);
                    Page.ColumnFooter.Objects.Add(RichTextFirme);
                }
            }
        }

        List<TextObject> RiportiPrimaPaginaDaNascondere = new List<TextObject>();
        private void AddObjectToPageHeader(bool FromDocumento, bool CreateHeaders)
        {
            VariabiliDiCicloBanda VDCB = new VariabiliDiCicloBanda(0, 0, 0, 0, ThirdSpace * 3, 0);

            RichObject RichTextINte = new RichObject();
            TextObject TextInt = new TextObject();
            string AttributoPerReport = null;

            bool NessunaTipologiadiRiporto = true;

            if (ReportSetting != null)
            {
                if (FromDocumento)
                {
                    RichTextINte = CreateRichObject("Text" + StampeKeys.ConstIntestazione + DbName, "", TraslationXRtf, 0, DimensionamentoRtf, HeightTextBoxPageHeader, @"" + ReportSetting.Intestazione);
                    VDCB.XTraslation = VDCB.XTraslation + ThirdSpace;
                    Page.PageHeader.Objects.Add(RichTextINte);
                }

                int ContatoreEtichetta = 0;

                if (CreateHeaders)
                {
                    VDCB = new VariabiliDiCicloBanda(0, 0, HeightTextBoxPageHeader * 2, 0, 0, 0);

                    string BaseName = "RiportoCalcolato";

                    int Contatore = 0;
                    if (ListaCelleCorpo.Any(r => r.RiportoPagina))
                    {
                        string ConditionRiporto = "[IIf(";
                        foreach (CellaTabella Cella in ListaCelleCorpo)
                        {
                            VDCB.Width = (Units.Centimeters * (float)ReportSetting.CorpiDocumento.ElementAt(Contatore).Size);
                            if (Cella.RiportoPagina)
                            {
                                AttributoPerReport = JReportHelper.ReplaceSymbolNotAllowInReport(Cella.AttributoPerReport);

                                if ((FormatoNumero.Contains(Cella.PropertyType) || FormatoValuta.Contains(Cella.PropertyType)) && Contatore != 1)
                                {
                                    //22/12/2023
                                    TextObject Text = CreateTextObject(BaseName + AttributoPerReport, "", VDCB.XTraslation + VDCB.WidthEtichetta, VDCB.YTraslation, VDCB.Width, HeightTextBoxPageHeader);
                                    
                                    if (Cella.Nascondi)
                                    {
                                        string totalGroup = null;
                                        int IndexGroup = 0;
                                        foreach (var SommaPerRaggruppamento in DictionarySommePerRaggrupamento.Reverse())
                                        {
                                            if (SommaPerRaggruppamento.Value.Where(t => t.Contains(AttributoPerReport)).FirstOrDefault() != null)
                                            {
                                                totalGroup = SommaPerRaggruppamento.Value.Where(t => t.Contains(AttributoPerReport)).FirstOrDefault();
                                                IndexGroup = SommaPerRaggruppamento.Key;
                                                break;
                                            }
                                        }

                                        if (!string.IsNullOrEmpty(totalGroup))
                                        {
                                            //SOMME PROGRESSIVE ULTIMO RAGGRUPPAMENTO
                                            Total Total = new Total();
                                            string TotalName = "TotalOfRiportoPaginaRaggr";
                                            TotalName = TotalName + AttributoPerReport + IndexDb;
                                            Total.Name = TotalName;
                                            Total.TotalType = TotalType.Sum;
                                            Total.Expression = "[" + DbName + "." + AttributoPerReport + "]";
                                            Total.Evaluator = Data;
                                            Total.ResetAfterPrint = false;
                                            Total.PrintOn = DictionaryBandaRaggruppamento[IndexGroup];
                                            ReportObject.Dictionary.Totals.Add(Total);
                                            Text.Text = "[" + TotalName + "] - [" + totalGroup + "]";
                                        }
                                        else
                                        {
                                            VDCB.XTraslation = VDCB.XTraslation + VDCB.Width;
                                            Contatore++;
                                            continue;
                                        }

                                    }
                                    if (!Cella.Nascondi)
                                    {
                                        Total Total = new Total();
                                        string TotalName = "TotalOfRiportoPagina";
                                        TotalName = TotalName + AttributoPerReport + IndexDb;
                                        Total.Name = TotalName;
                                        Total.TotalType = TotalType.Sum;
                                        Total.Expression = "[" + DbName + "." + AttributoPerReport + "]";
                                        Total.Evaluator = Data;
                                        Total.ResetAfterPrint = false;
                                        Total.PrintOn = Page.PageHeader;
                                        ReportObject.Dictionary.Totals.Add(Total);
                                        Text.Text = "[" + TotalName + "]";
                                    }

                                    Text.CanGrow = false;//AU 22/12/2023 true
                                    Text.CanShrink = false;

                                    Text.HorzAlign = HorzAlign.Right;
                                    Text.Font = new Font("Segoe UI", 9, FontStyle.Italic);
                                    string quote = "\"";
                                    ConditionRiporto = ConditionRiporto + Text.Text + " == 0 &&";
                                    Text.Text = "IIf(" + Text.Text + " == 0 ," + quote + quote + "," + Text.Text + ")";
                                    Text.AfterData += Riporti_AfterData;

                                    RetrieveFormatFromAttribute(AttributoPerReport, Text);
                                    RiportiPrimaPaginaDaNascondere.Add(Text);
                                    Page.PageHeader.Objects.Add(Text);
                                }
                            }

                            VDCB.XTraslation = VDCB.XTraslation + VDCB.Width;
                            Contatore++;
                        }

                        Contatore = 0;
                        VDCB.XTraslation = 0;
                        ConditionRiporto = ConditionRiporto.Substring(0, ConditionRiporto.Length - 2);

                        foreach (CellaTabella Cella in ListaCelleCorpo)
                        {
                            AttributoPerReport = JReportHelper.ReplaceSymbolNotAllowInReport(Cella.AttributoPerReport);
                            VDCB.Width = (Units.Centimeters * (float)ReportSetting.CorpiDocumento.ElementAt(Contatore).Size);

                            if (Contatore == 1)
                            {
                                string Testo = LocalizationProvider.GetString("RiportoPagina");
                                //string quote = "\"";
                                //ConditionRiporto = ConditionRiporto + "," + quote + quote + "," + quote + Testo + quote + ")]";
                                //if (!ConditionRiporto.StartsWith("[IIf("))
                                //{
                                //    ConditionRiporto = "";
                                //}
                                //Testo = ConditionRiporto;
                                TextObject Etichetta = CreateTextObject("TextRiporto", Testo, VDCB.XTraslation, VDCB.YTraslation, VDCB.Width, HeightTextBoxPageHeader);
                                Etichetta.Font = new Font("Segoe UI", 9, FontStyle.Italic);
                                Etichetta.CanShrink = false;
                                Etichetta.HorzAlign = HorzAlign.Right;
                                Etichetta.AfterData += Riporti_AfterData;
                                RiportiPrimaPaginaDaNascondere.Add(Etichetta);
                                Page.PageHeader.Objects.Add(Etichetta);
                            }
                            VDCB.XTraslation = VDCB.XTraslation + VDCB.Width;
                            Contatore++;
                        }

                        VDCB.YTraslation = VDCB.YTraslation + HeightTextBoxPageHeader;
                        VDCB.XTraslation = 0;
                    }

                    if (ReportSetting.Intestazioni != null)
                    {
                        foreach (var Attributo in ReportSetting.Intestazioni)
                        {
                            VDCB.Width = (float)Attributo.Size;
                            TextObject Text = CreateTextObject("TextIntestazioneColonna" + JReportHelper.ReplaceSymbolNotAllowInReport(Attributo.Etichetta), Attributo.Etichetta, VDCB.XTraslation, VDCB.YTraslation, Units.Centimeters * VDCB.Width, HeightTextBoxPageHeader);
                            SettingFontForTextObject(Text, Attributo.StileCarattere, Bordatura, false);
                            if (Bordatura)
                                CreaBordo(Text, true, true, true, true);

                            Page.PageHeader.Objects.Add(Text);

                            ContatoreEtichetta++;

                            if (ContatoreEtichetta != 0)
                            { VDCB.XTraslation = VDCB.XTraslation + Units.Centimeters * VDCB.Width; }
                            else { VDCB.XTraslation = 0; }
                        }
                        VDCB.YTraslation = VDCB.YTraslation + HeightTextBoxPageHeader;
                        VDCB.XTraslation = 0;
                    }

                    BaseName = "RiportoRaggruppamentoCalcolato";
                    Contatore = 0;

                    if (ListaCelleCorpo.Any(r => r.RiportoRaggruppamento))
                    {
                        NessunaTipologiadiRiporto = false;
                        string ConditionRiporto = "[IIf(";
                        int CounterColonna = 0;

                        foreach (CellaTabella Cella in ListaCelleCorpo)
                        {
                            AttributoPerReport = JReportHelper.ReplaceSymbolNotAllowInReport(Cella.AttributoPerReport);
                            bool TextBoxCreated = false;
                            VDCB.Width = (Units.Centimeters * (float)ReportSetting.CorpiDocumento.ElementAt(Contatore).Size);

                            if ((FormatoNumero.Contains(Cella.PropertyType) || FormatoValuta.Contains(Cella.PropertyType)) && Contatore != 1)
                            {
                                bool useConditionHideValue = false;
                                string TotalName = null;
                                if (!Cella.Nascondi)
                                {
                                    foreach (var SommaPerRaggruppamento in DictionarySommePerRaggrupamento.Reverse())
                                    {
                                        if (SommaPerRaggruppamento.Value.Where(t => t.Contains(AttributoPerReport)).FirstOrDefault() != null)
                                        {
                                            TotalName = SommaPerRaggruppamento.Value.Where(t => t.Contains(AttributoPerReport)).FirstOrDefault();
                                            TotalName = "[" + TotalName + "]";
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (var SommaPerRaggruppamento in DictionarySommePerRaggrupamento.Reverse())
                                    {
                                        if (SommaPerRaggruppamento.Value.Where(t => t.Contains(AttributoPerReport)).FirstOrDefault() != null)
                                        {
                                            useConditionHideValue = true;
                                            if (CounterColonna == 0) TotalName = "[" + RiportiVariables.PrimaColonnaRiportoString + "]";
                                            if (CounterColonna == 1) TotalName = "[" + RiportiVariables.SecondaColonnaRiportoString + "]";
                                            if (CounterColonna == 2) TotalName = "[" + RiportiVariables.TerzaColonnaRiportoString + "]";
                                            if (CounterColonna == 3) TotalName = "[" + RiportiVariables.QuartaColonnaRiportoString + "]";
                                            if (CounterColonna == 4) TotalName = "[" + RiportiVariables.QuintaColonnaRiportoString + "]";
                                            if (CounterColonna == 5) TotalName = "[" + RiportiVariables.SestaColonnaRiportoString + "]";
                                            if (CounterColonna == 6) TotalName = "[" + RiportiVariables.SettimaColonnaRiportoString + "]";
                                            if (CounterColonna == 7) TotalName = "[" + RiportiVariables.OttavaColonnaRiportoString + "]";
                                            if (CounterColonna == 8) TotalName = "[" + RiportiVariables.NonaColonnaRiportoString + "]";
                                            if (CounterColonna == 9) TotalName = "[" + RiportiVariables.DecimaColonnaRiportoString + "]";
                                            if (CounterColonna == 10) TotalName = "[" + RiportiVariables.UndicesimaColonnaRiportoString + "]";
                                            if (CounterColonna == 11) TotalName = "[" + RiportiVariables.DodicesimaColonnaRiportoString + "]";
                                            if (CounterColonna == 12) TotalName = "[" + RiportiVariables.TredicesimaColonnaRiportoString + "]";
                                            if (CounterColonna == 13) TotalName = "[" + RiportiVariables.QuattordicesimaColonnaRiportoString + "]";
                                            if (CounterColonna == 14) TotalName = "[" + RiportiVariables.QuindicesimaColonnaRiportoString + "]";
                                            if (CounterColonna == 15) TotalName = "[" + RiportiVariables.SedicesimaColonnaRiportoString + "]";
                                            if (CounterColonna == 16) TotalName = "[" + RiportiVariables.DiciassettesimaColonnaRiportoString + "]";
                                            if (CounterColonna == 17) TotalName = "[" + RiportiVariables.DiciottesimaColonnaRiportoString + "]";
                                            if (CounterColonna == 18) TotalName = "[" + RiportiVariables.DiciannovesimaColonnaRiportoString + "]";
                                            if (CounterColonna == 19) TotalName = "[" + RiportiVariables.VentesimaColonnaRiportoString + "]";
                                            break;
                                        }
                                    }
                                }

                                if (!String.IsNullOrEmpty(TotalName))
                                {
                                    TextObject Text = CreateTextObject(BaseName + AttributoPerReport, TotalName, VDCB.XTraslation, VDCB.YTraslation, VDCB.Width, HeightTextBoxPageHeader);
                                    Text.CanGrow = false;//22/12/2023 true
                                    Text.CanShrink = false;
                                    Text.HorzAlign = HorzAlign.Right;
                                    Text.Font = new Font("Segoe UI", 9, FontStyle.Italic);
                                    Text.TextColor = Color.SteelBlue;
                                    if (!useConditionHideValue)
                                    {
                                        string quote = "\"";
                                        ConditionRiporto = ConditionRiporto + Text.Text + " == 0 &&";
                                        Text.Text = "IIf(" + Text.Text + " == 0 ," + quote + quote + "," + Text.Text + ")";
                                    }
                                    Text.AfterData += Riporti_AfterData;

                                    RetrieveFormatFromAttribute(AttributoPerReport, Text); //era commentato (non veniva aggiunto il formato)
                                    
                                    if (Bordatura)
                                        CreaBordo(Text, true, true, true, true);
                                    RiportiPrimaPaginaDaNascondere.Add(Text);
                                    Page.PageHeader.Objects.Add(Text);
                                    TextBoxCreated = true;
                                }
                            }

                            if (!TextBoxCreated)
                            {
                                TextObject Etichetta = CreaEtichetta(new CellaTabella(), VDCB.XTraslation, VDCB.YTraslation, VDCB.Width);
                                Etichetta.CanGrow = false;
                                Etichetta.CanShrink = false;
                                Etichetta.Height = HeightTextBoxPageHeader;
                                Etichetta.HorzAlign = HorzAlign.Right;
                                Etichetta.Font = new Font("Segoe UI", 9, FontStyle.Italic);
                                Etichetta.TextColor = Color.SteelBlue;
                                Etichetta.PrintOn = PrintOn.EvenPages | PrintOn.LastPage | PrintOn.OddPages | PrintOn.RepeatedBand | PrintOn.SinglePage;
                                Etichetta.AfterData += Riporti_AfterData;
                                if (Bordatura)
                                    CreaBordo(Etichetta, true, true, true, true);
                                Page.PageHeader.Objects.Add(Etichetta);
                            }

                            //ETICHETTA PER COLMARE PRINT ON PRIMA PAGINA
                            TextObject EtichettaVuotaPrimaRiga = CreaEtichetta(new CellaTabella(), VDCB.XTraslation, VDCB.YTraslation, VDCB.Width);
                            EtichettaVuotaPrimaRiga.CanShrink = false;
                            EtichettaVuotaPrimaRiga.Height = HeightTextBoxPageHeader;
                            EtichettaVuotaPrimaRiga.GrowToBottom = true;
                            if (Bordatura)
                            {
                                CreaBordo(EtichettaVuotaPrimaRiga, true, true, true, true);
                                EtichettaVuotaPrimaRiga.Border.BottomLine.Color = Color.LightGray;
                                EtichettaVuotaPrimaRiga.Border.BottomLine.Width = (float)0.25;
                            }
                            Page.PageHeader.Objects.Add(EtichettaVuotaPrimaRiga);

                            VDCB.XTraslation = VDCB.XTraslation + VDCB.Width;
                            Contatore++;
                            CounterColonna++;
                        }

                        Contatore = 0;
                        VDCB.XTraslation = 0;
                        ConditionRiporto = ConditionRiporto.Substring(0, ConditionRiporto.Length - 2);

                        foreach (CellaTabella Cella in ListaCelleCorpo)
                        {
                            AttributoPerReport = JReportHelper.ReplaceSymbolNotAllowInReport(Cella.AttributoPerReport);
                            bool TextBoxCreated = false;
                            VDCB.Width = (Units.Centimeters * (float)ReportSetting.CorpiDocumento.ElementAt(Contatore).Size);

                            if (Contatore == 1)
                            {
                                string Testo = LocalizationProvider.GetString("RiportoRaggruppamento");
                                //string quote = "\"";
                                //ConditionRiporto = ConditionRiporto + "," + quote + quote + "," + quote + Testo + quote + ")]";
                                //if (!ConditionRiporto.StartsWith("[IIf("))
                                //{
                                //    ConditionRiporto = "";
                                //}
                                //Testo = ConditionRiporto;
                                TextObject Etichetta = CreateTextObject("TextRiportoRaggruppamento", Testo, VDCB.XTraslation, VDCB.YTraslation, VDCB.Width, HeightTextBoxPageHeader);
                                Etichetta.Font = new Font("Segoe UI", 9, FontStyle.Italic);
                                Etichetta.TextColor = Color.SteelBlue;
                                Etichetta.CanShrink = false;

                                Etichetta.HorzAlign = HorzAlign.Right;
                                Etichetta.AfterData += Riporti_AfterData;
                                if (Bordatura)
                                {
                                    CreaBordo(Etichetta, true, true, true, true);
                                    Etichetta.Border.BottomLine.Color = Color.LightGray;
                                    Etichetta.Border.BottomLine.Width = (float)0.25;
                                }
                                RiportiPrimaPaginaDaNascondere.Add(Etichetta);  
                                Page.PageHeader.Objects.Add(Etichetta);
                                TextBoxCreated = true;
                            }
                            VDCB.XTraslation = VDCB.XTraslation + VDCB.Width;
                            Contatore++;
                        }


                        VDCB.YTraslation = VDCB.YTraslation + HeightTextBoxPageHeader;
                        VDCB.XTraslation = 0;
                    }

                    if (NessunaTipologiadiRiporto)
                    {
                        CreateEmptyRow(true, VDCB.YTraslation);
                        VDCB.YTraslation = VDCB.YTraslation + HeightTextBoxPageHeader;
                    }
                }
            }
        }

        private void Riporti_AfterData(object sender, EventArgs e)
        {
        }

        //FUNZIONALITA' COMUNI A PIU BANDE 
        private void CreaBordo(TextObjectBase TextObjectBase, bool Left, bool Top, bool Right, bool Bottom)
        {

            FastReport.Border Border = new FastReport.Border();

            if (Left)
                Border.Lines |= BorderLines.Left;

            if (Top)
                Border.Lines |= BorderLines.Top;

            if (Right)
                Border.Lines |= BorderLines.Right;

            if (Bottom)
                Border.Lines |= BorderLines.Bottom;


            Border.Width = (float)0.25;
            TextObjectBase.Border = Border;
        }
        private void CreaBordo_old(TextObjectBase TextObjectBase, bool Left, bool Top, bool Right, bool Bottom)
        {

            Top = Right = Bottom = true;
            Left = false;



            FastReport.Border Border = new FastReport.Border();

            if (Left && !Top && !Right && !Bottom)
                Border.Lines = BorderLines.Left;
            if (!Left && Top && !Right && !Bottom)
                Border.Lines = BorderLines.Top;
            if (!Left && !Top && Right && !Bottom)
                Border.Lines = BorderLines.Right;
            if (!Left && !Top && !Right && Bottom)
                Border.Lines = BorderLines.Bottom;
            if (Left && !Top && Right && !Bottom)
                Border.Lines = BorderLines.Left | BorderLines.Right;
            if (Left && Top && Right && !Bottom)
                Border.Lines = BorderLines.Left | BorderLines.Top | BorderLines.Right;
            if (Left && !Top && Right && Bottom)
                Border.Lines = BorderLines.Left | BorderLines.Bottom | BorderLines.Right;
            if (Left && Top && Right && Bottom)
                Border.Lines = BorderLines.Left | BorderLines.Top | BorderLines.Right | BorderLines.Bottom;

            Border.Width = (float)0.25;
            TextObjectBase.Border = Border;
        }

        private TextObject CreaEtichetta(CellaTabella CellaTabella, float XTraslation, float YTraslation, float WidthEtichetta)
        {
            string Name = "TextEtichetta" + ContatoreEtichetteSenzaAttributo.ToString();
            string Text = FormatEtichettaIfIsEmpty(CellaTabella?.Etichetta);

            TextObject TextEtichetta = CreateTextObject(Name, Text, XTraslation, YTraslation, WidthEtichetta, DefaultHeightTextBox);

            if (CellaTabella != null)
            {
                if (CellaTabella.StileCarattere != null)
                    SettingFontForTextObject(TextEtichetta, CellaTabella.StileCarattere, Bordatura, false);
            }

            ContatoreEtichetteSenzaAttributo++;
            return TextEtichetta;
        }
        private TextObject CreateTextObject(string Name, string Text, float XTraslation, float YTraslation, float Width, float Height)
        {
            TextObject TextOject = new TextObject();
            TextOject.Name = Name.Trim().Replace(" ", "_").Replace("-", "_").Replace(".", "") + "DB" + IndexDb;
            TextOject.Bounds = new RectangleF(XTraslation, YTraslation, Width, Height);
            TextOject.Text = Text;
            TextOject.CanBreak = true;
            TextOject.CanGrow = true;
            TextOject.GrowToBottom = false; //22/12/2023 true

            //Add by Ale 23/01/2023 gestione del valore da nascondere
            if (!string.IsNullOrEmpty(Text))
            {

                string formatQta = string.Format("{0}0:{1}{2}", "{", GetFormatFromAttribute(BuiltInCodes.EntityType.Computo + BuiltInCodes.Attributo.Quantita), "}");
                string formatQtaTot = string.Format("{0}0:{1}{2}", "{", GetFormatFromAttribute(BuiltInCodes.EntityType.Computo + BuiltInCodes.Attributo.QuantitaTotale), "}");
                string formatPU = string.Format("{0}0:{1}{2}", "{", GetFormatFromAttribute(BuiltInCodes.EntityType.Computo + BuiltInCodes.Attributo.PU), "}");

                string qta0 = string.Format(formatQta, 0.0);
                string qtaTot0 = string.Format(formatQtaTot, 0.0);

                Dictionary<string, string> attValsToHide = new Dictionary<string, string>();
                attValsToHide.Add(string.Format("[{0}.{1}{2}]", DbName, BuiltInCodes.EntityType.Computo, BuiltInCodes.Attributo.Quantita), qta0);
                attValsToHide.Add(string.Format("[{0}.{1}{2}]", DbName, BuiltInCodes.EntityType.Computo, BuiltInCodes.Attributo.QuantitaTotale), qtaTot0);
                attValsToHide.Add(string.Format("[{0}.{1}{2}Formula]", DbName, BuiltInCodes.EntityType.Computo, BuiltInCodes.Attributo.Quantita), qta0);
                attValsToHide.Add(string.Format("[{0}.{1}{2}Formula]", DbName, BuiltInCodes.EntityType.Computo, BuiltInCodes.Attributo.QuantitaTotale), qtaTot0);

                if (attValsToHide.ContainsKey(Text))
                    TextOject.HideValue = attValsToHide[Text];

            }
            //end Add

            return TextOject;
        }

        private string FormatEtichettaIfIsEmpty(string etichetta)
        {
            string etichettaFormattata = etichetta;
            if (etichettaFormattata == StampeKeys.LocalizeEtichettaWizard || String.IsNullOrEmpty(etichettaFormattata))
            {
                etichettaFormattata = "";
            }
            else
            {
                etichettaFormattata = etichettaFormattata + " ";
            }
            return etichettaFormattata;
        }
        private void SettingFontForTextObject(TextObject Text, ProprietaCarattere stileCarattere, bool IsWithBorder = false, bool GrowToBottom = true)
        {
            Font _font = null;

            if (stileCarattere.ColorCharacther != null) { Text.TextColor = ColorTranslator.FromHtml(stileCarattere.ColorCharacther.HexValue); }
            if (stileCarattere.ColorBackground != null) { Text.FillColor = ColorTranslator.FromHtml(stileCarattere.ColorBackground.HexValue); }
            if (stileCarattere.IsGrassetto && stileCarattere.IsSottolineato == false && stileCarattere.IsBarrato == false && stileCarattere.IsCorsivo == false) { _font = new Font(stileCarattere.FontFamily, Convert.ToInt32(stileCarattere.Size), FontStyle.Bold); }
            if (stileCarattere.IsGrassetto && stileCarattere.IsSottolineato && stileCarattere.IsBarrato == false && stileCarattere.IsCorsivo == false) { _font = new Font(stileCarattere.FontFamily, Convert.ToInt32(stileCarattere.Size), FontStyle.Bold | FontStyle.Underline); }
            if (stileCarattere.IsGrassetto == false && stileCarattere.IsSottolineato && stileCarattere.IsBarrato == false && stileCarattere.IsCorsivo == false) { _font = new Font(stileCarattere.FontFamily, Convert.ToInt32(stileCarattere.Size), FontStyle.Underline); }
            if (stileCarattere.IsGrassetto == false && stileCarattere.IsSottolineato && stileCarattere.IsBarrato && stileCarattere.IsCorsivo == false) { _font = new Font(stileCarattere.FontFamily, Convert.ToInt32(stileCarattere.Size), FontStyle.Underline | FontStyle.Strikeout); }
            if (stileCarattere.IsGrassetto == false && stileCarattere.IsSottolineato == false && stileCarattere.IsBarrato && stileCarattere.IsCorsivo == false) { _font = new Font(stileCarattere.FontFamily, Convert.ToInt32(stileCarattere.Size), FontStyle.Strikeout); }
            if (stileCarattere.IsGrassetto && stileCarattere.IsSottolineato == false && stileCarattere.IsBarrato && stileCarattere.IsCorsivo == false) { _font = new Font(stileCarattere.FontFamily, Convert.ToInt32(stileCarattere.Size), FontStyle.Bold | FontStyle.Strikeout); }
            if (stileCarattere.IsGrassetto && stileCarattere.IsSottolineato && stileCarattere.IsBarrato && stileCarattere.IsCorsivo == false) { _font = new Font(stileCarattere.FontFamily, Convert.ToInt32(stileCarattere.Size), FontStyle.Bold | FontStyle.Underline | FontStyle.Strikeout); }
            if (stileCarattere.IsGrassetto == false && stileCarattere.IsSottolineato == false && stileCarattere.IsBarrato == false && stileCarattere.IsCorsivo) { _font = new Font(stileCarattere.FontFamily, Convert.ToInt32(stileCarattere.Size), FontStyle.Italic); }
            if (stileCarattere.IsGrassetto == false && stileCarattere.IsSottolineato == false && stileCarattere.IsBarrato && stileCarattere.IsCorsivo) { _font = new Font(stileCarattere.FontFamily, Convert.ToInt32(stileCarattere.Size), FontStyle.Italic | FontStyle.Strikeout); }
            if (stileCarattere.IsGrassetto == false && stileCarattere.IsSottolineato && stileCarattere.IsBarrato && stileCarattere.IsCorsivo) { _font = new Font(stileCarattere.FontFamily, Convert.ToInt32(stileCarattere.Size), FontStyle.Italic | FontStyle.Strikeout | FontStyle.Underline); }
            if (stileCarattere.IsGrassetto && stileCarattere.IsSottolineato && stileCarattere.IsBarrato && stileCarattere.IsCorsivo) { _font = new Font(stileCarattere.FontFamily, Convert.ToInt32(stileCarattere.Size), FontStyle.Italic | FontStyle.Strikeout | FontStyle.Underline | FontStyle.Bold); }
            if (stileCarattere.IsGrassetto && stileCarattere.IsSottolineato == false && stileCarattere.IsBarrato == false && stileCarattere.IsCorsivo) { _font = new Font(stileCarattere.FontFamily, Convert.ToInt32(stileCarattere.Size), FontStyle.Italic | FontStyle.Bold); }
            if (stileCarattere.IsGrassetto == false && stileCarattere.IsSottolineato && stileCarattere.IsBarrato == false && stileCarattere.IsCorsivo) { _font = new Font(stileCarattere.FontFamily, Convert.ToInt32(stileCarattere.Size), FontStyle.Italic | FontStyle.Underline); }
            if (stileCarattere.IsGrassetto == false && stileCarattere.IsSottolineato == false && stileCarattere.IsBarrato == false && stileCarattere.IsCorsivo == false) { _font = new Font(stileCarattere.FontFamily, Convert.ToInt32(stileCarattere.Size)); };
            if (stileCarattere.IsGrassetto && stileCarattere.IsSottolineato && stileCarattere.IsBarrato == false && stileCarattere.IsCorsivo) { _font = new Font(stileCarattere.FontFamily, Convert.ToInt32(stileCarattere.Size), FontStyle.Bold | FontStyle.Underline | FontStyle.Italic); };
            if (stileCarattere.IsGrassetto && stileCarattere.IsSottolineato == false && stileCarattere.IsBarrato && stileCarattere.IsCorsivo) { _font = new Font(stileCarattere.FontFamily, Convert.ToInt32(stileCarattere.Size), FontStyle.Bold | FontStyle.Italic | FontStyle.Strikeout); };

            Text.Font = _font;

            if (stileCarattere.TextAlignementCode == 0)
            {
                Text.HorzAlign = HorzAlign.Left;
                stileCarattere.TextAlignementCode = 1;
            }
            else
            {
                if (stileCarattere.TextAlignementCode == 1)
                    Text.HorzAlign = HorzAlign.Left;
                if (stileCarattere.TextAlignementCode == 2)
                    Text.HorzAlign = HorzAlign.Center;
                if (stileCarattere.TextAlignementCode == 3)
                    Text.HorzAlign = HorzAlign.Right;
                if (stileCarattere.TextAlignementCode == 4)
                    Text.HorzAlign = HorzAlign.Justify;
            }

            if (stileCarattere.TextVerticalAlignementCode == 0)
            {
                Text.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left;
                stileCarattere.TextVerticalAlignementCode = 1;
            }
            else
            {
                if (stileCarattere.TextVerticalAlignementCode == 1)
                    Text.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left;
                if (stileCarattere.TextVerticalAlignementCode == 2)
                    Text.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            }

            if (IsWithBorder)
            {
                CreaBordo(Text, true, false, true, false);

                if (GrowToBottom)
                {
                    Text.GrowToBottom = true;
                    Text.CanShrink = false;
                }
            }
        }
        private RichObject CreateRichObject(string Name, string DataColumn, float XTraslation, float YTraslation, float Width, float Height, string Text = null)
        {
            RichObject RichText = new RichObject();
            RichText.Name = Name + ContatoreRtfObject;
            RichText.Bounds = new RectangleF(XTraslation, YTraslation, Width, Height);
            RichText.DataColumn = DataColumn;
            RichText.CanBreak = true;
            RichText.CanGrow = true;
            if (!string.IsNullOrWhiteSpace(Text))
            {
                RichText.Text = Text;
            }
            ContatoreRtfObject++;
            return RichText;
        }
        //private void RetrieveFormatFromAttribute(string Attributo, TextObject textObject)
        //{
        //    if (((System.Data.DataRow)ReportObject.Dictionary.DataSources[IndexDb].CurrentRow).Table.Columns["V_DDC_CDlS_CGS" + Attributo] != null)
        //    {
        //        int Index = ((System.Data.DataRow)ReportObject.Dictionary.DataSources[IndexDb].CurrentRow).Table.Columns["V_DDC_CDlS_CGS" + Attributo].Ordinal;

        //        var el = (((System.Data.DataRow)ReportObject.Dictionary.DataSources[IndexDb].CurrentRow).ItemArray.ElementAtOrDefault(Index));
        //        if (el != System.DBNull.Value)
        //        {
        //            var formattazione = ((string)(((System.Data.DataRow)ReportObject.Dictionary.DataSources[IndexDb].CurrentRow).ItemArray.ElementAt(Index)));
        //            textObject.Text = @"[Format(""{0:" + formattazione + @"}""," + textObject.Text.Replace("[[", "[").Replace("]]", "]") + ")]";
        //        }
        //    }
        //}

        private void RetrieveFormatFromAttribute(string Attributo, TextObject textObject)
        {
            string format = GetFormatFromAttribute(Attributo);
            if (format.Any())
                textObject.Text = @"[Format(""{0:" + format + @"}""," + textObject.Text.Replace("[[", "[").Replace("]]", "]") + ")]";
        }

        private string GetFormatFromAttribute(string Attributo)
        {
            if (((System.Data.DataRow)ReportObject.Dictionary.DataSources[IndexDb].CurrentRow).Table.Columns["V_DDC_CDlS_CGS" + Attributo] != null)
            {
                int Index = ((System.Data.DataRow)ReportObject.Dictionary.DataSources[IndexDb].CurrentRow).Table.Columns["V_DDC_CDlS_CGS" + Attributo].Ordinal;

                var el = (((System.Data.DataRow)ReportObject.Dictionary.DataSources[IndexDb].CurrentRow).ItemArray.ElementAtOrDefault(Index));
                if (el != System.DBNull.Value)
                {
                    string format = ((string)(((System.Data.DataRow)ReportObject.Dictionary.DataSources[IndexDb].CurrentRow).ItemArray.ElementAt(Index)));
                    return format;
                }
            }
            return string.Empty;
        }

        private void CreateEmptyRow(bool IsRiporto, float yTraslation)
        {
            VariabiliDiCicloBanda VDCB = new VariabiliDiCicloBanda(0, 0, 0, 0, 0, 0);
            VDCB.YTraslation = yTraslation;

            if (ReportSetting.CorpiDocumento != null)
            {
                foreach (var AttributiColonna in ReportSetting.CorpiDocumento)
                {
                    VDCB.Width = (Units.Centimeters * (float)AttributiColonna.Size);
                    TextObject Etichetta = CreaEtichetta(null, VDCB.XTraslation, VDCB.YTraslation, VDCB.Width);
                    if (IsRiporto)
                    {
                        if (Bordatura)
                            CreaBordo(Etichetta, true, true, true, false);
                        Page.PageHeader.Objects.Add(Etichetta);
                    }
                    else
                    {
                        if (Bordatura)
                            CreaBordo(Etichetta, true, false, true, true);
                        Page.ColumnFooter.Objects.Add(Etichetta);
                    }

                    VDCB.ContatoreColonna++;
                    VDCB.ContatoreRiga = 0;

                    if (VDCB.ContatoreColonna != 0)
                    { VDCB.XTraslation = VDCB.XTraslation + VDCB.Width; }
                    else { VDCB.XTraslation = 0; }
                    VDCB.YTraslation = yTraslation;
                }
            }
        }
        private CorpoDocumento RicercaAttributoCorpiDcoumentoInRigaColonna(int contatoreColonna, int contatoreRiga)
        {
            int _ContatoreRiga = 0;
            int _ContatoreColonna = 0;

            foreach (var AttributiColonna in ReportSetting.CorpiDocumento)
            {
                foreach (var Attributo in AttributiColonna.CorpoColonna)
                {
                    if (_ContatoreRiga == contatoreRiga && _ContatoreColonna == contatoreColonna)
                    {
                        return Attributo;
                    }
                    _ContatoreRiga++;
                }
                _ContatoreColonna++;
                _ContatoreRiga = 0;
            }

            return new CorpoDocumento();
        }
        private GroupHeaderBand FindLastGroup(GroupHeaderBand GroupHeaderBand)
        {
            if (GroupHeaderBand.NestedGroup == null)
            {
                return GroupHeaderBand;
            }
            else
            {
                return FindLastGroup(GroupHeaderBand.NestedGroup);
            }
        }
        private ChildBand FindLastChild(ChildBand ChildBand)
        {
            if (ChildBand.Child == null)
            {
                return ChildBand;
            }
            else
            {
                return FindLastChild(ChildBand.Child);
            }
        }
        private void CreateGroupHeaderSettingData(GroupHeaderBand Header, string Title, OpzioniDiStampa opzioniDiStampa)
        {
            CreateGroupSettingData(Header, true, Title);
            Header.Condition = "[" + DbName + "." + JReportHelper.ReplaceSymbolNotAllowInReport(Title) + "]";
            Header.SortOrder = SortOrder.None;

            if (opzioniDiStampa != null)
            {
                if (opzioniDiStampa.IsCheckedNuovaPagina) { Header.StartNewPage = true; }
            }
        }
        private GroupFooterBand CreateGroupFooterSettingData(string Title)
        {
            GroupFooterBand Footer = new GroupFooterBand();
            CreateGroupSettingData(Footer, false, Title);
            
            return Footer;
        }
        private void CreateGroupSettingData(HeaderFooterBandBase HeaderFooterBandBase, bool IsHeader, string Title)
        {
            HeaderFooterBandBase.CanGrow = true;
            HeaderFooterBandBase.CanBreak = true;
            HeaderFooterBandBase.CanShrink = true;
            if (IsHeader)
                HeaderFooterBandBase.Name = StampeKeys.ConstGroupHeader + JReportHelper.ReplaceSymbolNotAllowInReport(Title);
            else
                HeaderFooterBandBase.Name = StampeKeys.ConstGroupFooter + JReportHelper.ReplaceSymbolNotAllowInReport(Title);
            HeaderFooterBandBase.Height = DefaultHeightBand / 2;

        }
        private List<RaggruppamentiValori> RicercaAttributoPerEspressioneFunzioneInRiga(List<RaggruppamentiDocumento> raggruppamentiDocumento, int contatoreRiga)
        {
            int _ContatoreRiga = 0;
            int _ContatoreColonna = 0;
            List<RaggruppamentiValori> RaggruppamentiDocumento = new List<RaggruppamentiValori>();

            foreach (var AttributiColonna in raggruppamentiDocumento)
            {
                foreach (var Attributo in AttributiColonna.RaggruppamentiValori)
                {
                    if (_ContatoreRiga == contatoreRiga)
                    {
                        RaggruppamentiDocumento.Add(Attributo);
                    }
                    _ContatoreRiga++;
                }
                _ContatoreColonna++;
                _ContatoreRiga = 0;
            }

            return RaggruppamentiDocumento;
        }
        public void CreateSubreportStructure(bool FromDocumento = false)
        {
            Subreport = new SubreportObject();
            Subreport.ReportPage = new ReportPage();
        }
    }

    public class VariabiliDiCicloBanda
    {
        public int ContatoreRiga { get; set; }
        public int ContatoreColonna { get; set; }
        public float YTraslation { get; set; }
        public float XTraslation { get; set; }
        public float Width { get; set; }
        public float WidthEtichetta { get; set; }
        public VariabiliDiCicloBanda(int contatoreRiga, int contatoreColonna, float yTraslation, float xTraslation, float width, float widthEtichetta)
        {
            ContatoreRiga = contatoreRiga;
            ContatoreColonna = contatoreRiga;
            YTraslation = yTraslation;
            XTraslation = xTraslation;
            Width = width;
            WidthEtichetta = widthEtichetta;
        }
    }

    public class ValoreProprietaConPagina
    {
        public double Pagina { get; set; }
        public double Valore { get; set; }
    }
}

