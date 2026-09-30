using System;
using System.Collections.Generic;
using AgitekBackend.Common;

namespace AgitekBackend.Tests.Builders
{
    public sealed class TestClock
    {
        public DateTime UtcNow { get; private set; }
        public TestClock(DateTime initialUtc) => UtcNow = DateTime.SpecifyKind(initialUtc, DateTimeKind.Utc);
        public void Advance(TimeSpan delta) => UtcNow = UtcNow.Add(delta);
    }

    public sealed class TestContextFixture
    {
        public string ModuleName { get; }
        public List<string> Steps { get; } = new();
        public Dictionary<string, string> RegisteredServices { get; } = new();

        public TestContextFixture(string moduleName) => ModuleName = moduleName;
        public void RegisterService(string name, string revision) => RegisteredServices[name] = revision;
        public void RecordStep(string phase, string scenario, string detail)
        {
            if (string.IsNullOrWhiteSpace(phase)) throw new ArgumentException("Phase is required");
            Steps.Add($"{ModuleName}|{phase}:{scenario}:{detail}");
        }

        public bool HasPhase(string phase) => Steps.Exists(item => item.Contains($"|{phase}:"));
    }

    public sealed class DomainAssertionHelper
    {
        public void EnsureValidState(string moduleKey, string status)
        {
            if (string.IsNullOrWhiteSpace(moduleKey)) throw new ArgumentException("Module key is required.");
            if (string.IsNullOrWhiteSpace(status)) throw new ArgumentException("Status is required.");
        }
    }

    public static class AgitekSeedFactory
    {
        public static object CreateUserSeed(string role = "Customer", bool hasPassword = true, bool isVerified = true) =>
            new { Id = 1, Username = "agitek_user", Email = "user@agitek.vn", Role = role, HasPassword = hasPassword, IsVerified = isVerified, IsActive = true };

        public static object CreateProductSeed(string sku = "ASU-VGA-4070", decimal price = 15990000m, int stock = 12) =>
            new { Id = 10, SKU = sku, ProductName = "ASUS Dual RTX 4070 Super", OriginalPrice = price, DiscountPrice = price - 500000m, StockQuantity = stock, IsPrimaryImage = true };

        public static object CreateVoucherSeed(string code = "AGITEK10", decimal value = 10m, decimal minOrder = 1000000m) =>
            new { Id = 5, Code = code, DiscountValue = value, MinOrderValue = minOrder, Quantity = 50, IsActive = true };

        public static object CreateOrderSeed(string tracking = "AGT261016ABC123456", decimal total = 16020000m, string method = "Bank") =>
            new { Id = 21, TrackingNumber = tracking, TotalPrice = total, ShippingFee = 30000m, PaymentMethod = method, Status = "Ordered" };

        public static object CreateLocationSeed(string code = "HN01", double lat = 21.028511, double lng = 105.854167) =>
            new { Id = 3, LocationCode = code, LocationName = "Kho Ha Noi", Latitude = lat, Longitude = lng, IsWarehouse = true };

        public static object CreateWarrantySeed(string code = "WRT-2026-001", int quantity = 1) =>
            new { Id = 7, Code = code, OrderId = 21, ProductId = 10, Quantity = quantity, Status = "Pending" };

        public static object CreateReviewSeed(int rating = 5, string comment = "San pham rat tot") =>
            new { Id = 9, OrderId = 21, ProductId = 10, Rating = rating, Comment = comment };

        public static object CreatePcBuildSeed(string name = "Gaming 2K Build", int partsCount = 8) =>
            new { Id = 4, Name = name, PartsCount = partsCount, TotalTdpWatts = 485, RecommendedPsuWatts = 650 };
    }
}

namespace AgitekBackend.Tests.Builders
{
    public static class SePayTestPayloadFactory
    {
        public static object CreateWebhookPayload(string trackingNumber, decimal amount, string transferType = "in") =>
            new
            {
                transferType,
                content = $"Thanh toan don {trackingNumber}",
                code = trackingNumber,
                transferAmount = amount,
                referenceNumber = $"FT{DateTime.UtcNow:yyyyMMddHHmmss}"
            };

        public static object CreateReportPeriodSeed(string label, decimal gross, decimal refunds, int stockIn, int stockOut) =>
            new { Label = label, GrossRevenue = gross, RefundAmount = refunds, NetRevenue = gross - refunds, StockIn = stockIn, StockOut = stockOut };
    }
}
