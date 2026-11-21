using AgitekBackend.Authorization.Requirements;
using AgitekBackend.Common.Exceptions;
using AgitekBackend.DTOs.AuditLogs;
using AgitekBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgitekBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AuditLogController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;
        private readonly IAuthorizationService _authorizationService;

        public AuditLogController(IAuditLogService auditLogService, IAuthorizationService authorizationService)
        {
            _auditLogService = auditLogService;
            _authorizationService = authorizationService;
        }

        [HttpGet]
        public async Task<ActionResult<AuditLogPageDTO>> GetAll([FromQuery] AuditLogQueryDTO query)
        {
            await EnsureAuthorizedAsync();
            return Ok(await _auditLogService.GetPageAsync(query));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AuditLogDTO>> GetById(int id)
        {
            await EnsureAuthorizedAsync();
            return Ok(await _auditLogService.GetByIdAsync(id));
        }

        private async Task EnsureAuthorizedAsync()
        {
            var result = await _authorizationService.AuthorizeAsync(User, null, new AuditLogManagementRequirement());
            if (!result.Succeeded)
                throw new ForbiddenException("You do not have permission to access audit logs");
        }
    }
}
