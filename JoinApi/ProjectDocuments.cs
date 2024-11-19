using AutoMapper;
using JoinApi.Utilities;
using ModelData.Model;
using MongoDB.Bson;
using ZstdSharp.Unsafe;

namespace JoinApi.Models
{
    public class ProjectDocuments
    {
        IMapper _mapper;

        public List<DefinizioneAttributoDoc>? DefinizioniAttributo { get; set; }
        public List<EntityTypeDoc>? EntityTypes { get; set; }
        public List<CapitoliItemDoc>? CapitoliItems { get; set; }
        public List<ElementiItemDoc>? ElementiItems { get; set; }
        public List<PrezzarioItemDoc>? PrezzarioItems { get; set; }
        public List<ComputoItemDoc>? ComputoItems { get; set; }
        public List<DivisioneItemDoc>? DivisioniItems { get; set; }
        public VariabiliItemDoc? VariabiliItem { get; set; }
        public Model3dFilesInfoDoc? Model3dFilesInfo { get; set; }
        public List<AllegatiItemDoc>? AllegatiItems { get; set; }
        public List<ContattiItemDoc>? ContattiItems { get; set; }
        public InfoProgettoItemDoc? InfoProgettoItem { get; set; }
        public List<WBSItemDoc>? WBSItems { get; set; }
        public List<CalendariItemDoc>? CalendariItems { get; set; }
        public ViewSettingsDoc? ViewSettings { get; set; }
        public Model3dFiltersDataDoc? Model3dFiltersData { get; set; }
        public Model3dValuesDataDoc? Model3dValuesData { get; set; }
        public Model3dTagsDataDoc? Model3dTagsData { get; set; }
        public Model3dPreferencesDataDoc? Model3dPreferencesData { get; set; }
        public List<DocumentiItemDoc>? DocumentiItems { get; set; }
        public List<ReportItemDoc>? ReportItems { get; set; }
        public List<StiliItemDoc>? StiliItems { get; set; }
        public List<NumericFormatDoc>? NumericFormats { get; set; }
        public Model3dUserViewListDoc? Model3dUserViewList { get; set; }
        public List<ElencoAttivitaItemDoc>? ElencoAttivitaItems { get; set; }
        public Model3dUserRotoTranslationDoc? Model3dUserRotoTranslation { get; set; }
        public GanttDataDoc? GanttData { get; set; }
        public WBSItemsCreationDataDoc? WBSItemsCreationData { get; set; }
        public FogliDiCalcoloDataDoc? FogliDiCalcoloData { get; set; }
        public List<TagItemDoc>? TagItems { get; set; }


        public ProjectDocuments(IMapper mapper)
        {
            _mapper = mapper;
        }

