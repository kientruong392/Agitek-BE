using AgitekBackend.Common;

namespace AgitekBackend.DTOs.Medias
{
    public class UpdateMediaDTO
    {
        public string? MediaUrl { get; set; } 
        public MediaType? MediaType { get; set; }
        public int? ReviewId { get; set; }
        public int? WarrantyId { get; set; }
    }
}


