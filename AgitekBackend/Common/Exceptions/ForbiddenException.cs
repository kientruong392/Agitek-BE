using Microsoft.AspNetCore.Http;

namespace AgitekBackend.Common.Exceptions
{
    public class ForbiddenException : AppException
    {
        public ForbiddenException(string message = "You do not have permission to access this resource", string errorCode = "FORBIDDEN")
            : base(StatusCodes.Status403Forbidden, message, errorCode)
        {
        }
    }
}
