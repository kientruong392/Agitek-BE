using System.ComponentModel.DataAnnotations;

namespace AgitekBackend.DTOs.PcBuilds
{
    public class SavePcBuildDTO
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        [Required, MaxLength(8)]
        public List<int> ProductIds { get; set; } = new();
    }
}
