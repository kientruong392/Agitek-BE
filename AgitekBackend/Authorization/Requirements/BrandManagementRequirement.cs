using Microsoft.AspNetCore.Authorization;

namespace AgitekBackend.Authorization.Requirements
{
    public enum BrandOperation
    {
        Read,
        Create,
        Update,
        Delete
    }

    public class BrandManagementRequirement : IAuthorizationRequirement
    {
        public BrandOperation Operation { get; }

        public BrandManagementRequirement(BrandOperation operation)
        {
            Operation = operation;
        }
    }
}
