

using _3DModelExchange;
using Commons;
using MasterDetailModel;
using Model.Calculators;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;



namespace Model
{

    public interface IDataService
    {
        public event EventHandler<ProgressEventArgs> ProgressChanged;

        Dictionary<string, DefinizioneAttributo> GetDefinizioniAttributo();
        
        //EntityType
        Dictionary<string, EntityType> GetEntityTypes();
        bool SetEntityType(EntityType entityType, bool removeInvalidRiferimenti, bool forceUpdateEntities = false);
        EntityType GetEntityType(string entTypeKey);

        //Get entities Id
        List<EntityMasterInfo> GetFilteredEntities(string entTypeCode, FilterData filter, SortData sort, GroupData group, out List<Guid> entitiesFound);
        Dictionary<string, Guid> CreateKey(string entityTypeKey, string separator, List<string> attributiCodice, out Dictionary<Guid, string> keysById);
        HashSet<Guid> ApplyFilter(string entTypeCode, HashSet<Guid> entitiesToFilter, FilterData filter);
        List<Guid> GetDependentIds(string entityTypeKey, Guid id, string dependentIdsEntityTypeKey);

        //Fetch entities
        List<Entity> GetEntitiesById(string entTypeCode, IEnumerable<Guid> ids);
        Entity GetEntityById(string entTypeKey, Guid id);
        List<TreeEntityMasterInfo> GetFilteredTreeEntities(string entTypeCode, FilterData filter, SortData sort, out List<Guid> entitiesFound);
        List<TreeEntity> GetTreeEntitiesById(string entTypeCode, IEnumerable<Guid> ids, bool compactFormat = false);
        IEnumerable<TreeEntity> GetTreeEntitiesDeepById(string entTypeCode, IEnumerable<Guid> ids);

        //Varie
        List<EntityAttributo> GetAttributiValoriComuni(string entityTypeCode, List<Guid> entitiesId);
        ModelActionResponse CommitAction(ModelAction action);
        //Dictionary<string, HashSet<SuggestionPackage>> GetSuggestions();
        Task<List<string>> GetValoriUnivociAsync(string entityTypeKey, List<Guid> entitiesId, string codiceAttributo, int takeResults, string textSearched);
        Entity NewEntity(string codice);
        bool GetGroupRecordsData(GroupData groupData);
        bool IsReadOnly { get; }
        HashSet<Guid> CalcolaEntities(string entityTypeKey, EntityCalcOptions options, List<Guid> entitiesId = null, EntitiesError error = null);
        List<EntitiesError> GetEntitiesErrors();

        //ViewSettings
        ViewSettings GetViewSettings();
        void SetViewSettings(ViewSettings viewSettings);

        //Divisioni
        Guid AddDivisione(string codice, string name = null, Model3dClassEnum model3dClassName = Model3dClassEnum.Nothing);
        bool RemoveDivisione(Guid id);
        bool RenameDivisione(Guid id, string newName, string codice = null);
        bool SortDivisioni(Dictionary<Guid, int> divTypesIdPos);

        //Model3d FiltersData
        Model3dFiltersData GetModel3dFiltersData();
        bool SetModel3dFiltersData(Model3dFiltersData filtersData);

        //Model3d TagsData
        Model3dTagsData GetModel3dTagsData();

        //Model 3d files Info
        Model3dFilesInfo GetModel3dFilesInfo();
        bool SetModel3dFilesInfo(Model3dFilesInfo filesInfo);
        Model3dValuesData GetModel3dValuesData();

        //Model3d RotoTranslation
        void SetModel3dUserRotoTranslation(Model3dUserRotoTranslation rotoTra);
        Model3dUserRotoTranslation GetModel3dUserRotoTranslation();

        //Model 3d values
        bool SetModel3dValuesData(Model3dValuesData model3dValuesData);
        bool PrepareBeforeSave();
        bool UpdateValuesFromModel3d(Model3dType model3dType, bool alsoByService);
        bool CalculateModel3dValues(Model3dValues model3dValues);
        bool ClearValuesFromModel3d(Model3dType model3dType);

        //Import Entities
        bool ImportEntities(EntitiesImportStatus entitiesImportStatus);

        //Formati numerici
        List<NumericFormat> GetNumericFormats();
        void SetNumericFormats(List<NumericFormat> numberFormats);

        //Undo/Redo
        Task<List<Patch>> GetProjectPatchsAsync(bool reset = false);
        Project ProjectUndo(List<Patch> patchs);

        //Gantt
        GanttData GetGanttData();
        void SetGanttData(GanttData ganttData);
        Task<bool> CreateWBSItems(WBSItemsCreationData data);

        //FogliDiCalcolo
        FogliDiCalcoloData GetFogliDiCalcoloData();
        void SetFogliDiCalcoloData(FogliDiCalcoloData fogliDiCalcoloData);
        bool SetModel3dTagsData(Model3dTagsData tagsData);
        Model3dPreferencesData GetModel3dPreferencesData();
        bool SetModel3dPreferencesData(Model3dPreferencesData preferencesData);
        Model3dUserViewList GetModel3dUserViews();
        bool SetModel3dUserViews(Model3dUserViewList userViews);
    }

    public class ProgressEventArgs : EventArgs
    {
        public string Message { get; set; }
        public int ProgressValue { get; set; }//%
    }

}