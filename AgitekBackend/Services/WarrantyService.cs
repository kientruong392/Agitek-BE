using AgitekBackend.Common;
using AgitekBackend.Common.Exceptions;
using AgitekBackend.Common.Utils;
using AgitekBackend.Data;
using AgitekBackend.DTOs.Warranties;
using AgitekBackend.Models;
using AgitekBackend.Services.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AgitekBackend.Services
{
    public class WarrantyService : IWarrantyService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IMediaService _mediaService;

        public WarrantyService(AppDbContext context, IMapper mapper, IMediaService mediaService)
        {
            _context = context;
            _mapper = mapper;
            _mediaService = mediaService;
        }

        public async Task<IEnumerable<WarrantyDTO>> GetAllAsync(CurrentUser currentUser)
        {
            if (currentUser.Role != RoleName.SuperAdmin &&
                currentUser.Role != RoleName.Admin &&
                currentUser.Role != RoleName.Manager &&
                currentUser.Role != RoleName.Staff)
            {
                throw new ForbiddenException("You do not have permission to access this resource");
            }

            var warranties = await WarrantiesWithDetails()
                .AsNoTracking()
                .OrderByDescending(warranty => warranty.CreatedAt)
                .ToListAsync();

            return _mapper.Map<IEnumerable<WarrantyDTO>>(warranties);
        }

        public async Task<IEnumerable<WarrantyDTO>> GetMyAsync(int currentUserId)
        {
            var warranties = await WarrantiesWithDetails()
                .AsNoTracking()
                .Where(warranty => warranty.UserId == currentUserId)
                .OrderByDescending(warranty => warranty.CreatedAt)
                .ToListAsync();

            return _mapper.Map<IEnumerable<WarrantyDTO>>(warranties);
        }

        public async Task<WarrantyDTO?> GetByIdAsync(int id)
        {
            var warranty = await WarrantiesWithDetails()
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == id);
            if (warranty == null)
                throw new NotFoundException("Warranty not found");

            return _mapper.Map<WarrantyDTO>(warranty);
        }

        public async Task<WarrantyDTO> CreateAsync(CreateWarrantyDTO dto, int currentUserId)
        {
            if (dto.Medias == null || !dto.Medias.Any())
                throw new BadRequestException("Warranty must have at least one media");

            var orderDetail = await _context.OrderDetails
                .Include(detail => detail.Order)
                .Include(detail => detail.Product)
                .FirstOrDefaultAsync(od => od.OrderId == dto.OrderId && od.ProductId == dto.ProductId);
            if (orderDetail == null)
                throw new NotFoundException("Order detail not found");

            if (orderDetail.Order.UserId != currentUserId)
                throw new BadRequestException("Order does not belong to the current user");

            if (orderDetail.Order.Status != OrderStatus.Delivered &&
                orderDetail.Order.Status != OrderStatus.Completed)
            {
                throw new BadRequestException("Warranties are only available after the order is delivered");
            }

            if (orderDetail.Product.WarrantyPeriod <= 0)
                throw new BadRequestException("This product does not have a warranty period");

            if (dto.Quantity < 1 || dto.Quantity > orderDetail.Quantity)
                throw new BadRequestException("Warranty quantity must be between 1 and the purchased quantity");

            if (DateTime.UtcNow > orderDetail.Order.UpdatedAt.AddMonths(orderDetail.Product.WarrantyPeriod))
                throw new BadRequestException("The product warranty period has expired");

            var hasActiveWarranty = await _context.Warranties.AnyAsync(warranty =>
                warranty.OrderId == dto.OrderId &&
                warranty.ProductId == dto.ProductId &&
                (warranty.Status == WarrantyStatus.Pending || warranty.Status == WarrantyStatus.Processing));
            if (hasActiveWarranty)
                throw new BadRequestException("This order item already has an active warranty request");

            var warranty = _mapper.Map<Warranty>(dto);
            warranty.Code = Helpers.GenerateWarrantyCode();
            warranty.UserId = currentUserId;
            warranty.Status = WarrantyStatus.Pending;
            warranty.CreatedAt = DateTime.UtcNow;
            warranty.UpdatedAt = DateTime.UtcNow;

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Warranties.Add(warranty);
                _context.ChangeTracker.DetectChanges();

                foreach (var mediaDto in dto.Medias)
                {
                    var media = _mapper.Map<Media>(mediaDto);
                    media.ReviewId = null;
                    media.OrderId = null;
                    media.RefundOrderId = null;
                    media.RefundProductId = null;
                    media.Warranty = warranty;
                    warranty.Media.Add(media);
                    await _mediaService.ValidateMediaType(media);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return await GetByIdAsync(warranty.Id)
                ?? throw new NotFoundException("Warranty not found");
        }

        public async Task<WarrantyDTO?> UpdateAsync(int id, UpdateWarrantyDTO dto)
        {
            var warranty = await _context.Warranties
                .Include(w => w.Media)
                .FirstOrDefaultAsync(w => w.Id == id);
            if (warranty == null)
                throw new NotFoundException("Warranty not found");

            if (warranty.Status != WarrantyStatus.Pending)
                throw new BadRequestException("Warranty can only be updated while in Pending status");

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _mapper.Map(dto, warranty);

                if (dto.Medias != null)
                {
                    _context.Medias.RemoveRange(warranty.Media);
                    warranty.Media.Clear();

                    foreach (var mediaDto in dto.Medias)
                    {
                        var media = _mapper.Map<Media>(mediaDto);
                        media.ReviewId = null;
                        media.OrderId = null;
                        media.RefundOrderId = null;
                        media.RefundProductId = null;
                        media.Warranty = warranty;
                        warranty.Media.Add(media);
                        await _mediaService.ValidateMediaType(media);
                    }
                }

                warranty.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return await GetByIdAsync(warranty.Id)
                ?? throw new NotFoundException("Warranty not found");
        }

        public async Task<WarrantyDTO> ApproveAsync(int id, int currentUserId, ApproveWarrantyDTO dto)
        {
            var warranty = await _context.Warranties
                .FirstOrDefaultAsync(w => w.Id == id);
            if (warranty == null)
                throw new NotFoundException("Warranty not found");

            var isPendingTransition = warranty.Status == WarrantyStatus.Pending &&
                (dto.Status == WarrantyStatus.Rejected ||
                 dto.Status == WarrantyStatus.Processing);

            var isProcessingTransition = warranty.Status == WarrantyStatus.Processing &&
                (dto.Status == WarrantyStatus.Rejected ||
                 dto.Status == WarrantyStatus.Completed);

            if (!isPendingTransition && !isProcessingTransition)
                throw new BadRequestException("Warranty status transition is not allowed");

            _mapper.Map(dto, warranty);
            warranty.UpdatedAt = DateTime.UtcNow;

            if (dto.Status == WarrantyStatus.Processing && warranty.StartDate == null)
                warranty.StartDate = DateTime.UtcNow;

            if (dto.Status == WarrantyStatus.Completed)
                warranty.CompletedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await GetByIdAsync(warranty.Id)
                ?? throw new NotFoundException("Warranty not found");
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var warranty = await _context.Warranties
                .Include(w => w.Media)
                .FirstOrDefaultAsync(w => w.Id == id);
            if (warranty == null)
                throw new NotFoundException("Warranty not found");

            if (warranty.Status != WarrantyStatus.Pending)
                throw new BadRequestException("Warranty can only be deleted while in Pending status");

            _context.Medias.RemoveRange(warranty.Media);
            _context.Warranties.Remove(warranty);
            await _context.SaveChangesAsync();
            return true;
        }

        private IQueryable<Warranty> WarrantiesWithDetails()
        {
            return _context.Warranties
                .Include(warranty => warranty.Media)
                .Include(warranty => warranty.User)
                    .ThenInclude(user => user!.Profile)
                .Include(warranty => warranty.OrderDetail)
                    .ThenInclude(detail => detail!.Product)
                        .ThenInclude(product => product!.Images)
                .Include(warranty => warranty.OrderDetail)
                    .ThenInclude(detail => detail!.Order)
                        .ThenInclude(order => order.User)
                            .ThenInclude(user => user.Profile)
                .AsSplitQuery();
        }
    }
}
