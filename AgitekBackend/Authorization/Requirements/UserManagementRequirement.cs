using Microsoft.AspNetCore.Authorization;

namespace AgitekBackend.Authorization.Requirements
{
    public enum UserOperation
    {
        Read,
        Create,
        Update,
        Delete
    }

    public class UserManagementRequirement : IAuthorizationRequirement
    {
        public UserOperation Operation { get; }

        public UserManagementRequirement(UserOperation operation)
        {
            Operation = operation;
        }
    }
}
