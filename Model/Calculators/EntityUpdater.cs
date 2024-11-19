using _3DModelExchange;
using CommonResources;
using Commons;
using DevExpress.Mvvm.Native;
using DevExpress.Xpf.Core.Native;
using MasterDetailModel;
using Model.Calculators;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;

namespace Model
{

    public class EntityAttributiUpdater
    {
        ProjectServiceBase _projectService = null;
        Dictionary<string, SectionEntityAttributiUpdater> _sectionUpdaters = new Dictionary<string, SectionEntityAttributiUpdater>();

        //scopo: tener traccia delle entità modificate
        private HashSet<Guid> _entityIdsChanged = new HashSet<Guid>();


        internal EntityAttributiUpdater(ProjectServiceBase projectService)
        {
            _projectService = projectService;

            InfoProgettoEntityAttributiUpdater infoProgettoEntityAttributiUpdater = new InfoProgettoEntityAttributiUpdater(_projectService);

            _sectionUpdaters.TryAdd(BuiltInCodes.EntityType.Prezzario, new PrezzarioEntityAttributiUpdater(_projectService));
            _sectionUpdaters.TryAdd(BuiltInCodes.EntityType.Capitoli, new CapitoliEntityAttributiUpdater(_projectService));
            _sectionUpdaters.TryAdd(BuiltInCodes.EntityType.Computo, new ComputoEntityAttributiUpdater(_projectService));
            _sectionUpdaters.TryAdd(BuiltInCodes.EntityType.Elementi, new ElementiEntityAttributiUpdater(_projectService));
            _sectionUpdaters.TryAdd(BuiltInCodes.EntityType.Divisione, new DivisioneEntityAttributiUpdater(_projectService));
            _sectionUpdaters.TryAdd(BuiltInCodes.EntityType.Contatti, new ContattiEntityAttributiUpdater(_projectService));
            _sectionUpdaters.TryAdd(BuiltInCodes.EntityType.InfoProgetto, infoProgettoEntityAttributiUpdater);/*new InfoProgettoEntityAttributiUpdater(_projectService)*/
            _sectionUpdaters.TryAdd(BuiltInCodes.EntityType.Documenti, infoProgettoEntityAttributiUpdater);/*new InfoProgettoEntityAttributiUpdater(_projectService)*/
            _sectionUpdaters.TryAdd(BuiltInCodes.EntityType.Report, infoProgettoEntityAttributiUpdater);/*new InfoProgettoEntityAttributiUpdater(_projectService)*/
            _sectionUpdaters.TryAdd(BuiltInCodes.EntityType.ElencoAttivita, new ElencoAttivitaEntityAttributiUpdater(_projectService));
            _sectionUpdaters.TryAdd(BuiltInCodes.EntityType.WBS, new WBSEntityAttributiUpdater(_projectService));
            _sectionUpdaters.TryAdd(BuiltInCodes.EntityType.Calendari, new CalendariEntityAttributiUpdater(_projectService));
            _sectionUpdaters.TryAdd(BuiltInCodes.EntityType.Variabili, new VarEntityAttributiUpdater(_projectService));
            _sectionUpdaters.TryAdd(BuiltInCodes.EntityType.Allegati, new AllEntityAttributiUpdater(_projectService));

        }



        /// <summary>
        /// Calcola le entità, le entità riferite nella stessa sezione e  le entità riferite in altre sezioni
        /// </summary>
        /// <param name="entTypeKey"></param>
        /// <param name="entitiesId"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        internal bool CalcolaEntityValuesAndDependentEntityTypes(string entTypeKey, IEnumerable<Guid> entitiesId, EntityCalcOptions calcOptions, EntitiesError error)
        {
            if (string.IsNullOrEmpty(entTypeKey))
                return false;

            if (entitiesId == null)
            {
                var allEntities = _projectService.GetEntities(entTypeKey);
                if (allEntities != null)
                    entitiesId = allEntities.Select(x => x.EntityId);
            }

            HashSet<string> entityTypesCalculated = new HashSet<string>();

            //Trovo tutte le entità che fanno riferimento nelle sezioni dipendenti
            //key: EntityId, value: Entity
            Dictionary<Guid, Entity> allDepEntsId = new Dictionary<Guid, Entity>();

            GetAllReferencedEntities(entTypeKey, entitiesId, ref allDepEntsId);



            //Calcolo le entità di entTypeKey se non sono state cancellate
            List<Entity> entities = _projectService.GetEntitiesById(entTypeKey, entitiesId);
            //HashSet<Guid> invalidEntsId = entitiesId.Where(x => !_projectService.IsEntityValid(entTypeKey, x)).ToHashSet();
            //var validEntsId = entitiesId.Where(x => _projectService.IsEntityValid(entTypeKey, x));

            SectionEntityAttributiUpdater sectionUpdater = GetSectionUpdater(entTypeKey);
            if (entitiesId.Any() && sectionUpdater != null)
            {
                //sectionUpdater.CalcolaEntities(entTypeKey, entities, error);
                sectionUpdater.CalcolaEntities(entTypeKey, calcOptions, entitiesId, error);
            }
            entityTypesCalculated.Add(entTypeKey);

            //////////////////////////////////////////////////////////////
            //Calcolo le entità  nelle altre sezioni che fanno riferimento


            //Calcolo le sezioni dipendenti ordinate per il ricalcolo
            List<string> dependentTypes = GetDependentEntityTypesKey(entTypeKey);

            


            //Ricalcolo

            foreach (string depEntTypeKey in dependentTypes)
            {
                //non eseguo il ricalcolo della sezione WBS (va fatto a mano col pulsante
                if (depEntTypeKey == BuiltInCodes.EntityType.WBS)
                {
                    if (entTypeKey != depEntTypeKey)
                        continue;
                }


                SectionEntityAttributiUpdater sectionUpdaterRef = GetSectionUpdater(depEntTypeKey);

                //////////////////////////////////////////////
                ////Aggiornamento degli attributi ValoreGuid e ValoreGuidCollection 

                //Aggiorno a Guid.Empty gli attributi ValoreGuid il cui id non esiste più
                //Aggiorno gli attributi di tipo ValoreGuidCollection che riferiscono ad una delle sezioni ricalcolate
                EntityType depEntityType = _projectService.GetEntityTypes()[depEntTypeKey];
                var attsGuidColl = depEntityType.Attributi.Values.Where(x => (x.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid || x.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.GuidCollection) && entityTypesCalculated.Contains(x.GuidReferenceEntityTypeKey));
                if (attsGuidColl.Any())
                {
                    IEnumerable<Entity> allEntities = _projectService.GetEntities(depEntTypeKey);
                    foreach (Entity ent in allEntities)
                    {
                        sectionUpdaterRef.CalcolaAttributiGuid(ent);
                        sectionUpdaterRef.CalcolaAttributiGuidCollection(ent);
                    }
                }
                /////////////////////////////////////////////////

                //Ricalcolo
                var entsToCalculate = allDepEntsId.Values.Where(x => x.EntityTypeCodice == depEntTypeKey);
                var entsIdToCalculate = entsToCalculate.Select(x => x.EntityId);

                //sectionUpdaterRef.CalcolaEntities(depEntTypeKey, entsToCalculate, error);
                sectionUpdaterRef.CalcolaEntities(depEntTypeKey, calcOptions, entsIdToCalculate, error);
                entityTypesCalculated.Add(depEntTypeKey);

            }

            return true;
        }

        /// <summary>
        /// Ricalcolo tutti i valori dell'entità senza ricalcolare gli attributi riferiti
        /// </summary>
        /// <param name="ent"></param>
        /// <returns></returns>
        internal bool CalcolaEntitiesValues(string entTypeKey, IEnumerable<Entity> ents, EntityCalcOptions options, EntitiesError error = null)
        {
            Debug.Print(string.Format("{0}({1})", System.Reflection.MethodBase.GetCurrentMethod().ToString(), entTypeKey));


            SectionEntityAttributiUpdater sectionUpdaterRef = GetSectionUpdater(entTypeKey);


            if (sectionUpdaterRef != null)
            {
                //var calcOptions = new EntityCalcOptions() { ResetCalulatedValues = true, CalcolaAttributiResults = true };
                sectionUpdaterRef.CalcolaEntities(entTypeKey, options, ents.Select(x => x.EntityId), error);
            }

            //sectionUpdaterRef.CalcolaEntities(entTypeKey, ents, error);

            return true;
        }


        internal bool CalcolaEntitiesValues(string entTypeKey, IEnumerable<Guid> entitiesId, EntityCalcOptions calcOptions, EntitiesError error)
        {
            if (entitiesId == null)
            {
                var allEntities = _projectService.GetEntities(entTypeKey);
                if (allEntities != null)
                    entitiesId = allEntities.Select(x => x.EntityId);
            }

            HashSet<string> entityTypesCalculated = new HashSet<string>();


            //Calcolo le entità di entTypeKey se non sono state cancellate
            List<Entity> entities = _projectService.GetEntitiesById(entTypeKey, entitiesId);
            HashSet<Guid> invalidEntsId = entitiesId.Where(x => !_projectService.IsEntityValid(entTypeKey, x)).ToHashSet();
            var validEntsId = entitiesId.Where(x => _projectService.IsEntityValid(entTypeKey, x));

            SectionEntityAttributiUpdater sectionUpdater = GetSectionUpdater(entTypeKey);
            if (entities.Any() && sectionUpdater != null)
            {
                //sectionUpdater.CalcolaEntities(entTypeKey, entities, error);
                sectionUpdater.CalcolaEntities(entTypeKey, calcOptions, validEntsId, error);
            }
            entityTypesCalculated.Add(entTypeKey);

            return true;
        }

            //public static SectionEntityAttributiUpdater GetSectionUpdater(string entityTypeCode)
            //{
            //    SectionEntityAttributiUpdater sectionUpdater = null;
            //    _sectionUpdaters.TryGetValue(entityTypeCode, out sectionUpdater);
            //    return sectionUpdater;

            //}



            ///// <summary>
            ///// Ricalcolo tutti i valori dell'entità senza ricalcolare gli attributi riferiti
            ///// </summary>
            ///// <param name="ent"></param>
            ///// <returns></returns>
            //internal bool CalcolaEntityValues(Entity ent)
            //{
            //    Debug.Print(System.Reflection.MethodBase.GetCurrentMethod().ToString());

            //    try
            //    {
            //        SectionEntityAttributiUpdater sectionUpdater = GetSectionUpdater(ent.EntityTypeCodice);

            //        if (sectionUpdater != null)
            //        {
            //            sectionUpdater.ClearCalculatedValues();
            //            sectionUpdater.CalcolaAttributiGuidCollection(ent);
            //            sectionUpdater.CalcolaAttributiRiferimento(ent);
            //            sectionUpdater.CalcolaAttributiValues(ent);
            //        }

            //    }
            //    catch (Exception e)
            //    {
            //        MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), e.Message);
            //    }
            //    return true;
            //}


