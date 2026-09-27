using System.ComponentModel.DataAnnotations;

namespace AgitekBackend.Models
{
    public class Location
    {
        [Key]
        public int Id { get; set; }
        [Required, MaxLength(50)]
        public string LocationCode { get; set; } = string.Empty;
        [MaxLength(255)]
        public string LocationName { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool IsWarehouse { get; set; } = false;
        public ICollection<OrderTrackingLog> OrderTrackingLogs { get; set; } = new List<OrderTrackingLog>();
    }
}
