// Middleware/ExceptionHandlingMiddleware.cs
using Javo2.Filters.ExceptionHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Javo2.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _environment;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger,
            IHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
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
            // Log the exception
            _logger.LogError(exception,
                "Excepción no manejada: {Message}. Ruta: {Path}",
                exception.Message,
                context.Request.Path);

            // Set appropriate status code
            var statusCode = exception switch
            {
                BusinessException => (int)HttpStatusCode.BadRequest,
                ValidationException => (int)HttpStatusCode.BadRequest,
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                KeyNotFoundException => (int)HttpStatusCode.NotFound,
                NotImplementedException => (int)HttpStatusCode.NotImplemented,
                _ => (int)HttpStatusCode.InternalServerError
            };

            context.Response.StatusCode = statusCode;

            // Prepare the response
            var response = new
            {
                statusCode,
                message = GetUserFriendlyErrorMessage(exception),
                detailedMessage = _environment.IsDevelopment() ? exception.ToString() : null,
                errors = exception is ValidationException validationEx ? validationEx.ValidationErrors : null
            };

            // Write the response
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }

        private string GetUserFriendlyErrorMessage(Exception exception)
        {
            return exception switch
            {
                BusinessException businessEx => businessEx.UserErrorMessage,
                ValidationException => "Se encontraron errores de validación.",
                UnauthorizedAccessException => "No tiene permiso para realizar esta operación.",
                KeyNotFoundException => "El recurso solicitado no fue encontrado.",
                NotImplementedException => "Esta funcionalidad aún no está implementada.",
                _ => _environment.IsDevelopment()
                    ? exception.Message
                    : "Ha ocurrido un error en el servidor. Por favor, inténtelo nuevamente más tarde."
            };
        }
    }
}