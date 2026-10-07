using AgitekBackend.Common;
using AgitekBackend.Common.Utils;
using AgitekBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace AgitekBackend.Data
{
    public static class Seeder
    {
        public static async Task SeedDataAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await context.Database.MigrateAsync();
            await SeedUsersAsync(context);
            await SeedCatalogAsync(context);
        }

        private static async Task SeedUsersAsync(AppDbContext context)
        {
            if (await context.Users.AnyAsync()) return;

            var defaultPasswordHash = AuthUtils.HashPassword("123123");
            var users = new List<User>();
            var userCounter = 1;

            void CreateUser(RoleName role, string usernamePrefix, int count, bool isGuest = false)
            {
                for (var i = 1; i <= count; i++)
                {
                    var user = new User
                    {
                        Role = role,
                        IsActive = true,
                        IsVerified = true,
                        Providers = new List<Provider> { Provider.Credentials },
                        Profile = new Profile
                        {
                            FullName = isGuest ? $"Guest User {i}" : $"{role} User {i}",
                            PhoneNumber = $"098765432{userCounter % 10}",
                            Gender = GenderType.Other,
                            DateOfBirth = new DateTime(1990, 1, 1).AddDays(userCounter),
                            AvatarUrl = "https://ui-avatars.com/api/?name=" + (isGuest ? "Guest" : role.ToString())
                        },
                        Username = isGuest ? null : $"{usernamePrefix}{i}",
                        Email = isGuest ? null : $"{usernamePrefix}{i}@agitek.com",
                        PasswordHash = isGuest ? null : defaultPasswordHash
                    };

                    users.Add(user);
                    userCounter++;
                }
            }

            CreateUser(RoleName.SuperAdmin, "superadmin", 1);
            CreateUser(RoleName.Admin, "admin", 3);
            CreateUser(RoleName.Manager, "manager", 2);
            CreateUser(RoleName.Staff, "staff", 2);
            CreateUser(RoleName.Delivery, "delivery", 2);
            CreateUser(RoleName.Customer, "customer", 2);
            CreateUser(RoleName.Guest, "guest", 2, isGuest: true);

            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();
        }

        private static async Task SeedCatalogAsync(AppDbContext context)
        {
            if (!await context.Categories.AnyAsync())
            {
                var categoryNames = new[]
                {
                    "PC Gaming", "PC Văn phòng", "Linh kiện máy tính", "CPU", "Card đồ họa",
                    "Bo mạch chủ", "RAM", "Ổ cứng", "Nguồn máy tính", "Phụ kiện máy tính"
                };

                var categoryEntities = categoryNames.Select((name, index) => new Category
                {
                    Code = $"C{index + 1:00}",
                    CategoryName = name,
                    Slug = Helpers.GenerateSlug(name)
                }).ToList();

                await context.Categories.AddRangeAsync(categoryEntities);
                await context.SaveChangesAsync();
            }

            if (!await context.Brands.AnyAsync())
            {
                var brandNames = new[]
                {
                    "ASUS", "MSI", "GIGABYTE", "Intel", "AMD",
                    "NVIDIA", "Kingston", "Samsung", "Corsair", "Cooler Master"
                };

                var brandEntities = brandNames.Select((name, index) => new Brand
                {
                    Code = $"B{index + 1:00}",
                    Name = name,
                    Slug = Helpers.GenerateSlug(name),
                    LogoUrl = null
                }).ToList();

                await context.Brands.AddRangeAsync(brandEntities);
                await context.SaveChangesAsync();
            }

            if (await context.Products.AnyAsync()) return;

            var categories = await context.Categories.OrderBy(c => c.Id).Take(10).ToListAsync();
            var brands = await context.Brands.OrderBy(b => b.Id).Take(10).ToListAsync();
            var seedUser = await context.Users
                .Where(u => u.Role != RoleName.Guest)
                .OrderBy(u => u.Id)
                .FirstOrDefaultAsync();

            if (categories.Count < 10 || brands.Count < 10 || seedUser == null) return;

            var random = new Random(20251005);
            var statuses = new[]
            {
                ProductStatus.InStock, ProductStatus.InStock, ProductStatus.InStock, ProductStatus.InStock,
                ProductStatus.InStock, ProductStatus.InStock, ProductStatus.InStock, ProductStatus.InStock,
                ProductStatus.InStock, ProductStatus.InStock, ProductStatus.InStock, ProductStatus.InStock,
                ProductStatus.InStock, ProductStatus.InStock, ProductStatus.InStock, ProductStatus.InStock,
                ProductStatus.InStock, ProductStatus.InStock, ProductStatus.InStock, ProductStatus.InStock,
                ProductStatus.InStock, ProductStatus.InStock, ProductStatus.InStock, ProductStatus.InStock,
                ProductStatus.InStock, ProductStatus.InStock, ProductStatus.InStock, ProductStatus.InStock,
                ProductStatus.InStock, ProductStatus.InStock, ProductStatus.InStock, ProductStatus.InStock,
                ProductStatus.InStock, ProductStatus.InStock, ProductStatus.InStock,
                ProductStatus.LowStock, ProductStatus.LowStock, ProductStatus.LowStock, ProductStatus.LowStock,
                ProductStatus.LowStock, ProductStatus.LowStock, ProductStatus.LowStock,
                ProductStatus.OutOfStock, ProductStatus.OutOfStock, ProductStatus.OutOfStock,
                ProductStatus.PreOrder, ProductStatus.PreOrder, ProductStatus.Pending,
                ProductStatus.Hidden, ProductStatus.Deleted
            };

            var products = new List<Product>();
            for (var index = 1; index <= 50; index++)
            {
                var status = statuses[index - 1];
                var initialQuantity = random.Next(40, 301);
                var stockQuantity = status switch
                {
                    ProductStatus.OutOfStock or ProductStatus.Deleted => 0,
                    ProductStatus.LowStock => random.Next(1, Math.Max(2, initialQuantity / 5)),
                    ProductStatus.PreOrder => 0,
                    ProductStatus.Pending or ProductStatus.Hidden => random.Next(5, initialQuantity),
                    _ => random.Next(Math.Max(1, initialQuantity / 2), initialQuantity + 1)
                };
                var originalPrice = random.Next(8, 151) * 1_000_000m;
                var hasDiscount = random.Next(0, 100) < 45;
                var createdAt = DateTime.Now.AddDays(-random.Next(0, 365));

                products.Add(new Product
                {
                    SKU = $"AGI-{index:000}",
                    ProductName = $"{brands[(index - 1) % brands.Count].Name} {categories[(index - 1) % categories.Count].CategoryName} Pro {index:00}",
                    Slug = $"{Helpers.GenerateSlug(brands[(index - 1) % brands.Count].Name)}-{Helpers.GenerateSlug(categories[(index - 1) % categories.Count].CategoryName)}-pro-{index:00}",
                    OriginalPrice = originalPrice,
                    DiscountPrice = hasDiscount ? Math.Round(originalPrice * (decimal)(random.Next(80, 96) / 100.0), 0) : null,
                    InitialQuantity = initialQuantity,
                    StockQuantity = stockQuantity,
                    SoldQuantity = random.Next(0, 501),
                    AverageRating = Math.Round(2.5 + random.NextDouble() * 2.5, 1),
                    Description = $"Sản phẩm mẫu {index} dành cho hệ sinh thái Agitek PC.",
                    Status = status,
                    WarrantyPeriod = random.Next(12, 37),
                    CategoryId = categories[(index - 1) % categories.Count].Id,
                    BrandId = brands[(index - 1) % brands.Count].Id,
                    UserId = seedUser.Id,
                    CreatedAt = createdAt,
                    UpdatedAt = createdAt
                });
            }

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }
    }
}
