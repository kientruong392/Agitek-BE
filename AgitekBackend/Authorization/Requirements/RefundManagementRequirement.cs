using Microsoft.AspNetCore.Authorization;

namespace AgitekBackend.Authorization.Requirements
{
    public enum RefundOperation
    {
        Create,
        Approve
    }

    public class RefundManagementRequirement : IAuthorizationRequirement
    {
        public RefundOperation Operation { get; }

        public RefundManagementRequirement(RefundOperation operation)
        {
            Operation = operation;
        }
    }
}
