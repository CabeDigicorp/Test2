


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _3DModelExchange;
using Commons;
using DevExpress.Mvvm.POCO;
using DevExpress.XtraPrinting.Native;
using MasterDetailModel;
using Model.Calculators;


namespace Model
{

    public class ClientDataService : IDataService
    {
        IDataService _dataService = null;
        ModelActionsStack _modelActionsStack = null;

        public event EventHandler<ProgressEventArgs> ProgressChanged;

        public bool Suspended
        {
            get;
            set;
        } = false;
        public bool IsReadOnly { get; set; } = false;

        Dictionary<string, DefinizioneAttributo> DefinizioniAttributo { get; set; } = new Dictionary<string, DefinizioneAttributo>();
        Dictionary<string, EntityType> EntityTypes { get; set; } = new Dictionary<string, EntityType>();


        public ClientDataService(IDataService dataService, ModelActionsStack modelActionsStack)
        {
            _dataService = dataService;
            _modelActionsStack = modelActionsStack;
        }

        public void Init()
        {
            ResetCache();
            _dataService.ProgressChanged += _dataService_ProgressChanged;
        }

        public IDataService Service { get => _dataService; }

        private void _dataService_ProgressChanged(object sender, ProgressEventArgs e)
        {
            ProgressChanged?.Invoke(sender, e);
        }

        public void ResetCache()
        {
            DefinizioniAttributo = _dataService.GetDefinizioniAttributo();

            EntityTypes = _dataService?.GetEntityTypes();
            foreach (EntityType entType in EntityTypes.Values)
                entType.ResolveReferences(EntityTypes, DefinizioniAttributo);
        }

        public Dictionary<string, DefinizioneAttributo> GetDefinizioniAttributo()
        {
            return DefinizioniAttributo;
        }

        public Dictionary<string, EntityType> GetEntityTypes()
        {
            return EntityTypes;

            //Dictionary<string, EntityType> entityTypes = _dataService.GetEntityTypes();
            //foreach (EntityType entType in entityTypes.Values)
            //    entType.ResolveReferences(entityTypes, _dataService.GetDefinizioniAttributo());

            //return entityTypes;
        }

        public EntityType GetEntityType(string entTypeKey)
        {
            Dictionary<string, EntityType> entTypes = GetEntityTypes();
            if (entTypes.ContainsKey(entTypeKey))
                return entTypes[entTypeKey];

            return null;
        }

        public bool SetEntityType(EntityType entityType, bool removeInvalidRiferimenti, bool forceUpdateEntities = false)
        {
            bool res = _dataService.SetEntityType(entityType, removeInvalidRiferimenti, forceUpdateEntities);
            if (res)
            {
                Init();
            }


            return true;
        }

       
        public List<TreeEntity> GetTreeEntitiesById(string entTypeCode, IEnumerable<Guid> ids, bool compactFormat = false)
        {
            List<TreeEntity> entities = _dataService.GetTreeEntitiesById(entTypeCode, ids, compactFormat);


            TreeEntityCollection collection = new TreeEntityCollection();
            collection.TreeEntities = new List<TreeEntity>();

            foreach (TreeEntity ent in entities)
            {
                collection.TreeEntities.Add(ent);
            }

            collection.ResolveAllReferences(_dataService.GetEntityTypes());
            return collection.TreeEntities;
        }

        public List<TreeEntity> GetCloneTreeEntitiesById(string entTypeCode, IEnumerable<Guid> ids)
        {
            List<TreeEntity> entities = _dataService.GetTreeEntitiesById(entTypeCode, ids);


            TreeEntityCollection collection = new TreeEntityCollection();
            collection.TreeEntities = new List<TreeEntity>();

            foreach (TreeEntity ent in entities)
            {
                collection.TreeEntities.Add((TreeEntity) ent.Clone());
            }

            collection.ResolveAllReferences(_dataService.GetEntityTypes());
            return collection.TreeEntities;
        }


        /// <summary>
        /// Ritorna le entità richieste con i padri nell'attributo Parent di TreeEntity
        /// </summary>
        /// <param name="entTypeCode"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        public IEnumerable<TreeEntity> GetTreeEntitiesDeepById(string entTypeCode, IEnumerable<Guid> ids)
        {
            IEnumerable<TreeEntity> entities = _dataService.GetTreeEntitiesDeepById(entTypeCode, ids);
            foreach (TreeEntity ent in entities)
                ent.ResolveReferences(_dataService.GetEntityTypes());

            return entities.Where(item => ids.Contains(item.EntityId));
        }

