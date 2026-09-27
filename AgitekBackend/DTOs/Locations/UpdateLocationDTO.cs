using System.ComponentModel.DataAnnotations;

namespace AgitekBackend.DTOs.Locations
{
    public class UpdateLocationDTO
    {
        [MaxLength(30, ErrorMessage = "Location code cannot exceed 30 characters")]
        public string? LocationCode { get; set; }
        [MaxLength(30, ErrorMessage = "Location name cannot exceed 30 characters")]
        public string? LocationName { get; set; }
        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
        public decimal? Latitude { get; set; }
        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
        public decimal? Longitude { get; set; }
        public int? ParentId { get; set; }
        public bool? IsWarehouse { get; set; }
        [MaxLength(255, ErrorMessage = "Address line cannot exceed 255 characters")]
        public string? AddressLine { get; set; }
    }
}