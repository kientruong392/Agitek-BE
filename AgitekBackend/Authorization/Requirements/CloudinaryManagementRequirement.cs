using Microsoft.AspNetCore.Authorization;

namespace AgitekBackend.Authorization.Requirements
{
    public enum CloudinaryOperation
    {
        Upload,
        UploadOrderProof
    }

    public class CloudinaryManagementRequirement : IAuthorizationRequirement
    {
        public CloudinaryOperation Operation { get; }

        public CloudinaryManagementRequirement(CloudinaryOperation operation)
        {
            Operation = operation;
        }
    }
}
