using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgitekBackend.Models
{
    public class Location
    {
        [Key]
        public int Id { get; set; }
        [Required, MaxLength(30)]
        public string LocationCode { get; set; } = string.Empty;
        [Required, MaxLength(30)]
        public string LocationName { get; set; } = string.Empty;
        [Column(TypeName = "decimal(10, 8)")]
        public decimal Latitude { get; set; }
        [Column(TypeName = "decimal(11, 8)")]
        public decimal Longitude { get; set; }
        public int? ParentId { get; set; }
        public Location? Parent { get; set; }
        public bool IsWarehouse { get; set; } = false;
        [MaxLength(255)]
        public string? AddressLine { get; set; }
        public ICollection<Location>? Children { get; set; } = new List<Location>();
    }
}
