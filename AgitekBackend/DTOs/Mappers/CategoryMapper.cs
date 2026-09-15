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
            CreateMap<UpdateCategoryDTO, Category>();
            CreateMap<Category, CategoryDTO>();
        }
    }
}



