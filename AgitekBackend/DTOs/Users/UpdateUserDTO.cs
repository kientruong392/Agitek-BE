using AgitekBackend.Common;
using System.ComponentModel.DataAnnotations;

namespace AgitekBackend.DTOs.Users
{
    public class UpdateUserRoleDTO
    {
        [Required]
        public RoleName Role { get; set; }
    }

    public class UpdateUserStatusDTO
    {
        [Required]
        public bool IsActive { get; set; }
    }
}
