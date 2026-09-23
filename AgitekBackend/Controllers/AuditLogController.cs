using AgitekBackend.DTOs.AuditLogs;
using AgitekBackend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgitekBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuditLogController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;

        public AuditLogController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AuditLogDTO>>> GetAll()
        {
            var logs = await _auditLogService.GetAllAsync();
            return Ok(logs);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AuditLogDTO>> GetById(int id)
        {
            var log = await _auditLogService.GetByIdAsync(id);
            return Ok(log);
        }

        [HttpPost]
        public async Task<ActionResult<AuditLogDTO>> Create([FromBody] CreateAuditLogDTO dto)
        {
            var createdLog = await _auditLogService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = createdLog.Id }, createdLog);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<AuditLogDTO>> Update(int id, [FromBody] UpdateAuditLogDTO dto)
        {
            var updatedLog = await _auditLogService.UpdateAsync(id, dto);
            return Ok(updatedLog);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _auditLogService.DeleteAsync(id);
            return Ok();
        }
    }
}


