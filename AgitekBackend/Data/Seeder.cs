using AgitekBackend.Common;
using AgitekBackend.Common.Utils;
using AgitekBackend.Models;
using AgitekBackend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AgitekBackend.Data
{
    public static class Seeder
    {
        public static async Task SeedDataAsync(this WebApplication app, bool backfillAuditLogs = true)
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.AuditEnabled = false;

            await context.Database.MigrateAsync();
            await SeedUsersAsync(context);
            await SeedCatalogAsync(context);
            await SeedVouchersAsync(context);
            await SeedLocationsAsync(context);
            if (backfillAuditLogs)
                await SeedExistingResourceAuditLogsAsync(context);
        }

        public static async Task ReseedOrdersAsync(this WebApplication app)
        {
            await app.SeedDataAsync(backfillAuditLogs: false);

            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.AuditEnabled = false;
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
            await SeedExistingResourceAuditLogsAsync(context);
        }

        public static async Task SeedReportOrdersAsync(this WebApplication app)
        {
            await app.SeedDataAsync(backfillAuditLogs: false);

            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.AuditEnabled = false;

            if (!await context.Orders.AnyAsync(order => order.TrackingNumber.Contains("SEED")))
                await SeedOrdersForReviewTestingAsync(context);

            await SeedExistingResourceAuditLogsAsync(context);
        }

        private static async Task SeedExistingResourceAuditLogsAsync(AppDbContext context)
        {
            var products = await context.Products.AsNoTracking()
                .Select(product => new { product.Id, product.UserId, product.SKU, product.CreatedAt })
                .ToListAsync();
            var orders = await context.Orders.AsNoTracking()
                .Select(order => new { order.Id, order.UserId, order.TrackingNumber, order.CreatedAt })
                .ToListAsync();
            var existingLogs = await context.AuditLogs.AsNoTracking()
                .Where(log => (log.EntityName == nameof(Product) || log.EntityName == nameof(Order)) &&
                    log.Action.StartsWith("Created #"))
                .Select(log => new { log.EntityName, log.Action, log.Timestamp })
                .ToListAsync();
            var existingActions = existingLogs
                .Select(log => (log.EntityName, log.Action))
                .ToHashSet();
            var normalCreationTimes = existingLogs
                .Where(log => !log.Action.Contains("[backfill:"))
                .ToLookup(log => (log.EntityName, log.Action), log => log.Timestamp);
            var newLogs = new List<AuditLog>();

            bool HasCreationLog(string entityName, int id, DateTime createdAt, string backfillAction)
            {
                if (existingActions.Contains((entityName, backfillAction)))
                    return true;

                var originalAction = $"Created #{id}";
                return normalCreationTimes[(entityName, originalAction)]
                    .Any(timestamp => Math.Abs((timestamp - createdAt).TotalHours) <= 24);
            }

            foreach (var product in products)
            {
                var action = $"Created #{product.Id} [backfill:{product.SKU}]";
                if (HasCreationLog(nameof(Product), product.Id, product.CreatedAt, action))
                    continue;

                newLogs.Add(new AuditLog
                {
                    Timestamp = product.CreatedAt,
                    UserId = product.UserId,
                    EntityName = nameof(Product),
                    Action = action
                });
            }

            foreach (var order in orders)
            {
                var action = $"Created #{order.Id} [backfill:{order.TrackingNumber}]";
                if (HasCreationLog(nameof(Order), order.Id, order.CreatedAt, action))
                    continue;

                newLogs.Add(new AuditLog
                {
                    Timestamp = order.CreatedAt,
                    UserId = order.UserId,
                    EntityName = nameof(Order),
                    Action = action
                });
            }

            if (newLogs.Count == 0)
                return;

            context.AuditLogs.AddRange(newLogs);
            await context.SaveChangesAsync();
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
        public static async Task ReseedCatalogAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.AuditEnabled = false;
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
            await SeedExistingResourceAuditLogsAsync(context);
            app.Logger.LogInformation(
                "Reseeded {ProductCount} real-world catalog products and synchronized their embeddings. Existing product IDs and order relationships were preserved.",
                productIds.Count);
        }

        public static async Task ReseedCatalogImagesAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.AuditEnabled = false;
            var brands = await context.Brands.AsNoTracking()
                .ToDictionaryAsync(brand => brand.Name, brand => brand.Code);
            var products = await context.Products.Include(product => product.Images)
                .ToDictionaryAsync(product => product.SKU);

            var updated = 0;
            foreach (var seed in Products)
            {
                var sku = $"{brands[seed.BrandName]}-{seed.CategoryCode}-{seed.ProductCode}";
                if (!products.TryGetValue(sku, out var product))
                    continue;

                var imageUrl = GetCatalogImageUrl(seed.ProductCode);
                var primaryImage = product.Images.FirstOrDefault(image => image.IsPrimary)
                    ?? product.Images.OrderBy(image => image.SortOrder).FirstOrDefault();
                if (primaryImage == null)
                {
                    product.Images.Add(new ProductImage
                    {
                        ImageUrl = imageUrl,
                        IsPrimary = true,
                        SortOrder = 0
                    });
                }
                else
                {
                    primaryImage.ImageUrl = imageUrl;
                    primaryImage.IsPrimary = true;
                }
                updated++;
            }

            await context.SaveChangesAsync();
            app.Logger.LogInformation("Updated primary images for {ProductCount} products without changing product details.", updated);
        }

        private static async Task SeedCatalogAsync(AppDbContext context)
        {
            await using var transaction = await context.Database.BeginTransactionAsync();
            if (!await context.Products.AnyAsync()) await WriteProductCatalogAsync(context);
            else await SeedMissingBuilderProductsAsync(context);
            await transaction.CommitAsync();
        }

        private static async Task SeedMissingBuilderProductsAsync(AppDbContext context)
        {
            await SeedCatalogCategoriesAsync(context);
            await SeedCatalogBrandsAsync(context);
            var categories = await context.Categories.ToDictionaryAsync(category => category.Code);
            var brands = (await context.Brands.ToListAsync()).ToDictionary(brand => brand.Name, StringComparer.OrdinalIgnoreCase);
            var products = await context.Products.Include(product => product.Specifications).ToListAsync();
            var userId = await context.Users.Where(user => user.Role == RoleName.SuperAdmin || user.Role == RoleName.Admin)
                .OrderBy(user => user.Id).Select(user => user.Id).FirstAsync();

            foreach (var seed in Products.Where(seed =>
                seed.CategoryCode is "MAIN" or "CPU" or "COOL" or "RAM" or "VGA" or "DRIVE" or "PSU" or "CASE"))
            {
                var sku = $"{brands[seed.BrandName].Code}-{seed.CategoryCode}-{seed.ProductCode}";
                var product = products.FirstOrDefault(item => item.SKU == sku);
                if (product == null)
                {
                    product = new Product
                    {
                        SKU = sku, ProductName = seed.ProductName, Slug = Helpers.GenerateSlug(seed.ProductName),
                        BrandId = brands[seed.BrandName].Id, CategoryId = categories[seed.CategoryCode].Id, UserId = userId,
                        OriginalPrice = seed.Price, Description = seed.Description, WarrantyPeriod = seed.WarrantyPeriod,
                        InitialQuantity = 20, StockQuantity = 20, Status = ProductStatus.InStock,
                        Images = [new ProductImage { ImageUrl = GetCatalogImageUrl(seed.ProductCode), IsPrimary = true }]
                    };
                    context.Products.Add(product);
                }
                foreach (var group in GetSpecifications(seed).GroupBy(spec => spec.Name))
                    if (!product.Specifications.Any(spec => spec.SpecName == group.Key))
                        foreach (var spec in group)
                            product.Specifications.Add(new ProductSpecification { SpecName = spec.Name, SpecValue = spec.Value });
            }
            await context.SaveChangesAsync();
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
            foreach (var seed in Products)
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
                product.Specifications = GetSpecifications(seed).Select(specification => new ProductSpecification
                {
                    SpecName = specification.Name,
                    SpecValue = specification.Value
                }).ToList();
                product.Images.Add(new ProductImage
                {
                    ImageUrl = GetCatalogImageUrl(seed.ProductCode),
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
                    ("VGA", "VGA"), ("Ổ cứng (SSD/HDD)", "DRIVE"), ("Nguồn (PSU)", "PSU"), ("Vỏ case", "CASE"), ("Tản nhiệt CPU", "COOL")]),
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
                ("TP-Link", "TPLINK"), ("SanDisk", "SDISK"), ("Anker", "ANKER"), ("LG", "LGE"),
                ("DeepCool", "DCOOL"), ("Noctua", "NOCT"), ("Jonsbo", "JON")
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
            var seeds = Products;
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
                    || seed.Specifications.Distinct().Count() != seed.Specifications.Length
                    || seed.Specifications.Any(specification => string.IsNullOrWhiteSpace(specification.Name)
                        || specification.Name.Length > 50 || string.IsNullOrWhiteSpace(specification.Value)
                        || specification.Value.Length > 200))
                    throw new InvalidOperationException($"Invalid catalog seed data: {seed.ProductCode}");
            }
        }

        // Fixed real-world models/configurations for semantic-search testing.
        // Prices, warranty periods and availability are demo store data, not live retail offers.
        // ReferenceUrl is provenance only; it is not added to the embedding text.
        internal sealed record ProductSeed(
            int LegacyNumber,
            string BrandName,
            string CategoryCode,
            string ProductCode,
            string ProductName,
            decimal Price,
            int WarrantyPeriod,
            string Description,
            string ReferenceUrl,
            (string Name, string Value)[] Specifications,
            string? ImageUrl = null);

        // Files live in agitek-frontend/public/images/catalog; sources.json records their public sources.
        private static readonly Dictionary<string, string> CatalogImageFiles = new()
        {
            ["I512400F"] = "cpu-intel-12400f.jpg",
            ["B760MAD4"] = "main-asus-b760.png",
            ["KF432C16BBK216"] = "ram-kingston-ddr4.jpg",
            ["TUF3090O24G"] = "gpu-asus-3090.png",
            ["990PRO2TB"] = "ssd-samsung-990.jpg",
            ["RM750E2023"] = "psu-corsair-750.png",
            ["4000DAIRFLOW"] = "case-corsair-4000d.png",
            ["HACOM022"] = "pc-hacom-022.jpg",
            ["POSEIDON3090"] = "pc-poseidon-3090.jpg",
            ["ECONOMY035"] = "pc-economy-035.jpg",
            ["I513400F"] = "cpu-intel-13400f.jpg",
            ["B550MPROVDHW"] = "main-msi-b550.png",
            ["KF556C36BBEK232"] = "ram-corsair-ddr5.png",
            ["RTX3060VENTUS12"] = "gpu-msi-3060.jpg",
            ["970EVOPLUS1TB"] = "ssd-samsung-970.jpg",
            ["MWE650BRONZEV2"] = "psu-corsair-750.png",
            ["TD500MESHV2"] = "case-corsair-4000d.png",
            ["TD500PC14700KF"] = "pc-hacom-022.jpg",
            ["MACMINIM28256"] = "mac-mini-m2.jpg",
            ["ECONOMY083"] = "pc-economy-083.jpg",
            ["I514600K"] = "cpu-intel-13400f.jpg",
            ["B650MDS3H"] = "main-msi-b550.png",
            ["CMK32G6000C36"] = "ram-corsair-ddr5.png",
            ["DUAL4060O8G"] = "gpu-asus-3090.png",
            ["SN7701TB"] = "ssd-samsung-990.jpg",
            ["TUF850GOLD"] = "psu-asus-850.png",
            ["FA507NULP101W"] = "laptop-asus.jpg",
            ["83GS001RVN"] = "laptop-lenovo.png",
            ["ANV155158AN"] = "laptop-acer.png",
            ["X1504VANJ526W"] = "laptop-asus.jpg",
            ["RYZEN55600"] = "cpu-amd-7800x3d.jpg",
            ["N5I5007W1"] = "laptop-dell.jpg",
            ["KVR32S22S816"] = "ram-kingston-ddr4.jpg",
            ["4070SUPERWF12"] = "gpu-msi-3060.jpg",
            ["NV2500GB"] = "ssd-samsung-970.jpg",
            ["MBAIRM28256"] = "macbook-air-m2.png",
            ["G413SE"] = "keyboard-g413.jpg",
            ["240G108F134PA"] = "laptop-hp.jpg",
            ["G502HERO"] = "mouse-g502.png",
            ["CLOUDIIWIRED"] = "headset-hyperx.jpg",
            ["RYZEN75700X"] = "cpu-amd-7800x3d.jpg",
            ["RYZEN77800X3D"] = "cpu-amd-7800x3d.jpg",
            ["KF432C16BB8"] = "ram-kingston-ddr4.jpg",
            ["4060TIVENTUS16"] = "gpu-msi-3060.jpg",
            ["ST2000DM008"] = "hdd-seagate-2tb.jpg",
            ["ARCHERAX55"] = "router-tplink.jpg",
            ["LS1005G"] = "switch-tplink.jpg",
            ["ULTRAFLAIR128"] = "usb-sandisk.png",
            ["ANKER555A8383"] = "hub-anker.png",
            ["27GP850B"] = "monitor-lg.png",
            ["TUF650PLUSW"] = "main-asus-b760.png",
            ["AG400"] = "cooler-ag400.jpg",
            ["NHL9I17XX"] = "cooler-ag400.jpg",
            ["C6BLACK"] = "case-jonsbo-c6.jpg"
        };

        private static string GetCatalogImageUrl(string productCode)
        {
            if (!CatalogImageFiles.TryGetValue(productCode, out var fileName))
                throw new InvalidOperationException($"No catalog image mapped for {productCode}.");

            return $"/images/catalog/{fileName}";
        }

        internal static IReadOnlyList<ProductSeed> Products { get; } =
        [
            new(1, "Intel", "CPU", "I512400F", "CPU Intel Core i5-12400F - 6 nhân 12 luồng, LGA1700", 2_790_000m, 36,
                "Bộ xử lý desktop Alder Lake với 6 nhân hiệu năng và 12 luồng, phù hợp lắp máy chơi Liên Minh Huyền Thoại, Valorant và học lập trình. Phiên bản F không có đồ họa tích hợp, cần card màn hình rời. Hỗ trợ DDR4 hoặc DDR5 tùy bo mạch chủ.",
                "https://www.intel.com/content/www/us/en/products/sku/134587/intel-core-i512400f-processor-18m-cache-up-to-4-40-ghz/specifications.html",
                [("Socket", "LGA1700"), ("Nhân / luồng", "6 / 12"), ("Xung cơ bản / turbo", "2.5 / 4.4 GHz"), ("Bộ nhớ đệm L3", "18MB"), ("RAM hỗ trợ", "DDR4-3200 hoặc DDR5-4800"), ("Đồ họa tích hợp", "Không; cần GPU rời")]),

            new(2, "ASUS", "MAIN", "B760MAD4", "Mainboard ASUS PRIME B760M-A D4 - LGA1700, DDR4, Micro-ATX", 2_990_000m, 36,
                "Bo mạch chủ Intel B760 cho hệ thống Core thế hệ 12, 13 và 14 theo danh sách CPU hỗ trợ và phiên bản BIOS. Dùng RAM desktop DDR4, không lắp được DDR5. Kích thước Micro-ATX, có hai khe M.2 cho SSD NVMe, phù hợp nâng cấp một bộ máy Intel phổ thông.",
                "https://www.asus.com/motherboards-components/motherboards/prime/prime-b760m-a-d4/techspec/",
                [("Socket", "LGA1700"), ("Chipset", "Intel B760"), ("Kích thước", "Micro-ATX"), ("Khe RAM", "4 khe DIMM DDR4"), ("Lưu trữ", "2 khe M.2 PCIe 4.0 x4"), ("Mạng dây", "Realtek 2.5Gb Ethernet")]),

            new(3, "Kingston", "RAM", "KF432C16BBK216", "RAM Kingston FURY Beast 16GB (2x8GB) DDR4-3200 CL16", 1_090_000m, 36,
                "Bộ nhớ desktop gồm hai thanh 8GB DDR4, chạy dual-channel trên bo mạch tương thích. Tản nhiệt nhôm thấp, hỗ trợ Intel XMP. Phù hợp nâng cấp máy tính chơi game Full HD và đa nhiệm cơ bản; đây là RAM hệ thống, không phải bộ nhớ của card đồ họa.",
                "https://www.kingston.com/datasheets/KF432C16BBK2_16.pdf",
                [("Mã model", "KF432C16BBK2/16"), ("Dung lượng", "16GB (2 x 8GB)"), ("Chuẩn RAM", "DDR4 UDIMM desktop"), ("Tốc độ", "3200 MT/s"), ("Độ trễ", "CL16"), ("Điện áp XMP", "1.35V")]),

            new(4, "ASUS", "VGA", "TUF3090O24G", "VGA ASUS TUF Gaming GeForce RTX 3090 OC 24GB GDDR6X", 27_990_000m, 36,
                "Card đồ họa NVIDIA Ampere với 24GB VRAM GDDR6X, 10496 nhân CUDA và ba quạt tản nhiệt. Dung lượng VRAM lớn phục vụ dựng cảnh Blender, render CUDA, xử lý video và thử nghiệm mô hình AI local phù hợp bộ nhớ. GPU hỗ trợ ray tracing và DLSS; nguồn khuyến nghị 850W.",
                "https://www.asus.com/au/motherboards-components/graphics-cards/tuf-gaming/tuf-rtx3090-o24g-gaming/techspec/",
                [("Mã model", "TUF-RTX3090-O24G-GAMING"), ("GPU", "NVIDIA GeForce RTX 3090"), ("VRAM", "24GB GDDR6X"), ("Bus bộ nhớ", "384-bit"), ("Nhân CUDA", "10496"), ("Nguồn khuyến nghị", "850W"), ("Nguồn phụ", "2 x 8-pin")]),

            new(5, "Samsung", "DRIVE", "990PRO2TB", "SSD Samsung 990 PRO 2TB NVMe PCIe 4.0 x4 M.2 2280", 4_990_000m, 60,
                "SSD NVMe dung lượng 2TB dùng cho cài hệ điều hành, thư viện game lớn và ổ làm việc cho dự án dựng phim. Chuẩn M.2 2280 PCIe Gen4, tốc độ đọc tuần tự tối đa 7450MB/s và ghi tối đa 6900MB/s trong điều kiện thử nghiệm của hãng. Cần khe M.2 NVMe tương thích.",
                "https://www.samsung.com/us/memory-storage/nvme-ssd/990-pro-2tb-nvme-pcie-gen-4-mz-v9p2t0b-am/",
                [("Mã model", "MZ-V9P2T0BW"), ("Dung lượng", "2TB"), ("Kích thước", "M.2 2280"), ("Giao tiếp", "PCIe 4.0 x4 NVMe"), ("Đọc tuần tự tối đa", "7450MB/s"), ("Ghi tuần tự tối đa", "6900MB/s")]),

            new(6, "Corsair", "PSU", "RM750E2023", "Nguồn Corsair RM750e 2023 750W 80 PLUS Gold Full Modular", 2_490_000m, 84,
                "Bộ nguồn ATX công suất 750W, chứng nhận 80 PLUS Gold và dây cáp tháo rời hoàn toàn để đi dây gọn. Phiên bản 2023 hỗ trợ ATX 3.0 và có cáp cấp điện cho card đồ họa PCIe 5.0. Phù hợp hệ thống dùng GPU rời khi tổng công suất và đầu cắm được tính toán đúng.",
                "https://www.corsair.com/us/en/p/psu/cp-9020262-na/rme-series-rm750e-fully-modular-low-noise-atx-power-supply-cp-9020262-na",
                [("Mã model", "CP-9020262"), ("Công suất", "750W"), ("Hiệu suất", "80 PLUS Gold"), ("Đi dây", "Full Modular"), ("Chuẩn nguồn", "ATX 3.0, phiên bản 2023")]),

            new(7, "Corsair", "CASE", "4000DAIRFLOW", "Vỏ case Corsair 4000D AIRFLOW Black - Mid Tower ATX", 2_190_000m, 24,
                "Thùng máy mặt trước dạng lưới và hông kính cường lực, dành cho người muốn lắp PC thoáng khí, đi dây gọn. Hỗ trợ bo mạch ATX, Micro-ATX và Mini-ITX; khoảng trống card đồ họa tối đa 360mm. Sản phẩm là vỏ case, không bao gồm CPU, RAM, GPU hoặc nguồn.",
                "https://www.corsair.com/us/en/p/pc-cases/cc-9011200-ww/4000d-airflow-tempered-glass-mid-tower-atx-case-black-cc-9011200-ww",
                [("Mã model", "CC-9011200-WW"), ("Loại case", "Mid Tower"), ("Mainboard hỗ trợ", "ATX / Micro-ATX / Mini-ITX"), ("Chiều dài GPU tối đa", "360mm"), ("Chiều cao tản CPU tối đa", "170mm"), ("Mặt trước", "Lưới Airflow")]),

            new(8, "HACOM", "GAMPC", "HACOM022", "PC HACOM 022 Powered by ASUS - i5-12400F, RTX 4060 8GB, RAM 16GB, SSD 512GB", 18_590_000m, 36,
                "Bộ máy lắp ráp dùng CPU Intel Core i5-12400F và ASUS Dual RTX 4060 8GB, RAM DDR4 16GB cùng SSD Lexar NM620 512GB. Cấu hình phục vụ chơi game Full HD, học lập trình và livestream với bộ mã hóa NVENC. Mainboard ASUS PRIME B760M-A D4 và nguồn ASUS TUF 650W Bronze. Không bao gồm màn hình.",
                "https://hacom.vn/pc-hacom-022-intel-i5-12400f-vga-rtx-4060-powered-by-asus",
                [("CPU", "Intel Core i5-12400F, 6 nhân 12 luồng"), ("GPU", "ASUS Dual GeForce RTX 4060 8GB GDDR6"), ("RAM hệ thống", "16GB DDR4-3200 (2 x 8GB)"), ("SSD", "Lexar NM620 512GB NVMe"), ("Mainboard", "ASUS PRIME B760M-A D4"), ("Nguồn", "ASUS TUF Gaming 650W Bronze"), ("Màn hình đi kèm", "Không")],
                "https://cdn-files.hacom.vn/hacom//88096_pc_hacom_022_intel_i5_12400f_vga_rtx_4060_powered_by_asus__2.jpg"),

            // The bill of materials specifies DDR4 (Z690-PLUS WIFI D4), despite the page's DDR5 summary.
            new(9, "HACOM", "GRAPC", "POSEIDON3090", "PC HACOM POSEIDON - i7-13700K, ASUS RTX 3090 24GB, RAM 32GB, SSD 500GB", 70_990_000m, 36,
                "Bộ PC dùng Intel Core i7-13700K, ASUS ROG STRIX RTX 3090 24GB và 32GB RAM DDR4-3600. Cấu hình có tản nhiệt nước custom, case Lian Li O11 Dynamic Evo và nguồn FSP Hydro G Pro 850W Gold. Phù hợp dựng hình 3D, Blender, biên tập video và thử nghiệm AI cần nhiều VRAM. Không kèm màn hình.",
                "https://hacom.vn/hacom-poseidon-i7-13700k-rtx-3090",
                [("CPU", "Intel Core i7-13700K, 16 nhân 24 luồng"), ("GPU", "ASUS ROG STRIX RTX 3090 OC 24GB GDDR6X"), ("RAM hệ thống", "32GB DDR4-3600 (4 x 8GB)"), ("SSD", "WD Black SN770 500GB NVMe"), ("Mainboard", "ASUS TUF GAMING Z690-PLUS WIFI D4"), ("Nguồn", "FSP Hydro G Pro 850W 80 PLUS Gold"), ("Tản nhiệt", "Nước custom Freezemod")],
                "https://hanoicomputercdn.com/media/product/68681_pcgm626_23_11.jpg"),

            new(10, "HACOM", "OFFPC", "ECONOMY035", "PC HACOM ECONOMY 035 - Core i3-12100, RAM 8GB, SSD 256GB, Wi-Fi", 7_990_000m, 36,
                "Máy tính để bàn Intel Core i3-12100 với RAM 8GB và SSD 256GB, sử dụng đồ họa tích hợp Intel UHD 730. Có Wi-Fi, phù hợp công việc Word, Excel, kế toán, học online và tra cứu web. Không trang bị card đồ họa rời, không kèm màn hình.",
                "https://hacom.vn/pc-hacom-economy-035-pcvp0590",
                [("CPU", "Intel Core i3-12100, 4 nhân 8 luồng"), ("RAM hệ thống", "8GB"), ("SSD", "256GB"), ("GPU", "Intel UHD Graphics 730 tích hợp"), ("Kết nối", "Wi-Fi"), ("Màn hình đi kèm", "Không")]),

            new(11, "Intel", "CPU", "I513400F", "CPU Intel Core i5-13400F - 10 nhân 16 luồng, LGA1700", 4_390_000m, 36,
                "CPU desktop Intel thế hệ 13, kết hợp 6 nhân hiệu năng và 4 nhân tiết kiệm điện. Thích hợp vừa chơi game vừa mở ứng dụng nền, biên dịch mã nguồn và đa nhiệm. Phiên bản F yêu cầu GPU rời; loại RAM sử dụng phụ thuộc bo mạch chủ DDR4 hoặc DDR5.",
                "https://www.intel.com/content/www/us/en/products/sku/230501/intel-core-i513400f-processor-20m-cache-up-to-4-60-ghz/specifications.html",
                [("Socket", "LGA1700"), ("Nhân / luồng", "10 (6P + 4E) / 16"), ("Turbo tối đa", "4.6 GHz"), ("Bộ nhớ đệm L3", "20MB"), ("Đồ họa tích hợp", "Không")]),

            new(12, "MSI", "MAIN", "B550MPROVDHW", "Mainboard MSI B550M PRO-VDH WIFI - AM4, DDR4, Wi-Fi AC", 2_390_000m, 36,
                "Bo mạch chủ Micro-ATX nền tảng AMD AM4 cho máy Ryzen dùng RAM DDR4. Có Wi-Fi AC, Bluetooth và hai khe M.2. Phù hợp xây dựng hoặc nâng cấp PC Ryzen 5 5600 và Ryzen 7 5700X sau khi kiểm tra BIOS tương thích; không dùng cho Ryzen AM5.",
                "https://www.msi.com/Motherboard/B550M-PRO-VDH-WIFI/Specification",
                [("Socket", "AMD AM4"), ("Chipset", "AMD B550"), ("Kích thước", "Micro-ATX"), ("Khe RAM", "4 khe DIMM DDR4"), ("Lưu trữ", "2 khe M.2"), ("Không dây", "Wi-Fi AC, Bluetooth 4.2")]),

            // Preserve the former PROD013 RAM's ID and its review/order/warranty relationships.
            new(13, "Kingston", "RAM", "KF556C36BBEK232", "RAM Kingston FURY Beast 32GB (2x16GB) DDR5-5600 CL36", 2_490_000m, 36,
                "Kit RAM desktop DDR5 gồm hai thanh 16GB, tổng 32GB, hỗ trợ AMD EXPO và Intel XMP 3.0. Phù hợp mở nhiều tab trình duyệt, chạy máy ảo, lập trình và làm việc với ảnh hoặc video. Cần bo mạch dùng DDR5; không thể cắm vào khe DDR4.",
                "https://www.kingston.com/datasheets/KF556C36BBEK2-32.pdf",
                [("Mã model", "KF556C36BBEK2-32"), ("Dung lượng", "32GB (2 x 16GB)"), ("Chuẩn RAM", "DDR5 UDIMM desktop"), ("Tốc độ", "5600 MT/s"), ("Độ trễ", "CL36"), ("Điện áp profile", "1.25V"), ("Profile", "AMD EXPO, Intel XMP 3.0")]),

            new(14, "MSI", "VGA", "RTX3060VENTUS12", "VGA MSI GeForce RTX 3060 VENTUS 2X OC 12GB GDDR6", 6_990_000m, 36,
                "Card NVIDIA Ampere với 12GB VRAM, 3584 nhân CUDA và thiết kế hai quạt. Dùng cho chơi game Full HD, biên tập video và thử nghiệm CUDA hoặc Stable Diffusion trong giới hạn bộ nhớ. Đây là phiên bản RTX 3060 12GB, không phải RTX 3060 8GB hay RTX 3060 Ti.",
                "https://us.msi.com/Graphics-Card/GeForce-RTX-3060-VENTUS-2X-12G-OC/Specification",
                [("GPU", "NVIDIA GeForce RTX 3060"), ("VRAM", "12GB GDDR6"), ("Bus bộ nhớ", "192-bit"), ("Nhân CUDA", "3584"), ("Boost clock", "1807 MHz"), ("Nguồn khuyến nghị", "550W")]),

            new(15, "Samsung", "DRIVE", "970EVOPLUS1TB", "SSD Samsung 970 EVO Plus 1TB NVMe PCIe 3.0 x4 M.2 2280", 1_990_000m, 60,
                "SSD NVMe Gen3 dung lượng 1TB cho nâng cấp laptop hoặc desktop có khe M.2 tương thích. Dùng để cài Windows, phần mềm và lưu dự án; đọc tuần tự tối đa 3500MB/s, ghi tối đa 3300MB/s. Không phải ổ M.2 SATA.",
                "https://image.semiconductor.samsung.com/resources/data-sheet/Samsung_NVMe_SSD_970_EVO_Plus_Data_Sheet_Rev.3.0.pdf",
                [("Mã model", "MZ-V7S1T0BW"), ("Dung lượng", "1TB"), ("Kích thước", "M.2 2280"), ("Giao tiếp", "PCIe 3.0 x4 NVMe"), ("Đọc tuần tự tối đa", "3500MB/s"), ("Ghi tuần tự tối đa", "3300MB/s")]),

            new(16, "Cooler Master", "PSU", "MWE650BRONZEV2", "Nguồn Cooler Master MWE 650 Bronze V2 650W", 1_390_000m, 36,
                "Nguồn ATX 650W đạt chứng nhận 80 PLUS Bronze, sử dụng thiết kế DC-to-DC và quạt 120mm. Dành cho cấu hình desktop phổ thông có GPU rời khi công suất tổng phù hợp. Dây cáp gắn liền, không phải phiên bản modular.",
                "https://www.coolermaster.com/en-my/products/mwe-650-bronze-v2-full-range.html",
                [("Công suất", "650W"), ("Hiệu suất", "80 PLUS Bronze"), ("Chuẩn kích thước", "ATX"), ("Quạt", "120mm"), ("Đi dây", "Non-modular")]),

            new(17, "Cooler Master", "CASE", "TD500MESHV2", "Vỏ case Cooler Master MasterBox TD500 Mesh V2 Black ARGB", 2_190_000m, 24,
                "Case Mid Tower mặt lưới với ba quạt ARGB 120mm lắp sẵn phía trước, phù hợp bộ máy nhiều linh kiện cần lưu thông khí. Hỗ trợ radiator 360mm và GPU dài tối đa 410mm. Có kính cường lực cùng USB-C phía trước. Chỉ bao gồm vỏ case và phụ kiện theo hãng.",
                "https://sfcc.coolermaster.com/en-us/products/masterbox-td500-mesh-v2.html",
                [("Mã model", "TD500V2-KGNN-S00"), ("Mainboard hỗ trợ", "ATX / Micro-ATX / Mini-ITX"), ("Quạt đi kèm", "3 x 120mm ARGB phía trước"), ("Chiều dài GPU tối đa", "410mm"), ("Tản CPU tối đa", "165mm"), ("Radiator hỗ trợ", "Tối đa 360mm trước hoặc trên")]),

            new(18, "Cooler Master", "GAMPC", "TD500PC14700KF", "PC Cooler Master TD500 Mesh V2 - i7-14700KF, RTX 4070 SUPER, RAM 16GB, SSD 1TB", 42_990_000m, 36,
                "Bộ máy Cooler Master TD500 Mesh V2 dùng Intel Core i7-14700KF, GeForce RTX 4070 SUPER 12GB và RAM DDR5 16GB. SSD NVMe 1TB phục vụ lưu trò chơi và dự án. Phù hợp chơi game 1440p, livestream, dựng video và render GPU, với case mặt lưới ARGB. Không kèm màn hình.",
                "https://legacy.coolermaster.com/en-gb/products/td500-mesh-v2-gaming-pc/",
                [("CPU", "Intel Core i7-14700KF, 20 nhân 28 luồng"), ("GPU", "NVIDIA GeForce RTX 4070 SUPER 12GB GDDR6X"), ("RAM hệ thống", "16GB DDR5"), ("SSD", "1TB NVMe"), ("Vỏ case", "Cooler Master TD500 Mesh V2"), ("Màn hình đi kèm", "Không")]),

            new(19, "Apple", "OFFPC", "MACMINIM28256", "Apple Mac mini M2 2023 - CPU 8 nhân, GPU 10 nhân, RAM 8GB, SSD 256GB", 12_990_000m, 12,
                "Máy tính để bàn nhỏ gọn dùng Apple M2 và macOS, bộ nhớ hợp nhất 8GB cùng SSD 256GB. Phù hợp làm việc với tài liệu, lập trình ứng dụng trong hệ sinh thái Apple và chỉnh sửa nội dung nhẹ. Có Thunderbolt 4 và HDMI; RAM không thể nâng cấp bằng thanh DIMM. Không kèm màn hình.",
                "https://support.apple.com/en-gb/111837",
                [("CPU", "Apple M2, 8 nhân"), ("GPU", "Apple tích hợp 10 nhân"), ("RAM hợp nhất", "8GB"), ("SSD", "256GB"), ("Kết nối", "Thunderbolt 4, USB-A, HDMI, Gigabit Ethernet"), ("Hệ điều hành", "macOS")]),

            new(20, "HACOM", "OFFPC", "ECONOMY083", "PC HACOM ECONOMY 083 - Ryzen 5 5600GT, RAM 16GB, SSD 512GB", 10_490_000m, 36,
                "Máy tính để bàn nền tảng AMD Ryzen 5 5600GT, RAM 16GB và SSD 512GB, dùng đồ họa Radeon tích hợp. Cấu hình phục vụ Excel, phần mềm kế toán, học trực tuyến và đa nhiệm cơ bản. Bo mạch A520, xuất hình HDMI; không có card đồ họa rời và không kèm màn hình.",
                "https://hacom.vn/pc-hacom-economy-083-pcvp0724",
                [("CPU", "AMD Ryzen 5 5600GT, 6 nhân 12 luồng"), ("Mainboard", "AMD A520"), ("RAM hệ thống", "16GB DDR4"), ("SSD", "512GB"), ("GPU", "AMD Radeon tích hợp"), ("Cổng hình ảnh", "HDMI")]),

            new(21, "Intel", "CPU", "I514600K", "CPU Intel Core i5-14600K - 14 nhân 20 luồng, UHD Graphics 770", 6_490_000m, 36,
                "CPU Intel thế hệ 14 có 6 P-core và 8 E-core, hỗ trợ ép xung trên nền tảng tương thích. Phù hợp biên dịch chương trình, chơi game và biên tập video. Có Intel UHD Graphics 770 và Quick Sync; cần giải pháp tản nhiệt được chọn theo mức tiêu thụ điện thực tế.",
                "https://www.intel.com/content/www/us/en/products/sku/236799/intel-core-i5-processor-14600k-24m-cache-up-to-5-30-ghz/specifications.html",
                [("Socket", "LGA1700"), ("Nhân / luồng", "14 (6P + 8E) / 20"), ("Turbo tối đa", "5.3 GHz"), ("Bộ nhớ đệm L3", "24MB"), ("Đồ họa tích hợp", "Intel UHD Graphics 770"), ("Công suất cơ bản / turbo tối đa", "125W / 181W")]),

            new(22, "GIGABYTE", "MAIN", "B650MDS3H", "Mainboard GIGABYTE B650M DS3H - AM5, DDR5, LAN 2.5GbE", 3_390_000m, 36,
                "Bo mạch Micro-ATX AMD B650 dùng socket AM5 và RAM DDR5. Có bốn khe RAM, hai khe M.2 PCIe 4.0 và mạng LAN 2.5GbE. Phù hợp xây dựng PC Ryzen 7000, bao gồm Ryzen 7 7800X3D theo BIOS hỗ trợ. Không tương thích CPU AM4 hay RAM DDR4.",
                "https://www.gigabyte.com/Motherboard/B650M-DS3H-rev-10-11-12/sp",
                [("Socket", "AMD AM5"), ("Chipset", "AMD B650"), ("Kích thước", "Micro-ATX"), ("Khe RAM", "4 khe DIMM DDR5"), ("Lưu trữ", "2 khe M.2 PCIe 4.0 x4"), ("Mạng dây", "2.5Gb Ethernet")]),

            new(23, "Corsair", "RAM", "CMK32G6000C36", "RAM Corsair Vengeance 32GB (2x16GB) DDR5-6000 CL36", 2_790_000m, 36,
                "Kit RAM desktop DDR5 dung lượng 32GB, gồm hai thanh 16GB với profile Intel XMP 3.0 tốc độ 6000 MT/s. Tản nhiệt nhôm thấp, phù hợp PC đa nhiệm, chạy máy ảo và dựng nội dung. Tốc độ profile phụ thuộc CPU, BIOS và bo mạch chủ tương thích.",
                "https://www.corsair.com/us/en/p/memory/cmk32gx5m2d6000c36/vengeance-32gb-2x16gb-ddr5-dram-6000mt-s-cl36-memory-kit-black-cmk32gx5m2d6000c36",
                [("Mã model", "CMK32GX5M2D6000C36"), ("Dung lượng", "32GB (2 x 16GB)"), ("Chuẩn RAM", "DDR5 UDIMM desktop"), ("Tốc độ", "6000 MT/s"), ("Độ trễ", "CL36"), ("Profile", "Intel XMP 3.0")]),

            new(24, "ASUS", "VGA", "DUAL4060O8G", "VGA ASUS Dual GeForce RTX 4060 OC 8GB GDDR6", 8_390_000m, 36,
                "Card NVIDIA Ada Lovelace hai quạt, 8GB VRAM GDDR6, hỗ trợ DLSS 3 và bộ mã hóa AV1 cho livestream. Phù hợp chơi game ở Full HD và làm nội dung cá nhân. Có 3072 nhân CUDA, dùng một đầu nguồn 8-pin; đây là card rời cho desktop, không phải GPU laptop.",
                "https://www.asus.com/motherboards-components/graphics-cards/dual/dual-rtx4060-o8g/techspec/",
                [("Mã model", "DUAL-RTX4060-O8G"), ("GPU", "NVIDIA GeForce RTX 4060"), ("VRAM", "8GB GDDR6"), ("Bus bộ nhớ", "128-bit"), ("Nhân CUDA", "3072"), ("Nguồn phụ", "1 x 8-pin"), ("Nguồn khuyến nghị", "550W")]),

            new(25, "Western Digital", "DRIVE", "SN7701TB", "SSD WD_BLACK SN770 1TB NVMe PCIe 4.0 M.2 2280", 1_890_000m, 60,
                "SSD NVMe Gen4 dung lượng 1TB, phục vụ ổ hệ điều hành, thư viện game và lưu dự án trên laptop hoặc PC có khe M.2 2280. Phiên bản 1TB đọc tuần tự tối đa 5150MB/s, ghi tối đa 4900MB/s. Không sử dụng cáp SATA.",
                "https://www.sandisk.com/products/ssd/internal-ssd/wd-black-sn770-nvme-ssd?sku=WDS100T3X0E-00B3N0",
                [("Mã model", "WDS100T3X0E"), ("Dung lượng", "1TB"), ("Kích thước", "M.2 2280"), ("Giao tiếp", "PCIe 4.0 x4 NVMe"), ("Đọc tuần tự tối đa", "5150MB/s"), ("Ghi tuần tự tối đa", "4900MB/s")]),

            new(26, "ASUS", "PSU", "TUF850GOLD", "Nguồn ASUS TUF Gaming 850W Gold Full Modular", 3_290_000m, 120,
                "Nguồn ATX 850W chứng nhận 80 PLUS Gold, cáp tháo rời hoàn toàn. Dành cho hệ thống dùng CPU và card đồ họa rời hiệu năng cao, có các cơ chế bảo vệ quá dòng, quá áp và quá nhiệt. Cần kiểm tra đầu cấp điện và công suất khuyến nghị của GPU trước khi lắp.",
                "https://www.asus.com/us/motherboards-components/power-supply-units/tuf-gaming/tuf-gaming-850g/",
                [("Mã model", "TUF-GAMING-850G"), ("Công suất", "850W"), ("Hiệu suất", "80 PLUS Gold"), ("Đi dây", "Full Modular"), ("Điện áp đầu vào", "100-240V AC")]),

            new(27, "ASUS", "GAMLAP", "FA507NULP101W", "Laptop ASUS TUF A15 FA507NU-LP101W - Ryzen 5 7535HS, RTX 4050, RAM 16GB, SSD 512GB", 23_990_000m, 24,
                "Laptop ASUS TUF Gaming A15 15.6 inch Full HD 144Hz, Ryzen 5 7535HS và GeForce RTX 4050 Laptop 6GB. Bộ nhớ DDR5 16GB, SSD 512GB. Phù hợp vừa học lập trình vừa chơi game, xử lý video và sử dụng ứng dụng tăng tốc GPU. GPU laptop có cấu hình công suất khác bản desktop.",
                "https://business.currys.co.uk/catalogue/computing/laptops/windows-laptop/tuf-15-r5-7535hs-16gb-512gb-4050/P307794P",
                [("Mã model", "FA507NU-LP101W"), ("CPU", "AMD Ryzen 5 7535HS, 6 nhân 12 luồng"), ("GPU", "RTX 4050 Laptop 6GB GDDR6"), ("RAM hệ thống", "16GB DDR5"), ("SSD", "512GB NVMe"), ("Màn hình", "15.6 inch, 1920 x 1080, 144Hz"), ("Hệ điều hành", "Windows 11 Home")]),

            new(28, "Lenovo", "GAMLAP", "83GS001RVN", "Laptop Lenovo LOQ 15IAX9 83GS001RVN - i5-12450HX, RTX 3050, RAM 12GB, SSD 512GB", 19_990_000m, 24,
                "Lenovo LOQ 15IAX9 dùng Core i5-12450HX và RTX 3050 Laptop 6GB, màn hình Full HD 144Hz. Phiên bản 83GS001RVN có RAM DDR5 12GB và SSD 512GB, hai khe RAM để nâng cấp. Phù hợp sinh viên học lập trình, thiết kế cơ bản và chơi game ở độ phân giải Full HD.",
                "https://gearvn.com/products/laptop-gaming-lenovo-loq-15iax9-83gs001rvn",
                [("Mã model", "83GS001RVN"), ("CPU", "Intel Core i5-12450HX, 8 nhân 12 luồng"), ("GPU", "RTX 3050 Laptop 6GB GDDR6, TGP 95W"), ("RAM hệ thống", "12GB DDR5-4800 (1 x 12GB)"), ("SSD", "512GB NVMe"), ("Màn hình", "15.6 inch, Full HD IPS, 144Hz"), ("Hệ điều hành", "Windows 11")],
                "https://product.hstatic.net/200000722513/product/khung-laptop-23_fd54463b52cb4cf79732623e567e930e.png"),

            new(29, "Acer", "GAMLAP", "ANV155158AN", "Laptop Acer Nitro V ANV15-51-58AN - i5-13420H, RTX 2050, RAM 8GB, SSD 512GB", 16_990_000m, 12,
                "Laptop Acer Nitro V 15.6 inch IPS Full HD 144Hz, Intel Core i5-13420H và RTX 2050 Laptop 4GB. Phiên bản NH.QNASV.001 trang bị RAM DDR5 8GB, SSD 512GB. Dành cho học tập và game phổ thông; có thể cân nhắc nâng RAM khi chạy nhiều ứng dụng.",
                "https://pico.vn/laptop-acer-n23q22-anv15-51-58an-core-i5-13420h-8gb-512gb-rtx20504gb-156-fhd-144hz-nhqnasv001-ANV155158AN",
                [("Mã model", "ANV15-51-58AN / NH.QNASV.001"), ("CPU", "Intel Core i5-13420H, 8 nhân 12 luồng"), ("GPU", "RTX 2050 Laptop 4GB GDDR6"), ("RAM hệ thống", "8GB DDR5-5200"), ("SSD", "512GB NVMe"), ("Màn hình", "15.6 inch IPS Full HD 144Hz"), ("Hệ điều hành", "Windows 11 Home SL")]),

            new(30, "ASUS", "OFFLAP", "X1504VANJ526W", "Laptop ASUS Vivobook X1504VA-NJ526W - i5-1335U, RAM 16GB, SSD 512GB, 15.6 FHD", 14_990_000m, 24,
                "Laptop Vivobook màn hình 15.6 inch Full HD với Intel Core i5-1335U, bộ nhớ 16GB và SSD 512GB. Dành cho học online, soạn tài liệu, bảng tính và lập trình web. Sử dụng đồ họa tích hợp, phù hợp người cần laptop làm việc hàng ngày thay vì card đồ họa rời.",
                "https://fptshop.com.vn/may-tinh-xach-tay/asus-vivobook-x1504va-nj526w-i5-1335u",
                [("Mã model", "X1504VA-NJ526W"), ("CPU", "Intel Core i5-1335U, 10 nhân 12 luồng"), ("RAM hệ thống", "16GB"), ("SSD", "512GB"), ("Màn hình", "15.6 inch, 1920 x 1080"), ("GPU", "Intel tích hợp"), ("Hệ điều hành", "Windows 11")]),

            new(31, "AMD", "CPU", "RYZEN55600", "CPU AMD Ryzen 5 5600 - 6 nhân 12 luồng, AM4, Zen 3", 2_590_000m, 36,
                "Bộ xử lý desktop AMD Ryzen 5000 dùng socket AM4 và RAM DDR4. Sáu nhân mười hai luồng cho máy học lập trình và chơi game phổ thông, có thể tái sử dụng bo mạch AM4 tương thích sau khi cập nhật BIOS. Không có đồ họa tích hợp nên cần GPU rời.",
                "https://www.amd.com/en/newsroom/press-releases/2022-3-15-amd-launches-the-ultimate-gaming-processor-brings.html",
                [("Socket", "AMD AM4"), ("Kiến trúc", "Zen 3"), ("Nhân / luồng", "6 / 12"), ("Xung cơ bản / boost", "3.5 / 4.4 GHz"), ("TDP", "65W"), ("Đồ họa tích hợp", "Không")]),

            new(32, "Dell", "OFFLAP", "N5I5007W1", "Laptop Dell Inspiron 3530 N5I5007W1 - i5-1335U, RAM 16GB, SSD 512GB, FHD 120Hz", 16_490_000m, 12,
                "Dell Inspiron 3530 màn hình 15.6 inch Full HD 120Hz, Core i5-1335U và RAM 16GB. SSD 512GB cho phần mềm và tài liệu. Cấu hình đồ họa tích hợp phục vụ họp trực tuyến, kế toán, học tập và sử dụng nhiều ứng dụng văn phòng; phiên bản này không có NVIDIA MX550.",
                "https://hacom.vn/laptop-dell-inspiron-3530-n5i5007w1-i5-1335u-16gb-ram-512gb-ssd-15.6-inch-fhd-120hz-win11-officehs21-bac",
                [("Mã model", "N5I5007W1"), ("CPU", "Intel Core i5-1335U"), ("RAM hệ thống", "16GB"), ("SSD", "512GB"), ("Màn hình", "15.6 inch, Full HD, 120Hz"), ("GPU", "Intel tích hợp"), ("Hệ điều hành", "Windows 11")]),

            new(33, "Kingston", "RAM", "KVR32S22S816", "RAM laptop Kingston ValueRAM 16GB DDR4-3200 SO-DIMM CL22", 890_000m, 36,
                "Thanh RAM 16GB DDR4 SO-DIMM 260 chân cho laptop hoặc mini PC có khe nhớ tương thích. Điện áp 1.2V và tốc độ JEDEC 3200 MT/s, dùng nâng cấp bộ nhớ để mở nhiều ứng dụng. Không phải RAM DIMM cho desktop và không dùng cho máy có RAM hàn không có khe nâng cấp.",
                "https://www.kingston.com/dataSheets/KVR32S22S8_16.pdf",
                [("Mã model", "KVR32S22S8/16"), ("Dung lượng", "16GB (1 x 16GB)"), ("Chuẩn RAM", "DDR4 SO-DIMM laptop"), ("Tốc độ", "3200 MT/s"), ("Độ trễ", "CL22"), ("Điện áp", "1.2V"), ("Số chân", "260-pin")]),

            new(34, "GIGABYTE", "VGA", "4070SUPERWF12", "VGA GIGABYTE GeForce RTX 4070 SUPER WINDFORCE OC 12GB GDDR6X", 17_990_000m, 36,
                "Card NVIDIA RTX 4070 SUPER dùng kiến trúc Ada Lovelace, VRAM 12GB GDDR6X và tản nhiệt WINDFORCE ba quạt. Phù hợp chơi game 1440p, tăng tốc dựng phim, render GPU và livestream AV1. Hỗ trợ DLSS 3; cần kiểm tra nguồn và không gian lắp trong case.",
                "https://www.gigabyte.com/Graphics-Card/GV-N407SWF3OC-12GD-rev-10",
                [("Mã model", "GV-N407SWF3OC-12GD"), ("GPU", "NVIDIA GeForce RTX 4070 SUPER"), ("VRAM", "12GB GDDR6X"), ("Bus bộ nhớ", "192-bit"), ("Nhân CUDA", "7168"), ("Tản nhiệt", "WINDFORCE 3 quạt")]),

            new(35, "Kingston", "DRIVE", "NV2500GB", "SSD Kingston NV2 500GB NVMe PCIe 4.0 M.2 2280", 790_000m, 36,
                "SSD NVMe phổ thông dung lượng 500GB cho nâng cấp ổ hệ điều hành và cài ứng dụng. Chuẩn PCIe 4.0 x4 M.2 2280, bản 500GB đọc tuần tự tối đa 3500MB/s và ghi tối đa 2100MB/s. Dành cho laptop hoặc PC có khe NVMe phù hợp, không phải ổ SATA 2.5 inch.",
                "https://www.kingston.com/datasheets/SNV2S_en.pdf",
                [("Mã model", "SNV2S/500G"), ("Dung lượng", "500GB"), ("Kích thước", "M.2 2280"), ("Giao tiếp", "PCIe 4.0 x4 NVMe"), ("Đọc tuần tự tối đa", "3500MB/s"), ("Ghi tuần tự tối đa", "2100MB/s")]),

            new(36, "Apple", "OFFLAP", "MBAIRM28256", "Apple MacBook Air M2 2022 13.6 inch - RAM 8GB, SSD 256GB, GPU 8 nhân", 20_990_000m, 12,
                "MacBook Air 13.6 inch Liquid Retina dùng Apple M2, RAM hợp nhất 8GB và SSD 256GB. Thiết kế không quạt, nặng khoảng 1.24kg, phù hợp mang đi học, viết mã và làm tài liệu trên macOS. Hỗ trợ sạc MagSafe 3; RAM và SSD không nâng cấp bằng linh kiện rời thông thường.",
                "https://support.apple.com/en-us/111867",
                [("CPU", "Apple M2, 8 nhân"), ("GPU", "Apple tích hợp 8 nhân"), ("RAM hợp nhất", "8GB"), ("SSD", "256GB"), ("Màn hình", "13.6 inch Liquid Retina, 2560 x 1664"), ("Khối lượng", "1.24kg"), ("Hệ điều hành", "macOS")]),

            new(37, "Logitech", "KBD", "G413SE", "Bàn phím cơ Logitech G413 SE Full-size - Tactile, keycap PBT", 1_490_000m, 24,
                "Bàn phím cơ có dây layout đầy đủ gồm cụm phím số, switch tactile và keycap PBT. Đèn nền trắng và 6-key rollover phục vụ chơi game và gõ văn bản. Phù hợp người cần cảm giác gõ có khấc và bàn phím làm việc có numpad; không phải bàn phím không dây.",
                "https://www.logitechg.com/en-us/shop/p/g413-se-gaming-keyboard",
                [("Loại switch", "Cơ học tactile"), ("Layout", "Full-size, có numpad"), ("Keycap", "PBT"), ("Đèn nền", "LED trắng"), ("Kết nối", "USB có dây"), ("Rollover", "6-key")]),

            new(38, "HP", "OFFLAP", "240G108F134PA", "Laptop HP 240 G10 8F134PA - i5-1335U, RAM 8GB, SSD 512GB, 14 FHD", 12_490_000m, 12,
                "Laptop HP 240 G10 màn hình 14 inch Full HD, CPU Intel Core i5-1335U, RAM 8GB và SSD 512GB. Thiết kế gọn cho học tập, mang đi làm, xử lý văn bản, bảng tính và họp trực tuyến. Đồ họa tích hợp, không có GPU NVIDIA rời.",
                "https://www.thegioididong.com/laptop/hp-240-g10-i5-8f134pa",
                [("Mã model", "8F134PA"), ("CPU", "Intel Core i5-1335U"), ("RAM hệ thống", "8GB"), ("SSD", "512GB"), ("Màn hình", "14 inch Full HD"), ("GPU", "Intel tích hợp"), ("Hệ điều hành", "Windows 11")]),

            new(39, "Logitech", "MOUSE", "G502HERO", "Chuột Logitech G502 HERO có dây - cảm biến HERO 25K, 11 nút", 990_000m, 24,
                "Chuột gaming có dây dùng cảm biến HERO 25K, DPI điều chỉnh tối đa 25600 và 11 nút lập trình qua Logitech G HUB. Có tạ điều chỉnh trọng lượng, cuộn tốc độ cao và LED LIGHTSYNC RGB. Phù hợp người cần nhiều nút gán thao tác cho game và phần mềm làm việc.",
                "https://www.logitechg.com/en-us/shop/p/g502-hero-gaming-mouse.910-005469",
                [("Cảm biến", "HERO 25K"), ("DPI tối đa", "25600"), ("Nút lập trình", "11"), ("Kết nối", "USB có dây"), ("Đèn", "LIGHTSYNC RGB"), ("Trọng lượng", "Có tạ điều chỉnh")]),

            new(40, "HyperX", "HEADP", "CLOUDIIWIRED", "Tai nghe HyperX Cloud II Wired - driver 53mm, âm thanh vòm 7.1 ảo", 1_890_000m, 24,
                "Tai nghe chụp tai có dây với driver 53mm, đệm memory foam và micro tháo rời. Kết nối 3.5mm hoặc qua hộp âm thanh USB; chế độ 7.1 là âm thanh vòm giả lập. Phục vụ giao tiếp trong game, học trực tuyến và họp; đây là phiên bản có dây, không phải Cloud II Wireless.",
                "https://hyperx.com/products/hyperx-cloud-ii",
                [("Driver", "Dynamic 53mm"), ("Kết nối", "3.5mm; USB qua hộp âm thanh"), ("Âm thanh", "Stereo; virtual 7.1 qua USB"), ("Micro", "Tháo rời, chống ồn"), ("Đệm tai", "Memory foam")]),

            new(41, "AMD", "CPU", "RYZEN75700X", "CPU AMD Ryzen 7 5700X - 8 nhân 16 luồng, AM4, Zen 3", 3_890_000m, 36,
                "CPU Ryzen 5000 tám nhân mười sáu luồng cho người nâng cấp nền tảng AM4 để chạy máy ảo, biên dịch chương trình và xử lý nội dung. Dùng RAM DDR4, TDP 65W. Không có đồ họa tích hợp; phải có GPU rời và tản nhiệt phù hợp.",
                "https://www.amd.com/en/newsroom/press-releases/2022-3-15-amd-launches-the-ultimate-gaming-processor-brings.html",
                [("Socket", "AMD AM4"), ("Kiến trúc", "Zen 3"), ("Nhân / luồng", "8 / 16"), ("Xung cơ bản / boost", "3.4 / 4.6 GHz"), ("TDP", "65W"), ("Đồ họa tích hợp", "Không")]),

            new(42, "AMD", "CPU", "RYZEN77800X3D", "CPU AMD Ryzen 7 7800X3D - 8 nhân 16 luồng, AM5, 3D V-Cache", 9_990_000m, 36,
                "CPU AMD Zen 4 với bộ nhớ đệm 3D V-Cache, socket AM5 và bộ nhớ DDR5. Tám nhân mười sáu luồng, xung boost tối đa 5GHz, dành cho hệ thống ưu tiên hiệu năng chơi game. Có Radeon Graphics tích hợp cho xuất hình cơ bản; không tương thích bo mạch AM4.",
                "https://www.amd.com/en/products/processors/desktops/ryzen/7000-series/amd-ryzen-7-7800x3d.html",
                [("Socket", "AMD AM5"), ("Kiến trúc", "Zen 4, 3D V-Cache"), ("Nhân / luồng", "8 / 16"), ("Boost tối đa", "5.0 GHz"), ("Bộ nhớ đệm L3", "96MB"), ("RAM hỗ trợ", "DDR5"), ("TDP", "120W")]),

            new(43, "Kingston", "RAM", "KF432C16BB8", "RAM Kingston FURY Beast 8GB DDR4-3200 CL16 - 1 thanh desktop", 590_000m, 36,
                "Một thanh RAM desktop DDR4 dung lượng 8GB, tốc độ profile 3200 MT/s và độ trễ CL16. Phù hợp thay thế hoặc bổ sung bộ nhớ cho máy dùng khe DIMM DDR4. Cần kiểm tra tương thích khi ghép với thanh cũ; không phải RAM SO-DIMM laptop.",
                "https://www.kingston.com/datasheets/KF432C16BB_8.pdf",
                [("Mã model", "KF432C16BB/8"), ("Dung lượng", "8GB (1 x 8GB)"), ("Chuẩn RAM", "DDR4 UDIMM desktop"), ("Tốc độ", "3200 MT/s"), ("Độ trễ", "CL16"), ("Điện áp XMP", "1.35V")]),

            new(44, "MSI", "VGA", "4060TIVENTUS16", "VGA MSI RTX 4060 Ti VENTUS 2X BLACK OC 16GB GDDR6", 13_490_000m, 36,
                "Card NVIDIA Ada Lovelace RTX 4060 Ti phiên bản VRAM 16GB, hỗ trợ CUDA, DLSS 3 và AV1. Phù hợp làm việc với dự án đồ họa hoặc mô hình AI cần dung lượng bộ nhớ lớn hơn bản 8GB. Có hai quạt, 4352 nhân CUDA và bus 128-bit; 16GB ở đây là VRAM, không phải RAM hệ thống.",
                "https://www.msi.com/Graphics-Card/GeForce-RTX-4060-Ti-VENTUS-2X-BLACK-16G-OC/Specification",
                [("GPU", "NVIDIA GeForce RTX 4060 Ti"), ("VRAM", "16GB GDDR6"), ("Bus bộ nhớ", "128-bit"), ("Nhân CUDA", "4352"), ("Công suất", "165W"), ("Nguồn phụ", "1 x 8-pin")]),

            new(45, "Seagate", "DRIVE", "ST2000DM008", "HDD Seagate BarraCuda 2TB ST2000DM008 - SATA 3.5 inch, 7200rpm", 1_490_000m, 24,
                "Ổ cứng cơ desktop dung lượng 2TB, SATA 6Gb/s, tốc độ quay 7200 vòng/phút và cache 256MB. Phù hợp lưu ảnh, phim và dữ liệu dung lượng lớn với chi phí theo GB thấp. Dùng công nghệ ghi SMR; không phải SSD NVMe hay ổ chuyên dụng NAS.",
                "https://www.seagate.com/www-content/datasheets/pdfs/3-5-barracudaDS1900-14-2007US-en_US.pdf",
                [("Mã model", "ST2000DM008"), ("Dung lượng", "2TB"), ("Kích thước", "3.5 inch"), ("Giao tiếp", "SATA 6Gb/s"), ("Tốc độ quay", "7200rpm"), ("Bộ nhớ đệm", "256MB"), ("Công nghệ ghi", "SMR")]),

            new(46, "TP-Link", "ROUTER", "ARCHERAX55", "Router TP-Link Archer AX55 AX3000 Wi-Fi 6 băng tần kép", 1_590_000m, 24,
                "Bộ định tuyến Wi-Fi 6 hai băng tần 2.4GHz và 5GHz cho gia đình có nhiều thiết bị học tập, làm việc và giải trí. Tốc độ liên kết lý thuyết cộng dồn AX3000, hỗ trợ OFDMA và cổng mạng Gigabit. Tốc độ Internet thực tế phụ thuộc gói cước, thiết bị và môi trường sóng.",
                "https://www.tp-link.com/vn/home-networking/wifi-router/archer-ax55/",
                [("Chuẩn Wi-Fi", "Wi-Fi 6 / IEEE 802.11ax"), ("Băng tần", "2.4GHz và 5GHz"), ("Tốc độ liên kết lý thuyết", "574Mbps + 2402Mbps"), ("Cổng mạng", "1 WAN Gigabit, 4 LAN Gigabit"), ("USB", "USB 3.0")]),

            new(47, "TP-Link", "SWITCH", "LS1005G", "Switch TP-Link LS1005G - 5 cổng Gigabit Ethernet", 290_000m, 24,
                "Bộ chia mạng Ethernet không quản lý với năm cổng 10/100/1000Mbps, cắm là dùng. Phù hợp mở rộng kết nối dây cho PC, NAS và thiết bị trong phòng làm việc. Thiết kế không quạt, không phát Wi-Fi và không cấp nguồn PoE.",
                "https://www.tp-link.com/us/home-networking/5-port-switch/ls1005g/",
                [("Số cổng", "5 cổng RJ45"), ("Tốc độ mỗi cổng", "10/100/1000Mbps"), ("Quản lý", "Unmanaged, Plug and Play"), ("PoE", "Không"), ("Tản nhiệt", "Không quạt")]),

            new(48, "SanDisk", "USB", "ULTRAFLAIR128", "USB SanDisk Ultra Flair 128GB USB 3.0 - vỏ kim loại", 290_000m, 60,
                "USB flash dung lượng 128GB, đầu USB-A và vỏ kim loại, dùng mang tài liệu học tập, bộ cài hoặc chuyển dữ liệu giữa các máy tính. Chuẩn USB 3.0, tương thích ngược USB 2.0. Không phải SSD di động và không có đầu USB-C trực tiếp.",
                "https://www.sandisk.com/products/usb-flash-drives/sandisk-ultra-flair-usb-3-0?sku=SDCZ73-128G-G46",
                [("Mã model", "SDCZ73-128G-G46"), ("Dung lượng", "128GB"), ("Giao tiếp", "USB 3.0"), ("Đầu cắm", "USB-A"), ("Vỏ", "Kim loại")]),

            new(49, "Anker", "HUB", "ANKER555A8383", "Hub Anker 555 A8383 USB-C 8-in-1 - HDMI 4K60, LAN Gigabit, PD", 1_690_000m, 18,
                "Hub USB-C mở rộng cổng cho laptop, có HDMI, Ethernet Gigabit, USB dữ liệu và đầu đọc SD/microSD. HDMI hỗ trợ tối đa 4K60 khi máy chủ đáp ứng DisplayPort 1.4 qua USB-C; với DP 1.2 có thể chỉ đạt 4K30. Có USB-C Power Delivery passthrough, cần bộ sạc tương thích.",
                "https://www.anker.com/de/products/a8383",
                [("Mã model", "A8383"), ("Kết nối máy chủ", "USB-C"), ("Số cổng mở rộng", "8"), ("HDMI", "Tối đa 4K 60Hz với DP 1.4"), ("Ethernet", "Gigabit"), ("USB dữ liệu", "Tối đa 10Gbps"), ("Thẻ nhớ", "SD và microSD"), ("Nguồn", "USB-C PD passthrough")]),

            new(50, "LG", "MON", "27GP850B", "Màn hình LG UltraGear 27GP850-B - 27 inch QHD Nano IPS 165Hz", 7_490_000m, 24,
                "Màn hình 27 inch độ phân giải 2560 x 1440, tấm nền Nano IPS, tần số quét 165Hz và ép xung tối đa 180Hz qua DisplayPort. Phù hợp chơi game 1440p và làm việc cần không gian hiển thị rộng. Hỗ trợ Adaptive-Sync, G-SYNC Compatible và chân đế điều chỉnh độ cao; không phải màn hình 4K.",
                "https://www.lg.com/de/monitore/gaming/27gp850-b/",
                [("Kích thước", "27 inch"), ("Độ phân giải", "2560 x 1440 (QHD)"), ("Tấm nền", "Nano IPS"), ("Tần số quét", "165Hz; OC 180Hz qua DisplayPort"), ("Đồng bộ hình ảnh", "AMD FreeSync Premium, NVIDIA G-SYNC Compatible"), ("Kết nối", "HDMI, DisplayPort, USB hub")]),
            new(51, "ASUS", "MAIN", "TUF650PLUSW", "Mainboard ASUS TUF GAMING B650-PLUS WIFI - AM5, DDR5, ATX", 5_290_000m, 36,
                "Bo mạch chủ ATX dùng socket AM5, RAM desktop DDR5, hỗ trợ SSD M.2 NVMe và ổ SATA.",
                "https://www.asus.com/motherboards-components/motherboards/tuf-gaming/tuf-gaming-b650-plus-wifi/techspec/",
                [("Socket", "AM5"), ("RamType", "DDR5"), ("MemoryFormFactor", "DIMM"), ("Interface", "M.2 NVMe"), ("Interface", "SATA"), ("FormFactor", "ATX"), ("TDP", "40")]),
            new(52, "DeepCool", "COOL", "AG400", "Tản nhiệt CPU DeepCool AG400 - LGA1700, AM4, AM5", 490_000m, 24,
                "Tản nhiệt khí dùng quạt 120mm, hỗ trợ socket Intel LGA1700 và AMD AM4/AM5. TDP trong PC Builder là điện tiêu thụ của quạt.",
                "https://www.deepcool.com/products/Cooling/cpuaircoolers/GAMMAXX-AG400-Single-Tower-CPU-Cooler-1700-AM5/2022/15898.shtml",
                [("Socket", "LGA1700"), ("Socket", "LGA1200"), ("Socket", "LGA1151"), ("Socket", "LGA1150"), ("Socket", "LGA1155"), ("Socket", "AM4"), ("Socket", "AM5"), ("TDP", "2.04")]),
            new(53, "Noctua", "COOL", "NHL9I17XX", "Tản nhiệt CPU Noctua NH-L9i-17xx - low-profile, LGA1700", 1_290_000m, 72,
                "Tản nhiệt low-profile 37mm cho nền tảng Intel LGA1700. Cần đối chiếu khả năng giải nhiệt với CPU thực tế trước khi sử dụng.",
                "https://www.noctua.at/en/products/nh-l9i-17xx",
                [("Socket", "LGA1700"), ("TDP", "2.52")]),
            new(54, "Jonsbo", "CASE", "C6BLACK", "Vỏ case Jonsbo C6 Black - Micro-ATX, Mini-ITX", 790_000m, 12,
                "Case nhỏ hỗ trợ Micro-ATX/Mini-ITX. Builder sử dụng giới hạn VGA bảo thủ 200mm trong khoảng 200–255mm tùy vị trí lắp; cần kiểm tra thêm chiều cao tản nhiệt.",
                "https://www.jonsbo.com/products/C6.html",
                [("FormFactor", "Micro-ATX"), ("FormFactor", "Mini-ITX"), ("MaxVgaLength", "200"), ("TDP", "0")])
        ];

        // PC Builder uses canonical values; one row per supported socket/interface/form factor.
        // W and mm are implicit for the numeric specs below.
        private static readonly Dictionary<string, (string Name, string Value)[]> BuilderSpecifications = new()
        {
            ["I512400F"] = [("Socket", "LGA1700"), ("TDP", "65")],
            ["I513400F"] = [("Socket", "LGA1700"), ("TDP", "65")],
            ["I514600K"] = [("Socket", "LGA1700"), ("TDP", "125")],
            ["RYZEN55600"] = [("Socket", "AM4"), ("TDP", "65")],
            ["RYZEN75700X"] = [("Socket", "AM4"), ("TDP", "65")],
            ["RYZEN77800X3D"] = [("Socket", "AM5"), ("TDP", "120")],
            ["B760MAD4"] = [("Socket", "LGA1700"), ("RamType", "DDR4"), ("MemoryFormFactor", "DIMM"), ("Interface", "M.2 NVMe"), ("Interface", "SATA"), ("FormFactor", "Micro-ATX"), ("TDP", "35")],
            ["B550MPROVDHW"] = [("Socket", "AM4"), ("RamType", "DDR4"), ("MemoryFormFactor", "DIMM"), ("Interface", "M.2 NVMe"), ("Interface", "SATA"), ("FormFactor", "Micro-ATX"), ("TDP", "35")],
            ["B650MDS3H"] = [("Socket", "AM5"), ("RamType", "DDR5"), ("MemoryFormFactor", "DIMM"), ("Interface", "M.2 NVMe"), ("Interface", "SATA"), ("FormFactor", "Micro-ATX"), ("TDP", "35")],
            ["KF432C16BBK216"] = [("RamType", "DDR4"), ("MemoryFormFactor", "DIMM"), ("TDP", "6")],
            ["KF556C36BBEK232"] = [("RamType", "DDR5"), ("MemoryFormFactor", "DIMM"), ("TDP", "8")],
            ["CMK32G6000C36"] = [("RamType", "DDR5"), ("MemoryFormFactor", "DIMM"), ("TDP", "8")],
            ["KF432C16BB8"] = [("RamType", "DDR4"), ("MemoryFormFactor", "DIMM"), ("TDP", "3")],
            ["KVR32S22S816"] = [("RamType", "DDR4"), ("MemoryFormFactor", "SO-DIMM"), ("TDP", "3")],
            ["TUF3090O24G"] = [("VgaLength", "299.9"), ("TDP", "350")],
            ["RTX3060VENTUS12"] = [("VgaLength", "235"), ("TDP", "170")],
            ["DUAL4060O8G"] = [("VgaLength", "227.2"), ("TDP", "115")],
            ["4070SUPERWF12"] = [("VgaLength", "261"), ("TDP", "220")],
            ["4060TIVENTUS16"] = [("VgaLength", "199"), ("TDP", "165")],
            ["990PRO2TB"] = [("Interface", "M.2 NVMe"), ("TDP", "8")],
            ["970EVOPLUS1TB"] = [("Interface", "M.2 NVMe"), ("TDP", "6")],
            ["SN7701TB"] = [("Interface", "M.2 NVMe"), ("TDP", "6")],
            ["NV2500GB"] = [("Interface", "M.2 NVMe"), ("TDP", "6")],
            ["ST2000DM008"] = [("Interface", "SATA"), ("TDP", "8")],
            ["RM750E2023"] = [("Watt", "750")], ["MWE650BRONZEV2"] = [("Watt", "650")], ["TUF850GOLD"] = [("Watt", "850")],
            ["4000DAIRFLOW"] = [("FormFactor", "ATX"), ("FormFactor", "Micro-ATX"), ("FormFactor", "Mini-ITX"), ("MaxVgaLength", "360"), ("TDP", "0")],
            ["TD500MESHV2"] = [("FormFactor", "ATX"), ("FormFactor", "Micro-ATX"), ("FormFactor", "Mini-ITX"), ("MaxVgaLength", "410"), ("TDP", "0")]
        };

        internal static IEnumerable<(string Name, string Value)> GetSpecifications(ProductSeed seed)
        {
            var builderSpecs = BuilderSpecifications.GetValueOrDefault(seed.ProductCode) ?? seed.Specifications;
            return seed.Specifications.Where(spec => !builderSpecs.Any(item => item.Name == spec.Name))
                .Concat(builderSpecs).Distinct();
        }
    }
}
