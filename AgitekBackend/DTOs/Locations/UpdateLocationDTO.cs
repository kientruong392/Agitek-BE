using System.ComponentModel.DataAnnotations;

namespace AgitekBackend.DTOs.Locations
{
    public class UpdateLocationDTO
    {
        [MaxLength(3, ErrorMessage = "Location code cannot exceed 3 characters")]
        public string? LocationCode { get; set; } = string.Empty;
        [MaxLength(50, ErrorMessage = "Location name cannot exceed 50 characters")]
        public string? LocationName { get; set; } = string.Empty;
        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
        public double? Latitude { get; set; }
        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
        public double? Longitude { get; set; }
        public bool? IsWarehouse { get; set; }
    }
}