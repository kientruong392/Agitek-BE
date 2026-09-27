using Microsoft.AspNetCore.Authorization;

namespace AgitekBackend.Authorization.Requirements
{
    public enum AddressOperation
    {
        Read,
        Create,
        Update,
        Delete
    }

    public class AddressManagementRequirement : IAuthorizationRequirement
    {
        public AddressOperation Operation { get; }

        public AddressManagementRequirement(AddressOperation operation)
        {
            Operation = operation;
        }
    }
}
