using Microsoft.AspNetCore.Authorization;

namespace AgitekBackend.Authorization.Requirements
{
    public enum ReviewOperation
    {
        Create,
        Update,
        Reply
    }

    public class ReviewManagementRequirement : IAuthorizationRequirement
    {
        public ReviewOperation Operation { get; }

        public ReviewManagementRequirement(ReviewOperation operation)
        {
            Operation = operation;
        }
    }
}
