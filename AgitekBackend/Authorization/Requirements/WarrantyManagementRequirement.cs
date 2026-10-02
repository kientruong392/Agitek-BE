using Microsoft.AspNetCore.Authorization;

namespace AgitekBackend.Authorization.Requirements
{
    public enum WarrantyOperation
    {
        Create,
        Read,
        ReadAll,
        Update,
        Delete,
        Approve
    }

    public class WarrantyManagementRequirement : IAuthorizationRequirement
    {
        public WarrantyOperation Operation { get; }

        public WarrantyManagementRequirement(WarrantyOperation operation)
        {
            Operation = operation;
        }
    }
}
