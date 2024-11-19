


using System.Collections.Generic;
using _3DModelExchange;
using Model;
using System;
using MasterDetailModel;
using System.Linq;
using MasterDetailView;
using System.IO;
using System.Threading.Tasks;



namespace MainApp
{

    /// <summary>
    /// Classe che implementa i servizi richiesti da model 3d viewer
    /// </summary>
    public class MainAppService : IMainAppService
    {
        MainView _mainView = null;
        public MainView MainView { get => _mainView; }

        //impostazioni all'avvio del dialogo selectPrezzarioItems
        EntityTypeViewSettings _selectPrezzarioItemsViewSettings = null;
        string _selectPrezzarioItemsCurrentPrezzario = string.Empty;


        public MainAppService(MainView mainView)
        {
            _mainView = mainView;
        }



        public bool SetCurrentProjectModel3dFilters(FiltersData filtersData)
        {

            if (MainView.MainMenuView.ClientDataService == null)
                return false;

            var model3dType = MainView.MainMenuView.Model3dService.GetModel3dType();

            //sostituisco solo quelli di model3dType
            Model3dFiltersData model3dFiltersData = Model3dFiltersDataConverter.ConvertFromFiltersData(filtersData, model3dType);

            Model3dFiltersData model3DFiltersDataAll = MainView.MainMenuView.ClientDataService?.GetModel3dFiltersData();
            if (model3dType == Model3dType.Ifc)
                model3DFiltersDataAll.Items.RemoveAll(item => item.IfcFilterConditions.Count > 0);
            if (model3dType == Model3dType.Revit)
                model3DFiltersDataAll.Items.RemoveAll(item => item.RvtFilter != null);

            model3DFiltersDataAll.Items.RemoveAll(item => item.IfcFilterConditions.Count == 0 && item.RvtFilter == null);

            model3DFiltersDataAll.Items.AddRange(model3dFiltersData.Items);

            return MainView.MainMenuView.ClientDataService.SetModel3dFiltersData(model3DFiltersDataAll);
        }

        public FiltersData GetCurrentProjectModel3dFilters()
        {
            //Model3dFiltersData model3DFiltersData = MainView.GetCurrentProjectModel3dFilters();
            Model3dFiltersData model3DFiltersData = MainView.MainMenuView.ClientDataService?.GetModel3dFiltersData();
            if (model3DFiltersData == null)
                return null;

            var model3dType = MainView.MainMenuView.Model3dService.GetModel3dType();

            FiltersData filtersData = Model3dFiltersDataConverter.ConvertToFiltersData(model3DFiltersData, MainView.MainMenuView.CurrentFileVersion, model3dType);

            //ricavo gli attributi che potranno essere coinvolti dalle regole
            EntityType computoType = MainView.MainMenuView.ClientDataService.GetEntityType(BuiltInCodes.EntityType.Computo);
            if (computoType != null)
            {
                var attributi = computoType.Attributi.Values.OrderBy(item => item.DetailViewOrder);

                IEnumerable<AttributoRegola> attsRegola = attributi.Where(item =>
                {
                    //la quantità ci và sempre 
                    if (item.Codice == BuiltInCodes.Attributo.Quantita)
                        return true;

                    if (item.Codice == BuiltInCodes.Attributo.Model3dRule)
                        return false;

                    if (item.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Testo
                    || item.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Reale
                    || item.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Contabilita)
                    {
                        return true;
                    }

                    return false;
                })
                .Select(item => 
                {
                    return new AttributoRegola()
                    {
                        EntityTypeKey = BuiltInCodes.EntityType.Computo,
                        Codice = item.Codice,
                        Etichetta = item.Etichetta,
                        Definizione = item.DefinizioneAttributoCodice,
                        Formula = item.ValoreDefault.ToPlainText(),
                        IsQuantita = (item.Codice == BuiltInCodes.Attributo.Quantita),
                        IsValoreLockedByDefault = item.IsValoreLockedByDefault,
                    };
                });
                
                filtersData.AttributiRegola = attsRegola.ToList();
            }
            return filtersData;
        }


