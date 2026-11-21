using AgitekBackend.Common;
using AgitekBackend.Common.Exceptions;
using AgitekBackend.Data;
using AgitekBackend.DTOs.Vouchers;
using AgitekBackend.Models;
using AgitekBackend.Services.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AgitekBackend.Services
{
    public class VoucherService : IVoucherService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IAuditLogService _auditLogService;

        public VoucherService(AppDbContext context, IMapper mapper, IAuditLogService auditLogService)
        {
            _context = context;
            _mapper = mapper;
            _auditLogService = auditLogService;
        }

        public async Task<IEnumerable<VoucherDTO>> GetAllAsync(CurrentUser? currentUser)
        {
            var query = _context.Vouchers.AsQueryable();
            if (!IsManagementRole(currentUser?.Role))
            {
                query = query.Where(v => v.Status != VoucherStatus.Inactive);
            }

            var vouchers = await query.ToListAsync();
            return _mapper.Map<IEnumerable<VoucherDTO>>(vouchers);
        }

        public async Task<VoucherDTO?> GetByIdAsync(int id)
        {
            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher == null) throw new NotFoundException($"Voucher not found");
            return _mapper.Map<VoucherDTO>(voucher);
        }

        public async Task<VoucherApplicationDTO> ValidateForOrderAsync(string code, decimal subtotalPrice, int? userId)
        {
            var normalizedCode = code.ToUpperInvariant();
            var voucher = await _context.Vouchers
                .FirstOrDefaultAsync(item => item.Code.ToUpper() == normalizedCode);
            if (voucher == null ||
                (voucher.Status != VoucherStatus.Active && voucher.Status != VoucherStatus.LowStock) ||
                voucher.StartDate > DateTime.Now || voucher.EndDate < DateTime.Now || voucher.Quantity <= 0)
                throw new BadRequestException("Voucher is invalid or unavailable");
            if (voucher.MinOrderValue < 0 || voucher.DiscountValue < 0 ||
                voucher.MaxDiscountValue.HasValue && voucher.MaxDiscountValue.Value < 0)
                throw new BadRequestException("Voucher configuration is invalid");
            if (subtotalPrice < voucher.MinOrderValue)
                throw new BadRequestException("Order does not meet the voucher minimum amount");
            if (userId.HasValue && await _context.Orders.AnyAsync(order => order.UserId == userId && order.VoucherId == voucher.Id))
                throw new BadRequestException("This voucher has already been used in one of your orders");

            if (!Enum.IsDefined(voucher.DiscountType) ||
                voucher.DiscountType == DiscountType.Percentage && voucher.DiscountValue > 100)
                throw new BadRequestException("Voucher configuration is invalid");

            var discountAmount = voucher.DiscountType == DiscountType.Percentage
                ? subtotalPrice * voucher.DiscountValue / 100m
                : voucher.DiscountValue;
            if (voucher.MaxDiscountValue.HasValue)
                discountAmount = Math.Min(discountAmount, voucher.MaxDiscountValue.Value);
            discountAmount = Math.Round(Math.Min(discountAmount, subtotalPrice), 2, MidpointRounding.AwayFromZero);

            return new VoucherApplicationDTO
            {
                VoucherId = voucher.Id,
                Code = voucher.Code,
                DiscountAmount = decimal.Round(discountAmount, 2)
            };
        }

        public async Task<VoucherDTO> CreateAsync(CreateVoucherDTO dto)
        {
            ValidateDiscount(dto.DiscountType, dto.DiscountValue, dto.MaxDiscountValue);
            ValidateStatus(dto.Status);
            ValidateDates(dto.StartDate, dto.EndDate);

            var code = string.IsNullOrWhiteSpace(dto.Code)
                ? Guid.NewGuid().ToString("N")
                : dto.Code.Trim();

            if (await _context.Vouchers.AnyAsync(v => v.Code == code))
            {
                throw new BadRequestException("Voucher code already exists");
            }

            var voucher = _mapper.Map<Voucher>(dto);
            voucher.Code = code;
            voucher.UsedCount = 0;
            voucher.Quantity = voucher.InitialQuantity;

            _context.Vouchers.Add(voucher);
            await _context.SaveChangesAsync();
            return _mapper.Map<VoucherDTO>(voucher);
        }

        public async Task<VoucherDTO?> UpdateAsync(int id, UpdateVoucherDTO dto)
        {
            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher == null) throw new NotFoundException($"Voucher not found");

            if (dto.Status.HasValue)
            {
                ValidateStatus(dto.Status.Value);
            }

            if (dto.Quantity.HasValue)
            {
                if (dto.Quantity.Value < 1 || dto.Quantity.Value > 9999)
                {
                    throw new BadRequestException("Quantity must be between 1 and 9999");
                }

                if (dto.Quantity.Value < voucher.UsedCount)
                {
                    throw new BadRequestException("Quantity cannot be less than used count");
                }

                ApplyQuantityUpdate(voucher, dto.Quantity.Value);
            }

            if (dto.EndDate.HasValue && dto.EndDate.Value < voucher.StartDate)
            {
                throw new BadRequestException("End date cannot be before start date");
            }

            _mapper.Map(dto, voucher);
            CheckVoucherStatus(voucher);
            await _context.SaveChangesAsync();

            return _mapper.Map<VoucherDTO>(voucher);
        }

        public async Task DecreaseQuantityAsync(int id)
        {
            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher == null) throw new NotFoundException($"Voucher not found");

            await using var transaction = _context.Database.CurrentTransaction == null
                ? await _context.Database.BeginTransactionAsync()
                : null;
            var now = DateTime.Now;
            var updated = await _context.Vouchers
                .Where(item => item.Id == id &&
                    (item.Status == VoucherStatus.Active || item.Status == VoucherStatus.LowStock) &&
                    item.StartDate <= now && item.EndDate >= now && item.Quantity > 0)
                .ExecuteUpdateAsync(update => update
                    .SetProperty(item => item.Quantity, item => item.Quantity - 1)
                    .SetProperty(item => item.UsedCount, item => item.UsedCount + 1)
                    .SetProperty(item => item.Status, item => item.Quantity == 1
                        ? VoucherStatus.OutOfStock
                        : item.InitialQuantity > 0 && (decimal)(item.Quantity - 1) / item.InitialQuantity * 100 <= 20
                            ? VoucherStatus.LowStock
                            : VoucherStatus.Active));

            if (updated != 1)
                throw new BadRequestException("Voucher is out of stock");

            await _auditLogService.RecordActionAsync("Voucher", $"Updated #{id} [Quantity, UsedCount, Status]");
            if (transaction != null) await transaction.CommitAsync();
        }

        private bool IsManagementRole(RoleName? role)
        {
            return role == RoleName.Manager ||
                   role == RoleName.Admin ||
                   role == RoleName.SuperAdmin;
        }

        private void ApplyQuantityUpdate(Voucher voucher, int newQuantity)
        {
            voucher.InitialQuantity += newQuantity - voucher.Quantity;
            voucher.Quantity = newQuantity;
        }

        private void CheckVoucherStatus(Voucher voucher)
        {
            if (voucher.Status == VoucherStatus.Inactive || voucher.Status == VoucherStatus.Expired)
            {
                return;
            }

            if (voucher.Quantity == 0)
            {
                voucher.Status = VoucherStatus.OutOfStock;
            }
            else if (voucher.InitialQuantity > 0 && (decimal)voucher.Quantity / voucher.InitialQuantity * 100 <= 20)
            {
                voucher.Status = VoucherStatus.LowStock;
            }
            else
            {
                voucher.Status = VoucherStatus.Active;
            }
        }

        private void ValidateDiscount(
            DiscountType discountType,
            decimal discountValue,
            decimal? maxDiscountValue)
        {
            if (!Enum.IsDefined(discountType))
            {
                throw new BadRequestException("Invalid discount type");
            }

            if (discountType == DiscountType.Percentage)
            {
                if (discountValue < 0 || discountValue > 100)
                {
                    throw new BadRequestException("Percentage discount must be between 0 and 100");
                }

                if (maxDiscountValue.HasValue && maxDiscountValue.Value < 0)
                {
                    throw new BadRequestException("Max discount value cannot be negative");
                }
            }
            else if (discountType == DiscountType.FixedAmount)
            {
                if (discountValue < 0 || discountValue > 10_000_000)
                {
                    throw new BadRequestException("Fixed amount discount must be between 0 and 10000000");
                }

                if (maxDiscountValue.HasValue)
                {
                    throw new BadRequestException("Max discount value is only allowed for percentage discounts");
                }
            }
        }

        private void ValidateStatus(VoucherStatus status)
        {
            if (status != VoucherStatus.Active && status != VoucherStatus.Inactive)
            {
                throw new BadRequestException("Voucher status can only be Active or Inactive");
            }
        }

        private void ValidateDates(DateTime startDate, DateTime endDate)
        {
            var now = DateTime.Now;
            if (startDate < now.AddMinutes(-5))
            {
                throw new BadRequestException("INVALID_START_DATE");
            }

            if (endDate < startDate)
            {
                throw new BadRequestException("INVALID_END_DATE");
            }
        }
    }
}
