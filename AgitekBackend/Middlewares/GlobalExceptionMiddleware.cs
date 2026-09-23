using AgitekBackend.Common.Exceptions;
using Microsoft.AspNetCore.Http;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AgitekBackend.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            int statusCode;
            string message;
            string errorCode;
            object? errors = null;

            switch (exception)
            {
                case AppException appEx:
                    statusCode = appEx.StatusCode;
                    message = appEx.Message;
                    errorCode = appEx.ErrorCode;
                    break;

                case BadHttpRequestException badReq:
                    statusCode = StatusCodes.Status400BadRequest;
                    message = "Invalid request body";
                    errorCode = "INVALID_REQUEST_BODY";
                    break;

                case JsonException jsonEx:
                    statusCode = StatusCodes.Status400BadRequest;
                    message = "Invalid JSON format";
                    errorCode = "INVALID_JSON_FORMAT";
                    break;

                default:
                    _logger.LogError(exception, "Unhandled exception occurred");
                    statusCode = StatusCodes.Status500InternalServerError;
                    message = "Internal Server Error";
                    errorCode = "INTERNAL_SERVER_ERROR";
                    break;
            }

            context.Response.StatusCode = statusCode;

            var response = new
            {
                success = false,
                statusCode = statusCode,
                errorCode = errorCode,
                message = message,
                errors = errors,
                timestamp = DateTime.UtcNow
            };

            var options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            await context.Response.WriteAsJsonAsync(response, options);
        }
    }
}
