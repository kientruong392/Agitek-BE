using AgitekBackend.DTOs.Cloudinary;

namespace AgitekBackend.Services.Interfaces
{
    public interface ICloudinaryService
    {
        CloudinarySignatureDTO GetUploadSignature(string folder);
    }
}
