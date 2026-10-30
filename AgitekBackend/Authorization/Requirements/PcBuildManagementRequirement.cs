using Microsoft.AspNetCore.Authorization;

namespace AgitekBackend.Authorization.Requirements
{
    public enum PcBuildOperation
    {
        ReadAll,
        Read,
        Create,
        Update,
        Delete
    }

    public class PcBuildManagementRequirement : IAuthorizationRequirement
    {
        public PcBuildOperation Operation { get; }

        public PcBuildManagementRequirement(PcBuildOperation operation)
        {
            Operation = operation;
        }
    }
}
