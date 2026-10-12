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
            await SeedLocationsAsync(context);
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

        private static async Task SeedLocationsAsync(AppDbContext context)
        {
            var warehouses = new List<Location>
            {
                new() { LocationCode = "HN01", LocationName = "Kho Hà Nội", Latitude = 21.028511m, Longitude = 105.854167m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "SG01", LocationName = "Kho Hồ Chí Minh", Latitude = 10.776889m, Longitude = 106.700806m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "HP01", LocationName = "Kho Hải Phòng", Latitude = 20.844912m, Longitude = 106.688084m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "DN01", LocationName = "Kho Đà Nẵng", Latitude = 16.054407m, Longitude = 108.202167m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "CT01", LocationName = "Kho Cần Thơ", Latitude = 10.045162m, Longitude = 105.746857m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "AG01", LocationName = "Kho An Giang", Latitude = 10.383333m, Longitude = 105.416667m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "VT01", LocationName = "Kho Bà Rịa - Vũng Tàu", Latitude = 10.345987m, Longitude = 107.084260m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "BG01", LocationName = "Kho Bắc Giang", Latitude = 21.273060m, Longitude = 106.194600m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "BK01", LocationName = "Kho Bắc Kạn", Latitude = 22.147040m, Longitude = 105.834820m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "BL01", LocationName = "Kho Bạc Liêu", Latitude = 9.294140m, Longitude = 105.727760m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "BN01", LocationName = "Kho Bắc Ninh", Latitude = 21.186080m, Longitude = 106.076310m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "BT01", LocationName = "Kho Bến Tre", Latitude = 10.241470m, Longitude = 106.375850m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "BD01", LocationName = "Kho Bình Dương", Latitude = 10.980460m, Longitude = 106.651940m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "BI01", LocationName = "Kho Bình Định", Latitude = 13.782970m, Longitude = 109.219660m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "BP01", LocationName = "Kho Bình Phước", Latitude = 11.533330m, Longitude = 106.883330m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "BH01", LocationName = "Kho Bình Thuận", Latitude = 10.933330m, Longitude = 108.100000m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "CM01", LocationName = "Kho Cà Mau", Latitude = 9.176820m, Longitude = 105.152420m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "CB01", LocationName = "Kho Cao Bằng", Latitude = 22.666670m, Longitude = 106.250000m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "DL01", LocationName = "Kho Đắk Lắk", Latitude = 12.666670m, Longitude = 108.050000m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "DK01", LocationName = "Kho Đắk Nông", Latitude = 12.000000m, Longitude = 107.683330m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "DB01", LocationName = "Kho Điện Biên", Latitude = 21.383330m, Longitude = 103.016670m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "NA01", LocationName = "Kho Đồng Nai", Latitude = 10.950000m, Longitude = 106.816670m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "DT01", LocationName = "Kho Đồng Tháp", Latitude = 10.457810m, Longitude = 105.632480m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "GL01", LocationName = "Kho Gia Lai", Latitude = 13.983330m, Longitude = 108.000000m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "HG01", LocationName = "Kho Hà Giang", Latitude = 22.823330m, Longitude = 104.983890m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "HM01", LocationName = "Kho Hà Nam", Latitude = 20.583330m, Longitude = 105.916670m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "HT01", LocationName = "Kho Hà Tĩnh", Latitude = 18.333330m, Longitude = 105.900000m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "HD01", LocationName = "Kho Hải Dương", Latitude = 20.933330m, Longitude = 106.316670m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "HA01", LocationName = "Kho Hậu Giang", Latitude = 9.783330m, Longitude = 105.466670m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "HB01", LocationName = "Kho Hòa Bình", Latitude = 20.816670m, Longitude = 105.333330m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "HY01", LocationName = "Kho Hưng Yên", Latitude = 20.650000m, Longitude = 106.050000m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "KH01", LocationName = "Kho Khánh Hòa", Latitude = 12.250000m, Longitude = 109.183330m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "KG01", LocationName = "Kho Kiên Giang", Latitude = 10.016670m, Longitude = 105.083330m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "KT01", LocationName = "Kho Kon Tum", Latitude = 14.350000m, Longitude = 108.000000m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "LC01", LocationName = "Kho Lai Châu", Latitude = 22.400000m, Longitude = 103.466670m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "LD01", LocationName = "Kho Lâm Đồng", Latitude = 11.950000m, Longitude = 108.433330m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "LS01", LocationName = "Kho Lạng Sơn", Latitude = 21.850000m, Longitude = 106.750000m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "LO01", LocationName = "Kho Lào Cai", Latitude = 22.483330m, Longitude = 103.966670m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "LA01", LocationName = "Kho Long An", Latitude = 10.533330m, Longitude = 106.400000m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "ND01", LocationName = "Kho Nam Định", Latitude = 20.433330m, Longitude = 106.166670m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "NE01", LocationName = "Kho Nghệ An", Latitude = 18.666670m, Longitude = 105.666670m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "NB01", LocationName = "Kho Ninh Bình", Latitude = 20.250000m, Longitude = 105.966670m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "NT01", LocationName = "Kho Ninh Thuận", Latitude = 11.566670m, Longitude = 108.983330m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "PT01", LocationName = "Kho Phú Thọ", Latitude = 21.316670m, Longitude = 105.400000m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "PY01", LocationName = "Kho Phú Yên", Latitude = 13.083330m, Longitude = 109.300000m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "QB01", LocationName = "Kho Quảng Bình", Latitude = 17.483330m, Longitude = 106.600000m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "QM01", LocationName = "Kho Quảng Nam", Latitude = 15.566670m, Longitude = 108.483330m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "QG01", LocationName = "Kho Quảng Ngãi", Latitude = 15.116670m, Longitude = 108.800000m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "QN01", LocationName = "Kho Quảng Ninh", Latitude = 20.950000m, Longitude = 107.083330m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "QT01", LocationName = "Kho Quảng Trị", Latitude = 16.816670m, Longitude = 107.100000m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "ST01", LocationName = "Kho Sóc Trăng", Latitude = 9.600000m, Longitude = 105.966670m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "SL01", LocationName = "Kho Sơn La", Latitude = 21.316670m, Longitude = 103.900000m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "TN01", LocationName = "Kho Tây Ninh", Latitude = 11.300000m, Longitude = 106.100000m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "TB01", LocationName = "Kho Thái Bình", Latitude = 20.450000m, Longitude = 106.333330m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "TY01", LocationName = "Kho Thái Nguyên", Latitude = 21.583330m, Longitude = 105.833330m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "TA01", LocationName = "Kho Thanh Hóa", Latitude = 19.800000m, Longitude = 105.766670m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "TH01", LocationName = "Kho Thừa Thiên Huế", Latitude = 16.466670m, Longitude = 107.583330m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "TG01", LocationName = "Kho Tiền Giang", Latitude = 10.350000m, Longitude = 106.366670m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "TV01", LocationName = "Kho Trà Vinh", Latitude = 9.933330m, Longitude = 106.333330m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "TQ01", LocationName = "Kho Tuyên Quang", Latitude = 21.816670m, Longitude = 105.216670m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "VL01", LocationName = "Kho Vĩnh Long", Latitude = 10.250000m, Longitude = 105.966670m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "VP01", LocationName = "Kho Vĩnh Phúc", Latitude = 21.300000m, Longitude = 105.583330m, IsWarehouse = true, Status = LocationStatus.Active },
                new() { LocationCode = "YB01", LocationName = "Kho Yên Bái", Latitude = 21.700000m, Longitude = 104.866670m, IsWarehouse = true, Status = LocationStatus.Active },
            };

            var existingCodes = await context.Locations.Select(l => l.LocationCode).ToListAsync();
            var toAdd = warehouses.Where(w => !existingCodes.Contains(w.LocationCode)).ToList();
            if (toAdd.Count > 0)
            {
                await context.Locations.AddRangeAsync(toAdd);
                await context.SaveChangesAsync();
            }
        }
    }
}
