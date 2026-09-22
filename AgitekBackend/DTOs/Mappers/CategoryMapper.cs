using AgitekBackend.DTOs.Categories;
using AgitekBackend.Models;
using AutoMapper;

namespace AgitekBackend.DTOs.Mappers
{
    public class CategoryMapper : AutoMapper.Profile
    {
        public CategoryMapper()
        {
            CreateMap<CreateCategoryDTO, Category>();
            CreateMap<UpdateCategoryDTO, Category>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Category, CategoryDTO>();
        }
    }
}



