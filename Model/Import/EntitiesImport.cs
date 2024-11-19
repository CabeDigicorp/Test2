using CommonResources;
using MasterDetailModel;
using Model.Calculators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Services.Description;

namespace Model
{
    /// <summary>
    /// Classe per l'importazione da file esterno di Entità
    /// </summary>
    public class EntitiesImport
    {
        public ProjectService Target { get; set; }
        EntitiesImportStatus _status = null;
        Factory Factory = null;
        //Dictionary<string, Guid> _targetGuidsByKey { get; set; } = null;



        public void Run(EntitiesImportStatus status)
        {

          

            _status = status;
            Factory = new Factory(Target);


            if (_status.Status == EntityImportStatusEnum.Undefined)
            {
                Load(status);

                Target.ClearEntitiesGuidByKey();
                _status.PerformedEntitiesId.Clear();
                _status.EntityTypes.Clear();
            }
            _status.Status = EntityImportStatusEnum.Running;

            
            foreach (EntityImportId sourceEnt in _status.StartingEntitiesId)
            {
                EntityImportStatus entStatus = null;
                if (_status.EntitiesBySourceId.ContainsKey(sourceEnt.SourceId))
                    entStatus = _status.EntitiesBySourceId[sourceEnt.SourceId];


                var ids = AddEntity(entStatus);
                
                if (_status.Status == EntityImportStatusEnum.Waiting)
                    return;

                if (ids != null)
                    sourceEnt.TargetId = entStatus.TargetId;
            }

            //update indexes
            List<EntityType> targetEntTypes = new List<EntityType>();
            foreach (EntityTypeImportStatus entTypeImp in _status.EntityTypes.Values)
            {
                Target.UpdateEntitiesIndexes(entTypeImp.TargetEntityTypeKey);

                EntityType entType = Target.GetEntityTypes()[entTypeImp.TargetEntityTypeKey];
                targetEntTypes.Add(entType);
            }


            //ricalcolo
            EntityAttributiUpdater entityAttributiUpdater = new EntityAttributiUpdater(Target);

            string entityTypeKey = null;
            IEnumerable<Guid> entsId = null;
            List<Entity> ents = new List<Entity>();

            var calcOptions = new EntityCalcOptions() { ResetCalulatedValues = true, CalcolaAttributiResults = true };

            //Ricalcolo delle entità importate 
            IOrderedEnumerable <EntityType> targetOrderedEntTypes = targetEntTypes.OrderBy(item => item.DependentTypesEnum);
            foreach (EntityType entType in targetOrderedEntTypes)
            {
                entityTypeKey = entType.GetKey();
                entsId = _status.PerformedEntitiesId.Where(item => item.TargetEntityTypeKey == entityTypeKey && item.TargetId != Guid.Empty).Select(item => item.TargetId);

                ents = Target.GetEntitiesById(entityTypeKey, entsId);

                entityAttributiUpdater.CalcolaEntitiesValues(entityTypeKey, ents, calcOptions);

            }


            //Ricalcolo delle entità dipendenti in altre sezioni 
            //entityAttributiUpdater.CalcolaDependentEntityTypes(entityTypeKey, ents);
            var entsError = new EntitiesError();
            entityAttributiUpdater.CalcolaEntityValuesAndDependentEntityTypes(entityTypeKey, ents.Select(x => x.EntityId), calcOptions, entsError);


            _status.Status = EntityImportStatusEnum.Completed;
        }


