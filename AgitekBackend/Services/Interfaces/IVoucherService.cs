using AgitekBackend.DTOs.Vouchers;

namespace AgitekBackend.Services.Interfaces
{
    public interface IVoucherService
    {
        Task<IEnumerable<VoucherDTO>> GetAllAsync();
        Task<VoucherDTO?> GetByIdAsync(int id);
        Task<VoucherDTO> CreateAsync(CreateVoucherDTO dto);
        Task<VoucherDTO?> UpdateAsync(int id, UpdateVoucherDTO dto);
        Task<bool> DeleteAsync(int id);
    }
}


