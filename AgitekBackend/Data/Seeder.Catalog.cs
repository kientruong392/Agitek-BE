using AgitekBackend.Common;
using AgitekBackend.Common.Utils;
using AgitekBackend.Models;
using AgitekBackend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AgitekBackend.Data
{
    public static partial class Seeder
    {
        public static async Task ReseedCatalogAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var semanticSearchService = scope.ServiceProvider.GetRequiredService<ISemanticSearchService>();

            await context.Database.MigrateAsync();
            await SeedUsersAsync(context);

            await using var transaction = await context.Database.BeginTransactionAsync();
            var productIds = await WriteProductCatalogAsync(context);

            // Read the saved specifications again when generating embeddings in the same transaction.
            context.ChangeTracker.Clear();
            foreach (var productId in productIds)
            {
                await semanticSearchService.SynchronizeProductAsync(productId);
            }

            await transaction.CommitAsync();
            app.Logger.LogInformation(
                "Reseeded {ProductCount} real-world catalog products and synchronized their embeddings. Existing product IDs and order relationships were preserved.",
                productIds.Count);
        }

        private static async Task SeedCatalogAsync(AppDbContext context)
        {
            if (await context.Products.AnyAsync()) return;

            await using var transaction = await context.Database.BeginTransactionAsync();
            await WriteProductCatalogAsync(context);
            await transaction.CommitAsync();
        }

        private static async Task<List<int>> WriteProductCatalogAsync(AppDbContext context)
        {
            ValidateCatalogSeedData();
            await SeedCatalogCategoriesAsync(context);
            await SeedCatalogBrandsAsync(context);

            var categories = await context.Categories.ToDictionaryAsync(category => category.Code);
            var brands = (await context.Brands.ToListAsync())
                .ToDictionary(brand => brand.Name, StringComparer.OrdinalIgnoreCase);
            var seedUser = await context.Users
                .Where(user => user.Role == RoleName.SuperAdmin || user.Role == RoleName.Admin)
                .OrderBy(user => user.Id)
                .FirstOrDefaultAsync()
                ?? throw new InvalidOperationException("An administrator is required to seed the product catalog");

            var existingProducts = await context.Products
                .Include(product => product.Specifications)
                .Include(product => product.Images)
                .ToListAsync();
            var reservedQuantities = await context.OrderDetails
                .Where(detail => detail.Order != null
                    && detail.Order.Status != OrderStatus.Cancelled
                    && detail.Order.Status != OrderStatus.Refunded
                    && detail.Order.Status != OrderStatus.Returned)
                .GroupBy(detail => detail.ProductId)
                .ToDictionaryAsync(group => group.Key, group => group.Sum(detail => detail.Quantity));
            var soldQuantities = await context.OrderDetails
                .Where(detail => detail.Order != null
                    && (detail.Order.Status == OrderStatus.Delivered || detail.Order.Status == OrderStatus.Completed))
                .GroupBy(detail => detail.ProductId)
                .ToDictionaryAsync(group => group.Key, group => group.Sum(detail => detail.Quantity));
            var averageRatings = await context.Reviews
                .GroupBy(review => review.ProductId)
                .ToDictionaryAsync(group => group.Key, group => group.Average(review => (double)review.Rating));

            var now = DateTime.UtcNow;
            var seededProducts = new List<Product>();
            foreach (var seed in ProductCatalogSeedData.Products)
            {
                var brand = brands[seed.BrandName];
                var category = categories[seed.CategoryCode];
                var sku = $"{brand.Code}-{category.Code}-{seed.ProductCode}";
                if (sku.Length > 30)
                    throw new InvalidOperationException($"Seed SKU exceeds 30 characters: {sku}");

                var legacySuffix = $"-PROD{seed.LegacyNumber:000}";
                var product = existingProducts.FirstOrDefault(item => item.SKU == sku)
                    ?? existingProducts.FirstOrDefault(item => item.SKU.EndsWith(legacySuffix, StringComparison.Ordinal)
                        && item.ProductName.Contains("Cao Cấp", StringComparison.OrdinalIgnoreCase));

                if (product == null)
                {
                    product = new Product
                    {
                        UserId = seedUser.Id,
                        CreatedAt = now.AddDays(-seed.LegacyNumber)
                    };
                    context.Products.Add(product);
                }
                else
                {
                    context.ProductSpecifications.RemoveRange(product.Specifications);
                    context.ProductImages.RemoveRange(product.Images);
                    product.Specifications.Clear();
                    product.Images.Clear();
                }

                var reservedQuantity = reservedQuantities.GetValueOrDefault(product.Id);
                product.SKU = sku;
                product.ProductName = seed.ProductName;
                product.Slug = Helpers.GenerateSlug(seed.ProductName);
                product.Description = seed.Description;
                product.OriginalPrice = seed.Price;
                product.DiscountPrice = seed.LegacyNumber % 4 == 0 ? Math.Round(seed.Price * 0.95m, 0) : null;
                product.BrandId = brand.Id;
                product.CategoryId = category.Id;
                product.WarrantyPeriod = seed.WarrantyPeriod;
                product.InitialQuantity = Math.Max(60, reservedQuantity + 40);
                product.StockQuantity = product.InitialQuantity - reservedQuantity;
                product.SoldQuantity = soldQuantities.GetValueOrDefault(product.Id);
                product.AverageRating = averageRatings.GetValueOrDefault(product.Id);
                product.Status = ProductStatus.InStock;
                product.DeletedAt = null;
                product.UpdatedAt = now;
                product.Specifications = seed.Specifications.Select(specification => new ProductSpecification
                {
                    SpecName = specification.Name,
                    SpecValue = specification.Value
                }).ToList();
                product.Images.Add(new ProductImage
                {
                    ImageUrl = seed.ImageUrl
                        ?? $"https://placehold.co/900x900/png?text={Uri.EscapeDataString($"{seed.BrandName} {seed.ProductCode}")}",
                    IsPrimary = true,
                    SortOrder = 0
                });

                seededProducts.Add(product);
            }

            await context.SaveChangesAsync();
            return seededProducts.Select(product => product.Id).ToList();
        }

        private static async Task SeedCatalogCategoriesAsync(AppDbContext context)
        {
            var categoryStructure = new (string Name, string Code, (string Name, string Code)[] Children)[]
            {
                ("Linh kiện PC", "PCPART", [("CPU", "CPU"), ("Mainboard", "MAIN"), ("RAM", "RAM"),
                    ("VGA", "VGA"), ("Ổ cứng (SSD/HDD)", "DRIVE"), ("Nguồn (PSU)", "PSU"), ("Vỏ case", "CASE")]),
                ("Bộ PC", "BOPC", [("PC Gaming", "GAMPC"), ("PC Đồ họa", "GRAPC"), ("PC Văn phòng", "OFFPC")]),
                ("Laptop", "LAP", [("Laptop Gaming", "GAMLAP"), ("Laptop Đồ họa", "GRALAP"), ("Laptop Văn phòng", "OFFLAP")]),
                ("Gaming Gear", "GEAR", [("Bàn phím", "KBD"), ("Chuột", "MOUSE"), ("Tai nghe", "HEADP")]),
                ("Thiết bị mạng", "NET", [("Router", "ROUTER"), ("Switch", "SWITCH"), ("Kìm bấm mạng", "KBNET"), ("LAN Adapter", "LAN")]),
                ("Phụ kiện", "ACC", [("USB", "USB"), ("Cáp kết nối (HDMI, DisplayPort, Type-C)", "CABLE"),
                    ("Hub chuyển đổi", "HUB"), ("Đầu đọc thẻ", "CARD")]),
                ("Màn hình", "SCREEN", [("Màn hình máy tính", "MON")])
            };
            var existingCategories = await context.Categories.ToListAsync();

            foreach (var parentSeed in categoryStructure)
            {
                var parent = existingCategories.FirstOrDefault(category => category.Code == parentSeed.Code);
                if (parent == null)
                {
                    parent = new Category
                    {
                        Code = parentSeed.Code,
                        CategoryName = parentSeed.Name,
                        Slug = Helpers.GenerateSlug(parentSeed.Name)
                    };
                    context.Categories.Add(parent);
                    existingCategories.Add(parent);
                }

                foreach (var childSeed in parentSeed.Children)
                {
                    if (existingCategories.Any(category => category.Code == childSeed.Code)) continue;

                    var child = new Category
                    {
                        Code = childSeed.Code,
                        CategoryName = childSeed.Name,
                        Slug = Helpers.GenerateSlug(childSeed.Name),
                        ParentCategory = parent
                    };
                    context.Categories.Add(child);
                    existingCategories.Add(child);
                }
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedCatalogBrandsAsync(AppDbContext context)
        {
            var brandSeeds = new (string Name, string Code)[]
            {
                ("ASUS", "ASUS"), ("MSI", "MSI"), ("GIGABYTE", "GIGA"), ("Intel", "INTEL"),
                ("AMD", "AMD"), ("Kingston", "KING"), ("Samsung", "SAM"), ("Corsair", "COR"),
                ("Cooler Master", "CMAS"), ("HACOM", "HACOM"), ("Apple", "APPLE"), ("Lenovo", "LENOVO"),
                ("Acer", "ACER"), ("Dell", "DELL"), ("HP", "HPINC"), ("Western Digital", "WDC"),
                ("Seagate", "SEAG"), ("Logitech", "LOGI"), ("HyperX", "HYPERX"),
                ("TP-Link", "TPLINK"), ("SanDisk", "SDISK"), ("Anker", "ANKER"), ("LG", "LGE")
            };
            var existingNames = await context.Brands.Select(brand => brand.Name).ToListAsync();
            foreach (var (name, code) in brandSeeds)
            {
                if (existingNames.Contains(name, StringComparer.OrdinalIgnoreCase)) continue;

                context.Brands.Add(new Brand
                {
                    Code = code,
                    Name = name,
                    Slug = Helpers.GenerateSlug(name)
                });
            }

            await context.SaveChangesAsync();
        }

        private static void ValidateCatalogSeedData()
        {
            var seeds = ProductCatalogSeedData.Products;
            if (seeds.Select(seed => seed.LegacyNumber).Distinct().Count() != seeds.Count
                || seeds.Select(seed => seed.ProductCode).Distinct().Count() != seeds.Count
                || seeds.Select(seed => Helpers.GenerateSlug(seed.ProductName)).Distinct().Count() != seeds.Count)
                throw new InvalidOperationException("Catalog seed identifiers and slugs must be unique");

            foreach (var seed in seeds)
            {
                if (string.IsNullOrWhiteSpace(seed.ProductName) || seed.ProductName.Length > 100
                    || seed.Price <= 0 || seed.WarrantyPeriod < 0
                    || !Uri.TryCreate(seed.ReferenceUrl, UriKind.Absolute, out var referenceUri)
                    || referenceUri.Scheme != Uri.UriSchemeHttps
                    || seed.Specifications.Length == 0
                    || seed.Specifications.Select(specification => specification.Name).Distinct().Count() != seed.Specifications.Length
                    || seed.Specifications.Any(specification => string.IsNullOrWhiteSpace(specification.Name)
                        || specification.Name.Length > 50 || string.IsNullOrWhiteSpace(specification.Value)
                        || specification.Value.Length > 200))
                    throw new InvalidOperationException($"Invalid catalog seed data: {seed.ProductCode}");
            }
        }
    }
}
