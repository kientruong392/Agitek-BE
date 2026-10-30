using System.Security.Claims;
using AgitekBackend.Authorization.Requirements;
using AgitekBackend.Common;
using AgitekBackend.Common.Exceptions;
using AgitekBackend.DTOs.PcBuilds;
using AgitekBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgitekBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PcBuildController : ControllerBase
    {
        private readonly IPcBuildService _service;
        private readonly IAuthorizationService _authorizationService;

        public PcBuildController(IPcBuildService service, IAuthorizationService authorizationService)
        {
            _service = service;
            _authorizationService = authorizationService;
        }

        [HttpPost("products/{component}")]
        public async Task<IActionResult> GetProductsBySpecs(PcComponentType component, PcBuildDTO dto)
        {
            await AuthorizeBuildAsync(null, PcBuildOperation.ReadAll);
            return Ok(await _service.GetProductsBySpecsAsync(component, dto));
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout(int[] productIds)
        {
            await AuthorizeBuildAsync(null, PcBuildOperation.ReadAll);
            return Ok(await _service.GetCheckoutProductsAsync(productIds));
        }

        [HttpGet]
        public async Task<IActionResult> GetMy()
        {
            await AuthorizeBuildAsync(null, PcBuildOperation.ReadAll);
            return Ok(await _service.GetMyAsync(GetUserId()));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            await AuthorizeBuildAsync(id, PcBuildOperation.Read);
            return Ok(await _service.GetByIdAsync(id, GetUserId()));
        }

        [HttpPost]
        public async Task<IActionResult> Create(SavePcBuildDTO dto)
        {
            await AuthorizeBuildAsync(null, PcBuildOperation.Create);
            var build = await _service.CreateAsync(dto, GetUserId());
            return CreatedAtAction(nameof(GetById), new { id = build.Id }, build);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, SavePcBuildDTO dto)
        {
            await AuthorizeBuildAsync(id, PcBuildOperation.Update);
            return Ok(await _service.UpdateAsync(id, dto, GetUserId()));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await AuthorizeBuildAsync(id, PcBuildOperation.Delete);
            await _service.DeleteAsync(id, GetUserId());
            return Ok();
        }

        private async Task AuthorizeBuildAsync(int? id, PcBuildOperation operation)
        {
            var result = await _authorizationService.AuthorizeAsync(
                User, id, new PcBuildManagementRequirement(operation));
            if (!result.Succeeded)
                throw new ForbiddenException("You do not have permission to access this resource");
        }

        private int GetUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? User.FindFirstValue("id");
            if (!int.TryParse(claim, out var id) || id <= 0) throw new UnauthorizedException("User ID not found");
            return id;
        }
    }
}
