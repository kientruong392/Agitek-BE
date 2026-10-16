using AgitekBackend.Common;
using AgitekBackend.Data;
using AgitekBackend.Services.Interfaces;
using AgitekBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace AgitekBackend.Services
{
    public class PaymentExpiryBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PaymentExpiryBackgroundService> _logger;

        public PaymentExpiryBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<PaymentExpiryBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ExpirePaymentsAsync(stoppingToken);
                    await timer.WaitForNextTickAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Failed to expire overdue bank payments");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }
        }

        private async Task ExpirePaymentsAsync(CancellationToken cancellationToken)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await using var transaction = await context.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable,
                cancellationToken);

            var now = DateTime.UtcNow;
            var payments = await context.Payments
                .Include(payment => payment.Order)
                    .ThenInclude(order => order.OrderDetails)
                .Where(payment => payment.PaymentMethod == PaymentMethod.Bank &&
                    payment.Status == PaymentStatus.Pending &&
                    payment.ExpiresAt.HasValue && payment.ExpiresAt.Value < now)
                .ToListAsync(cancellationToken);

            if (payments.Count == 0)
            {
                await transaction.CommitAsync(cancellationToken);
                return;
            }

            var orderDetails = payments
                .GroupBy(payment => payment.OrderId)
                .SelectMany(group => group.First().Order.OrderDetails)
                .ToList();
            var productIds = orderDetails.Select(detail => detail.ProductId).Distinct().ToList();
            var products = await context.Products
                .Where(product => productIds.Contains(product.Id))
                .ToDictionaryAsync(product => product.Id, cancellationToken);
            foreach (var detail in orderDetails)
            {
                if (!products.TryGetValue(detail.ProductId, out var product))
                    continue;

                product.StockQuantity += detail.Quantity;
                product.SoldQuantity = Math.Max(0, product.SoldQuantity - detail.Quantity);
                product.UpdatedAt = now;
                product.Status = GetProductStatus(product);
            }

            foreach (var payment in payments)
            {
                payment.Status = PaymentStatus.Expired;
                payment.ExpiresAt = null;
                payment.Order.Status = OrderStatus.Cancelled;
                payment.Order.UpdatedAt = now;
            }

            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }

        private static ProductStatus GetProductStatus(Product product)
        {
            if (product.Status != ProductStatus.InStock &&
                product.Status != ProductStatus.LowStock &&
                product.Status != ProductStatus.OutOfStock)
                return product.Status;

            if (product.StockQuantity <= 0)
                return ProductStatus.OutOfStock;

            if (product.InitialQuantity > 0 &&
                (decimal)product.StockQuantity * 100m <= (decimal)product.InitialQuantity * 20m)
                return ProductStatus.LowStock;

            return ProductStatus.InStock;
        }
    }
}
