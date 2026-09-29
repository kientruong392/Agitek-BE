using Microsoft.AspNetCore.Authorization;

namespace AgitekBackend.Authorization.Requirements
{
    public enum PaymentOperation
    {
        Read,
        Create
    }

    public class PaymentManagementRequirement : IAuthorizationRequirement
    {
        public PaymentOperation Operation { get; }

        public PaymentManagementRequirement(PaymentOperation operation)
        {
            Operation = operation;
        }
    }
}
