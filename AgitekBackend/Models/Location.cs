using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgitekBackend.Models
{
    public class Location
    {
        [Key]
        public int Id { get; set; }
        [Required, MaxLength(3)]
        public string LocationCode { get; set; } = string.Empty;
        [MaxLength(50)]
        public string LocationName { get; set; } = string.Empty;
        [Column(TypeName = "decimal(10, 8)")]
        public double Latitude { get; set; }
        [Column(TypeName = "decimal(11, 8)")]
        public double Longitude { get; set; }
        public bool IsWarehouse { get; set; } = false;
    }
}
