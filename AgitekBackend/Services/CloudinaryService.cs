using AgitekBackend.Common.Exceptions;
using AgitekBackend.DTOs.Cloudinary;
using AgitekBackend.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace AgitekBackend.Services
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly IConfiguration _configuration;

        public CloudinaryService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public CloudinarySignatureDTO GetUploadSignature(string folder)
        {
            var apiSecret = _configuration["Cloudinary:ApiSecret"];
            var apiKey = _configuration["Cloudinary:ApiKey"];
            var cloudName = _configuration["Cloudinary:CloudName"];

            if (string.IsNullOrEmpty(apiSecret) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(cloudName))
                throw new BadRequestException("Cloudinary is not configured");

            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var paramsToSign = $"folder={folder}&timestamp={timestamp}{apiSecret}";

            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(paramsToSign));
            var signature = Convert.ToHexString(hashBytes).ToLowerInvariant();

            return new CloudinarySignatureDTO
            {
                Signature = signature,
                Timestamp = timestamp,
                ApiKey = apiKey,
                CloudName = cloudName,
                Folder = folder
            };
        }
    }
}
