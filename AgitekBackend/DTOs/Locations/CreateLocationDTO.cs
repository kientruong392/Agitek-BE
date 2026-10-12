using System.ComponentModel.DataAnnotations;
using AgitekBackend.Common;

namespace AgitekBackend.DTOs.Locations
{
    public class CreateLocationDTO
    {
        [Required(ErrorMessage = "Location code cannot be empty")]
        [RegularExpression(@"^[A-Z]{2}[0-9]{2}$", ErrorMessage = "Location code must consist of 2 uppercase letters followed by 2 digits (e.g. HN01, SG02)")]
        public string LocationCode { get; set; } = string.Empty;
        [Required(ErrorMessage = "Location name cannot be empty")]
        [MaxLength(255, ErrorMessage = "Location name cannot exceed 255 characters")]
        public string LocationName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Latitude cannot be empty")]
        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
        public decimal Latitude { get; set; }
        [Required(ErrorMessage = "Longitude cannot be empty")]
        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
        public decimal Longitude { get; set; }
        public bool IsWarehouse { get; set; } = false;
        public LocationStatus? Status { get; set; }
    }
}