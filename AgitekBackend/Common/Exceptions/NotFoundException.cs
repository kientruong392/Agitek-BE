using Microsoft.AspNetCore.Http;

namespace AgitekBackend.Common.Exceptions
{
    public class NotFoundException : AppException
    {
        public NotFoundException(string message, string errorCode = "NOT_FOUND") 
            : base(StatusCodes.Status404NotFound, message, errorCode)
        {
        }
    }
}
