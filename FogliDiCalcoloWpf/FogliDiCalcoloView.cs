using _3DModelExchange;
using AttivitaWpf;
using AttivitaWpf.View;
using CommonResources;
using Commons;
using DevExpress.XtraPrinting.Native;
using FogliDiCalcoloWpf.View;
using MasterDetailModel;
using MasterDetailView;
using Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;

namespace FogliDiCalcoloWpf
{
    public class FogliDiCalcoloView : SectionItemTemplateView
    {
        public ClientDataService DataService { get; set; } = null;
        public IEntityWindowService WindowService { get; set; } = null;
        public ModelActionsStack ModelActionsStack { get; set; } = null;
        public IMainOperation MainOperation { get; set; } = null;
        public AttivitaWpf.View.GanttView GanttView { get; set; }

        public FoglioDiCalcoloDataView FoglioDiCalcoloDataView { get; set; }//Dati del Computo
        public FoglioDiCalcoloGanttDataView FoglioDiCalcoloGanttDataView { get; set; }//Dati del Gantt
        public FoglioDiCalcoloWBSGanttSchedDataView FoglioDiCalcoloGanttSchedDataView { get; set; }//dati per Schedulazione
        public event EventHandler SerializedDataChanged;
        public event EventHandler SerializedDataToChange;
        public RibbonView RibbonView { get; set; }
           
        public I3DModelService Model3dService { get; set; }

        public FogliDiCalcoloView()
        {

        }

        public int Code { get => (int)RootSectionItemsId.FogliDiCalcolo; }
        

        public void Init()
        {
            OnSerializedDataChanged(new EventArgs());
        }

        public void LoadData()
        {
            if (RibbonView == null)
            {
                RibbonView = new RibbonView();
            }

            RibbonView.SALAttibutes = new System.Collections.ObjectModel.ObservableCollection<RibbonAttributeView>();
            var GanttData = DataService?.GetGanttData();
            if (GanttData?.ProgrammazioneSAL != null)
            {
                foreach (var attributo in GanttData.ProgrammazioneSAL.AttributiStandard)
                {
                    if (attributo.ProgressiveAmount)
                        RibbonView.SALAttibutes.Add(new RibbonAttributeView() { Codice = attributo.CodiceOrigine, Etichetta = attributo.Etichetta, DefinizioneAttributo = attributo.DefinizioneAttributo });
                }
            }
            RibbonView.SALAttibute = RibbonView.SALAttibutes.FirstOrDefault();

        }

