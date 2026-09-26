using Microsoft.AspNetCore.Authorization;

namespace AgitekBackend.Authorization.Requirements
{
    public enum ProductOperation
    {
        Read,
        Create,
        Update,
        Delete,
        ChangeStatus,
        UpdateQuantity,
        UpdateInitialQuantity,
        UpdateStockQuantity,
        SoftDelete,
        HardDelete
    }

    public class ProductManagementRequirement : IAuthorizationRequirement
    {
        public ProductOperation Operation { get; }

        public ProductManagementRequirement(ProductOperation operation)
        {
            Operation = operation;
        }
    }
}
