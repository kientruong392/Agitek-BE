namespace AgitekBackend.DTOs.ProductSpecifications
{
    public class ProductSpecificationDTO
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string SpecName { get; set; } = string.Empty;
        public string SpecValue { get; set; } = string.Empty;
    }
}