        public List<Model3dPrezzarioItem> SelectPrezzarioItems(string windowTitle, Guid selectedPrezzarioItemId, bool isSingleSelection)
        {
            EntitiesHelper entsHelper = new EntitiesHelper(MainView.MainMenuView.ClientDataService);

            List<Model3dPrezzarioItem> model3dPrezzarioItems = new List<Model3dPrezzarioItem>();

            List<Guid> selectedItems = new List<Guid>() { selectedPrezzarioItemId };
            EntityTypeViewSettings viewSettings = null;

            string externalPrezzarioFileName = null;
            if (!isSingleSelection)
            {
                externalPrezzarioFileName = _selectPrezzarioItemsCurrentPrezzario;
                viewSettings = _selectPrezzarioItemsViewSettings;
            }

            SelectIdsWindowOptions opts = SelectIdsWindowOptions.Nothing;
            if (isSingleSelection)
                opts |= SelectIdsWindowOptions.IsSingleSelection;

            if (MainView.MainMenuView.WindowService.SelectPrezzarioIdsWindow(ref selectedItems, ref externalPrezzarioFileName,
                windowTitle, opts, true, true, true, ref viewSettings))
            {
                _selectPrezzarioItemsViewSettings = viewSettings;
                _selectPrezzarioItemsCurrentPrezzario = externalPrezzarioFileName;

                IEnumerable<Guid> prezzarioIds = null;

                Dictionary<string, IDataService> prezzariCache = MainView.MainMenuView.MainOperation.GetPrezzariCache();
                if (prezzariCache.ContainsKey(externalPrezzarioFileName))
                {
                    //Importazione nel prezzario interno degli articoli 

                    EntitiesImportStatus importStatus = new EntitiesImportStatus();
                    importStatus.TargetPosition = TargetPosition.Bottom;
                    importStatus.ConflictAction = EntityImportConflictAction.Undefined;
                    importStatus.Source = prezzariCache[externalPrezzarioFileName];
                    importStatus.SourceName = externalPrezzarioFileName;
                    selectedItems.ForEach(item => importStatus.StartingEntitiesId.Add(new EntityImportId() { SourceId = item, SourceEntityTypeKey = PrezzarioItemType.CreateKey() }));

                    //MainView.MainMenuView.ClientDataService.ImportEntities(importStatus);
                    while (importStatus.Status != EntityImportStatusEnum.Completed)
                    {
                        MainView.MainMenuView.ClientDataService.ImportEntities(importStatus);
                        if (importStatus.Status == EntityImportStatusEnum.Waiting)
                        {
                            if (!MainView.MainMenuView.WindowService.EntitiesImportWindow(importStatus))
                                break;
                        }
                    }

                    prezzarioIds = importStatus.StartingEntitiesId.Select(item => item.TargetId);

                    MainView.MainMenuView.MainOperation.UpdateEntityTypesView(new List<string>(importStatus.EntityTypes.Keys));

                }
                else
                {
                    prezzarioIds = selectedItems;
                }

                //List<Entity> prezzarioItems = MainView.MainMenuView.ClientDataService.GetEntitiesById(BuiltInCodes.EntityType.Prezzario, prezzarioIds);
                IEnumerable<TreeEntity> prezzarioItems = MainView.MainMenuView.ClientDataService.GetTreeEntitiesDeepById(BuiltInCodes.EntityType.Prezzario, prezzarioIds);

                foreach (PrezzarioItem prezItem in prezzarioItems)
                {
                    Model3dPrezzarioItem model3DPrezzarioItem = new Model3dPrezzarioItem();

                    model3DPrezzarioItem.Id = prezItem.EntityId;

                    //codice
                    Valore valCod = prezItem.GetValoreAttributo("Codice", false, true);
                    if (valCod != null)
                        model3DPrezzarioItem.Codice = valCod.ToPlainText();

                    //desc

                    model3DPrezzarioItem.Descrizione = entsHelper.GetValorePlainText(prezItem, BuiltInCodes.Attributo.DescrizioneRTF, true, true);

//                    ValoreTestoRtf valDesc = prezItem.GetValoreAttributo("DescrizioneRTF", true, true) as ValoreTestoRtf;
//                    if (valDesc != null)
//                        model3DPrezzarioItem.Descrizione = valDesc.BriefPlainText;


                    //UM
                    Valore valUnitaMisura = prezItem.GetValoreAttributo("UnitaMisura", false, true);
                    if (valUnitaMisura != null)
                        model3DPrezzarioItem.UM = valUnitaMisura.ToPlainText();

                    //Prezzo
                    //string valoreformat = prezItem.Attributi["Prezzo"].Attributo.ValoreFormat;
                    //string valoreformat = MainView.MainMenuView.ElencoPrezziView.PrezzarioView.ItemsView.AttributoFormatHelper.GetValorePaddedFormat(prezItem.Attributi["Prezzo"]);
                    string valoreformat = MainView.MainMenuView.ElencoPrezziView.PrezzarioView.ItemsView.AttributoFormatHelper.GetValorePaddedFormat(prezItem, BuiltInCodes.Attributo.Prezzo);
                    ValoreContabilita valPrezzo = prezItem.GetValoreAttributo("Prezzo", false, true) as ValoreContabilita;
                    if (valPrezzo != null)
                    {
                        //model3DPrezzarioItem.Prezzo = valPrezzo.FormatRealResult(valoreformat);
                        //model3DPrezzarioItem.Prezzo = valPrezzo.ToPlainText();
                        model3DPrezzarioItem.Prezzo = valPrezzo.RealResult.ToString();
                        model3DPrezzarioItem.PrezzoFormat = valoreformat;
                    }

                    //Formato relativo all'unità di misura
                    //string umFormat = MainView.MainMenuView.ElencoPrezziView.PrezzarioView.ItemsView.AttributoFormatHelper.GetUnitaMisuraPaddedFormat(model3DPrezzarioItem.UM);
                    Valore valFormatoQta = prezItem.GetValoreAttributo(BuiltInCodes.Attributo.FormatoQuantita, false, true);
                    model3DPrezzarioItem.QuantitaFormat = valFormatoQta.ToPlainText();

                    //Prezzario
                    Valore valPrezzario = prezItem.GetValoreAttributo(BuiltInCodes.Attributo.Origine, false, true);
                    if (valPrezzario != null)
                        model3DPrezzarioItem.Prezzario = valPrezzario.ToPlainText();


                    model3dPrezzarioItems.Add(model3DPrezzarioItem);
                }
            }


            return model3dPrezzarioItems;

        }

