using AutoMapper;
using ModelData.Dto;
using JoinWebUI.Models;

namespace Configuration
{
    public class DtoMappingProfile : Profile
    {

        

        public DtoMappingProfile()
        {
            var configuration = new MapperConfiguration(cfg => cfg.ShouldUseConstructor = constructor => constructor.IsPublic);

            //CreateMap<PasswordForgottenFormModel, PasswordForgottenDto>();
            //CreateMap<PasswordResetFormModel, PasswordResetDto>();
            //CreateMap<PasswordChangeFormModel, PasswordChangeDto>();

            CreateMap<OperaModel, OperaCreateDto>();
            CreateMap<OperaCreateDto, OperaModel>();
            CreateMap<OperaModel, OperaDto>();
            CreateMap<OperaDto, OperaModel>().BeforeMap<OperaModel>();
            CreateMap<OperaModel, OperaUpdateDto>();
            CreateMap<OperaUpdateDto, OperaModel>();
            
            CreateMap<ProgettoModel, ProgettoContentDto>();
            CreateMap<ProgettoModel, ProgettoDto>();
            //CreateMap<ProgettoModel, ProgettoUpdateDto>();
            CreateMap<ProgettoDto, ProgettoModel>().BeforeMap<ProgettoModel>();

            //CreateMap<RegisterBeginFormModel, RegisterBeginDto>();
            //CreateMap<RegisterFormModel, RegisterDto>();

            CreateMap<TagDto, TagModel>();
            CreateMap<TagModel, TagDto>();
            CreateMap<AllegatoDto, AllegatoModel>();

            CreateMap<GruppoUtentiDto, GruppoUtentiModel>().BeforeMap<GruppoUtentiModel>();
            CreateMap<GruppoUtentiModel, GruppoUtentiUpdateDto>();
            CreateMap<GruppoUtentiModel, GruppoUtentiCreateDto>();

			CreateMap<ClienteDto, ClienteModel>().BeforeMap<ClienteModel>();
            CreateMap<ClienteModel, ClienteDto>();
            CreateMap<UtenteDto, UtenteModel>().BeforeMap<UtenteModel>();
			CreateMap<UtenteModel, UtenteDto>();
            CreateMap<RuoloDto, RuoloModel>();

            CreateMap<TeamCreateDto, TeamModel>();
            CreateMap<TeamUpdateDto, TeamModel>();
            CreateMap<TeamDto, TeamModel>().BeforeMap<TeamModel>();
            CreateMap<TeamModel, TeamCreateDto>();
            CreateMap<TeamModel, TeamUpdateDto>();
            CreateMap<TeamModel, TeamDto>();

            CreateMap<UtenteInfoWithClienteDto, UtenteModel>().BeforeMap<UtenteModel>();

            CreateMap<SettoreModel, SettoreDto>();
            CreateMap<SettoreModel, SettoreCreateDto>();
            CreateMap<SettoreModel, SettoreUpdateDto>();
            CreateMap<SettoreDto, SettoreModel>().BeforeMap<SettoreModel>();

            CreateMap<PermessoModel, PermessoDto>();
            CreateMap<PermessoDto, PermessoModel>();



        }

    }
}
