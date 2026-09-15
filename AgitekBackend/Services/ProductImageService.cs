using AgitekBackend.Data;
using AgitekBackend.DTOs.ProductImages;
using AgitekBackend.Models;
using AgitekBackend.Services.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AgitekBackend.Services
{
    public class ProductImageService : IProductImageService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ProductImageService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProductImageDTO>> GetAllAsync()
        {
            var images = await _context.ProductImages.ToListAsync();
            return _mapper.Map<IEnumerable<ProductImageDTO>>(images);
        }

        public async Task<ProductImageDTO?> GetByIdAsync(int id)
        {
            var image = await _context.ProductImages.FindAsync(id);
            return image == null ? null : _mapper.Map<ProductImageDTO>(image);
        }

        public async Task<ProductImageDTO> CreateAsync(CreateProductImageDTO dto)
        {
            var image = _mapper.Map<ProductImage>(dto);
            _context.ProductImages.Add(image);
            await _context.SaveChangesAsync();
            return _mapper.Map<ProductImageDTO>(image);
        }

        public async Task<ProductImageDTO?> UpdateAsync(int id, UpdateProductImageDTO dto)
        {
            var image = await _context.ProductImages.FindAsync(id);
            if (image == null) return null;

            _mapper.Map(dto, image);
            await _context.SaveChangesAsync();
            return _mapper.Map<ProductImageDTO>(image);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var image = await _context.ProductImages.FindAsync(id);
            if (image == null) return false;

            _context.ProductImages.Remove(image);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}


