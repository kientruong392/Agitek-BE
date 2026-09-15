using AgitekBackend.Common;

namespace AgitekBackend.DTOs.Medias
{
    public class CreateMediaDTO
    {
        public string MediaUrl { get; set; } = string.Empty;
        public MediaType MediaType { get; set; }
        public int? ReviewId { get; set; }
        public int? WarrantyId { get; set; }
    }
}


