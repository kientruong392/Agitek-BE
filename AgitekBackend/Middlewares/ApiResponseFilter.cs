using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Threading.Tasks;

namespace AgitekBackend.Middlewares
{
    public class ApiResponseFilter : IAsyncResultFilter
    {
        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if (context.Result is ObjectResult objectResult)
            {
                if (objectResult.StatusCode is >= 200 and < 300 || objectResult.StatusCode == null)
                {
                    var type = objectResult.Value?.GetType();
                    var isAlreadyWrapped = type != null && type.GetProperty("success") != null && type.GetProperty("data") != null;

                    if (!isAlreadyWrapped)
                    {
                        var response = new
                        {
                            success = true,
                            statusCode = objectResult.StatusCode ?? 200,
                            message = "SUCCESS",
                            data = objectResult.Value,
                            timestamp = DateTime.UtcNow
                        };
                        context.Result = new ObjectResult(response)
                        {
                            StatusCode = objectResult.StatusCode ?? 200
                        };
                    }
                }
            }
            else if (context.Result is EmptyResult || context.Result is OkResult)
            {
                var statusCode = context.Result is OkResult ? 200 : 204;
                var response = new
                {
                    success = true,
                    statusCode = statusCode,
                    message = "SUCCESS",
                    data = (object?)null,
                    timestamp = DateTime.UtcNow
                };
                context.Result = new ObjectResult(response)
                {
                    StatusCode = statusCode
                };
            }

            await next();
        }
    }
}