        public List<Entity> GetEntitiesById(string entTypeCode, IEnumerable<Guid> ids)
        {
            List<Entity> entities = _dataService.GetEntitiesById(entTypeCode, ids);
            foreach (Entity ent in entities)
            {
                if (ent != null)
                    ent.ResolveReferences(_dataService.GetEntityTypes());
            }
            return entities;
        }

        public List<Entity> GetCloneEntitiesById(string entTypeCode, IEnumerable<Guid> ids)
        {
            List<Entity> entities = _dataService.GetEntitiesById(entTypeCode, ids);
            List<Entity> cloneEnts = new List<Entity>();
            foreach (Entity ent in entities)
            {
                if (ent != null)
                {
                    Entity clone = ent.Clone();
                    clone.ResolveReferences(_dataService.GetEntityTypes());
                    cloneEnts.Add(clone);
                }
            }
            return cloneEnts;
        }



        public Entity GetEntityById(string entTypeKey, Guid id)
        {
            Entity ent = _dataService.GetEntityById(entTypeKey, id);
            if (ent != null)
                ent.ResolveReferences(_dataService.GetEntityTypes());

            return ent;
        }


        //public List<EntityMasterInfo> GetIndexedEntities(string entTypeCode, FilterView filter, SortView sort, GroupView group, out List<Guid> entitiesFound)
        //{
        //    return _dataService.GetIndexedEntities(entTypeCode, filter.Data, sort.GetData(), group.Data, out entitiesFound);
        //}



        public List<EntityMasterInfo> GetFilteredEntities(string entTypeCode, FilterData filterData, SortData sortData, GroupData groupData, out List<Guid> entitiesFound)
        {
            return _dataService.GetFilteredEntities(entTypeCode, filterData, sortData, groupData, out entitiesFound);
        }

        public HashSet<Guid> ApplyFilter(string entTypeCode, HashSet<Guid> entitiesToFilter, FilterData filter)
        {
            return _dataService.ApplyFilter(entTypeCode, entitiesToFilter, filter);
        }



        //public List<TreeEntityMasterInfo> GetIndexedTreeEntities(string entTypeCode, FilterView filter, SortView sort, out List<Guid> entitiesFound) => _dataService.GetIndexedTreeEntities(entTypeCode, filter.Data, sort.GetData(), out entitiesFound);

        public List<TreeEntityMasterInfo> GetFilteredTreeEntities(string entTypeCode, FilterData filterData, SortData sortData, out List<Guid> entitiesFound) => _dataService.GetFilteredTreeEntities(entTypeCode, filterData, sortData, out entitiesFound);

        public Dictionary<string, Guid> CreateKey(string entityTypeKey, string separator, List<string> attributiCodice, out Dictionary<Guid, string> keysById)
        {
            return _dataService.CreateKey(entityTypeKey, separator, attributiCodice, out keysById);
        }

        /// <summary>
        /// Original
        /// </summary>
        /// <param name="entityTypeCode"></param>
        /// <param name="entitiesId"></param>
        /// <returns></returns>
        public List<EntityAttributo> GetAttributiValoriComuni(string entityTypeCode, List<Guid> entitiesId)
        {
            return _dataService.GetAttributiValoriComuni(entityTypeCode, entitiesId);
        }

        /// <summary>
        /// Original
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public ModelActionResponse CommitAction(ModelAction action)
        {
            //return _dataService.CommitAction(action);
            ModelActionResponse res = _dataService.CommitAction(action);
            if (res.ActionResponse == ActionResponse.OK )
            {
                _modelActionsStack.CommitAction(action);
            }

            return res;
        }



        //public class SuggestionService
        //{
            //public Dictionary<string, HashSet<SuggestionPackage>> GetSuggestions()
            //{
            //    return _dataService.GetSuggestions();
            //}
        //}


        public async Task<List<string>> GetValoriUnivociAsync(string entityTypeKey, List<Guid> entitiesId, string codiceAttributo, int takeResults, string textSearched)
        {
            return await _dataService.GetValoriUnivociAsync(entityTypeKey, entitiesId, codiceAttributo, takeResults, textSearched);
        }