        /// <summary>
        /// Torna i nuovi guid o i vecchi se è stato trovato con la stessa chiave nel target
        /// </summary>
        /// <param name="entStatus"></param>
        /// <returns></returns>
        List<Guid> AddEntity(EntityImportStatus entStatus)
        {
            if (entStatus == null)
                return null;

            if (entStatus.Status == EntityImportStatusEnum.Completed)
                return null;

            
            EntityType sourceEntType = null;
            if (_status.Source.GetEntityTypes().ContainsKey(entStatus.SourceEntityTypeKey))
                sourceEntType = _status.Source.GetEntityTypes()[entStatus.SourceEntityTypeKey];

            if (sourceEntType == null)//entityType source non valida
                return new List<Guid>();

            EntityTypeImportStatus entTypeStatus = GetEntityTypeImportStatus(sourceEntType);

            EntityType targetEntType = null;

            if (entTypeStatus != null)
                targetEntType = Target.GetEntityTypes()[entTypeStatus.TargetEntityTypeKey];

            if (targetEntType == null) //entityType target non valida
                return new List<Guid>();

            EntityService sourceEntService = null;
            if (sourceEntType.IsTreeMaster)
                sourceEntService = new TreeEntityService(_status.Source, sourceEntType.GetKey());
            else
                sourceEntService = new EntityService(_status.Source, sourceEntType.GetKey());

            
            EntityService targetEntService = null;
            if (targetEntType.IsTreeMaster)
                targetEntService = new TreeEntityService(Target, targetEntType.GetKey(), Factory);
            else
                targetEntService = new EntityService(Target, targetEntType.GetKey(), Factory);

            IEnumerable<Entity> entSourceList = sourceEntService.GetDeepEntity(entStatus.SourceId);

            int depth = 0;
            Entity entTarget = null;
            List<Guid> ids = new List<Guid>();
            foreach (Entity entSource in entSourceList) //Deep TreeEntity (solo l'ultimo è la foglia)
            {
                bool isTreeParentEntity = false;
                if (entSource != entSourceList.Last())//se non è l'ultimo è un tree parent
                    isTreeParentEntity = true;

                entStatus = _status.EntitiesBySourceId[entSource.EntityId];

                bool entConflict = CreateEntity(entStatus, targetEntService, isTreeParentEntity, entSource, out entTarget);
                if (_status.Status == EntityImportStatusEnum.Waiting)
                    return null;


                if (entTarget != null) //aggiungo la nuova entità
                {

                    if (!entConflict)
                    {
                        //Aggiunta
                        targetEntService.SetDepth(entTarget, depth);

                        if (_status.TargetPosition == TargetPosition.Bottom)
                        {
                            Guid parentId = ids.LastOrDefault();
                            Target.AddEntity(targetEntType.GetKey(), entTarget, parentId);
                        }
                        else if (_status.TargetPosition == TargetPosition.TargetAfter)
                        {
                            if (_status.TargetId != Guid.Empty)
                            {

                            }
                        }
                    }

                    entStatus.TargetId = entTarget.EntityId;
                }
                else //non aggiungo la nuova entità
                {
                    entStatus.TargetId = Guid.Empty;
                }

                entStatus.Status = EntityImportStatusEnum.Completed;
                ids.Add(entStatus.TargetId);
                depth++;


                //Aggiungo alle entità eseguite (performed)
                EntityImportId entImportId = new EntityImportId()
                {
                    SourceEntityTypeKey = entStatus.SourceEntityTypeKey,
                    SourceId = entStatus.SourceId,
                    TargetEntityTypeKey = entStatus.TargetEntityTypeKey,
                    TargetEntityTypeName = entStatus.TargetEntityTypeName,
                    TargetId = entStatus.TargetId,
                    ConflictAction = entStatus.ConflictAction,
                };

                _status.PerformedEntitiesId.Add(entImportId);


            }

            return ids;
        }

