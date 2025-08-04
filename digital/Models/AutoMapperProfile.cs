using AutoMapper;
using digital.Models;
using digital.ViewModels;
using AutoMapper;
using digital.Models;
using digital.ViewModels;
using AutoMapper;
using digital.Models;
using digital.ViewModels;

namespace digital.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {

            CreateMap<User, UserViewModel>().ReverseMap();


            CreateMap<Category, CategoryViewModel>().ReverseMap();


            CreateMap<SubCategory, SubCategoryViewModel>().ReverseMap();


            CreateMap<Student, StudentViewModel>().ReverseMap();


            CreateMap<TimeTable, TimeTableViewModel>().ReverseMap();


            CreateMap<Attendance, AttendanceViewModel>().ReverseMap();


            CreateMap<TeacherMaster, TeacherMasterViewModel>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
                .ForMember(dest => dest.SubCategoryName, opt => opt.MapFrom(src => src.SubCategory.Name))
                .ForMember(dest => dest.SubjectName, opt => opt.MapFrom(src => src.Subject.Name))
                .ForMember(dest => dest.TeacherName, opt => opt.MapFrom(src => src.Teacher.Name))
                .ReverseMap();
        }
    }
}