        internal void FromProject(Project prj)
        {

            try
            {
                ComputoItems = prj.ComputoItems.Select(item => _mapper.Map<ComputoItemDoc>(item)).ToList();

                PrezzarioItems = prj.PrezzarioItems.Select(item => _mapper.Map<PrezzarioItemDoc>(item)).ToList();

                ElementiItems = prj.ElementiItems.Select(item => _mapper.Map<ElementiItemDoc>(item)).ToList();

                CapitoliItems = prj.CapitoliItems.Select(item => _mapper.Map<CapitoliItemDoc>(item)).ToList();

                DefinizioniAttributo = prj.DefinizioniAttributo.Values.Select(item => _mapper.Map<DefinizioneAttributoDoc>(item)).ToList();

                EntityTypes = prj.EntityTypes.Values.Select(item =>
                {
                    EntityTypeDoc doc = null;
                    if (item is DivisioneItemType)
                        doc = _mapper.Map<DivisioneItemTypeDoc>(item);
                    else if (item is DivisioneItemParentType)
                        doc = _mapper.Map<DivisioneItemParentTypeDoc>(item);
                    else
                        doc = _mapper.Map<EntityTypeDoc>(item);
                    return doc;
                }).ToList();

                DivisioniItems = prj.DivisioniItems.SelectMany(item =>
                {
                    List<DivisioneItemDoc> docs = new List<DivisioneItemDoc>();
                    if (item.Value != null)
                    {
                        foreach (DivisioneItem divItem in item.Value)
                        {
                            DivisioneItemDoc doc = _mapper.Map<DivisioneItemDoc>(divItem);
                            doc.DivisioneId = item.Key;
                            docs.Add(doc);
                        }
                    }
                    
                    return docs;
                    
                }).ToList();

                VariabiliItem = _mapper.Map<VariabiliItemDoc>(prj.VariabiliItem);
                Model3dFilesInfo = _mapper.Map<Model3dFilesInfoDoc>(prj.Model3dFilesInfo);
                AllegatiItems = prj.AllegatiItems.Select(item => _mapper.Map<AllegatiItemDoc>(item)).ToList();
                ContattiItems = prj.ContattiItems.Select(item => _mapper.Map<ContattiItemDoc>(item)).ToList();
                InfoProgettoItem = _mapper.Map<InfoProgettoItemDoc>(prj.InfoProgettoItem);
                WBSItems = prj.WBSItems.Select(item => _mapper.Map<WBSItemDoc>(item)).ToList();
                CalendariItems = prj.CalendariItems.Select(item => _mapper.Map<CalendariItemDoc>(item)).ToList();
                ViewSettings = _mapper.Map<ViewSettingsDoc>(prj.ViewSettings);
                Model3dFiltersData = _mapper.Map<Model3dFiltersDataDoc>(prj.Model3dFiltersData);
                Model3dValuesData = _mapper.Map<Model3dValuesDataDoc>(prj.Model3dValuesData);
                Model3dTagsData = _mapper.Map<Model3dTagsDataDoc>(prj.Model3dTagsData);
                Model3dPreferencesData = _mapper.Map<Model3dPreferencesDataDoc>(prj.Model3dPreferencesData);
                DocumentiItems = prj.DocumentiItems.Select(item => _mapper.Map<DocumentiItemDoc>(item)).ToList();
                ReportItems = prj.ReportItems.Select(item => _mapper.Map<ReportItemDoc>(item)).ToList();
                StiliItems = prj.StiliItems.Select(item => _mapper.Map<StiliItemDoc>(item)).ToList();
                NumericFormats = prj.NumericFormats.Select(item => _mapper.Map<NumericFormatDoc>(item)).ToList();
                Model3dUserViewList = _mapper.Map<Model3dUserViewListDoc>(prj.Model3dUserViews);
                ElencoAttivitaItems = prj.ElencoAttivitaItems.Select(item => _mapper.Map<ElencoAttivitaItemDoc>(item)).ToList();
                Model3dUserRotoTranslation = _mapper.Map<Model3dUserRotoTranslationDoc>(prj.Model3dUserRotoTranslation);
                GanttData = _mapper.Map<GanttDataDoc>(prj.Gantt);
                WBSItemsCreationData = _mapper.Map<WBSItemsCreationDataDoc>(prj.WBSItemsCreationData);
                FogliDiCalcoloData = _mapper.Map<FogliDiCalcoloDataDoc>(prj.FogliDiCalcolo);
                TagItems = prj.TagItems.Select(item => _mapper.Map<TagItemDoc>(item)).ToList();


            }
            catch (Exception exc)
            {

            }
        }