        private bool CreateEntity(EntityImportStatus entStatus, EntityService targetEntService,
            bool isTreeParentEntity, Entity entSource, out Entity entTarget)
        {

            EntityType targetEntType = targetEntService.GeEntityType();
            string targetEntTypeKey = targetEntType.GetKey();

            EntityComparer entComparer = null;
            _status.CustomEntityComparers.TryGetValue(targetEntType.GetKey(), out entComparer);

            if (entComparer == null)
                entComparer = entSource.EntityType.EntityComparer;


            string sourceKey = entSource.GetComparerKey(entComparer);

            //Dictionary<string, Guid> targetGuidsByKey = Target.GetEntitiesGuidByKey(targetEntType.GetKey());
            //if (targetGuidsByKey == null)
            //    targetGuidsByKey = Target.UpdateEntsGuidIndexedByKey(targetEntType.GetKey());

            //if (_targetGuidsByKey == null)
            //{
            //    if (_status.CustomTargetGuidsByKey != null)
            //        _targetGuidsByKey = _status.CustomTargetGuidsByKey;
            //    else
            //        _targetGuidsByKey = Target.CreateKey(targetEntTypeKey, entComparer.KeySeparator, entComparer.AttributiCode, out _);
            //}

            if (entStatus.TargetGuidsByKey == null)
            {
                if (targetEntTypeKey == _status.CustomTargetEntityTypeKey && _status.CustomTargetGuidsByKey != null)
                    entStatus.TargetGuidsByKey = _status.CustomTargetGuidsByKey;
                else
                    entStatus.TargetGuidsByKey = Target.CreateKey(targetEntTypeKey, entComparer.KeySeparator, entComparer.AttributiCode, out _);
            }

            if (targetEntTypeKey == _status.CustomTargetEntityTypeKey)
            {
                if (_status.CustomTargetGuidsByKey != null && !_status.CustomTargetGuidsByKey.ContainsKey(sourceKey))
                {
                    //se non è tra le voci di target che posso modificare o inserire salto l'entità (viene usato in Aggiorna da prezzario)
                    entTarget = null;
                    return false;
                }
            }


            //Controllo se entSource è già stato inserito nel target //AU 29/07/2020
            if (_status.EntitiesBySourceId.ContainsKey(entSource.EntityId))
            {
                Guid targetId = _status.EntitiesBySourceId[entSource.EntityId].TargetId;
                if (targetId != Guid.Empty)
                {
                    entTarget = Target.GetEntityById(targetEntType.GetKey(), targetId);
                    return true;
                }
            }

            ////////////////////////
            //gestione conflitto
            EntityImportConflictAction conflictAction = GetConflictAction(entStatus);
            bool entConflict = HasConflict(targetEntType, sourceKey, entStatus.TargetGuidsByKey);
            if (entConflict)
            {
                //entTarget = Target.GetEntityById(targetEntType.GetKey(), _targetGuidsByKey[sourceKey]);
                entTarget = Target.GetEntityById(targetEntType.GetKey(), entStatus.TargetGuidsByKey[sourceKey]);

                if (conflictAction == EntityImportConflictAction.Undefined)
                {
                    _status.WaitingInfo = new EntityImportWaitingInfo();
                    _status.WaitingInfo.SourceId = entSource.EntityId;
                    _status.WaitingInfo.TargetId = entTarget.EntityId;



                    if (entSource.EntityType is DivisioneItemType)
                        _status.WaitingInfo.SourceEntityTypeName = string.Format("{0} {1}", LocalizationProvider.GetString("Divisione"), entSource.EntityType.Name);
                    else
                        _status.WaitingInfo.SourceEntityTypeName = entSource.EntityType.Name;

                    if (entSource.EntityType is DivisioneItemType)
                        _status.WaitingInfo.TargetEntityTypeName = string.Format("{0} {1}", LocalizationProvider.GetString("Divisione"), entTarget.EntityType.Name);
                    else
                        _status.WaitingInfo.TargetEntityTypeName = entTarget.EntityType.Name;

                    _status.WaitingInfo.SourceEntityKey = sourceKey;

                    _status.Status = EntityImportStatusEnum.Waiting;
                    return entConflict;
                }
            }
            else //non c'è conflitto
            {
                if (_status.LimitedEntityTypes != null) //ad esempio nel computo non viene importata nessuna voce di altre sezioni o divisioni
                {
                    if (!_status.LimitedEntityTypes.Contains(targetEntType.GetKey()))
                    {
                        entTarget = null;
                        return false;
                    }
                }

                entTarget = targetEntService.Create(isTreeParentEntity);
            }

            //////////////////
            //Aggiornamento dei valori degli attributi
            if (!entConflict || conflictAction == EntityImportConflictAction.Overwrite)
            {

                //Importazione degli attributi di entSource
                foreach (EntityAttributo sourceEntAtt in entSource.Attributi.Values)
                {
                    //Non importo gli attributi che riguardano la regola
                    if (sourceEntAtt.AttributoCodice == BuiltInCodes.Attributo.Model3dRuleId
                        || sourceEntAtt.AttributoCodice == BuiltInCodes.Attributo.Model3dRule
                        || sourceEntAtt.AttributoCodice == BuiltInCodes.Attributo.Model3dRuleElementiItemId)
                        continue;

                    EntityAttributo targetEntAtt = GetTargetEntityAttributo(entTarget, sourceEntAtt);

                    //Non importo l'attributo se non presente nel target
                    if (targetEntAtt == null)
                        continue;

                    //Non importo il valore se l'attributo è luchettato o readOnly
                    //readonly è stato tolto perchè su report e documenti gli attributri sono readonly solo per l'utente
                    if (targetEntAtt.Attributo.IsValoreLockedByDefault/* || targetEntAtt.Attributo.IsValoreReadOnly*/)
                        continue;


                    if (sourceEntAtt.Attributo.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid)
                    {
                        string entityTypeCode = sourceEntAtt.Attributo.GuidReferenceEntityTypeKey;

                        Guid sourceId = (sourceEntAtt.Valore as ValoreGuid).V;

                        if (sourceId == Guid.Empty)
                            continue;

                        //Dovrebbe sempre esserci (vengono aggiunti da AddSourceEntityId)
                        if (!_status.EntitiesBySourceId.ContainsKey(sourceId))
                            continue;

                        EntityImportStatus newEntityStatus = _status.EntitiesBySourceId[sourceId];
                        newEntityStatus.SourceId = sourceId;
                        newEntityStatus.SourceEntityTypeKey = entityTypeCode;
                        

                        if (newEntityStatus.Status != EntityImportStatusEnum.Completed)
                        {

                            List<Guid> targetId = AddEntity(newEntityStatus);//recursion
                            if (_status.Status == EntityImportStatusEnum.Waiting)
                                return false;

                            newEntityStatus.TargetId = targetId.LastOrDefault();
                        }

                        targetEntAtt.Valore = new ValoreGuid() { V = newEntityStatus.TargetId };
                    }
                    else if (sourceEntAtt.Attributo.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.GuidCollection)
                    {
                        string entityTypeCode = sourceEntAtt.Attributo.GuidReferenceEntityTypeKey;

                        List<ValoreCollectionItem> valCollGuids = new List<ValoreCollectionItem>();
                        foreach (ValoreGuidCollectionItem valCollItem in (sourceEntAtt.Valore as ValoreGuidCollection).Items)
                        {
                            if (valCollItem.Id == Guid.Empty)
                                continue;

                            //EntityImportStatus newEntityStatus = new EntityImportStatus();
                            EntityImportStatus newEntityStatus = _status.EntitiesBySourceId[valCollItem.EntityId];
                            newEntityStatus.SourceId = valCollItem.EntityId;
                            newEntityStatus.SourceEntityTypeKey = entityTypeCode;

                            if (newEntityStatus.Status != EntityImportStatusEnum.Completed)
                            {

                                //newEntityStatus.TargetId = AddEntity(newEntityStatus).Last();//recursion
                                List<Guid> targetIds = AddEntity(newEntityStatus);//recursion
                                if (_status.Status == EntityImportStatusEnum.Waiting)
                                    return false;

                                newEntityStatus.TargetId = targetIds.LastOrDefault();
                            }

                            valCollGuids.Add(new ValoreGuidCollectionItem() { Id = Guid.NewGuid(), EntityId = newEntityStatus.TargetId });
                        }
                        targetEntAtt.Valore = new ValoreGuidCollection() { V = valCollGuids };
                    }
                    else if (sourceEntAtt.Attributo.DefinizioneAttributoCodice != BuiltInCodes.DefinizioneAttributo.Riferimento)
                    {
                        Valore val = sourceEntAtt.Entity.GetValoreAttributo(sourceEntAtt.AttributoCodice, false, true);
                        targetEntService.SetEntityAttributoValore(targetEntAtt, val.Clone());
                        //targetEntAtt.Valore = sourceEntAtt.Valore.Clone();
                    }
                }

                //Setto l'attributo FileOrigine nell'entità target se presente con il nome del file sorgente
                if (entTarget.Attributi.ContainsKey(BuiltInCodes.Attributo.FileOrigine))
                {
                    entTarget.Attributi[BuiltInCodes.Attributo.FileOrigine].Valore = new ValoreTesto() { V = _status.SourceName };
                }


            }

            return entConflict;
        }