            ///// <summary>
            ///// Ricalcola tutti i valori degli attributi dell'entità ent che dipendono da attNewValore
            ///// </summary>
            ///// <param name="ent">entità di cui ricalcolare gli attributi</param>
            ///// <param name="attNewValore">valore modificato</param>
            ///// <returns></returns>
            //internal bool CalcolaEntityValues(Entity ent, EntityAttributo attNewValore, EntitiesError error)
            //{
            //    Debug.Print(System.Reflection.MethodBase.GetCurrentMethod().ToString());

            //    if (ent == null)
            //        return false;

            //    string entTypeKey = ent.EntityType.GetKey();

            //    SectionEntityAttributiUpdater sectionUpdater = GetSectionUpdater(entTypeKey);

            //    if (sectionUpdater != null)
            //    {
            //        //Tutti gli attributi nella sezione che dipendono da attNewValore tramite funzione
            //        List<EntityAttributo> attsRifByFunction = new List<EntityAttributo>();
            //        attsRifByFunction.Add(attNewValore);

            //        sectionUpdater.ClearCalculatedValues();
            //        sectionUpdater.CalcolaAttributiGuidCollection(ent);
            //        sectionUpdater.CalcolaAttributiRiferimento(ent);
            //        sectionUpdater.CalcolaAttributiValues(ent, attNewValore, attsRifByFunction);

            //        CalcolaDependentEntityTypes(ent, error);
            //    }



            //    return true;
            //}




            //private void CalcolaDependentEntityTypes(Entity ent, EntitiesError error)
            //{
            //    //Calcolo le sezioni dipendenti
            //    IEnumerable<string> dependentTypes = GetDependentEntityTypesKey(ent.EntityTypeCodice);

            //    foreach (string entTypeKey in dependentTypes)
            //    { 
            //        SectionEntityAttributiUpdater sectionUpdaterRef = GetSectionUpdater(entTypeKey);

            //        IEnumerable<Entity> entities = GetReferencedEntities(entTypeKey, ent.EntityId, ent.EntityTypeCodice);

            //        sectionUpdaterRef.CalcolaEntities(entTypeKey, entities, error);

            //        CalcolaDependentEntityTypes(entTypeKey, entities, error);
            //    }
            //}

            ///// <summary>
            ///// Funzione ricorsiva
            ///// </summary>
            ///// <param name="entityTypeKey"></param>
            ///// <param name="entities"></param>
            //public void CalcolaDependentEntityTypes(string entityTypeKey, IEnumerable<Entity> entities, EntitiesError error = null)
            //{
            //    if (string.IsNullOrEmpty(entityTypeKey))
            //        return;


            //    //Calcolo le sezioni dipendenti
            //    IEnumerable<string> dependentTypes = GetDependentEntityTypesKey(entityTypeKey);

            //    foreach (string depEntTypeKey in dependentTypes)
            //    {
            //        SectionEntityAttributiUpdater sectionUpdaterRef = GetSectionUpdater(depEntTypeKey);

            //        //Aggiorno gli attributi di tipo GuidCollection
            //        IEnumerable<Entity> allEntities = _projectService.GetEntities(depEntTypeKey);
            //        foreach (Entity ent in allEntities)
            //            sectionUpdaterRef.CalcolaAttributiGuidCollection(ent);

            //        IEnumerable<Entity> depEntities = new HashSet<Entity>(entities.SelectMany(item => GetReferencedEntities(depEntTypeKey, item.EntityId, item.EntityTypeCodice)));
            //        sectionUpdaterRef.CalcolaEntities(depEntTypeKey, new EntityCalcOptions(true), depEntities.Select(x => x.EntityId), error);

            //        CalcolaDependentEntityTypes(depEntTypeKey, depEntities, error);
            //    }

            //}

            /// <summary>
            /// metodo ricorsivo
            /// </summary>
            /// <param name="entityTypeKey"></param>
            /// <param name="entities"></param>
            /// <param name="allReferencedEntsId"></param>
            private void GetAllReferencedEntities(string entityTypeKey, IEnumerable<Guid> entitiesId, ref Dictionary<Guid, Entity> allReferencedEntsId)
        {
            if (string.IsNullOrEmpty(entityTypeKey))
                return;


            //Calcolo le sezioni dipendenti
            IEnumerable<string> dependentTypes = GetDependentEntityTypesKey(entityTypeKey);

            foreach (string depEntTypeKey in dependentTypes)
            {
                SectionEntityAttributiUpdater sectionUpdaterRef = GetSectionUpdater(depEntTypeKey);

                IEnumerable<Entity> depEntities = new HashSet<Entity>(entitiesId.SelectMany(item => GetReferencedEntities(depEntTypeKey, item, entityTypeKey)));

                foreach (Entity ent in depEntities)
                    allReferencedEntsId.TryAdd(ent.EntityId, ent);

                GetAllReferencedEntities(depEntTypeKey, depEntities.Select(x=>x.EntityId), ref allReferencedEntsId);
            }

        }

        /// <summary>
        /// Ricavo le entità di tipo entityTypeCodice che sono riferite a questo ent
        /// </summary>
        /// <param name="depEntityTypeKey"></param>
        /// <param name="ent"></param>
        /// <returns></returns>
        private IEnumerable<Entity> GetReferencedEntities(string depEntityTypeKey, Guid entId, string entTypeKey)
        {
            var entTypes = _projectService.GetEntityTypes();

            if (entTypeKey == BuiltInCodes.EntityType.Variabili)
            {
                EntityType entType = _projectService.GetEntityTypes()[depEntityTypeKey];
                if(entType.Attributi.Values.FirstOrDefault(item => item.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Variabile) != null)
                    return _projectService.GetEntities(depEntityTypeKey);

                return new List<Entity>();
            }

            return _projectService.GetEntities(depEntityTypeKey).Where(item =>
            {
                item.ResolveReferences(entTypes);


                IEnumerable<EntityAttributo> elAttsGuid = item.Attributi.Where(entAtt => (entAtt.Value.Attributo.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid || entAtt.Value.Attributo.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.GuidCollection)
                                                                                            && entAtt.Value.Attributo.GuidReferenceEntityTypeKey == entTypeKey)
                                                                        .Select(entAtt => entAtt.Value);
                foreach (EntityAttributo elAttGuid in elAttsGuid)
                {
                    if (elAttGuid.Attributo.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid)
                    {
                        if (item.GetAttributoGuidId(elAttGuid.AttributoCodice) == entId)
                            return true;
                    }
                    else if (elAttGuid.Attributo.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.GuidCollection)
                    {
                        ValoreGuidCollection valGuidCollection = elAttGuid.Valore as ValoreGuidCollection;
                        if (valGuidCollection != null && valGuidCollection.HasTesto(entId.ToString()))
                            return true;
                    }
                }
                return false;
            }).ToList();
        }


        public List<string> GetDependentEntityTypesKey(string entityTypeCodice)
        {
            EntitiesHelper entitiesHelper = new EntitiesHelper(_projectService as ProjectService);
            return entitiesHelper.GetDependentEntityTypesKey(entityTypeCodice);
        }

        //internal void CalcolaEntities(string entityTypeKey, EntityCalcOptions options, List<Guid> entitiesId = null, EntitiesError error = null)
        //{
        //    Debug.Print(System.Reflection.MethodBase.GetCurrentMethod().ToString());

        //    Dictionary<string, EntityType> entTypes = _projectService.GetEntityTypes();

        //    if (!entTypes.ContainsKey(entityTypeKey))
        //        return;

        //    SectionEntityAttributiUpdater sectionUpdater = GetSectionUpdater(entityTypeKey);

        //    if (sectionUpdater == null)
        //        return;

        //    sectionUpdater.CalcolaEntities(entityTypeKey, options, entitiesId, error);

        //    IEnumerable<Entity> entities = null;
        //    if (entitiesId == null)
        //        entities = _projectService.GetEntities(entityTypeKey);
        //    else
        //        entities = _projectService.GetEntitiesById(entityTypeKey, entitiesId);

        //    CalcolaDependentEntityTypes(entityTypeKey, entities, error);

        //    //oss: non ritorno gli id di altre sezioni perchè non sapendo di quali sezioni non saprei cosa farmene
        //    //_entityIdsChanged = sectionUpdater.EntityIdsChanged;
        //}

        private SectionEntityAttributiUpdater GetSectionUpdater(string entityTypeKey)
        {
            if (entityTypeKey == null)
                return null;

            var entTypes = _projectService.GetEntityTypes();

            EntityType entType = entTypes[entityTypeKey];

            SectionEntityAttributiUpdater sectionUpdater = null;

            if (entType is DivisioneItemType && _sectionUpdaters.ContainsKey(BuiltInCodes.EntityType.Divisione))
                sectionUpdater = _sectionUpdaters[BuiltInCodes.EntityType.Divisione];
            else if (_sectionUpdaters.ContainsKey(entityTypeKey))
                sectionUpdater = _sectionUpdaters[entityTypeKey];

            return sectionUpdater;
        }

        public void ClearEntitiesChanged()
        {
            foreach (var sectionUpdater in _sectionUpdaters.Values)
            {
                sectionUpdater.EntityIdsChanged.Clear();
            }
        }
        public HashSet<Guid> GetEntitiesChanged(string entityTypeKey = null)
        {
            if (entityTypeKey != null)
            {
                SectionEntityAttributiUpdater sectionUpdater = GetSectionUpdater(entityTypeKey);
                return sectionUpdater.EntityIdsChanged;
            }
            else
            {
                HashSet<Guid> entityIdsChanged = new HashSet<Guid>();
                foreach (var sectionUpdater in _sectionUpdaters.Values)
                {
                    entityIdsChanged.UnionWith(sectionUpdater.EntityIdsChanged);
                }
                return entityIdsChanged;
            }
        }
    }

    public class SectionEntityAttributiUpdater
    {
        protected ProjectServiceBase _projectService = null;
        protected EntitiesHelper _entitiesHelper = null;
        protected ValoreCalculatorFunction _calculatorFunction = null;
        protected ProjectService ProjectService { get => _projectService as ProjectService;}

        //scopo: tener traccia delle entità modificate
        public HashSet<Guid> EntityIdsChanged { get; protected set; } = new HashSet<Guid>();

        //scopo: memorizzare il risultato dei filtri per l'attributo di tipo GuidCollection (key: FilterData)
        //protected Dictionary<string, List<Guid>> GuidCollectionsCalculated { get; private set; } = new Dictionary<string, List<Guid>>();


        public SectionEntityAttributiUpdater(ProjectServiceBase projectService)
        {
            _projectService = projectService;
            _entitiesHelper = new EntitiesHelper(_projectService as ProjectService);
        }



        public void ClearCurrentEntityCalculatedValues()
        {
            if (_calculatorFunction != null)
                _calculatorFunction.ClearCurrentEntityCalculatedValue();

            //oss: se la metto cancella i risultati di GuidCollection by Filter al ricalcolo di ogni entità
            //GuidCollectionsCalculated.Clear();
        }

        public void ClearCalculatedValues()
        {
            if (_calculatorFunction != null)
                _calculatorFunction.ClearCalculatedValue();

            //GuidCollectionsCalculated.Clear();
            ProjectService.Calculator.ClearGuidCollectionsCalculated();
        }


