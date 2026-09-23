using AgitekBackend.DTOs.Addresses;
using AgitekBackend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgitekBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AddressController : ControllerBase
    {
        private readonly IAddressService _addressService;

        public AddressController(IAddressService addressService)
        {
            _addressService = addressService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AddressDTO>>> GetAll()
        {
            var addresses = await _addressService.GetAllAsync();
            return Ok(addresses);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AddressDTO>> GetById(int id)
        {
            var address = await _addressService.GetByIdAsync(id);
            return Ok(address);
        }

        [HttpPost]
        public async Task<ActionResult<AddressDTO>> Create([FromBody] CreateAddressDTO dto)
        {
            var createdAddress = await _addressService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = createdAddress.Id }, createdAddress);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<AddressDTO>> Update(int id, [FromBody] UpdateAddressDTO dto)
        {
            var updatedAddress = await _addressService.UpdateAsync(id, dto);
            return Ok(updatedAddress);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _addressService.DeleteAsync(id);
            return Ok();
        }
    }
}


