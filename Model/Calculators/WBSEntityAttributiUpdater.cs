using CommonResources;
using Commons;
using DevExpress.Data;
using DevExpress.Mvvm.POCO;
using DevExpress.Utils.Serializing.Helpers;
using DevExpress.Xpf.Core.ReflectionExtensions.Internal;
using MasterDetailModel;
using Model.Calculators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace Model
{
    public class WBSEntityAttributiUpdater : SectionTreeEntityAttributiUpdater
    {
        ProjectService DataService { get => _projectService as ProjectService; }
        Dictionary<Guid, DateTimeCalculator> _timeCalculatorsByCalendario = new Dictionary<Guid, DateTimeCalculator>();
        //EntitiesHelper _entitiesHelper = null;

        public WBSEntityAttributiUpdater(ProjectServiceBase projectService) : base(projectService)
        {
            _calculatorFunction = _projectService.Calculator.WBSCalculatorFunction;
            _entitiesHelper = new EntitiesHelper(DataService);
        }

        //public override void CalcolaEntities(string entityTypeKey, IEnumerable<Entity> entities, EntitiesError error)
        //{
        //    //N.B. L'aggiornamento alla maodifica della WBS e Gantt a fronte di un evento esterno alla sezione WBS viene demandata al comando di aggiornamento presente nella sezione WBS

        //    //CalcolaEntities();
        //    //base.CalcolaEntitiesByExternalAttributo(entities);
        //}

        public override void CalcolaEntities(string entityTypeKey, EntityCalcOptions options, IEnumerable<Guid> entitiesId = null, EntitiesError error = null)
        {

            //entitiesId != null => modifica nultipla




            List<FilterData> filterByDepth = new List<FilterData>();
            //Dictionary<Guid, DateTimeCalculator> timeCalculatorsByEntity = new Dictionary<Guid, DateTimeCalculator>();
            _timeCalculatorsByCalendario.Clear();

            List<TreeEntity> allEntities = _projectService.GetTreeEntitiesList(BuiltInCodes.EntityType.WBS);

            List<TreeEntity> entities = null;
            if (entitiesId == null)
                entities = allEntities;
            else
            {
                List<Guid> entitiesFound = null;
                List<TreeEntityMasterInfo> TreeInfo = DataService.GetFilteredTreeEntities(BuiltInCodes.EntityType.WBS, null, null, out entitiesFound);
                TreeInfo = EntitiesHelper.TreeFilterById(TreeInfo, entitiesId);
                entities = DataService.GetTreeEntitiesById(BuiltInCodes.EntityType.WBS, TreeInfo.Select(item => item.Id), false).ToList();
                
                //entities = _projectService.GetTreeEntitiesDeepById(BuiltInCodes.EntityType.WBS, entitiesId).ToList();
            }

            if (options.ResetCalulatedValues)
                ClearCalculatedValues();

            //Calcolo degli attributi dipendenti dall'esterno
            for (int i =0; i< entities.Count; i++)
            {
                WBSItem ent = entities[i] as WBSItem;
                    
                //if (ent.IsParent)
                //    continue;

                filterByDepth = filterByDepth.Take(ent.Depth).ToList();

                FilterData filterData = filterByDepth.LastOrDefault();
                if (filterData == null)
                    filterData = new FilterData();
                else
                    filterData = filterData.Clone();


                filterByDepth.Add(filterData);


                ClearCurrentEntityCalculatedValues();

                ///////////////////////////////
                /////servono?
                ////Questi metodi stanno molto tempo...
                //CalcolaComputoItemIds(ent, filterData);
                //CalcolaElementiItemIds(ent, filterData);
                /////////////////////////////////

                CalcolaAttributiGuidCollection(ent);
                CalcolaAttributiRiferimento(ent);

                CreateDateTimeCalculator(ent);
            }


            //Calcolo degli attributi delle foglie
            if (entitiesId == null)
            {
                //Ricalcolo di tutti gli attributi di tutte le entità
                for (int i = 0; i < allEntities.Count; i++)
                {
                    WBSItem ent = allEntities[i] as WBSItem;

                    if (ent.Depth == 0)
                        CalcolaEntityAttributiDeep(i, allEntities, options, true);
                }
            }
            else
            {
                //Ricalcolo di tutti gli attributi di tutte le entità in entities
                for (int i = 0; i < entities.Count; i++)
                {
                    WBSItem ent = entities[i] as WBSItem;
                    int allEntsIndex = allEntities.FindIndex(item => item.EntityId == ent.EntityId);
                    CalcolaEntityAttributiDeep(allEntsIndex, allEntities, options, true);
                }
            }

            //Ricalcolo degli attributi delle foglie in base ai predecessori
            WBSPredecessorCalculator predecessorCalc = new WBSPredecessorCalculator(_projectService);
            //predecessorCalc.Init(_timeCalculatorsByCalendario);
            predecessorCalc.Init();

            //elimino i poredecessori che linkano a entità inesistenti
            HashSet<Guid> invalidEntsId = new HashSet<Guid>();
            if (entitiesId != null)
                invalidEntsId = entitiesId.Where(x => !_projectService.IsEntityValid(entityTypeKey, x)).ToHashSet();
            predecessorCalc.RemovePredecessorsTo(invalidEntsId);

            predecessorCalc.Run(entities.Select(item => item.EntityId));
            EntityIdsChanged.UnionWith(predecessorCalc.EntityIdsChanged);


            //Calcolo degli attributi dei padri
            if (entitiesId == null)
                CalcolaEntityAttributiParents(null);
            else
                CalcolaEntityAttributiParents(EntityIdsChanged);

            //CalcolaInfoProgetto();
        }

        private void CalcolaInfoProgetto()
        {
            DateTime maxDataFine = DateTime.MinValue;
            List<TreeEntity> allEntities = _projectService.GetTreeEntitiesList(BuiltInCodes.EntityType.WBS);
            foreach (WBSItem wbsItem in allEntities)
            {
                DateTime datafine = wbsItem.GetDataFine().GetValueOrDefault();
                if (datafine > maxDataFine)
                    maxDataFine = datafine;
            }

            //data fine
            EntityType infoProgettoType = _projectService.GetEntityTypes()[BuiltInCodes.EntityType.InfoProgetto];
            infoProgettoType.Attributi[BuiltInCodes.Attributo.DataFine].ValoreDefault = new ValoreData() { V = maxDataFine };

            //data inizio
            GanttData ganttData = (_projectService as ProjectService).GetGanttData();
            DateTime minDataInizio = DateTime.MinValue;
            if (ganttData != null)
                minDataInizio = ganttData.DataInizio;

            infoProgettoType.Attributi[BuiltInCodes.Attributo.DataInizio].ValoreDefault = new ValoreData() { V = minDataInizio };
        }

        /// <summary>
        /// Calcola i padri di entitiesId
        /// </summary>
        /// <param name="entitiesId"></param>
        private void CalcolaEntityAttributiParents(IEnumerable<Guid> entitiesId)
        {
            List<TreeEntity> allEntities = _projectService.GetTreeEntitiesList(BuiltInCodes.EntityType.WBS);
            HashSet<Guid> calculated = new HashSet<Guid>();


            if (entitiesId == null)
            {
                //Ricalcolo di tutti gli attributi dei padri di tutte le entità
                for (int i = 0; i < allEntities.Count; i++)
                {
                    WBSItem ent = allEntities[i] as WBSItem;

                    if (ent.Depth == 0)
                    {

                        var calcOptions = new EntityCalcOptions();
                        CalcolaEntityAttributiDeep(i, allEntities, calcOptions, false);
                    }
                }
            }
            else
            {

                foreach (Guid id in entitiesId)
                {
                    if (calculated.Contains(id))
                        continue;

                    WBSItem wbsEnt = _projectService.GetEntityById(BuiltInCodes.EntityType.WBS, id) as WBSItem;
                    int wbsEntIndex = allEntities.FindIndex(item => item.EntityId == id);

                    int depth = wbsEnt.Depth;
                    for (int i = wbsEntIndex; i >= 0; i--)
                    {
                        if (depth <= 0)
                            break;

                        if (allEntities[i].Depth < depth)
                        {
                            CalcolaAttributiParent(i, allEntities);
                            depth = allEntities[i].Depth;
                            calculated.Add(allEntities[i].EntityId);
                        }
                    }
                }
            }

            EntityIdsChanged.UnionWith(calculated);
        }

        /// <summary>
        /// Calcola gli attributi dell'entià senza ricalcolare alcuna altra entità (serve per la CalcolaEntities())
        /// </summary>
        /// <param name="ent"></param>
        protected override void CalcolaAttributiLeaf(TreeEntity ent, EntityCalcOptions options)
        {
            WBSItem wbsItem = ent as WBSItem;

            if (options.CalcolaAttributiResults)
            {

                ClearCurrentEntityCalculatedValues();
                CalcolaAttributiGuidCollection(ent);
                CalcolaAttributiRiferimento(wbsItem);

                string wbsCalendarioIdCodice = string.Join(WBSItemType.AttributoCodiceSeparator, BuiltInCodes.EntityType.Calendari, BuiltInCodes.Attributo.Id);
                bool isNewEntity = false;

                if (wbsItem.GetAttributoGuidId(wbsCalendarioIdCodice) == Guid.Empty)
                {
                    string projectInfoCalendarioIdCodice = string.Join(InfoProgettoItemType.AttributoCodiceSeparator, BuiltInCodes.EntityType.Calendari, BuiltInCodes.Attributo.Id);
                    ValoreGuid valId = _entitiesHelper.GetValoreAttributo(BuiltInCodes.EntityType.InfoProgetto, projectInfoCalendarioIdCodice, false, false) as ValoreGuid;
                    wbsItem.Attributi[wbsCalendarioIdCodice].Valore = valId.Clone();
                    isNewEntity = true;
                }

                DateTimeCalculator timeCalc = CreateDateTimeCalculator(wbsItem);
                //DateTimeCalculator timeCalc = null;
                //Guid wbsCalendarioId = wbsItem.GetAttributoGuidId(wbsCalendarioIdCodice);
                //if (wbsCalendarioId != Guid.Empty)
                //{

                //    CalendariItem calendario = DataService.GetEntitiesById(BuiltInCodes.EntityType.Calendari, new List<Guid> { wbsCalendarioId }).FirstOrDefault() as CalendariItem;
                //    if (calendario == null)
                //        timeCalc = new DateTimeCalculator(WeekHours.Default, new CustomDays());
                //    else
                //        timeCalc = CreateDateTimeCalculator(wbsItem);
                //        //timeCalc = new DateTimeCalculator(calendario.GetWeekHours(), calendario.GetCustomDays());

                //}


                if (!timeCalc.IsValid())
                    return;

                //AttributoDependencyComparer attDepComparer = new AttributoDependencyComparer(_projectService, wbsItem);
                //IOrderedEnumerable<EntityAttributo> attsOrdered = wbsItem.Attributi.Values.OrderBy(item => item.AttributoCodice, attDepComparer);
                List<EntityAttributo> attsOrdered = OrderEntityAttributiByFunctions(wbsItem);

                foreach (EntityAttributo entAtt in attsOrdered)
                {
                    bool calcoloAttributoBuiltIn = false;
                    if (entAtt.AttributoCodice == BuiltInCodes.Attributo.DataInizio ||
                        entAtt.AttributoCodice == BuiltInCodes.Attributo.DataFine ||
                        entAtt.AttributoCodice == BuiltInCodes.Attributo.Lavoro ||
                        entAtt.AttributoCodice == BuiltInCodes.Attributo.Durata ||
                        entAtt.AttributoCodice == wbsCalendarioIdCodice ||
                        entAtt.AttributoCodice == BuiltInCodes.Attributo.Predecessor
                        )
                        calcoloAttributoBuiltIn = true;


                    if (calcoloAttributoBuiltIn)
                    {

                        DateTime? dataInizio = wbsItem.GetDataInizio();
                        DateTime? dataFine = wbsItem.GetDataFine();

                        if (isNewEntity)
                        {
                            //imposto data inizio a inizio giornata
                            if (dataInizio == null)
                            {
                                GanttData ganttData = ProjectService.GetGanttData();
                                dataInizio = ganttData.DataInizio;


                                if (timeCalc.IsWorkingMoment(dataInizio.Value))
                                    dataInizio = timeCalc.GetStartingDateTimeOfDay(ganttData.DataInizio);
                                else
                                    dataInizio = timeCalc.GetNextWorkingMoment(ganttData.DataInizio);

                                dataInizio = timeCalc.GetStartingDateTimeOfDay(dataInizio.Value);
                            }


                            //calcolo il lavoro e imposto fine giornata durata e durata calendario
                            _projectService.Calculator.Calculate(wbsItem, wbsItem.Attributi[BuiltInCodes.Attributo.Lavoro].Attributo, wbsItem.Attributi[BuiltInCodes.Attributo.Lavoro].Valore);
                            double? oreLavoro = (wbsItem.Attributi[BuiltInCodes.Attributo.Lavoro].Valore as ValoreReale).RealResult;

                            DateTime dataFineNew = timeCalc.AddWorkingMinutes(dataInizio.Value, oreLavoro.Value * 60);
                            if (oreLavoro > 0)
                                dataFineNew = timeCalc.AsEndingDateTime(dataFineNew);
                            double days = timeCalc.GetWorkingDaysBetween(dataInizio.Value, dataFineNew);
                            int daysCal = (int)(dataFineNew - dataInizio.Value).TotalDays + 1;

                            wbsItem.Attributi[BuiltInCodes.Attributo.DataInizio].Valore = new ValoreData() { V = dataInizio };
                            wbsItem.Attributi[BuiltInCodes.Attributo.Durata].Valore = new ValoreReale() { V = days.ToString() };
                            _projectService.Calculator.Calculate(wbsItem, wbsItem.Attributi[BuiltInCodes.Attributo.Durata].Attributo, wbsItem.Attributi[BuiltInCodes.Attributo.Durata].Valore);
                            wbsItem.Attributi[BuiltInCodes.Attributo.DataFine].Valore = new ValoreData() { V = dataFineNew };
                            wbsItem.Attributi[BuiltInCodes.Attributo.DurataCalendario].Valore = new ValoreReale() { V = daysCal.ToString() };
                            _projectService.Calculator.Calculate(wbsItem, wbsItem.Attributi[BuiltInCodes.Attributo.DurataCalendario].Attributo, wbsItem.Attributi[BuiltInCodes.Attributo.DurataCalendario].Valore);

                        }
                        else
                        {
                            //prendo per buone ore lavoro e data inizio

                            if (!dataInizio.HasValue)
                                return;

                            _projectService.Calculator.Calculate(wbsItem, wbsItem.Attributi[BuiltInCodes.Attributo.Lavoro].Attributo, wbsItem.Attributi[BuiltInCodes.Attributo.Lavoro].Valore);
                            double? oreLavoro = (wbsItem.Attributi[BuiltInCodes.Attributo.Lavoro].Valore as ValoreReale).RealResult;

                            if (!oreLavoro.HasValue)
                                return;

                            DateTime dataInizioNew = dataInizio.Value;
                            if (!timeCalc.IsWorkingMoment(dataInizio.Value))
                                dataInizioNew = timeCalc.GetNextWorkingMoment(dataInizio.Value);//va ricalcolata la data inizio perchè potrebbe essere diventata un momento di ferie

                            DateTime dataFineNew = timeCalc.AddWorkingMinutes(dataInizioNew, oreLavoro.Value * 60);
                            if (oreLavoro != 0)
                                dataFineNew = timeCalc.AsEndingDateTime(dataFineNew);
                            double days = timeCalc.GetWorkingDaysBetween(dataInizioNew, dataFineNew);
                            int daysCal = (int)(dataFineNew - dataInizioNew).TotalDays + 1;

                            wbsItem.Attributi[BuiltInCodes.Attributo.DataInizio].Valore = new ValoreData() { V = dataInizioNew };
                            wbsItem.Attributi[BuiltInCodes.Attributo.Durata].Valore = new ValoreReale() { V = days.ToString() };
                            _projectService.Calculator.Calculate(wbsItem, wbsItem.Attributi[BuiltInCodes.Attributo.Durata].Attributo, wbsItem.Attributi[BuiltInCodes.Attributo.Durata].Valore);
                            wbsItem.Attributi[BuiltInCodes.Attributo.DataFine].Valore = new ValoreData() { V = dataFineNew };
                            wbsItem.Attributi[BuiltInCodes.Attributo.DurataCalendario].Valore = new ValoreReale() { V = daysCal.ToString() };
                            _projectService.Calculator.Calculate(wbsItem, wbsItem.Attributi[BuiltInCodes.Attributo.DurataCalendario].Attributo, wbsItem.Attributi[BuiltInCodes.Attributo.DurataCalendario].Valore);


                        }
                    }
                    else
                    {
                        _projectService.Calculator.Calculate(wbsItem, entAtt.Attributo, entAtt.Valore);

                        if (_calculatorFunction != null)
                        {
                            string et = entAtt.Attributo.Etichetta;
                            _calculatorFunction.AddCalculatedValue(entAtt.Entity.EntityId, et, entAtt.Valore);
                        }
                    }
                }
                /////////////////////////////
                ////Calcolo gli altri attributi

                //AttributoDependencyComparer attDepComparer = new AttributoDependencyComparer(_projectService, wbsItem);
                //IOrderedEnumerable<EntityAttributo> attsOrdered = wbsItem.Attributi.Values.OrderBy(item => item.AttributoCodice, attDepComparer);

                //foreach (EntityAttributo entAtt in attsOrdered)
                //{
                //    if (entAtt.AttributoCodice == BuiltInCodes.Attributo.DataInizio)
                //        continue;

                //    if (entAtt.AttributoCodice == BuiltInCodes.Attributo.DataFine)
                //        continue;

                //    if (entAtt.AttributoCodice == BuiltInCodes.Attributo.Lavoro)
                //        continue;

                //    if (entAtt.AttributoCodice == BuiltInCodes.Attributo.Durata)
                //        continue;

                //    if (entAtt.AttributoCodice == wbsCalendarioIdCodice)
                //        continue;

                //    if (entAtt.AttributoCodice == BuiltInCodes.Attributo.Predecessor)
                //        continue;


                //    bool isCalculated = CalcolaAttributiBuiltIn(wbsItem, entAtt);
                //    if (!isCalculated)
                //        _projectService.Calculator.Calculate(wbsItem, entAtt.Attributo, entAtt.Valore);

                //    if (_calculatorFunction != null)
                //    {
                //        string et = entAtt.Attributo.Etichetta;
                //        _calculatorFunction.AddCalculatedValue(et, entAtt.Valore);
                //    }
                //}
                ////////////////////////////////////////////////////////


            }
            //oss: deve essere eseguita prima dell'aggiornamento dei padri
            UpdateHighlighterColorName(wbsItem);


        }
        public override bool CalcolaAttributiValues(Entity ent)
        {
            return CalcolaAttributiValues(ent, null);
        }

        /// <summary>
        /// Ricalcola gli attributi di ent e le entità dipendenti (predecessori, padri)
        /// </summary>
        /// <param name="ent"></param>
        /// <param name="timeCalc"></param>
        /// <returns></returns>
        private bool CalcolaAttributiValues(Entity ent, DateTimeCalculator timeCalc)
        {
            //NB: Questo metodo non chiama CalcolaAttributiBuiltIn

            CalcolaDescendantsFilterItemIds(ent);


            WBSItem wbsItem = ent as WBSItem;
            if (wbsItem == null)
                return false;

            string wbsCalendarioIdCodice = string.Join(WBSItemType.AttributoCodiceSeparator, BuiltInCodes.EntityType.Calendari, BuiltInCodes.Attributo.Id);
            List<TreeEntity> allTreeEntities = DataService.GetTreeEntitiesList(BuiltInCodes.EntityType.WBS);

            if (wbsItem.IsParent)
            {
                //ricalcolo gli WBSItems che dipendono da  wbsItem mediante predecessori
                //Ho per esempio modificato il filtro del padre
                
                WBSPredecessorCalculator predecessorCalculator = new WBSPredecessorCalculator(_projectService);
                predecessorCalculator.Init();
                predecessorCalculator.Run(wbsItem);

                TreeEntity parent = wbsItem;
                while (parent != null)
                {
                    int parentIndex = allTreeEntities.IndexOf(parent);
                    CalcolaAttributiParent(parentIndex, allTreeEntities);
                    parent = parent.Parent;
                }
            }
            else
            {
                bool isNewEntity = false;

                //if (ent.GetAttributoGuidId(wbsCalendarioIdCodice) == Guid.Empty)
                if (wbsItem.GetGiorniDurata() == null)//per controllare se sto inserendo una nuova entità
                {
                    string projectInfoCalendarioIdCodice = string.Join(InfoProgettoItemType.AttributoCodiceSeparator, BuiltInCodes.EntityType.Calendari, BuiltInCodes.Attributo.Id);
                    ValoreGuid valId = _entitiesHelper.GetValoreAttributo(BuiltInCodes.EntityType.InfoProgetto, projectInfoCalendarioIdCodice, false, false) as ValoreGuid;
                    ent.Attributi[wbsCalendarioIdCodice].Valore = valId.Clone();
                    isNewEntity = true;
                }


                if (timeCalc == null)
                    timeCalc = CreateDateTimeCalculator(wbsItem);

                //Guid wbsCalendarioId = ent.GetAttributoGuidId(wbsCalendarioIdCodice);
                if (timeCalc != null)
                {
                    DateTime? dataInizio = wbsItem.GetDataInizio();
                    DateTime? dataFine = wbsItem.GetDataFine();

                    if (isNewEntity)
                    {

                        if (timeCalc.IsValid())
                        {
                            //imposto data inizio a inizio giornata
                            if (dataInizio == null)
                            {
                                GanttData ganttData = ProjectService.GetGanttData();
                                dataInizio = ganttData.DataInizio;


                                if (timeCalc.IsWorkingMoment(dataInizio.Value))
                                    dataInizio = timeCalc.GetStartingDateTimeOfDay(ganttData.DataInizio);
                                else
                                    dataInizio = timeCalc.GetNextWorkingMoment(ganttData.DataInizio);

                                dataInizio = timeCalc.GetStartingDateTimeOfDay(dataInizio.Value);
                            }


                            //calcolo il lavoro e imposto fine giornata durata e durata calendario
                            _projectService.Calculator.Calculate(ent, wbsItem.Attributi[BuiltInCodes.Attributo.Lavoro].Attributo, wbsItem.Attributi[BuiltInCodes.Attributo.Lavoro].Valore);
                            double? oreLavoro = (wbsItem.Attributi[BuiltInCodes.Attributo.Lavoro].Valore as ValoreReale).RealResult;

                            DateTime dataFineNew = timeCalc.AddWorkingMinutes(dataInizio.Value, oreLavoro.Value * 60);
                            if (oreLavoro != 0)
                                dataFineNew = timeCalc.AsEndingDateTime(dataFineNew);
                            double days = timeCalc.GetWorkingDaysBetween(dataInizio.Value, dataFineNew);
                            int daysCal = (int)(dataFineNew - dataInizio.Value).TotalDays + 1;

                            wbsItem.Attributi[BuiltInCodes.Attributo.DataInizio].Valore = new ValoreData() { V = dataInizio };
                            wbsItem.Attributi[BuiltInCodes.Attributo.Durata].Valore = new ValoreReale() { V = days.ToString() };
                            _projectService.Calculator.Calculate(ent, wbsItem.Attributi[BuiltInCodes.Attributo.Durata].Attributo, wbsItem.Attributi[BuiltInCodes.Attributo.Durata].Valore);
                            wbsItem.Attributi[BuiltInCodes.Attributo.DataFine].Valore = new ValoreData() { V = dataFineNew };
                            wbsItem.Attributi[BuiltInCodes.Attributo.DurataCalendario].Valore = new ValoreReale() { V = daysCal.ToString() };
                            _projectService.Calculator.Calculate(ent, wbsItem.Attributi[BuiltInCodes.Attributo.DurataCalendario].Attributo, wbsItem.Attributi[BuiltInCodes.Attributo.DurataCalendario].Valore);

                            CalculateOthersAttributo(ent, wbsCalendarioIdCodice);
                            
                            //oss: deve essere eseguita prima dell'aggiornamento dei padri
                            UpdateHighlighterColorName(wbsItem);

                            //Aggiornamento dei padri
                            TreeEntity parent = wbsItem.Parent;
                            while (parent != null)
                            {
                                int parentIndex = allTreeEntities.IndexOf(parent);
                                CalcolaAttributiParent(parentIndex, allTreeEntities);
                                EntityIdsChanged.Add(parent.EntityId);
                                parent = parent.Parent;
                            }
                        }

                    }
                    else
                    {
                        //calcolo data fine partendo da data inizio e lavoro, ricalcolo durata, durata calendario

                        //calcolo gli attributi che servono per calcolare il lavoro
                        CalculateOthersAttributo(ent, wbsCalendarioIdCodice, null, BuiltInCodes.Attributo.Lavoro);

                        _projectService.Calculator.Calculate(ent, wbsItem.Attributi[BuiltInCodes.Attributo.Lavoro].Attributo, wbsItem.Attributi[BuiltInCodes.Attributo.Lavoro].Valore);
                        double? oreLavoro = (wbsItem.Attributi[BuiltInCodes.Attributo.Lavoro].Valore as ValoreReale).RealResult;
                        if (timeCalc.IsValid() && dataInizio.HasValue && oreLavoro.HasValue)
                        {
                            DateTime dataInizioNew = dataInizio.Value;
                            if (!timeCalc.IsWorkingMoment(dataInizio.Value))
                                dataInizioNew = timeCalc.GetNextWorkingMoment(dataInizio.Value);//va ricalcolata la data inizio perchè potrebbe essere diventata un momento di ferie

                            DateTime dataFineNew = timeCalc.AddWorkingMinutes(dataInizioNew, oreLavoro.Value * 60);
                            if (oreLavoro != 0)
                                dataFineNew = timeCalc.AsEndingDateTime(dataFineNew);
                            double days = timeCalc.GetWorkingDaysBetween(dataInizioNew, dataFineNew);
                            int daysCal = (int)(dataFineNew - dataInizioNew).TotalDays + 1;

                            wbsItem.Attributi[BuiltInCodes.Attributo.DataInizio].Valore = new ValoreData() { V = dataInizioNew };
                            wbsItem.Attributi[BuiltInCodes.Attributo.Durata].Valore = new ValoreReale() { V = days.ToString() };
                            _projectService.Calculator.Calculate(ent, wbsItem.Attributi[BuiltInCodes.Attributo.Durata].Attributo, wbsItem.Attributi[BuiltInCodes.Attributo.Durata].Valore);
                            wbsItem.Attributi[BuiltInCodes.Attributo.DataFine].Valore = new ValoreData() { V = dataFineNew };
                            wbsItem.Attributi[BuiltInCodes.Attributo.DurataCalendario].Valore = new ValoreReale() { V = daysCal.ToString() };
                            _projectService.Calculator.Calculate(ent, wbsItem.Attributi[BuiltInCodes.Attributo.DurataCalendario].Attributo, wbsItem.Attributi[BuiltInCodes.Attributo.DurataCalendario].Valore);

                            //ricalcolo wbsItem in base ai suoi predecessori e gli WBSItems che dipendono da  wbsItem mediante predecessori
                            WBSPredecessorCalculator predecessorCalculator = new WBSPredecessorCalculator(_projectService);
                            predecessorCalculator.Init();
                            predecessorCalculator.ValidateItem(wbsItem);
                            predecessorCalculator.Run(wbsItem);

                            //calcolo gli attributi non built-in dopo il lavoro
                            CalculateOthersAttributo(ent, wbsCalendarioIdCodice, BuiltInCodes.Attributo.Lavoro);

                            //oss: deve essere eseguita prima dell'aggiornamento dei padri
                            UpdateHighlighterColorName(wbsItem);


                            //Aggiornamento di padri dell'item originariamente modificato
                            TreeEntity parent = wbsItem.Parent;
                            while (parent != null)
                            {
                                int parentIndex = allTreeEntities.IndexOf(parent);
                                CalcolaAttributiParent(parentIndex, allTreeEntities);
                                predecessorCalculator.Run(parent as WBSItem);
                                EntityIdsChanged.Add(parent.EntityId);

                                parent = parent.Parent;
                            }

                            EntityIdsChanged.UnionWith(predecessorCalculator.EntityIdsChanged);

                            //Aggiornamento dei padri di tutti gli WBSItesm modificati dal calcolo dei predecessori
                            HashSet<Guid> entityIdsChanged = new HashSet<Guid>(EntityIdsChanged);
                            foreach (Guid wbsItemByPredId in entityIdsChanged)
                            {
                                WBSItem wbsItemChanged = DataService.GetEntityById(BuiltInCodes.EntityType.WBS, wbsItemByPredId) as WBSItem;

                                parent = wbsItemChanged.Parent;
                                while (parent != null)
                                {
                                    int parentIndex = allTreeEntities.IndexOf(parent);
                                    CalcolaAttributiParent(parentIndex, allTreeEntities);
                                    EntityIdsChanged.Add(parent.EntityId);

                                    parent = parent.Parent;
                                }
                            }
                        }
                    }
                }
            }

            return true;
        }

        private void CalculateOthersAttributo(Entity ent, string wbsCalendarioIdCodice, string startAttCodice=null, string endAttCodice=null)
        {
            /////////////////////////////
            ////Calcolo gli altri attributi
            ///
            //parte calcolando da startAttCodice escluso e finisce con endAttCodice escluso

            List<EntityAttributo> attsOrdered = OrderEntityAttributiByFunctions(ent);

            foreach (EntityAttributo entAtt in attsOrdered)
            {
                if (entAtt.AttributoCodice == endAttCodice)
                    break;

                if (entAtt.AttributoCodice != startAttCodice && startAttCodice != null)
                    continue;

                if (entAtt.AttributoCodice == BuiltInCodes.Attributo.DataInizio)
                    continue;

                if (entAtt.AttributoCodice == BuiltInCodes.Attributo.DataFine)
                    continue;

                if (entAtt.AttributoCodice == BuiltInCodes.Attributo.Lavoro)
                    continue;

                if (entAtt.AttributoCodice == BuiltInCodes.Attributo.Durata)
                    continue;

                if (entAtt.AttributoCodice == wbsCalendarioIdCodice)
                    continue;

                if (entAtt.AttributoCodice == BuiltInCodes.Attributo.Predecessor)
                    continue;


                bool isCalculated = CalcolaAttributiBuiltIn(ent, entAtt);
                if (!isCalculated)
                    _projectService.Calculator.Calculate(ent, entAtt.Attributo, entAtt.Valore);

                if (_calculatorFunction != null)
                {
                    string et = entAtt.Attributo.Etichetta;
                    _calculatorFunction.AddCalculatedValue(entAtt.Entity.EntityId, et, entAtt.Valore);
                }



            }
        }

        private DateTimeCalculator CreateDateTimeCalculator(WBSItem ent)
        {
            DateTimeCalculator timeCalc = null;
                
            string wbsCalendarioIdCodice = string.Join(WBSItemType.AttributoCodiceSeparator, BuiltInCodes.EntityType.Calendari, BuiltInCodes.Attributo.Id);

            Guid wbsCalendarioId = ent.GetAttributoGuidId(wbsCalendarioIdCodice);

            if (!_timeCalculatorsByCalendario.TryGetValue(wbsCalendarioId, out timeCalc))
            {
                if (wbsCalendarioId != Guid.Empty)
                {
                    CalendariItem calendario = DataService.GetEntityById(BuiltInCodes.EntityType.Calendari, wbsCalendarioId) as CalendariItem;
                    if (calendario == null)
                        return new DateTimeCalculator(WeekHours.Default, new CustomDays());

                    timeCalc = new DateTimeCalculator(calendario.GetWeekHours(), calendario.GetCustomDays());
                    _timeCalculatorsByCalendario.Add(wbsCalendarioId, timeCalc);
                }
                else
                {
                    return new DateTimeCalculator(WeekHours.Default, new CustomDays());
                }

            }
            
            return timeCalc;
        }

        public override bool CalcolaAttributiValues(Entity ent, EntityAttributo attNewValore, Queue<string> functions, List<EntityAttributo> attsRifByFunction = null)
        {
            try
            {

                bool isCalculated = CalcolaAttributiBuiltIn(ent, attNewValore);
                if (!isCalculated)
                {
                    _projectService.Calculator.Calculate(ent, attNewValore.Attributo, attNewValore.Valore);
                }

                CalcolaAttributiRifByFunction(ent, functions, attsRifByFunction);

                functions.Dequeue();


            }
            catch (Exception e)
            {
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), e.Message);
            }

            return true;
        }

        protected override bool CalcolaAttributiBuiltIn(Entity ent, EntityAttributo attNewValore)
        {

            if (attNewValore.AttributoCodice == BuiltInCodes.Attributo.AttributoFilterText)
                return true;

            string wbsCalendarioIdCodice = string.Join(WBSItemType.AttributoCodiceSeparator, BuiltInCodes.EntityType.Calendari, BuiltInCodes.Attributo.Id);

            if (attNewValore.AttributoCodice == BuiltInCodes.Attributo.DataInizio ||
                attNewValore.AttributoCodice == BuiltInCodes.Attributo.Lavoro ||
                attNewValore.AttributoCodice == BuiltInCodes.Attributo.DataFine ||
                attNewValore.AttributoCodice == BuiltInCodes.Attributo.Durata ||
                attNewValore.AttributoCodice == wbsCalendarioIdCodice ||
                attNewValore.AttributoCodice == BuiltInCodes.Attributo.Predecessor)
            {

                WBSItem wbsItem = ent as WBSItem;
                if (wbsItem == null)
                    return false;


                //CalendariItem calendario = DataService.GetEntitiesById(BuiltInCodes.EntityType.Calendari, new List<Guid> { wbsCalendarioId }).FirstOrDefault() as CalendariItem;
                //if (calendario == null)
                //    return false;

                //DateTimeCalculator timeCalc = new DateTimeCalculator(calendario.GetWeekHours(), calendario.GetCustomDays());
                DateTimeCalculator timeCalc = CreateDateTimeCalculator(wbsItem);

                if (attNewValore.AttributoCodice == BuiltInCodes.Attributo.DataInizio)
                {
                    //ricalcolo data fine,inizio,durata,durata calendario
                    DateTime? dataInizio = (attNewValore.Valore as ValoreData).V;
                    //_projectService.Calculator.Calculate(ent, wbsItem.Attributi[BuiltInCodes.Attributo.Lavoro].Attributo, wbsItem.Attributi[BuiltInCodes.Attributo.Lavoro].Valore);
                    double? oreLavoro = wbsItem.GetOreLavoro();
                    if (timeCalc.IsValid() && dataInizio.HasValue && oreLavoro.HasValue)
                    {
                        DateTime dataInizioNew = dataInizio.Value;
                        //if (dataInizio.Value.Date != oldDataInizio.Value.Date)
                        //    dataInizioNew = timeCalc.GetStartingDateTimeOfDate(dataInizio.Value);
                        DateTime dataFineNew = timeCalc.AddWorkingMinutes(dataInizioNew, oreLavoro.Value * 60);
                        if (oreLavoro != 0)
                            dataFineNew = timeCalc.AsEndingDateTime(dataFineNew);
                        double days = timeCalc.GetWorkingDaysBetween(dataInizioNew, dataFineNew);
                        int daysCal = (int)(dataFineNew.Date - dataInizioNew.Date).TotalDays + 1;


                        wbsItem.Attributi[BuiltInCodes.Attributo.DataInizio].Valore = new ValoreData() { V = dataInizioNew };
                        wbsItem.Attributi[BuiltInCodes.Attributo.DataFine].Valore = new ValoreData() { V = dataFineNew };
                        wbsItem.Attributi[BuiltInCodes.Attributo.Durata].Valore = new ValoreReale() { V = days.ToString() };
                        _projectService.Calculator.Calculate(ent, wbsItem.Attributi[BuiltInCodes.Attributo.Durata].Attributo, wbsItem.Attributi[BuiltInCodes.Attributo.Durata].Valore);
                        wbsItem.Attributi[BuiltInCodes.Attributo.DurataCalendario].Valore = new ValoreReale() { V = daysCal.ToString() };
                        _projectService.Calculator.Calculate(ent, wbsItem.Attributi[BuiltInCodes.Attributo.DurataCalendario].Attributo, wbsItem.Attributi[BuiltInCodes.Attributo.DurataCalendario].Valore);

                    }
                }
                else if (attNewValore.AttributoCodice == BuiltInCodes.Attributo.Lavoro)
                {
                    //ricalcolo data fine,durata
                    DateTime? dataInizio = wbsItem.GetDataInizio();

                    _projectService.Calculator.Calculate(ent, attNewValore.Attributo, attNewValore.Valore);
                    double? oreLavoro = (attNewValore.Valore as ValoreReale).RealResult;
                    if (timeCalc.IsValid() && dataInizio.HasValue && oreLavoro.HasValue)
                    {
                        DateTime dataFineNew = timeCalc.AddWorkingMinutes(dataInizio.Value, oreLavoro.Value * 60);
                        if (oreLavoro != 0)
                            dataFineNew = timeCalc.AsEndingDateTime(dataFineNew);
                        double days = timeCalc.GetWorkingDaysBetween(dataInizio.Value, dataFineNew);
                        int daysCal = (int)(dataFineNew - dataInizio.Value).TotalDays + 1;


                        wbsItem.Attributi[BuiltInCodes.Attributo.DataFine].Valore = new ValoreData() { V = dataFineNew };
                        wbsItem.Attributi[BuiltInCodes.Attributo.Durata].Valore = new ValoreReale() { V = days.ToString() };
                        _projectService.Calculator.Calculate(ent, wbsItem.Attributi[BuiltInCodes.Attributo.Durata].Attributo, wbsItem.Attributi[BuiltInCodes.Attributo.Durata].Valore);
                        wbsItem.Attributi[BuiltInCodes.Attributo.DurataCalendario].Valore = new ValoreReale() { V = daysCal.ToString() };
                        _projectService.Calculator.Calculate(ent, wbsItem.Attributi[BuiltInCodes.Attributo.DurataCalendario].Attributo, wbsItem.Attributi[BuiltInCodes.Attributo.DurataCalendario].Valore);

                    }

                }
                else if (attNewValore.AttributoCodice == BuiltInCodes.Attributo.DataFine)
                {
                    //prendo per buona la data di inizio e ricalcolo il lavoro, durata, durata calendario
                    DateTime? dataFine = (attNewValore.Valore as ValoreData).V;
                    DateTime? dataInizio = wbsItem.GetDataInizio();
                    if (timeCalc.IsValid() && dataFine.HasValue && dataInizio.HasValue)
                    {
                        DateTime dataFineNew = dataFine.Value;
                        double oreLavoro = timeCalc.GetWorkingMinutesBetween(dataInizio.Value, dataFine.Value) / 60.0;
                        double days = timeCalc.GetWorkingDaysBetween(dataInizio.Value, dataFine.Value);
                        int daysCal = (int)(dataFine.Value - dataInizio.Value).TotalDays + 1;

                        wbsItem.Attributi[BuiltInCodes.Attributo.Lavoro].Valore = new ValoreReale() { V = oreLavoro.ToString() };
                        _projectService.Calculator.Calculate(ent, wbsItem.Attributi[BuiltInCodes.Attributo.Lavoro].Attributo, wbsItem.Attributi[BuiltInCodes.Attributo.Lavoro].Valore);
                        wbsItem.Attributi[BuiltInCodes.Attributo.Durata].Valore = new ValoreReale() { V = days.ToString() };
                        _projectService.Calculator.Calculate(ent, wbsItem.Attributi[BuiltInCodes.Attributo.Durata].Attributo, wbsItem.Attributi[BuiltInCodes.Attributo.Durata].Valore);
                        wbsItem.Attributi[BuiltInCodes.Attributo.DurataCalendario].Valore = new ValoreReale() { V = daysCal.ToString() };
                        _projectService.Calculator.Calculate(ent, wbsItem.Attributi[BuiltInCodes.Attributo.DurataCalendario].Attributo, wbsItem.Attributi[BuiltInCodes.Attributo.DurataCalendario].Valore);
                    }
                }
                else if (attNewValore.AttributoCodice == BuiltInCodes.Attributo.Durata)
                {
                    //ricalcolo data fine,lavoro
                    DateTime? dataInizio = wbsItem.GetDataInizio();

                    _projectService.Calculator.Calculate(ent, attNewValore.Attributo, attNewValore.Valore);
                    double? days = (attNewValore.Valore as ValoreReale).RealResult;
                    if (timeCalc.IsValid() && dataInizio.HasValue && days.HasValue)
                    {
                        DateTime dataFineNew = timeCalc.AddWorkingDays(dataInizio.Value, days.Value);
                        if (days != 0)
                            dataFineNew = timeCalc.AsEndingDateTime(dataFineNew);
                        double ore = timeCalc.GetWorkingMinutesBetween(dataInizio.Value, dataFineNew) / 60;
                        int daysCal = (int)(dataFineNew - dataInizio.Value).TotalDays + 1;

                        wbsItem.Attributi[BuiltInCodes.Attributo.DataFine].Valore = new ValoreData() { V = dataFineNew };
                        wbsItem.Attributi[BuiltInCodes.Attributo.Lavoro].Valore = new ValoreReale() { V = ore.ToString() };
                        _projectService.Calculator.Calculate(ent, wbsItem.Attributi[BuiltInCodes.Attributo.Lavoro].Attributo, wbsItem.Attributi[BuiltInCodes.Attributo.Lavoro].Valore);
                        wbsItem.Attributi[BuiltInCodes.Attributo.DurataCalendario].Valore = new ValoreReale() { V = daysCal.ToString() };
                        _projectService.Calculator.Calculate(ent, wbsItem.Attributi[BuiltInCodes.Attributo.DurataCalendario].Attributo, wbsItem.Attributi[BuiltInCodes.Attributo.DurataCalendario].Valore);

                    }

                }
                else if (attNewValore.AttributoCodice == wbsCalendarioIdCodice)
                {
                    //calcolo data fine partendo da data inizio e lavoro, ricalcolo durata, durata calendario
                    if (timeCalc.IsValid())
                    {
                        DateTime? dataInizio = wbsItem.GetDataInizio();
                        if (timeCalc.IsWorkingMoment(dataInizio.Value))
                            dataInizio = timeCalc.GetStartingDateTimeOfDay(dataInizio.Value);
                        else
                            dataInizio = timeCalc.GetNextWorkingMoment(dataInizio.Value);
                        double? oreLavoro = (wbsItem.Attributi[BuiltInCodes.Attributo.Lavoro].Valore as ValoreReale).RealResult;
                        if (oreLavoro == null)
                            oreLavoro = timeCalc.GetWorkingMinutesPerDay(dataInizio.Value) / 60.0;

                        if (dataInizio.HasValue && oreLavoro.HasValue)
                        {
                            DateTime dataFineNew = timeCalc.AddWorkingMinutes(dataInizio.Value, oreLavoro.Value * 60);
                            if (oreLavoro != 0)
                                dataFineNew = timeCalc.AsEndingDateTime(dataFineNew);
                            double days = timeCalc.GetWorkingDaysBetween(dataInizio.Value, dataFineNew);
                            int daysCal = (int)(dataFineNew - dataInizio.Value).TotalDays + 1;

                            wbsItem.Attributi[BuiltInCodes.Attributo.DataInizio].Valore = new ValoreData() { V = dataInizio };
                            wbsItem.Attributi[BuiltInCodes.Attributo.Lavoro].Valore = new ValoreReale() { V = oreLavoro.ToString() };
                            _projectService.Calculator.Calculate(ent, wbsItem.Attributi[BuiltInCodes.Attributo.Lavoro].Attributo, wbsItem.Attributi[BuiltInCodes.Attributo.Lavoro].Valore);
                            wbsItem.Attributi[BuiltInCodes.Attributo.Durata].Valore = new ValoreReale() { V = days.ToString() };
                            _projectService.Calculator.Calculate(ent, wbsItem.Attributi[BuiltInCodes.Attributo.Durata].Attributo, wbsItem.Attributi[BuiltInCodes.Attributo.Durata].Valore);
                            wbsItem.Attributi[BuiltInCodes.Attributo.DataFine].Valore = new ValoreData() { V = dataFineNew };
                            wbsItem.Attributi[BuiltInCodes.Attributo.DurataCalendario].Valore = new ValoreReale() { V = daysCal.ToString() };
                            _projectService.Calculator.Calculate(ent, wbsItem.Attributi[BuiltInCodes.Attributo.DurataCalendario].Attributo, wbsItem.Attributi[BuiltInCodes.Attributo.DurataCalendario].Valore);

                        }
                    }


                }

                EntityIdsChanged.Add(ent.EntityId);

                if (attNewValore.AttributoCodice == BuiltInCodes.Attributo.Predecessor)
                {

                    WBSPredecessors preds = null;

                    string json = ent.Attributi[BuiltInCodes.Attributo.Predecessor].Valore.PlainText;
                    if (!JsonSerializer.JsonDeserialize(json, out preds, typeof(WBSPredecessors)))
                        preds = new WBSPredecessors();

                    WBSPredecessor lastPred = preds.Items.LastOrDefault();
                    if (lastPred != null)
                    {
                        //ricalcolo gli WBSItems che dipendono da  lastPred.
                        WBSPredecessorCalculator predecessorCalculator = new WBSPredecessorCalculator(_projectService);
                        predecessorCalculator.Init();
                        predecessorCalculator.Run(lastPred.WBSItemId);
                        EntityIdsChanged.UnionWith(predecessorCalculator.EntityIdsChanged);
                    }
                }
                else
                {

                    //ricalcolo gli WBSItems che dipendono da  wbsItem
                    WBSPredecessorCalculator predecessorCalculator = new WBSPredecessorCalculator(_projectService);
                    predecessorCalculator.Init();
                    predecessorCalculator.Run(wbsItem);
                    EntityIdsChanged.UnionWith(predecessorCalculator.EntityIdsChanged);

                }




                //Aggiornamento dei padri di tutti gli WBSItesm modificati dal calcolo dei predecessori
                List<TreeEntity> allTreeEntities = _projectService.GetTreeEntitiesList(BuiltInCodes.EntityType.WBS);
                HashSet<Guid> entityIdsChangedLocal = new HashSet<Guid>(EntityIdsChanged);
                foreach (Guid wbsItemByPredId in entityIdsChangedLocal)
                {
                    WBSItem wbsItemChanged = _projectService.GetEntityById(BuiltInCodes.EntityType.WBS, wbsItemByPredId) as WBSItem;

                    WBSItem parent = wbsItemChanged.Parent as WBSItem;
                    while (parent != null)
                    {
                        int parentIndex = allTreeEntities.IndexOf(parent);
                        CalcolaAttributiParent(parentIndex, allTreeEntities);
                        EntityIdsChanged.Add(parent.EntityId);

                        parent = parent.Parent as WBSItem;
                    }
                }


            }


            return base.CalcolaAttributiBuiltIn(ent, attNewValore);
        }

       

        protected override void CalcolaAttributiParent(int parentIndex, List<TreeEntity> allEntities)
        {


            if (parentIndex < 0)
                return;

            ClearCurrentEntityCalculatedValues();

            WBSItem parentItem = allEntities[parentIndex] as WBSItem;
            int parentDepth = parentItem.Depth;
            double lavoro = 0;
            double durata = 0;
            double durataCalendario = 0;
            DateTime dataInizio = DateTime.MaxValue;
            DateTime dataFine = DateTime.MinValue;
            string highlighterColorName = null;

          


            for (int i = parentIndex + 1; i < allEntities.Count; i++)
            {
                WBSItem item = allEntities[i] as WBSItem;

                if (item.Depth <= parentDepth)
                    break;

                if (item.Depth > parentDepth + 1)
                    continue;

                double? lav = item.GetOreLavoro();
                if (lav.HasValue)
                    lavoro += lav.Value;

                double? d = item.GetGiorniDurata();
                if (d.HasValue)
                    durata += d.Value;

                DateTime? inizio = item.GetDataInizio();
                if (inizio.HasValue && inizio < dataInizio)
                    dataInizio = inizio.Value;

                DateTime? fine = item.GetDataFine();
                if (fine.HasValue && fine > dataFine)
                    dataFine = fine.Value;


                string itemHighlighterColName = item.HighlighterColorName;
                if (highlighterColorName == null)
                    highlighterColorName = itemHighlighterColName;
                else if (itemHighlighterColName != highlighterColorName)
                    highlighterColorName = MyColorsEnum.Transparent.ToString();
            }

            durataCalendario = ((int) (dataFine - dataInizio).TotalDays) + 1;

            parentItem.Attributi[BuiltInCodes.Attributo.Lavoro].Valore = new ValoreReale() { V = lavoro.ToString() };
            _projectService.Calculator.Calculate(parentItem, parentItem.Attributi[BuiltInCodes.Attributo.Lavoro].Attributo, parentItem.Attributi[BuiltInCodes.Attributo.Lavoro].Valore);
            parentItem.Attributi[BuiltInCodes.Attributo.Durata].Valore = new ValoreReale() { V = durata.ToString() };
            _projectService.Calculator.Calculate(parentItem, parentItem.Attributi[BuiltInCodes.Attributo.Durata].Attributo, parentItem.Attributi[BuiltInCodes.Attributo.Durata].Valore);
            parentItem.Attributi[BuiltInCodes.Attributo.DataInizio].Valore = new ValoreData() { V = dataInizio };
            parentItem.Attributi[BuiltInCodes.Attributo.DataFine].Valore = new ValoreData() { V = dataFine };
            parentItem.Attributi[BuiltInCodes.Attributo.DurataCalendario].Valore = new ValoreReale() { V = durataCalendario.ToString() };
            _projectService.Calculator.Calculate(parentItem, parentItem.Attributi[BuiltInCodes.Attributo.DurataCalendario].Attributo, parentItem.Attributi[BuiltInCodes.Attributo.DurataCalendario].Valore);
            parentItem.HighlighterColorName = highlighterColorName;
            _projectService.Calculator.Calculate(parentItem, parentItem.Attributi[BuiltInCodes.Attributo.Codice].Attributo, parentItem.Attributi[BuiltInCodes.Attributo.Codice].Valore);
            _projectService.Calculator.Calculate(parentItem, parentItem.Attributi[BuiltInCodes.Attributo.Nome].Attributo, parentItem.Attributi[BuiltInCodes.Attributo.Nome].Valore);

        }

        private void CalcolaDescendantsFilterItemIds(Entity ent)
        {

            ProjectService projectService = _projectService as ProjectService;
            TreeEntity treeEnt = ent as TreeEntity;


            if (treeEnt.IsParent)
            {
                List<TreeEntity> ents = projectService.GetTreeEntitiesList(BuiltInCodes.EntityType.WBS);

                int lastChildIndex = -1;
                int count = projectService.DescendantsCountOf(BuiltInCodes.EntityType.WBS, treeEnt, out lastChildIndex);
                if (count > 0)
                {
                    for (int i = lastChildIndex - count; i <= lastChildIndex; i++)
                    {
                        CalcolaFilterItemIds(ents[i]);
                    }
                }
            }
            else
            {
                CalcolaFilterItemIds(treeEnt);
            }
        }

        void CalcolaFilterItemIds(TreeEntity treeEnt)
        {
            if (treeEnt.IsParent)
                return;

            FilterData filterData = GetAttributoFilter(treeEnt.EntityId);
            CalcolaComputoItemIds(treeEnt, filterData);
            CalcolaElementiItemIds(treeEnt, filterData);
        }  
        
        public override void CalcolaAttributiGuidCollection(Entity ent)
        {
            CalcolaFilterItemIds(ent as TreeEntity);
        }

        public FilterData GetAttributoFilter(Guid id)
        {
            ProjectService projectService = _projectService as ProjectService;

            List<Guid> entitiesFound = new List<Guid>();

            FilterData filterData = null;
            IEnumerable<TreeEntity> ents = projectService.GetTreeEntitiesDeepById(BuiltInCodes.EntityType.WBS, new List<Guid>() { id });
            TreeEntity ent = ents.LastOrDefault();
            if (ent == null)
                return null;

            TreeEntity parent = ent;
            if (parent == null)
                return null;

            while (parent != null)
            {
                if (filterData == null)
                    filterData = new FilterData();

                AttributoFilterData attFilterData = null;
                Valore val = parent.GetValoreAttributo(BuiltInCodes.Attributo.AttributoFilter, false, false);
                string json = val.PlainText;

                //if (!JsonSerializer.JsonDeserialize(json, out attFilterData, typeof(AttributoFilterData)))
                //    break;//AU 15/11/2023

                if (JsonSerializer.JsonDeserialize(json, out attFilterData, typeof(AttributoFilterData)))
                {
                    if (attFilterData != null)
                        filterData.Items.Add(attFilterData);
                }

                parent = parent.Parent;
            }

            return filterData;
        }

        private void CalcolaComputoItemIds(TreeEntity ent, FilterData filterData)
        {


            ProjectService projectService = _projectService as ProjectService;

            ValoreGuidCollection newValCollection = new ValoreGuidCollection();

            //if (filterData != null)
            //{

            //    AttributoFilterData attFilterData = null;
            //    Valore val = ent.GetValoreAttributo(BuiltInCodes.Attributo.AttributoFilter, false, false);
            //    string json = val.PlainText;
            //    if (!JsonSerializer.JsonDeserialize(json, out attFilterData, typeof(AttributoFilterData)))
            //        return;


            //    if (attFilterData.EntityTypeKey != BuiltInCodes.EntityType.Computo)
            //        return;

            //    if (attFilterData != null && attFilterData.EntityTypeKey == BuiltInCodes.EntityType.Computo)
            //        filterData.Items.Add(attFilterData);

            //}

            if (ent.Attributi.ContainsKey(BuiltInCodes.Attributo.ComputoItemIds))
            {
                List<Guid> computoIds = new List<Guid>();

                //se nessun filtro o filtri tutti di Computo
                if (filterData == null || (filterData.Items.Any() && filterData.Items.All(item => item.EntityTypeKey == BuiltInCodes.EntityType.Computo)))
                    projectService.GetFilteredEntities(BuiltInCodes.EntityType.Computo, filterData, null, null, out computoIds);


                //if (DeveloperVariables.IsUnderConstruction)
                //{
                    newValCollection.Filter = filterData;
                    newValCollection.V.Clear();
                    newValCollection.FilterResult = computoIds.Select(item => new ValoreGuidCollectionItem() { EntityId = item }).Cast<ValoreCollectionItem>().ToList();
                //}
                //else
                //{
                //    List<ValoreCollectionItem> listItems = computoIds.Select(item => new ValoreGuidCollectionItem() { Id = Guid.NewGuid(), EntityId = item } as ValoreCollectionItem).ToList();
                //    newValCollection.V = listItems;
                //}
                EntityAttributo entAtt = ent.Attributi[BuiltInCodes.Attributo.ComputoItemIds];
                entAtt.Valore = newValCollection;
            }
        }

        private void CalcolaElementiItemIds(TreeEntity ent, FilterData filterData)
        {
            ProjectService projectService = _projectService as ProjectService;

            ValoreGuidCollection newValCollection = new ValoreGuidCollection();

            //if (filterData != null)
            //{

            //    AttributoFilterData attFilterData = null;
            //    Valore val = ent.GetValoreAttributo(BuiltInCodes.Attributo.AttributoFilter, false, false);
            //    string json = val.PlainText;
            //    if (!JsonSerializer.JsonDeserialize(json, out attFilterData, typeof(AttributoFilterData)))
            //        return;

            //    if (attFilterData.EntityTypeKey != BuiltInCodes.EntityType.Elementi)
            //        return;


            //    if (attFilterData != null)
            //        filterData.Items.Add(attFilterData);

            //}

            if (ent.Attributi.ContainsKey(BuiltInCodes.Attributo.ElementiItemIds))
            {
                List<Guid> elementiIds = new List<Guid>();

                //se nessun filtro o filtri tutti di elementi
                if (filterData == null || (filterData.Items.Any() && filterData.Items.All(item => item.EntityTypeKey == BuiltInCodes.EntityType.Elementi)))
                    projectService.GetFilteredEntities(BuiltInCodes.EntityType.Elementi, filterData, null, null, out elementiIds);

                //if (DeveloperVariables.IsUnderConstruction)
                //{
                    newValCollection.V.Clear();
                    newValCollection.FilterResult = elementiIds.Select(item => new ValoreGuidCollectionItem() { EntityId = item }).Cast<ValoreCollectionItem>().ToList(); 
                //}
                //else
                //{
                //    List<ValoreCollectionItem> listItems = elementiIds.Select(item => new ValoreGuidCollectionItem() { Id = Guid.NewGuid(), EntityId = item } as ValoreCollectionItem).ToList();
                //    newValCollection.V = listItems;
                //}
                EntityAttributo entAtt = ent.Attributi[BuiltInCodes.Attributo.ElementiItemIds];
                entAtt.Valore = newValCollection;

            }
        }



        internal async Task<bool> CreateWBSItems(WBSItemsCreationData data)
        {
            Factory factory = new Factory(_projectService);
            string wbsAttivitaIdCodice = string.Join(WBSItemType.AttributoCodiceSeparator, BuiltInCodes.EntityType.ElencoAttivita, BuiltInCodes.Attributo.Id);
            string wbsAttivitaNomeCodice = string.Join(WBSItemType.AttributoCodiceSeparator, BuiltInCodes.EntityType.ElencoAttivita, BuiltInCodes.Attributo.Nome);

            List<WBSItemProxy> itemsProxy = new List<WBSItemProxy>();

            bool ok = await CreateWBSItemsProxyChildren(0, null, null, data, itemsProxy);
            //bool ok = await CreateWBSItemsProxyChildren(null, null, data, itemsProxy);



            if (ok)
            {
                //costruisco in dizionario degli wbsItems presenti
                List<TreeEntity> allEnts = _projectService.GetTreeEntitiesList(BuiltInCodes.EntityType.WBS);
                Dictionary<string, TreeEntity> attFiltersDict = new Dictionary<string, TreeEntity>();
                foreach (WBSItem ent in allEnts)
                {
                    FilterData filterData = GetAttributoFilter(ent.EntityId);
                    string key = CreateFilterDataKey(filterData);
                    if (!attFiltersDict.ContainsKey(key))
                        attFiltersDict.Add(key, ent);

                }

                List<Guid> parentIdsByLevel = new List<Guid>();
                HashSet<Guid> newEntsId = new HashSet<Guid>();

                Dictionary<Guid, int> childCountsById = new Dictionary<Guid, int>();
                Dictionary<Guid, string> codiceById = new Dictionary<Guid, string>();

                for (int i = 0; i < itemsProxy.Count; i++)
                {
                    WBSItemProxy itemProxy = itemsProxy[i];

                    AttributoFilterData attFilterData = new AttributoFilterData();
                    attFilterData.CodiceAttributo = itemProxy.CodiceAttributo;
                    attFilterData.EntityTypeKey = itemProxy.EnitityTypeKey;
                    attFilterData.CheckedValori = itemProxy.Valore;


                    bool addParentsDesc = !data.Items[itemProxy.WBSLevelIndex].IsTreeInLevel;


                    string json = null;
                    if (!JsonSerializer.JsonSerialize(attFilterData, out json))
                        return false;


                    if (attFiltersDict.ContainsKey(itemProxy.Key) && attFiltersDict[itemProxy.Key].Depth == itemProxy.Level)
                    {
                        //elemento esistente (deve essere anche allo stesso livello)
                        //item esistente: procedo con l'aggiornamento
                        
                        TreeEntity entEx = attFiltersDict[itemProxy.Key] as TreeEntity;
                        TreeEntity entExParent = entEx.Parent;

                        Guid parentId = Guid.Empty;
                        if (0 < itemProxy.Level && itemProxy.Level - 1 < parentIdsByLevel.Count)
                            parentId = parentIdsByLevel[itemProxy.Level - 1];

                        if (newEntsId.Contains(parentId) || (entExParent != null && entExParent.EntityId != parentId && parentId != Guid.Empty)) //move
                        {         
                            List<TreeEntity> allEnts2 = _projectService.GetTreeEntitiesList(BuiltInCodes.EntityType.WBS);//ToList added
                            entEx.Attributi[BuiltInCodes.Attributo.AttributoFilter].Valore = new ValoreTesto() { V = json };
                            entEx.Attributi[BuiltInCodes.Attributo.AttributoFilterText].Valore = new ValoreTesto() { V = GetAttributoFilterTextDescription(attFilterData, addParentsDesc) };
                            entEx.Attributi[BuiltInCodes.Attributo.Nome].Valore = new ValoreTesto() { V = string.Format("att{{{0}}}", entEx.Attributi[BuiltInCodes.Attributo.AttributoFilterText].Attributo.Etichetta) };

                            int lastChildIndex;
                            int descendantsCount = _projectService.DescendantsCountOf(BuiltInCodes.EntityType.WBS, entEx, out lastChildIndex);
                            HashSet<Guid> entsId = new HashSet<Guid>(allEnts2.GetRange(lastChildIndex - descendantsCount, descendantsCount+1).Select(item => item.EntityId));

                            TargetReference targetRef = new TargetReference() { Id = parentId, TargetReferenceName = TargetReferenceName.CHILD_OF };

                            ModelAction actionMove = new ModelAction()
                            {
                                EntityTypeKey = BuiltInCodes.EntityType.WBS,
                                ActionName = ActionName.TREEENTITY_MOVE_CHILD_OF,
                                EntitiesId = entsId,
                                NewTargetEntitiesId = new List<TargetReference>() { targetRef }
                            };

                            HashSet<Guid> entsChanged = new HashSet<Guid>();
                            _projectService.TreeEntityMove(actionMove, entsChanged, allEnts2);

                            //eliminare il padre (prima di spostare) se non ha più figli
                            TreeEntity parent = entExParent;
                            while (parent != null)
                            {
                                int descendantCount = _projectService.DescendantsCountOf(BuiltInCodes.EntityType.WBS, parent, out _);
                                if (descendantCount == 0)
                                {
                                    
                                    ModelAction actionDelete = new ModelAction()
                                    {
                                        EntityTypeKey = BuiltInCodes.EntityType.WBS,
                                        ActionName = ActionName.TREEENTITY_DELETE,
                                        EntitiesId = new HashSet<Guid>() { parent.EntityId },
                                    };

                                    parent = parent.Parent;
                                    _projectService.TreeEntityDelete(actionDelete, entsChanged, allEnts2);
                                }
                                else
                                    break;
                                
                            }

                        }

                        //item esistente: procedo con l'aggiornamento

                        if (itemProxy.Level < parentIdsByLevel.Count)
                            parentIdsByLevel[itemProxy.Level] = entEx.EntityId;
                        else
                            parentIdsByLevel.Add(entEx.EntityId);


                    }
                    else
                    {
                        //aggiungo
                        TreeEntity newEnt = factory.NewEntity(BuiltInCodes.EntityType.WBS) as TreeEntity;
                        if (i >= itemsProxy.Count - 1 || itemsProxy[i + 1].Level <= itemProxy.Level)
                        {
                            //leaf
                            newEnt.CreaAttributi();
                        }
                        else
                        {
                            //parent
                            newEnt.CreaAttributiParent();
                        }


                        Guid parentId = Guid.Empty;
                        if (itemProxy.Level > 0)
                            parentId = parentIdsByLevel[itemProxy.Level - 1];

                        if (itemProxy.AttivitaId == Guid.Empty)
                        {
                            newEnt.Attributi[BuiltInCodes.Attributo.AttributoFilter].Valore = new ValoreTesto() { V = json };
                            newEnt.Attributi[BuiltInCodes.Attributo.AttributoFilterText].Valore = new ValoreTesto() { V = GetAttributoFilterTextDescription(attFilterData, addParentsDesc) };
                            //newEnt.Attributi[BuiltInCodes.Attributo.Nome].Valore = new ValoreTesto() { V = string.Format("att{{{0}}}", newEnt.Attributi[BuiltInCodes.Attributo.AttributoFilterText].Attributo.Etichetta) };
                        }
                        else
                        {
                            newEnt.Attributi[BuiltInCodes.Attributo.AttributoFilter].Valore = new ValoreTesto() { V = json };
                            newEnt.Attributi[BuiltInCodes.Attributo.AttributoFilterText].Valore = new ValoreTesto() { V = GetAttributoFilterTextDescription(attFilterData, addParentsDesc) };
                            newEnt.Attributi[wbsAttivitaIdCodice].Valore = new ValoreGuid() { V = itemProxy.AttivitaId };
                            //newEnt.Attributi[BuiltInCodes.Attributo.Nome].Valore = new ValoreTesto() { V = string.Format("att{{{0}}}", newEnt.Attributi[wbsAttivitaNomeCodice].Attributo.Etichetta) };
                            if (itemProxy.LavoroZero)//Articolo con più attività. L'utente deve scegliere quanto lavoro per le varie attività
                            {
                                newEnt.Attributi[BuiltInCodes.Attributo.Lavoro].Valore = new ValoreReale() { V = string.Empty };
                                _projectService.Calculator.Calculate(newEnt, newEnt.Attributi[BuiltInCodes.Attributo.Lavoro].Attributo, newEnt.Attributi[BuiltInCodes.Attributo.Lavoro].Valore);
                            }
                        }
                        newEnt.Attributi[BuiltInCodes.Attributo.Nome].Valore = new ValoreTesto() { V = string.Format("att{{{0}}}", newEnt.Attributi[BuiltInCodes.Attributo.AttributoFilterText].Attributo.Etichetta) };
                        newEnt.Depth = itemProxy.Level;

                        //////////////////////////////
                        //Imposto il codice
                        if (!childCountsById.ContainsKey(parentId))
                            childCountsById.Add(parentId, 1);
                        else
                            childCountsById[parentId] = childCountsById[parentId] + 1;

                        if (codiceById.ContainsKey(parentId))
                            codiceById.Add(newEnt.EntityId, string.Format("{0}.{1}", codiceById[parentId], childCountsById[parentId]));
                        else
                            codiceById.Add(newEnt.EntityId, childCountsById[parentId].ToString());

                        string codice = codiceById[newEnt.EntityId];
                        newEnt.Attributi[BuiltInCodes.Attributo.Codice].Valore = new ValoreTesto() { V = codice };
                        //////////////////////////////

                        DataService.AddEntity(BuiltInCodes.EntityType.WBS, newEnt, parentId);

                        if (itemProxy.Level < parentIdsByLevel.Count)
                            parentIdsByLevel[itemProxy.Level] = newEnt.EntityId;
                        else
                            parentIdsByLevel.Add(newEnt.EntityId);

                        newEntsId.Add(newEnt.EntityId);
                    }


                }


                //ricalcolo
                var calcOptions = new EntityCalcOptions() { CalcolaAttributiResults = true, ResetCalulatedValues = true };
                CalcolaEntities(BuiltInCodes.EntityType.WBS, calcOptions);



                return true;
            }

            return false;
        }

        /// <summary>
        /// Calcola l'unione di tutte le attività associate agli articoli associati alle voci di computo
        /// </summary>
        /// <param name="computoItemsId"></param>
        /// <returns></returns>
        private async Task<List<string>> AttivitaItemsIdByComputoItemsId(List<Guid> computoItemsId)
        {
            List<string> prezzarioItemsIdStr = await DataService.GetValoriUnivociAsync(BuiltInCodes.EntityType.Computo, computoItemsId, BuiltInCodes.Attributo.PrezzarioItem_Guid, -1, null);
            List<Guid> prezzarioItemsId = prezzarioItemsIdStr.Select(item => new Guid(item)).ToList();

            List<string> attivitaItemsId = await DataService.GetValoriUnivociAsync(BuiltInCodes.EntityType.Prezzario, prezzarioItemsId, BuiltInCodes.Attributo.Attivita, -1, null);
            return attivitaItemsId;
        }

        /// <summary>
        /// Verifica se è stato filtrato anche per attività
        /// </summary>
        /// <param name="filterData"></param>
        /// <param name="attivitasId"></param>
        /// <returns></returns>
        private bool IsFilterByAttivita(FilterData filterData, out IEnumerable<Guid> attivitasId)
        {
            attivitasId = null;
            EntitiesHelper entsHelper = new EntitiesHelper(DataService);

            if (filterData == null)
                return false;

            var attivitasFilterData = filterData.Items.Where(item =>
            {
                Attributo sourceAtt = entsHelper.GetSourceAttributo(item.EntityTypeKey, item.CodiceAttributo);
                if (sourceAtt.GuidReferenceEntityTypeKey == BuiltInCodes.EntityType.ElencoAttivita)
                    return true;

                return false;
            });
            
            if (attivitasFilterData.Any())
            {
                attivitasId = attivitasFilterData.SelectMany(item => item.CheckedValori).Select(item => new Guid(item));
                return true;
            }

            return false;//non è stato filtrato per un attributo riferito alle attività
        }

        private async Task<List<WBSItemProxy>> CreateWBSItemsProxyAttivita(List<Guid> computoItemsId, WBSItemProxy parentProxy, FilterData filterData)
        {


            List<WBSItemProxy> attivitasUnivoche = new List<WBSItemProxy>();

            

            List<WBSItemProxy> attivitas = new List<WBSItemProxy>();

            List<string> prezzarioItemsIdStr = await DataService.GetValoriUnivociAsync(BuiltInCodes.EntityType.Computo, computoItemsId, BuiltInCodes.Attributo.PrezzarioItem_Guid, -1, null);
            List<Guid> prezzarioItemsId = prezzarioItemsIdStr.Select(item => new Guid(item)).ToList();

            List<Entity> prezItems = DataService.GetEntitiesById(BuiltInCodes.EntityType.Prezzario, prezzarioItemsIdStr.Select(item => new Guid(item)));
            Dictionary<string, FilterData> filterDataByKey = new Dictionary<string, FilterData>();

            foreach (PrezzarioItem prezItem in prezItems)
            {

                ValoreGuidCollection valAttivitaIds = prezItem.GetValoreAttributo(BuiltInCodes.Attributo.Attivita, false, false) as ValoreGuidCollection;

                bool lavoroZero = valAttivitaIds.Items.Count > 1;

                foreach (ValoreGuidCollectionItem valAttivitaId in valAttivitaIds.Items)
                {
                    Guid attivitaId = valAttivitaId.EntityId;

                    /////////////////
                    //Preparo il filtro per attività
                    FilterData filterDataNew = null;
                    if (filterData == null)
                        filterDataNew = new FilterData();
                    else
                        filterDataNew = filterData.Clone();

                    AttributoFilterData attFilterData = new AttributoFilterData();
                    attFilterData.EntityTypeKey = BuiltInCodes.EntityType.Computo;
                    attFilterData.CodiceAttributo = string.Join(ComputoItemType.AttributoCodiceSeparator, BuiltInCodes.EntityType.Prezzario, BuiltInCodes.Attributo.Attivita);// BuiltInCodes.Attributo.PrezzarioItem_Guid;
                    //attFilterData.CheckedValori = new HashSet<string>() { prezItem.EntityId.ToString() };
                    attFilterData.CheckedValori = new HashSet<string>() { attivitaId.ToString() };
                    filterDataNew.Items.Add(attFilterData);
                    //////////////////


                    WBSItemProxy attivitaProxy = new WBSItemProxy();
                    attivitaProxy.AttivitaId = attivitaId;
                    attivitaProxy.EnitityTypeKey = BuiltInCodes.EntityType.Computo;
                    //attivitaProxy.CodiceAttributo = BuiltInCodes.Attributo.PrezzarioItem_Guid;
                    attivitaProxy.CodiceAttributo = string.Join(ComputoItemType.AttributoCodiceSeparator, BuiltInCodes.EntityType.Prezzario, BuiltInCodes.Attributo.Attivita);
                    attivitaProxy.Valore = new HashSet<string>() { attivitaId.ToString() };
                    attivitaProxy.Key = CreateFilterDataKey(filterDataNew);
                    filterDataByKey.TryAdd(attivitaProxy.Key, filterDataNew);

                    if (parentProxy != null)
                    {
                        attivitaProxy.Parent = parentProxy;
                        attivitaProxy.Level = parentProxy.Level + 1;
                    }
                    else
                    {
                        attivitaProxy.Parent = null;
                        attivitaProxy.Level = 0;
                    }
                    attivitaProxy.LavoroZero = lavoroZero;

                    attivitas.Add(attivitaProxy);
                }//end attività
            }//end articoli

            //raggruppo le attività uguali

            IEnumerable<IGrouping<Guid, WBSItemProxy>> itemsGrouped = attivitas.GroupBy(item => item.AttivitaId);
            foreach (var group in itemsGrouped)
            {
                WBSItemProxy atv = null;
                foreach (var item in group)
                {
                    if (atv == null)
                        atv = item;
                    else
                    {
                        atv.Valore.UnionWith(item.Valore);

                        FilterData filterAtv = filterDataByKey[atv.Key];
                        AttributoFilterData attFilterData = filterAtv.Items.Last();

                        attFilterData.CheckedValori.UnionWith(item.Valore);
                    }
                }

                atv.Key = CreateFilterDataKey(filterDataByKey[atv.Key]);
                attivitasUnivoche.Add(atv);
            }


            //////////////////////
            ///Se è presente un filtro per attività limito il risultato
            IEnumerable<Guid> attivitasIdInFilter;
            if (IsFilterByAttivita(filterData, out attivitasIdInFilter))
            {
                HashSet<Guid> attivitasIdInFilterHS = attivitasIdInFilter.ToHashSet();
                attivitasUnivoche = attivitasUnivoche.Where(item => attivitasIdInFilterHS.Contains(item.AttivitaId)).ToList();
            }

            return attivitasUnivoche;
        }

        string GetAttributoFilterTextDescription(AttributoFilterData attFilterData, bool addParentsDescription)
        {

            string str = string.Empty;

            if (attFilterData == null)
                return string.Empty;

            EntityType entType = DataService.GetEntityTypes().Values.FirstOrDefault(item => item.Codice == attFilterData.EntityTypeKey);
            if (entType == null)
                return string.Empty;

            Attributo att = null;
            if (!entType.Attributi.TryGetValue(attFilterData.CodiceAttributo, out att))
                return string.Empty;

            Attributo sourceAtt = _entitiesHelper.GetSourceAttributo(att);
            if (sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid || sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.GuidCollection)
            {

                IEnumerable<Guid> ids = attFilterData.CheckedValori.Select(item => new Guid(item));
                List<Entity> ents = DataService.GetEntitiesById(sourceAtt.GuidReferenceEntityTypeKey, ids);

                if (addParentsDescription)
                {
                    str = string.Join("\n", ents.Select(item => item.ToUserIdentity(UserIdentityMode.SingleLine)));
                }
                else
                {
                    str = string.Join("\n", ents.Select(item => item.ToUserIdentity(UserIdentityMode.Nothing)));
                }
            }
            else
            {
                str = string.Join("\n", attFilterData.CheckedValori);

            }

            return str;
        }



        /// <summary>
        /// metodo ricorsivo
        /// </summary>
        /// <param name="itemProxy"></param>
        /// <param name="filterData"></param>
        /// <param name="data"></param>
        /// <param name="itemsProxy"></param>
        async Task<bool> CreateWBSItemsProxyChildren(int wbsLevelIndex, WBSItemProxy itemProxy, FilterData filterData, WBSItemsCreationData data, List<WBSItemProxy> itemsProxy)
        {

            if (wbsLevelIndex >= data.Items.Count)
                return false;

            WBSLevel wbsLevel = data.Items[wbsLevelIndex];

            string entityTypeKey = wbsLevel.AttributoFilterData.EntityTypeKey;
            string codiceAttributo = wbsLevel.AttributoFilterData.CodiceAttributo;

            List<Guid> entitiesId = null;
            DataService.GetFilteredEntities(entityTypeKey, filterData, null, null, out entitiesId);
            HashSet<string> valori = new HashSet<string>(await DataService.GetValoriUnivociAsync(entityTypeKey, entitiesId, codiceAttributo, -1, null));

            List<string> valoriOrdered = new List<string>(valori);

            Dictionary<Guid, Guid> treeEntsParent = null;

            //Se l'attributo è di tipo riferiemento o guid ordino i valori secondo la sezione sorgente
            EntityType entType = DataService.GetEntityTypes()[entityTypeKey];
            if (entType.Attributi.ContainsKey(codiceAttributo))
            {
                Attributo att = null;
                entType.Attributi.TryGetValue(codiceAttributo, out att);
                Attributo sourceAtt = _entitiesHelper.GetSourceAttributo(att);

                if (sourceAtt != null)
                {
                    if (sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid && wbsLevel.IsTreeInLevel)//mantieni struttura
                    {

                        EntityType guidReferenceEntityType = DataService.GetEntityTypes()[sourceAtt.GuidReferenceEntityTypeKey];

                        if (guidReferenceEntityType.IsTreeMaster)
                        {

                            List<Guid> ids = new List<Guid>();
                            var treeEnts = DataService.GetFilteredTreeEntities(sourceAtt.GuidReferenceEntityTypeKey, null, null, out ids);

                            for (int i = treeEnts.Count - 1; i >= 0; i--)
                            {
                                Guid id = treeEnts[i].Id;
                                string strId = id.ToString();
                                if (!valori.Contains(strId))
                                {
                                    if (!treeEnts.Any(item => item.ParentId == id))
                                    {
                                        treeEnts.RemoveAt(i);
                                    }
                                }
                            }

                            treeEntsParent = treeEnts.ToDictionary(item => item.Id, item => item.ParentId);

                            valoriOrdered = new List<string>(treeEnts.Select(item => item.Id.ToString()));
                            if (valori.Contains(Guid.Empty.ToString()))
                                valoriOrdered.Insert(0, Guid.Empty.ToString());
                        }
                    }
                    else if ( (att != null && att is AttributoRiferimento) || sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid)
                    {
                        List<Guid> ids = new List<Guid>();
                        DataService.GetFilteredEntities(sourceAtt.EntityTypeKey, null, null, null, out ids);
                        List<string> valRef = await DataService.GetValoriUnivociAsync(sourceAtt.EntityTypeKey, ids, sourceAtt.Codice, -1, null);
                        if (valori.Contains(ValoreHelper.ValoreNullAsText))
                            valRef.Insert(0, ValoreHelper.ValoreNullAsText);

                        valoriOrdered = new List<string>(valRef.Where(item => valori.Contains(item)));

                    }
                }

            }

            /////mappe utilizzate nel caso di wbsLevel.IsTreeInLevel == true (mantieni struttura)
            Dictionary<Guid, FilterData> filterDataById = new Dictionary<Guid, FilterData>();
            Dictionary<Guid, WBSItemProxy> itemsProxyById = new Dictionary<Guid, WBSItemProxy>();
            /////

            foreach (string val in valoriOrdered)
            {
                if (!wbsLevel.IsValoriAuto)
                {
                    if (treeEntsParent != null)
                    {

                        if (wbsLevel.IsTreeInLevel)//mantieni struttura
                        {
                            //se val non è checkato e non ha nessun figlio checkato => continue
                            if (!wbsLevel.AttributoFilterData.CheckedValori.Contains(val))
                            {
                                if (!AnyIdDescendsFromAnchestor(wbsLevel.AttributoFilterData.CheckedValori, treeEntsParent, val))
                                    continue;
                            }
                        }
                        else
                        {
                            
                            if (AnyIdDescendsFromAnchestor(wbsLevel.AttributoFilterData.CheckedValori, treeEntsParent, val))
                                continue;//se val ha almeno un figlio checkato => continue
                            else if (!wbsLevel.AttributoFilterData.CheckedValori.Contains(val))
                                continue;//se val non è checkato e non ha nessun figlio checkato => continue
                        }
                    }
                    else
                    {
                        if (!wbsLevel.AttributoFilterData.CheckedValori.Contains(val))
                            continue;
                    }
                }

                FilterData filterDataNew = null;
                

                WBSItemProxy childProxy = new WBSItemProxy()
                {
                    EnitityTypeKey = entityTypeKey,
                    CodiceAttributo = codiceAttributo,
                    Valore = new HashSet<string>() { val },
                    WBSLevelIndex = wbsLevelIndex,
                    
                };
                


                bool isParentInWBSLevel = false;
                if (treeEntsParent != null && wbsLevel.IsTreeInLevel)//mantieni struttura
                {

                    Guid id = new Guid(val);
                    if (treeEntsParent.Values.Any(item => item == id))
                        isParentInWBSLevel = true;

                    Guid parentId = Guid.Empty;
                    treeEntsParent.TryGetValue(id, out parentId);

                    WBSItemProxy parentItemProxy = itemProxy;
                    if (parentId == Guid.Empty)
                    {
                        parentItemProxy = itemProxy;

                        if (filterData == null)
                            filterDataNew = new FilterData();
                        else
                            filterDataNew = filterData.Clone();
                    }
                    else 
                    {
                        parentItemProxy = itemsProxyById[parentId];

                        FilterData fd = null;
                        filterDataById.TryGetValue(parentId, out fd);
                        filterDataNew = fd.Clone();
                    }
                    ///Filtro
                    AttributoFilterData attFilterData = wbsLevel.AttributoFilterData.Clone();
                    attFilterData.CheckedValori = new HashSet<string>() { val };
                    filterDataNew.Items.Add(attFilterData);

                    filterDataById.TryAdd(id, filterDataNew);
                    ////

                    childProxy.Key = CreateFilterDataKey(filterDataNew);
                    childProxy.Level = (parentItemProxy != null) ? parentItemProxy.Level + 1 : ((itemProxy == null) ? 0 : itemProxy.Level + 1);
                    childProxy.Parent = parentItemProxy;

                    itemsProxyById.TryAdd(id, childProxy);
                    
                }
                else
                {
                    /////////////////
                    //Preparo il filtro

                    if (filterData == null)
                        filterDataNew = new FilterData();
                    else
                        filterDataNew = filterData.Clone();


                    AttributoFilterData attFilterData = wbsLevel.AttributoFilterData.Clone();
                    attFilterData.CheckedValori = new HashSet<string>() { val };
                    filterDataNew.Items.Add(attFilterData);
                    ////

                    childProxy.Key = CreateFilterDataKey(filterDataNew);
                    childProxy.Level = (itemProxy == null) ? 0 : itemProxy.Level + 1;
                    childProxy.Parent = itemProxy;
                }

                itemsProxy.Add(childProxy);
                



                if (!isParentInWBSLevel)//è una foglia del wbsLevel: posso aggiungere come figli attività o il prossimo wbsLevel
                {

                    if (IsFilterByAttivita(filterDataNew, out _) && wbsLevelIndex == data.Items.Count - 1 && data.EntityTypeKey == BuiltInCodes.EntityType.Computo)
                    {
                        List<Guid> computoIds = null;
                        DataService.GetFilteredEntities(BuiltInCodes.EntityType.Computo, filterDataNew, null, null, out computoIds);

                        List<WBSItemProxy> attivitas = await CreateWBSItemsProxyAttivita(computoIds, childProxy, filterDataNew);

                        if (attivitas.Count == 1)
                        {
                            childProxy.AttivitaId = attivitas.First().AttivitaId;
                        }
                        else
                        {
                            itemsProxy.AddRange(attivitas);
                        }
                    }


                    await CreateWBSItemsProxyChildren(wbsLevelIndex + 1, childProxy, filterDataNew, data, itemsProxy);

                }
            }

            return true;
        }

        private static bool AnyIdDescendsFromAnchestor(HashSet<string> ids, Dictionary<Guid, Guid> treeEntsParent, string anchestor)
        {
            Guid anchestorId = new Guid(anchestor);

            return ids.Any(item =>
            {
                Guid parentId = new Guid(item);
                treeEntsParent.TryGetValue(parentId, out parentId);

                while (parentId != Guid.Empty)
                {
                    if (parentId == anchestorId)
                        return true;

                    treeEntsParent.TryGetValue(parentId, out parentId);
                }
                return false;
            });
        }

        //        /// <summary>
        //        /// metodo ricorsivo
        //        /// </summary>
        //        /// <param name="itemProxy"></param>
        //        /// <param name="filterData"></param>
        //        /// <param name="data"></param>
        //        /// <param name="itemsProxy"></param>
        //        async Task<bool> CreateWBSItemsProxyChildren(WBSItemProxy itemProxy, FilterData filterData, WBSItemsCreationData data, List<WBSItemProxy> itemsProxy)
        //        {
        //            int level = 0;

        //            if (itemProxy != null)
        //                level = itemProxy.Level + 1;


        //            if (level >= data.Items.Count)
        //                return false;

        //            WBSLevel wbsLevel = data.Items[level];

        //            string entityTypeKey = wbsLevel.AttributoFilterData.EntityTypeKey;
        //            string codiceAttributo = wbsLevel.AttributoFilterData.CodiceAttributo;

        //            List<Guid> entitiesId = null;
        //            DataService.GetFilteredEntities(entityTypeKey, filterData, null, null, out entitiesId);
        //            HashSet<string> valori = new HashSet<string>(await DataService.GetValoriUnivociAsync(entityTypeKey, entitiesId, codiceAttributo, -1, null));

        //            List<string> valoriOrdered = new List<string>(valori);

        //            Dictionary<Guid, Guid> treeEntsParent = null;

        //            //Se l'attributo è di tipo riferiemento ordino i valori secondo la sezione sorgente
        //            EntityType entType = DataService.GetEntityTypes()[entityTypeKey];
        //            if (entType.Attributi.ContainsKey(codiceAttributo))
        //            {
        //                Attributo att = entType.Attributi[codiceAttributo];
        //                if (att != null && att is AttributoRiferimento)
        //                {
        //                    Attributo sourceAtt = _entitiesHelper.GetSourceAttributo(att);
        //                    List<Guid> ids = new List<Guid>();
        //                    DataService.GetFilteredEntities(sourceAtt.EntityTypeKey, null, null, null, out ids);
        //                    List<string> valRef = await DataService.GetValoriUnivociAsync(sourceAtt.EntityTypeKey, ids, sourceAtt.Codice, -1, null);
        //                    if (valori.Contains(ValoreHelper.ValoreNullAsText))
        //                        valRef.Insert(0, ValoreHelper.ValoreNullAsText);

        //                    valoriOrdered = new List<string>(valRef.Where(item => valori.Contains(item)));

        //                }
        //            }

        //            foreach (string val in valoriOrdered)
        //            {
        //                if (!wbsLevel.IsValoriAuto && !wbsLevel.AttributoFilterData.CheckedValori.Contains(val))
        //                    continue;

        //                /////////////////
        //                //Preparo il filtro
        //                FilterData filterDataNew = null;
        //                if (filterData == null)
        //                    filterDataNew = new FilterData();
        //                else
        //                    filterDataNew = filterData.Clone();


        //                AttributoFilterData attFilterData = wbsLevel.AttributoFilterData.Clone();
        //                attFilterData.CheckedValori = new HashSet<string>() { val };
        //                filterDataNew.Items.Add(attFilterData);
        //                //

        //                WBSItemProxy childProxy = new WBSItemProxy()
        //                {
        //                    EnitityTypeKey = entityTypeKey,
        //                    CodiceAttributo = codiceAttributo,
        //                    Valore = new HashSet<string>() { val },
        //                    Level = level,
        //                };
        //                childProxy.Key = CreateFilterDataKey(filterDataNew);
        //                childProxy.Parent = itemProxy;
        //                itemsProxy.Add(childProxy);


        //                if (IsFilterByAttivita(filterDataNew, out _) && childProxy.Level == data.Items.Count - 1 && data.EntityTypeKey == BuiltInCodes.EntityType.Computo)
        //                {
        //                    List<Guid> computoIds = null;
        //                    DataService.GetFilteredEntities(BuiltInCodes.EntityType.Computo, filterDataNew, null, null, out computoIds);

        //                    List<WBSItemProxy> attivitas = await CreateWBSItemsProxyAttivita(computoIds, childProxy, filterDataNew);

        //                    if (attivitas.Count == 1)
        //                    {
        //                        childProxy.AttivitaId = attivitas.First().AttivitaId;
        //                    }
        //                    else
        //                    {
        //                        itemsProxy.AddRange(attivitas);
        //                    }
        //                }


        //                await CreateWBSItemsProxyChildren(childProxy, filterDataNew, data, itemsProxy);


        //            }

        //            return true;
        //        }



        string CreateFilterDataKey(FilterData filterData, Guid? attivitaId = null)
        {
            string key = string.Empty;
            if (filterData == null)
            {
                key = string.Join("|", key, attivitaId);
            }
            else
            {
                key = string.Join("|", filterData.Items.Select(item => CreateAttributoFilterDataKey(item)).OrderBy(item => item));
                if (attivitaId != null && attivitaId != Guid.Empty)
                    key = string.Join("|", key, attivitaId);
            }
            return key;
        }

        string CreateAttributoFilterDataKey(AttributoFilterData attFilterData)
        {
            if (attFilterData == null)
                return string.Empty;

            string key = CreateAttributoFilterDataKey(attFilterData.EntityTypeKey, attFilterData.CodiceAttributo, attFilterData.CheckedValori);
            return key;
        }

        string CreateAttributoFilterDataKey(string entityTypeKey, string codiceAttributo, string valore)
        {
            string key = string.Join("|", entityTypeKey, codiceAttributo, valore);
            return key;
        }

        string CreateAttributoFilterDataKey(string entityTypeKey, string codiceAttributo, HashSet<string> checkedValori)
        {
            string key = CreateAttributoFilterDataKey(entityTypeKey, codiceAttributo, string.Join("|", checkedValori));
            return key;
        }

        public static string GetAttributoPredecessoriTextDescription(WBSPredecessors WBSPredecessors, IDataService dataService)
        {

            EntitiesHelper entsHelper = new EntitiesHelper(dataService);
            string str = string.Empty;

            foreach (WBSPredecessor Item in WBSPredecessors?.Items)
            {
                Entity Entity = dataService.GetEntityById(BuiltInCodes.EntityType.WBS, Item.WBSItemId);
                string Codice = entsHelper.GetValoreAttributo(Entity, BuiltInCodes.Attributo.Codice, false, true)?.PlainText;
                string Relazione = null;
                if (Item.Type == WBSPredecessorType.FinishToFinish)
                    Relazione = LocalizationProvider.GetString("FF");
                if (Item.Type == WBSPredecessorType.StartToStart)
                    Relazione = LocalizationProvider.GetString("II");
                if (Item.Type == WBSPredecessorType.FinishToStart)
                    Relazione = LocalizationProvider.GetString("FI");
                if (Item.Type == WBSPredecessorType.StartToFinish)
                    Relazione = LocalizationProvider.GetString("IF");
                str = str + Codice + " " + Relazione + " " + "(" + Item.DelayDays + "); ";
            }

            return str;
        }

    }

    internal class WBSItemProxy
    {
        public string Key { get; set; } = string.Empty;
        public int Level { get; set; } = 0;
        public WBSItemProxy Parent { get; set; } = null;
        public string EnitityTypeKey { get; set; } = string.Empty;
        public string CodiceAttributo { get; set; } = string.Empty;
        public HashSet<string> Valore { get; set; } = new HashSet<string>();
        public Guid AttivitaId { get; set; } = Guid.Empty;
        public bool LavoroZero { get; internal set; } = false;
        public int WBSLevelIndex { get; set; } = 0;
    }
}
