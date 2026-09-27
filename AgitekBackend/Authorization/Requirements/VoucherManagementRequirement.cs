using Microsoft.AspNetCore.Authorization;

namespace AgitekBackend.Authorization.Requirements
{
    public enum VoucherOperation
    {
        Read,
        Create,
        Update
    }

    public class VoucherManagementRequirement : IAuthorizationRequirement
    {
        public VoucherOperation Operation { get; }

        public VoucherManagementRequirement(VoucherOperation operation)
        {
            Operation = operation;
        }
    }
}