        public void AddElementi(List<Model3dObjectKey> elmsKey, bool addToGroup)  
        {
            MainView.MainMenuView.MainOperation.AddElementi(elmsKey, addToGroup);
        }

        public void UpdateModel3dValues()
        {
            MainView.MainMenuView.UpdateValuesFromModel3d();
        }

        public void ApplyComputoRules()
        {
            MainView.MainMenuView.MainOperation.ApplyComputoRules();
        }

        /// <summary>
        /// Ritorna gli articoli aggiornati
        /// NB.Se si fa riferimento al prezzario interno la ricerca avviene per Id, se si fa riferimento a un prezzario esterno la ricerca avviene per Codice e nome prezzario
        /// </summary>
        /// <param name="model3dPrezzarioItemsOld"></param>
        /// <returns></returns>
        public List<Model3dPrezzarioItem> GetUpdatedPrezzarioItems(List<Model3dPrezzarioItem> model3dPrezzarioItemsOld)
        {
            ClientDataService dataService = MainView.MainMenuView.ClientDataService;

            EntitiesHelper entsHelper = new EntitiesHelper(dataService);
            
            //////////////////////////////////////////////
            //Per la ricerca per codice e nome prezzario (key)
            //mappa degli elementi del prezzario interno per nome prezzario e codice articolo
            Dictionary<string, Guid> prezzarioInternoIdsByKey = null;
            Dictionary<Guid, Entity> prezzarioInternoItemsByKey = null;
            /////////////////////////////////////////////



            IEnumerable<Guid> prezzarioInternoSubsetItemsId = model3dPrezzarioItemsOld.Select(item => item.Id);

            //costruisco una mappa per id degli prezzarioItems
            List<Entity> ents = dataService.GetEntitiesById(BuiltInCodes.EntityType.Prezzario, prezzarioInternoSubsetItemsId);
            Dictionary<Guid, Entity> prezzarioInternoItems = new Dictionary<Guid, Entity>();

            ents.ForEach(item =>
            {
                if (!prezzarioInternoItems.ContainsKey(item.EntityId))
                    prezzarioInternoItems.Add(item.EntityId, item);
            });

            


            List<Model3dPrezzarioItem> model3dPrezzarioItems = new List<Model3dPrezzarioItem>();

            foreach (Model3dPrezzarioItem m3dprezItemOld in model3dPrezzarioItemsOld)
            {
                Model3dPrezzarioItem model3DPrezzarioItem = new Model3dPrezzarioItem();



                PrezzarioItem prezItem = null;

                if (prezzarioInternoItems.ContainsKey(m3dprezItemOld.Id))
                {
                    //Trovato nel prezzario interno un articolo con stesso id


                    model3DPrezzarioItem.Id = m3dprezItemOld.Id;
                    prezItem = prezzarioInternoItems[m3dprezItemOld.Id] as PrezzarioItem;
                }
                else
                {
                    //Non trovato nel prezzario interno l'id cercato.
                    //Cerco per codice e nome prezzario

                    if (prezzarioInternoItemsByKey == null)
                    {
                        prezzarioInternoIdsByKey = dataService.CreateKey(BuiltInCodes.EntityType.Prezzario, "|", new List<string>() { BuiltInCodes.Attributo.Origine, BuiltInCodes.Attributo.Codice }, out _);

                        IEnumerable<string> keys = model3dPrezzarioItemsOld.Select(item => string.Format("{0}|{1}", item.Prezzario, item.Codice));
                        IEnumerable<Guid> keysIds = prezzarioInternoIdsByKey.Where(item => keys.Contains(item.Key)).Select(item => item.Value);
                        List<Entity> keysEnt = dataService.GetEntitiesById(BuiltInCodes.EntityType.Prezzario, keysIds);
                        prezzarioInternoItemsByKey = new Dictionary<Guid, Entity>();

                        keysEnt.ForEach(item =>
                        {
                            if (!prezzarioInternoItemsByKey.ContainsKey(item.EntityId))
                                prezzarioInternoItemsByKey.Add(item.EntityId, item);
                        });
                    }

                    string prezzarioCodiceKey = string.Join("|", m3dprezItemOld.Prezzario, m3dprezItemOld.Codice);
                    if (prezzarioInternoIdsByKey.ContainsKey(prezzarioCodiceKey))
                    {
                        //trovato articolo con stesso codice e nome prezzario
                        Guid newId = prezzarioInternoIdsByKey[prezzarioCodiceKey];
                        model3DPrezzarioItem.Id = newId;
                        prezItem = prezzarioInternoItemsByKey[newId] as PrezzarioItem;
                    }
                }


                if (prezItem != null)
                {

                    //codice
                    Valore valCod = prezItem.GetValoreAttributo("Codice", false, true);
                    if (valCod != null)
                        model3DPrezzarioItem.Codice = valCod.ToPlainText();

                    //desc
                    model3DPrezzarioItem.Descrizione = entsHelper.GetValorePlainText(prezItem, BuiltInCodes.Attributo.DescrizioneRTF, true, true);
                    //ValoreTestoRtf valDesc = prezItem.GetValoreAttributo("DescrizioneRTF", true, true) as ValoreTestoRtf;
                    //if (valDesc != null)
                    //    model3DPrezzarioItem.Descrizione = valDesc.BriefPlainText;

                    //UM
                    Valore valUnitaMisura = prezItem.GetValoreAttributo("UnitaMisura", false, true);
                    if (valUnitaMisura != null)
                    {
                        model3DPrezzarioItem.UM = valUnitaMisura.ToPlainText();

                        //Formato relativo all'unità di misura
                        //string umFormat = MainView.MainMenuView.ElencoPrezziView.PrezzarioView.ItemsView.AttributoFormatHelper.GetUnitaMisuraPaddedFormat(model3DPrezzarioItem.UM);
                        Valore valFormatoQta = prezItem.GetValoreAttributo(BuiltInCodes.Attributo.FormatoQuantita, false, true);
                        model3DPrezzarioItem.QuantitaFormat = valFormatoQta.ToPlainText();
                    }

                    //Prezzo
                    ValoreContabilita valPrezzo = prezItem.GetValoreAttributo("Prezzo", false, true) as ValoreContabilita;
                    if (valPrezzo != null)
                    {
                        //string valoreformat = MainView.MainMenuView.ElencoPrezziView.PrezzarioView.ItemsView.AttributoFormatHelper.GetValorePaddedFormat(prezItem.Attributi["Prezzo"]);
                        string valoreformat = MainView.MainMenuView.ElencoPrezziView.PrezzarioView.ItemsView.AttributoFormatHelper.GetValorePaddedFormat(prezItem, BuiltInCodes.Attributo.Prezzo);
                        //model3DPrezzarioItem.Prezzo = valPrezzo.ToPlainText();
                        //model3DPrezzarioItem.Prezzo = valPrezzo.FormatRealResult(valoreformat);
                        model3DPrezzarioItem.Prezzo = valPrezzo.RealResult.ToString();
                        model3DPrezzarioItem.PrezzoFormat = valoreformat;

                    }

                    
                    //Prezzario di provenienza
                    Valore valPrez = prezItem.GetValoreAttributo(BuiltInCodes.Attributo.Origine, false, true);
                    if (valPrez != null)
                        model3DPrezzarioItem.Prezzario = valPrez.ToPlainText();

                }
                else
                {
                    //Articolo non trovato nel prezzario interno
                    model3DPrezzarioItem.Prezzario = m3dprezItemOld.Prezzario;
                    model3DPrezzarioItem.Codice = m3dprezItemOld.Codice;
                    //model3DPrezzarioItem.Descrizione = m3dprezItemOld.Descrizione;


                }

                model3dPrezzarioItems.Add(model3DPrezzarioItem);
            }

            return model3dPrezzarioItems;
        }

