using AgitekBackend.DTOs.OrderDetails;
using AgitekBackend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgitekBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderDetailController : ControllerBase
    {
        private readonly IOrderDetailService _orderDetailService;

        public OrderDetailController(IOrderDetailService orderDetailService)
        {
            _orderDetailService = orderDetailService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDetailDTO>>> GetAll()
        {
            var orderDetails = await _orderDetailService.GetAllAsync();
            return Ok(orderDetails);
        }

        [HttpGet("{orderId}/{productId}")]
        public async Task<ActionResult<OrderDetailDTO>> GetById(int orderId, int productId)
        {
            var orderDetail = await _orderDetailService.GetByIdAsync(orderId, productId);
            if (orderDetail == null)
            {
                return NotFound();
            }
            return Ok(orderDetail);
        }

        [HttpPost]
        public async Task<ActionResult<OrderDetailDTO>> Create([FromBody] CreateOrderDetailDTO dto)
        {
            var createdOrderDetail = await _orderDetailService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { orderId = createdOrderDetail.OrderId, productId = createdOrderDetail.ProductId }, createdOrderDetail);
        }

        [HttpPatch("{orderId}/{productId}")]
        public async Task<ActionResult<OrderDetailDTO>> Update(int orderId, int productId, [FromBody] UpdateOrderDetailDTO dto)
        {
            var updatedOrderDetail = await _orderDetailService.UpdateAsync(orderId, productId, dto);
            if (updatedOrderDetail == null)
            {
                return NotFound();
            }
            return Ok(updatedOrderDetail);
        }

        [HttpDelete("{orderId}/{productId}")]
        public async Task<ActionResult> Delete(int orderId, int productId)
        {
            var success = await _orderDetailService.DeleteAsync(orderId, productId);
            if (!success)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}


