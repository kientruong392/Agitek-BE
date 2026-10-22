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
            await SeedProductDetailAsync(context);
            await SeedVouchersAsync(context);
            await SeedLocationsAsync(context);
        }

        public static async Task ReseedOrdersAsync(this WebApplication app)
        {
            await app.SeedDataAsync();

            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var orderedQuantities = await context.OrderDetails
                .AsNoTracking()
                .GroupBy(detail => detail.ProductId)
                .ToDictionaryAsync(group => group.Key, group => group.Sum(detail => detail.Quantity));

            await using (var transaction = await context.Database.BeginTransactionAsync())
            {
                await context.Medias.ExecuteDeleteAsync();
                await context.Refunds.ExecuteDeleteAsync();
                await context.Warranties.ExecuteDeleteAsync();
                await context.Reviews.ExecuteDeleteAsync();
                await context.Payments.ExecuteDeleteAsync();
                await context.OrderTrackingLogs.ExecuteDeleteAsync();
                await context.OrderDetails.ExecuteDeleteAsync();
                await context.Orders.ExecuteDeleteAsync();
                await context.Locations
                    .Where(location => !location.IsWarehouse && location.LocationCode.StartsWith("DLAGT"))
                    .ExecuteDeleteAsync();
                await context.Products.ExecuteUpdateAsync(setters => setters
                    .SetProperty(product => product.AverageRating, 0d));
                await RestoreProductInventoryAsync(context, orderedQuantities);

                await transaction.CommitAsync();
            }

            await ResetOrderDataIdentitiesAsync(context);
            await SeedOrdersForReviewTestingAsync(context);
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
            if (await context.Products.AnyAsync()) return;
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

        private static async Task SeedProductDetailAsync(AppDbContext context)
        {
            var product = await context.Products
                .FirstOrDefaultAsync(item => item.SKU == "B03-RAM-PROD013");
            if (product == null) return;

            product.Description = "RAM DDR5 32GB hiệu năng cao, phù hợp cho máy tính chơi game và làm việc chuyên nghiệp. Thiết kế tản nhiệt giúp duy trì hoạt động ổn định, hỗ trợ đa nhiệm mượt mà và dễ dàng nâng cấp hệ thống. Sản phẩm gồm hai thanh nhớ 16GB, bảo hành 36 tháng.";

            if (!await context.ProductImages.AnyAsync(image => image.ProductId == product.Id))
            {
                context.ProductImages.AddRange(
                    new ProductImage { ProductId = product.Id, ImageUrl = "https://placehold.co/900x900/png?text=DDR5+RAM+Front", IsPrimary = true, SortOrder = 0 },
                    new ProductImage { ProductId = product.Id, ImageUrl = "https://placehold.co/900x900/png?text=DDR5+RAM+Heatspreader", SortOrder = 1 },
                    new ProductImage { ProductId = product.Id, ImageUrl = "https://placehold.co/900x900/png?text=DDR5+RAM+Details", SortOrder = 2 }
                );
            }

            var specifications = new Dictionary<string, string>
            {
                ["Dung lượng"] = "32GB (2 x 16GB)",
                ["Loại RAM"] = "DDR5",
                ["Tốc độ"] = "5600MHz",
                ["Độ trễ"] = "CL36",
                ["Điện áp"] = "1.25V",
                ["Tản nhiệt"] = "Nhôm",
                ["Bảo hành"] = "36 tháng"
            };

            var existingSpecifications = await context.ProductSpecifications
                .Where(specification => specification.ProductId == product.Id)
                .ToListAsync();
            foreach (var (name, value) in specifications)
            {
                var specification = existingSpecifications.FirstOrDefault(item => item.SpecName == name);
                if (specification == null)
                {
                    context.ProductSpecifications.Add(new ProductSpecification
                    {
                        ProductId = product.Id,
                        SpecName = name,
                        SpecValue = value
                    });
                }
                else
                {
                    specification.SpecValue = value;
                }
            }

            if (!await context.Reviews.AnyAsync(review => review.ProductId == product.Id))
            {
                var customers = await context.Users
                    .Where(user => user.Role == RoleName.Customer)
                    .OrderBy(user => user.Id)
                    .ToListAsync();
                if (customers.Count == 0)
                {
                    customers = await context.Users
                        .Where(user => user.Role != RoleName.Guest)
                        .OrderBy(user => user.Id)
                        .Take(5)
                        .ToListAsync();
                }

                if (customers.Count > 0)
                {
                    var sampleReviews = new[]
                    {
                        new { Rating = 5, Comment = "RAM chạy ổn định, bật XMP dễ dàng và máy khởi động nhanh. Đóng gói cẩn thận." },
                        new { Rating = 4, Comment = "Hiệu năng tốt, đa nhiệm mượt. Tản nhiệt đẹp và nhiệt độ khi chơi game vẫn ổn." },
                        new { Rating = 3, Comment = "RAM hoạt động ổn định, lắp đặt đơn giản. Mong có thêm hướng dẫn cấu hình XMP chi tiết." },
                        new { Rating = 5, Comment = "Đúng thông số, tương thích tốt với cấu hình DDR5 của mình. Rất hài lòng." },
                        new { Rating = 4, Comment = "Sản phẩm tốt, giao hàng nhanh. Dùng cho công việc và chơi game đều đáp ứng." }
                    };
                    var now = DateTime.Now;
                    var orders = new List<Order>();
                    var orderDetails = new List<OrderDetail>();
                    var reviews = new List<Review>();

                    for (var index = 0; index < sampleReviews.Length; index++)
                    {
                        var customer = customers[index % customers.Count];
                        var createdAt = now.AddDays(-(sampleReviews.Length - index) * 3);
                        var order = new Order
                        {
                            TrackingNumber = $"SEED-RAM-{index + 1:000}",
                            UserId = customer.Id,
                            Address = "Địa chỉ mẫu, Quận 1, TP. Hồ Chí Minh",
                            SubtotalPrice = product.OriginalPrice,
                            ShippingFee = 0,
                            DiscountAmount = 0,
                            TotalPrice = product.OriginalPrice,
                            Status = OrderStatus.Completed,
                            CreatedAt = createdAt,
                            UpdatedAt = createdAt
                        };
                        orders.Add(order);
                        orderDetails.Add(new OrderDetail
                        {
                            Order = order,
                            ProductId = product.Id,
                            Quantity = 1,
                            UnitPrice = product.OriginalPrice,
                            TotalPrice = product.OriginalPrice
                        });
                        reviews.Add(new Review
                        {
                            OrderDetail = orderDetails[^1],
                            ProductId = product.Id,
                            UserId = customer.Id,
                            Rating = sampleReviews[index].Rating,
                            Comment = sampleReviews[index].Comment,
                            CreatedAt = createdAt,
                            UpdatedAt = createdAt
                        });
                    }

                    await context.Orders.AddRangeAsync(orders);
                    await context.OrderDetails.AddRangeAsync(orderDetails);
                    await context.Reviews.AddRangeAsync(reviews);
                }
            }

            await context.SaveChangesAsync();
            var averageRating = await context.Reviews
                .Where(review => review.ProductId == product.Id)
                .AverageAsync(review => (double?)review.Rating);
            product.AverageRating = averageRating ?? 0;
            product.UpdatedAt = DateTime.Now;
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

        private static async Task RestoreProductInventoryAsync(
            AppDbContext context,
            IReadOnlyDictionary<int, int> orderedQuantities)
        {
            if (orderedQuantities.Count == 0) return;

            var products = await context.Products
                .Where(product => orderedQuantities.Keys.Contains(product.Id))
                .ToDictionaryAsync(product => product.Id);
            var now = DateTime.UtcNow;

            foreach (var (productId, quantity) in orderedQuantities)
            {
                if (!products.TryGetValue(productId, out var product)) continue;

                product.StockQuantity = Math.Min(product.InitialQuantity, product.StockQuantity + quantity);
                product.SoldQuantity = Math.Max(0, product.SoldQuantity - quantity);
                product.UpdatedAt = now;
                UpdateInventoryStatus(product);
            }

            await context.SaveChangesAsync();
        }

        private static void UpdateInventoryStatus(Product product)
        {
            if (product.Status is ProductStatus.Deleted or ProductStatus.Hidden or ProductStatus.Pending or ProductStatus.PreOrder)
                return;

            product.Status = product.StockQuantity == 0
                ? ProductStatus.OutOfStock
                : product.InitialQuantity > 0 && (decimal)product.StockQuantity * 100m <= (decimal)product.InitialQuantity * 20m
                    ? ProductStatus.LowStock
                    : ProductStatus.InStock;
        }

        private static async Task ResetOrderDataIdentitiesAsync(AppDbContext context)
        {
            var commands = new[]
            {
                "DBCC CHECKIDENT ('[Payments]', RESEED, 0)",
                "DBCC CHECKIDENT ('[OrderTrackingLogs]', RESEED, 0)",
                "DBCC CHECKIDENT ('[Reviews]', RESEED, 0)",
                "DBCC CHECKIDENT ('[Warranties]', RESEED, 0)",
                "DBCC CHECKIDENT ('[Medias]', RESEED, 0)",
                "DBCC CHECKIDENT ('[Orders]', RESEED, 0)"
            };

            foreach (var command in commands)
            {
                await context.Database.ExecuteSqlRawAsync(command);
            }
        }

        private static async Task SeedOrdersForReviewTestingAsync(AppDbContext context)
        {
            var customers = await context.Users
                .Where(user => user.Role == RoleName.Customer && user.IsActive)
                .Include(user => user.Profile)
                .OrderBy(user => user.Id)
                .Take(2)
                .ToListAsync();
            var products = await context.Products
                .Where(product => product.Status == ProductStatus.InStock && product.DeletedAt == null && product.StockQuantity >= 3)
                .OrderBy(product => product.Id)
                .Take(8)
                .ToListAsync();
            var locations = await context.Locations
                .Where(location => location.IsWarehouse && location.Status == LocationStatus.Active)
                .OrderBy(location => location.Id)
                .Take(2)
                .ToListAsync();
            var approverId = await context.Users
                .Where(user => user.Role == RoleName.Staff || user.Role == RoleName.Manager || user.Role == RoleName.Admin)
                .OrderBy(user => user.Id)
                .Select(user => (int?)user.Id)
                .FirstOrDefaultAsync();
            var deliveryUserId = await context.Users
                .Where(user => user.Role == RoleName.Delivery && user.IsActive)
                .OrderBy(user => user.Id)
                .Select(user => (int?)user.Id)
                .FirstOrDefaultAsync();

            if (customers.Count == 0)
                throw new InvalidOperationException("At least one active customer is required to seed orders");
            if (products.Count < 5)
                throw new InvalidOperationException("At least five in-stock products are required to seed orders");
            if (locations.Count == 0 || !deliveryUserId.HasValue)
                throw new InvalidOperationException("An active warehouse and delivery user are required to seed orders");

            var customerIds = customers.Select(customer => customer.Id).ToList();
            var addressesByUserId = await context.Addresses
                .Where(address => customerIds.Contains(address.UserId))
                .OrderByDescending(address => address.IsDefault)
                .ThenBy(address => address.Id)
                .GroupBy(address => address.UserId)
                .ToDictionaryAsync(group => group.Key, group => group.First());
            foreach (var customer in customers.Where(customer => !addressesByUserId.ContainsKey(customer.Id)))
            {
                var address = new Address
                {
                    UserId = customer.Id,
                    ReceiverName = customer.Profile?.FullName ?? $"Customer {customer.Id}",
                    PhoneNumber = customer.Profile?.PhoneNumber ?? "0900000000",
                    AddressLine = $"{customer.Id} Nguyen Hue, District 1, Ho Chi Minh City",
                    IsDefault = true
                };
                context.Addresses.Add(address);
                addressesByUserId[customer.Id] = address;
            }
            await context.SaveChangesAsync();

            var now = DateTime.UtcNow;
            var orderNumber = 0;
            var productIndex = 0;
            var orders = new List<Order>();
            var deliveredOrders = new List<Order>();
            var deliveryLocations = new List<Location>();
            var fromLocation = locations[0];

            Order CreateOrder(
                User customer,
                Address address,
                OrderStatus status,
                int daysAgo,
                params (Product Product, int Quantity)[] requestedProducts)
            {
                orderNumber++;
                var createdAt = now.AddDays(-daysAgo);
                var confirmedAt = createdAt.AddHours(4);
                var transitAt = confirmedAt.AddHours(8);
                var deliveredAt = transitAt.AddDays(1);
                var trackingNumber = $"AGT{createdAt:yyMMdd}SEED{orderNumber:000}";
                var addressSnapshot = $"{address.ReceiverName.Trim()}, {address.PhoneNumber.Trim()}, {address.AddressLine.Trim()}";
                var deliveryLocation = new Location
                {
                    LocationCode = $"DL{trackingNumber}",
                    LocationName = addressSnapshot.Length > 255 ? addressSnapshot[..255] : addressSnapshot,
                    Latitude = 10.776889m + orderNumber * 0.001m,
                    Longitude = 106.700806m + orderNumber * 0.001m,
                    IsWarehouse = false,
                    Status = LocationStatus.Active
                };
                deliveryLocations.Add(deliveryLocation);
                var shippingType = orderNumber % 2 == 0 ? ShippingType.Express : ShippingType.Standard;
                var shippingFee = shippingType == ShippingType.Express ? 50_000m : 30_000m;
                var details = requestedProducts.Select(request =>
                {
                    var unitPrice = request.Product.DiscountPrice.HasValue && request.Product.DiscountPrice.Value < request.Product.OriginalPrice
                        ? request.Product.DiscountPrice.Value
                        : request.Product.OriginalPrice;
                    return new OrderDetail
                    {
                        ProductId = request.Product.Id,
                        Quantity = request.Quantity,
                        UnitPrice = unitPrice,
                        TotalPrice = unitPrice * request.Quantity
                    };
                }).ToList();
                var subtotal = details.Sum(detail => detail.TotalPrice);
                var order = new Order
                {
                    TrackingNumber = trackingNumber,
                    UserId = customer.Id,
                    ApprovedByUserId = approverId,
                    DeliveredByUserId = deliveryUserId,
                    FromLocationId = fromLocation.Id,
                    AddressId = address.Id,
                    ToLocation = deliveryLocation,
                    Address = addressSnapshot,
                    ShippingType = shippingType,
                    SubtotalPrice = subtotal,
                    ShippingFee = shippingFee,
                    DiscountAmount = 0m,
                    TotalPrice = subtotal + shippingFee,
                    Status = status,
                    Note = "Dữ liệu mẫu phục vụ kiểm thử đơn hàng và đánh giá sản phẩm.",
                    CreatedAt = createdAt,
                    UpdatedAt = status == OrderStatus.Delivered ? deliveredAt : transitAt,
                    OrderDetails = details
                };

                var paymentMethod = orderNumber % 2 == 0 ? PaymentMethod.Bank : PaymentMethod.COD;
                var isPaid = status == OrderStatus.Delivered || paymentMethod == PaymentMethod.Bank;
                order.Payments.Add(new Payment
                {
                    Amount = order.TotalPrice,
                    PaymentMethod = paymentMethod,
                    Status = isPaid ? PaymentStatus.Success : PaymentStatus.Pending,
                    CreatedAt = createdAt,
                    PaidAt = isPaid ? (paymentMethod == PaymentMethod.COD ? deliveredAt : createdAt.AddMinutes(10)) : null,
                    TransactionId = paymentMethod == PaymentMethod.Bank ? $"SEED-BANK-{orderNumber:000}" : null
                });
                order.OrderTrackingLogs.Add(new OrderTrackingLog
                {
                    Order = order,
                    LocationId = fromLocation.Id,
                    Description = "Đơn hàng đã được tạo.",
                    Timestamp = createdAt
                });
                order.OrderTrackingLogs.Add(new OrderTrackingLog
                {
                    Order = order,
                    LocationId = fromLocation.Id,
                    Description = "Đơn hàng đã được xác nhận và bàn giao vận chuyển.",
                    Timestamp = createdAt.AddHours(4)
                });
                if (status == OrderStatus.Delivered)
                {
                    order.OrderTrackingLogs.Add(new OrderTrackingLog
                    {
                        Order = order,
                        Location = deliveryLocation,
                        Description = "Đơn hàng đã giao thành công.",
                        Timestamp = deliveredAt
                    });
                }

                return order;
            }

            foreach (var customer in customers)
            {
                var address = addressesByUserId[customer.Id];
                var firstDelivered = CreateOrder(
                    customer,
                    address,
                    OrderStatus.Delivered,
                    45 - orderNumber * 2,
                    (products[productIndex++ % products.Count], 2),
                    (products[productIndex++ % products.Count], 1));
                var secondDelivered = CreateOrder(
                    customer,
                    address,
                    OrderStatus.Delivered,
                    24 - orderNumber,
                    (products[productIndex++ % products.Count], 1));

                orders.Add(firstDelivered);
                orders.Add(secondDelivered);
                deliveredOrders.Add(firstDelivered);
                deliveredOrders.Add(secondDelivered);
            }

            orders.Add(CreateOrder(
                customers[0],
                addressesByUserId[customers[0].Id],
                OrderStatus.InTransit,
                3,
                (products[productIndex % products.Count], 1)));

            foreach (var product in products)
            {
                var quantity = orders
                    .SelectMany(order => order.OrderDetails)
                    .Where(detail => detail.ProductId == product.Id)
                    .Sum(detail => detail.Quantity);
                if (quantity == 0) continue;

                product.StockQuantity -= quantity;
                product.SoldQuantity += quantity;
                product.UpdatedAt = now;
                UpdateInventoryStatus(product);
            }

            var reviewedDetail = deliveredOrders[0].OrderDetails.First();
            var reviewedProduct = products.First(product => product.Id == reviewedDetail.ProductId);
            reviewedProduct.AverageRating = 5;
            reviewedProduct.UpdatedAt = now;

            context.Locations.AddRange(deliveryLocations);
            context.Orders.AddRange(orders);
            context.Reviews.Add(new Review
            {
                OrderDetail = reviewedDetail,
                ProductId = reviewedDetail.ProductId,
                UserId = deliveredOrders[0].UserId,
                Rating = 5,
                Comment = "Sản phẩm đúng mô tả, đây là đánh giá mẫu đã có sẵn.",
                CreatedAt = deliveredOrders[0].UpdatedAt,
                UpdatedAt = deliveredOrders[0].UpdatedAt
            });

            await context.SaveChangesAsync();
        }
    }
}