        public void WindowActivate()
        {
            MainView.WindowActivate();
        }

        public void SelectElements(List<Model3dObjectKey> elementsId)
        {
            MainView.MainMenuView.MainOperation.SelectItemsByModel3d(elementsId);
        }

        public bool SetCurrentProjectModel3dTags(TagsData tagsData)
        {
            if (MainView.MainMenuView.ClientDataService == null)
                return false;

            Model3dTagsData model3dTagsData = Model3dTagsDataConverter.ConvertFromTagsData(tagsData);
            return MainView.MainMenuView.ClientDataService.SetModel3dTagsData(model3dTagsData);
        }

        public TagsData GetCurrentProjectModel3dTags()
        {
            Model3dTagsData model3DTagsData = MainView.MainMenuView.ClientDataService?.GetModel3dTagsData();
            if (model3DTagsData == null)
                return null;

            TagsData tagsData = Model3dTagsDataConverter.ConvertToTagsData(model3DTagsData, MainView.MainMenuView.CurrentFileVersion);
            return tagsData;
        }

        public bool SetCurrentProjectModel3dPreferences(PreferencesData preferencesData)
        {
            if (MainView.MainMenuView.ClientDataService == null)
                return false;

            Model3dPreferencesData model3dPreferencesData = Model3dPreferencesDataConverter.ConvertFromPreferencesData(preferencesData);
            return MainView.MainMenuView.ClientDataService.SetModel3dPreferencesData(model3dPreferencesData);
        }