        public void CalcolaAttributiRiferimento(Entity entity)
        {
            //Calcolo tutti i valori di tipo riferimento

            //oss: nelle divisoni non è necessario calcolare i riferimenti perchè non possono essercene
            if (_calculatorFunction != null)
            {
                //ricavo le funzioni da cercare negli attributi di tipo numero
                //foreach (string attRifKey in entity.Attributi.Keys)
                var atts = _entitiesHelper.GetAttributi(entity);
                foreach (string attRifKey in atts.Keys)
                {
                    Attributo att = atts[attRifKey];
                    AttributoRiferimento attRif = att as AttributoRiferimento;
                    if (attRif != null || att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Variabile)
                    {
                        string et = att.Etichetta;

                        if (et == "Totale Lavori+Sicurezza")
                        { }

                        Valore val = _entitiesHelper.GetValoreAttributo(entity, attRifKey, false, false);
                        _calculatorFunction.AddCalculatedValue(entity.EntityId, et, val);
                        
                    }


                }

            }
        }

        /// <summary>
        /// Calcola attributi ent + Parents + collegati all'interno della sezione
        /// </summary>
        /// <param name="ent"></param>
        /// <returns></returns>
        public virtual bool CalcolaAttributiValues(Entity ent)
        {

            List<EntityAttributo> attsOrdered = OrderEntityAttributiByFunctions(ent);

            foreach (EntityAttributo entAtt in attsOrdered)
            {
                if (entAtt.Attributo.Etichetta == "Descrizione quantità")
                { }


                bool isCalculated = CalcolaAttributiBuiltIn(ent, entAtt);
                if (!isCalculated)
                    _projectService.Calculator.Calculate(ent, entAtt.Attributo, entAtt.Valore);

                if (_calculatorFunction != null)
                {
                    string et = entAtt.Attributo.Etichetta;
                    _calculatorFunction.AddCalculatedValue(entAtt.Entity.EntityId, et, entAtt.Valore);
                }
            }




            return true;
        }

        protected virtual bool CalcolaAttributiBuiltIn(Entity ent, EntityAttributo attNewValore) { return false; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ent"></param>
        /// <param name="functions"></param>
        /// <param name="attsRifByFunction"> eventualmente ritorna gli attributi che sono stati ricalcolati</param>
        protected virtual void CalcolaAttributiRifByFunction(Entity ent, Queue<string> functions, List<EntityAttributo> attsRifByFunction = null)
        {
            //Calcolo tutti gli attributi che contengono funzioni
            foreach (EntityAttributo entAtt in ent.Attributi.Values)
            {
                if (entAtt.Attributo.IsInternal)
                    continue;


                if (entAtt.Attributo.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Reale
                    || entAtt.Attributo.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Contabilita
                    || entAtt.Attributo.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Testo)
                {

                    //if (functions.Any() && entAtt.Valore.ContainsTesto(functions.Peek()))
                    //{
                    //    string function = CalculatorExpression.CreateFormula(ent.EntityType.FunctionName, entAtt.Attributo.Etichetta);

                    //    if (!functions.Contains(function))
                    //    {
                    //        functions.Enqueue(function);
                    //        if (attsRifByFunction != null)
                    //            attsRifByFunction.Add(entAtt);

                    //        CalcolaAttributiValues(ent, entAtt, functions);
                    //    }
                    //}
                    List<string> funcs = new List<string>(functions);
                    foreach (string func in funcs)
                    {
                        if (entAtt.Valore.ContainsTesto(func))
                        {
                            string function = CalculatorExpression.CreateFormula(ent.EntityType.FunctionName, entAtt.Attributo.Etichetta);

                            if (!functions.Contains(function))
                            {
                                functions.Enqueue(function);
                                if (attsRifByFunction != null)
                                    attsRifByFunction.Add(entAtt);

                                CalcolaAttributiValues(ent, entAtt, functions);
                            }
                        }
                    }
                }
            }
        }

        public virtual bool CalcolaAttributiValues(Entity ent, EntityAttributo attNewValore, Queue<string> functions, List<EntityAttributo> attsRifByFunction = null)
        {
            _projectService.Calculator.Calculate(ent, attNewValore.Attributo, attNewValore.Valore);

            CalcolaAttributiRifByFunction(ent, functions, attsRifByFunction);

            functions.Dequeue();//.Pop();



            return true;
        }

        /// <summary>
        /// calcola attNewValore e tutti gli attributi di ent che dipendono da attNewValore
        /// </summary>
        /// <param name="ent"></param>
        /// <param name="attNewValore"></param>
        /// <param name="attsRifByFunction">lista degli attributi ricalcolati</param>
        /// <returns></returns>
        public virtual bool CalcolaAttributiValues(Entity ent, EntityAttributo attNewValore, List<EntityAttributo> attsRifByFunction = null)
        {
            //Nel caso sia stato cambiato un id (Articolo)
            //aggiungere alle funzioni le funzioni degli attributi che dipendono da questo articolo
            if (attNewValore.Attributo.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid
                && !attNewValore.Attributo.IsInternal)
            {
                //foreach (EntityAttributo entAtt in ent.Attributi.Values)
                foreach( Attributo att in ent.EntityType.Attributi.Values)
                {
                    //AttributoRiferimento attRif = entAtt.Attributo as AttributoRiferimento;
                    //if (attRif == null)
                    //    continue;

                    AttributoRiferimento attRif = att as AttributoRiferimento;
                    if (attRif == null)
                        continue;


                    if (attRif.ReferenceCodiceGuid != attNewValore.AttributoCodice)
                        continue;


                    Queue<string> functions = new Queue<string>();
                    string func = CalculatorExpression.CreateFormula(ent.EntityType.FunctionName, attRif.Etichetta);
                    functions.Enqueue(func);

                    CalcolaAttributiValues(ent, attNewValore, functions, attsRifByFunction);
                }
            }
            else
            {
                Queue<string> functions = new Queue<string>();
                string function = CalculatorExpression.CreateFormula(ent.EntityType.FunctionName, attNewValore.Attributo.Etichetta);
                functions.Enqueue(function);

                CalcolaAttributiValues(ent, attNewValore, functions, attsRifByFunction);
            }

            //evidenziatore dell'entità
            UpdateHighlighterColorName(ent);

            return true;
        }

        public virtual void CalcolaEntities(string entityTypeKey, EntityCalcOptions options, IEnumerable<Guid> entitiesId = null, EntitiesError error = null)
        {    
            try
            {

                var validEntsId = entitiesId.Where(x => _projectService.IsEntityValid(entityTypeKey, x));


                IEnumerable<Entity> entities = null;
                if (entitiesId == null)
                    entities = _projectService.GetEntities(entityTypeKey);
                else
                    entities = _projectService.GetEntitiesById(entityTypeKey, validEntsId);

                //IEnumerable<string> form = null;
                if (options.ResetCalulatedValues)
                    ClearCalculatedValues();


                List<Entity> refEnts = OrderSectionEntitesByReference(entityTypeKey, entities, error);

                int entCount = 0;

                foreach (Entity ent in refEnts)
                {
                    ClearCurrentEntityCalculatedValues();

                    CalcolaAttributiGuid(ent);
                    
                    CalcolaAttributiGuidCollection(ent);

                    CalcolaAttributiRiferimento(ent);

                    if (options.CalcolaAttributiResults)
                        CalcolaAttributiValues(ent);

                    //evidenziatore dell'entità
                    UpdateHighlighterColorName(ent);

                    //form = _projectService.Calculator.CalculatorExpression._results.Select(item => item.Formula);

                    EntityIdsChanged.Add(ent.EntityId);

                    entCount++;
                }

                if (error != null && error.ActionErrorType == ActionErrorType.LOOP_REFERENCE)
                    CalcolaLoopEntities(entityTypeKey, error);

            }
            catch (Exception ex)
            {
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex.Message);
            }

            //if (entityTypeKey == BuiltInCodes.EntityType.Computo)
            //{
            //    if (error != null)
            //        error.Ids = entities.Select(item => item.EntityId).ToHashSet();

            //    CalcolaLoopEntities(entityTypeKey, error);
            //}
            //else
            //{

            //    //for (int i = 0; i < orderedEntities.Count; i++)
            //    foreach (Entity ent in entities)
            //    {
            //        //Entity ent = orderedEntities[i];

            //        ClearCalculatedValues();
            //        CalcolaAttributiGuidCollection(ent);
            //        CalcolaAttributiRiferimento(ent);

            //        if (options.CalcolaAttributiResults)
            //        {
            //            CalcolaAttributiValues(ent);
            //        }

            //        //evidenziatore dell'entità
            //        UpdateHighlighterColorName(ent);

            //    }
            //}




            ////sono state aggiornate tutte e sole le entità da aggiornare
            //if (entitiesId != null)
            //    EntityIdsChanged = new HashSet<Guid>(entitiesId);
        }