        public DataTable GeneraDataSource(string sezioneKey, string sheetName = null)
        {
            if (sezioneKey == BuiltInCodes.EntityType.WBS)
            {
                FogliDiCalcoloData fogliDiCalcoloData = DataService.GetFogliDiCalcoloData();
                var foglio = fogliDiCalcoloData.FoglioDiCalcolo.Where(f => f.SezioneKey == sezioneKey && f.Foglio == sheetName).FirstOrDefault();
                if (foglio != null)
                {
                    var AttibutiUtilizzati = foglio.AttributiStandardFoglioDiCalcolo;
                    FogliDiCalcoloDataGanttSourceGenerator fogliDiCalcoloDataGanttSourceGenerator = new FogliDiCalcoloDataGanttSourceGenerator(DataService);
                    fogliDiCalcoloDataGanttSourceGenerator.Init(AttibutiUtilizzati);
                    fogliDiCalcoloDataGanttSourceGenerator.PrepareProgressiveValuesPerDateEnd();
                    fogliDiCalcoloDataGanttSourceGenerator.GetSALProgrammatoView(AttibutiUtilizzati);
                    //fogliDiCalcoloDataGanttSourceGenerator.CreateGanttDataSource(sezioneKey, MainOperation, AttibutiUtilizzati);
                    //return fogliDiCalcoloDataGanttSourceGenerator.GetDataTable();
                    return fogliDiCalcoloDataGanttSourceGenerator.GetDataTableFormSALProgrammatoView(AttibutiUtilizzati);
                }
            }
            else if (sezioneKey.StartsWith(BuiltInCodes.EntityType.WBS) && sezioneKey.EndsWith(FogliDiCalcoloKeys.ConstSAL))
            {
                var GanttData = DataService.GetGanttData();
                FogliDiCalcoloDataGanttSourceGenerator fogliDiCalcoloDataGanttSourceGenerator = new FogliDiCalcoloDataGanttSourceGenerator(DataService);
                List<AttributoFoglioDiCalcolo> AttibutiUtilizzati = GetAttributiUtilizzati(GanttData);
                fogliDiCalcoloDataGanttSourceGenerator.Init(AttibutiUtilizzati);
                fogliDiCalcoloDataGanttSourceGenerator.PrepareProgressiveValuesPerDateEnd();
                List<SALProgrammatoView> salProgrammati = new List<SALProgrammatoView>();
                if (GanttData.ProgrammazioneSAL != null)
                {
                    if (sezioneKey.Contains(FogliDiCalcoloKeys.ConstProg))
                    {
                        if (GanttData.ProgrammazioneSAL.PuntiNotevoliPerData != null)
                        {
                            fogliDiCalcoloDataGanttSourceGenerator.GetSALProgrammatoView(AttibutiUtilizzati, GanttData.ProgrammazioneSAL.PuntiNotevoliPerData.Select(d => d.Data).ToList()
                            , GanttData.ProgrammazioneSAL.PuntiNotevoliPerData.Select(d => d.IsSAL).ToList());
                        }
                    }
                    else
                    {
                        if (GanttData.ProgrammazioneSAL.PuntiNotevoliPerData != null)
                        {
                            fogliDiCalcoloDataGanttSourceGenerator.GetSALProgrammatoView(AttibutiUtilizzati, GanttData.ProgrammazioneSAL.PuntiNotevoliPerData.Where(s => s.IsSAL).Select(d => d.Data).ToList()
                            , GanttData.ProgrammazioneSAL.PuntiNotevoliPerData.Where(s => s.IsSAL).Select(d => d.IsSAL).ToList());
                        }
                    }

                }
                return fogliDiCalcoloDataGanttSourceGenerator.GetDataTableFormSALProgrammatoView(GanttData, RibbonView.SALAttibute?.Codice, RibbonView.SALAttibute?.Etichetta);
            }
            //if (sezioneKey.StartsWith(BuiltInCodes.EntityType.WBS) && sezioneKey.EndsWith(FogliDiCalcoloKeys.ConstSched))
            //{
            //    var GanttData = DataService.GetGanttData();
            //    FogliDiCalcoloDataGanttSourceGenerator fogliDiCalcoloDataGanttSourceGenerator = new FogliDiCalcoloDataGanttSourceGenerator(DataService);
            //    List<AttributoFoglioDiCalcolo> AttibutiUtilizzati = FoglioDiCalcoloGanttSchedDataView.GetAttributi();
            //    fogliDiCalcoloDataGanttSourceGenerator.Init(AttibutiUtilizzati);
            //    fogliDiCalcoloDataGanttSourceGenerator.PrepareProgressiveValuesPerDateEnd();
            //    if (FoglioDiCalcoloGanttSchedDataView.Period.Value != null)
            //        fogliDiCalcoloDataGanttSourceGenerator.GeneraSchedulazioneValori(FoglioDiCalcoloGanttSchedDataView.Period.Key, AttibutiUtilizzati);
            //}
            else
            {
                FogliDiCalcoloDataSourceGenerator foglioDiCalcoloDataSourceGenerator = new FogliDiCalcoloDataSourceGenerator(DataService);
                foglioDiCalcoloDataSourceGenerator.CreateGenericDataSource(sezioneKey.Replace(FogliDiCalcoloKeys.ConstSched, ""), MainOperation);
                return foglioDiCalcoloDataSourceGenerator.GetDataTable();
            }
            return new DataTable();
        }

