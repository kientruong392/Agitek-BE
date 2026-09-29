using Microsoft.AspNetCore.Authorization;

namespace AgitekBackend.Authorization.Requirements
{
    public enum OrderOperation
    {
        Read,
        Create,
        Update,
        ReadAll
    }

    public class OrderManagementRequirement : IAuthorizationRequirement
    {
        public OrderOperation Operation { get; }

        public OrderManagementRequirement(OrderOperation operation)
        {
            Operation = operation;
        }
    }
}
