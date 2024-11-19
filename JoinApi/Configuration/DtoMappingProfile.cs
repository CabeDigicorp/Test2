using AutoMapper;
using JoinApi.Models;
using ModelData.Dto;
using ModelData.Utilities;
//using ModelData.Model;

namespace JoinApi.Configuration
{
    public class DtoMappingProfile : Profile
    {
        public DtoMappingProfile()
        {
            AllowNullCollections = true;

            CreateMap<OperaDoc, OperaDto>();
            CreateMap<OperaCreateDto, OperaDoc>();
            CreateMap<OperaUpdateDto, OperaDoc>();

            CreateMap<ProgettoDoc, ProgettoDto>();
            CreateMap<ProgettoContentDto, ProgettoDoc>().ReverseMap();
            //CreateMap<ProgettoUpdateDto, ProgettoDoc>();

            #region Project content mapping
            CreateMap<ModelData.Model.DefinizioneAttributo, DefinizioneAttributoDoc>().ReverseMap();

            # region entityType mapping
            CreateMap<ModelData.Model.EntityType, EntityTypeDoc>().ReverseMap();
            CreateMap<ModelData.Model.DivisioneItemType, DivisioneItemTypeDoc>().ReverseMap();
            CreateMap<ModelData.Model.DivisioneItemParentType, DivisioneItemParentTypeDoc>().ReverseMap();
            CreateMap<EntityTypeDoc, ModelData.Model.AllegatiItemType>();
            CreateMap<EntityTypeDoc, ModelData.Model.CalendariItemType>();
            CreateMap<EntityTypeDoc, ModelData.Model.CapitoliItemType>();
            CreateMap<EntityTypeDoc, ModelData.Model.CapitoliItemParentType>();
            CreateMap<EntityTypeDoc, ModelData.Model.ComputoItemType>();
            CreateMap<EntityTypeDoc, ModelData.Model.ContattiItemType>();
            CreateMap<EntityTypeDoc, ModelData.Model.DocumentiItemType>();
            CreateMap<EntityTypeDoc, ModelData.Model.DocumentiItemParentType>();
            CreateMap<EntityTypeDoc, ModelData.Model.ElementiItemType>();
            CreateMap<EntityTypeDoc, ModelData.Model.ElencoAttivitaItemType>();
            CreateMap<EntityTypeDoc, ModelData.Model.InfoProgettoItemType>();
            CreateMap<EntityTypeDoc, ModelData.Model.PrezzarioItemType>();
            CreateMap<EntityTypeDoc, ModelData.Model.PrezzarioItemParentType>();
            CreateMap<EntityTypeDoc, ModelData.Model.ReportItemType>();
            CreateMap<EntityTypeDoc, ModelData.Model.StiliItemType>();
            CreateMap<EntityTypeDoc, ModelData.Model.TagItemType>();
            CreateMap<EntityTypeDoc, ModelData.Model.VariabiliItemType>();
            CreateMap<EntityTypeDoc, ModelData.Model.WBSItemType>();
            CreateMap<EntityTypeDoc, ModelData.Model.WBSItemParentType>();
            #endregion

            CreateMap<ModelData.Model.DivisioneItem, DivisioneItemDoc>().ReverseMap();
            CreateMap<ModelData.Model.ComputoItem, ComputoItemDoc>().ReverseMap();
            CreateMap<ModelData.Model.PrezzarioItem, PrezzarioItemDoc>().ReverseMap();
            CreateMap<ModelData.Model.ElementiItem, ElementiItemDoc>().ReverseMap();
            CreateMap<ModelData.Model.CapitoliItem, CapitoliItemDoc>().ReverseMap();
            CreateMap<ModelData.Model.VariabiliItem, VariabiliItemDoc>().ReverseMap();
            CreateMap<ModelData.Model.Model3dFilesInfo, Model3dFilesInfoDoc>().ReverseMap();
            //CreateMap<ModelData.Model.Model3dFileInfo, Model3dFileInfoDoc>().ReverseMap();
            CreateMap<ModelData.Model.AllegatiItem, AllegatiItemDoc>().ReverseMap();
            CreateMap<ModelData.Model.ContattiItem, ContattiItemDoc>().ReverseMap();
            CreateMap<ModelData.Model.InfoProgettoItem, InfoProgettoItemDoc>().ReverseMap();
            CreateMap<ModelData.Model.WBSItem, WBSItemDoc>().ReverseMap();
            CreateMap<ModelData.Model.CalendariItem, CalendariItemDoc>().ReverseMap();
            CreateMap<ModelData.Model.ViewSettings, ViewSettingsDoc>().ReverseMap();
            CreateMap<ModelData.Model.Model3dFiltersData, Model3dFiltersDataDoc>().ReverseMap();
            CreateMap<ModelData.Model.Model3dValuesData, Model3dValuesDataDoc>().ReverseMap();
            CreateMap<ModelData.Model.Model3dTagsData, Model3dTagsDataDoc>().ReverseMap();
            CreateMap<ModelData.Model.Model3dPreferencesData, Model3dPreferencesDataDoc>().ReverseMap();
            CreateMap<ModelData.Model.DocumentiItem, DocumentiItemDoc>().ReverseMap();
            CreateMap<ModelData.Model.ReportItem, ReportItemDoc>().ReverseMap();
            CreateMap<ModelData.Model.StiliItem, StiliItemDoc>().ReverseMap();
            CreateMap<ModelData.Model.NumericFormat, NumericFormatDoc>().ReverseMap();
            CreateMap<ModelData.Model.Model3dUserViewList, Model3dUserViewListDoc>().ReverseMap();
            CreateMap<ModelData.Model.ElencoAttivitaItem, ElencoAttivitaItemDoc>().ReverseMap();
            CreateMap<ModelData.Model.Model3dUserRotoTranslation, Model3dUserRotoTranslationDoc>().ReverseMap();
            CreateMap<ModelData.Model.GanttData, GanttDataDoc>().ReverseMap();
            CreateMap<ModelData.Model.WBSItemsCreationData, WBSItemsCreationDataDoc>().ReverseMap();
            CreateMap<ModelData.Model.FogliDiCalcoloData, FogliDiCalcoloDataDoc>().ReverseMap();
            CreateMap<ModelData.Model.TagItem, TagItemDoc>().ReverseMap();
            #endregion

            CreateMap<TagDoc, TagDto>();
            CreateMap<TagDto, TagDoc> ();

            CreateMap<GruppoUtentiDoc, GruppoUtentiDto>();
            CreateMap<GruppoUtentiCreateDto, GruppoUtentiDoc>();
            CreateMap<UtenteDoc, UtenteDto>();
            CreateMap<UtenteDoc, UtenteInfoWithClienteDto>();
            CreateMap<RuoloDoc, RuoloDto>();

            CreateMap<ClienteDoc, ClienteDto>();
            CreateMap<ClienteDto, ClienteDoc>();
            CreateMap<TeamDoc, TeamDto>();
            CreateMap<TeamCreateDto, TeamDoc>();
            CreateMap<TeamUpdateDto, TeamDoc>();

            CreateMap<SettoreDoc, SettoreDto>();
            CreateMap<SettoreCreateDto, SettoreDoc>();
            CreateMap<SettoreUpdateDto, SettoreDoc>();

            CreateMap<PermessoDoc, PermessoDto>().ForMember(dest => dest.OggettoTipo, opt => opt.MapFrom(src => Enum.Parse<TipiOggettoPermessi>(src.OggettoTipo)));
            CreateMap<PermessoDto, PermessoDoc>().ForMember(dest => dest.OggettoTipo, opt => opt.MapFrom(src => Enum.GetName<TipiOggettoPermessi>(src.OggettoTipo)));

        }
    }
}