        public List<SALProgrammatoView> GeneraDataSourceSched(string sezioneKey, string sheetName = null)
        {
            var GanttData = DataService.GetGanttData();
            DateTime dateFrom = new DateTime();
            int periodoKey = 0;
            FogliDiCalcoloDataGanttSourceGenerator fogliDiCalcoloDataGanttSourceGenerator = new FogliDiCalcoloDataGanttSourceGenerator(DataService);
            List<AttributoFoglioDiCalcolo> AttibutiUtilizzati = new List<AttributoFoglioDiCalcolo>();
            if (GanttData.SchedulazioneValori != null)
            {
                AttibutiUtilizzati = GanttData.SchedulazioneValori.Attributi;
                dateFrom = GanttData.SchedulazioneValori.DateFrom;
                periodoKey = GanttData.SchedulazioneValori.Periodo;
            }

            fogliDiCalcoloDataGanttSourceGenerator.Init(AttibutiUtilizzati);
            fogliDiCalcoloDataGanttSourceGenerator.PrepareProgressiveValuesPerDateEnd();
            //if (FoglioDiCalcoloGanttSchedDataView.Period.Value != null)
            return fogliDiCalcoloDataGanttSourceGenerator.GeneraSchedulazioneValori(periodoKey, AttibutiUtilizzati, dateFrom);
            //return new List<SALProgrammatoView>();
        }

