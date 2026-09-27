namespace AgitekBackend.DTOs.Locations
{
    public class CreateLocationDTO
    {
        public string LocationCode { get; set; } = string.Empty;
        public string LocationName { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool IsWarehouse { get; set; } = false;
    }
}