        private bool HasConflict(EntityType targetEntType, string sourceKey, Dictionary<string, Guid> targetGuidsByKey)
        {
            //if (sourceKey == null || !sourceKey.Any())
            //    return false;

            //oss: la key vuota è valida per un entità. (per esempio: articolo senza codice)
            if (sourceKey == null)
                return false;

            //sovrascrivo il comparer se l'utente ne vuole uno particolare (ad esempio nell'aggiornamento dell'articolo da un prezzario più recente)
            EntityComparer entComparer = targetEntType.EntityComparer;
            if (_status.CustomEntityComparers.ContainsKey(targetEntType.GetKey()))
                entComparer = _status.CustomEntityComparers[targetEntType.GetKey()];

            if (targetGuidsByKey.Keys.Contains(sourceKey, entComparer))
            {
                ////oss: non ritorna conflitto se è appena stato aggiunto
                ////(ad esempio se sto aggiungendo due treeEntity con lo stesso padre non può chiedere se voglio sovrascrivere il padre che ho appena aggiunto
                //Guid targetId = targetGuidsByKey[sourceKey];
                //EntityImportStatus entStatus = _status.EntitiesBySourceId.Values.FirstOrDefault(item => item.TargetId == targetId);
                //if (entStatus.Status == EntityImportStatusEnum.Completed)
                //    return false;
                return true;
            }
            return false;
        }