        private List<AttributoFoglioDiCalcolo> GetAttributiUtilizzati(GanttData ganttData)
        {
            List<AttributoFoglioDiCalcolo> AttibutiUtilizzati = new List<AttributoFoglioDiCalcolo>();

            if (ganttData.ProgrammazioneSAL != null)
            {
                foreach (AttributoFoglioDiCalcolo item in ganttData.ProgrammazioneSAL.AttributiStandard.Where(a => a.ProgressiveAmount || a.Amount || a.ProductivityPerHour))
                {
                    AttributoFoglioDiCalcolo attributoFoglioDiCalcolo = new AttributoFoglioDiCalcolo();
                    attributoFoglioDiCalcolo.CodiceOrigine = item.CodiceOrigine;
                    attributoFoglioDiCalcolo.Etichetta = item.Etichetta;
                    attributoFoglioDiCalcolo.Formula = item.Formula;
                    attributoFoglioDiCalcolo.Note = item.Note;
                    attributoFoglioDiCalcolo.Amount = true;
                    attributoFoglioDiCalcolo.ProgressiveAmount = true;
                    attributoFoglioDiCalcolo.ProductivityPerHour = true;
                    AttibutiUtilizzati.Add(attributoFoglioDiCalcolo);
                }
            }
            return AttibutiUtilizzati;
        }
        public List<AttibutiFogiloDiCalcoloView> GetColumns(string sezioneKey, string sheetName = null)
        {
            List<AttibutiFogiloDiCalcoloView> attributoFoglioDiCalcolo = new List<AttibutiFogiloDiCalcoloView>();

            if (sezioneKey == BuiltInCodes.EntityType.WBS)
            {
                AttibutiFogiloDiCalcoloView attributoFiltrato = new AttibutiFogiloDiCalcoloView();
                attributoFiltrato.CodiceOrigine = "Data";
                attributoFiltrato.Etichetta = "Data";
                attributoFiltrato.EntityTypeKey = BuiltInCodes.EntityType.WBS;
                attributoFoglioDiCalcolo.Add(attributoFiltrato);

                FogliDiCalcoloData fogliDiCalcoloData = DataService.GetFogliDiCalcoloData();
                //foreach (var item in fogliDiCalcoloData.FoglioDiCalcolo.Where(f => f.SezioneKey == sezioneKey && f.Foglio == sheetName).FirstOrDefault().AttributiStandardFoglioDiCalcolo)
                var foglio = fogliDiCalcoloData.FoglioDiCalcolo.FirstOrDefault(f => f.SezioneKey == sezioneKey && f.Foglio == sheetName);
                if (foglio != null)
                {
                    foreach (var item in foglio.AttributiStandardFoglioDiCalcolo)
                    {
                        if (item.Amount)
                        {
                            AttibutiFogiloDiCalcoloView attributoFiltratoDelta = new AttibutiFogiloDiCalcoloView();
                            attributoFiltratoDelta.CodiceOrigine = item.CodiceOrigine;
                            attributoFiltratoDelta.DefinizioneAttributo = item.DefinizioneAttributo;
                            attributoFiltratoDelta.Etichetta = item.Etichetta + GanttKeys.LocalizeDelta;
                            attributoFiltratoDelta.Amount = item.Amount;
                            attributoFiltratoDelta.EntityTypeKey = BuiltInCodes.EntityType.WBS;
                            attributoFiltratoDelta.SezioneRiferita = item.SezioneRiferita;
                            attributoFoglioDiCalcolo.Add(attributoFiltratoDelta);
                        }

                        if (item.ProgressiveAmount)
                        {
                            AttibutiFogiloDiCalcoloView attributoFiltratoProgressive = new AttibutiFogiloDiCalcoloView();
                            attributoFiltratoProgressive.CodiceOrigine = item.CodiceOrigine;
                            attributoFiltratoProgressive.DefinizioneAttributo = item.DefinizioneAttributo;
                            attributoFiltratoProgressive.Etichetta = item.Etichetta + GanttKeys.LocalizeCumulato;
                            attributoFiltratoProgressive.ProgressiveAmount = item.ProgressiveAmount;
                            attributoFiltratoProgressive.EntityTypeKey = BuiltInCodes.EntityType.WBS;
                            attributoFiltratoProgressive.SezioneRiferita = item.SezioneRiferita;
                            attributoFoglioDiCalcolo.Add(attributoFiltratoProgressive);
                        }

                        if (item.ProductivityPerHour)
                        {
                            AttibutiFogiloDiCalcoloView attributoFiltratoProduttivita = new AttibutiFogiloDiCalcoloView();
                            attributoFiltratoProduttivita.CodiceOrigine = item.CodiceOrigine;
                            attributoFiltratoProduttivita.DefinizioneAttributo = item.DefinizioneAttributo;
                            attributoFiltratoProduttivita.Etichetta = item.Etichetta + GanttKeys.LocalizeProduttivitaH;
                            attributoFiltratoProduttivita.ProductivityPerHour = item.ProductivityPerHour;
                            attributoFiltratoProduttivita.EntityTypeKey = BuiltInCodes.EntityType.WBS;
                            attributoFiltratoProduttivita.SezioneRiferita = item.SezioneRiferita;
                            attributoFoglioDiCalcolo.Add(attributoFiltratoProduttivita);
                        }
                    }
                }
            }
            else if (sezioneKey.StartsWith(BuiltInCodes.EntityType.WBS) && sezioneKey.EndsWith(FogliDiCalcoloKeys.ConstSAL))
            {
                AttibutiFogiloDiCalcoloView attributoFiltrato = new AttibutiFogiloDiCalcoloView();
                attributoFiltrato.CodiceOrigine = "Data";
                attributoFiltrato.Etichetta = LocalizationProvider.GetString("Data");
                attributoFiltrato.EntityTypeKey = BuiltInCodes.EntityType.WBS;
                attributoFoglioDiCalcolo.Add(attributoFiltrato);

                AttibutiFogiloDiCalcoloView attributoFiltratoPeriodo = new AttibutiFogiloDiCalcoloView();
                attributoFiltratoPeriodo.CodiceOrigine = "GiorniPeriodo";
                attributoFiltratoPeriodo.Etichetta = LocalizationProvider.GetString("GiorniPeriodo");
                attributoFiltratoPeriodo.EntityTypeKey = BuiltInCodes.EntityType.WBS;
                attributoFoglioDiCalcolo.Add(attributoFiltratoPeriodo);

                AttibutiFogiloDiCalcoloView attributoFiltratoProgressivo = new AttibutiFogiloDiCalcoloView();
                attributoFiltratoProgressivo.CodiceOrigine = "GiorniProgressivo";
                attributoFiltratoProgressivo.Etichetta = LocalizationProvider.GetString("GiorniProg");
                attributoFiltratoProgressivo.EntityTypeKey = BuiltInCodes.EntityType.WBS;
                attributoFoglioDiCalcolo.Add(attributoFiltratoProgressivo);

                var ganttData = DataService.GetGanttData();
                if (ganttData.ProgrammazioneSAL != null)
                {
                    foreach (var item in ganttData.ProgrammazioneSAL.AttributiStandard)
                    {
                        if (item.Amount)
                        {
                            AttibutiFogiloDiCalcoloView attributoFiltratoDelta = new AttibutiFogiloDiCalcoloView();
                            attributoFiltratoDelta.CodiceOrigine = item.CodiceOrigine;
                            attributoFiltratoDelta.DefinizioneAttributo = item.DefinizioneAttributo;
                            attributoFiltratoDelta.Etichetta = item.Etichetta + GanttKeys.LocalizeDelta;
                            attributoFiltratoDelta.Amount = item.Amount;
                            attributoFiltratoDelta.EntityTypeKey = BuiltInCodes.EntityType.WBS;
                            attributoFiltratoDelta.SezioneRiferita = item.SezioneRiferita;
                            attributoFoglioDiCalcolo.Add(attributoFiltratoDelta);
                        }

                        if (item.ProgressiveAmount)
                        {
                            AttibutiFogiloDiCalcoloView attributoFiltratoProgressive = new AttibutiFogiloDiCalcoloView();
                            attributoFiltratoProgressive.CodiceOrigine = item.CodiceOrigine;
                            attributoFiltratoProgressive.DefinizioneAttributo = item.DefinizioneAttributo;
                            attributoFiltratoProgressive.Etichetta = item.Etichetta + GanttKeys.LocalizeCumulato;
                            attributoFiltratoProgressive.ProgressiveAmount = item.ProgressiveAmount;
                            attributoFiltratoProgressive.EntityTypeKey = BuiltInCodes.EntityType.WBS;
                            attributoFiltratoProgressive.SezioneRiferita = item.SezioneRiferita;
                            attributoFoglioDiCalcolo.Add(attributoFiltratoProgressive);
                        }

                        if (item.ProductivityPerHour)
                        {
                            AttibutiFogiloDiCalcoloView attributoFiltratoProduttivita = new AttibutiFogiloDiCalcoloView();
                            attributoFiltratoProduttivita.CodiceOrigine = item.CodiceOrigine;
                            attributoFiltratoProduttivita.DefinizioneAttributo = item.DefinizioneAttributo;
                            attributoFiltratoProduttivita.Etichetta = item.Etichetta + GanttKeys.LocalizeProduttivitaH;
                            attributoFiltratoProduttivita.ProductivityPerHour = item.ProductivityPerHour;
                            attributoFiltratoProduttivita.EntityTypeKey = BuiltInCodes.EntityType.WBS;
                            attributoFiltratoProduttivita.SezioneRiferita = item.SezioneRiferita;
                            attributoFoglioDiCalcolo.Add(attributoFiltratoProduttivita);
                        }

                        if (item.ProgressiveAmount)
                        {
                            AttibutiFogiloDiCalcoloView attributoFiltratoPercProgressive = new AttibutiFogiloDiCalcoloView();
                            attributoFiltratoPercProgressive.CodiceOrigine = item.CodiceOrigine;
                            attributoFiltratoPercProgressive.DefinizioneAttributo = item.DefinizioneAttributo;
                            attributoFiltratoPercProgressive.Etichetta = item.Etichetta + GanttKeys.LocalizeValorePercentualeProgressiva;
                            attributoFiltratoPercProgressive.ProgressiveAmount = item.ProgressiveAmount;
                            attributoFiltratoPercProgressive.EntityTypeKey = BuiltInCodes.EntityType.WBS;
                            attributoFiltratoPercProgressive.SezioneRiferita = item.SezioneRiferita;
                            attributoFoglioDiCalcolo.Add(attributoFiltratoPercProgressive);
                        }
                    }

                    if (sezioneKey.Contains(FogliDiCalcoloKeys.ConstProg))
                    {
                        AttibutiFogiloDiCalcoloView attributoFiltratoSAL = new AttibutiFogiloDiCalcoloView();
                        attributoFiltratoSAL.CodiceOrigine = GanttKeys.ConstSAL;
                        attributoFiltratoSAL.Etichetta = GanttKeys.ConstSAL;
                        if (RibbonView.SALAttibute != null)
                            attributoFiltratoSAL.Etichetta = attributoFiltratoSAL.Etichetta + " (" + RibbonView.SALAttibute.Etichetta + ")";
                        attributoFoglioDiCalcolo.Add(attributoFiltratoSAL);
                    }
                }
            }
            else if (sezioneKey.StartsWith(BuiltInCodes.EntityType.WBS) && sezioneKey.EndsWith(FogliDiCalcoloKeys.ConstSched))
            {
                


                FogliDiCalcoloData fogliDiCalcoloData = DataService.GetFogliDiCalcoloData();

                AttibutiFogiloDiCalcoloView attibutiFogiloDiCacoloViewCodice = new AttibutiFogiloDiCalcoloView();
                attibutiFogiloDiCacoloViewCodice.CodiceOrigine = BuiltInCodes.Attributo.Codice;
                attibutiFogiloDiCacoloViewCodice.Etichetta = LocalizationProvider.GetString("Codice");
                attibutiFogiloDiCacoloViewCodice.EntityTypeKey = BuiltInCodes.EntityType.WBS;
                attributoFoglioDiCalcolo.Insert(0, attibutiFogiloDiCacoloViewCodice);

                AttibutiFogiloDiCalcoloView attibutiFogiloDiCacoloViewDescr = new AttibutiFogiloDiCalcoloView();
                attibutiFogiloDiCacoloViewDescr.CodiceOrigine = BuiltInCodes.Attributo.Nome;
                attibutiFogiloDiCacoloViewDescr.Etichetta = LocalizationProvider.GetString("Descrizione");
                attibutiFogiloDiCacoloViewDescr.EntityTypeKey = BuiltInCodes.EntityType.WBS;
                attributoFoglioDiCalcolo.Insert(1, attibutiFogiloDiCacoloViewDescr);

                var ganttData = DataService.GetGanttData();
                if (ganttData.SchedulazioneValori != null)
                {
                    foreach (var item in ganttData.SchedulazioneValori.Attributi)
                    {
                        if (item.Amount)
                        {
                            AttibutiFogiloDiCalcoloView attributoFiltratoDelta = new AttibutiFogiloDiCalcoloView();
                            attributoFiltratoDelta.CodiceOrigine = item.CodiceOrigine;
                            attributoFiltratoDelta.DefinizioneAttributo = item.DefinizioneAttributo;
                            attributoFiltratoDelta.Etichetta = item.Etichetta;
                            attributoFiltratoDelta.Amount = item.Amount;
                            attributoFiltratoDelta.EntityTypeKey = BuiltInCodes.EntityType.WBS;
                            attributoFiltratoDelta.SezioneRiferita = item.SezioneRiferita;
                            attributoFoglioDiCalcolo.Add(attributoFiltratoDelta);
                        }
                    }
                }
            }
            else
            {
                EntitiesHelper entitiesHelper = new EntitiesHelper(DataService);
                FogliDiCalcoloData fogliDiCalcoloData = DataService.GetFogliDiCalcoloData();

                var columnsCountByAttributo = GetAttributoColumnsCount(sezioneKey);

                IOrderedEnumerable<Attributo> attributi = DataService.GetEntityTypes()[sezioneKey].Attributi.Values.OrderBy(item => item.DetailViewOrder);

                foreach (Attributo att in attributi)
                {
                    if (att.IsInternal)
                        continue;

                    if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid)
                    {
                        continue;
                    }
                    if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Riferimento)
                    {
                        if (entitiesHelper.GetSourceAttributo(att).DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid)
                        {
                            continue;
                        }
                    }



                    int attColumnsCount = 0;
                    columnsCountByAttributo.TryGetValue(att.Codice, out attColumnsCount);

                    string pathPrefix = string.Empty;
                    for (int iCol = 0; iCol < attColumnsCount; iCol++)
                    {
                        AttibutiFogiloDiCalcoloView attributoFiltrato = new AttibutiFogiloDiCalcoloView();
                        attributoFiltrato.CodiceOrigine = att.Codice;
                        attributoFiltrato.Etichetta = string.Format("{0}{1}", pathPrefix, att.Etichetta);

                        attributoFiltrato.EntityTypeKey = att.EntityTypeKey;
                        attributoFiltrato.DefinizioneAttributo = entitiesHelper.GetSourceAttributo(att).DefinizioneAttributoCodice;
                        attributoFiltrato.IsChecked = true;
                        attributoFoglioDiCalcolo.Add(attributoFiltrato);

                        pathPrefix += FogliDiCalcoloDataSourceGenerator.HeaderTreeLevelSymbol;
                    }


                    //AttibutiFogiloDiCalcoloView attributoFiltrato = new AttibutiFogiloDiCalcoloView();
                    //attributoFiltrato.CodiceOrigine = att.Codice;
                    //attributoFiltrato.Etichetta = att.Etichetta;
                    //attributoFiltrato.EntityTypeKey = att.EntityTypeKey;
                    //attributoFiltrato.DefinizioneAttributo = entitiesHelper.GetSourceAttributo(att).DefinizioneAttributoCodice;
                    //attributoFiltrato.IsChecked = true;
                    //attributoFoglioDiCalcolo.Add(attributoFiltrato);






                }

