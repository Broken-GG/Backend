using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Backend.Middleware
{
    /// <summary>
    /// Global error handling middleware to catch and format exceptions
    /// </summary>
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
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
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            HttpStatusCode statusCode = HttpStatusCode.InternalServerError;
            string message = "An internal server error occurred";
            string? details = null;

            // Customize status codes and messages based on exception type
            switch (exception)
            {
                case ArgumentNullException:
                    statusCode = HttpStatusCode.BadRequest;
                    message = exception.Message;
                    break;
                case ArgumentException:
                    statusCode = HttpStatusCode.BadRequest;
                    message = exception.Message;
                    break;
                case KeyNotFoundException:
                    statusCode = HttpStatusCode.NotFound;
                    message = exception.Message;
                    break;
                case UnauthorizedAccessException:
                    statusCode = HttpStatusCode.Unauthorized;
                    message = "Unauthorized access";
                    break;
                case HttpRequestException httpEx:
                    statusCode = HttpStatusCode.BadGateway;
                    message = "External API error";
                    details = httpEx.Message;
                    break;
                default:
                    details = exception.Message;
                    break;
            }

            object response = new
            {
                success = false,
                error = new
                {
                    message,
                    details,
                    statusCode = (int)statusCode,
                    timestamp = DateTime.UtcNow
                }
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            return context.Response.WriteAsync(JsonConvert.SerializeObject(response));
        }
    }
}
