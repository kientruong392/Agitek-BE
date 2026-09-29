using AgitekBackend.Data;
using AgitekBackend.DTOs.Reviews;
using AgitekBackend.Models;
using AgitekBackend.Common.Exceptions;
using AgitekBackend.Services.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AgitekBackend.Services
{
    public class ReviewService : IReviewService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ReviewService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ReviewDTO>> GetAllAsync()
        {
            var reviews = await _context.Reviews.ToListAsync();
            return _mapper.Map<IEnumerable<ReviewDTO>>(reviews);
        }

        public async Task<ReviewDTO?> GetByIdAsync(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null) throw new NotFoundException("Review not found");
            return _mapper.Map<ReviewDTO>(review);
        }

        public async Task<ReviewDTO> CreateAsync(CreateReviewDTO dto)
        {
            var review = _mapper.Map<Review>(dto);
            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();
            return _mapper.Map<ReviewDTO>(review);
        }

        public async Task<ReviewDTO?> UpdateAsync(int id, UpdateReviewDTO dto)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null) throw new NotFoundException("Review not found");

            _mapper.Map(dto, review);
            review.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return _mapper.Map<ReviewDTO>(review);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null) throw new NotFoundException("Review not found");

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}