        public PreferencesData GetCurrentProjectModel3dPreferences()
        {
            Model3dPreferencesData model3DPreferencesData = MainView.MainMenuView.ClientDataService?.GetModel3dPreferencesData();
            if (model3DPreferencesData == null)
                return null;

            PreferencesData preferencesData = Model3dPreferencesDataConverter.ConvertToPreferencesData(model3DPreferencesData, MainView.MainMenuView.CurrentFileVersion);
            return preferencesData;
        }

        public List<Model3dObjectKey> GetComputoModel3dObjectsKey()
        {
            List<Model3dObjectKey> model3dObjsKey = new List<Model3dObjectKey>();
            
            ClientDataService dataService = MainView.MainMenuView.ClientDataService;

            Dictionary<string, Guid> computoIdsByElementId = dataService.CreateKey(BuiltInCodes.EntityType.Computo, "|", new List<string>()
                                                                { BuiltInCodes.Attributo.ElementiItem_Guid}, out _);

            FilterData filterData = new FilterData();
            AttributoFilterData attFilter = new AttributoFilterData()
            {
                CodiceAttributo = BuiltInCodes.Attributo.GlobalId,
                CheckedValori = new HashSet<string>(computoIdsByElementId.Keys),
                EntityTypeKey = BuiltInCodes.EntityType.Elementi,
                IsFiltroAttivato = true,
            };
            filterData.Items.Add(attFilter);


            Dictionary<string, Guid> elementiIdsByModel3dObjectKey = dataService.CreateKey(BuiltInCodes.EntityType.Elementi, "|", new List<string>()
                                                                { BuiltInCodes.Attributo.ProjectGlobalId, BuiltInCodes.Attributo.GlobalId }, out _);


            foreach (string str in elementiIdsByModel3dObjectKey.Keys)
            {
                string strElId = elementiIdsByModel3dObjectKey[str].ToString();
                if (computoIdsByElementId.ContainsKey(strElId))
                {
                    string[] strArr = str.Split('|');
                    model3dObjsKey.Add(new Model3dObjectKey()
                    {
                        ProjectGlobalId = strArr[0],
                        GlobalId = strArr[1],
                    });
                }
            }

            return model3dObjsKey;
        }