        public bool CalculateModel3dValues(Model3dValues model3dValues)
        {
            return _dataService.CalculateModel3dValues(model3dValues);
        }

        public Entity NewEntity(string codice)
        {
            return _dataService.NewEntity(codice);
        }

        public bool FillGroupData(GroupData groupData)
        {
            return _dataService.GetGroupRecordsData(groupData);
        }

        public ViewSettings GetViewSettings()
        {
            return _dataService.GetViewSettings();
        }

        public void SetViewSettings(ViewSettings viewSettings)
        {
            _dataService.SetViewSettings(viewSettings);
            _modelActionsStack.CommitAction(new ModelAction() { ActionName = ActionName.VIEW_SETTINGS });
        }

        public bool GetGroupRecordsData(GroupData groupData)
        {
            return _dataService.GetGroupRecordsData(groupData);
        }

        public Guid AddDivisione(string codice, string name = null, Model3dClassEnum model3dClassName = Model3dClassEnum.Nothing)
        {
            Guid newId = _dataService.AddDivisione(codice, name, model3dClassName);
            ResetCache();

            if (newId != Guid.Empty)
                _modelActionsStack.CommitAction(new ModelAction() { ActionName = ActionName.DIVISIONE_ADD });

            return newId;
        }

        public bool RemoveDivisione(Guid id)
        {
            bool res =  _dataService.RemoveDivisione(id);

            if (res)
            {
                _modelActionsStack.CommitAction(new ModelAction() { ActionName = ActionName.DIVISIONE_REMOVE });
            }

            ResetCache();
            return res;
        }

        public bool RenameDivisione(Guid id, string newName, string codice = null)
        {
            if (id == Guid.Empty)
                return false;

            bool res =  _dataService.RenameDivisione(id, newName, codice);

            if (res)
                _modelActionsStack.CommitAction(new ModelAction() { ActionName = ActionName.DIVISIONE_RENAME });

            ResetCache();
            return res;
        }

        public bool SortDivisioni(Dictionary<Guid, int> divTypesIdPos)
        {
            if (divTypesIdPos == null)
                return false;

            bool res = _dataService.SortDivisioni(divTypesIdPos);

            if (res)
                _modelActionsStack.CommitAction(new ModelAction() { ActionName = ActionName.DIVISIONI_SORT });

            ResetCache();
            return res;
        }

        public Model3dFiltersData GetModel3dFiltersData()
        {
            return _dataService?.GetModel3dFiltersData();
        }

        public bool SetModel3dFiltersData(Model3dFiltersData filtersData)
        {
            var ret = _dataService.SetModel3dFiltersData(filtersData);

            if (ret)
                _modelActionsStack.CommitAction(new ModelAction() { ActionName = ActionName.SETMODEL3D_FILTERDATA });
            return ret;
        }

        public Model3dTagsData GetModel3dTagsData()
        {
            return _dataService.GetModel3dTagsData();
        }

        public Model3dFilesInfo GetModel3dFilesInfo()
        {
            return _dataService.GetModel3dFilesInfo().Clone();
        }

        public bool SetModel3dFilesInfo(Model3dFilesInfo filesInfo)
        {
            var ret = _dataService.SetModel3dFilesInfo(filesInfo);

            if (ret)
                _modelActionsStack.CommitAction(new ModelAction() { ActionName = ActionName.SETMODEL3D_FILEINFO });
            return ret;
        }

        public Model3dValuesData GetModel3dValuesData()
        {
            return _dataService.GetModel3dValuesData();
        }

        public bool SetModel3dValuesData(Model3dValuesData model3dValuesData)
        {
            return _dataService.SetModel3dValuesData(model3dValuesData);
        }

        public bool PrepareBeforeSave()
        {
            return _dataService.PrepareBeforeSave();
        }

        public bool UpdateValuesFromModel3d(Model3dType model3dType, bool alsoByService)
        {
            return _dataService.UpdateValuesFromModel3d(model3dType, alsoByService);
        }

        public bool ClearValuesFromModel3d(Model3dType model3dType)
        {
            return _dataService.ClearValuesFromModel3d(model3dType);
        }

        public bool ImportEntities(EntitiesImportStatus entitiesImportStatus)
        {
            bool res = _dataService.ImportEntities(entitiesImportStatus);

            if (res)
                _modelActionsStack.CommitAction(new ModelAction() { ActionName = ActionName.ENTITIES_IMPORT });

            return res;
        }


