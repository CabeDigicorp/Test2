using AutoMapper;
using ModelData.Dto;
using JoinWebUI.Models;

namespace Configuration
{
    public class ViewModelMappingProfile : Profile
    {
        public ViewModelMappingProfile()
        {
            CreateMap<TagModel, TagRenameFormModel>().ReverseMap();
            CreateMap<TagCreateFormModel, TagModel>();
        }

    }
}
