namespace AgitekBackend.DTOs.Locations
{
    public class UpdateLocationDTO
    {
        public string? LocationCode { get; set; }
        public string? LocationName { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public bool? IsWarehouse { get; set; }
    }
}
