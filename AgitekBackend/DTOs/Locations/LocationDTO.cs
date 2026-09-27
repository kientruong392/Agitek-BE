namespace AgitekBackend.DTOs.Locations
{
    public class LocationDTO
    {
        public int Id { get; set; }
        public string LocationCode { get; set; } = string.Empty;
        public string LocationName { get; set; } = string.Empty;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public int? ParentId { get; set; }
        public bool IsWarehouse { get; set; }
        public string? AddressLine { get; set; }
        public LocationDTO? Parent { get; set; }
        public ICollection<LocationDTO>? Children { get; set; }
    }
}