                //SOSTITUITA CON PEZZO SOPRA, IN FASE DI AGGIORNAMENTO AGGIUNGE EVENTUALI ATTRIBUTI MENTRE PRIMA AGGIORNAVA LE IMPOSTAZIONI SALVATE DELLA MASCHERA
                //foreach (var item in fogliDiCalcoloData.FoglioDiCalcolo.Where(f => f.SezioneKey == sezioneKey).FirstOrDefault().AttributiStandardFoglioDiCalcolo)
                //{
                //    AttibutiFogiloDiCalcoloView attributoFiltrato = new AttibutiFogiloDiCalcoloView();
                //    attributoFiltrato.CodiceOrigine = item.CodiceOrigine;
                //    attributoFiltrato.DefinizioneAttributo = item.DefinizioneAttributo;
                //    attributoFiltrato.Etichetta = item.Etichetta;
                //    attributoFoglioDiCalcolo.Add(attributoFiltrato);
                //}

                foreach (var item in fogliDiCalcoloData.FoglioDiCalcolo.Where(f => f.SezioneKey == sezioneKey).FirstOrDefault().AttributiFormuleFoglioDiCalcolo)
                {
                    AttibutiFogiloDiCalcoloView attibutiFogiloDiCacoloView = new AttibutiFogiloDiCalcoloView();
                    attibutiFogiloDiCacoloView.CodiceOrigine = item.CodiceOrigine;
                    attibutiFogiloDiCacoloView.Etichetta = item.Etichetta;
                    attibutiFogiloDiCacoloView.EntityTypeKey = sezioneKey;
                    attibutiFogiloDiCacoloView.SezioneRiferita = item.SezioneRiferita;
                    attibutiFogiloDiCacoloView.Formula = item.Formula;
                    attibutiFogiloDiCacoloView.Note = item.Note;
                    attributoFoglioDiCalcolo.Add(attibutiFogiloDiCacoloView);
                }
            }