        internal Project ToProject()
        {
            Project prj = new Project();

            try
            {
                prj.ComputoItems = ComputoItems.OrderBy(item => item.Position.Result).Select(item => _mapper.Map<ComputoItem>(item)).ToList();

                prj.PrezzarioItems = PrezzarioItems.OrderBy(item => item.Position.Result).Select(item => _mapper.Map<PrezzarioItem>(item)).ToList();

                prj.ElementiItems = ElementiItems.OrderBy(item => item.Position.Result).Select(item => _mapper.Map<ElementiItem>(item)).ToList();

                prj.CapitoliItems = CapitoliItems.OrderBy(item => item.Position.Result).Select(item => _mapper.Map<CapitoliItem>(item)).ToList();

                prj.DefinizioniAttributo = DefinizioniAttributo.ToDictionary(item => item.Codice, item => _mapper.Map<DefinizioneAttributo>(item));

                prj.EntityTypes = EntityTypes.ToDictionary(item =>
                {
                    if (item is DivisioneItemTypeDoc)
                    {
                        DivisioneItemTypeDoc divItem = (DivisioneItemTypeDoc)item;
                        return EntityHelper.CreateEntityTypeKey(BuiltInCodes.EntityType.Divisione, divItem.DivisioneId);
                    }
                    else if (item is DivisioneItemParentTypeDoc)
                    {
                        DivisioneItemParentTypeDoc divParentItem = (DivisioneItemParentTypeDoc)item;
                        return EntityHelper.CreateEntityTypeKey(BuiltInCodes.EntityType.DivisioneParent, divParentItem.DivisioneId);
                    }
                    else
                        return item.Codice;

                },
                item =>
                {
                    if (item is DivisioneItemTypeDoc)
                        return _mapper.Map<DivisioneItemType>(item);
                    else if (item is DivisioneItemParentTypeDoc)
                        return _mapper.Map<DivisioneItemParentType>(item);
                    else
                    {
                        if (item.Codice == BuiltInCodes.EntityType.Allegati)
                            return _mapper.Map<AllegatiItemType>(item);
                        else if (item.Codice == BuiltInCodes.EntityType.Calendari)
                            return _mapper.Map<CalendariItemType>(item);
                        else if (item.Codice == BuiltInCodes.EntityType.Capitoli)
                            return _mapper.Map<CapitoliItemType>(item);
                        else if (item.Codice == BuiltInCodes.EntityType.CapitoliParent)
                            return _mapper.Map<CapitoliItemParentType>(item);
                        else if (item.Codice == BuiltInCodes.EntityType.Computo)
                            return _mapper.Map<ComputoItemType>(item);
                        else if (item.Codice == BuiltInCodes.EntityType.Contatti)
                            return _mapper.Map<ContattiItemType>(item);
                        else if (item.Codice == BuiltInCodes.EntityType.Documenti)
                            return _mapper.Map<DocumentiItemType>(item);
                        else if (item.Codice == BuiltInCodes.EntityType.DocumentiParent)
                            return _mapper.Map<DocumentiItemParentType>(item);
                        else if (item.Codice == BuiltInCodes.EntityType.Elementi)
                            return _mapper.Map<ElementiItemType>(item);
                        else if (item.Codice == BuiltInCodes.EntityType.ElencoAttivita)
                            return _mapper.Map<ElencoAttivitaItemType>(item);
                        else if (item.Codice == BuiltInCodes.EntityType.InfoProgetto)
                            return _mapper.Map<InfoProgettoItemType>(item);
                        else if (item.Codice == BuiltInCodes.EntityType.Prezzario)
                            return _mapper.Map<PrezzarioItemType>(item);
                        else if (item.Codice == BuiltInCodes.EntityType.PrezzarioParent)
                            return _mapper.Map<PrezzarioItemParentType>(item);
                        else if (item.Codice == BuiltInCodes.EntityType.Report)
                            return _mapper.Map<ReportItemType>(item);
                        else if (item.Codice == BuiltInCodes.EntityType.Stili)
                            return _mapper.Map<StiliItemType>(item);
                        else if (item.Codice == BuiltInCodes.EntityType.Tag)
                            return _mapper.Map<TagItemType>(item);
                        else if (item.Codice == BuiltInCodes.EntityType.Variabili)
                            return _mapper.Map<VariabiliItemType>(item);
                        else if (item.Codice == BuiltInCodes.EntityType.WBS)
                            return _mapper.Map<WBSItemType>(item);
                        else if (item.Codice == BuiltInCodes.EntityType.WBSParent)
                            return _mapper.Map<WBSItemParentType>(item);
                        
                        return _mapper.Map<EntityType>(item);
                    }
                    
                });
                

                prj.DivisioniItems = DivisioniItems.GroupBy(item => item.DivisioneId).ToDictionary(item => item.Key, item => item.OrderBy(item => item.Position.Result).Select(x => _mapper.Map<DivisioneItem>(x)).ToList());

                prj.VariabiliItem = _mapper.Map<VariabiliItem>(VariabiliItem);

                prj.Model3dFilesInfo = _mapper.Map<Model3dFilesInfo>(Model3dFilesInfo);

                prj.AllegatiItems = AllegatiItems.Select(item => _mapper.Map<AllegatiItem>(item)).ToList();

                prj.ContattiItems = ContattiItems.Select(item => _mapper.Map<ContattiItem>(item)).ToList();

                prj.InfoProgettoItem = _mapper.Map<InfoProgettoItem>(InfoProgettoItem);

                prj.WBSItems = WBSItems.OrderBy(item => item.Position.Result).Select(item => _mapper.Map<WBSItem>(item)).ToList();

                prj.CalendariItems = CalendariItems.Select(item => _mapper.Map<CalendariItem>(item)).ToList();

                prj.ViewSettings = _mapper.Map<ViewSettings>(ViewSettings);

                prj.Model3dFiltersData = _mapper.Map<Model3dFiltersData>(Model3dFiltersData);

                prj.Model3dValuesData = _mapper.Map<Model3dValuesData>(Model3dValuesData);

                prj.Model3dTagsData = _mapper.Map<Model3dTagsData>(Model3dTagsData);

                prj.Model3dPreferencesData = _mapper.Map<Model3dPreferencesData>(Model3dPreferencesData);

                prj.DocumentiItems = DocumentiItems.OrderBy(item => item.Position.Result).Select(item => _mapper.Map<DocumentiItem>(item)).ToList();

                prj.ReportItems = ReportItems.Select(item => _mapper.Map<ReportItem>(item)).ToList();

                prj.StiliItems = StiliItems.Select(item => _mapper.Map<StiliItem>(item)).ToList();

                prj.NumericFormats = NumericFormats.Select(item => _mapper.Map<NumericFormat>(item)).ToList();

                prj.Model3dUserViews = _mapper.Map<Model3dUserViewList>(Model3dUserViewList);

                prj.ElencoAttivitaItems = ElencoAttivitaItems.Select(item => _mapper.Map<ElencoAttivitaItem>(item)).ToList();

                prj.Model3dUserRotoTranslation = _mapper.Map<Model3dUserRotoTranslation>(Model3dUserRotoTranslation);

                prj.Gantt = _mapper.Map<GanttData>(GanttData);

                prj.WBSItemsCreationData = _mapper.Map<WBSItemsCreationData>(WBSItemsCreationData);

                prj.FogliDiCalcolo = _mapper.Map<FogliDiCalcoloData>(FogliDiCalcoloData);

                prj.TagItems = TagItems.Select(item => _mapper.Map<TagItem>(item)).ToList();

            }
            catch (Exception exc)
            {
                
            }
            
            return prj;
        }

