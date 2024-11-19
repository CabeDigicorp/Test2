using AttivitaWpf;
using MasterDetailModel;
using Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FogliDiCalcoloWpf
{
    public class FogliDiCalcoloDataGanttSourceGenerator : ProgrammazioneSALCalculator
    {
        public FogliDiCalcoloDataGanttSourceGenerator(ClientDataService dataService) : base(dataService)
        {

        }
        //private List<dynamic> UnknowDatasource;
        //private FilterData filter;
        //private EntitiesHelper entitiesHelper;
        //private AttributoFormatHelper attributoFormatHelper;
        //private ClientDataService DataService;
        //private List<DateTimeWBSItemEntity> DateTimeWBSItemEntitys = new List<DateTimeWBSItemEntity>();
        //public List<AttributoFoglioDiCalcolo> attributoFoglioDiCalcolo { get; set; }

        //public DataTable DataTable { get; set; }

        //public FogliDiCalcoloDataGanttSourceGenerator(ClientDataService dataService)
        //{
        //    UnknowDatasource = new List<dynamic>();
        //    DataService = dataService;
        //}
        //public void CreateGanttDataSource(string SezioneItem, IMainOperation MainOperation, List<AttributoFoglioDiCalcolo> attibutiUtilizzati)
        //{

        //    ViewSettings viewSettings = DataService.GetViewSettings();
        //    if (!viewSettings.EntityTypes.ContainsKey(SezioneItem))
        //        return;

        //    EntityTypeViewSettings EntityViewSettings = viewSettings.EntityTypes[SezioneItem];

        //    filter = new FilterData();
        //    EntityViewSettings.Filters.ForEach(item =>
        //    {
        //        AttributoFilterData attFilter = new AttributoFilterData()
        //        {
        //            CodiceAttributo = item.CodiceAttributo,
        //            CheckedValori = new HashSet<string>(item.CheckedValori),
        //            EntityTypeKey = SezioneItem,
        //            IsFiltroAttivato = true,
        //        };
        //        filter.Items.Add(attFilter);
        //    });

        //    entitiesHelper = new EntitiesHelper(DataService);
        //    attributoFormatHelper = new AttributoFormatHelper(DataService);

        //    UnknowDatasource.Clear();

        //    HashSet<Guid> FilteredEntitiesIds = new HashSet<Guid>();

        //    List<TreeEntity> WBSItems = RetrieveWBSItem(SezioneItem, FilteredEntitiesIds);

        //    CreateDateScaleWithWBSItems(WBSItems);

        //    GenerateDatasource(attibutiUtilizzati);
        //}

        //private List<TreeEntity> RetrieveWBSItem(string sezioneItem, HashSet<Guid> filteredEntitiesIds)
        //{
        //    List<Guid> entitiesFound = null;
        //    List<TreeEntity> TreeEntities = new List<TreeEntity>();
        //    List<Entity> Entities = new List<Entity>();
        //    Dictionary<string, EntityType> EntitiesList = DataService.GetEntityTypes();
        //    EntityType EntitySelected = EntitiesList[BuiltInCodes.EntityType.WBS];
        //    List<TreeEntityMasterInfo> TreeInfo = DataService.GetFilteredTreeEntities(BuiltInCodes.EntityType.WBS, null, null, out entitiesFound);
        //    TreeInfo = EntitiesHelper.TreeFilterById(TreeInfo, entitiesFound);
        //    TreeEntities = DataService.GetTreeEntitiesById(BuiltInCodes.EntityType.WBS, TreeInfo.Select(item => item.Id));
        //    return TreeEntities.Where(t => t.IsParent == false).ToList();
        //}

        //private void CreateDateScaleWithWBSItems(List<TreeEntity> wBSItems)
        //{
        //    DateTime Date = new DateTime();
        //    foreach (WBSItem Entity in wBSItems)
        //    {
        //        ValoreData DataInizio = (ValoreData)entitiesHelper.GetValoreAttributo(Entity, BuiltInCodes.Attributo.DataInizio, false, false);
        //        Date = (DateTime)DataInizio.V;
        //        DateTimeWBSItemEntity dateTimeWBSItemEntity = new DateTimeWBSItemEntity();

        //        if (DateTimeWBSItemEntitys.Where(d => d.Data == Date).FirstOrDefault() == null)
        //        {
        //            dateTimeWBSItemEntity.Data = Date;
        //            dateTimeWBSItemEntity.WBSItemsDataInizio.Add(Entity);
        //            DateTimeWBSItemEntitys.Add(dateTimeWBSItemEntity);
        //        }
        //        else
        //        {
        //            dateTimeWBSItemEntity = DateTimeWBSItemEntitys.Where(d => d.Data == Date).FirstOrDefault();
        //            dateTimeWBSItemEntity.WBSItemsDataInizio.Add(Entity);
        //        }


        //        ValoreData DataFine = (ValoreData)entitiesHelper.GetValoreAttributo(Entity, BuiltInCodes.Attributo.DataFine, false, false);
        //        Date = (DateTime)DataFine.V;
        //        dateTimeWBSItemEntity = new DateTimeWBSItemEntity();

        //        if (DateTimeWBSItemEntitys.Where(d => d.Data == Date).FirstOrDefault() == null)
        //        {
        //            dateTimeWBSItemEntity.Data = Date;
        //            dateTimeWBSItemEntity.WBSItemsDataInizio.Add(Entity);
        //            //dateTimeWBSItemEntity.WBSItemsDataFine.Add(Entity);
        //            DateTimeWBSItemEntitys.Add(dateTimeWBSItemEntity);
        //        }
        //        else
        //        {
        //            dateTimeWBSItemEntity = DateTimeWBSItemEntitys.Where(d => d.Data == Date).FirstOrDefault();
        //            dateTimeWBSItemEntity.WBSItemsDataInizio.Add(Entity);
        //            //dateTimeWBSItemEntity.WBSItemsDataFine.Add(Entity);
        //        }

        //    }

        //    DateTimeWBSItemEntitys = DateTimeWBSItemEntitys.OrderBy(s => s.Data).ToList();
        //}

        //private void GenerateDatasource(List<AttributoFoglioDiCalcolo> attibutiUtilizzati)
        //{
        //    List<string> AttributiRaleContabilita = new List<string>();
        //    attributoFoglioDiCalcolo = new List<AttributoFoglioDiCalcolo>();

        //    if (AttributiRaleContabilita.Count() == 0)
        //    {
        //        if (DateTimeWBSItemEntitys.FirstOrDefault() != null)
        //        {
        //            foreach (var item in DateTimeWBSItemEntitys.FirstOrDefault()?.WBSItemsDataInizio.FirstOrDefault()?.Attributi)
        //            {
        //                if (attibutiUtilizzati.Where(y => y.CodiceOrigine == item.Value.Attributo.Codice).FirstOrDefault() != null)
        //                {
        //                    if (item.Value.Attributo.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Reale || item.Value.Attributo.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Contabilita)
        //                    {
        //                        AttributiRaleContabilita.Add(item.Value.AttributoCodice);
        //                    }

        //                    if (item.Value.Attributo.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Riferimento)
        //                    {
        //                        Entity entRef = null;
        //                        Attributo attref = null;
        //                        Dictionary<string, EntityType> entTypes = DataService.GetEntityTypes();
        //                        AttributoRiferimento attributoRiferimento = (AttributoRiferimento)item.Value.Attributo;
        //                        attref = entitiesHelper.GetSourceAttributo(item.Value.Attributo);
        //                        if (attref.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Reale || attref.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Contabilita)
        //                        {
        //                            AttributiRaleContabilita.Add(item.Value.AttributoCodice);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    int DateCounter = 0;


        //    dynamic UnknowObjectLocal = new System.Dynamic.ExpandoObject();
        //    IList<DateTimeWBSItemEntity> DateTimeWBSItemEntitysJustIterated = new List<DateTimeWBSItemEntity>();
        //    IList<TreeEntity> TreeEntityJustCalculated = new List<TreeEntity>();
        //    Dictionary<string, double> DictionaryTotalProgressive = new Dictionary<string, double>();

        //    foreach (DateTimeWBSItemEntity dateTimeWBSItemEntity in DateTimeWBSItemEntitys)
        //    {
        //        UnknowObjectLocal = new System.Dynamic.ExpandoObject();
        //        AddProperty(UnknowObjectLocal, "Data", dateTimeWBSItemEntity.Data);

        //        if (DateCounter == 0)
        //        {
        //            foreach (string attributocodice in AttributiRaleContabilita)
        //            {
        //                AddProperty(UnknowObjectLocal, attributocodice + FogliDiCalcoloKeys.LocalizeDelta, 0.0);
        //                AddProperty(UnknowObjectLocal, attributocodice + FogliDiCalcoloKeys.LocalizeCumulato, 0.0);
        //                DictionaryTotalProgressive.Add(attributocodice, 0);
        //            }
        //            UnknowDatasource.Add(UnknowObjectLocal);
        //            DateTimeWBSItemEntitysJustIterated.Add(dateTimeWBSItemEntity);
        //            DateCounter++;
        //            continue;
        //        }

        //        TreeEntityJustCalculated.Clear();

        //        foreach (string attributocodice in AttributiRaleContabilita)
        //        {
        //            double TotalAmout = 0;
        //            double EuroOra = 0;

        //            foreach (DateTimeWBSItemEntity DateTimeWBSItemEntitysJustPrevious in DateTimeWBSItemEntitysJustIterated.Reverse())
        //            {
        //                foreach (TreeEntity wbsPrecedente in DateTimeWBSItemEntitysJustPrevious.WBSItemsDataInizio)
        //                {
        //                    DateTime DataInizioPrecedente = (DateTime)((WBSItem)wbsPrecedente).GetDataInizio();
        //                    DateTime DataFinePrecedente = (DateTime)((WBSItem)wbsPrecedente).GetDataFine();

        //                    bool Case1 = false;
        //                    bool Case2 = false;
        //                    bool Case3 = false;

        //                    if (DataInizioPrecedente < dateTimeWBSItemEntity.Data && DataFinePrecedente == dateTimeWBSItemEntity.Data)
        //                        Case1 = true;
        //                    if (dateTimeWBSItemEntity.Data >= DataInizioPrecedente && dateTimeWBSItemEntity.Data <= DataFinePrecedente)
        //                        Case2 = true;
        //                    if (TreeEntityJustCalculated.Where(t => t.Id == wbsPrecedente.Id).FirstOrDefault() == null)
        //                        Case3 = true;


        //                    if ((Case1 || Case2) && Case3)
        //                    {
        //                        double EuroPerMinute = 0;
        //                        ValoreReale valoreReale = null;
        //                        ValoreContabilita valoreContabilita = null;

        //                        if (entitiesHelper.GetValoreAttributo(wbsPrecedente, attributocodice, false, false) is ValoreReale)
        //                        {
        //                            valoreReale = (ValoreReale)entitiesHelper.GetValoreAttributo(wbsPrecedente, attributocodice, false, false);
        //                        }
        //                        else
        //                        {
        //                            valoreContabilita = (ValoreContabilita)entitiesHelper.GetValoreAttributo(wbsPrecedente, attributocodice, false, false);
        //                        }
        //                        ValoreReale Lavoro = (ValoreReale)entitiesHelper.GetValoreAttributo(wbsPrecedente, BuiltInCodes.Attributo.Lavoro, false, false);

        //                        if (valoreReale != null)
        //                            EuroPerMinute = (double)valoreReale?.RealResult / ((double)Lavoro.RealResult * 60);
        //                        if (valoreContabilita != null)
        //                            EuroPerMinute = (double)valoreContabilita?.RealResult / ((double)Lavoro.RealResult * 60);

        //                        CalendariItem calendario = GetCalendarioItem((WBSItem)wbsPrecedente);
        //                        DateTimeCalculator timeCalc = null;
        //                        timeCalc = new DateTimeCalculator(calendario.GetWeekHours(), calendario.GetCustomDays());
        //                        //ValoreData DataInzioWBS = (ValoreData)entitiesHelper.GetValoreAttributo(wbsPrecedente, BuiltInCodes.Attributo.DataInizio, false, false);
        //                        DateTime DataInzioIntervallo = DateTimeWBSItemEntitys.ElementAt(DateTimeWBSItemEntitys.FindIndex(a => a.Data == dateTimeWBSItemEntity.Data) - 1).Data;
        //                        double WorkingMinute = 0;
        //                        //WorkingMinute = timeCalc.GetWorkingMinutesBetween((DateTime)DataInzioWBS.V, dateTimeWBSItemEntity.Data);
        //                        WorkingMinute = timeCalc.GetWorkingMinutesBetween(DataInzioIntervallo, dateTimeWBSItemEntity.Data);
        //                        double amount = WorkingMinute * EuroPerMinute;
        //                        TotalAmout = TotalAmout + amount;
        //                        EuroOra = EuroOra + EuroPerMinute * 60;
        //                        TreeEntityJustCalculated.Add(wbsPrecedente);
        //                    }
        //                }
        //            }
        //            DictionaryTotalProgressive[attributocodice] = DictionaryTotalProgressive[attributocodice] + TotalAmout;

        //            if (attibutiUtilizzati.Where(a => a.CodiceOrigine == attributocodice && a.Amount).FirstOrDefault() !=null)
        //                AddProperty(UnknowObjectLocal, attributocodice + FogliDiCalcoloKeys.LocalizeDelta, TotalAmout);

        //            if (attibutiUtilizzati.Where(a => a.CodiceOrigine == attributocodice && a.ProgressiveAmount).FirstOrDefault() != null)
        //                AddProperty(UnknowObjectLocal, attributocodice + FogliDiCalcoloKeys.LocalizeCumulato, DictionaryTotalProgressive[attributocodice]);

        //            if (attibutiUtilizzati.Where(a => a.CodiceOrigine == attributocodice && a.ProductivityPerHour).FirstOrDefault() != null)
        //                AddProperty(UnknowObjectLocal, attributocodice + FogliDiCalcoloKeys.LocalizeProduttivitaH, EuroOra);

        //            TreeEntityJustCalculated.Clear();
        //        }

        //        DateCounter++;
        //        UnknowDatasource.Add(UnknowObjectLocal);
        //        DateTimeWBSItemEntitysJustIterated.Add(dateTimeWBSItemEntity);
        //    }

        //    string json = Newtonsoft.Json.JsonConvert.SerializeObject(UnknowDatasource);
        //    DataTable = JsonConvert.DeserializeObject<DataTable>(json);
        //}

        //private CalendariItem GetCalendarioItem(WBSItem ent)
        //{
        //    string wbsCalendarioIdCodice = string.Join(WBSItemType.AttributoCodiceSeparator, BuiltInCodes.EntityType.Calendari, BuiltInCodes.Attributo.Id);

        //    Guid wbsCalendarioId = ent.GetAttributoGuidId(wbsCalendarioIdCodice);
        //    if (wbsCalendarioId != Guid.Empty)
        //    {
        //        CalendariItem calendario = DataService.GetEntitiesById(BuiltInCodes.EntityType.Calendari, new List<Guid> { wbsCalendarioId }).FirstOrDefault() as CalendariItem;
        //        return calendario;
        //    }
        //    return null;
        //}

        //private void AddProperty(System.Dynamic.ExpandoObject expando, string propertyName, object propertyValue)
        //{
        //    var expandoDict = expando as IDictionary<string, object>;
        //    expandoDict.Add(propertyName, propertyValue);
        //}
    }

    //public class DateTimeWBSItemEntity
    //{
    //    public DateTime Data { get; set; }
    //    public List<TreeEntity> WBSItemsDataInizio { get; set; } = new List<TreeEntity>();
    //    public List<TreeEntity> WBSItemsDataFine { get; set; } = new List<TreeEntity>();
    //}
}

