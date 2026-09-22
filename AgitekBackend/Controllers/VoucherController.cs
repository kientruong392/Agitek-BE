using AgitekBackend.DTOs.Vouchers;
using AgitekBackend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgitekBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VoucherController : ControllerBase
    {
        private readonly IVoucherService _voucherService;

        public VoucherController(IVoucherService voucherService)
        {
            _voucherService = voucherService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<VoucherDTO>>> GetAll()
        {
            var vouchers = await _voucherService.GetAllAsync();
            return Ok(vouchers);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<VoucherDTO>> GetById(int id)
        {
            var voucher = await _voucherService.GetByIdAsync(id);
            if (voucher == null)
            {
                return NotFound();
            }
            return Ok(voucher);
        }

        [HttpPost]
        public async Task<ActionResult<VoucherDTO>> Create([FromBody] CreateVoucherDTO dto)
        {
            var createdVoucher = await _voucherService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = createdVoucher.Id }, createdVoucher);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<VoucherDTO>> Update(int id, [FromBody] UpdateVoucherDTO dto)
        {
            var updatedVoucher = await _voucherService.UpdateAsync(id, dto);
            if (updatedVoucher == null)
            {
                return NotFound();
            }
            return Ok(updatedVoucher);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var success = await _voucherService.DeleteAsync(id);
            if (!success)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}


