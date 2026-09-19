using Microsoft.AspNetCore.Http;

namespace AgitekBackend.Common.Exceptions
{
    public class UnauthorizedException : AppException
    {
        public UnauthorizedException(string message, string errorCode = "UNAUTHORIZED")
            : base(StatusCodes.Status401Unauthorized, message, errorCode)
        {
        }
    }
}
