using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AgitekBackend.Common;

namespace AgitekBackend.Models
{
    public class Location
    {
        [Key]
        public int Id { get; set; }
        [Required, MaxLength(30)]
        public string LocationCode { get; set; } = string.Empty;
        [Required, MaxLength(255)]
        public string LocationName { get; set; } = string.Empty;
        [Column(TypeName = "decimal(10, 8)")]
        public decimal Latitude { get; set; }
        [Column(TypeName = "decimal(11, 8)")]
        public decimal Longitude { get; set; }
        public bool IsWarehouse { get; set; } = false;
        public LocationStatus? Status { get; set; }
    }
}