            return attributoFoglioDiCalcolo;
        }

        public string GetFormatoSchedulazione()
        {
            string formatDate = "dd/mm/yy";
            var ganttData = DataService.GetGanttData();
            if (ganttData.SchedulazioneValori != null)
            {
                if (ganttData.SchedulazioneValori.Periodo == 2)
                {
                    formatDate = "mmmm yyyy";
                }
                if (ganttData.SchedulazioneValori.Periodo == 3)
                {
                    formatDate = "yyyy";
                }
            }
            return formatDate;
        }
        public bool CheckIfWriteDataColumn()
        {
            if (GanttView != null)
            {
                if (GanttView.IsActiveCalendario)
                    return true;
                else
                    return false;
            }
            else
                return true;
        }

        protected void OnSerializedDataChanged(EventArgs e)
        {
            SerializedDataChanged?.Invoke(this, e);
        }

        protected void OnSerializedDataToChange(EventArgs e)
        {
            SerializedDataToChange?.Invoke(this, e);
        }

        public void UpdateFogliDiCalcoloData()
        {
            OnSerializedDataToChange(new EventArgs());
        }

        public static string ToCellFormat(string format)
        {
            string res = format;
            NumberFormat nf = NumericFormatHelper.DecomposeFormat(format);

            if (nf != null)
            {
                if (!string.IsNullOrEmpty(nf.SymbolText))
                {
                    if (nf.SymbolText == "%")
                        res = format.Replace(nf.SymbolText, string.Format("{0}", nf.SymbolText));
                    else
                        res = format.Replace(nf.SymbolText, string.Format("\"{0}\"", nf.SymbolText));
                }
            }
            //if (nf != null && !string.IsNullOrEmpty(nf.SymbolText))
            //    res = format.Replace(nf.SymbolText, string.Format("\"{0}\"", nf.SymbolText));

            return res;
        }

        /// <summary>
        /// Calcola il numero di colonne da inserire per ogni attributo (i riferimenti ad alberi vengono divisi per livelli)
        /// </summary>
        /// <param name="entityTypeKey"></param>
        /// <returns></returns>
        public Dictionary<string, int> GetAttributoColumnsCount(string entityTypeKey)
        {
            //key: codiceAttributo, value: numero colonne
            Dictionary<string, int> columnsCount = new Dictionary<string, int>();

            EntitiesHelper entsHelper = new EntitiesHelper(DataService);

            var entsInfo = DataService.GetFilteredEntities(entityTypeKey, null, null, null, out _);


            foreach (var entInfo in entsInfo)
            {
                Entity ent = DataService.GetEntityById(entityTypeKey, entInfo.Id);


                foreach (Attributo att in entsHelper.GetAttributi(ent).Values)
                {
                    if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid || att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.GuidCollection)
                        continue;

                    columnsCount.TryAdd(att.Codice, 1);

                    EntityAttributo sourceEntAtt = entsHelper.GetSourceEntityAttributo(ent, att.Codice);

                    if (sourceEntAtt != null && sourceEntAtt.Entity is TreeEntity)
                    {
                        TreeEntity treeEnt = sourceEntAtt.Entity as TreeEntity;

                        if (entsHelper.IsAttributoDeep(treeEnt.EntityTypeCodice, sourceEntAtt.AttributoCodice))
                        {
                            if (treeEnt != null && treeEnt.Depth >= columnsCount[att.Codice])
                                columnsCount[att.Codice] = treeEnt.Depth + 1;
                        }
                    }
                }
            }

            return columnsCount;

        }

    }

    public class ValorePerColonna
    {
        public string Colonna { get; set; }
        public double Valore { get; set; }
    }
}