        public void SetNumericFormats(List<NumericFormat> numberFormats)
        {
            _dataService.SetNumericFormats(numberFormats);
        }

        public List<NumericFormat> GetNumericFormats()
        {
            return _dataService.GetNumericFormats();
        }


        Task<List<Patch>> IDataService.GetProjectPatchsAsync(bool reset = false)
        {
            return _dataService.GetProjectPatchsAsync(reset);
        }

        public Project ProjectUndo(List<Patch> patchs)
        {
            return _dataService.ProjectUndo(patchs);
        }

        public List<Guid> GetDependentIds(string entityTypeKey, Guid id, string dependentIdsEntityTypeKey)
        {
            return _dataService.GetDependentIds(entityTypeKey, id, dependentIdsEntityTypeKey);
        }

        public void SetModel3dUserRotoTranslation(Model3dUserRotoTranslation rotoTra)
        {
            _dataService.SetModel3dUserRotoTranslation(rotoTra);
            _modelActionsStack.CommitAction(new ModelAction() { ActionName = ActionName.SETMODEL3D_USERROTOTRANSLATION });
        }

        public Model3dUserRotoTranslation GetModel3dUserRotoTranslation()
        {
            return _dataService.GetModel3dUserRotoTranslation();
        }

        public GanttData GetGanttData()
        {
            return _dataService.GetGanttData();
        }

        public void SetGanttData(GanttData ganttData)
        {
            _dataService.SetGanttData(ganttData);
            _modelActionsStack.CommitAction(new ModelAction() { ActionName = ActionName.SAVE_GANTTDATA });
        }

        public async Task<bool> CreateWBSItems(WBSItemsCreationData data)
        {
            bool ok = await _dataService.CreateWBSItems(data);

            if (ok)
                _modelActionsStack.CommitAction(new ModelAction() { ActionName = ActionName.CREATE_WBS_ITEMS, EntityTypeKey = BuiltInCodes.EntityType.WBS });
            
            return ok;
        }

        public HashSet<Guid> CalcolaEntities(string entityTypeKey, EntityCalcOptions options, List<Guid> entitiesId = null, EntitiesError error = null)
        {
            HashSet<Guid> calculated = null;

            calculated = _dataService.CalcolaEntities(entityTypeKey, options, entitiesId, error);

           _modelActionsStack.CommitAction(new ModelAction() { EntityTypeKey = entityTypeKey,  ActionName = ActionName.CALCOLA_ENTITES });

            return calculated;
        }

        public FogliDiCalcoloData GetFogliDiCalcoloData()
        {
            return _dataService.GetFogliDiCalcoloData();
        }

        public void SetFogliDiCalcoloData(FogliDiCalcoloData fogliDiCalcoloData)
        {
            _dataService.SetFogliDiCalcoloData(fogliDiCalcoloData);
            _modelActionsStack.CommitAction(new ModelAction() { ActionName = ActionName.SAVE_FOGLIDICALCOLODATA });
        }

        public List<EntitiesError> GetEntitiesErrors()
        {
            return _dataService.GetEntitiesErrors();
        }

        public bool SetModel3dTagsData(Model3dTagsData tagsData)
        {
            var ret = _dataService.SetModel3dTagsData(tagsData);
            if (ret)
                _modelActionsStack.CommitAction(new ModelAction() { ActionName = ActionName.SETMODEL3D_TAGSDATA });

            return ret;
        }

        public Model3dPreferencesData GetModel3dPreferencesData()
        {
            return _dataService.GetModel3dPreferencesData();
        }

        public bool SetModel3dPreferencesData(Model3dPreferencesData preferencesData)
        {
            var ret = _dataService.SetModel3dPreferencesData(preferencesData);
            if (ret)
                _modelActionsStack.CommitAction(new ModelAction() { ActionName = ActionName.SETMODEL3D_PREFERENCESDATA });

            return ret;
        }

        public Model3dUserViewList GetModel3dUserViews()
        {
            return _dataService.GetModel3dUserViews();
        }

        public bool SetModel3dUserViews(Model3dUserViewList userViews)
        {
            var ret = _dataService.SetModel3dUserViews(userViews);
            if (ret)
                _modelActionsStack.CommitAction(new ModelAction() { ActionName = ActionName.SETMODEL3D_USERVIEWS });

            return ret;
        }
    }

}