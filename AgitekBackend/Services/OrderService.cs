using AgitekBackend.Common;
using AgitekBackend.Common.Exceptions;
using AgitekBackend.Data;
using AgitekBackend.DTOs.Auths;
using AgitekBackend.DTOs.Medias;
using AgitekBackend.DTOs.OrderDetails;
using AgitekBackend.DTOs.Orders;
using AgitekBackend.DTOs.Payments;
using AgitekBackend.DTOs.Vouchers;
using AgitekBackend.Models;
using AgitekBackend.Services.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AgitekBackend.Services
{
    public class OrderService : IOrderService
    {
        private const double MaxOrderAmount = (double)decimal.MaxValue;

        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IPaymentService _paymentService;
        private readonly IMediaService _mediaService;
        private readonly IVoucherService _voucherService;
        private readonly IAddressService _addressService;
        private readonly IAuthService _authService;

        public OrderService(
            AppDbContext context,
            IMapper mapper,
            IPaymentService paymentService,
            IMediaService mediaService,
            IVoucherService voucherService,
            IAddressService addressService,
            IAuthService authService)
        {
            _context = context;
            _mapper = mapper;
            _paymentService = paymentService;
            _mediaService = mediaService;
            _voucherService = voucherService;
            _addressService = addressService;
            _authService = authService;
        }

        public async Task<IEnumerable<OrderDTO>> GetAllAsync(int userId, bool assignedToDelivery = false)
        {
            IQueryable<Order> query = _context.Orders
                .AsNoTracking()
                .Include(o => o.Payments)
                .Include(o => o.OrderDetails)
                    .ThenInclude(detail => detail.Product)
                        .ThenInclude(product => product!.Images)
                .Include(o => o.OrderDetails)
                    .ThenInclude(detail => detail.Reviews)
                .Include(o => o.OrderTrackingLogs)
                .Include(o => o.Medias)
                .Include(o => o.FromLocation)
                .Include(o => o.ToLocation);

            if (assignedToDelivery)
                query = query.Where(o => o.DeliveredByUserId == userId);
            else
                query = query.Where(o => o.UserId == userId);

            var orders = await query.ToListAsync();
            return _mapper.Map<IEnumerable<OrderDTO>>(orders);
        }

        public async Task<OrderDTO> GetByIdAsync(int id)
        {
            var order = await _context.Orders
                .AsNoTracking()
                .Include(o => o.Payments)
                .Include(o => o.OrderDetails)
                    .ThenInclude(detail => detail.Product)
                        .ThenInclude(product => product!.Images)
                .Include(o => o.OrderDetails)
                    .ThenInclude(detail => detail.Reviews)
                .Include(o => o.OrderTrackingLogs)
                .Include(o => o.FromLocation)
                .Include(o => o.ToLocation)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) throw new NotFoundException("Order not found");
            var dto = _mapper.Map<OrderDTO>(order);
            dto.PaymentId = order.Payments.OrderByDescending(payment => payment.Id).Select(payment => (int?)payment.Id).FirstOrDefault();
            return dto;
        }

        public async Task<OrderDTO> CreateAsync(CreateOrderDTO dto, int currentUserId)
        {
            ValidatePaymentMethod(dto.PaymentMethod);
            await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            try
            {
                var address = await _addressService.GetByIdAsync(dto.AddressId);
                if (address.UserId != currentUserId)
                    throw new BadRequestException("Address does not belong to the current user");

                var addressSnapshot = FormatAddress(address.ReceiverName, address.PhoneNumber, address.AddressLine);
                var createdOrder = await CreateOrderCoreAsync(dto, currentUserId, addressSnapshot, address.Id);
                await transaction.CommitAsync();
                return await GetByIdAsync(createdOrder.Id);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private static void ValidatePaymentMethod(PaymentMethod paymentMethod)
        {
            if (paymentMethod != PaymentMethod.COD && paymentMethod != PaymentMethod.Bank)
                throw new BadRequestException("Invalid payment method");
        }

        public async Task<OrderDTO> CreateGuestOrderAsync(CreateGuestOrderDTO dto)
        {
            ValidatePaymentMethod(dto.PaymentMethod);
            await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            try
            {
                var guestUser = await _authService.CreateGuestUserAsync(dto.CreateGuest);
                var addressSnapshot = FormatAddress(
                    dto.CreateGuest.FullName,
                    dto.CreateGuest.PhoneNumber,
                    dto.CreateGuest.Address);
                var orderDetails = await BuildOrderDetailsAsync(dto.OrderDetails);
                var subtotal = orderDetails.Sum(detail => detail.TotalPrice);
                var voucher = await GetVoucherApplicationAsync(dto.VoucherId, dto.VoucherCode, subtotal, guestUser.Id);
                var order = await BuildOrderAsync(dto, guestUser.Id, addressSnapshot, orderDetails, subtotal, voucher);

                SetOrderStatus(order, OrderStatus.Ordered);
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                await _paymentService.CreateAsync(new CreatePaymentDTO
                {
                    OrderId = order.Id,
                    PaymentMethod = dto.PaymentMethod,
                    TransactionId = null
                });

                if (voucher != null)
                    await _voucherService.DecreaseQuantityAsync(voucher.VoucherId);

                await DecreaseProductStockAsync(orderDetails);
                await transaction.CommitAsync();
                return await GetByIdAsync(order.Id);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task<Order> CreateOrderCoreAsync(CreateOrderDTO dto, int userId, string addressSnapshot, int addressId)
        {
            var orderDetails = await BuildOrderDetailsAsync(dto.OrderDetails);
            var subtotal = orderDetails.Sum(detail => detail.TotalPrice);
            var voucher = await GetVoucherApplicationAsync(dto.VoucherId, dto.VoucherCode, subtotal, userId);
            var order = await BuildOrderAsync(dto, userId, addressSnapshot, addressId, orderDetails, subtotal, voucher);

            SetOrderStatus(order, OrderStatus.Ordered);
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            await _paymentService.CreateAsync(new CreatePaymentDTO
            {
                OrderId = order.Id,
                PaymentMethod = dto.PaymentMethod,
                TransactionId = null
            });

            if (voucher != null)
                await _voucherService.DecreaseQuantityAsync(voucher.VoucherId);

            await DecreaseProductStockAsync(orderDetails);
            return order;
        }

        private async Task<List<OrderDetail>> BuildOrderDetailsAsync(
            IEnumerable<CreateOrderDetailDTO> detailDtos)
        {
            var quantities = detailDtos
                .GroupBy(detail => detail.ProductId)
                .Select(group => new
                {
                    ProductId = group.Key,
                    Quantity = group.Sum(detail => (long)detail.Quantity),
                    Source = group.First()
                })
                .ToList();
            if (quantities.Any(item => item.Quantity > int.MaxValue))
                throw new BadRequestException("Requested product quantity is too large");

            var productIds = quantities.Select(item => item.ProductId).ToList();
            var products = await _context.Products
                .AsNoTracking()
                .Where(product => productIds.Contains(product.Id) && product.DeletedAt == null)
                .ToDictionaryAsync(product => product.Id);

            var orderDetails = new List<OrderDetail>(quantities.Count);
            decimal subtotal = 0m;
            foreach (var requested in quantities.OrderBy(item => item.ProductId))
            {
                if (!products.TryGetValue(requested.ProductId, out var product) ||
                    (product.Status != ProductStatus.InStock && product.Status != ProductStatus.LowStock) ||
                    product.StockQuantity < requested.Quantity)
                    throw new BadRequestException($"Product {requested.ProductId} is unavailable or has insufficient stock");

                if (product.OriginalPrice < 0 || product.DiscountPrice < 0 ||
                    product.DiscountPrice > product.OriginalPrice)
                    throw new BadRequestException($"Product {requested.ProductId} has an invalid price");

                var quantity = (int)requested.Quantity;
                var detail = _mapper.Map<OrderDetail>(requested.Source);
                detail.ProductId = product.Id;
                detail.Quantity = quantity;
                detail.UnitPrice = product.DiscountPrice.HasValue && product.DiscountPrice.Value < product.OriginalPrice
                    ? product.DiscountPrice.Value
                    : product.OriginalPrice;
                detail.TotalPrice = detail.UnitPrice * quantity;

                subtotal += detail.TotalPrice;
                if ((double)subtotal > MaxOrderAmount)
                    throw new BadRequestException("Order subtotal exceeds the supported limit");
                orderDetails.Add(detail);
            }

            return orderDetails;
        }

        private async Task<VoucherApplicationDTO?> GetVoucherApplicationAsync(
            int? voucherId,
            string? voucherCode,
            decimal subtotal,
            int currentUserId)
        {
            if (!voucherId.HasValue && string.IsNullOrWhiteSpace(voucherCode))
                return null;

            if (voucherId.HasValue)
            {
                var voucher = await _context.Vouchers
                    .AsNoTracking()
                    .Where(item => item.Id == voucherId.Value)
                    .Select(item => item.Code)
                    .FirstOrDefaultAsync();
                if (voucher == null)
                    throw new BadRequestException("Voucher is invalid or unavailable");
                voucherCode = voucher;
            }

            return await _voucherService.ValidateForOrderAsync(voucherCode!, subtotal, currentUserId);
        }

        private async Task<Order> BuildOrderAsync(
            CreateOrderDTO dto,
            int currentUserId,
            string addressSnapshot,
            int? addressId,
            List<OrderDetail> orderDetails,
            decimal subtotal,
            VoucherApplicationDTO? voucher)
        {
            return await BuildOrderAsync(
                _mapper.Map<Order>(dto),
                dto.ShippingType,
                currentUserId,
                addressSnapshot,
                addressId,
                orderDetails,
                subtotal,
                voucher);
        }

        private async Task<Order> BuildOrderAsync(
            CreateGuestOrderDTO dto,
            int currentUserId,
            string addressSnapshot,
            List<OrderDetail> orderDetails,
            decimal subtotal,
            VoucherApplicationDTO? voucher)
        {
            return await BuildOrderAsync(
                _mapper.Map<Order>(dto),
                dto.ShippingType,
                currentUserId,
                addressSnapshot,
                null,
                orderDetails,
                subtotal,
                voucher);
        }

        private static string FormatAddress(string receiverName, string phoneNumber, string addressLine)
        {
            return $"{receiverName.Trim()}, {phoneNumber.Trim()}, {addressLine.Trim()}";
        }

        private async Task<Order> BuildOrderAsync(
            Order order,
            ShippingType shippingType,
            int currentUserId,
            string addressSnapshot,
            int? addressId,
            List<OrderDetail> orderDetails,
            decimal subtotal,
            VoucherApplicationDTO? voucher)
        {
            var shippingFee = shippingType == ShippingType.Express ? 50000m : 30000m;
            var discountAmount = voucher?.DiscountAmount ?? 0m;
            var total = subtotal + shippingFee - discountAmount;
            if (total < 0 || (double)total > MaxOrderAmount)
                throw new BadRequestException("Order total is outside the supported range");

            order.TransactionId = null;
            order.Voucher = voucher == null ? null : new Voucher { Id = voucher.VoucherId };
            order.TrackingNumber = await GenerateTrackingNumberAsync();
            order.UserId = currentUserId;
            order.CreatedAt = DateTime.UtcNow;
            order.AddressId = addressId;
            order.Address = addressSnapshot;
            order.ShippingFee = shippingFee;
            order.VoucherId = voucher?.VoucherId;
            order.DiscountAmount = discountAmount;
            order.SubtotalPrice = subtotal;
            order.TotalPrice = Math.Max(0m, total);
            order.OrderDetails = orderDetails;
            order.Payments = [];
            return order;
        }

        private async Task DecreaseProductStockAsync(IEnumerable<OrderDetail> orderDetails)
        {
            var details = orderDetails.ToList();
            var productIds = details.Select(detail => detail.ProductId).ToList();
            var products = await _context.Products
                .Where(product => productIds.Contains(product.Id))
                .ToDictionaryAsync(product => product.Id);
            var now = DateTime.UtcNow;

            foreach (var detail in details)
            {
                if (!products.TryGetValue(detail.ProductId, out var product) ||
                    product.StockQuantity < detail.Quantity)
                    throw new BadRequestException($"Product {detail.ProductId} is unavailable or has insufficient stock");

                product.StockQuantity -= detail.Quantity;
                product.SoldQuantity += detail.Quantity;
                product.UpdatedAt = now;
                product.Status = product.StockQuantity == 0
                    ? ProductStatus.OutOfStock
                    : product.Status == ProductStatus.PreOrder
                        ? ProductStatus.PreOrder
                        : product.InitialQuantity > 0 &&
                          (decimal)product.StockQuantity * 100m <= (decimal)product.InitialQuantity * 20m
                            ? ProductStatus.LowStock
                            : ProductStatus.InStock;
            }

            await _context.SaveChangesAsync();
        }


        public async Task<OrderDTO> UpdateAsync(int id, UpdateOrderDTO dto)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) throw new NotFoundException("Order not found");
            if (order.Status != OrderStatus.Ordered)
                throw new BadRequestException("Order can only be updated while in Ordered status");
            if (dto.Note?.Length > 255)
                throw new BadRequestException("Order note cannot exceed 255 characters");
            if (dto.AddressId.HasValue)
            {
                var address = await _context.Addresses.FirstOrDefaultAsync(item => item.Id == dto.AddressId.Value);
                if (address == null || address.UserId != order.UserId)
                    throw new BadRequestException("Address does not belong to the order owner");
                order.AddressId = address.Id;
                order.Address = $"{address.ReceiverName}, {address.PhoneNumber}, {address.AddressLine}";
            }
            if (dto.ShippingType.HasValue && !Enum.IsDefined(dto.ShippingType.Value))
                throw new BadRequestException("Invalid shipping type");

            _mapper.Map(dto, order);
            if (dto.ShippingType.HasValue)
            {
                order.ShippingFee = dto.ShippingType.Value == ShippingType.Express ? 50000m : 30000m;
                order.TotalPrice = Math.Max(0m, order.SubtotalPrice + order.ShippingFee - order.DiscountAmount);
                var pendingPayment = await _context.Payments
                    .Where(payment => payment.OrderId == order.Id && payment.Status == PaymentStatus.Pending)
                    .FirstOrDefaultAsync();
                if (pendingPayment != null)
                    pendingPayment.Amount = order.TotalPrice;
            }

            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return _mapper.Map<OrderDTO>(order);
        }

        public async Task<OrderDTO> CancelOrderAsync(int id)
        {
            var order = await GetTrackedOrderAsync(id);

            await UpdateOrderStatusAsync(id, OrderStatus.Cancelled);
            await UpdateCodPaymentStatusAsync(order.Id, PaymentStatus.Failed);

            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return _mapper.Map<OrderDTO>(order);
        }

        public async Task<OrderDTO> CompleteOrderAsync(int id)
        {
            var order = await GetTrackedOrderAsync(id);

            await UpdateOrderStatusAsync(id, OrderStatus.Completed);

            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return _mapper.Map<OrderDTO>(order);
        }

        public async Task<OrderDTO> ConfirmOrderAsync(int id, ConfirmOrderDTO dto)
        {
            var order = await GetTrackedOrderAsync(id);

            if (dto.Status != OrderStatus.Confirmed && dto.Status != OrderStatus.Shipped)
                throw new BadRequestException("Confirm order can only set status to Confirmed or Shipped");

            if (dto.Status == OrderStatus.Confirmed)
            {
                if (order.DeliveredByUserId.HasValue)
                    throw new BadRequestException("Delivery user has already been assigned");
                if (!dto.DeliveredByUserId.HasValue)
                    throw new BadRequestException("Delivery user is required when confirming");
                order.DeliveredByUserId = dto.DeliveredByUserId.Value;
            }
            await UpdateOrderStatusAsync(id, dto.Status);

            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return _mapper.Map<OrderDTO>(order);
        }

        public async Task<OrderDTO> ShipOrderAsync(int id, ShipOrderDTO dto)
        {
            var order = await GetTrackedOrderAsync(id);

            if (dto.Status != OrderStatus.Delivered && dto.Status != OrderStatus.Returned)
                throw new BadRequestException("Ship order can only set status to Delivered or Returned");

            if (!dto.Medias.Any())
                throw new BadRequestException("Ship order must have at least one media");

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await UpdateOrderStatusAsync(id, dto.Status);
                await UpdateCodPaymentStatusAsync(id, dto.Status == OrderStatus.Delivered ? PaymentStatus.Success : PaymentStatus.Failed);

                foreach (var mediaDto in dto.Medias)
                {
                    var media = _mapper.Map<Media>(mediaDto);
                    media.OrderId = order.Id;
                    order.Medias.Add(media);
                    await _mediaService.ValidateMediaType(media);
                }

                order.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return _mapper.Map<OrderDTO>(order);
        }

        private async Task<Order> GetTrackedOrderAsync(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) throw new NotFoundException("Order not found");
            return order;
        }

        public async Task<OrderDTO> UpdateOrderStatusAsync(int id, OrderStatus targetStatus)
        {
            var order = await _context.Orders
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) throw new NotFoundException("Order not found");

            SetOrderStatus(order, targetStatus);
            await _context.SaveChangesAsync();
            return _mapper.Map<OrderDTO>(order);
        }

        private static void SetOrderStatus(Order order, OrderStatus targetStatus)
        {
            order.Status = targetStatus;
            order.UpdatedAt = DateTime.UtcNow;
        }

        private async Task UpdateCodPaymentStatusAsync(int orderId, PaymentStatus targetPaymentStatus)
        {
            var payment = await _paymentService.GetPendingPaymentAsync(orderId);
            if (payment == null || payment.PaymentMethod != PaymentMethod.COD)
                return;

            await _paymentService.UpdateStatusAsync(payment.Id, targetPaymentStatus);
        }

        private async Task<string> GenerateTrackingNumberAsync()
        {
            var datePart = DateTime.UtcNow.ToString("yyMMdd");

            for (var attempt = 0; attempt < 3; attempt++)
            {
                var randomPart = Guid.NewGuid().ToString("N")[..9].ToUpperInvariant();
                var trackingNumber = $"AGT{datePart}{randomPart}";

                if (!await _context.Orders.AnyAsync(o => o.TrackingNumber == trackingNumber))
                    return trackingNumber;
            }

            throw new BadRequestException("Could not generate a unique tracking number");
        }
    }
}