        public long GetSize()
        {
            long totalSizeInBytes = 0;


            DefinizioniAttributo?.ForEach(item => totalSizeInBytes += item.ToBson().Length);
            EntityTypes?.ForEach(item => totalSizeInBytes += item.ToBson().Length);
            CapitoliItems?.ForEach(item => totalSizeInBytes += item.ToBson().Length);
            ElementiItems?.ForEach(item => totalSizeInBytes += item.ToBson().Length);
            PrezzarioItems?.ForEach(item => totalSizeInBytes += item.ToBson().Length);
            ComputoItems?.ForEach(item => totalSizeInBytes += item.ToBson().Length);
            DivisioniItems?.ForEach(item => totalSizeInBytes += item.ToBson().Length);
            totalSizeInBytes += (VariabiliItem != null)? VariabiliItem.ToBson().Length : 0;
            totalSizeInBytes += (Model3dFilesInfo != null)? Model3dFilesInfo.ToBson().Length : 0;
            AllegatiItems?.ForEach(item => totalSizeInBytes += item.ToBson().Length);
            ContattiItems?.ForEach(item => totalSizeInBytes += item.ToBson().Length);
            totalSizeInBytes += (InfoProgettoItem != null)? InfoProgettoItem.ToBson().Length : 0;
            WBSItems?.ForEach(item => totalSizeInBytes += item.ToBson().Length);
            CalendariItems?.ForEach(item => totalSizeInBytes += item.ToBson().Length);
            totalSizeInBytes += (ViewSettings != null)? ViewSettings.ToBson().Length : 0;
            totalSizeInBytes += (Model3dFiltersData != null)? Model3dFiltersData.ToBson().Length : 0;
            totalSizeInBytes += (Model3dValuesData != null)? Model3dValuesData.ToBson().Length : 0;
            totalSizeInBytes += (Model3dTagsData != null)? Model3dTagsData.ToBson().Length : 0;
            totalSizeInBytes += (Model3dPreferencesData!= null) ?Model3dPreferencesData.ToBson().Length : 0;
            DocumentiItems?.ForEach(item => totalSizeInBytes += item.ToBson().Length);
            ReportItems?.ForEach(item => totalSizeInBytes += item.ToBson().Length);
            StiliItems?.ForEach(item => totalSizeInBytes += item.ToBson().Length);
            NumericFormats?.ForEach(item => totalSizeInBytes += item.ToBson().Length);
            totalSizeInBytes += (Model3dUserViewList != null)? Model3dUserViewList.ToBson().Length : 0;
            ElencoAttivitaItems?.ForEach(item => totalSizeInBytes += item.ToBson().Length);
            totalSizeInBytes += (Model3dUserRotoTranslation != null) ? Model3dUserRotoTranslation.ToBson().Length : 0;
            totalSizeInBytes += (GanttData != null) ? GanttData.ToBson().Length : 0;
            totalSizeInBytes += (WBSItemsCreationData != null) ? WBSItemsCreationData.ToBson().Length : 0;
            totalSizeInBytes += (FogliDiCalcoloData != null) ? FogliDiCalcoloData.ToBson().Length : 0;
            TagItems?.ForEach(item => totalSizeInBytes += item.ToBson().Length);

            return totalSizeInBytes;

        }


    }
}
