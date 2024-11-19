using AttivitaWpf.View;
using CommonResources;
using Commons;
using DevExpress.ClipboardSource.SpreadsheetML;
using DevExpress.XtraRichEdit.Layout.Engine;
using MasterDetailModel;
using Model;
using Newtonsoft.Json;
using Syncfusion.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttivitaWpf
{
    public class ProgrammazioneSALCalculator
    {
        public static List<AttivitaPerCalendario> CalendariItems = new List<AttivitaPerCalendario>();
        List<TreeEntity> WBSItems = new List<TreeEntity>();
        List<TreeEntity> WBSItemsWithParents = new List<TreeEntity>();
        public DataTable DataTable { get; set; }

        private List<dynamic> UnknowDatasource;
        private FilterData filter;
        private EntitiesHelper entitiesHelper;
        private AttributoFormatHelper attributoFormatHelper;
        private IDataService DataService;
        private List<DateTimeWBSItemEntity> DateTimeWBSItemEntitys = new List<DateTimeWBSItemEntity>();
        List<string> AttributiRealeContabilita = new List<string>();
        private DateTime MaxDate = new DateTime();
        private DateTime MinDate = new DateTime(2500, 12, 31);
        private List<SALProgrammatoView> listaSALProgrammatiView = new List<SALProgrammatoView>();

        public ProgrammazioneSALCalculator(IDataService dataService)
        {
            UnknowDatasource = new List<dynamic>();
            attributoFormatHelper = new AttributoFormatHelper(dataService);
            DataService = dataService;
        }

        public void GetWBSItems()
        {
            WBSItems = RetrieveWBSItem(BuiltInCodes.EntityType.WBS, new HashSet<Guid>());
        }

        public void GetWBSItemsWithParents()
        {
            List<Guid> entitiesFound = null;
            List<TreeEntity> TreeEntities = new List<TreeEntity>();
            List<Entity> Entities = new List<Entity>();
            Dictionary<string, EntityType> EntitiesList = DataService.GetEntityTypes();
            EntityType EntitySelected = EntitiesList[BuiltInCodes.EntityType.WBS];
            List<TreeEntityMasterInfo> TreeInfo = DataService.GetFilteredTreeEntities(BuiltInCodes.EntityType.WBS, null, null, out entitiesFound);
            TreeInfo = EntitiesHelper.TreeFilterById(TreeInfo, entitiesFound);
            TreeEntities = DataService.GetTreeEntitiesById(BuiltInCodes.EntityType.WBS, TreeInfo.Select(item => item.Id));
            WBSItemsWithParents = TreeEntities.ToList();
        }
        private List<TreeEntity> RetrieveWBSItem(string sezioneItem, HashSet<Guid> filteredEntitiesIds)
        {
            List<Guid> entitiesFound = null;
            List<TreeEntity> TreeEntities = new List<TreeEntity>();
            List<Entity> Entities = new List<Entity>();
            Dictionary<string, EntityType> EntitiesList = DataService.GetEntityTypes();
            EntityType EntitySelected = EntitiesList[BuiltInCodes.EntityType.WBS];
            List<TreeEntityMasterInfo> TreeInfo = DataService.GetFilteredTreeEntities(BuiltInCodes.EntityType.WBS, null, null, out entitiesFound);
            TreeInfo = EntitiesHelper.TreeFilterById(TreeInfo, entitiesFound);
            TreeEntities = DataService.GetTreeEntitiesById(BuiltInCodes.EntityType.WBS, TreeInfo.Select(item => item.Id));
            return TreeEntities.Where(t => t.IsParent == false).ToList();
        }

        private void CreateDateScaleWithWBSItemsEndDate(List<TreeEntity> wBSItems)
        {
            DateTimeWBSItemEntitys.Clear();
            DateTime Date = new DateTime();
            foreach (WBSItem Entity in wBSItems)
            {
                ValoreData DataFine = (ValoreData)entitiesHelper.GetValoreAttributo(Entity, BuiltInCodes.Attributo.DataFine, false, false);
                Date = (DateTime)DataFine.V;
                DateTimeWBSItemEntity dateTimeWBSItemEntity = new DateTimeWBSItemEntity();

                if (DateTimeWBSItemEntitys.Where(d => d.Data == Date).FirstOrDefault() == null)
                {
                    dateTimeWBSItemEntity.Data = Date;
                    //dateTimeWBSItemEntity.WBSItemsDataInizio.Add(Entity);
                    dateTimeWBSItemEntity.WBSItemsDataFine.Add(Entity);
                    DateTimeWBSItemEntitys.Add(dateTimeWBSItemEntity);
                }
                else
                {
                    dateTimeWBSItemEntity = DateTimeWBSItemEntitys.Where(d => d.Data == Date).FirstOrDefault();
                    //dateTimeWBSItemEntity.WBSItemsDataInizio.Add(Entity);
                    dateTimeWBSItemEntity.WBSItemsDataFine.Add(Entity);
                }

            }

            DateTimeWBSItemEntitys = DateTimeWBSItemEntitys.OrderBy(s => s.Data).ToList();
        }

        public void Init(List<AttributoFoglioDiCalcolo> attibutiUtilizzati)
        {
            entitiesHelper = new EntitiesHelper(DataService);
            GetWBSItems();
            CreateDateScaleWithWBSItemsEndDate(WBSItems);
            PrepareListOfOriginalAttributeDataFine(attibutiUtilizzati);
        }

        public List<SALProgrammatoView> GetSALProgrammatoView(List<AttributoFoglioDiCalcolo> attibutiUtilizzati)
        {
            listaSALProgrammatiView = new List<SALProgrammatoView>();
            List<DateTime> datetimesList = new List<DateTime>();
            DateTime date = new DateTime();
            foreach (WBSItem entity in WBSItems)
            {
                if (!entity.IsParent)
                {
                    date = entity.GetDataInizio().Value;
                    if (!datetimesList.Contains(date))
                        datetimesList.Add(date);
                    date = entity.GetDataFine().Value;
                    if (!datetimesList.Contains(date))
                        datetimesList.Add(date);
                }
            }

            datetimesList = datetimesList.OrderBy(x => x).ToList();

            foreach (DateTime data in datetimesList)
            {
                listaSALProgrammatiView.Add(GetSALProgrammatoViewByDate(attibutiUtilizzati, data));
            }

            return listaSALProgrammatiView;
        }

        private List<DateTime> GetDateAProduttivitaCostante()
        {
            List<DateTime> dates = new List<DateTime>();
            DateTime date = new DateTime();

            foreach (WBSItem entity in WBSItems)
            {
                if (!entity.IsParent)
                {
                    date = entity.GetDataInizio().Value;
                    if (!dates.Contains(date))
                        dates.Add(date);
                    date = entity.GetDataFine().Value;
                    if (!dates.Contains(date))
                        dates.Add(date);
                }
            }

            dates = dates.OrderBy(x => x).ToList();
            return dates;
        }

        public List<SALProgrammatoView> GetSALProgrammatoView(List<AttributoFoglioDiCalcolo> attibutiUtilizzati, List<DateTime> newDates, List<bool> ListIsSAL = null)
        {
            listaSALProgrammatiView = new List<SALProgrammatoView>();
            int counter = 0;
            foreach (DateTime date in newDates)
            {
                listaSALProgrammatiView.Add(GetSALProgrammatoViewByDate(attibutiUtilizzati, date));
                if (ListIsSAL != null)
                {
                    listaSALProgrammatiView.LastOrDefault().IsSAL = ListIsSAL.ElementAt(counter);
                }
                counter++;
            }
            CalulateOtherColumnByUsingProgressiveOne();
            return listaSALProgrammatiView;
        }

        public SALProgrammatoView GetSALProgrammatoView(List<AttributoFoglioDiCalcolo> attibutiUtilizzati, DateTime newDate)
        {
            return GetSALProgrammatoViewByDate(attibutiUtilizzati, newDate);
        }

        private string codice = null;
        private double codicevalue = 0;
        private Dictionary<double, bool> codicevalues = new Dictionary<double, bool>();
        List<ValueWithPercDate> ValueWithPercDate = new List<ValueWithPercDate>();
        double finalValue = 0;
        private int FoundValue = -1;
        private bool ForceValue = false;
        int counterOperation = 0;

        public SALProgrammatoView GetSALProgrammatoView(List<AttributoFoglioDiCalcolo> attibutiUtilizzati, string attributeCode, double newValue, DateTime InitialDate, DateTime FinalDate, bool useCalendar)
        {
            ForceValue = false;
            counterOperation = 0;
            var salPr = GetSALProgrammatoViewByValueRecursive(attibutiUtilizzati, attributeCode, newValue, InitialDate, FinalDate, useCalendar);
            return salPr;
        }

        public SALProgrammatoView GetSALProgrammatoViewPerc(List<AttributoFoglioDiCalcolo> attibutiUtilizzati, string attributeCode, double newValuePerc, DateTime InitialDate, DateTime FinalDate, bool useCalendar)
        {
            ForceValue = false;
            counterOperation = 0;
            double newValue = newValuePerc * GetTotalAttributoValue(attributeCode) / 100;
            if (newValuePerc == 100)
            {
                newValue = GetTotalAttributoValue(attributeCode);
            }
            return GetSALProgrammatoViewByValueRecursive(attibutiUtilizzati, attributeCode, newValue, InitialDate, FinalDate, useCalendar);
        }
        private SALProgrammatoView GetSALProgrammatoViewByValueRecursive(List<AttributoFoglioDiCalcolo> attibutiUtilizzati, string attributeCode, double newValue, DateTime InitialDate, DateTime FinalDate, bool useCalendar)
        {
            counterOperation++;
            codice = attributeCode;
            codicevalue = newValue;
            ForceValue = false;

            DateTime newInitialDate = InitialDate;
            double giorni = (FinalDate - newInitialDate).Days;
            double ore = (FinalDate - newInitialDate).Hours;
            double minuti = (FinalDate - newInitialDate).Minutes;

            if (giorni == 1)
                ore = 24;
            if (ore == 1)
                minuti = 60;

            if (giorni > 1)
            {
                DateTime newInitialDateChecked = ReturnFirstDataAvailable(newInitialDate.AddDays(1), useCalendar);
                SALProgrammatoView sALProgrammatoView = GetSALProgrammatoViewByDate(attibutiUtilizzati, newInitialDateChecked);
                if (FoundValue == 1)
                    return sALProgrammatoView;
                if (FoundValue == 0)
                {
                    return GetSALProgrammatoViewByValueRecursive(attibutiUtilizzati, attributeCode, newValue, newInitialDate, newInitialDate.AddDays(1), useCalendar);
                }
                if (FoundValue == -1)
                    return GetSALProgrammatoViewByValueRecursive(attibutiUtilizzati, attributeCode, newValue, newInitialDateChecked, FinalDate, useCalendar);

            }
            if (giorni <= 1 && ore > 1)
            {
                DateTime newInitialDateChecked = ReturnFirstDataAvailable(newInitialDate.AddHours(1), useCalendar);
                SALProgrammatoView sALProgrammatoView = GetSALProgrammatoViewByDate(attibutiUtilizzati, newInitialDateChecked);
                if (FoundValue == 1)
                    return sALProgrammatoView;
                if (FoundValue == 0)
                {
                    return GetSALProgrammatoViewByValueRecursive(attibutiUtilizzati, attributeCode, newValue, newInitialDate, newInitialDate.AddHours(1), useCalendar);
                }
                if (FoundValue == -1)
                    return GetSALProgrammatoViewByValueRecursive(attibutiUtilizzati, attributeCode, newValue, newInitialDateChecked, FinalDate, useCalendar);

            }

            if (giorni <= 1 && ore <= 1 && minuti > 1)
            {
                DateTime newInitialDateChecked = ReturnFirstDataAvailable(newInitialDate.AddMinutes(1), useCalendar);
                ForceValue = true;
                SALProgrammatoView sALProgrammatoView = GetSALProgrammatoViewByDate(attibutiUtilizzati, newInitialDateChecked);
                if (FoundValue == 1)
                    return sALProgrammatoView;
                if (FoundValue == 0)
                {
                    return GetSALProgrammatoViewByValueRecursive(attibutiUtilizzati, attributeCode, newValue, newInitialDate, newInitialDate.AddMinutes(1), useCalendar);
                }
                if (FoundValue == -1)
                {
                    if (newInitialDateChecked.AddMinutes(1) == FinalDate)
                    {
                        return null;
                    }
                    else
                    {
                        return GetSALProgrammatoViewByValueRecursive(attibutiUtilizzati, attributeCode, newValue, newInitialDateChecked, FinalDate, useCalendar);
                    }
                }
            }

            return new SALProgrammatoView();
        }

        private SALProgrammatoView GetSALProgrammatoViewByDate(List<AttributoFoglioDiCalcolo> attibutiUtilizzati, DateTime date)
        {
            DateTime newDate = date;
            SALProgrammatoView salProgrammatoView = new SALProgrammatoView();
            List<DateTimeWBSItemEntity> DateTimeWBSItemEntitysToIterate = new List<DateTimeWBSItemEntity>(DateTimeWBSItemEntitys);
            IList<TreeEntity> TreeEntityJustCalculated = new List<TreeEntity>();
            Dictionary<string, double> DictionaryTotalProgressive = new Dictionary<string, double>();
            Dictionary<string, double> DictionaryTotalProductivity = new Dictionary<string, double>();
            double PreviousProgressive = 0;
            bool enableObjectCreation = false;
            bool breakAllCycle = false;
            int index = 0;

            int ContatoreColonna = 1;
            foreach (string attributocodice in AttributiRealeContabilita)
            {
                PregressiveDateValue pregressiveDateValue = null;
                if (ProgressiveValuesPerDateEnd.Count() > 0)
                {
                    if (ProgressiveValuesPerDateEnd.ContainsKey(attributocodice))
                    {
                        pregressiveDateValue = ProgressiveValuesPerDateEnd[attributocodice].Where(f => f.Date < newDate).LastOrDefault();
                        index = ProgressiveValuesPerDateEnd[attributocodice].FindIndex(a => a == pregressiveDateValue);
                    }
                }

                if (attibutiUtilizzati.Where(a => a.CodiceOrigine == attributocodice && a.Amount).FirstOrDefault() != null)
                {
                    ContatoreColonna++;
                }
                if (attibutiUtilizzati.Where(a => a.CodiceOrigine == attributocodice && a.ProgressiveAmount).FirstOrDefault() != null)
                {
                    ContatoreColonna++;
                }
                if (attibutiUtilizzati.Where(a => a.CodiceOrigine == attributocodice && a.ProductivityPerHour).FirstOrDefault() != null)
                {
                    ContatoreColonna++;
                }
                if (attibutiUtilizzati.Where(a => a.CodiceOrigine == attributocodice && a.ProgressiveAmount).FirstOrDefault() != null)//(% progressivo)
                {
                    ContatoreColonna++;
                }
                if (pregressiveDateValue != null)
                {
                    DictionaryTotalProgressive.Add(attributocodice, pregressiveDateValue.Value);
                }
                else
                {
                    DictionaryTotalProgressive.Add(attributocodice, 0);
                }
                DictionaryTotalProductivity.Add(attributocodice, 0);
            }

            if (DateTimeWBSItemEntitysToIterate != null)
            {
                if (DateTimeWBSItemEntitysToIterate.Count() > 1)
                {
                    DateTimeWBSItemEntitysToIterate?.RemoveRange(0, index + 1);
                }
            }


            dynamic UnknowObjectLocal = new System.Dynamic.ExpandoObject();
            AddProperty(UnknowObjectLocal, "Data", newDate);
            ContatoreColonna = 1;

            foreach (string attributocodice in AttributiRealeContabilita)
            {
                double totalAmountPeriodo = 0;
                double TotalAmout = 0;
                double EuroOra = 0;
                breakAllCycle = false;

                foreach (DateTimeWBSItemEntity DateTimeWBSItemEntity in DateTimeWBSItemEntitysToIterate)
                {
                    if (enableObjectCreation)
                    {
                        enableObjectCreation = false;
                    }

                    TotalAmout = 0;
                    EuroOra = 0;

                    foreach (TreeEntity WBSItem in DateTimeWBSItemEntity.WBSItemsDataFine)
                    {
                        DateTime DataInizioPrecedente = (DateTime)((WBSItem)WBSItem).GetDataInizio();
                        DateTime DataFinePrecedente = (DateTime)((WBSItem)WBSItem).GetDataFine();
                        CalendariItem calendario = GetCalendarioItem((WBSItem)WBSItem);

                        if (DataInizioPrecedente > newDate)
                            continue;

                        if (newDate >= DataFinePrecedente)
                        {
                            ValoreReale valoreReale = null;
                            ValoreContabilita valoreContabilita = null;

                            Valore valore = entitiesHelper.GetValoreAttributo(WBSItem, attributocodice, false, false);
                            if (valore != null)
                            {
                                if (entitiesHelper.GetValoreAttributo(WBSItem, attributocodice, false, false) is ValoreReale)
                                {
                                    valoreReale = (ValoreReale)entitiesHelper.GetValoreAttributo(WBSItem, attributocodice, false, false);
                                }
                                else
                                {
                                    valoreContabilita = (ValoreContabilita)entitiesHelper.GetValoreAttributo(WBSItem, attributocodice, false, false);
                                }
                                if (DataInizioPrecedente != DataFinePrecedente)
                                {
                                    if (valoreReale != null)
                                    {
                                        TotalAmout = TotalAmout + (double)valoreReale.RealResult;
                                    }
                                    else
                                    {
                                        if (valoreContabilita.RealResult != null)
                                            TotalAmout = TotalAmout + (double)valoreContabilita.RealResult;
                                        else
                                            TotalAmout = TotalAmout + 0;
                                    }
                                }
                            }
                            if (newDate == DataFinePrecedente)
                            {
                                DictionaryTotalProductivity[attributocodice] = DictionaryTotalProductivity[attributocodice] + GetProductivityByMinute(WBSItem, attributocodice) * 60;
                            }
                        }
                        if (newDate > DataInizioPrecedente && newDate < DataFinePrecedente)
                        {
                            double EuroPerMinute = 0;
                            EuroPerMinute = GetProductivityByMinute(WBSItem, attributocodice);
                            DictionaryTotalProductivity[attributocodice] = DictionaryTotalProductivity[attributocodice] + EuroPerMinute * 60;
                            DateTimeCalculator timeCalc = null;
                            if (calendario != null)

                                timeCalc = new DateTimeCalculator(calendario.GetWeekHours(), calendario.GetCustomDays());
                            else
                                timeCalc = new DateTimeCalculator(WeekHours.Default, new CustomDays());
                            DateTime DataInzioIntervallo = DataInizioPrecedente;
                            double WorkingMinute = timeCalc.GetWorkingMinutesBetween(DataInzioIntervallo, newDate);
                            double amount = WorkingMinute * EuroPerMinute;
                            TotalAmout = TotalAmout + amount;
                            EuroOra = EuroOra + EuroPerMinute * 60;
                            TreeEntityJustCalculated.Add(WBSItem);
                            enableObjectCreation = true;
                        }
                    }

                    totalAmountPeriodo += TotalAmout;

                    PreviousProgressive = DictionaryTotalProgressive[attributocodice];
                    DictionaryTotalProgressive[attributocodice] = DictionaryTotalProgressive[attributocodice] + TotalAmout;

                    //FOR RECURSIVE VALUE FUNCTION
                    if (attributocodice == codice && DictionaryTotalProgressive[attributocodice] == codicevalue)
                        FoundValue = 1;
                    if (attributocodice == codice && DictionaryTotalProgressive[attributocodice] > codicevalue)
                        FoundValue = 0;
                    if (attributocodice == codice && DictionaryTotalProgressive[attributocodice] > codicevalue && ForceValue)
                        FoundValue = 1;
                    if (attributocodice == codice && codicevalue > DictionaryTotalProgressive[attributocodice])
                        FoundValue = -1;
                }
                if (attibutiUtilizzati.Where(a => a.CodiceOrigine == attributocodice && a.Amount).FirstOrDefault() != null)
                {
                    AddProperty(UnknowObjectLocal, GanttKeys.ColonnaAttributo + ContatoreColonna, totalAmountPeriodo/*TotalAmout*/);
                    ContatoreColonna++;
                }

                if (attibutiUtilizzati.Where(a => a.CodiceOrigine == attributocodice && a.ProgressiveAmount).FirstOrDefault() != null)
                {
                    AddProperty(UnknowObjectLocal, GanttKeys.ColonnaAttributo + ContatoreColonna, DictionaryTotalProgressive[attributocodice]);
                    ContatoreColonna++;
                }

                if (attibutiUtilizzati.Where(a => a.CodiceOrigine == attributocodice && a.ProductivityPerHour).FirstOrDefault() != null)
                {
                    AddProperty(UnknowObjectLocal, GanttKeys.ColonnaAttributo + ContatoreColonna, DictionaryTotalProductivity[attributocodice]);
                    ContatoreColonna++;
                }

                if (attibutiUtilizzati.Where(a => a.CodiceOrigine == attributocodice && a.ProgressiveAmount).FirstOrDefault() != null) //% progressivo
                {
                    AddProperty(UnknowObjectLocal, GanttKeys.ColonnaAttributo + ContatoreColonna, 0);
                    ContatoreColonna++;
                }
            }

            salProgrammatoView = GetGeneric<SALProgrammatoView>(UnknowObjectLocal);

            return salProgrammatoView;
        }


        private static DateTime GetFinePeriodo(TipologiaPeriodo tipologiaPeriodo, DateTime date)
        {
            DateTime dateFine = date;

            switch (tipologiaPeriodo)
            {
                case TipologiaPeriodo.Giorno:
                    dateFine = date.AddDays(1);
                    break;
                case TipologiaPeriodo.Settimana:
                    dateFine = date.AddDays(7);
                    break;
                case TipologiaPeriodo.Mese:
                    dateFine = date.AddMonths(1);
                    break;
                case TipologiaPeriodo.Anno:
                    dateFine = date.AddYears(1);
                    break;
                case TipologiaPeriodo.ProduzioneCostante:
                    break;
                case TipologiaPeriodo.ProgrammazioneSAL:
                    break;

            }
            return dateFine;
        }

        private List<SALProgrammatoView> GetSALProgrammatoViewByDateByActivity(List<AttributoFoglioDiCalcolo> attibutiUtilizzati, List<DateTime> dates, TipologiaPeriodo tipologiaPeriodo)
        {
            //oss: dates sono inizio periodo per giorno, settimana, mese, anno 
            //dates sono di inizio periodo per Produttività costante e SAL programmati

            List<SALProgrammatoView> listSALProgrammatoView = new List<SALProgrammatoView>();
            List<Guid> StrutturaAlberoPadri = new List<Guid>();
            int ContatoreColonna = 1;
            int ContatoreDate = 0;
            int DepthPrecedente = -1;
            AttributoFormatHelper attributoFormatHelper = new AttributoFormatHelper(DataService);
            GetWBSItemsWithParents();

            foreach (TreeEntity WBSItem in WBSItemsWithParents)
            {
                DateTime DataInizioAttivita = (DateTime)((WBSItem)WBSItem).GetDataInizio();
                DateTime DataFineAttivita = (DateTime)((WBSItem)WBSItem).GetDataFine();
                ContatoreDate = 0;

                //if (DepthPrecedente < WBSItem.Depth)
                //    if (WBSItem.IsParent)
                //        StrutturaAlberoPadri.Add(WBSItem.EntityId);

                //if (DepthPrecedente > WBSItem.Depth)
                //    if (WBSItem.IsParent)
                //        StrutturaAlberoPadri.RemoveRange(WBSItem.Depth, StrutturaAlberoPadri.Count - WBSItem.Depth);

                //if (WBSItem.Depth == 0 && !WBSItem.IsParent)
                //    StrutturaAlberoPadri.Clear();

                //if (WBSItem.Depth == 0 && WBSItem.IsParent && DepthPrecedente != -1)
                //    StrutturaAlberoPadri.Add(WBSItem.EntityId);

                StrutturaAlberoPadri.Clear();
                TreeEntity wbsItemParent = WBSItem.Parent;
                while (wbsItemParent != null)
                {
                    StrutturaAlberoPadri.Add(wbsItemParent.EntityId);
                    wbsItemParent = wbsItemParent.Parent;
                }


                DepthPrecedente = WBSItem.Depth;

                foreach (DateTime date in dates)
                {

                    if (WBSItem.IsParent)
                    {
                        //if (date >= DataInizioAttivita.Date || DataFineAttivita.Date <= date)
                        if (DataFineAttivita.Date >= date && DataInizioAttivita.Date <= GetFinePeriodo(tipologiaPeriodo, date))
                        {
                            if (listSALProgrammatoView.Where(r => r.Guid == WBSItem.EntityId && r.Data == date).FirstOrDefault() == null)
                            {
                                SALProgrammatoView salProgrammatoView = new SALProgrammatoView();
                                salProgrammatoView.Data = date;
                                string Codice = ((ValoreTesto)entitiesHelper.GetValoreAttributo(WBSItem, BuiltInCodes.Attributo.Codice, false, false)).PlainText;
                                string Descrizione = ((ValoreTesto)entitiesHelper.GetValoreAttributo(WBSItem, BuiltInCodes.Attributo.Nome, false, false)).PlainText;
                                salProgrammatoView.Guid = WBSItem.EntityId;
                                salProgrammatoView.Codice = Codice;
                                salProgrammatoView.Descrizione = Descrizione;
                                salProgrammatoView.Livello = WBSItem.Depth;
                                salProgrammatoView.IsParent = WBSItem.IsParent;
                                listSALProgrammatoView.Add(salProgrammatoView);
                            }
                        }
                        continue;
                    }

                    ContatoreColonna = 1;

                    DateTime dataInizioPerCalcolo = DataInizioAttivita;
                    DateTime dataFinePerCalcolo = DataFineAttivita;
                    //if (ContatoreDate != 0)
                    //{
                    //    if (dates.ElementAt(ContatoreDate - 1) > DataInizioAttivita)
                    //    {
                    //        dataInizioPerCalcolo = dates.ElementAt(ContatoreDate - 1);
                    //    }
                    //}
                    //if (ContatoreDate + 1 != dates.Count())
                    //{
                    //    if (DataFineAttivita > date.AddDays(1))
                    //    {
                    //        dataFinePerCalcolo = date.AddDays(1);
                    //    }
                    //}

                    DateTime dateInizioPeriodo = date;
                    DateTime dateFinePeriodo = date;
                    if (tipologiaPeriodo == TipologiaPeriodo.ProduzioneCostante || tipologiaPeriodo == TipologiaPeriodo.ProgrammazioneSAL)
                    {
                        if (ContatoreDate == 0)
                            dateInizioPeriodo = DataInizioAttivita;
                        else
                            dateInizioPeriodo = dates[ContatoreDate - 1];

                        dateFinePeriodo = date;
                    }
                    else
                    {
                        dateInizioPeriodo = date;
                        dateFinePeriodo = GetFinePeriodo(tipologiaPeriodo, date);
                    }

                    if (dateInizioPeriodo > DataInizioAttivita)
                    {
                        dataInizioPerCalcolo = dateInizioPeriodo;
                    }

                    if (dateFinePeriodo < DataFineAttivita)
                    {
                        dataFinePerCalcolo = dateFinePeriodo;
                    }

                    if (dataInizioPerCalcolo < dataFinePerCalcolo)
                    {
                        foreach (string attributocodice in AttributiRealeContabilita)
                        {
                            double TotalAmout = 0;
                            double EuroOra = 0;
                            double WorkingMinute = 0;
                            double amount = 0;
                            string format = "#,#00.00";

                            CalendariItem calendario = GetCalendarioItem((WBSItem)WBSItem);

                            double EuroPerMinute = 0;
                            EuroPerMinute = GetProductivityByMinute(WBSItem, attributocodice);
                            DateTimeCalculator timeCalc = null;
                            if (calendario != null)
                                timeCalc = new DateTimeCalculator(calendario.GetWeekHours(), calendario.GetCustomDays());
                            else
                                timeCalc = new DateTimeCalculator(WeekHours.Default, new CustomDays());
                            WorkingMinute = timeCalc.GetWorkingMinutesBetween(dataInizioPerCalcolo, dataFinePerCalcolo);
                            amount = WorkingMinute * EuroPerMinute;
                            TotalAmout = TotalAmout + amount;
                            EuroOra = EuroOra + EuroPerMinute * 60;
                            //if (ContatoreDate > 0)
                            //{
                            //    if ((DataFineAttivita < dates.ElementAt(ContatoreDate - 1) && DataFineAttivita < date) || (DataInizioAttivita > dates.ElementAt(ContatoreDate - 1) && DataInizioAttivita > date))
                            //    {
                            //        TotalAmout = 0;
                            //        EuroOra = 0;
                            //    }

                            //}
                            //if (WorkingMinute < 0)
                            //{
                            //    TotalAmout = 0;
                            //    EuroOra = 0;
                            //}

                            if (listSALProgrammatoView.Where(r => r.Guid == WBSItem.EntityId && r.Data == date).FirstOrDefault() == null)
                            {
                                SALProgrammatoView salProgrammatoView = new SALProgrammatoView();
                                salProgrammatoView.Data = date;
                                string Codice = ((ValoreTesto)entitiesHelper.GetValoreAttributo(WBSItem, BuiltInCodes.Attributo.Codice, false, false)).PlainText;
                                string Descrizione = ((ValoreTesto)entitiesHelper.GetValoreAttributo(WBSItem, BuiltInCodes.Attributo.Nome, false, false)).PlainText;
                                salProgrammatoView.Guid = WBSItem.EntityId;
                                salProgrammatoView.Codice = Codice;
                                salProgrammatoView.Descrizione = Descrizione;
                                Attributo att = entitiesHelper.GetAttributo(WBSItem, attributocodice);
                                if (att != null)
                                {
                                    if (att is AttributoRiferimento)
                                    {
                                        AttributoRiferimento attributoRiferimento = (AttributoRiferimento)att;
                                        Attributo attorigine = entitiesHelper.GetSourceAttributo(attributoRiferimento);
                                        format = attributoFormatHelper.GetValorePaddedFormat(attorigine);
                                        salProgrammatoView.Formato = format;
                                    }
                                }
                                //EntityAttributo prova = WBSItem.Attributi[attributocodice];
                                //if (prova != null)
                                //{
                                //    if (prova.Attributo is AttributoRiferimento)
                                //    {
                                //        AttributoRiferimento attributoRiferimento = (AttributoRiferimento)prova.Attributo;
                                //        Attributo attorigine = entitiesHelper.GetSourceAttributo(attributoRiferimento);
                                //        format = attributoFormatHelper.GetValorePaddedFormat(attorigine);
                                //        salProgrammatoView.Formato = format;
                                //    }
                                //}

                                salProgrammatoView.Livello = WBSItem.Depth;
                                salProgrammatoView.IsParent = WBSItem.IsParent;
                                if (TotalAmout == 0)
                                {
                                    salProgrammatoView.SetValue(GanttKeys.ColonnaAttributo + ContatoreColonna, null);
                                }
                                else
                                {
                                    salProgrammatoView.SetValue(GanttKeys.ColonnaAttributo + ContatoreColonna, TotalAmout);
                                }
                                listSALProgrammatoView.Add(salProgrammatoView);
                            }
                            else
                            {
                                SALProgrammatoView salProgrammatoView = listSALProgrammatoView.Where(r => r.Guid == WBSItem.EntityId && r.Data == date).FirstOrDefault();
                                if (TotalAmout == 0)
                                {
                                    salProgrammatoView.SetValue(GanttKeys.ColonnaAttributo + ContatoreColonna, null);
                                }
                                else
                                {
                                    salProgrammatoView.SetValue(GanttKeys.ColonnaAttributo + ContatoreColonna, TotalAmout);
                                }
                            }

                            //SALProgrammatoView salProgrammatoView = new SALProgrammatoView();
                            //salProgrammatoView.Data = date;
                            //string Codice = ((ValoreTesto)entitiesHelper.GetValoreAttributo(WBSItem, BuiltInCodes.Attributo.Codice, false, false)).PlainText;
                            //string Descrizione = ((ValoreTesto)entitiesHelper.GetValoreAttributo(WBSItem, BuiltInCodes.Attributo.Nome, false, false)).PlainText;
                            //salProgrammatoView.Guid = WBSItem.EntityId;
                            //salProgrammatoView.Codice = Codice;
                            //salProgrammatoView.Descrizione = Descrizione;
                            //salProgrammatoView.Livello = WBSItem.Depth;
                            //salProgrammatoView.IsParent = WBSItem.IsParent;
                            //salProgrammatoView.SetValue(GanttKeys.ColonnaAttributo + ContatoreColonna, TotalAmout);
                            //listSALProgrammatoView.Add(salProgrammatoView);

                            foreach (Guid guid in StrutturaAlberoPadri)
                            {
                                SALProgrammatoView salProgrammatoViewPadre = listSALProgrammatoView.Where(s => s.Guid == guid && s.Data == date).FirstOrDefault();
                                if (salProgrammatoViewPadre != null)
                                {
                                    salProgrammatoViewPadre.SetValue(GanttKeys.ColonnaAttributo + ContatoreColonna, salProgrammatoViewPadre.GetValue(GanttKeys.ColonnaAttributo + ContatoreColonna) + TotalAmout);
                                    salProgrammatoViewPadre.Formato = format;
                                }

                            }

                            ContatoreColonna++;
                        }
                    }
                    ContatoreDate++;
                }
            }

            //listSALProgrammatoView = listSALProgrammatoView.OrderBy(x => x.Data).ToList();

            return listSALProgrammatoView;
        }


        private double GetProductivityByMinute(TreeEntity treeEntity, string attributocodice)
        {
            double EuroPerMinute = 0;
            ValoreReale valoreReale = null;
            ValoreContabilita valoreContabilita = null;

            if (entitiesHelper.GetValoreAttributo(treeEntity, attributocodice, false, false) is ValoreReale)
            {
                valoreReale = (ValoreReale)entitiesHelper.GetValoreAttributo(treeEntity, attributocodice, false, false);
            }
            else
            {
                valoreContabilita = (ValoreContabilita)entitiesHelper.GetValoreAttributo(treeEntity, attributocodice, false, false);
            }
            ValoreReale Lavoro = (ValoreReale)entitiesHelper.GetValoreAttributo(treeEntity, BuiltInCodes.Attributo.Lavoro, false, false);

            if (valoreReale != null)
            {
                if (Double.IsFinite(valoreReale.RealResult.Value) && (double)Lavoro.RealResult != 0)
                    EuroPerMinute = (double)valoreReale?.RealResult / ((double)Lavoro.RealResult * 60);
                else
                    EuroPerMinute = 0;
            }
            if (valoreContabilita != null)
            {
                if (valoreContabilita?.RealResult.HasValue == true && (double)Lavoro.RealResult != 0)
                    EuroPerMinute = (double)valoreContabilita?.RealResult / ((double)Lavoro.RealResult * 60);
                else
                    EuroPerMinute = 0;

            }

            return EuroPerMinute;
        }

        Dictionary<string, List<PregressiveDateValue>> ProgressiveValuesPerDateEnd = new Dictionary<string, List<PregressiveDateValue>>();
        public void PrepareProgressiveValuesPerDateEnd()
        {
            MaxDate = new DateTime();
            MinDate = new DateTime(2500, 12, 31);
            ProgressiveValuesPerDateEnd.Clear();
            CalendariItems.Clear();
            double TotalValue = 0;
            ValueWithPercDate.Clear();

            if (AttributiRealeContabilita.Count() > 0)
            {
                foreach (string attributocodice in AttributiRealeContabilita)
                {
                    TotalValue = 0;
                    ProgressiveValuesPerDateEnd.Add(attributocodice, new List<PregressiveDateValue>());
                    foreach (DateTimeWBSItemEntity DateTimeWBSItemEntity in DateTimeWBSItemEntitys)
                    {
                        foreach (TreeEntity WBSItem in DateTimeWBSItemEntity.WBSItemsDataFine)
                        {
                            DateTime DataInizioPrecedente = (DateTime)((WBSItem)WBSItem).GetDataInizio();
                            if (DataInizioPrecedente < MinDate)
                                MinDate = DataInizioPrecedente;
                            DateTime DataFinePrecedente = (DateTime)((WBSItem)WBSItem).GetDataFine();
                            if (DataFinePrecedente > MaxDate)
                                MaxDate = DataFinePrecedente;
                            CalendariItem calendario = GetCalendarioItem((WBSItem)WBSItem);

                            if (calendario != null)
                            {
                                if (CalendariItems.Count() == 0)
                                {
                                    CalendariItems.Add(new AttivitaPerCalendario());
                                    CalendariItems.FirstOrDefault().CalendarioItemGuid = calendario.EntityId;
                                    CalendariItems.FirstOrDefault().CalendarioItem = calendario;
                                    CalendariItems.FirstOrDefault().WBSItemsProperty = new List<WBSItemProperty>();
                                    CalendariItems.FirstOrDefault().WBSItemsProperty.Add(new WBSItemProperty() { DateInizio = DataInizioPrecedente, DateFine = DataFinePrecedente });
                                }
                                else
                                {
                                    if (CalendariItems.Where(g => g.CalendarioItem.EntityId == calendario.EntityId).FirstOrDefault() == null)
                                    {
                                        CalendariItems.Add(new AttivitaPerCalendario());
                                        CalendariItems.LastOrDefault().CalendarioItemGuid = calendario.EntityId;
                                        CalendariItems.LastOrDefault().CalendarioItem = calendario;
                                        CalendariItems.LastOrDefault().WBSItemsProperty = new List<WBSItemProperty>();
                                        CalendariItems.LastOrDefault().WBSItemsProperty.Add(new WBSItemProperty() { DateInizio = DataInizioPrecedente, DateFine = DataFinePrecedente });
                                    }
                                    else
                                    {
                                        CalendariItems.Where(g => g.CalendarioItem.EntityId == calendario.EntityId).FirstOrDefault().WBSItemsProperty.Add(new WBSItemProperty() { DateInizio = DataInizioPrecedente, DateFine = DataFinePrecedente });
                                    }
                                }
                            }

                            ValoreReale valoreReale = null;
                            ValoreContabilita valoreContabilita = null;
                            if (entitiesHelper.GetValoreAttributo(WBSItem, attributocodice, false, false) is ValoreReale)
                            {
                                valoreReale = (ValoreReale)entitiesHelper.GetValoreAttributo(WBSItem, attributocodice, false, false);
                            }
                            else
                            {
                                valoreContabilita = (ValoreContabilita)entitiesHelper.GetValoreAttributo(WBSItem, attributocodice, false, false);
                            }
                            if (DataInizioPrecedente != DataFinePrecedente)
                            {
                                if (valoreReale != null)
                                {
                                    if (valoreReale != null)
                                    {
                                        TotalValue = TotalValue + (double)valoreReale.RealResult;
                                    }

                                }
                                else
                                {
                                    if (valoreContabilita != null)
                                    {
                                        if (valoreContabilita.RealResult != null)
                                            TotalValue = TotalValue + (double)valoreContabilita.RealResult;
                                        else
                                            TotalValue = TotalValue + 0;
                                    }
                                }
                            }
                        }

                        ProgressiveValuesPerDateEnd[attributocodice].Add(new PregressiveDateValue() { Date = DateTimeWBSItemEntity.Data, Value = TotalValue });
                    }
                }
            }
            else
            {
                foreach (DateTimeWBSItemEntity DateTimeWBSItemEntity in DateTimeWBSItemEntitys)
                {
                    foreach (TreeEntity WBSItem in DateTimeWBSItemEntity.WBSItemsDataFine)
                    {
                        DateTime DataInizioPrecedente = (DateTime)((WBSItem)WBSItem).GetDataInizio();
                        if (DataInizioPrecedente < MinDate)
                            MinDate = DataInizioPrecedente;
                        DateTime DataFinePrecedente = (DateTime)((WBSItem)WBSItem).GetDataFine();
                        if (DataFinePrecedente > MaxDate)
                            MaxDate = DataFinePrecedente;
                        CalendariItem calendario = GetCalendarioItem((WBSItem)WBSItem);

                        if (calendario == null)
                            continue;

                        if (CalendariItems.Count() == 0)
                        {
                            CalendariItems.Add(new AttivitaPerCalendario());
                            CalendariItems.FirstOrDefault().CalendarioItemGuid = calendario.EntityId;
                            CalendariItems.FirstOrDefault().CalendarioItem = calendario;
                            CalendariItems.FirstOrDefault().WBSItemsProperty = new List<WBSItemProperty>();
                            CalendariItems.FirstOrDefault().WBSItemsProperty.Add(new WBSItemProperty() { DateInizio = DataInizioPrecedente, DateFine = DataFinePrecedente });
                        }
                        else
                        {
                            if (CalendariItems.Where(g => g.CalendarioItem.EntityId == calendario.EntityId).FirstOrDefault() == null)
                            {
                                CalendariItems.Add(new AttivitaPerCalendario());
                                CalendariItems.LastOrDefault().CalendarioItemGuid = calendario.EntityId;
                                CalendariItems.LastOrDefault().CalendarioItem = calendario;
                                CalendariItems.LastOrDefault().WBSItemsProperty = new List<WBSItemProperty>();
                                CalendariItems.LastOrDefault().WBSItemsProperty.Add(new WBSItemProperty() { DateInizio = DataInizioPrecedente, DateFine = DataFinePrecedente });
                            }
                            else
                            {
                                CalendariItems.Where(g => g.CalendarioItem.EntityId == calendario.EntityId).FirstOrDefault().WBSItemsProperty.Add(new WBSItemProperty() { DateInizio = DataInizioPrecedente, DateFine = DataFinePrecedente });
                            }
                        }
                    }
                }
            }

        }

        public double GetTotalAttributoValue(string attributeCode)
        {
            double TotalValue = 0;
            foreach (TreeEntity WBSItem in WBSItems)
            {
                DateTime DataInizioPrecedente = (DateTime)((WBSItem)WBSItem).GetDataInizio();
                DateTime DataFinePrecedente = (DateTime)((WBSItem)WBSItem).GetDataFine();

                ValoreReale valoreReale = null;
                ValoreContabilita valoreContabilita = null;
                if (entitiesHelper.GetValoreAttributo(WBSItem, attributeCode, false, false) is ValoreReale)
                {
                    valoreReale = (ValoreReale)entitiesHelper.GetValoreAttributo(WBSItem, attributeCode, false, false);
                }
                else
                {
                    valoreContabilita = (ValoreContabilita)entitiesHelper.GetValoreAttributo(WBSItem, attributeCode, false, false);
                }
                if (DataInizioPrecedente != DataFinePrecedente)
                {
                    if (valoreReale != null)
                    {
                        TotalValue = TotalValue + (double)valoreReale.RealResult;
                    }
                    else
                    {
                        if (valoreContabilita != null)
                        {
                            if (valoreContabilita.RealResult != null)
                                TotalValue = TotalValue + (double)valoreContabilita.RealResult;
                            else
                                TotalValue = TotalValue + 0;
                        }
                    }
                }
            }
            return TotalValue;
        }

        private void PrepareListOfOriginalAttributeDataFine(List<AttributoFoglioDiCalcolo> attibutiUtilizzati)
        {
            AttributiRealeContabilita.Clear();
            if (DateTimeWBSItemEntitys.FirstOrDefault()?.WBSItemsDataFine.FirstOrDefault() != null)
            {
                foreach (AttributoFoglioDiCalcolo attributo in attibutiUtilizzati)
                {
                    //var atts = DateTimeWBSItemEntitys.FirstOrDefault()?.WBSItemsDataFine.FirstOrDefault().Attributi;
                    var atts = entitiesHelper.GetAttributi(DateTimeWBSItemEntitys.FirstOrDefault()?.WBSItemsDataFine.FirstOrDefault());

                    if (!atts.ContainsKey(attributo.CodiceOrigine))
                        continue; 

                    Attributo att = atts[attributo.CodiceOrigine];
                    if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Reale || att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Contabilita)
                    {
                        AttributiRealeContabilita.Add(att.Codice);
                    }

                    if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Riferimento)
                    {
                        Entity entRef = null;
                        Attributo attref = null;
                        Dictionary<string, EntityType> entTypes = DataService.GetEntityTypes();
                        AttributoRiferimento attributoRiferimento = (AttributoRiferimento)att;
                        attref = entitiesHelper.GetSourceAttributo(att);
                        if (attref.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Reale || attref.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Contabilita)
                        {
                            AttributiRealeContabilita.Add(att.Codice);
                        }
                    }


                    //EntityAttributo item = atts[attributo.CodiceOrigine];
                    //if (item.Attributo.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Reale || item.Attributo.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Contabilita)
                    //{
                    //    AttributiRealeContabilita.Add(item.AttributoCodice);
                    //}

                    //if (item.Attributo.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Riferimento)
                    //{
                    //    Entity entRef = null;
                    //    Attributo attref = null;
                    //    Dictionary<string, EntityType> entTypes = DataService.GetEntityTypes();
                    //    AttributoRiferimento attributoRiferimento = (AttributoRiferimento)item.Attributo;
                    //    attref = entitiesHelper.GetSourceAttributo(item.Attributo);
                    //    if (attref.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Reale || attref.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Contabilita)
                    //    {
                    //        AttributiRealeContabilita.Add(item.AttributoCodice);
                    //    }
                    //}
                }
            }
        }

        private CalendariItem GetCalendarioItem(WBSItem ent)
        {
            string wbsCalendarioIdCodice = string.Join(WBSItemType.AttributoCodiceSeparator, BuiltInCodes.EntityType.Calendari, BuiltInCodes.Attributo.Id);

            Guid wbsCalendarioId = ent.GetAttributoGuidId(wbsCalendarioIdCodice);
            if (wbsCalendarioId != Guid.Empty)
            {
                CalendariItem calendario = DataService.GetEntitiesById(BuiltInCodes.EntityType.Calendari, new List<Guid> { wbsCalendarioId }).FirstOrDefault() as CalendariItem;
                return calendario;
            }
            return null;
        }

        public DateTime ReturnFirstDataAvailable(DateTime date, bool useCalendar)
        {
            List<DateTime> listDate = new List<DateTime>();
            WeekDay weekHours = null;
            List<int> weekDays = new List<int>();
            List<AttivitaPerCalendario> calendari = GetCalendari();
            List<DateTime> availableDates = new List<DateTime>();

            foreach (AttivitaPerCalendario calendario in calendari)
            {
                DateTime NewDate = date;
                DateTimeCalculator timeCalc = new DateTimeCalculator(calendario.CalendarioItem.GetWeekHours(), calendario.CalendarioItem.GetCustomDays());
                bool isWorking = timeCalc.IsWorkingMoment(NewDate);
                if (!isWorking)
                    NewDate = timeCalc.GetNextWorkingMoment(NewDate);

                if (useCalendar)
                {
                    double workingMinute = timeCalc.GetWorkingMinutesPerDay(NewDate);
                    if (workingMinute == 0)
                        NewDate = timeCalc.GetNextWorkingDay(NewDate);
                }
                listDate.Add(NewDate);
            }

            listDate = listDate.OrderBy(x => x).ToList();
            return listDate.FirstOrDefault();
        }

        public DateTime ReturnAsEndingDateTime(DateTime date, bool useCalendar)
        {
            List<DateTime> listDate = new List<DateTime>();
            List<AttivitaPerCalendario> calendari = GetCalendari();
            List<DateTime> availableDates = new List<DateTime>();

            foreach (AttivitaPerCalendario calendario in calendari)
            {
                DateTime NewDate = date;
                DateTimeCalculator timeCalc = new DateTimeCalculator(calendario.CalendarioItem.GetWeekHours(), calendario.CalendarioItem.GetCustomDays());
                listDate.Add(timeCalc.AsEndingDateTime(NewDate));
            }

            listDate = listDate.OrderBy(x => x).ToList();
            return listDate.FirstOrDefault();
        }
        public DateTime ReturnLastDataAvailable(DateTime date, bool useCalendar)
        {
            if (!useCalendar)
                return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0).AddDays(1);

            List<DateTime> listDate = new List<DateTime>();
            WeekDay weekHours = null;
            List<int> weekDays = new List<int>();
            List<AttivitaPerCalendario> calendari = GetCalendari();
            List<DateTime> availableDates = new List<DateTime>();

            foreach (AttivitaPerCalendario calendario in calendari)
            {
                if (calendario.WBSItemsProperty.Where(a => a.DateInizio <= date && a.DateFine >= date).Count() > 0)
                {
                    DateTime NewDate = date;
                    DateTimeCalculator timeCalc = new DateTimeCalculator(calendario.CalendarioItem.GetWeekHours(), calendario.CalendarioItem.GetCustomDays());
                    //VEIRIFY IF I CAN WORK AT THAT TIME
                    bool isWorking = timeCalc.IsWorkingMoment(NewDate);
                    //CAMBIO GESTIONE, IN CASO DI  MOMENTO NON LAVORATIVO SI VA AL PRIMO OMNETO PRECEDENTE UTILE
                    if (!isWorking)
                        NewDate = timeCalc.GetPrevWorkingMoment(NewDate);
                    //NewDate = timeCalc.GetNextWorkingMoment(NewDate);

                    if (useCalendar)
                    {
                        double workingMinute = timeCalc.GetWorkingMinutesPerDay(NewDate);
                        //CAMBIO GESTIONE, IN CASO DI  MOMENTO NON LAVORATIVO SI VA AL PRIMO OMNETO PRECEDENTE UTILE
                        if (workingMinute == 0)
                            NewDate = timeCalc.GetPreviousWorkingDay(NewDate);
                        //NewDate = timeCalc.GetNextWorkingDay(NewDate);
                    }
                    NewDate = timeCalc.GetEndingDateTimeOfDay(NewDate);
                    listDate.Add(NewDate);
                }
            }

            listDate = listDate.OrderBy(x => x).ToList();
            if (listDate.Where(d => d.Date.Day == date.Day).Count() > 0)
            {
                return listDate.Where(d => d.Date.Day == date.Day).LastOrDefault();
            }
            return listDate.LastOrDefault();
        }

        private void CalulateOtherColumnByUsingProgressiveOne()
        {
            List<int> ProgressiveValuePosition = new List<int>();
            ProgressiveValuePosition.Add(2);
            ProgressiveValuePosition.Add(6);
            ProgressiveValuePosition.Add(10);
            ProgressiveValuePosition.Add(14);
            ProgressiveValuePosition.Add(18);
            ProgressiveValuePosition.Add(22);

            double durataPeriodo = 0;
            double durataProgressivo = 0;

            int ContatoreLista = 0;
            foreach (SALProgrammatoView salProgrammatoView in listaSALProgrammatiView)
            {
                if (ContatoreLista == 0)
                {
                    ContatoreLista++;
                    continue;
                }
                for (int i = 1; i < AttributiRealeContabilita.Count() * 4; i++)
                {
                    if (ProgressiveValuePosition.Contains(i))
                    {
                        //CALCULATE AMOUNT DIFERENCE BETWEEN THIS AND PREVIOUS ROW
                        double? TotalValue = listaSALProgrammatiView.LastOrDefault().GetValue(GanttKeys.ColonnaAttributo + i);
                        double? difference = salProgrammatoView.GetValue(GanttKeys.ColonnaAttributo + i) - listaSALProgrammatiView.ElementAt(ContatoreLista - 1).GetValue(GanttKeys.ColonnaAttributo + i);
                        salProgrammatoView.SetValue(GanttKeys.ColonnaAttributo + (i - 1), difference);

                        //CALCULATE PERCENT FOR EACH ROW  BETWEEN THIS AND PREVIOUS ROW
                        if (ContatoreLista > 0)
                        {
                            if (listaSALProgrammatiView.Count() == ContatoreLista + 1)
                            {
                                salProgrammatoView.SetValue(GanttKeys.ColonnaAttributo + (i + 2), 100);
                                double? value = 100 / listaSALProgrammatiView.LastOrDefault().GetValue(GanttKeys.ColonnaAttributo + i);
                                double? preogressivePercValue = listaSALProgrammatiView.ElementAt(ContatoreLista - 1).GetValue(GanttKeys.ColonnaAttributo + i) * value;
                                listaSALProgrammatiView.ElementAt(ContatoreLista - 1).SetValue(GanttKeys.ColonnaAttributo + (i + 2), preogressivePercValue);
                            }
                            else
                            {
                                double? value = 100 / listaSALProgrammatiView.LastOrDefault().GetValue(GanttKeys.ColonnaAttributo + i);
                                double? preogressivePercValue = listaSALProgrammatiView.ElementAt(ContatoreLista - 1).GetValue(GanttKeys.ColonnaAttributo + i) * value;
                                listaSALProgrammatiView.ElementAt(ContatoreLista - 1).SetValue(GanttKeys.ColonnaAttributo + (i + 2), preogressivePercValue);
                            }
                        }
                    }
                }

                double days = (double)(salProgrammatoView.Data - listaSALProgrammatiView.ElementAt(ContatoreLista - 1).Data).TotalDays;
                durataPeriodo = Math.Ceiling(days);
                durataProgressivo = Math.Ceiling((salProgrammatoView.Data - listaSALProgrammatiView.FirstOrDefault().Data).TotalDays);

                salProgrammatoView.GiorniPeriodo = durataPeriodo;
                salProgrammatoView.GiorniProgressivo = durataProgressivo;

                ContatoreLista++;
            }
        }

        private void AddProperty(System.Dynamic.ExpandoObject expando, string propertyName, object propertyValue)
        {
            var expandoDict = expando as IDictionary<string, object>;
            expandoDict.Add(propertyName, propertyValue);
        }

        public DataTable GetDataTableFormSALProgrammatoView(GanttData ganttData, string codice, string etichetta)
        {
            DataTable dataTable = new DataTable();
            dataTable.TableName = typeof(SALProgrammatoView).FullName;
            dataTable.Columns.Add(LocalizationProvider.GetString("Data"), Type.GetType("System.DateTime"));
            dataTable.Columns.Add(LocalizationProvider.GetString("GiorniPeriodo"), Type.GetType("System.Double"));
            dataTable.Columns.Add(LocalizationProvider.GetString("GiorniProg"), Type.GetType("System.Double"));
            int ColumnCounter = 3;
            int AttributeSALPosition = -1;

            if (listaSALProgrammatiView.Count() == 0)
                return dataTable;

            if (ganttData.ProgrammazioneSAL != null)
            {
                foreach (AttributoFoglioDiCalcolo item in ganttData.ProgrammazioneSAL.AttributiStandard)
                {
                    string columnName = item.CodiceOrigine;
                    if (item.Amount)
                    {
                        columnName = item.CodiceOrigine + GanttKeys.LocalizeDelta;
                        dataTable.Columns.Add(columnName, Type.GetType("System.Double"));
                        ColumnCounter++;
                    }

                    if (item.ProgressiveAmount)
                    {
                        columnName = item.CodiceOrigine + GanttKeys.LocalizeCumulato;
                        dataTable.Columns.Add(columnName, Type.GetType("System.Double"));
                        ColumnCounter++;
                        if (codice == item.CodiceOrigine)
                            AttributeSALPosition = ColumnCounter;
                    }

                    if (item.ProductivityPerHour)
                    {
                        columnName = item.CodiceOrigine + GanttKeys.LocalizeProduttivitaH;
                        dataTable.Columns.Add(columnName, Type.GetType("System.Double"));
                        ColumnCounter++;
                    }

                    if (item.ProgressiveAmount)
                    {
                        columnName = item.CodiceOrigine + GanttKeys.LocalizeValorePercentualeProgressiva;
                        dataTable.Columns.Add(columnName, Type.GetType("System.Double"));
                        ColumnCounter++;
                    }
                }
            }

            dataTable.Columns.Add(GanttKeys.ConstSAL + " (" + etichetta + ")", Type.GetType("System.Double"));
            ColumnCounter++;

            List<double> salValues = new List<double>();

            foreach (SALProgrammatoView SALProgrammatiView in listaSALProgrammatiView)
            {
                object[] values = new object[ColumnCounter];
                values[0] = SALProgrammatiView.Data;
                values[1] = SALProgrammatiView.GiorniPeriodo;
                values[2] = SALProgrammatiView.GiorniProgressivo;
                int CounterAttribute = 0;
                int CounterValues = 3;

                foreach (AttributoFoglioDiCalcolo item in ganttData.ProgrammazioneSAL.AttributiStandard)
                {
                    CounterAttribute++;
                    if (item.Amount)
                    {
                        values[CounterValues] = SALProgrammatiView.GetValue(GanttKeys.ColonnaAttributo + CounterAttribute);
                        CounterValues++;
                    }

                    CounterAttribute++;
                    if (item.ProgressiveAmount)
                    {
                        values[CounterValues] = SALProgrammatiView.GetValue(GanttKeys.ColonnaAttributo + CounterAttribute);
                        if (AttributeSALPosition != -1 && AttributeSALPosition == (CounterValues + 1) && SALProgrammatiView.IsSAL)
                        {
                            if (values[CounterValues] == null)
                                salValues.Add(0);
                            else
                                salValues.Add((double)values[CounterValues]);
                        }
                        //salValues.Add((double)values[CounterValues]);
                        CounterValues++;
                    }

                    CounterAttribute++;
                    if (item.ProductivityPerHour)
                    {
                        values[CounterValues] = SALProgrammatiView.GetValue(GanttKeys.ColonnaAttributo + CounterAttribute);
                        CounterValues++;
                    }

                    CounterAttribute++;
                    if (item.ProgressiveAmount)
                    {
                        values[CounterValues] = SALProgrammatiView.GetValue(GanttKeys.ColonnaAttributo + CounterAttribute);
                        CounterValues++;
                    }
                }
                if (SALProgrammatiView.IsSAL)
                    values[CounterValues] = salValues.LastOrDefault();
                dataTable.Rows.Add(values);
            }
            listaSALProgrammatiView = new List<SALProgrammatoView>();
            return dataTable;
        }

        public DataTable GetDataTableFormSALProgrammatoView(List<AttributoFoglioDiCalcolo> Attributi)
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add(LocalizationProvider.GetString("Data"), Type.GetType("System.DateTime"));
            int ColumnCounter = 1;

            foreach (AttributoFoglioDiCalcolo item in Attributi)
            {
                string columnName = item.CodiceOrigine;
                if (item.Amount)
                {
                    columnName = item.CodiceOrigine + GanttKeys.LocalizeDelta;
                    dataTable.Columns.Add(columnName, Type.GetType("System.Double"));
                    ColumnCounter++;
                }

                if (item.ProgressiveAmount)
                {
                    columnName = item.CodiceOrigine + GanttKeys.LocalizeCumulato;
                    dataTable.Columns.Add(columnName, Type.GetType("System.Double"));
                    ColumnCounter++;
                }

                if (item.ProductivityPerHour)
                {
                    columnName = item.CodiceOrigine + GanttKeys.LocalizeProduttivitaH;
                    dataTable.Columns.Add(columnName, Type.GetType("System.Double"));
                    ColumnCounter++;
                }

                if (item.ProgressiveAmount)
                {
                    columnName = item.CodiceOrigine + GanttKeys.LocalizeValorePercentualeProgressiva;
                    dataTable.Columns.Add(columnName, Type.GetType("System.Double"));
                    ColumnCounter++;
                }
            }

            foreach (SALProgrammatoView SALProgrammatiView in listaSALProgrammatiView)
            {
                object[] values = new object[ColumnCounter];
                values[0] = SALProgrammatiView.Data;
                int CounterValues = 1;

                foreach (AttributoFoglioDiCalcolo item in Attributi)
                {
                    if (item.Amount)
                    {
                        values[CounterValues] = SALProgrammatiView.GetValue(GanttKeys.ColonnaAttributo + CounterValues);
                        CounterValues++;
                    }

                    if (item.ProgressiveAmount)
                    {
                        values[CounterValues] = SALProgrammatiView.GetValue(GanttKeys.ColonnaAttributo + CounterValues);
                        CounterValues++;
                    }
                    if (item.ProductivityPerHour)
                    {
                        values[CounterValues] = SALProgrammatiView.GetValue(GanttKeys.ColonnaAttributo + CounterValues);
                        CounterValues++;
                    }
                    if (item.ProgressiveAmount)
                    {
                        values[CounterValues] = SALProgrammatiView.GetValue(GanttKeys.ColonnaAttributo + CounterValues);
                        CounterValues++;
                    }
                }
                dataTable.Rows.Add(values);
            }
            listaSALProgrammatiView = new List<SALProgrammatoView>();
            return dataTable;
        }

        public List<SALProgrammatoView> GeneraSchedulazioneValori(int tipologiaPeriodo, List<AttributoFoglioDiCalcolo> attributiUtilizzati, DateTime dateFrom)
        {
            bool UseCalendar = false;

            DateTime dataInizio = GetDataInizio();
            DateTime dataFine = GetDataFine();
            List<DateTime> dates = new List<DateTime>();

            listaSALProgrammatiView = new List<SALProgrammatoView>();

            //CASO GIORNO
            if (tipologiaPeriodo == 0)
            {
                dataInizio = new DateTime(dataInizio.Year, dataInizio.Month, dataInizio.Day, 0, 0, 0);
                AddSalProgrammatoViewByDaysRecursive(dates, dataInizio, dataFine);
                listaSALProgrammatiView.AddRange(GetSALProgrammatoViewByDateByActivity(attributiUtilizzati, dates, TipologiaPeriodo.Giorno));
                
                
                //foreach (var item in listaSALProgrammatiView)
                //    item.Data = item.Data.AddDays(-1);

            }
            //CASO SETTIMANA
            if (tipologiaPeriodo == 1)
            {

                //dataInizio = dateFrom;
                //if (dataInizio > dataFine)
                //    return listaSALProgrammatiView;
                //AddSalProgrammatoViewByWeeksRecursive(dates, dataInizio, dataFine);
                //listaSALProgrammatiView.AddRange(GetSALProgrammatoViewByDateByActivity(attributiUtilizzati, dates, TipologiaPeriodo.Settimana));

                //calcolo inzio della settimana
                dataInizio = new DateTime(dataInizio.Year, dataInizio.Month, dataInizio.Day, 0, 0, 0);
                int nDays = dataInizio.StartOfWeek(DayOfWeek.Monday);
                dataInizio = dataInizio.AddDays(-nDays);

                AddSalProgrammatoViewByWeeksRecursive(dates, dataInizio, dataFine);
                listaSALProgrammatiView.AddRange(GetSALProgrammatoViewByDateByActivity(attributiUtilizzati, dates, TipologiaPeriodo.Settimana));

            }
            //CASO MESE
            if (tipologiaPeriodo == 2)
            {
                dataInizio = new DateTime(dataInizio.Year, dataInizio.Month, 1);
                AddSalProgrammatoViewByMonthsRecursive(dates, dataInizio, dataFine);
                listaSALProgrammatiView.AddRange(GetSALProgrammatoViewByDateByActivity(attributiUtilizzati, dates, TipologiaPeriodo.Mese));
                //PostCreationSALRecordOperationPerDate(UseCalendar, dataInizio, dataFine, listaSALProgrammatiView);
            }
            //CASO ANNO
            if (tipologiaPeriodo == 3)
            {
                dataInizio = new DateTime(dataInizio.Year, 1, 1);
                AddSalProgrammatoViewByYearsRecursive(dates, dataInizio, dataFine);
                listaSALProgrammatiView.AddRange(GetSALProgrammatoViewByDateByActivity(attributiUtilizzati, dates, TipologiaPeriodo.Anno));
                //PostCreationSALRecordOperationPerDate(UseCalendar, dataInizio, dataFine, listaSALProgrammatiView);
            }
            //CASO PRODUZIONE COSTANTE
            if (tipologiaPeriodo == 4)
            {
                //listaSALProgrammatiView.AddRange(GetSALProgrammatoView(attributiUtilizzati));
                dates = GetDateAProduttivitaCostante();
                //RIMUOVO LA PRIMA RIGA PERCHé E' INIZIO GANTT
                if (dates.Count() > 0)
                    dates.RemoveAt(0);
                listaSALProgrammatiView.AddRange(GetSALProgrammatoViewByDateByActivity(attributiUtilizzati, dates, TipologiaPeriodo.ProduzioneCostante));
            }
            //CASO TABELLA PROGRAMMAZIONE SAL
            if (tipologiaPeriodo == 5)
            {
                GanttData GanttData = DataService.GetGanttData();
                if (GanttData?.ProgrammazioneSAL != null)
                {
                    if (GanttData.ProgrammazioneSAL.PuntiNotevoliPerData != null)
                    {
                        foreach (PuntoNotevolePerData puntoNotevolePerData in GanttData.ProgrammazioneSAL.PuntiNotevoliPerData.OrderBy(d => d.Data))
                        {
                            if (puntoNotevolePerData.Data != new DateTime())
                            {
                                if (puntoNotevolePerData.IsSAL)
                                    dates.Add(puntoNotevolePerData.Data);
                            }
                        }
                    }
                }
                //RIMUOVO LA PRIMA RIGA PERCHé E' INIZIO GANTT
                if (dates.Count() > 0)
                    dates.RemoveAt(0);
                listaSALProgrammatiView.AddRange(GetSALProgrammatoViewByDateByActivity(attributiUtilizzati, dates, TipologiaPeriodo.ProgrammazioneSAL));
            }

            //PORTO ALLA FINE DELLA GIORNATA PRECEDENTE
            //foreach (var item in listaSALProgrammatiView)
            //{
            //    item.Data = ReturnAsEndingDateTime(item.Data, UseCalendar);
            //}
            return listaSALProgrammatiView;
        }

        enum TipologiaPeriodo
        {
            Nothing = -1,
            Giorno = 1,
            Settimana = 2,
            Mese = 3, 
            Anno = 4, 
            ProduzioneCostante = 5,
            ProgrammazioneSAL = 6,
        }

        private void AddSalProgrammatoViewByDaysRecursive(List<DateTime> dates, DateTime data, DateTime dataFine)
        {
            //DateTime newDate = ReturnLastDataAvailable(data, false);
            //if (newDate < dataFine)
            //{
            //    dates.Add(newDate);
            //    AddSalProgrammatoViewByDaysRecursive(dates, newDate, dataFine);
            //}
            //else
            //{
            //    dates.Add(newDate);
            //}
            if (data.AddDays(1) < dataFine)
            {
                dates.Add(data);
                AddSalProgrammatoViewByDaysRecursive(dates, data.AddDays(1), dataFine);
            }
            else
            {
                dates.Add(data);
            }

        }

        private void AddSalProgrammatoViewByWeeksRecursive(List<DateTime> dates, DateTime data, DateTime dataFine)
        {
            ////LA FUNZIONE AGGIUNGE GIA UN GIORNO QUINDI PER FARE LA SETTIMANA MANCANO 6
            //DateTime newDate = ReturnLastDataAvailable(data, false).AddDays(6);
            //if (newDate < dataFine)
            //{
            //    dates.Add(newDate);
            //    AddSalProgrammatoViewByWeeksRecursive(dates, newDate, dataFine);
            //}
            //else
            //{
            //    dates.Add(newDate);
            //}

            if (data.AddDays(7) < dataFine)
            {
                dates.Add(data);
                AddSalProgrammatoViewByWeeksRecursive(dates, data.AddDays(7), dataFine);
            }
            else
            {
                dates.Add(data);
            }

        }

        private void AddSalProgrammatoViewByMonthsRecursive(List<DateTime> dates, DateTime data, DateTime dataFine)
        {
            //DateTime newDate = new DateTime(data.Year, data.Month, DateTime.DaysInMonth(data.Year, data.Month));

            if (data.AddMonths(1) < dataFine)
            {
                dates.Add(data);
                AddSalProgrammatoViewByMonthsRecursive(dates, data.AddMonths(1), dataFine);
            }
            else
            {
                dates.Add(data);
            }
        }
        private void AddSalProgrammatoViewByYearsRecursive(List<DateTime> dates, DateTime data, DateTime dataFine)
        {
            //DateTime newDate = new DateTime(data.Year, 12, 31);
            //if (newDate < dataFine)
            //{
            //    //listSALProgrammatoView.AddRange(GetSALProgrammatoViewByDateByActivity(attributiUtilizzati, ReturnLastDataAvailable(date, false)));
            //    dates.Add(newDate);
            //    AddSalProgrammatoViewByYearsRecursive(dates, newDate.AddYears(1), dataFine);
            //}
            //else
            //{
            //    dates.Add(newDate);
            //}

            if (data.AddYears(1) < dataFine)
            {
                dates.Add(data);
                AddSalProgrammatoViewByYearsRecursive(dates, data.AddYears(1), dataFine);
            }
            else
            {
                dates.Add(data);
            }
        }


        public DataTable GetDataTable()
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(UnknowDatasource);
            DataTable = JsonConvert.DeserializeObject<DataTable>(json);
            return DataTable;
        }

        public T GetGenericList<T>()
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(UnknowDatasource);
            T genericList = JsonConvert.DeserializeObject<T>(json);
            return genericList;
        }
        public T GetGeneric<T>(dynamic unknowObjectLocal)
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(unknowObjectLocal);
            T genericList = JsonConvert.DeserializeObject<T>(json);
            return genericList;
        }

        public List<AttivitaPerCalendario> GetCalendari()
        {
            return CalendariItems;
        }

        public DateTime GetDataInizio()
        {
            return MinDate;
        }
        public DateTime GetDataFine()
        {
            return MaxDate;
        }

    }
    public class DateTimeWBSItemEntity
    {
        public double ProgressiveValue { get; set; }
        public DateTime Data { get; set; }
        public List<TreeEntity> WBSItemsDataInizio { get; set; } = new List<TreeEntity>();
        public List<TreeEntity> WBSItemsDataFine { get; set; } = new List<TreeEntity>();
    }
    public class ValueWithPercDate
    {
        public double valueperc { get; set; }
        public double value { get; set; }
        public DateTime date { get; set; }
    }

    public class PregressiveDateValue
    {
        public DateTime Date { get; set; }
        public double Value { get; set; }

    }

    public class AttivitaPerCalendario
    {
        public Guid CalendarioItemGuid { get; set; }
        public CalendariItem CalendarioItem { get; set; }
        public List<WBSItemProperty> WBSItemsProperty { get; set; }

    }

    public class WBSItemProperty
    {
        public Guid Id { get; set; }
        public DateTime DateInizio { get; set; }
        public DateTime DateFine { get; set; }
    }

}
