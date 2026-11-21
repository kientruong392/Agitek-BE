using AgitekBackend.Common;
using AgitekBackend.Common.Exceptions;
using AgitekBackend.Data;
using AgitekBackend.DTOs.Reports;
using AgitekBackend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AgitekBackend.Services
{
    public class ReportService : IReportService
    {
        private readonly AppDbContext _context;
        private readonly IAuditLogService _auditLogService;

        public ReportService(AppDbContext context, IAuditLogService auditLogService)
        {
            _context = context;
            _auditLogService = auditLogService;
        }

        public async Task<ReportDTO> GetAsync(ReportQueryDTO query)
        {
            var from = (query.From ?? new DateTime(DateTime.Today.Year, 1, 1)).Date;
            var to = (query.To ?? DateTime.Today).Date;
            if (!Enum.IsDefined(query.Interval) || from.Year < 1900 || from > to || to > DateTime.MaxValue.AddDays(-1))
                throw new BadRequestException("Invalid report interval or date range");

            var periodLimit = query.Interval switch
            {
                ReportInterval.Day => 366,
                ReportInterval.Month => 1830,
                ReportInterval.Quarter => 3650,
                _ => 7300
            };
            if ((to - from).TotalDays > periodLimit)
                throw new BadRequestException("Date range is too large for the selected interval");

            var end = to.AddDays(1);
            var previousFrom = from - (end - from);
            var report = new ReportDTO { From = from, To = to, Interval = query.Interval };

            var payments = await _context.Payments.AsNoTracking()
                .Where(payment => (payment.Status == PaymentStatus.Success || payment.Status == PaymentStatus.Refunded) &&
                    (payment.PaidAt ?? payment.CreatedAt) >= previousFrom &&
                    (payment.PaidAt ?? payment.CreatedAt) < end)
                .Select(payment => new { payment.OrderId, payment.Amount, payment.Status, Date = payment.PaidAt ?? payment.CreatedAt })
                .ToListAsync();

            var products = await _context.Products.AsNoTracking()
                .Select(product => new ProductReportDTO
                {
                    ProductId = product.Id,
                    ProductName = product.ProductName,
                    SKU = product.SKU,
                    CategoryName = product.Category != null ? product.Category.CategoryName : string.Empty,
                    TotalReceived = product.InitialQuantity,
                    CurrentStock = product.StockQuantity
                })
                .ToListAsync();
            var productMap = products.ToDictionary(product => product.ProductId);

            var periodMap = new Dictionary<DateTime, ReportPeriodDTO>();
            for (var cursor = BucketStart(from, query.Interval); cursor < end; cursor = NextBucket(cursor, query.Interval))
            {
                periodMap[cursor] = new ReportPeriodDTO { Start = cursor };
            }

            var currentPayments = payments.Where(payment => payment.Date >= from).ToList();
            foreach (var payment in currentPayments)
            {
                var period = periodMap[BucketStart(payment.Date, query.Interval)];
                if (payment.Status == PaymentStatus.Success)
                {
                    report.GrossRevenue += payment.Amount;
                    period.Revenue += payment.Amount;
                }
                else
                {
                    report.RefundAmount += payment.Amount;
                    period.Refunds += payment.Amount;
                }
            }

            report.NetRevenue = report.GrossRevenue - report.RefundAmount;
            report.PreviousNetRevenue = payments
                .Where(payment => payment.Date < from)
                .Sum(payment => payment.Status == PaymentStatus.Success ? payment.Amount : -payment.Amount);
            report.GrowthPercent = report.PreviousNetRevenue == 0
                ? null
                : Math.Round((report.NetRevenue - report.PreviousNetRevenue) / Math.Abs(report.PreviousNetRevenue) * 100m, 2);

            var paidOrderIds = currentPayments.Where(payment => payment.Status == PaymentStatus.Success)
                .Select(payment => payment.OrderId).Distinct().ToArray();
            report.PaidOrderCount = paidOrderIds.Length;
            if (paidOrderIds.Length > 0)
            {
                var paidDetails = await _context.OrderDetails.AsNoTracking()
                    .Where(detail => paidOrderIds.Contains(detail.OrderId))
                    .Select(detail => new
                    {
                        detail.ProductId,
                        detail.Quantity,
                        detail.TotalPrice,
                        detail.OrderId,
                        detail.Order.SubtotalPrice,
                        detail.Order.DiscountAmount
                    })
                    .ToListAsync();
                foreach (var detail in paidDetails)
                {
                    if (!productMap.TryGetValue(detail.ProductId, out var product)) continue;
                    product.UnitsSold += detail.Quantity;
                    var discountFactor = detail.SubtotalPrice > 0
                        ? Math.Clamp((detail.SubtotalPrice - detail.DiscountAmount) / detail.SubtotalPrice, 0m, 1m)
                        : 1m;
                    product.Revenue += Math.Round(detail.TotalPrice * discountFactor, 2);
                }
            }

            var approvedRefunds = await _context.Refunds.AsNoTracking()
                .Where(refund => refund.Status == RefundStatus.Approved && refund.UpdatedAt >= from && refund.UpdatedAt < end)
                .Select(refund => new { refund.ProductId, refund.Quantity, refund.Amount })
                .ToListAsync();
            foreach (var refund in approvedRefunds)
            {
                if (!productMap.TryGetValue(refund.ProductId, out var product)) continue;
                product.UnitsSold -= refund.Quantity;
                product.Revenue -= refund.Amount;
            }

            // Stock is deducted when an order is created. Expired/cancelled orders are excluded.
            var dispatches = await _context.OrderDetails.AsNoTracking()
                .Where(detail => detail.Order.CreatedAt >= from && detail.Order.CreatedAt < end &&
                    detail.Order.Status != OrderStatus.Cancelled)
                .Select(detail => new { detail.ProductId, detail.Quantity, detail.Order.CreatedAt })
                .ToListAsync();
            foreach (var dispatch in dispatches)
            {
                if (productMap.TryGetValue(dispatch.ProductId, out var product))
                    product.DispatchedInPeriod += dispatch.Quantity;
                periodMap[BucketStart(dispatch.CreatedAt, query.Interval)].StockOut += dispatch.Quantity;
                report.StockOut += dispatch.Quantity;
            }

            // Existing AuditLog records stock receipts from the moment this feature is deployed.
            var receipts = await _auditLogService.GetStockReceiptsAsync(from, end);
            foreach (var receipt in receipts)
            {
                if (!productMap.TryGetValue(receipt.ProductId, out var product)) continue;
                product.ReceivedInPeriod += receipt.Quantity;
                periodMap[BucketStart(receipt.Timestamp, query.Interval)].StockIn += receipt.Quantity;
                report.StockIn += receipt.Quantity;
            }

            report.Periods = periodMap.Values.OrderBy(period => period.Start).ToList();
            foreach (var period in report.Periods)
                period.NetRevenue = period.Revenue - period.Refunds;
            report.Products = products;
            return report;
        }

        private static DateTime BucketStart(DateTime date, ReportInterval interval)
        {
            return interval switch
            {
                ReportInterval.Day => date.Date,
                ReportInterval.Month => new DateTime(date.Year, date.Month, 1),
                ReportInterval.Quarter => new DateTime(date.Year, ((date.Month - 1) / 3) * 3 + 1, 1),
                _ => new DateTime(date.Year, 1, 1)
            };
        }

        private static DateTime NextBucket(DateTime date, ReportInterval interval)
        {
            return interval switch
            {
                ReportInterval.Day => date.AddDays(1),
                ReportInterval.Month => date.AddMonths(1),
                ReportInterval.Quarter => date.AddMonths(3),
                _ => date.AddYears(1)
            };
        }
    }
}
