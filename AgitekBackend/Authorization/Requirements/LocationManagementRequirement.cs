using Microsoft.AspNetCore.Authorization;

namespace AgitekBackend.Authorization.Requirements
{
    public enum LocationOperation
    {
        Read,
        Create,
        Update,
        Delete
    }

    public class LocationManagementRequirement : IAuthorizationRequirement
    {
        public LocationOperation Operation { get; }

        public LocationManagementRequirement(LocationOperation operation)
        {
            Operation = operation;
        }
    }
}
