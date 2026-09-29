using AgitekBackend.Common;
using AgitekBackend.Data;
using AgitekBackend.DTOs.Orders;
using AgitekBackend.DTOs.Payments;
using AgitekBackend.Models;
using AgitekBackend.Common.Exceptions;
using AgitekBackend.Services.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AgitekBackend.Services
{
    public class OrderService : IOrderService
    {
        private const decimal ShippingFeeAmount = 30000m;

        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IPaymentService _paymentService;

        public OrderService(AppDbContext context, IMapper mapper, IPaymentService paymentService)
        {
            _context = context;
            _mapper = mapper;
            _paymentService = paymentService;
        }

        public async Task<IEnumerable<OrderDTO>> GetAllAsync(int userId)
        {
            var orders = await _context.Orders
                .AsNoTracking()
                .Include(o => o.Payments)
                .Include(o => o.OrderDetails)
                .Include(o => o.OrderTrackingLogs)
                .Include(o => o.FromLocation)
                .Include(o => o.ToLocation)
                .Where(o => o.UserId == userId)
                .ToListAsync();

            return _mapper.Map<IEnumerable<OrderDTO>>(orders);
        }

        public async Task<OrderDTO?> GetByIdAsync(int id)
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
                .Include(a => a.Location)
                    .ThenInclude(l => l!.Parent)
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
            order.ToLocationId = address.LocationId;
            order.Address = BuildAddressSnapshot(address);
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

            return await GetByIdAsync(order.Id) ?? _mapper.Map<OrderDTO>(order);
        }

        public async Task<OrderDTO?> UpdateAsync(int id, UpdateOrderDTO dto)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) throw new NotFoundException("Order not found");

            if (dto.AddressId.HasValue && dto.AddressId.Value != order.AddressId)
            {
                var address = await _context.Addresses
                    .Include(a => a.Location)
                        .ThenInclude(l => l!.Parent)
                    .FirstOrDefaultAsync(a => a.Id == dto.AddressId.Value);

                if (address == null)
                    throw new BadRequestException("Address not found");

                order.AddressId = address.Id;
                order.ToLocationId = address.LocationId;
                order.Address = BuildAddressSnapshot(address);
            }

            if (dto.Note != null)
                order.Note = dto.Note;

            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return await GetByIdAsync(order.Id);
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

        private string BuildAddressSnapshot(Address address)
        {
            var parts = new List<string> { address.AddressLine };

            var child = address.Location;
            if (child != null)
            {
                parts.Add(child.LocationName);

                var parent = child.Parent;
                if (parent != null)
                    parts.Add(parent.LocationName);
            }

            return string.Join(", ", parts);
        }
    }
}
