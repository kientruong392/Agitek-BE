using AgitekBackend.Data;
using AgitekBackend.DTOs.Brands;
using AgitekBackend.Models;
using AgitekBackend.Common.Exceptions;
using AgitekBackend.Services.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

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

        public async Task<BrandDTO?> GetByIdAsync(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null) throw new NotFoundException($"Brand with id {id} not found.");
            return _mapper.Map<BrandDTO>(brand);
        }

        public async Task<BrandDTO> CreateAsync(CreateBrandDTO dto)
        {
            var brand = _mapper.Map<Brand>(dto);
            _context.Brands.Add(brand);
            await _context.SaveChangesAsync();
            return _mapper.Map<BrandDTO>(brand);
        }

        public async Task<BrandDTO?> UpdateAsync(int id, UpdateBrandDTO dto)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null) throw new NotFoundException($"Brand with id {id} not found.");

            _mapper.Map(dto, brand);
            await _context.SaveChangesAsync();
            return _mapper.Map<BrandDTO>(brand);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null) throw new NotFoundException($"Brand with id {id} not found.");

            _context.Brands.Remove(brand);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}




