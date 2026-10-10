namespace AgitekBackend.DTOs.Cloudinary
{
    public class CloudinarySignatureDTO
    {
        public string Signature { get; set; } = string.Empty;
        public long Timestamp { get; set; }
        public string ApiKey { get; set; } = string.Empty;
        public string CloudName { get; set; } = string.Empty;
        public string Folder { get; set; } = string.Empty;
    }
}
