using Microsoft.AspNetCore.Http;

namespace AgitekBackend.Common.Exceptions
{
    public class ForbiddenException : AppException
    {
        public ForbiddenException(string message, string errorCode = "FORBIDDEN")
            : base(StatusCodes.Status403Forbidden, message, errorCode)
        {
        }
    }
}