        protected bool IsAnyHighlighter(string entityTypeKey)
        {
            string highlighterColorName = MyColorsEnum.Transparent.ToString();
            ViewSettings viewSettings = ProjectService.GetViewSettings();

            EntityTypeViewSettings entTypeViewSettings = null;
            if (viewSettings.EntityTypes.TryGetValue(entityTypeKey, out entTypeViewSettings))
            {
                if (entTypeViewSettings.EntityHighlighters != null)
                {
                    string codiceAtt = entTypeViewSettings.EntityHighlighters.CodiceAttributo;

                    if (!string.IsNullOrEmpty(codiceAtt))
                        return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Aggiornamento del colore dell'evidenziatore per l'entità
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="codiceAttributoHighlighter">Aggiornamento viene effettuato soo se è stato modificato l'attributo codiceAttributoHighlighter</param>
        protected void UpdateHighlighterColorName(Entity entity)
        {
            string highlighterColorName = MyColorsEnum.Transparent.ToString();
            ViewSettings viewSettings = ProjectService.GetViewSettings();

            EntityType entType = entity.EntityType;

            EntityTypeViewSettings entTypeViewSettings = null;
            if (viewSettings.EntityTypes.TryGetValue(entType.GetKey(), out entTypeViewSettings))
            {
                if (entTypeViewSettings.EntityHighlighters != null)
                {
                    string codiceAtt = entTypeViewSettings.EntityHighlighters.CodiceAttributo;

                    if (!string.IsNullOrEmpty(codiceAtt))
                    {
                        string value = _entitiesHelper.GetValorePlainText(entity, codiceAtt, false, false);

                        if (!string.IsNullOrEmpty(value))
                        {
                            Attributo att = null;
                            if (entType.Attributi.TryGetValue(codiceAtt, out att))
                            {
                                if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Colore)
                                {
                                    highlighterColorName = value;
                                }
                                else
                                {
                                    ValoreHighlighter valHighlighter = null;
                                    if (entTypeViewSettings.EntityHighlighters.Highlighters.TryGetValue(value, out valHighlighter))
                                    {
                                        highlighterColorName = valHighlighter.Colore.PlainText;
                                    }
                                }
                            }
                        }
                        else
                        {
                            ValoreHighlighter valHighlighter = null;
                            if (entTypeViewSettings.EntityHighlighters.Highlighters.TryGetValue(ValoreHelper.ValoreNullAsText, out valHighlighter))
                            {
                                highlighterColorName = valHighlighter.Colore.PlainText;
                            }
                        }
                    }
                }
            }

            entity.HighlighterColorName = highlighterColorName;
        }

        /// <summary>
        /// Ordina gli attributi dell'entità per l'aggiornamento in base alle formule
        /// </summary>
        /// <param name="ent"></param>
        /// <returns></returns>
        protected List<EntityAttributo> OrderEntityAttributiByFunctions(Entity ent)
        {
            List<EntityAttributo> orderedList = new List<EntityAttributo>();

            //AttributoDependencyComparer attDepComparer = new AttributoDependencyComparer(_projectService, ent);
            IEnumerable<EntityAttributo> entAtts = ent.Attributi.Values;

            List<AttributoFunctionsDependency> funcAtts = entAtts.Select(item => new AttributoFunctionsDependency()
            {
                Etichetta = item.Attributo.Etichetta,
                Codice = item.AttributoCodice,
                Function = string.Format("att{{{0}}}", item.Attributo.Etichetta),
                IsRiferimento = item.Attributo is AttributoRiferimento,
            }).ToList();



            List<string> functions = funcAtts.Select(item => item.Function).ToList();

            foreach (AttributoFunctionsDependency funcAtt in funcAtts)
            {
                if (funcAtt.IsRiferimento)
                    continue;

                Valore val =  _projectService.GetValoreAttributo(ent, funcAtt.Codice);

                if (val != null)
                {
                    funcAtt.Formula = val.GetFormula();

                    foreach (AttributoFunctionsDependency funcAtt1 in funcAtts)
                    {
                        if (funcAtt.Formula != null && funcAtt.Formula.Contains(funcAtt1.Function))
                        {
                            //funcAtt dipende da funcAtt1
                            funcAtt.DependBy.Add(funcAtt1);
                        }
                    }
                }
            }

            //Calcolo i livelli (di dipendenza)
            foreach (AttributoFunctionsDependency funcAtt in funcAtts)
            {
                funcAtts.ForEach(item => item.IsLoopLevel = false);
                funcAtt.Level = funcAtt.GetLevel();
            }


            foreach (AttributoFunctionsDependency funcAtt in funcAtts.OrderBy(item => item.Level))
            {
                EntityAttributo entAtt = ent.Attributi[funcAtt.Codice];
                orderedList.Add(entAtt);
            }

            return orderedList;
        }

        //Ordina le entità della sezione per l'aggiornamento in base alle referenze interne alla sezione
        public List<Entity> OrderSectionEntitesByReference(string entityTypeKey, IEnumerable<Entity> entities = null, EntitiesError error = null)
        {





            List<Entity> orderedList = new List<Entity>();

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //le entità vengono ordinate nel caso di riferimenti alla stessa sezione o multi-riferimenti alla stessa sezione byHand
            var entTypes = ProjectService.GetEntityTypes();
            EntityType entType = null;
            if (entTypes.TryGetValue(entityTypeKey, out entType))
            {
                bool anyAttsSelfGuid = entType.Attributi.Values.Any(item => item.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid && item.GuidReferenceEntityTypeKey == entityTypeKey);

                bool anyAttsSelfGuidCollByHand = false;
                foreach (var att in entType.Attributi.Values.Where(item => item.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.GuidCollection && item.GuidReferenceEntityTypeKey == entityTypeKey))
                {
                    if (att.ValoreAttributo is ValoreAttributoGuidCollection attGuidCollSettings)
                    {
                        if (attGuidCollSettings.ItemsSelectionType == ItemsSelectionTypeEnum.ByHand)
                        {
                            anyAttsSelfGuidCollByHand = true;
                            break;
                        }
                    }
                }  
                    
                if (!anyAttsSelfGuid && !anyAttsSelfGuidCollByHand)
                {
                    if (entities != null)
                        orderedList = entities.ToList();

                    return orderedList;
                }
            }
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            //old
            //List<string> depEntTypesKey = _entitiesHelper.GetDependentEntityTypesKey(entityTypeKey);

            //if (!depEntTypesKey.Contains(entityTypeKey))
            //{
            //    //Sezione per cui non è previsto il riferimento di entità all'interno della stessa sezione
            //    orderedList = entities.ToList();
            //    return orderedList;
            //}


            
            IEnumerable<Entity> ents = ProjectService.GetEntities(entityTypeKey);

            Dictionary<Guid, EntityReferenceDependency> entRefsByEntityId = ents.ToDictionary(item => item.EntityId, item => new EntityReferenceDependency() { Entity = item});

            //raccolgo gli id riferiti tramite ValoreGuid e ValoreGuidCollection ByHand
            //oss: i ValoreGuidCollection All e ByFilter vengono calcolati separatamente quindi in questi casi non è necessario ordinare le entità 
            foreach (var entRef in entRefsByEntityId.Values)
            {


                IEnumerable<EntityAttributo> elAttsGuid = entRef.Entity.Attributi.Where(entAtt => (entAtt.Value.Attributo.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid || entAtt.Value.Attributo.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.GuidCollection)
                                                                            && entAtt.Value.Attributo.GuidReferenceEntityTypeKey == entityTypeKey)
                                                        .Select(entAtt => entAtt.Value);

                foreach (EntityAttributo elAttGuid in elAttsGuid)
                {
                    if (elAttGuid.Attributo.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid)
                    {
                        Guid refId = entRef.Entity.GetAttributoGuidId(elAttGuid.AttributoCodice);

                        if (refId != Guid.Empty)
                        {
                            if (entRefsByEntityId.ContainsKey(refId))
                                entRef.AddDependBy(entRefsByEntityId[refId]);
                        }
                        
                    }
                    else if (elAttGuid.Attributo.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.GuidCollection)
                    {
                        if (elAttGuid.Attributo.ValoreAttributo is ValoreAttributoGuidCollection valAttGuidColl)
                        {
                            if (valAttGuidColl.ItemsSelectionType == ItemsSelectionTypeEnum.ByHand)
                            {
                                ValoreGuidCollection valGuidCollection = elAttGuid.Valore as ValoreGuidCollection;
                                List<Guid> refIds = valGuidCollection.GetEntitiesId();


                                entRef.AddDependBy(refIds
                                    .Where(item => item != Guid.Empty && entRefsByEntityId.ContainsKey(item))
                                    .Select(item => entRefsByEntityId[item]));
                            
                            }
                        
                        }
                    }
                }
            }


            HashSet<EntityReferenceDependency> entRefs = new HashSet<EntityReferenceDependency>();

            if (entities == null)//tutte le entità
            {
                entRefs = entRefsByEntityId.Values.ToHashSet();
            }
            else
            {
                foreach (Entity ent in entities)
                {
                    var entRef = entRefsByEntityId[ent.EntityId];
                    entRef.ClearLoops();
                    entRefs.UnionWith(entRef.GetDependedEntities());
                }

            }



            //Calcolo i livelli (di dipendenza)
            foreach (EntityReferenceDependency entRef in entRefs)
            {
                entRef.ClearLoops();
                entRef.Level = entRef.GetLevel();
            }

            orderedList = entRefs.Where(item => item.Level >= 0).OrderBy(item => item.Level).Select(item => item.Entity).ToList();

            HashSet<EntityReferenceDependency> loopList = entRefs.Where(item => item.Level < 0).ToHashSet();

            if (loopList.Any() && error != null)
            {
                error.EntityTypeKey = entityTypeKey;
                error.ActionErrorType = ActionErrorType.LOOP_REFERENCE;
                error.Ids = loopList.Select(item => item.Entity.EntityId).ToHashSet();
            }

            return orderedList;
        }

        public virtual void CalcolaAttributiGuid(Entity ent)
        {
            var entTypes = _projectService.GetEntityTypes();

            //Imposto a Guid.Empty gli attributi ValoreGuid il cui id non esiste più
            IEnumerable<EntityAttributo> guidAtts = ent.Attributi.Values.Where(item => item.Attributo.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid)
                .Where(x =>
                {
                    ValoreGuid valGuid = x.Valore as ValoreGuid;

                    if (entTypes.ContainsKey(x.Attributo.GuidReferenceEntityTypeKey))
                    {
                        if (valGuid != null && !_projectService.IsEntityValid(x.Attributo.GuidReferenceEntityTypeKey, valGuid.V))
                            return true;
                    }

                    return false;
                }).ToList();

            guidAtts.ForEach(x => x.Valore = new ValoreGuid());
        }


        public virtual void CalcolaAttributiGuidCollection(Entity ent)
        {
            IEnumerable<EntityAttributo> guidCollAtts = ent.Attributi.Values.Where(item => item.Attributo.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.GuidCollection);
            foreach (EntityAttributo entAtt in guidCollAtts)
            {
                CalcolaAttributoGuidCollection(entAtt);
            }
        }



        public virtual void CalcolaAttributoGuidCollection(EntityAttributo entAtt)
        {
            Attributo att = entAtt.Attributo;
            Entity ent = entAtt.Entity;

            ValoreAttributoGuidCollection valAttGuidColl = att.ValoreAttributo as ValoreAttributoGuidCollection;
            if (valAttGuidColl != null)
            {
                if (valAttGuidColl.ItemsSelectionType == ItemsSelectionTypeEnum.ByHand)
                {
                    //controllo se gli id nella collezione sono ancora validi o sono stati eliminati
                    string entTypeKeyRef = att.GuidReferenceEntityTypeKey;
                    ValoreGuidCollection val = _entitiesHelper.GetValoreAttributo(ent, att.Codice, false, false) as ValoreGuidCollection;
                    var ids = val.GetEntitiesId();

                    //cancello gli id non più validi
                    var idsToRemove = ids.Where(x => !_projectService.IsEntityValid(entTypeKeyRef, x)).ToHashSet();
                    if (idsToRemove.Any())
                        val.RemoveEntitiesId(idsToRemove);

                }
                else if (valAttGuidColl.ItemsSelectionType == ItemsSelectionTypeEnum.All)
                {
                    string entTypeKeyRef = att.GuidReferenceEntityTypeKey;

                    FilterData filterData = null;
                    List<Guid> ids = CalculateFilter(entTypeKeyRef, filterData);

                    //if (DeveloperVariables.IsUnderConstruction)
                    //{
                        //NB: non viene salvato il risultato del filtro
                        //entAtt.Valore = new ValoreGuidCollection() { V = ids.Select(item => new ValoreGuidCollectionItem() { EntityId = item }).Cast<ValoreCollectionItem>().ToList() };
                        ValoreGuidCollection valGuidColl = entAtt.Valore as ValoreGuidCollection;
                        valGuidColl.FilterResult = ids.Select(item => new ValoreGuidCollectionItem() { EntityId = item }).Cast<ValoreCollectionItem>().ToList(); 
                    //}
                    //else
                    //{
                    //    entAtt.Valore = new ValoreGuidCollection() { V = ids.Select(item => new ValoreGuidCollectionItem() { EntityId = item }).Cast<ValoreCollectionItem>().ToList() };
                    //}
                }
                else if (valAttGuidColl.ItemsSelectionType == ItemsSelectionTypeEnum.ByFilter)
                {
                    string entTypeKeyRef = att.GuidReferenceEntityTypeKey;

                    ValoreGuidCollection val = _entitiesHelper.GetValoreAttributo(ent, att.Codice, false, false) as ValoreGuidCollection;

                    ////////////////////////////////
                    //Imposto i valori per i ValoreAsItem

                    FilterData filterDataClone = val.Filter.Clone();
                    var attConds = filterDataClone.Items.Where(item => item.FilterType == FilterTypeEnum.Conditions)
                                    .SelectMany(item => item.ValoreConditions.MainGroup.Conditions)
                                    .Where(item => item is AttributoValoreConditionSingle)
                                    .Select(item => item as AttributoValoreConditionSingle)
                                    .Where(item => item.ValoreConditionSingle != null && item.ValoreConditionSingle.Valore != null && item.ValoreConditionSingle.Valore.PlainText == ValoreHelper.ValoreAsItem);

                    foreach (AttributoValoreConditionSingle attCond in attConds)
                    {
                        Valore valEnt = _entitiesHelper.GetValoreAttributo(ent, attCond.CodiceAttributo, false, false);
                        attCond.ValoreConditionSingle.Valore = valEnt;
                    }

                    ////////////////////////////////

                    //calcolo il risultato del filtro
                    List<Guid> ids = CalculateFilter(entTypeKeyRef, filterDataClone);


                    //if (DeveloperVariables.IsUnderConstruction)
                    //{

                        List<ValoreCollectionItem> valColls = new List<ValoreCollectionItem>();
                        ids.ForEach(item => valColls.Add(new ValoreGuidCollectionItem() { EntityId = item }));

                        entAtt.Valore = new ValoreGuidCollection()
                        {
                            //NB: non viene salvato il risultato del filtro
                            //V = ids.Select(item => new ValoreGuidCollectionItem() { EntityId = item }).Cast<ValoreCollectionItem>().ToList(),
                            Filter = val.Filter,
                            FilterResult = valColls,
                        };
                    //}
                    //else
                    //{ 

                    //    entAtt.Valore = new ValoreGuidCollection()
                    //    {
                    //        //NB: non viene salvato il risultato del filtro
                    //        //V = ids.Select(item => new ValoreGuidCollectionItem() { EntityId = item }).Cast<ValoreCollectionItem>().ToList(),
                    //        Filter = val.Filter,
                    //        FilterResult = ids.Select(item => new ValoreGuidCollectionItem() { EntityId = item }).Cast<ValoreCollectionItem>().ToList(),
                    //    };
                    //}




                }
            }
        }

        //public  List<Guid> CalculateFilter_old(string entityTypeKey, FilterData filterData)
        //{
        //    List<Guid> ids = null;

        //    //filterData.Items.ForEach(item => item.FoundEntitiesId = null);

        //    string filterDataSerialized = "";
        //    JsonSerializer.JsonSerialize(filterData, out filterDataSerialized);


        //    if (GuidCollectionsCalculated.ContainsKey(filterDataSerialized))
        //    {
        //        ids = GuidCollectionsCalculated[filterDataSerialized];
        //    }
        //    else
        //    {

        //        ProjectService.GetFilteredEntities(entityTypeKey, filterData, null, null, out ids);

        //        if (filterData != null)
        //            filterData.Items.ForEach(item => item.FoundEntitiesId = null);
                
        //        GuidCollectionsCalculated.Add(filterDataSerialized, ids);
        //    }
            



        //    return ids;
        //}

        public List<Guid> CalculateFilter(string entityTypeKey, FilterData filterData)
        {
            List<Guid> ids = null;


            ids = _projectService.Calculator.GetGuidCollectionsCalculatedIds(filterData);
            if (ids == null)
            {
                if (filterData != null)
                    filterData.Items.ForEach(item => item.FoundEntitiesId = null);

                ProjectService.GetFilteredEntities(entityTypeKey, filterData, null, null, out ids);

                if (ids == null || ids.Count == 0)
                {
                    int p = 0;
                }


                if (filterData != null)
                    filterData.Items.ForEach(item => item.FoundEntitiesId = null);

                _projectService.Calculator.SetGuidCollectionsCalculatedFilterIds(filterData, ids);
            }


            if (ids == null || ids.Count == 0)
            {
                int p = 0;
            }

            return ids;
        }


        /// <summary>
        /// Metodo per il calcolo degli attributi di entità che sono risulatate in loop
        /// </summary>
        /// <param name="entityTypeKey"></param>
        /// <param name="ids"></param>
        internal void CalcolaLoopEntities(string entityTypeKey, EntitiesError error)
        {

            

            if (error == null || error.Ids == null || !error.Ids.Any())
                return;

            ProjectService.OnProgressChanged(new ProgressEventArgs() { ProgressValue = 10, Message = LocalizationProvider.GetString("Ricalcolo in corso (voci riferite)...") });

            //Key:AttributoEntityId
            HashSet<string> attributiCalcolati = new HashSet<string>();

            //Key:AttributoEntityId
            Dictionary<string, Valore> valoriCalcolati = new Dictionary<string, Valore>();
            HashSet<string> dependentAttByFunction = new HashSet<string>();

            List<Entity> ents = _projectService.GetEntitiesById(entityTypeKey, error.Ids);



            /////////////////////////////////////////////////////////////////////////////////////////
            ///Scorro le entità e riempio gli attributi da calcolare ordinati per funzione

            //Key:AttributoEntityId
            Dictionary<string, EntityAttributo> attributiDaCalcolare = new Dictionary<string, EntityAttributo>();
            foreach (Entity ent in ents)
            {
                _calculatorFunction.ClearCurrentEntityCalculatedValue();
                CalcolaAttributiRiferimento(ent);

                List<EntityAttributo> attsOrdered = OrderEntityAttributiByFunctions(ent);

                dependentAttByFunction.Clear();

                foreach (EntityAttributo entAtt in attsOrdered)
                {
                    //AttributoEntityId attId = new AttributoEntityId() { Entities = ent.EntityId.ToString(), CodiceAttributo = entAtt.AttributoCodice, EntityTypeKey = entityTypeKey, EtichettaAttributo = entAtt.Attributo.Etichetta };
                    string attKey = AttributoEntityId.GetKey(entityTypeKey, ent.EntityId.ToString(), entAtt.Attributo.Etichetta, entAtt.AttributoCodice);


                    if (_entitiesHelper.IsAttributoRiferimento(entAtt.Attributo, entityTypeKey))
                    {
                        //Attributo riferimento riferito alla medesima sezione
                        attributiDaCalcolare.Add(attKey, entAtt);
                        dependentAttByFunction.Add(string.Format("att{{{0}}}", entAtt.Attributo.Etichetta));
                    }
                    else if (dependentAttByFunction.Any(s => entAtt.Valore.PlainText.Contains(s)))
                    {
                        //attributo che dipende (by function) da un attributo di riferimento (anche in maniera indiretta) nella stessa voce
                        attributiDaCalcolare.Add(attKey, entAtt);
                        dependentAttByFunction.Add(string.Format("att{{{0}}}", entAtt.Attributo.Etichetta));
                    }
                    else
                    {
                        bool isCalculated = CalcolaAttributiBuiltIn(ent, entAtt);
                        if (!isCalculated)
                            _projectService.Calculator.Calculate(ent, entAtt.Attributo, entAtt.Valore);

                        if (_calculatorFunction != null)
                        {
                            string et = entAtt.Attributo.Etichetta;
                            _calculatorFunction.AddCalculatedValue(entAtt.Entity.EntityId, et, entAtt.Valore);
                        }
                        attributiCalcolati.Add(attKey);
                    }
                }
            }

            ProjectService.OnProgressChanged(new ProgressEventArgs() { ProgressValue = 20, Message = LocalizationProvider.GetString("Ricalcolo in corso...") });
            //_calculatorFunction.ClearCalculatedValue();

            

            int attributiDaCalcolareCount = attributiDaCalcolare.Count;
            int iterCount = 0;
            while (iterCount == 0 || attributiDaCalcolare.Count > 0)
            {
                double progressUnit = 80.0 / attributiDaCalcolareCount;
                double progressCount = 0;

                Guid lastEntityId = Guid.Empty;
                bool isEntityChanged = false;

                foreach (string attIdStr in attributiDaCalcolare.Keys)
                {
                    progressCount++;

                    ProjectService.OnProgressChanged(new ProgressEventArgs() { ProgressValue = (int)(20+(progressUnit*progressCount)), Message = LocalizationProvider.GetString("Ricalcolo in corso (voci riferite)...") });

                    AttributoEntityId attId = new AttributoEntityId(attIdStr);

                    //if (attId.EtichettaAttributo == "N° Interventi (=Corrispettivi)" || attId.EtichettaAttributo == "Somma Interventi")
                    //{
                    //    int p = 0;
                    //}

                    var entAtt = attributiDaCalcolare[attIdStr];
                    var ent = entAtt.Entity;

                    if (lastEntityId != ent.EntityId)
                        ClearCurrentEntityCalculatedValues();

                    if (_entitiesHelper.IsAttributoRiferimento(entAtt.Attributo, entityTypeKey))
                    { 
                        //attributo riferita alla sezione medesima

                        AttributoRiferimento attRef = entAtt.Attributo as AttributoRiferimento;


                        //calcolo i riferiti di GuidCollection ByFilter
                        EntityAttributo sourceEntAtt = _entitiesHelper.GetSourceEntityAttributo(ent, attRef.ReferenceCodiceGuid);
                        if (sourceEntAtt.Attributo.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.GuidCollection)
                        {
                            if (sourceEntAtt.Attributo.ValoreAttributo is ValoreAttributoGuidCollection)
                            {
                                var attSettings = sourceEntAtt.Attributo.ValoreAttributo as ValoreAttributoGuidCollection;
                                if (attSettings.ItemsSelectionType == ItemsSelectionTypeEnum.ByFilter)
                                {
                                    ValoreGuidCollection valGuidColl = _entitiesHelper.GetValoreAttributo(ent, attRef.ReferenceCodiceGuid, false, false) as ValoreGuidCollection;
                                    
                                    //oss: affichè si possa fare il filtro delle entità occorre che siano già stati calcolati gli attributi contenuti nel filtro per tutte le entità
                                    HashSet<string> attsCodice = valGuidColl.Filter.Items.Select(item => item.CodiceAttributo).ToHashSet();
                                    if (attributiDaCalcolare.Keys.Any(item => attsCodice.Contains(AttributoEntityId.GetCodiceAttributo(item))))
                                        continue;

                                    CalcolaAttributoGuidCollection(sourceEntAtt);
                                }
                            }

                        }

                        //entità che vengono riferite dall'attributo
                        var entitiesId = _entitiesHelper.GetEntitiesIdReferencedByAttributo(entAtt);

                        string entitiesIdOrdered = string.Join(';', entitiesId.OrderBy(item => item));
                        Attributo refAtt = ent.Attributi[attRef.ReferenceCodiceAttributo].Attributo;


                        var sumAtt = new AttributoEntityId() { Entities = entitiesIdOrdered, CodiceAttributo = attRef.ReferenceCodiceAttributo, EntityTypeKey = attRef.ReferenceEntityTypeKey, EtichettaAttributo = refAtt.Etichetta };
                        if (valoriCalcolati.ContainsKey(sumAtt.To1String()))
                        {
                            //valore già calcolato ad una iterazione precedente
                            entAtt.Valore = valoriCalcolati[sumAtt.To1String()];
                            attributiCalcolati.Add(attId.To1String());
                            attributiDaCalcolare.Remove(attId.To1String());
                        }
                        else
                        {
                            //Valore mai calcolato precedentemente

                            var refAttsId = entitiesId.Select(item => new AttributoEntityId() { Entities = item.ToString(), CodiceAttributo = attRef.ReferenceCodiceAttributo, EntityTypeKey = attRef.ReferenceEntityTypeKey, EtichettaAttributo = refAtt.Etichetta });

                            bool canCalculate = true;
                            foreach (AttributoEntityId refAttId in refAttsId)
                                if (error.Ids.Contains(new Guid(refAttId.Entities)))
                                    if (!attributiCalcolati.Contains(refAttId.To1String()))
                                        canCalculate = false;


                            if (canCalculate)
                            {
                                //È possibile calcolare il valore (sono già stati calcolati tutti i suoi riferiti)

                                Valore val = _entitiesHelper.GetValoreAttributo(ent, entAtt.AttributoCodice, false, false);
                                entAtt.Valore = val;
                                attributiCalcolati.Add(attId.To1String());
                                attributiDaCalcolare.Remove(attId.To1String());

                                valoriCalcolati.Add(new AttributoEntityId()
                                {
                                    Entities = entitiesIdOrdered,
                                    CodiceAttributo = attRef.ReferenceCodiceAttributo,
                                    EntityTypeKey = attRef.ReferenceEntityTypeKey,
                                    EtichettaAttributo = refAtt.Etichetta
                                }.To1String(),
                                val);
                            }
                        }
                    }
                    else
                    {



                        //non posso calcolare se l'attributo riferisce by function ad un altro attributo non ancora calcolato
                        HashSet<string> funzioniNonCalcolate = attributiDaCalcolare.Keys.Where(item => AttributoEntityId.GetEntities(item) == entAtt.Entity.EntityId.ToString()).Select(item => string.Format("att{{{0}}}", AttributoEntityId.GetEtichettaAttributo(item))).ToHashSet();
                        if (!funzioniNonCalcolate.Any(item => entAtt.Valore.PlainText.Contains(item)))
                        {

                            bool isCalculated = CalcolaAttributiBuiltIn(ent, entAtt);
                            if (!isCalculated)
                                _projectService.Calculator.Calculate(ent, entAtt.Attributo, entAtt.Valore);

                            attributiCalcolati.Add(attId.To1String());
                            attributiDaCalcolare.Remove(attId.To1String());
                        }
                    }


                    lastEntityId = ent.EntityId;
                }


                if (attributiDaCalcolareCount <= attributiDaCalcolare.Count)
                {
                    IEnumerable<string> attsImpossibiliDaCalcolare = attributiDaCalcolare.Keys.Select(item => string.Format("{0} {1}", AttributoEntityId.GetEntities(item), AttributoEntityId.GetEtichettaAttributo(item)));
                    break;
                }

                attributiDaCalcolareCount = attributiDaCalcolare.Count;

                iterCount++;
            }

            var loopIds = new HashSet<Guid>(attributiDaCalcolare.Keys.Select(item => new Guid(AttributoEntityId.GetEntities(item))));

            
            foreach (Entity ent in ents.Where(item => !loopIds.Contains(item.EntityId)))
            {
                CalcolaAttributiGuidCollection(ent);
                UpdateHighlighterColorName(ent);
            }
            

            if (!loopIds.Any())
            {
                error.Ids.Clear();
                error.ActionErrorType = ActionErrorType.NOTHING;
            }
            else
                error.Ids = loopIds;


            ProjectService.OnProgressChanged(new ProgressEventArgs() { ProgressValue = 100 });
        }


    }

    /// <summary>
    /// Classe intermedia per il calcolo dell'ordine degli attributi in base alle formule
    /// </summary>
    public class AttributoFunctionsDependency
    {
        public string Etichetta { get; set; } = string.Empty;
        public string Codice { get; set; } = string.Empty;
        public string Function { get; set; } = string.Empty;
        public bool IsRiferimento { get; set; } = false;
        public string Formula { get; set; } = string.Empty;
        public int Level { get; set; } = 0;
        public List<AttributoFunctionsDependency> DependBy { get; set; } = new List<AttributoFunctionsDependency>();

        public bool IsLoopLevel = false;


        public int GetLevel()
        {
            if (IsLoopLevel)
                return 0;

            IsLoopLevel = true;

            int level = 0;
            if (!DependBy.Any())
                level = 0;
            else
                level = DependBy.Max(item => item.GetLevel()) + 1;

            IsLoopLevel = false;
            return level;
        }
        

    }

    /// <summary>
    /// Classe intermedia per il calcolo dell'ordine delle entità in base ai riferimenti
    /// </summary>
    public class EntityReferenceDependency
    {
        public Entity Entity = null;
        public int Level { get; set; } = 0;
        private List<EntityReferenceDependency> _dependBy = new List<EntityReferenceDependency>();

        private List<EntityReferenceDependency> _dependInverseBy  = new List<EntityReferenceDependency>();
        

        static int LevelError { get => -100; }
        static bool _isLoop = false;
        static bool IsLoop { get => _isLoop; }

        static HashSet<EntityReferenceDependency> _currents = new HashSet<EntityReferenceDependency>();
        static public IEnumerable<Entity> LoopEntities
        {
            get
            {
                if (_isLoop)
                    return _currents.Select(item => item.Entity);
                else
                    return null;
            }
        }

        public HashSet<EntityReferenceDependency> GetDependedEntities()
        {
            var dependedEntities = new HashSet<EntityReferenceDependency>();

            AddDependedEntities(this, ref dependedEntities);

            return dependedEntities;
        }

        private void AddDependedEntities(EntityReferenceDependency entRef, ref HashSet<EntityReferenceDependency> dependedEntities)
        {
            if (!dependedEntities.Contains(entRef))
                dependedEntities.Add(entRef);
            else
                return;

            foreach (var depEnt in entRef._dependInverseBy)
            {
                AddDependedEntities(depEnt, ref dependedEntities);
            }
        }

        public void ClearLoops()
        {
            _isLoop = false;
            _currents.Clear();
        }

        public void AddDependBy(EntityReferenceDependency item)
        {
            _dependBy.Add(item);
            item._dependInverseBy.Add(this);
        }

        public void AddDependBy(IEnumerable<EntityReferenceDependency> items)
        {
            _dependBy.AddRange(items);

            foreach (var item in items)
                item._dependInverseBy.Add(this);
        }


        public int GetLevel()
        {
            if (_currents.Contains(this))
            {
                _isLoop = true;
                return LevelError;
            }

            _currents.Add(this);

            int level = 0;
            if (!_dependBy.Any())
                level = 0;
            else
                level = _dependBy.Max(item => item.GetLevel()) + 1;

            if (_isLoop)
                return LevelError;

            _currents.Remove(this);

            return level;
        }


    }

    /// <summary>
    /// struct intermedia che identifica un attributo 
    /// </summary>
    public class AttributoEntityId
    {
        static string _sep { get; set; } = "|";
        public string EntityTypeKey { get; set; }
        public string Entities { get; set; }
        public string EtichettaAttributo { get; set; }
        public string CodiceAttributo { get; set; }


        public AttributoEntityId() { }

        public AttributoEntityId(string str)
        {
            FromString(str);
        }

        public string To1String()
        {
            return AttributoEntityId.GetKey(EntityTypeKey, Entities, EtichettaAttributo, CodiceAttributo);
        }

        public void FromString(string str)
        {
            string[] strs = str.Split(_sep);

            EntityTypeKey = strs[0];
            Entities = strs[1];
            EtichettaAttributo = strs[2];
            CodiceAttributo = strs[3];
        }

        public static string GetEntityTypeKey(string strAttId) { string[] strs = strAttId.Split(_sep); return strs[0]; }
        public static string GetEntities(string strAttId) { string[] strs = strAttId.Split(_sep); return strs[1]; }
        public static string GetEtichettaAttributo(string strAttId) { string[] strs = strAttId.Split(_sep); return strs[2]; }
        public static string GetCodiceAttributo(string strAttId) { string[] strs = strAttId.Split(_sep); return strs[3]; }

        public static string GetKey(string entityTypeKey, string entities, string etichettaAttributo, string codiceAttributo)
        {
            return string.Format("{0}{1}{2}{3}{4}{5}{6}", entityTypeKey, _sep, entities, _sep, etichettaAttributo, _sep, codiceAttributo);
        }
    }



    public class SectionTreeEntityAttributiUpdater : SectionEntityAttributiUpdater
    {
        public SectionTreeEntityAttributiUpdater(ProjectServiceBase projectService) : base(projectService)
        {
            _projectService = projectService;
            _entitiesHelper = new EntitiesHelper(_projectService as ProjectService);
        }

        public override void CalcolaEntities(string entityTypeKey, EntityCalcOptions options, IEnumerable<Guid> entitiesId = null, EntitiesError error = null)
        {
            //ClearCalculatedValues();

            var validEntsId = entitiesId.Where(x => _projectService.IsEntityValid(entityTypeKey, x));

            List<TreeEntity> entities = null;
            if (entitiesId == null)
                entities = _projectService.GetTreeEntitiesList(entityTypeKey);
            else
                entities = _projectService.GetTreeEntitiesDeepById(entityTypeKey, validEntsId).ToList();


            if (options.ResetCalulatedValues)
                ClearCalculatedValues();

            //Ricalcolo di tutti gli attributi di tutte le entità
            for (int i = 0; i < entities.Count; i++)
            {
                TreeEntity ent = entities[i];

                ClearCurrentEntityCalculatedValues();

                
                if (ent.Depth == 0)
                    CalcolaEntityAttributiDeep(i, entities, options);
            }

        }


        
        protected void CalcolaAttributiAllParents(TreeEntity entity, List<TreeEntity> allTreeEntities)
        {
            TreeEntity parent = entity;

            if (!parent.IsParent)
                parent = parent.Parent;

            //Ricalcolo i padri (evidenziatori)
            while (parent != null)
            {
                int parentIndex = allTreeEntities.IndexOf(parent);
                CalcolaAttributiParent(parentIndex, allTreeEntities);
                parent = parent.Parent;
            }
        }

        public override bool CalcolaAttributiValues(Entity ent, EntityAttributo attNewValore, List<EntityAttributo> attsRifByFunction = null)
        {
            bool ret =  base.CalcolaAttributiValues(ent, attNewValore, attsRifByFunction);

            //Ricalcolo i padri (evidenziatori)
            List<TreeEntity> treeEnts = ProjectService.GetTreeEntitiesList(ent.EntityType.GetKey());
            CalcolaAttributiAllParents(ent as TreeEntity, treeEnts);

            return ret;

        }

        //Calcola gli attributi di un entità e dei suoi figli ricorsivamente
        protected void CalcolaEntityAttributiDeep(int entIndex, List<TreeEntity> entities, EntityCalcOptions options, bool excludeParents = false)
        {
            TreeEntity wbsItem = entities[entIndex];

            if (wbsItem.IsParent)
            {

                for (int i = entIndex + 1; i < entities.Count; i++)
                {
                    TreeEntity item = entities[i];

                    if (item.Depth <= wbsItem.Depth)
                        break;

                    if (item.Depth > wbsItem.Depth + 1)
                        continue;

                    CalcolaEntityAttributiDeep(i, entities, options, excludeParents);
                }
                
                if (!excludeParents)
                    CalcolaAttributiParent(entIndex, entities);

                EntityIdsChanged.Add(wbsItem.EntityId);
            }
            else
            {
                
                CalcolaAttributiLeaf(wbsItem, options);
                EntityIdsChanged.Add(wbsItem.EntityId);
            }
        }

        //calcola gli attributi della sola entità padre
        protected virtual void CalcolaAttributiParent(int parentIndex, List<TreeEntity> entities)
        {
            if (parentIndex < 0)
                return;

            ClearCurrentEntityCalculatedValues();
            
            TreeEntity parentItem = entities[parentIndex];

            int parentDepth = parentItem.Depth;
            string highlighterColorName = null;

            for (int i = parentIndex + 1; i < entities.Count; i++)
            {
                TreeEntity item = entities[i];

                if (item.Depth <= parentDepth)
                    break;

                if (item.Depth > parentDepth + 1)
                    continue;

                string itemHighlighterColName = item.HighlighterColorName;
                if (highlighterColorName == null)
                    highlighterColorName = itemHighlighterColName;
                else if (itemHighlighterColName != highlighterColorName)
                    highlighterColorName = MyColorsEnum.Transparent.ToString();
            }
            parentItem.HighlighterColorName = highlighterColorName;
        }

        /// <summary>
        /// Calcola gli attributi dell'entià senza ricalcolare alcuna altra entità (serve per la CalcolaEntities())
        /// </summary>
        /// <param name="wbsItem"></param>
        /// <param name="timeCalc"></param>
        protected virtual void CalcolaAttributiLeaf(TreeEntity ent, EntityCalcOptions options)
        {
            if (options.CalcolaAttributiResults)
            {
                CalcolaAttributiGuidCollection(ent);
                CalcolaAttributiRiferimento(ent);

                //AttributoDependencyComparer attDepComparer = new AttributoDependencyComparer(_projectService, ent);
                //IOrderedEnumerable<EntityAttributo> attsOrdered = ent.Attributi.Values.OrderBy(item => item.AttributoCodice, attDepComparer);
                List<EntityAttributo> attsOrdered = OrderEntityAttributiByFunctions(ent);

                foreach (EntityAttributo entAtt in attsOrdered)
                {

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

            //evidenziatore dell'entità
            UpdateHighlighterColorName(ent);
        }

    }


    internal class DivisioneEntityAttributiUpdater : SectionTreeEntityAttributiUpdater
    {
        public DivisioneEntityAttributiUpdater(ProjectServiceBase projectService) : base(projectService)
        {
            _calculatorFunction = null;
        }
    }

    internal class ContattiEntityAttributiUpdater : SectionEntityAttributiUpdater
    {
        public ContattiEntityAttributiUpdater(ProjectServiceBase projectService) : base(projectService)
        {
            _calculatorFunction = _projectService.Calculator.CntCalculatorFunction;
        }
    }

    internal class InfoProgettoEntityAttributiUpdater : SectionEntityAttributiUpdater
    {
        //public Dictionary<string, SectionEntityAttributiUpdater> GuidCollectionSectionUpdaters { get; set; } = new Dictionary<string, SectionEntityAttributiUpdater>();

        public InfoProgettoEntityAttributiUpdater(ProjectServiceBase projectService) : base(projectService)
        {
            _calculatorFunction = _projectService.Calculator.InfCalculatorFunction;
        }

        //public override void CalcolaAttributiGuidCollection(Entity ent)
        //{
        //    IEnumerable<EntityAttributo> guidCollAtts = ent.Attributi.Values.Where(item => item.Attributo.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.GuidCollection);
        //    foreach (EntityAttributo entAtt in guidCollAtts)
        //    {
        //        Attributo att = entAtt.Attributo;
        //        ValoreAttributoGuidCollection valAttGuidColl = att.ValoreAttributo as ValoreAttributoGuidCollection;
        //        if (valAttGuidColl != null)
        //        {
        //            if (valAttGuidColl.ItemsSelectionType == ItemsSelectionTypeEnum.All)
        //            {
        //                string entTypeKeyRef = att.GuidReferenceEntityTypeKey;

        //                FilterData filterData = null;
        //                List<Guid> ids = CalculateFilter(entTypeKeyRef, filterData);
        //                entAtt.Valore = new ValoreGuidCollection() { V = ids.Select(item => new ValoreGuidCollectionItem() { EntityId = item }).Cast<ValoreCollectionItem>().ToList() };
        //            }
        //        }
        //    }
        //}
    }

    //internal class DivProgettoEntityAttributiUpdater : SectionEntityAttributiUpdater
    //{
    //    public DivProgettoEntityAttributiUpdater(ProjectServiceBase projectService) : base(projectService)
    //    {
    //        _calculatorFunction = _projectService.Calculator.DivCalculatorFunction;
    //    }
    //}

    internal class PrezzarioEntityAttributiUpdater : SectionTreeEntityAttributiUpdater
    {
        public PrezzarioEntityAttributiUpdater(ProjectServiceBase projectService) : base(projectService)
        {
            _calculatorFunction = _projectService.Calculator.EPCalculatorFunction;
        }
    }

    internal class CapitoliEntityAttributiUpdater : SectionTreeEntityAttributiUpdater
    {
        public CapitoliEntityAttributiUpdater(ProjectServiceBase projectService) : base(projectService)
        {
            _calculatorFunction = null;
        }
    }

    internal class ElencoAttivitaEntityAttributiUpdater : SectionEntityAttributiUpdater
    {
        public ElencoAttivitaEntityAttributiUpdater(ProjectServiceBase projectService) : base(projectService)
        {
            _calculatorFunction = _projectService.Calculator.EAtCalculatorFunction;
        }
    }

    internal class CalendariEntityAttributiUpdater : SectionEntityAttributiUpdater
    {
        public CalendariEntityAttributiUpdater(ProjectServiceBase projectService) : base(projectService)
        {
            _calculatorFunction = _projectService.Calculator.CalendariCalculatorFunction;
        }

        public override bool CalcolaAttributiValues(Entity ent)
        {
            ValoreTesto valJson = ent.GetValoreAttributo(BuiltInCodes.Attributo.WeekHours, false, false) as ValoreTesto;
            if (valJson == null || !valJson.HasValue())
            {
                string json = string.Empty;
                if (JsonSerializer.JsonSerialize(WeekHours.Default, out json))
                {
                    ent.Attributi[BuiltInCodes.Attributo.WeekHours].Valore = new ValoreTesto() { V = json };
                    ent.Attributi[BuiltInCodes.Attributo.WeekHoursText].Valore = new ValoreTesto() { V = WeekHours.Default.ToUserText() };
                }
            }
            
            return base.CalcolaAttributiValues(ent);
        }

    }



    internal class ElementiEntityAttributiUpdater : SectionEntityAttributiUpdater
    {
        public ElementiEntityAttributiUpdater(ProjectServiceBase projectService) : base(projectService)
        {
            _calculatorFunction = _projectService.Calculator.ElmCalculatorFunction;
        }

        public override bool CalcolaAttributiValues(Entity ent, EntityAttributo attNewValore, Queue<string> functions, List<EntityAttributo> attsRifByFunction = null)
        {
            _projectService.Calculator.Calculate(ent, attNewValore.Attributo, attNewValore.Valore);


            //Se cambio il globalId o il nome file ricarico tutti gli altri attributi
            if (attNewValore.AttributoCodice == BuiltInCodes.Attributo.GlobalId ||
                attNewValore.AttributoCodice == BuiltInCodes.Attributo.ProjectGlobalId)
            {
                foreach (EntityAttributo entAtt in ent.Attributi.Values)
                    _projectService.Calculator.Calculate(ent, entAtt.Attributo, entAtt.Valore);
            }

            CalcolaAttributiRifByFunction(ent, functions, attsRifByFunction);

            functions.Dequeue();//.Pop();

            return true;
        }
    }

    internal class ComputoEntityAttributiUpdater : SectionEntityAttributiUpdater
    {
        public ComputoEntityAttributiUpdater(ProjectServiceBase projectService) : base(projectService)
        {
            _calculatorFunction = _projectService.Calculator.CmpCalculatorFunction;
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

                functions.Dequeue();//.Pop();


            }
            catch (Exception e)
            {
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), e.Message);
            }

            return true;
        }

        protected override bool CalcolaAttributiBuiltIn(Entity ent, EntityAttributo attNewValore)
        {

            if (attNewValore.AttributoCodice == BuiltInCodes.Attributo.Quantita)
            {
                CalcolaAttributoRule(ent);
                _projectService.Calculator.Calculate(ent, attNewValore.Attributo, attNewValore.Valore);
                return true;
            }
            else if (attNewValore.AttributoCodice == BuiltInCodes.Attributo.QuantitaTotale)/* "QuantitaTotale"*/
            {
                //il calcolo della quantità totale la faccio a parte per gestire i campi pu o qta che sono nulli
                ValoreReale qtaTotaleVal = attNewValore.Valore as ValoreReale;
                //ValoreReale clone = qtaTotaleVal.Clone() as ValoreReale;
                double? real = 0;

                ValoreReale puVal = ent.Attributi[BuiltInCodes.Attributo.PU].Valore as ValoreReale;
                ValoreReale qtaVal = ent.Attributi[BuiltInCodes.Attributo.Quantita].Valore as ValoreReale;

                if (puVal.HasValue() && qtaVal.HasValue())
                {
                    real = puVal.RealResult * qtaVal.RealResult;
                }
                else if (!puVal.HasValue() && !qtaVal.HasValue())
                {
                    //clone.V = "0";
                    //clone.RealResult = 0;
                    real = 0;
                }
                else
                {
                    if (!puVal.HasValue())
                        real = qtaVal.RealResult;
                        //clone.RealResult = qtaVal.RealResult;

                    if (!qtaVal.HasValue())
                        real = puVal.RealResult;
                        //clone.RealResult = puVal.RealResult;
                }

                Attributo att = attNewValore.Attributo;
                var valAttReale = att.ValoreAttributo as ValoreAttributoReale;
                if (valAttReale != null && valAttReale.UseSignificantDigitsByFormat)
                {
                    NumberFormat nf = NumericFormatHelper.DecomposeFormat(att.ValoreFormat);
                    if (nf.DecimalDigitCount >= 0)
                    {
                        double realRounded = Math.Round(real.Value, nf.DecimalDigitCount, MidpointRounding.AwayFromZero);
                        qtaTotaleVal.RealResult = realRounded;
                    }
                }
                else
                    qtaTotaleVal.RealResult = real;

                ///////////////////////
                //Set ResultDescription
                string formulaPU = CalculatorExpression.CreateFormula(CmpCalculatorFunction.FunctionName, ent.Attributi[BuiltInCodes.Attributo.PU].Attributo.Etichetta);
                //tronco alla terza cifra decimale
                double dPU = Math.Truncate(puVal.RealResult.Value * 1000) / 1000;
                string puResDesc = puVal.ResultDescription.GetFirstLine();
                string row2 = string.Format("{0} = {1}", formulaPU, dPU);

                string formulaQta = CalculatorExpression.CreateFormula(CmpCalculatorFunction.FunctionName, ent.Attributi[BuiltInCodes.Attributo.Quantita].Attributo.Etichetta);
                //tronco alla terza cifra decimale
                double dQta = Math.Truncate(qtaVal.RealResult.Value * 1000) / 1000;
                string qtaResDesc = qtaVal.ResultDescription.GetFirstLine();
                string row3 = string.Format("{0} = {1}", formulaQta, dQta);

                string row1 = string.Empty;
                if (string.IsNullOrEmpty(puResDesc))
                    row1 = qtaResDesc;
                else
                {
                    if (!double.TryParse(puResDesc, out _))
                        puResDesc = String.Format("({0})", puResDesc);

                    if (!double.TryParse(qtaResDesc, out _))
                        qtaResDesc = String.Format("({0})", qtaResDesc);


                    row1 = string.Format("{0} * {1}", puResDesc, qtaResDesc);
                }


                qtaTotaleVal.ResultDescription = string.Format("{0}\n{1}\n{2}", row1, row2, row3);
                ///////////////////////

                return true;

            }
            else if (attNewValore.AttributoCodice == BuiltInCodes.Attributo.Importo)
            {
                ValoreReale qtaTotaleVal = ent.Attributi[BuiltInCodes.Attributo.QuantitaTotale].Valore as ValoreReale;
                ValoreContabilita prezzoVal = _projectService.GetValoreAttributo(ent, "PrezzarioItem_Prezzo") as ValoreContabilita;


                ValoreContabilita importoVal = ent.Attributi[BuiltInCodes.Attributo.Importo].Valore as ValoreContabilita;

                if (prezzoVal == null)
                {
                    importoVal.RealResult = null;
                    return true;
                }

                if (qtaTotaleVal.RealResult != null && prezzoVal.RealResult != null)
                {
                    if (double.IsNaN(qtaTotaleVal.RealResult.Value) || prezzoVal.RealResult == null)
                        importoVal.RealResult = null;
                    else
                        importoVal.RealResult = prezzoVal.RealResult.Value * (decimal)qtaTotaleVal.RealResult.Value;

                    ///////////////////////
                    //Set ResultDescription
                    //string formulaPrezzo = CalculatorExpression.CreateFormula(CmpCalculatorFunction.FunctionName, ent.Attributi["PrezzarioItem_Prezzo"].Attributo.Etichetta);
                    string formulaPrezzo = CalculatorExpression.CreateFormula(CmpCalculatorFunction.FunctionName, ent.EntityType.Attributi["PrezzarioItem_Prezzo"].Etichetta);
                    //tronco alla terza cifra decimale
                    decimal dPrezzo = Math.Truncate(prezzoVal.RealResult.Value * 1000) / 1000;
                    string row2 = string.Format("{0} = {1}", formulaPrezzo, dPrezzo);

                    string formulaQtaTot = CalculatorExpression.CreateFormula(CmpCalculatorFunction.FunctionName, ent.Attributi[BuiltInCodes.Attributo.QuantitaTotale].Attributo.Etichetta);
                    //tronco alla terza cifra decimale
                    double dQtaTot = Math.Truncate(qtaTotaleVal.RealResult.Value * 1000) / 1000;
                    string row3 = string.Format("{0} = {1}", formulaQtaTot, dQtaTot);

                    string row1 = string.Format("{0} * {1}", dPrezzo, dQtaTot);

                    importoVal.ResultDescription = string.Format("{0}\n{1}\n{2}", row1, row2, row3);
                    ///////////////////////

                }

                return true;

            }

            return false;
        }

        /// <summary>
        /// Ricalcola l'attributo che mostra la regola "Filtro Regola"
        /// </summary>
        /// <param name="ent"></param>
        private void CalcolaAttributoRule(Entity ent)
        {
            //ricalcolo del valore dell'attributo "Filtro Regola"
            ValoreTesto valModel3dRule = _projectService.GetValoreAttributo(ent, BuiltInCodes.Attributo.Model3dRule) as ValoreTesto;
            ValoreReale valQta = _projectService.GetValoreAttributo(ent, "Quantita") as ValoreReale;
            ValoreGuid valRuleId = _projectService.GetValoreAttributo(ent, BuiltInCodes.Attributo.Model3dRuleId) as ValoreGuid;
            ValoreGuid valPrezzarioItemId = _projectService.GetValoreAttributo(ent, BuiltInCodes.Attributo.PrezzarioItem_Guid) as ValoreGuid;
            ValoreGuid valElementiItemId = _projectService.GetValoreAttributo(ent, BuiltInCodes.Attributo.ElementiItem_Guid) as ValoreGuid;
            ValoreGuid valRuleElementiItemId = _projectService.GetValoreAttributo(ent, BuiltInCodes.Attributo.Model3dRuleElementiItemId) as ValoreGuid;

            valModel3dRule.V = "";
            ProjectService projectService = _projectService as ProjectService;
            foreach (Model3dFilterData filter in projectService.GetModel3dFiltersData().Items)
            {

                foreach (Model3dRuleComputo rule in filter.RulesComputo)
                {
                    if (rule.Id == valRuleId.V)
                    {
                        //regola trovata
                        valModel3dRule.V = filter.Descri;

                        if (rule.FormulaQta != null && valQta.PlainText != null && rule.FormulaQta.Trim() != valQta.PlainText.Trim() ||
                            rule.PrezzarioItemId != valPrezzarioItemId.V ||
                            valRuleElementiItemId.V != valElementiItemId.V)
                        {
                            //formula qta modificata dall'utente (qta o prezzarioItemId o elementiItemId)
                            valModel3dRule.V = string.Format("{0}{1}", filter.Descri, "*");
                        }
                    }
                }
            }
        }

        
    }

