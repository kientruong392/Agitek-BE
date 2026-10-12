using AgitekBackend.Common;
using AgitekBackend.Data;
using AgitekBackend.DTOs.Medias;
using AgitekBackend.DTOs.Orders;
using AgitekBackend.DTOs.Payments;
using AgitekBackend.Models;
using AgitekBackend.Common.Exceptions;
using AgitekBackend.Services.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using AgitekBackend.DTOs.Addresses;

namespace AgitekBackend.Services
{
    public class OrderService : IOrderService
    {
        private const decimal ShippingFeeAmount = 30000m;

        private static readonly Dictionary<OrderStatus, HashSet<OrderStatus>> AllowedTransitions = new()
        {
            [OrderStatus.Ordered] = new() { OrderStatus.Confirmed, OrderStatus.Cancelled },
            [OrderStatus.Confirmed] = new() { OrderStatus.Shipped },
            [OrderStatus.Shipped] = new() { OrderStatus.InTransit },
            [OrderStatus.InTransit] = new() { OrderStatus.Delivered, OrderStatus.Returned },
            [OrderStatus.Delivered] = new() { OrderStatus.Completed, OrderStatus.Refunded }
        };

        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IPaymentService _paymentService;
        private readonly IAddressService _addressService;
        private readonly IMediaService _mediaService;

        public OrderService(
            AppDbContext context,
            IMapper mapper,
            IPaymentService paymentService,
            IAddressService addressService,
            IMediaService mediaService)
        {
            _context = context;
            _mapper = mapper;
            _paymentService = paymentService;
            _addressService = addressService;
            _mediaService = mediaService;
        }

        public async Task<IEnumerable<OrderDTO>> GetAllAsync(int userId, bool assignedToDelivery = false)
        {
            IQueryable<Order> query = _context.Orders
                .AsNoTracking()
                .Include(o => o.Payments)
                .Include(o => o.OrderDetails)
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
                .Include(o => o.OrderTrackingLogs)
                .Include(o => o.FromLocation)
                .Include(o => o.ToLocation)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) throw new NotFoundException("Order not found");
            return _mapper.Map<OrderDTO>(order);
        }

        public async Task<OrderDTO> CreateAsync(CreateOrderDTO dto, int currentUserId)
        {
            if (dto.OrderDetails == null || !dto.OrderDetails.Any())
                throw new BadRequestException("Order must have at least one order detail");

            var address = await _context.Addresses
                .FirstOrDefaultAsync(a => a.Id == dto.AddressId);

            if (address == null)
                throw new BadRequestException("Address not found");

            if (address.UserId != currentUserId)
                throw new BadRequestException("Address does not belong to the current user");

            if (dto.VoucherId.HasValue)
            {
                var voucherAlreadyUsed = await _context.Orders
                    .AnyAsync(o => o.UserId == currentUserId && o.VoucherId == dto.VoucherId.Value);

                if (voucherAlreadyUsed)
                    throw new BadRequestException("This voucher has already been used in one of your orders");
            }

            var order = _mapper.Map<Order>(dto);
            order.TrackingNumber = await GenerateTrackingNumberAsync();
            order.UserId = currentUserId;
            order.AddressId = address.Id;
            order.ToLocationId = null;
            order.Address = BuildAddressSnapshot(_mapper.Map<AddressDTO>(address));
            order.ShippingFee = ShippingFeeAmount;
            order.DiscountAmount = 0;
            order.SubtotalPrice = order.OrderDetails.Sum(od => od.TotalPrice);
            order.TotalPrice = order.SubtotalPrice + order.ShippingFee - order.DiscountAmount;
            order.Status = OrderStatus.Ordered;
            order.CreatedAt = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                await _paymentService.CreateAsync(
                    new CreatePaymentDTO
                    {
                        OrderId = order.Id,
                        PaymentMethod = dto.PaymentMethod,
                        TransactionId = dto.TransactionId
                    });

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return await GetByIdAsync(order.Id);
        }

        public async Task<OrderDTO> UpdateAsync(int id, UpdateOrderDTO dto)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) throw new NotFoundException("Order not found");
            _mapper.Map(dto, order);
            if (order.AddressId.HasValue)
            {
                var address = await _addressService.GetByIdAsync(order.AddressId.Value);
                order.Address = BuildAddressSnapshot(address);
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

            order.Status = targetStatus;
            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return _mapper.Map<OrderDTO>(order);
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

        private string BuildAddressSnapshot(AddressDTO addressDto)
        {
            return addressDto.AddressLine;
        }
    }
}
