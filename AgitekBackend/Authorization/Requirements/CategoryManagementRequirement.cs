using Microsoft.AspNetCore.Authorization;

namespace AgitekBackend.Authorization.Requirements
{
    public enum CategoryOperation
    {
        Read,
        Create,
        Update,
        Delete
    }

    public class CategoryManagementRequirement : IAuthorizationRequirement
    {
        public CategoryOperation Operation { get; }

        public CategoryManagementRequirement(CategoryOperation operation)
        {
            Operation = operation;
        }
    }
}