    internal class DocumentiEntityAttributiUpdater : SectionTreeEntityAttributiUpdater
    {
        public DocumentiEntityAttributiUpdater(ProjectServiceBase projectService) : base(projectService)
        {
            _calculatorFunction = null;
        }
    }

    internal class ReportEntityAttributiUpdater : SectionEntityAttributiUpdater
    {
        public ReportEntityAttributiUpdater(ProjectServiceBase projectService) : base(projectService)
        {
            _calculatorFunction = null;
        }
    }

    internal class VarEntityAttributiUpdater : SectionEntityAttributiUpdater
    {
        public VarEntityAttributiUpdater(ProjectServiceBase projectService) : base(projectService)
        {
            _calculatorFunction = _projectService.Calculator.VarCalculatorFunction;
        }
    }

    internal class AllEntityAttributiUpdater : SectionEntityAttributiUpdater
    {
        EntitiesHelper _entsHelper = null;

        public AllEntityAttributiUpdater(ProjectServiceBase projectService) : base(projectService)
        {
            _calculatorFunction = null;
            _entsHelper = new EntitiesHelper(projectService as ProjectService);
        }

        public override bool CalcolaAttributiValues(Entity ent, EntityAttributo attNewValore, List<EntityAttributo> attsRifByFunction = null)
        {
            if (attNewValore.AttributoCodice == BuiltInCodes.Attributo.Link)
            {
                AllegatiItem allItem = ent as AllegatiItem;
                string link = _entsHelper.GetValorePlainText(ent, BuiltInCodes.Attributo.Link, false, false);

                string fileName = string.Empty;
                if (Path.IsPathFullyQualified(link))
                {

                    Uri uri = new Uri(link);
                    if (uri.IsFile)
                    {
                        fileName = Path.GetFileName(link);
                    }
                    else
                    {
                        fileName = HttpUtility.ParseQueryString(uri.Query).Get("filename");
                    }
                }
                else if (IsJoinApiSource(link))
                {
                    //web
                    Uri uri = new Uri(link);
                    fileName = HttpUtility.ParseQueryString(uri.Query).Get("fileName");
                }

                allItem.FileName = fileName;
            }

            return base.CalcolaAttributiValues(ent, attNewValore, attsRifByFunction);
        }

