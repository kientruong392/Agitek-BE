using AgitekBackend.Data;
using AgitekBackend.DTOs.Products;
using AgitekBackend.Models;
using AgitekBackend.Common.Exceptions;
using AgitekBackend.Services.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using AgitekBackend.Common.Utils;
using AgitekBackend.Common;

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

        private IQueryable<Product> ApplyVisibilityFilter(IQueryable<Product> query, CurrentUser? currentUser)
        {
            var userRole = currentUser?.Role;
            if (userRole == RoleName.SuperAdmin || userRole == RoleName.Admin || userRole == RoleName.Manager) 
                return query;
            else if (userRole == RoleName.Staff)
                return query.Where(p => p.Status != ProductStatus.Deleted);
            return query.Where(p => p.Status != ProductStatus.Pending
                && p.Status != ProductStatus.Deleted
                && p.Status != ProductStatus.Hidden);
        }

        private async Task<IEnumerable<ProductDTO>> GetVisibleProductsAsync(IQueryable<Product> query, CurrentUser? currentUser)
        {
            var products = await query.ToListAsync();
            return _mapper.Map<IEnumerable<ProductDTO>>(products);
        }

        public async Task<IEnumerable<ProductDTO>> GetAllAsync(CurrentUser? currentUser)
        {
            var query = _context.Products
                .Include(p => p.Images)
                .AsQueryable();

            query = ApplyVisibilityFilter(query, currentUser);

            return await GetVisibleProductsAsync(query, currentUser);
        }

        public async Task<IEnumerable<ProductDTO>> GetFeaturedAsync(int count, CurrentUser? currentUser)
        {
            if (count <= 0) return new List<ProductDTO>();

            var query = _context.Products
                .Include(p => p.Images)
                .AsQueryable();

            query = ApplyVisibilityFilter(query, currentUser);

            var products = await query
                .OrderByDescending(p => p.SoldQuantity)
                .ThenByDescending(p => p.AverageRating)
                .Take(count)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ProductDTO>>(products);
        }

        public async Task<IEnumerable<ProductDTO>> GetNewestAsync(int count, CurrentUser? currentUser)
        {
            if (count <= 0) return new List<ProductDTO>();

            var query = _context.Products
                .Include(p => p.Images)
                .AsQueryable();

            query = ApplyVisibilityFilter(query, currentUser);

            var products = await query
                .OrderByDescending(p => p.CreatedAt)
                .ThenByDescending(p => p.Id)
                .Take(count)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ProductDTO>>(products);
        }

        public async Task<IEnumerable<ProductDTO>> GetByCategoryAsync(int categoryId, CurrentUser? currentUser)
        {
            var query = _context.Products
                .Include(p => p.Images)
                .Where(p => p.CategoryId == categoryId)
                .AsQueryable();

            query = ApplyVisibilityFilter(query, currentUser);

            return await GetVisibleProductsAsync(query, currentUser);
        }

        public async Task<ProductDTO> GetByIdAsync(int id)
        {
            var product = await _context.Products
                .Include(p => p.Images)
                .Include(p => p.Specifications)
                .Include(p => p.Reviews)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) throw new NotFoundException("Product not found");
            return _mapper.Map<ProductDTO>(product);
        }

        public async Task<ProductDTO> GetBySlugAsync(string slug)
        {
            var product = await _context.Products
                .Include(p => p.Images)
                .Include(p => p.Specifications)
                .FirstOrDefaultAsync(p => p.Slug == slug
                    && p.Status != ProductStatus.Pending
                    && p.Status != ProductStatus.Deleted
                    && p.Status != ProductStatus.Hidden);
            if (product == null) throw new NotFoundException("Product not found");
            return _mapper.Map<ProductDTO>(product);
        }

        public async Task<ProductDTO> CreateAsync(CreateProductDTO dto, int currentUserId)
        {
            if (dto.Images == null || !dto.Images.Any())
                throw new BadRequestException("Product must have at least one image");
            if (dto.Images.Count > 5)
                throw new BadRequestException("Product cannot have more than 5 images", "PRODUCT_MAX_IMAGES_EXCEEDED");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var brand = await _context.Brands.FindAsync(dto.BrandId);
                if (brand == null) throw new NotFoundException("Brand not found");

                var category = await _context.Categories.FindAsync(dto.CategoryId);
                if (category == null) throw new NotFoundException("Category not found");

                var generatedSku = $"{brand.Code}-{category.Code}-{dto.ProductCode}";
                
                if (await _context.Products.AnyAsync(p => p.SKU == generatedSku))
                    throw new BadRequestException("Product with this SKU already exists", "PRODUCT_SKU_EXISTS");

                var product = _mapper.Map<Product>(dto);
                product.SKU = generatedSku;
                product.UserId = currentUserId;
                product.Slug = Helpers.GenerateSlug(product.ProductName);
                product.Status = ProductStatus.Pending;
                product.StockQuantity = product.InitialQuantity;

                if (product.Images.Any())
                {
                    foreach (var img in product.Images) img.IsPrimary = false;
                    var primaryImg = product.Images.OrderBy(i => i.SortOrder).First();
                    primaryImg.IsPrimary = true;
                }

                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return await GetByIdAsync(product.Id);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<ProductDTO> UpdateAsync(int id, UpdateProductDTO dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var product = await _context.Products
                    .Include(p => p.Images)
                    .Include(p => p.Specifications)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null) throw new NotFoundException("Product not found");

                var currentBrandId = dto.BrandId ?? product.BrandId;
                var currentCategoryId = dto.CategoryId ?? product.CategoryId;

                var brand = await _context.Brands.FindAsync(currentBrandId);
                if (brand == null) throw new NotFoundException("Brand not found");

                var category = await _context.Categories.FindAsync(currentCategoryId);
                if (category == null) throw new NotFoundException("Category not found");

                var oldSkuParts = product.SKU.Split('-');
                var oldProductCode = oldSkuParts.Length == 3 ? oldSkuParts[2] : product.SKU;
                var productCode = dto.ProductCode ?? oldProductCode;
                var generatedSku = $"{brand.Code}-{category.Code}-{productCode}";

                if (product.SKU != generatedSku && await _context.Products.AnyAsync(p => p.SKU == generatedSku))
                    throw new BadRequestException("Product with this SKU already exists", "PRODUCT_SKU_EXISTS");

                _mapper.Map(dto, product);
                product.SKU = generatedSku;
                
                if (dto.ProductName != null) product.Slug = Helpers.GenerateSlug(dto.ProductName);

                product.Status = GetStatus(product);

                product.UpdatedAt = DateTime.Now;

                if (dto.Specifications != null)
                {
                    _context.ProductSpecifications.RemoveRange(product.Specifications);
                    var newSpecs = _mapper.Map<List<ProductSpecification>>(dto.Specifications);
                    foreach (var s in newSpecs) s.ProductId = product.Id;
                    _context.ProductSpecifications.AddRange(newSpecs);
                }

                if (dto.Images != null)
                {
                    if (!dto.Images.Any()) throw new BadRequestException("Product must have at least one image");
                    if (dto.Images.Count > 5) throw new BadRequestException("Product cannot have more than 5 images", "PRODUCT_MAX_IMAGES_EXCEEDED");
                    _context.ProductImages.RemoveRange(product.Images);
                    var newImages = _mapper.Map<List<ProductImage>>(dto.Images);
                    foreach (var img in newImages)
                    {
                        img.ProductId = product.Id;
                        img.IsPrimary = false;
                    }
                    var primaryImg = newImages.OrderBy(i => i.SortOrder).First();
                    primaryImg.IsPrimary = true;
                    _context.ProductImages.AddRange(newImages);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return await GetByIdAsync(product.Id);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> ChangeStatusAsync(int id, ProductStatus newStatus)
        {
            if (newStatus == ProductStatus.Deleted || newStatus == ProductStatus.Pending || newStatus == ProductStatus.OutOfStock)
                throw new BadRequestException("Invalid status change");

            var product = await _context.Products.FindAsync(id);
            if (product == null) throw new NotFoundException("Product not found");

            product.Status = newStatus;
            product.UpdatedAt = DateTime.Now;
            
            await _context.SaveChangesAsync();
            return true;
        }   

        public async Task<bool> UpdateQuantityAsync(int id, int quantity)
        {
            if(quantity < 0) throw new BadRequestException("Quantity cannot be negative");
            var product = await _context.Products.FindAsync(id);
            if (product == null) throw new NotFoundException("Product not found");

            product.InitialQuantity += quantity;
            product.StockQuantity += quantity;

            product.Status = GetStatus(product);

            product.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> UpdateInitialQuantityAsync(int id, int initialQuantity)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) throw new NotFoundException("Product not found");

            if (product.Status != ProductStatus.Pending) throw new BadRequestException("Product must be in pending status to update initial quantity");

            product.InitialQuantity = initialQuantity;
            product.StockQuantity = initialQuantity;

            product.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> UpdateStockQuantityAsync(int id, int stockQuantity)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) throw new NotFoundException("Product not found");

            if (product.InitialQuantity < stockQuantity)
                throw new BadRequestException("Initial quantity cannot be less than stock quantity");

            product.StockQuantity = stockQuantity;

            product.Status = GetStatus(product);

            product.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SoftDeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null) throw new NotFoundException("Product not found");

            product.Status = ProductStatus.Deleted;
            product.DeletedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> HardDeleteAsync(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var product = await _context.Products
                    .Include(p => p.Images)
                    .Include(p => p.Specifications)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null) throw new NotFoundException("Product not found");

                _context.ProductImages.RemoveRange(product.Images);
                _context.ProductSpecifications.RemoveRange(product.Specifications);

                _context.Products.Remove(product);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> RestoreAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) throw new NotFoundException("Product not found");
            if (product.Status != ProductStatus.Deleted) throw new BadRequestException("Product is not deleted");

            product.DeletedAt = null;
            product.Status = GetStatus(new Product
            {
                InitialQuantity = product.InitialQuantity,
                StockQuantity = product.StockQuantity,
                Status = ProductStatus.InStock
            });
            product.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public ProductStatus GetStatus(Product product)
        {
            double stockPercentage = ((double)product.StockQuantity / product.InitialQuantity) * 100;
            if(product.Status == ProductStatus.InStock || product.Status == ProductStatus.LowStock || product.Status == ProductStatus.OutOfStock)
            {
                if (stockPercentage <= 20 && product.StockQuantity > 0)
                {
                    return ProductStatus.LowStock;
                }
                else if (product.StockQuantity > 0)
                {
                    return ProductStatus.InStock;
                }
                else if (product.StockQuantity == 0)
                {
                    return ProductStatus.OutOfStock;
                }
            }
            return product.Status;
        }
    }
}