        EntityAttributo GetTargetEntityAttributo(Entity targetEnt, EntityAttributo sourceEntAtt)
        {
            Attributo targetAtt = GetTargetAttributo(targetEnt.EntityType, sourceEntAtt);
            if (targetAtt != null)
            {
                if (targetEnt.Attributi.ContainsKey(targetAtt.Codice))
                    return targetEnt.Attributi[targetAtt.Codice];
            }

            return null;
        }


        Attributo GetTargetAttributo(EntityType targetEntType, EntityAttributo sourceEntAtt)
        {
            //Cerco l'attributo corrispondente per codice se BuiltIn
            //o per etichetta e che sia dello stesso valore se non BuiltIn

            Attributo targetAtt = null;
            //if (sourceEntAtt.Attributo.IsBuiltIn)
            if (EntitiesHelper.IsCodiceBuiltIn(sourceEntAtt.Attributo.Codice))
            {
                //cerco per codice
                targetAtt = targetEntType.Attributi.Values.FirstOrDefault(item => item.Codice == sourceEntAtt.Attributo.Codice);
            }
            else
            {
                //cerco per etichetta
                targetAtt = targetEntType.Attributi.Values.FirstOrDefault(item => item.Etichetta == sourceEntAtt.Attributo.Etichetta);
            }

            if (targetAtt != null)
            {
                if (targetAtt.DefinizioneAttributoCodice == sourceEntAtt.Attributo.DefinizioneAttributoCodice)
                    return targetAtt;
            }

            return null;
        }

        EntityTypeImportStatus GetEntityTypeImportStatus(EntityType sourceEntType)
        {
            Dictionary<string, EntityType> targetEntTypes = Target.GetEntityTypes();


            if (_status.EntityTypes.ContainsKey(sourceEntType.GetKey()))
                return _status.EntityTypes[sourceEntType.GetKey()];

            EntityTypeImportStatus entTypeStatus = null;
            if (sourceEntType is DivisioneItemType)
            {
                DivisioneItemType sourceDivType = sourceEntType as DivisioneItemType;

                //cerco la divisione nel target per Name
                IEnumerable<EntityType> targetDivsType = targetEntTypes.Values.Where(item => item is DivisioneItemType);
                EntityType targetEntType = targetDivsType.FirstOrDefault(item => item.Name == sourceDivType.Name);

                if (targetEntType != null)
                {
                    //trovata EntityType corrispondente nel target 
                    entTypeStatus = new EntityTypeImportStatus()
                    {
                        SourceEntityTypeKey = sourceDivType.GetKey(),
                        TargetEntityTypeKey = targetEntType.GetKey(),
                    };

                    _status.EntityTypes.Add(sourceEntType.GetKey(), entTypeStatus);
                }

                return entTypeStatus;
            }
            else
            {
                //cerco EntityType nel target per Codice (key)
                if (targetEntTypes.ContainsKey(sourceEntType.GetKey()))
                {
                    entTypeStatus = new EntityTypeImportStatus()
                    {
                        SourceEntityTypeKey = sourceEntType.GetKey(),
                        TargetEntityTypeKey = sourceEntType.GetKey(),
                    };

                    _status.EntityTypes.Add(sourceEntType.GetKey(), entTypeStatus);
                    return entTypeStatus;
                }
            }
            

            return null;
        }


