using System.ComponentModel.DataAnnotations;

namespace AgitekBackend.DTOs.Auths
{
    public class CreateGuestUserDTO
    {
        [Required(ErrorMessage = "Full name is required")]
        public string FullName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^(?:\+84|0)(3[2-9]|5[2689]|7[06-9]|8[1-9]|9[0-9])[0-9]{7}$", ErrorMessage = "Invalid phone number format")]
        public string PhoneNumber { get; set; } = string.Empty;
        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; } = string.Empty;
    }
}
