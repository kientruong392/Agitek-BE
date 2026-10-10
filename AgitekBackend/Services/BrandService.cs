using AgitekBackend.Data;
using AgitekBackend.DTOs.Brands;
using AgitekBackend.Models;
using AgitekBackend.Common.Exceptions;
using AgitekBackend.Services.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using AgitekBackend.Common.Utils;

namespace AgitekBackend.Services
{
    public class BrandService : IBrandService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public BrandService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<BrandDTO>> GetAllAsync()
        {
            var brands = await _context.Brands.ToListAsync();
            return _mapper.Map<IEnumerable<BrandDTO>>(brands);
        }

        public async Task<BrandDTO> GetByIdAsync(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null) throw new NotFoundException($"Brand not found");
            return _mapper.Map<BrandDTO>(brand);
        }
        
        public async Task<BrandDTO> GetByIdWithProductsAsync(int id)
        {
            var brand = await _context.Brands.Include(b => b.Products).FirstOrDefaultAsync(b => b.Id == id);
            if (brand == null) throw new NotFoundException($"Brand not found");
            return _mapper.Map<BrandDTO>(brand);
        }

        public async Task<BrandDTO> GetBySlugWithProductsAsync(string slug)
        {
            var brand = await _context.Brands.Include(b => b.Products).FirstOrDefaultAsync(b => b.Slug == slug);
            if (brand == null) throw new NotFoundException($"Brand not found");
            return _mapper.Map<BrandDTO>(brand);
        }

        public async Task<BrandDTO> CreateAsync(CreateBrandDTO dto)
        {
            var brand = _mapper.Map<Brand>(dto);
            brand.Slug = Helpers.GenerateSlug(brand.Name);
            if(await IsBrandExists(brand.Slug))
            {
                throw new BadRequestException($"Brand with slug '{brand.Slug}' already exists");
            }
            _context.Brands.Add(brand);
            await _context.SaveChangesAsync();
            return _mapper.Map<BrandDTO>(brand);
        }

        public async Task<BrandDTO> UpdateAsync(int id, UpdateBrandDTO dto)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null) throw new NotFoundException($"Brand not found");

            _mapper.Map(dto, brand);
            brand.Slug = Helpers.GenerateSlug(brand.Name);
            if (await IsBrandExists(brand.Slug, id))
            {
                throw new BadRequestException($"Brand with slug '{brand.Slug}' already exists", "BRAND_ALREADY_EXISTS");
            }
            await _context.SaveChangesAsync();
            return _mapper.Map<BrandDTO>(brand);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null) throw new NotFoundException($"Brand not found");
            var hasProducts = await _context.Products.AnyAsync(p => p.BrandId == id);
            if (hasProducts)
            {
                throw new BadRequestException($"Cannot delete this brand because it has associated products", "BRAND_HAS_PRODUCTS");
            }
            _context.Brands.Remove(brand);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> IsBrandExists(string slug, int? excludeId = null)
        {
            return await _context.Brands.AnyAsync(b => b.Slug == slug && (!excludeId.HasValue || b.Id != excludeId.Value));
        }
    }
}