        EntityImportConflictAction GetConflictAction(EntityImportStatus entStatus)
        {
            if (entStatus.Status == EntityImportStatusEnum.Completed)
                entStatus.ConflictAction = EntityImportConflictAction.Ignore;

            if (_status.ConflictAction != EntityImportConflictAction.Undefined)
            {
                if (entStatus.ConflictAction == EntityImportConflictAction.Undefined)
                    entStatus.ConflictAction = _status.ConflictAction;

                return _status.ConflictAction;
            }

            if (_status.EntityTypes.ContainsKey(entStatus.SourceEntityTypeKey))
            {
                if (_status.EntityTypes[entStatus.SourceEntityTypeKey].ConflictAction != EntityImportConflictAction.Undefined)
                {
                    if (entStatus.ConflictAction == EntityImportConflictAction.Undefined)
                        entStatus.ConflictAction = _status.EntityTypes[entStatus.SourceEntityTypeKey].ConflictAction;

                    return _status.EntityTypes[entStatus.SourceEntityTypeKey].ConflictAction;
                }
            }

            return entStatus.ConflictAction;
        }

        private bool Load(EntitiesImportStatus status)
        {
            _status.EntitiesBySourceId.Clear();
            //_status.LoadedSourceEntitiesId.Clear();

            foreach (EntityImportId sourceEnt in _status.StartingEntitiesId)
            {
                EntityImportStatus entStatus = new EntityImportStatus()
                {
                    SourceId = sourceEnt.SourceId,
                    SourceEntityTypeKey = sourceEnt.SourceEntityTypeKey,
                };
                AddSourceEntityId(entStatus);
            }

            return true;
        }

        void AddSourceEntityId(EntityImportStatus entStatus)
        {

            EntityType sourceEntType = null;
            if (_status.Source.GetEntityTypes().ContainsKey(entStatus.SourceEntityTypeKey))
                sourceEntType = _status.Source.GetEntityTypes()[entStatus.SourceEntityTypeKey];

            if (sourceEntType == null)//entityType source non valida
                return;


            EntityTypeImportStatus entTypeStatus = GetEntityTypeImportStatus(sourceEntType);

            EntityType targetEntType = null;

            if (entTypeStatus != null)
                targetEntType = Target.GetEntityTypes()[entTypeStatus.TargetEntityTypeKey];

            if (targetEntType == null) //entityType target non valida
                return;

            EntityService sourceEntService = null;
            if (sourceEntType.IsTreeMaster)
                sourceEntService = new TreeEntityService(_status.Source, sourceEntType.GetKey());
            else
                sourceEntService = new EntityService(_status.Source, sourceEntType.GetKey());


            IEnumerable<Entity> entSourceList = sourceEntService.GetDeepEntity(entStatus.SourceId);

            int depth = 0;
            foreach (Entity entSource in entSourceList) //Deep TreeEntity (solo l'ultimo è la foglia)
            {

                foreach (EntityAttributo sourceEntAtt in entSource.Attributi.Values)
                {
                    //if (sourceEntAtt.Valore.ToPlainText() == "bcbf3e16-a264-47fd-9498-01bec7999e09")
                    //{
                    //    int p = 0;
                    //}

                    Attributo targetAtt = GetTargetAttributo(targetEntType, sourceEntAtt);

                    //Non importo l'attributo se non presente nel target
                    if (targetAtt == null)
                        continue;


                    if (sourceEntAtt.Attributo.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid)
                    {
                        string entityTypeCode = sourceEntAtt.Attributo.GuidReferenceEntityTypeKey;

                        Guid sourceId = (sourceEntAtt.Valore as ValoreGuid).V;

                        if (sourceId == Guid.Empty)
                            continue;

                        EntityImportStatus newEntityStatus = new EntityImportStatus();
                        newEntityStatus.SourceId = sourceId;
                        newEntityStatus.SourceEntityTypeKey = entityTypeCode;

                        AddSourceEntityId(newEntityStatus);//recursion
                    }
                    else if (sourceEntAtt.Attributo.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.GuidCollection)
                    {
                        string entityTypeCode = sourceEntAtt.Attributo.GuidReferenceEntityTypeKey;

                        List<ValoreCollectionItem> valCollGuids = new List<ValoreCollectionItem>();
                        foreach (ValoreGuidCollectionItem valCollItem in (sourceEntAtt.Valore as ValoreGuidCollection).Items)
                        {
                            if (valCollItem.Id == Guid.Empty)
                                continue;

                            EntityImportStatus newEntityStatus = new EntityImportStatus();
                            newEntityStatus.SourceId = valCollItem.EntityId;
                            newEntityStatus.SourceEntityTypeKey = entityTypeCode;

                            AddSourceEntityId(newEntityStatus);//recursion
                        }
                    }

                }

                //_status.LoadedSourceEntitiesId.Add(new EntityImportId()
                //{
                //    SourceId = entSource.Id,
                //    SourceEntityTypeKey = sourceEntType.GetKey(),
                //    TargetEntityTypeCode = targetEntType.GetKey(),
                //    TargetEntityTypeName = targetEntType.Name,
                //});

                if (!_status.EntitiesBySourceId.ContainsKey(entSource.EntityId))
                {
                    _status.EntitiesBySourceId.Add(entSource.EntityId, new EntityImportStatus()
                    {
                        SourceId = entSource.EntityId,
                        SourceEntityTypeKey = sourceEntType.GetKey(),
                        TargetEntityTypeKey = targetEntType.GetKey(),
                        TargetEntityTypeName = targetEntType.Name,
                    });
                }

                depth++;
            }
            
        
        }



    }