        bool IsJoinApiSource(string url)
        {
            if (!url.StartsWith("https://"))
                return false;

            //Uri uri = new Uri(url);
            //Uri uriBase = new Uri(string.Format("{0}{1}", ServerAddress.ApiCurrent, CacheManager.ApiFileRepositoryPath));

            //var ret = uri.IsBaseOf(uriBase);

            return true;
        }

    }

    //public class AttributoDependencyComparer : IComparer<string>
    //{
    //    ProjectServiceBase _projectService = null;
    //    Entity _entity { get; set; } = null;

    //    public AttributoDependencyComparer(ProjectServiceBase projectService, Entity entity)
    //    {
    //        _projectService = projectService;
    //        _entity = entity;
    //    }

    //    public int Compare(string x, string y)//codici
    //    {
    //        //if (_entity.EntityType.Attributi[x].Etichetta == "IVA" || _entity.EntityType.Attributi[y].Etichetta == "IVA")
    //        //{
    //        //    int p = 0;
    //        //}

    //        if (x == y)
    //            return 0;

    //        Attributo attX = _entity.EntityType.Attributi[x];
    //        Attributo attY = _entity.EntityType.Attributi[y];

    //        if (attX is AttributoRiferimento && attY is AttributoRiferimento)
    //            return 0;

