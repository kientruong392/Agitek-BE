using Microsoft.AspNetCore.Http;

namespace AgitekBackend.Common.Exceptions
{
    public class BadRequestException : AppException
    {
        public BadRequestException(string message, string errorCode = "BAD_REQUEST") 
            : base(StatusCodes.Status400BadRequest, message, errorCode)
        {
        }
    }
}