    public class EntityService
    {
        protected IDataService _dataService = null;
        protected string _entityTypeCode = string.Empty;
        protected Factory _factory = null;
        protected EntitiesHelper _entitiesHelper = null;

        public EntityService(IDataService dataService, string entityTypeCode, Factory factory = null)
        {
            _dataService = dataService;
            _entityTypeCode = entityTypeCode;
            _factory = factory;
            _entitiesHelper = new EntitiesHelper(_dataService);
        } 

        public virtual IEnumerable<Entity> GetDeepEntity(Guid id)
        {
            Entity entSource = _dataService.GetEntityById(_entityTypeCode, id);
            entSource.ResolveReferences(_dataService.GetEntityTypes());
            return new List<Entity>() { entSource };
        }

        public virtual int GetDepth(Entity ent) { return 0; }
        public virtual void SetDepth(Entity ent, int depth) {  } 
        public virtual EntityType GeEntityType()
        {
            EntityType entType = _dataService.GetEntityTypes()[_entityTypeCode];
            return entType;
        }
        public virtual Entity Create(bool isParentTreeEntity)
        {
            Entity newEnt = _factory.NewEntity(_entityTypeCode);
            newEnt.CreaAttributi();
            return newEnt;
        }

        internal void SetEntityAttributoValore(EntityAttributo entAtt, Valore valore)
        {
            if (entAtt.Attributo.ValoreAttributo != null)
            {
                //entAtt.Attributo.ValoreAttributo.UpdateOnNewValore(valore);
                entAtt.Attributo.ValoreAttributo.UpdatePlainText(valore);
            }

            entAtt.Valore = valore;
        }

    }

    public class TreeEntityService : EntityService
    {
        public TreeEntityService(IDataService dataService, string entityTypeCode, Factory factory = null) : base(dataService, entityTypeCode, factory) { }

        public override IEnumerable<Entity> GetDeepEntity(Guid id)
        {
            IEnumerable<Entity> entsSource = _dataService.GetTreeEntitiesDeepById(_entityTypeCode, new List<Guid>(){ id});
            return entsSource;
        }

        public override int GetDepth(Entity ent)
        {
            TreeEntity treeEnt = ent as TreeEntity;
            if (treeEnt != null)
                return treeEnt.Depth;

            return 0;
        }

        public override void SetDepth(Entity ent, int depth)
        {
            TreeEntity treeEnt = ent as TreeEntity;
            if (treeEnt != null)
                treeEnt.Depth = depth;
        }

        public override Entity Create(bool isParentTreeEntity)
        {
            if (_factory == null)
                return null;

            if (isParentTreeEntity)
            {
                TreeEntityType treeEntType = GeEntityType() as TreeEntityType;
                TreeEntity newEnt = _factory.NewEntity(treeEntType.AssociedType.GetKey()) as TreeEntity;
                newEnt.CreaAttributiParent();
                return newEnt;
            }
            else
            {
                Entity newEnt = _factory.NewEntity(_entityTypeCode);
                newEnt.CreaAttributi();
                return newEnt;
            }
            
        }





    }
}
