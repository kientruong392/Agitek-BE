using AgitekBackend.DTOs.Orders;
using AgitekBackend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgitekBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDTO>>> GetAll()
        {
            var orders = await _orderService.GetAllAsync();
            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDTO>> GetById(int id)
        {
            var order = await _orderService.GetByIdAsync(id);
            return Ok(order);
        }

        [HttpPost]
        public async Task<ActionResult<OrderDTO>> Create([FromBody] CreateOrderDTO dto)
        {
            var createdOrder = await _orderService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = createdOrder.Id }, createdOrder);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<OrderDTO>> Update(int id, [FromBody] UpdateOrderDTO dto)
        {
            var updatedOrder = await _orderService.UpdateAsync(id, dto);
            return Ok(updatedOrder);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _orderService.DeleteAsync(id);
            return Ok();
        }
    }
}


