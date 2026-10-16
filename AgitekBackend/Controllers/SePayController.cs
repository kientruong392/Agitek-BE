using AgitekBackend.Common.Settings;
using AgitekBackend.DTOs.Payments;
using AgitekBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgitekBackend.Controllers
{
    [Route("api/sepay")]
    [ApiController]
    public class SePayController : ControllerBase
    {
        private readonly ISePayService _sePayService;
        private readonly IPaymentService _paymentService;

        public SePayController(ISePayService sePayService, IPaymentService paymentService)
        {
            _sePayService = sePayService;
            _paymentService = paymentService;
        }

        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> Webhook()
        {
            await using var body = new MemoryStream();
            await Request.Body.CopyToAsync(body, HttpContext.RequestAborted);

            try
            {
                await _sePayService.ProcessWebhookAsync(
                    body.ToArray(),
                    Request.Headers["X-SePay-Timestamp"].ToString(),
                    Request.Headers["X-SePay-Signature"].ToString());
            }
            catch (UnauthorizedAccessException e)
            {
                return Unauthorized(e.Message);
            }
            catch (ArgumentException)
            {
                return BadRequest();
            }

            return Ok(new { success = true, statusCode = StatusCodes.Status200OK, message = "SUCCESS", data = (object?)null, timestamp = DateTime.UtcNow });
        }

        [HttpGet("payment/{orderId:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<PublicPaymentDTO>> GetPayment(int orderId, [FromQuery] string trackingNumber)
        {
            var payment = await _paymentService.GetPublicByOrderAsync(orderId, trackingNumber);
            return Ok(payment);
        }
    }
}