        public bool SetCurrentProjectModel3dUserViews(UserViewList userViews)
        {
            if (MainView.MainMenuView.ClientDataService == null)
                return false;

            Model3dUserViewList model3dUserViews = Model3dUserViewsConverter.ConvertFromUserViews(userViews);
            return MainView.MainMenuView.ClientDataService.SetModel3dUserViews(model3dUserViews);
        }

        public UserViewList GetCurrentProjectModel3dUserViews()
        {
            Model3dUserViewList model3DUserViews = MainView.MainMenuView.ClientDataService.GetModel3dUserViews();
            if (model3DUserViews == null)
                return null;

            UserViewList userViews = Model3dUserViewsConverter.ConvertToUserViews(model3DUserViews, MainView.MainMenuView.CurrentFileVersion);
            return userViews;
        }

        public IMainAppProjectService GetProjectService(string fullFileName)
        {
            Int32 projectFileVersion = 0;
            IDataService dataService = MainView.MainMenuView.MainOperation.GetDataServiceByFile(fullFileName, out projectFileVersion);
            if (dataService == null)
                return null;

            MainAppProjectService mainAppProjectService = new MainAppProjectService(dataService, projectFileVersion);
            return mainAppProjectService;
        }

        public string GetAppSettingsPath()
        {
            return MainMenuView.AppSettingsPath;
        }

        public void SetUserRotoTranslation(Dictionary<UserTranslKey, UserTranslData> transl)
        {
            Model3dUserRotoTranslation result = Model3dUserRotoTranslationConverter.ConvertToUserRotoTranslation(transl);
            MainView.MainMenuView.ClientDataService.SetModel3dUserRotoTranslation(result);
        }

