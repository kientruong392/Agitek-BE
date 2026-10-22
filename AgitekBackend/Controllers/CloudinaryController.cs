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
            var operation = folder.ToLowerInvariant() switch
            {
                "agitek/orders/proof" => CloudinaryOperation.UploadOrderProof,
                "agitek/reviews" => CloudinaryOperation.UploadReview,
                "agitek/refunds" => CloudinaryOperation.UploadRefund,
                "agitek/warranties" => CloudinaryOperation.UploadWarranty,
                _ => CloudinaryOperation.Upload
            };
            var authResult = await _authorizationService.AuthorizeAsync(User, folder, new CloudinaryManagementRequirement(operation));
            if (!authResult.Succeeded) throw new ForbiddenException("You do not have permission to access this resource");

            var signatureDto = _cloudinaryService.GetUploadSignature(folder);
            return Ok(signatureDto);
        }
    }
}
