using AgitekBackend.Common.Exceptions;
using AgitekBackend.Data;
using AgitekBackend.DTOs.Medias;
using AgitekBackend.DTOs.Reviews;
using AgitekBackend.Models;
using AgitekBackend.Services.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AgitekBackend.Services
{
    public class ReviewService : IReviewService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IMediaService _mediaService;

        public ReviewService(AppDbContext context, IMapper mapper, IMediaService mediaService)
        {
            _context = context;
            _mapper = mapper;
            _mediaService = mediaService;
        }

        public async Task<IEnumerable<ReviewDTO>> GetAllAsync(int productId)
        {
            var reviews = await _context.Reviews
                .AsNoTracking()
                .Where(r => r.ProductId == productId)
                .Include(r => r.Medias)
                .Include(r => r.User)
                    .ThenInclude(user => user!.Profile)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ReviewDTO>>(reviews);
        }

        public async Task<ReviewDTO> GetByIdAsync(int id)
        {
            var review = await _context.Reviews
                .AsNoTracking()
                .Include(r => r.Medias)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (review == null)
                throw new NotFoundException("Review not found");

            return _mapper.Map<ReviewDTO>(review);
        }

        public async Task<ReviewDTO> CreateAsync(CreateReviewDTO dto, int currentUserId)
        {
            await EnsureOrderDetailExistsAsync(dto.OrderId, dto.ProductId);

            var review = _mapper.Map<Review>(dto);
            review.UserId = currentUserId;
            review.CreatedAt = DateTime.UtcNow;
            review.UpdatedAt = DateTime.UtcNow;

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Reviews.Add(review);
                _context.ChangeTracker.DetectChanges();

                foreach (var media in review.Medias)
                    await _mediaService.ValidateMediaType(media);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            await UpdateAverageRatingAsync(review.ProductId);
            return _mapper.Map<ReviewDTO>(review);
        }

        public async Task<ReviewDTO> UpdateAsync(int id, UpdateReviewDTO dto)
        {
            var review = await _context.Reviews
                .Include(r => r.Medias)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (review == null)
                throw new NotFoundException("Review not found");

            await EnsureOrderDetailExistsAsync(review.OrderId, review.ProductId);

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _mapper.Map(dto, review);

                if (dto.Medias != null)
                {
                    _context.Medias.RemoveRange(review.Medias);
                    review.Medias.Clear();

                    foreach (var mediaDto in dto.Medias)
                    {
                        var media = _mapper.Map<Media>(mediaDto);
                        media.ReviewId = review.Id;
                        review.Medias.Add(media);
                        await _mediaService.ValidateMediaType(media);
                    }
                }

                review.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            await UpdateAverageRatingAsync(review.ProductId);
            return _mapper.Map<ReviewDTO>(review);
        }

        public async Task<ReviewDTO> ReplyAsync(int id, int currentUserId, ReplyReviewDTO dto)
        {
            var review = await _context.Reviews.FirstOrDefaultAsync(r => r.Id == id);
            if (review == null)
                throw new NotFoundException("Review not found");

            if (review.ReplyUserId.HasValue || review.ReplyComment != null || review.ReplyCreatedAt.HasValue)
                throw new BadRequestException("Review already has a reply");

            _mapper.Map(dto, review);
            review.ReplyUserId = currentUserId;
            review.ReplyCreatedAt = DateTime.UtcNow;
            review.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return _mapper.Map<ReviewDTO>(review);
        }

        private async Task EnsureOrderDetailExistsAsync(int orderId, int productId)
        {
            var orderDetailExists = await _context.OrderDetails
                .AnyAsync(od => od.OrderId == orderId && od.ProductId == productId);
            if (!orderDetailExists)
                throw new NotFoundException("Order detail not found");
        }

        private async Task UpdateAverageRatingAsync(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                return;

            product.AverageRating = await _context.Reviews
                .Where(r => r.ProductId == productId)
                .AverageAsync(r => (double?)r.Rating) ?? 0;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
    }
}
