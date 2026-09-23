using AgitekBackend.Data;
using AgitekBackend.DTOs.Vouchers;
using AgitekBackend.Models;
using AgitekBackend.Common.Exceptions;
using AgitekBackend.Services.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AgitekBackend.Services
{
    public class VoucherService : IVoucherService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public VoucherService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<VoucherDTO>> GetAllAsync()
        {
            var vouchers = await _context.Vouchers.ToListAsync();
            return _mapper.Map<IEnumerable<VoucherDTO>>(vouchers);
        }

        public async Task<VoucherDTO?> GetByIdAsync(int id)
        {
            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher == null) throw new NotFoundException($"Voucher with id {id} not found");
            return _mapper.Map<VoucherDTO>(voucher);
        }

        public async Task<VoucherDTO> CreateAsync(CreateVoucherDTO dto)
        {
            var voucher = _mapper.Map<Voucher>(dto);
            _context.Vouchers.Add(voucher);
            await _context.SaveChangesAsync();
            return _mapper.Map<VoucherDTO>(voucher);
        }

        public async Task<VoucherDTO?> UpdateAsync(int id, UpdateVoucherDTO dto)
        {
            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher == null) throw new NotFoundException($"Voucher with id {id} not found");

            _mapper.Map(dto, voucher);
            await _context.SaveChangesAsync();
            return _mapper.Map<VoucherDTO>(voucher);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher == null) throw new NotFoundException($"Voucher with id {id} not found");

            _context.Vouchers.Remove(voucher);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}




