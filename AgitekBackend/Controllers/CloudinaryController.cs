using AgitekBackend.Authorization.Requirements;
using AgitekBackend.Common.Exceptions;
using AgitekBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgitekBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CloudinaryController : ControllerBase
    {
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IAuthorizationService _authorizationService;

        public CloudinaryController(ICloudinaryService cloudinaryService, IAuthorizationService authorizationService)
        {
            _cloudinaryService = cloudinaryService;
            _authorizationService = authorizationService;
        }

        [HttpGet("signature")]
        public async Task<ActionResult> GetUploadSignature([FromQuery] string folder = "agitek")
        {
            var authResult = await _authorizationService.AuthorizeAsync(User, null, new CloudinaryManagementRequirement(CloudinaryOperation.Upload));
            if (!authResult.Succeeded) throw new ForbiddenException("You do not have permission to access this resource");

            var signatureDto = _cloudinaryService.GetUploadSignature(folder);
            return Ok(signatureDto);
        }
    }
}
