using AgitekBackend.Common;
using System.ComponentModel.DataAnnotations;

namespace AgitekBackend.DTOs.Users
{
    public class UpdateUserDTO
    {
        public RoleName? Role { get; set; }
        public bool? IsActive { get; set; }
    }
}
