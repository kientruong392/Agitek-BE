using AgitekBackend.Common;
using AgitekBackend.DTOs.Vouchers;
using AgitekBackend.Models;

namespace AgitekBackend.Services.Interfaces
{
    public interface IVoucherService
    {
        Task<IEnumerable<VoucherDTO>> GetAllAsync(CurrentUser? currentUser);
        Task<VoucherDTO?> GetByIdAsync(int id);
        Task<VoucherDTO> CreateAsync(CreateVoucherDTO dto);
        Task<VoucherDTO?> UpdateAsync(int id, UpdateVoucherDTO dto);
        Task DecreaseQuantityAsync(int id);
        Task<VoucherApplicationDTO> ValidateForOrderAsync(string code, decimal subtotalPrice, int? userId);
    }
}
