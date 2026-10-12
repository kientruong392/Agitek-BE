using AgitekBackend.Common;

namespace AgitekBackend.DTOs.Locations
{
    public class LocationDTO
    {
        public int Id { get; set; }
        public string LocationCode { get; set; } = string.Empty;
        public string LocationName { get; set; } = string.Empty;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public bool IsWarehouse { get; set; }
        public LocationStatus? Status { get; set; }
    }
}