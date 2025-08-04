using AutoMapper;
using digital.Models;
using digital.ViewModels;

namespace digital.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Category, CategoryViewModel>().ReverseMap();
        }
    }
}