        public Dictionary<UserTranslKey, UserTranslData> GetUserRotoTranslation()
        {
            
            Int32 projectFileVersion = MainView.MainMenuView.CurrentFileVersion;
            Model3dUserRotoTranslation source = MainView.MainMenuView.ClientDataService.GetModel3dUserRotoTranslation();
            return Model3dUserRotoTranslationConverter.ConvertFromUserRotoTranslation(source, projectFileVersion);
        }

        public bool ImportPrezzarioItems(IMainAppProjectService source, List<Guid> sourceFiltersDataItemsSelected, ref Dictionary<Guid, Guid> prezzarioItemIdMap)
        {
            MainAppProjectService mainAppProjectService = source as MainAppProjectService;

            FiltersData filtersData = source.GetModel3dFilters();

            var selectedItems = filtersData.Items.Where(item => sourceFiltersDataItemsSelected.Contains(item.ID)).SelectMany(item => item.RulesIO.Select(item1 => item1.Id)).ToList();

            EntitiesImportStatus importStatus = new EntitiesImportStatus();
            importStatus.TargetPosition = TargetPosition.Bottom;
            importStatus.ConflictAction = EntityImportConflictAction.Undefined;
            importStatus.Source = mainAppProjectService.DataService;
            importStatus.SourceName = string.Empty;
            selectedItems.ForEach(item => importStatus.StartingEntitiesId.Add(new EntityImportId() { SourceId = item, SourceEntityTypeKey = PrezzarioItemType.CreateKey() }));

            while (importStatus.Status != EntityImportStatusEnum.Completed)
            {
                MainView.MainMenuView.ClientDataService.ImportEntities(importStatus);
                if (importStatus.Status == EntityImportStatusEnum.Waiting)
                {
                    if (!MainView.MainMenuView.WindowService.EntitiesImportWindow(importStatus))
                        break;
                }
            }

            //key: old prezzarioItemId, value: new prezzatioItemId
            prezzarioItemIdMap = importStatus.StartingEntitiesId.Where(item => item.TargetId != Guid.Empty).ToDictionary(item => item.SourceId, item => item.TargetId);

            MainView.MainMenuView.MainOperation.UpdateEntityTypesView(new List<string>(importStatus.EntityTypes.Keys));

            return false;
        }


        public Model3dType GetModel3DType()
        {
            if (MainView.MainMenuView.CurrentProject == null)
                return Model3dType.Unknown;

            if (MainView.MainMenuView.Model3dService == null)
                return Model3dType.Unknown;

            return MainView.MainMenuView.Model3dService.GetModel3dType();
        }

        public Model3dFiles GetCurrentProjectModel3dFiles()
        {

            if (MainView.MainMenuView.CurrentProject == null)
                return null;

            var task = Task.Run(async () => await MainView.MainMenuView.Model3dFilesInfoView.GetFilesToProcess(GetModel3DType()));
            Model3dFiles model3dFiles = task.Result;

            return model3dFiles;
        }

    }

    public class MainAppProjectService : IMainAppProjectService
    {
        IDataService _dataService = null;
        Int32 _projectFileVersion = 0;

        public IDataService DataService { get => _dataService; } 

        public MainAppProjectService(IDataService dataService, Int32 projectFileVersion)
        {
            _dataService = dataService;
            _projectFileVersion = projectFileVersion;
        }


        public FiltersData GetModel3dFilters()
        {
            if (_dataService == null)
                return null;

            Model3dFiltersData model3DFiltersData = _dataService.GetModel3dFiltersData();
            FiltersData filtersData = Model3dFiltersDataConverter.ConvertToFiltersData(model3DFiltersData, _projectFileVersion, Model3dType.Ifc);
            return filtersData;
        }

        public TagsData GetModel3dTags()
        {
            if (_dataService == null)
                return null;

            Model3dTagsData model3DTagsData = _dataService.GetModel3dTagsData();
            TagsData tagsData = Model3dTagsDataConverter.ConvertToTagsData(model3DTagsData, _projectFileVersion);
            return tagsData;
        }
    }

}