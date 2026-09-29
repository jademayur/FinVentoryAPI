using System.Net;
using System.Text.Json;

namespace FinVentoryAPI.Middleware
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
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
                _logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var (statusCode, message) = exception switch
            {
                KeyNotFoundException ex => (HttpStatusCode.NotFound, ex.Message),
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Unauthorized access."),
                ArgumentException ex => (HttpStatusCode.BadRequest, ex.Message),
                InvalidOperationException ex => (HttpStatusCode.BadRequest, ex.Message),
                Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException => (HttpStatusCode.Conflict, "The record was modified by another user. Please refresh and try again."),
                Microsoft.EntityFrameworkCore.DbUpdateException => (HttpStatusCode.BadRequest, "A database error occurred. Please try again."),
                _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred. Please contact support.")
            };

            context.Response.StatusCode = (int)statusCode;

            var response = new
            {
                success = false,
                message
            };

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }
}
