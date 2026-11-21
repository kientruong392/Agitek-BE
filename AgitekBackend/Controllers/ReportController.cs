using AgitekBackend.Authorization.Requirements;
using AgitekBackend.Common.Exceptions;
using AgitekBackend.DTOs.Reports;
using AgitekBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgitekBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly IAuthorizationService _authorizationService;

        public ReportController(IReportService reportService, IAuthorizationService authorizationService)
        {
            _reportService = reportService;
            _authorizationService = authorizationService;
        }

        [HttpGet]
        public async Task<ActionResult<ReportDTO>> Get([FromQuery] ReportQueryDTO query)
        {
            var authorization = await _authorizationService.AuthorizeAsync(User, null, new ReportManagementRequirement());
            if (!authorization.Succeeded)
                throw new ForbiddenException("You do not have permission to access reports");

            return Ok(await _reportService.GetAsync(query));
        }
    }
}
