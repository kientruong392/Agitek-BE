using AgitekBackend.DTOs.OrderTrackingLogs;
using AgitekBackend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgitekBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderTrackingLogController : ControllerBase
    {
        private readonly IOrderTrackingLogService _orderTrackingLogService;

        public OrderTrackingLogController(IOrderTrackingLogService orderTrackingLogService)
        {
            _orderTrackingLogService = orderTrackingLogService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderTrackingLogDTO>>> GetAll()
        {
            var logs = await _orderTrackingLogService.GetAllAsync();
            return Ok(logs);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderTrackingLogDTO>> GetById(int id)
        {
            var log = await _orderTrackingLogService.GetByIdAsync(id);
            if (log == null)
            {
                return NotFound();
            }
            return Ok(log);
        }

        [HttpPost]
        public async Task<ActionResult<OrderTrackingLogDTO>> Create([FromBody] CreateOrderTrackingLogDTO dto)
        {
            var createdLog = await _orderTrackingLogService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = createdLog.Id }, createdLog);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<OrderTrackingLogDTO>> Update(int id, [FromBody] UpdateOrderTrackingLogDTO dto)
        {
            var updatedLog = await _orderTrackingLogService.UpdateAsync(id, dto);
            if (updatedLog == null)
            {
                return NotFound();
            }
            return Ok(updatedLog);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var success = await _orderTrackingLogService.DeleteAsync(id);
            if (!success)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}


