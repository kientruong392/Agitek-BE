namespace AgitekBackend.DTOs.LocationCoordinates
{
    public class UpdateLocationCoordinatesDTO
    {
        public string? LocationCode { get; set; } 
        public string? LocationName { get; set; } 
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public bool? IsWarehouse { get; set; }
    }
}


