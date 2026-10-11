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
            await SeedVouchersAsync(context);
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
            if (await context.Products.AnyAsync())
            {
                context.Products.RemoveRange(context.Products);
                await context.SaveChangesAsync();
            }
            if (await context.Categories.AnyAsync())
            {
                context.Categories.RemoveRange(context.Categories);
                await context.SaveChangesAsync();
            }

            var categoryStructure = new[]
            {
                new { Name = "Linh kiện PC", Code = "PCPART", Sub = new[] { 
                    new { Name = "CPU", Code = "CPU" },
                    new { Name = "Mainboard", Code = "MAIN" },
                    new { Name = "RAM", Code = "RAM" },
                    new { Name = "VGA", Code = "VGA" },
                    new { Name = "Ổ cứng (SSD/HDD)", Code = "DRIVE" },
                    new { Name = "Nguồn (PSU)", Code = "PSU" },
                    new { Name = "Vỏ case", Code = "CASE" }
                }},
                new { Name = "Bộ PC", Code = "BOPC", Sub = new[] {
                    new { Name = "PC Gaming", Code = "GAMPC" },
                    new { Name = "PC Đồ họa", Code = "GRAPC" },
                    new { Name = "PC Văn phòng", Code = "OFFPC" }
                }},
                new { Name = "Laptop", Code = "LAP", Sub = new[] {
                    new { Name = "Laptop Gaming", Code = "GAMLAP" },
                    new { Name = "Laptop Đồ họa", Code = "GRALAP" },
                    new { Name = "Laptop Văn phòng", Code = "OFFLAP" }
                }},
                new { Name = "Gaming Gear", Code = "GEAR", Sub = new[] {
                    new { Name = "Bàn phím", Code = "KBD" },
                    new { Name = "Chuột", Code = "MOUSE" },
                    new { Name = "Tai nghe", Code = "HEADP" }
                }},
                new { Name = "Thiết bị mạng", Code = "NET", Sub = new[] {
                    new { Name = "Router", Code = "ROUTER" },
                    new { Name = "Switch", Code = "SWITCH" },
                    new { Name = "Kiềm bấm mạng", Code = "KBNET" },
                    new { Name = "LAN Adapter", Code = "LAN" }
                }},
                new { Name = "Phụ kiện", Code = "ACC", Sub = new[] {
                    new { Name = "USB", Code = "USB" },
                    new { Name = "Cáp kết nối (HDMI, DisplayPort, Type-C)", Code = "CABLE" },
                    new { Name = "Hub chuyển đổi", Code = "HUB" },
                    new { Name = "Đầu đọc thẻ", Code = "CARD" }
                }}
            };

            var categoryEntities = new List<Category>();

            foreach (var parent in categoryStructure)
            {
                var parentEntity = new Category
                {
                    Code = parent.Code,
                    CategoryName = parent.Name,
                    Slug = Helpers.GenerateSlug(parent.Name),
                    SubCategories = new List<Category>()
                };

                foreach (var child in parent.Sub)
                {
                    parentEntity.SubCategories.Add(new Category
                    {
                        Code = child.Code,
                        CategoryName = child.Name,
                        Slug = Helpers.GenerateSlug(child.Name)
                    });
                }
                
                categoryEntities.Add(parentEntity);
            }

            await context.Categories.AddRangeAsync(categoryEntities);
            await context.SaveChangesAsync();

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

            var categories = await context.Categories.Where(c => c.ParentCategoryId != null).OrderBy(c => c.Id).Take(10).ToListAsync();
            if (!categories.Any()) categories = await context.Categories.OrderBy(c => c.Id).Take(10).ToListAsync();
            
            var brands = await context.Brands.OrderBy(b => b.Id).Take(10).ToListAsync();
            var seedUser = await context.Users
                .Where(u => u.Role != RoleName.Guest)
                .OrderBy(u => u.Id)
                .FirstOrDefaultAsync();

            if (categories.Count == 0 || brands.Count < 10 || seedUser == null) return;

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

                var brand = brands[(index - 1) % brands.Count];
                var category = categories[(index - 1) % categories.Count];

                products.Add(new Product
                {
                    SKU = $"{brand.Code}-{category.Code}-PROD{index:000}",
                    ProductName = $"{brand.Name} {category.CategoryName} Cao Cấp {index:00}",
                    Slug = $"{Helpers.GenerateSlug(brand.Name)}-{Helpers.GenerateSlug(category.CategoryName)}-cao-cap-{index:00}",
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

        private static async Task SeedVouchersAsync(AppDbContext context)
        {
            if (await context.Vouchers.AnyAsync()) return;

            var random = new Random(20251006);
            var vouchers = new List<Voucher>();

            for (var index = 1; index <= 20; index++)
            {
                var isPercentage = random.Next(0, 2) == 0;
                var discountType = isPercentage ? DiscountType.Percentage : DiscountType.FixedAmount;
                var discountValue = isPercentage ? random.Next(5, 51) : random.Next(1, 21) * 10000m;
                var maxDiscountValue = isPercentage ? random.Next(5, 51) * 10000m : (decimal?)null;
                var minOrderValue = random.Next(5, 201) * 10000m;
                var initialQuantity = random.Next(50, 1001);
                var quantity = random.Next(Math.Max(1, initialQuantity / 2), initialQuantity + 1);
                var startDate = DateTime.Now.AddDays(-random.Next(0, 10));
                var endDate = DateTime.Now.AddDays(random.Next(15, 60));

                vouchers.Add(new Voucher
                {
                    Code = $"VOUCHER{index:00}-{random.Next(1000, 9999)}",
                    Name = $"Voucher Khuyến Mãi {index:00}",
                    Description = $"Mô tả cho voucher khuyến mãi số {index:00}. Giảm ngay {(isPercentage ? $"{discountValue}%" : $"{discountValue:N0}₫")}.",
                    DiscountType = discountType,
                    DiscountValue = discountValue,
                    MaxDiscountValue = maxDiscountValue,
                    MinOrderValue = minOrderValue,
                    InitialQuantity = initialQuantity,
                    Quantity = quantity,
                    UsedCount = initialQuantity - quantity,
                    StartDate = startDate,
                    EndDate = endDate,
                    Status = VoucherStatus.Active
                });
            }

            await context.Vouchers.AddRangeAsync(vouchers);
            await context.SaveChangesAsync();
        }
    }
}