    //        if (attX is AttributoRiferimento)
    //            return -1;

    //        if (attY is AttributoRiferimento)
    //            return 1;

    //        string plainTextX = null;
    //        string plainTextY = null;

    //        Valore valX = _projectService.GetValoreAttributo(_entity, x);
    //        if (valX != null)
    //        {
    //            if (valX is ValoreReale || valX is ValoreContabilita || valX is ValoreTesto)
    //                plainTextX = valX.PlainText;
    //        }


    //        Valore valY = _projectService.GetValoreAttributo(_entity, y);
    //        if (valY != null)
    //        {
    //            if (valY is ValoreReale || valY is ValoreContabilita || valY is ValoreTesto)
    //                plainTextY = valY.PlainText;
    //        }

    //        if (string.IsNullOrEmpty(plainTextX) && string.IsNullOrEmpty(plainTextY))
    //            return 0;
    //        if (string.IsNullOrEmpty(plainTextX))
    //            return -1;
    //        if (string.IsNullOrEmpty(plainTextY))
    //            return 1;

    //        string funcX = string.Format("att{{{0}}}", attX.Etichetta);
    //        string funcY = string.Format("att{{{0}}}", attY.Etichetta);



    //        if (plainTextX.Contains(funcY))
    //            return 1;
    //        else if (plainTextY.Contains(funcX))
    //            return -1;

    //        return 0;
    //    }
    //}


}
