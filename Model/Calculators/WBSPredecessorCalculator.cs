using Commons;
using MasterDetailModel;
using Syncfusion.Windows.Controls.Gantt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class WBSPredecessorCalculator
    {
        ProjectServiceBase _projectServiceBase = null;
        ProjectService DataService { get => _projectServiceBase as ProjectService; }
        Dictionary<Guid, WBSPredecessors> _predecessorsById = new Dictionary<Guid, WBSPredecessors>();
        Dictionary<Guid, List<Guid>> _dependentIds = new Dictionary<Guid, List<Guid>>();
        Dictionary<Guid, DateTimeCalculator> _timeCalulatorsByEntity = new Dictionary<Guid, DateTimeCalculator>();
        Dictionary<Guid, DateTimeCalculator> _timeCalulatorsByCalendario = new Dictionary<Guid, DateTimeCalculator>();

        //scopo: tener traccia delle entità modificate
        public HashSet<Guid> EntityIdsChanged { get; protected set; } = new HashSet<Guid>();

        public WBSPredecessorCalculator(ProjectServiceBase projectServiceBase)
        {
            _projectServiceBase = projectServiceBase;
        }
        public void Init()
        {
            _predecessorsById.Clear();
            _dependentIds.Clear();
            EntityIdsChanged.Clear();

            IEnumerable<Entity> entities = DataService.GetEntities(BuiltInCodes.EntityType.WBS);
            foreach (WBSItem ent in entities)
            {
                if (ent.IsParent)
                    continue;

                WBSPredecessors preds = null;

                string json = ent.Attributi[BuiltInCodes.Attributo.Predecessor].Valore.PlainText;
                if (!JsonSerializer.JsonDeserialize(json, out preds, typeof(WBSPredecessors)))
                    preds = new WBSPredecessors();

                _predecessorsById.Add(ent.EntityId, preds);

                if (!_dependentIds.ContainsKey(ent.EntityId))
                    _dependentIds.Add(ent.EntityId, new List<Guid>());

                foreach (WBSPredecessor pred in preds.Items)
                {
                    if (!_dependentIds.ContainsKey(pred.WBSItemId))
                        _dependentIds.Add(pred.WBSItemId, new List<Guid>());

                    _dependentIds[pred.WBSItemId].Add(ent.EntityId);
                }

                //if (timeCalulatorsByCalendario == null)
                //{
                //    DateTimeCalculator timeCalc = CreateDateTimeCalculator(ent);
                //    if (!_timeCalulatorsByEntity.ContainsKey(ent.Id))
                //        _timeCalulatorsByEntity.Add(ent.Id, timeCalc);
                //}
                //else
                //{
                //    _timeCalulatorsByEntity = timeCalulatorsByCalendario;
                //}

                DateTimeCalculator timeCalc = CreateDateTimeCalculator(ent);
                if (!_timeCalulatorsByEntity.ContainsKey(ent.EntityId))
                    _timeCalulatorsByEntity.Add(ent.EntityId, timeCalc);

            }
        }

        public void Run(IEnumerable<Guid> WBSItemsId)
        {
            foreach (Guid id in WBSItemsId)
                Run(id);
        }

        public void Run(Guid WBSItemId)
        {
            WBSItem depEnt = DataService.GetEntityById(BuiltInCodes.EntityType.WBS, WBSItemId) as WBSItem;
            Run(depEnt);
        }

        public void Run(WBSItem entChanged)
        {

            if (!entChanged.Deleted && !entChanged.IsParent)
                UpdateItSelf(entChanged);

            //Aggiornamento delle entità dipendenti da entChanged
            RunRecursive(entChanged);
        }



        private void RunRecursive(WBSItem entChanged)
        {
            if (entChanged == null)
                return;

            if (_dependentIds == null)
                return;

            List<Guid> dependentIds = null;
            if (!_dependentIds.TryGetValue(entChanged.EntityId, out dependentIds))
                return;


            foreach (Guid depId in dependentIds)
            {
                WBSPredecessors depEntPreds = null;
                if (!_predecessorsById.TryGetValue(depId, out depEntPreds))
                    return;

                WBSItem depEnt = DataService.GetEntityById(BuiltInCodes.EntityType.WBS, depId) as WBSItem;


                if (entChanged.IsParent || entChanged.Deleted)
                {
                    depEntPreds.Items.RemoveAll(item => item.WBSItemId == entChanged.EntityId);
                    string json = string.Empty;
                    JsonSerializer.JsonSerialize(depEntPreds, out json);
                    if (!string.IsNullOrEmpty(json))
                    {
                        depEnt.Attributi[BuiltInCodes.Attributo.Predecessor].Valore = new ValoreTesto() { V = json };
                        UpdateItSelf(depEnt);
                    }
                }
                else
                {
                    WBSPredecessor depEntPred = depEntPreds.Items.FirstOrDefault(item => item.WBSItemId == entChanged.EntityId) as WBSPredecessor;
                    if (depEntPred != null)
                    {
                        CalculateDependentItem(entChanged, depEnt, depEntPred, depEntPreds);
                        RunRecursive(depEnt);
                    }
                }

            }
        }

        /// <summary>
        /// Aggiornamento di se stesso in base ai predecessori
        /// </summary>
        /// <param name="entChanged"></param>
        private void UpdateItSelf(WBSItem entChanged)
        {
            if (entChanged == null)
                return;

            if (!_predecessorsById.ContainsKey(entChanged.EntityId))
                return;

            DateTime dataInizio = entChanged.GetDataInizio().Value;
            WBSPredecessors preds = _predecessorsById[entChanged.EntityId];
            DateTimeCalculator timeCalc = _timeCalulatorsByEntity[entChanged.EntityId];
            if (timeCalc == null)
                return;

            DateTime predecessorsDateLimit = CalculatePredecessorsDateLimit(entChanged, timeCalc);
            if (dataInizio < predecessorsDateLimit || preds.Items.FirstOrDefault(item => item.DelayFixed) != null)
            {
                dataInizio = timeCalc.AsStartingDateTime(predecessorsDateLimit);
                DateTime dataFine = timeCalc.AddWorkingMinutes(dataInizio, entChanged.GetOreLavoro().Value * 60);
                dataFine = timeCalc.AsEndingDateTime(dataFine);

                entChanged.Attributi[BuiltInCodes.Attributo.DataInizio].Valore = new ValoreData() { V = dataInizio };
                entChanged.Attributi[BuiltInCodes.Attributo.DataFine].Valore = new ValoreData() { V = dataFine };
            }
        }
        private void CalculateDependentItem(WBSItem entChanged, WBSItem depEnt, WBSPredecessor depPred, WBSPredecessors depPreds)
        {
            if (entChanged == null || depEnt == null || depPred == null)
                return;

            DateTimeCalculator timeCalc = _timeCalulatorsByEntity[depEnt.EntityId];
            if (timeCalc == null)
                return;

            EntityIdsChanged.Add(depEnt.EntityId);

            DateTime predecessorsDateLimit = CalculatePredecessorsDateLimit(depEnt, timeCalc);


            if (depPred.Type == WBSPredecessorType.FinishToStart)
            {
                // Finish to Start (FS)—The predecessor ends before the successor can begin.
                ValoreData valDataSource = entChanged.Attributi[BuiltInCodes.Attributo.DataFine].Valore as ValoreData;
                DateTime dataSource = valDataSource.V.Value;

                ValoreData valDataDep = depEnt.Attributi[BuiltInCodes.Attributo.DataInizio].Valore as ValoreData;
                DateTime dataInizioDep = valDataDep.V.Value;

                ValoreReale valLavoroDep = depEnt.Attributi[BuiltInCodes.Attributo.Lavoro].Valore as ValoreReale;
                double lavoroDep = valLavoroDep.RealResult.Value;

                dataInizioDep = CalculateDependentDate(depPred, timeCalc, dataSource, dataInizioDep);
                if (dataInizioDep < predecessorsDateLimit)
                    dataInizioDep = predecessorsDateLimit;

                dataInizioDep = timeCalc.AsStartingDateTime(dataInizioDep);

                DateTime dataFineDep = timeCalc.AddWorkingMinutes(dataInizioDep, lavoroDep * 60);
                dataFineDep = timeCalc.AsEndingDateTime(dataFineDep);

                depEnt.Attributi[BuiltInCodes.Attributo.DataInizio].Valore = new ValoreData() { V = dataInizioDep };
                depEnt.Attributi[BuiltInCodes.Attributo.DataFine].Valore = new ValoreData() { V = dataFineDep };
            }
            else if (depPred.Type == WBSPredecessorType.StartToStart)
            {
                //Start to Start (SS)—The predecessor begins before the successor can begin.
                ValoreData valDataSource = entChanged.Attributi[BuiltInCodes.Attributo.DataInizio].Valore as ValoreData;
                DateTime dataSource = valDataSource.V.Value;

                ValoreData valDataDep = depEnt.Attributi[BuiltInCodes.Attributo.DataInizio].Valore as ValoreData;
                DateTime dataInizioDep = valDataDep.V.Value;

                ValoreReale valLavoroDep = depEnt.Attributi[BuiltInCodes.Attributo.Lavoro].Valore as ValoreReale;
                double lavoroDep = valLavoroDep.RealResult.Value;

                dataInizioDep = CalculateDependentDate(depPred, timeCalc, dataSource, dataInizioDep);
                if (dataInizioDep < predecessorsDateLimit)
                    dataInizioDep = predecessorsDateLimit;

                dataInizioDep = timeCalc.AsStartingDateTime(dataInizioDep);

                DateTime dataFineDep = timeCalc.AddWorkingMinutes(dataInizioDep, lavoroDep * 60);
                dataFineDep = timeCalc.AsEndingDateTime(dataFineDep);

                depEnt.Attributi[BuiltInCodes.Attributo.DataInizio].Valore = new ValoreData() { V = dataInizioDep };
                depEnt.Attributi[BuiltInCodes.Attributo.DataFine].Valore = new ValoreData() { V = dataFineDep };
            }
            else if (depPred.Type == WBSPredecessorType.FinishToFinish)
            {
                //Finish to Finish (FF)—The predecessor ends before the successor can end.
                ValoreData valDataSource = entChanged.Attributi[BuiltInCodes.Attributo.DataFine].Valore as ValoreData;
                DateTime dataSource = valDataSource.V.Value;

                ValoreData valDataDep = depEnt.Attributi[BuiltInCodes.Attributo.DataFine].Valore as ValoreData;
                DateTime dataFineDep = valDataDep.V.Value;

                ValoreReale valLavoroDep = depEnt.Attributi[BuiltInCodes.Attributo.Lavoro].Valore as ValoreReale;
                double lavoroDep = valLavoroDep.RealResult.Value;

                dataFineDep = CalculateDependentDate(depPred, timeCalc, dataSource, dataFineDep);
                dataFineDep = timeCalc.AsEndingDateTime(dataFineDep);

                DateTime dataInizioDep = timeCalc.AddWorkingMinutes(dataFineDep, -(lavoroDep * 60));
                dataInizioDep = timeCalc.AsStartingDateTime(dataInizioDep);

                if (dataInizioDep < predecessorsDateLimit)
                {
                    dataInizioDep = predecessorsDateLimit;
                    dataFineDep = timeCalc.AddWorkingMinutes(dataInizioDep, lavoroDep * 60);
                    dataFineDep = timeCalc.AsEndingDateTime(dataFineDep);
                }

                depEnt.Attributi[BuiltInCodes.Attributo.DataInizio].Valore = new ValoreData() { V = dataInizioDep };
                depEnt.Attributi[BuiltInCodes.Attributo.DataFine].Valore = new ValoreData() { V = dataFineDep };
            }
            else if (depPred.Type == WBSPredecessorType.StartToFinish)
            {
                //Start to Finish (SF)—The predecessor begins before the successor can end.
                ValoreData valDataSource = entChanged.Attributi[BuiltInCodes.Attributo.DataInizio].Valore as ValoreData;
                DateTime dataSource = valDataSource.V.Value;

                ValoreData valDataDep = depEnt.Attributi[BuiltInCodes.Attributo.DataFine].Valore as ValoreData;
                DateTime dataFineDep = valDataDep.V.Value;

                ValoreReale valLavoroDep = depEnt.Attributi[BuiltInCodes.Attributo.Lavoro].Valore as ValoreReale;
                double lavoroDep = valLavoroDep.RealResult.Value;

                dataFineDep = CalculateDependentDate(depPred, timeCalc, dataSource, dataFineDep);
                dataFineDep = timeCalc.AsEndingDateTime(dataFineDep);

                DateTime dataInizioDep = timeCalc.AddWorkingMinutes(dataFineDep, -(lavoroDep * 60));
                dataInizioDep = timeCalc.AsStartingDateTime(dataInizioDep);

                if (dataInizioDep < predecessorsDateLimit)
                {
                    dataInizioDep = predecessorsDateLimit;
                    dataFineDep = timeCalc.AddWorkingMinutes(dataInizioDep, lavoroDep * 60);
                    dataFineDep = timeCalc.AsEndingDateTime(dataFineDep);
                }

                depEnt.Attributi[BuiltInCodes.Attributo.DataInizio].Valore = new ValoreData() { V = dataInizioDep };
                depEnt.Attributi[BuiltInCodes.Attributo.DataFine].Valore = new ValoreData() { V = dataFineDep };
            }

            
        }

        //private void UpdatePredecessorsDelayDays(WBSItem entChanged)
        //{
        //    WBSPredecessors entChangedPreds = _predecessorsById[entChanged.Id];
        //    DateTimeCalculator timeCalc = _timeCalulators[entChanged.Id];


        //    //Aggiorno i Delay di tutti i predecessori
        //    bool depPredsToSerialize = false;
        //    foreach (WBSPredecessor pred in entChangedPreds.Items)
        //    {
        //        if (pred.WBSItemId != entChanged.Id)
        //        {
        //            depPredsToSerialize = true;
        //            double delayDays = CalculatePredecessorDelay(entChanged, pred, timeCalc);
        //            if (delayDays < 0)
        //                delayDays = 0;

        //            pred.DelayDays = delayDays;
        //        }
        //    }

        //    if (depPredsToSerialize)
        //    {
        //        string json = string.Empty;
        //        if (JsonSerializer.JsonSerialize(entChangedPreds, out json))
        //        {
        //            entChanged.Attributi[BuiltInCodes.Attributo.Predecessor].Valore = new ValoreTesto() { V = json };

        //            string desc = WBSEntityAttributiUpdater.GetAttributoPredecessoriTextDescription(entChangedPreds, DataService);
        //            entChanged.Attributi[BuiltInCodes.Attributo.PredecessorText].Valore = new ValoreTesto() { V = desc };
        //        }
        //    }

        //}

        private DateTime CalculatePredecessorsDateLimit(WBSItem depEnt, DateTimeCalculator depTimeCalc)
        {
            DateTime predecessorsDateLimit = DateTime.MinValue;
            try
            {
                //Calcolo il limite per la data di inizio di depEnt
                WBSPredecessors depPreds = _predecessorsById[depEnt.EntityId];

                foreach (WBSPredecessor pred in depPreds.Items)
                {
                    WBSItem ent = DataService.GetEntityById(BuiltInCodes.EntityType.WBS, pred.WBSItemId) as WBSItem;
                    if (ent == null)
                        return predecessorsDateLimit;

                    if (pred.Type == WBSPredecessorType.FinishToStart)
                    {
                        DateTime date = ent.GetDataFine().Value;
                        date = depTimeCalc.AsStartingDateTime(date);
                        date = depTimeCalc.AddWorkingDays(date, pred.DelayDays);

                        if (date > predecessorsDateLimit)
                            predecessorsDateLimit = date;
                    }
                    else if (pred.Type == WBSPredecessorType.StartToStart)
                    {
                        DateTime date = ent.GetDataInizio().Value;
                        date = depTimeCalc.AsStartingDateTime(date);
                        date = depTimeCalc.AddWorkingDays(date, pred.DelayDays);

                        if (date > predecessorsDateLimit)
                            predecessorsDateLimit = date;
                    }
                    else if (pred.Type == WBSPredecessorType.FinishToFinish)
                    {
                        DateTime date = ent.GetDataFine().Value;
                        date = depTimeCalc.AsStartingDateTime(date);
                        //Add delayDays
                        date = depTimeCalc.AddWorkingDays(date, pred.DelayDays);

                        //Add depEnt lavoro
                        double oreLavoro = depEnt.GetOreLavoro().Value;
                        date = depTimeCalc.AddWorkingMinutes(date, -(oreLavoro * 60));

                        if (date > predecessorsDateLimit)
                            predecessorsDateLimit = date;
                    }
                    else if (pred.Type == WBSPredecessorType.StartToFinish)
                    {
                        DateTime date = ent.GetDataInizio().Value;
                        date = depTimeCalc.AsStartingDateTime(date);
                        date = depTimeCalc.AddWorkingDays(date, pred.DelayDays);

                        //Add depEnt lavoro
                        double oreLavoro = depEnt.GetOreLavoro().Value;
                        date = depTimeCalc.AddWorkingMinutes(date, -(oreLavoro * 60));

                        if (date > predecessorsDateLimit)
                            predecessorsDateLimit = date;
                    }

                }

            }
            catch (Exception ex) 
            {
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex.ToString());
            }

            return predecessorsDateLimit;
        }

        //public static bool TryCalculatePredecessorDelay(WBSItem depEnt, WBSItem predEnt, DateTimeCalculator depTimeCalc, out double delayDays)
        //{
        //    delayDays = 0;

        //    WBSPredecessors preds = depEnt.GetPredecessors();
        //    WBSPredecessor pred = preds.Items.FirstOrDefault(item => item.WBSItemId == predEnt.Id);
        //    if (pred == null)
        //        return false;

        //    if (pred.Type == WBSPredecessorType.FinishToStart)
        //    {
        //        delayDays = depTimeCalc.GetWorkingDaysBetween(predEnt.GetDataFine().Value, depEnt.GetDataInizio().Value);
        //    }
        //    else if (pred.Type == WBSPredecessorType.StartToStart)
        //    {
        //        delayDays = depTimeCalc.GetWorkingDaysBetween(predEnt.GetDataInizio().Value, depEnt.GetDataInizio().Value);
        //    }
        //    else if (pred.Type == WBSPredecessorType.FinishToFinish)
        //    {
        //        delayDays = depTimeCalc.GetWorkingDaysBetween(predEnt.GetDataFine().Value, depEnt.GetDataFine().Value);
        //    }
        //    else if (pred.Type == WBSPredecessorType.StartToFinish)
        //    {
        //        delayDays = depTimeCalc.GetWorkingDaysBetween(predEnt.GetDataInizio().Value, depEnt.GetDataFine().Value);
        //    }

        //    return true;
        //}

        public static bool IsCriticalPredecessor(WBSItem depEnt, WBSItem predEnt, DateTimeCalculator depTimeCalc, DateTimeCalculator predTimeCalc)
        {
         

            WBSPredecessors preds = depEnt.GetPredecessors();
            WBSPredecessor pred = preds.Items.FirstOrDefault(item => item.WBSItemId == predEnt.EntityId);
            if (pred == null)
                return false;

            DateTime depDate = DateTime.MinValue;

            if (pred.Type == WBSPredecessorType.FinishToStart)
            {
                depDate = depEnt.GetDataInizio().GetValueOrDefault();
                DateTime lastPredDate = depTimeCalc.AddWorkingDays(depDate, -pred.DelayDays);
                lastPredDate = depTimeCalc.GetStartingDateTimeOfDay(lastPredDate);
                double workingMinutes = predTimeCalc.GetWorkingMinutesBetween(predEnt.GetDataFine().GetValueOrDefault(), lastPredDate);
                if (workingMinutes > 0)
                    return false;
            }
            else if (pred.Type == WBSPredecessorType.StartToStart)
            {
                depDate = depEnt.GetDataInizio().GetValueOrDefault();
                DateTime lastPredDate = depTimeCalc.AddWorkingDays(depDate, -pred.DelayDays);
                lastPredDate = depTimeCalc.GetStartingDateTimeOfDay(lastPredDate);
                double workingMinutes = predTimeCalc.GetWorkingMinutesBetween(predEnt.GetDataInizio().GetValueOrDefault(), lastPredDate);
                if (workingMinutes > 0)
                    return false;
            }
            else if (pred.Type == WBSPredecessorType.FinishToFinish)
            {
                depDate = depEnt.GetDataFine().GetValueOrDefault();
                DateTime lastPredDate = depTimeCalc.AddWorkingDays(depDate, -pred.DelayDays);
                lastPredDate = depTimeCalc.GetStartingDateTimeOfDay(lastPredDate);
                double workingMinutes = predTimeCalc.GetWorkingMinutesBetween(predEnt.GetDataFine().GetValueOrDefault(), lastPredDate);
                if (workingMinutes > 0)
                    return false;
            }
            else if (pred.Type == WBSPredecessorType.StartToFinish)
            {
                depDate = depEnt.GetDataFine().GetValueOrDefault();
                DateTime lastPredDate = depTimeCalc.AddWorkingDays(depDate, -pred.DelayDays);
                lastPredDate = depTimeCalc.GetStartingDateTimeOfDay(lastPredDate);
                double workingMinutes = predTimeCalc.GetWorkingMinutesBetween(predEnt.GetDataInizio().GetValueOrDefault(), lastPredDate);
                if (workingMinutes > 0)
                    return false;
            }

            return true;
        }

        private static DateTime CalculateDependentDate(WBSPredecessor pred, DateTimeCalculator timeCalc, DateTime dataSource, DateTime dataDep)
        {
            DateTime dataDepTemp = DateTime.MinValue;
            if (pred.DelayDays != 0)//se positivo è avanti nel tempo (dopo)
            {
                if (pred.DelayDays > 0)
                    dataDepTemp = timeCalc.AsStartingDateTime(dataSource);
                else
                    dataDepTemp = timeCalc.AsEndingDateTime(dataSource);

                dataDepTemp = timeCalc.AddWorkingDays(dataDepTemp, pred.DelayDays);
            }
            else
                dataDepTemp = timeCalc.GetNextWorkingMoment(dataSource);

            if (pred.DelayFixed)
            {
                dataDep = dataDepTemp;
            }
            else
            {
                if (dataDepTemp > dataDep)
                    dataDep = dataDepTemp;
            }

            return dataDep;
        }

        private DateTimeCalculator CreateDateTimeCalculator(WBSItem ent)
        {
            DateTimeCalculator timeCalc = null;
            string wbsCalendarioIdCodice = string.Join(WBSItemType.AttributoCodiceSeparator, BuiltInCodes.EntityType.Calendari, BuiltInCodes.Attributo.Id);

            Guid wbsCalendarioId = ent.GetAttributoGuidId(wbsCalendarioIdCodice);
            if (wbsCalendarioId != Guid.Empty)
            {
                if (!_timeCalulatorsByCalendario.TryGetValue(wbsCalendarioId, out timeCalc))
                {

                    CalendariItem calendario = DataService.GetEntityById(BuiltInCodes.EntityType.Calendari, wbsCalendarioId) as CalendariItem;
                    if (calendario == null)
                        return new DateTimeCalculator(WeekHours.Default, new CustomDays());
                    else
                        timeCalc = new DateTimeCalculator(calendario.GetWeekHours(), calendario.GetCustomDays());

                    _timeCalulatorsByCalendario.Add(wbsCalendarioId, timeCalc);
                }
            }
            return timeCalc;
        }

        internal bool ValidateItem(WBSItem wbsItem)
        {
            //WBSPredecessors predecessors = null;
            //_predecessorsById.TryGetValue(wbsItem.Id, out predecessors);

            //if (predecessors.Items.Any())
            //{
            //    foreach (WBSPredecessor pred in predecessors.Items)
            //    {

            //    }
            //}

            return true;
        }

        internal void RemovePredecessorsTo(HashSet<Guid> invalidEntsId)
        {
            if (invalidEntsId == null)
                return;

            ModelAction action = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.WBS, ActionName = ActionName.MULTI };

            foreach (Guid invalidEntId in invalidEntsId)
            {

                List<Guid> depIds = null;
                _dependentIds.TryGetValue(invalidEntId, out depIds);
                if (depIds != null)
                {
                    foreach (Guid depId in depIds)
                    {
                        WBSItem depEnt = DataService.GetEntityById(BuiltInCodes.EntityType.WBS, depId) as WBSItem;

                        if (depEnt == null)
                            continue;
                        
                        WBSPredecessors preds = depEnt.GetPredecessors();

                        if (preds == null)
                            continue;

                        int predCount = preds.Items.Count;

                        preds.Items.RemoveAll(item => invalidEntsId.Contains(item.WBSItemId));

                        if (preds.Items.Count < predCount)
                        {
                            string json = string.Empty;
                            JsonSerializer.JsonSerialize(preds, out json);

                            depEnt.Attributi[BuiltInCodes.Attributo.Predecessor].Valore = new ValoreTesto() { V = json };

                            string predsText = WBSEntityAttributiUpdater.GetAttributoPredecessoriTextDescription(preds, DataService);
                            depEnt.Attributi[BuiltInCodes.Attributo.PredecessorText].Valore = new ValoreTesto() { V = predsText };

                            //UpdateItSelf(depEnt);
                        }
                    }
                }


            }


        }
    }
}
