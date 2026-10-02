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

            var warranties = await _context.Warranties
                .AsNoTracking()
                .Include(w => w.Media)
                .Include(w => w.OrderDetail)
                .Include(w => w.User)
                .ToListAsync();

            return _mapper.Map<IEnumerable<WarrantyDTO>>(warranties);
        }

        public async Task<WarrantyDTO?> GetByIdAsync(int id)
        {
            var warranty = await _context.Warranties
                .AsNoTracking()
                .Include(w => w.Media)
                .Include(w => w.OrderDetail)
                .Include(w => w.User)
                .FirstOrDefaultAsync(w => w.Id == id);
            if (warranty == null)
                throw new NotFoundException("Warranty not found");

            return _mapper.Map<WarrantyDTO>(warranty);
        }

        public async Task<WarrantyDTO> CreateAsync(CreateWarrantyDTO dto, int currentUserId)
        {
            var orderDetail = await _context.OrderDetails
                .AsNoTracking()
                .FirstOrDefaultAsync(od => od.OrderId == dto.OrderId && od.ProductId == dto.ProductId);
            if (orderDetail == null)
                throw new NotFoundException("Order detail not found");

            var order = await _context.Orders
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == dto.OrderId);
            if (order == null)
                throw new NotFoundException("Order not found");

            if (order.UserId != currentUserId)
                throw new BadRequestException("Order does not belong to the current user");

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

            return _mapper.Map<WarrantyDTO>(warranty);
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

            return _mapper.Map<WarrantyDTO>(warranty);
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
            return _mapper.Map<WarrantyDTO>(warranty);
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
    }
}
