using AgitekBackend.Data;
using AgitekBackend.DTOs.Products;
using AgitekBackend.Models;
using AgitekBackend.Common.Exceptions;
using AgitekBackend.Services.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AgitekBackend.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ProductService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProductDTO>> GetAllAsync()
        {
            var products = await _context.Products.ToListAsync();
            return _mapper.Map<IEnumerable<ProductDTO>>(products);
        }

        public async Task<ProductDTO?> GetByIdAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) throw new NotFoundException($"Product with id {id} not found");
            return _mapper.Map<ProductDTO>(product);
        }

        public async Task<ProductDTO> CreateAsync(CreateProductDTO dto)
        {
            var product = _mapper.Map<Product>(dto);
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return _mapper.Map<ProductDTO>(product);
        }

        public async Task<ProductDTO?> UpdateAsync(int id, UpdateProductDTO dto)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) throw new NotFoundException($"Product with id {id} not found");

            _mapper.Map(dto, product);
            product.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return _mapper.Map<ProductDTO>(product);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) throw new NotFoundException($"Product with id {id} not found");

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}




