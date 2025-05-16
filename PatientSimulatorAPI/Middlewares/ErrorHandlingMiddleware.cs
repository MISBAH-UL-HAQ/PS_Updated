using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace PatientSimulatorAPI.Middlewares
{
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
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Bad Request: {Message}", ex.Message);
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await WriteErrorAsync(context, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Conflict: {Message}", ex.Message);
                context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                await WriteErrorAsync(context, ex.Message);
            }
            catch (Azure.RequestFailedException ex)
            {
                _logger.LogError(ex, "Upstream service error: {Message}", ex.Message);
                context.Response.StatusCode = (int)HttpStatusCode.BadGateway;
                await WriteErrorAsync(context, "Upstream service error: " + ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred.");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await WriteErrorAsync(context, "An unexpected error occurred.");
            }
        }

        private static Task WriteErrorAsync(HttpContext context, string message)
        {
            context.Response.ContentType = "application/json";
            var errorObj = JsonSerializer.Serialize(new { error = message });
            return context.Response.WriteAsync(errorObj);
        }
    }
}