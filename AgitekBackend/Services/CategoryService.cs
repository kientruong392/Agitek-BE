using AgitekBackend.Data;
using AgitekBackend.DTOs.Categories;
using AgitekBackend.Models;
using AgitekBackend.Common.Exceptions;
using AgitekBackend.Services.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using AgitekBackend.Common.Utils;

namespace AgitekBackend.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public CategoryService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CategoryDTO>> GetAllAsync()
        {
            var categories = await _context.Categories.ToListAsync();
            return _mapper.Map<IEnumerable<CategoryDTO>>(categories);
        }
        public async Task<IEnumerable<CategoryDTO>> GetAllWithSubCategoriesAsync()
        {
            var categories = await _context.Categories
                .Where(c => c.ParentCategoryId == null)
                .Include(c => c.SubCategories)
                .AsNoTracking()
                .ToListAsync();
            return _mapper.Map<IEnumerable<CategoryDTO>>(categories);
        }

        public async Task<IEnumerable<CategoryDTO>> GetAllSubCategoriesAsync(int parentCategoryId)
        {
            var subCategories = await _context.Categories
                .Where(c => c.ParentCategoryId == parentCategoryId)
                .AsNoTracking()
                .ToListAsync();
            return _mapper.Map<IEnumerable<CategoryDTO>>(subCategories);
        }

        public async Task<CategoryDTO> GetByIdAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) throw new NotFoundException($"Category not found");
            return _mapper.Map<CategoryDTO>(category);
        }

        public async Task<CategoryDTO> GetByIdWithProducts(int id)
        {
            var category = await _context.Categories.Include(c => c.Products).FirstOrDefaultAsync(c => c.Id == id);
            if (category == null) throw new NotFoundException($"Category not found");
            return _mapper.Map<CategoryDTO>(category);
        }

        public async Task<CategoryDTO> GetBySlugWithProductsAsync(string slug)
        {
            var category = await _context.Categories.Include(c => c.Products).FirstOrDefaultAsync(c => c.Slug == slug);
            if (category == null) throw new NotFoundException($"Category not found");
            return _mapper.Map<CategoryDTO>(category);
        }
        public async Task<CategoryDTO> CreateAsync(CreateCategoryDTO dto)
        {
            var category = _mapper.Map<Category>(dto);
            category.Slug = Helpers.GenerateSlug(category.CategoryName);
            if(await IsCategoryExists(category.Slug))
            {
                throw new BadRequestException($"This category already exists");
            }
            if(category.ParentCategoryId.HasValue)
            {
                var parentCategory = await _context.Categories.FindAsync(category.ParentCategoryId.Value);
                if (parentCategory == null)
                {
                    throw new NotFoundException($"Parent category not found");
                }
            }
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return _mapper.Map<CategoryDTO>(category);
        }

        public async Task<CategoryDTO> UpdateAsync(int id, UpdateCategoryDTO dto)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) throw new NotFoundException($"Category not found");
            _mapper.Map(dto, category);
            category.Slug = Helpers.GenerateSlug(category.CategoryName);
            if (await IsCategoryExists(category.Slug))
            {
                throw new BadRequestException($"This category already exists");
            }
            if(category.ParentCategoryId.HasValue)
            {
                var parentCategory = await _context.Categories.FindAsync(category.ParentCategoryId.Value);
                if (parentCategory == null)
                {
                    throw new NotFoundException($"Parent category not found");
                }
            }
            await _context.SaveChangesAsync();
            return _mapper.Map<CategoryDTO>(category);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) throw new NotFoundException($"Category not found");
            var hasProducts = await _context.Products.AnyAsync(p => p.CategoryId == id);
            if (hasProducts)
            {
                throw new BadRequestException($"Cannot delete this category because it has associated products");
            }
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsCategoryExists(string slug)
        {
            return await _context.Categories.AnyAsync(c => c.Slug == slug);
        }
    }
}




