// Filters/GlobalExceptionFilter.cs
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net;

namespace Javo2.Filters
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IModelMetadataProvider _modelMetadataProvider;
        private readonly ILogger<GlobalExceptionFilter> _logger;

        public GlobalExceptionFilter(
            IWebHostEnvironment hostingEnvironment,
            IModelMetadataProvider modelMetadataProvider,
            ILogger<GlobalExceptionFilter> logger)
        {
            _hostingEnvironment = hostingEnvironment;
            _modelMetadataProvider = modelMetadataProvider;
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            // Log the exception
            LogException(context);

            // Handle AJAX requests
            if (IsAjaxRequest(context.HttpContext.Request))
            {
                HandleAjaxException(context);
                return;
            }

            // Handle API requests
            if (IsApiRequest(context))
            {
                HandleApiException(context);
                return;
            }

            // Handle regular requests
            HandleRegularException(context);
        }

        private void LogException(ExceptionContext context)
        {
            var exception = context.Exception;
            var request = context.HttpContext.Request;
            var controllerName = context.RouteData.Values["controller"]?.ToString() ?? "Unknown";
            var actionName = context.RouteData.Values["action"]?.ToString() ?? "Unknown";

            _logger.LogError(exception,
                "Excepción no manejada en {Controller}.{Action}: {Message}. Ruta: {Path}",
                controllerName,
                actionName,
                exception.Message,
                request.Path);

            // Log inner exception if exists
            if (exception.InnerException != null)
            {
                _logger.LogError(exception.InnerException,
                    "Excepción interna: {Message}",
                    exception.InnerException.Message);
            }
        }

        private bool IsAjaxRequest(Microsoft.AspNetCore.Http.HttpRequest request)
        {
            return request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                   request.ContentType?.Contains("application/json") == true ||
                   request.Headers["Accept"]?.Contains("application/json") == true;
        }

        private bool IsApiRequest(ExceptionContext context)
        {
            return context.HttpContext.Request.Path.StartsWithSegments("/api") ||
                   context.ActionDescriptor.DisplayName?.Contains("Api") == true ||
                   context.ActionDescriptor.DisplayName?.Contains("API") == true;
        }

        private void HandleAjaxException(ExceptionContext context)
        {
            // For AJAX requests, return a JSON response
            context.Result = new JsonResult(new
            {
                success = false,
                message = GetUserFriendlyErrorMessage(context.Exception),
                exceptionType = context.Exception.GetType().Name,
                errorCode = GetErrorCode(context.Exception)
            })
            {
                StatusCode = (int)GetStatusCode(context.Exception)
            };

            context.ExceptionHandled = true;
        }

        private void HandleApiException(ExceptionContext context)
        {
            var problemDetails = new ProblemDetails
            {
                Status = (int)GetStatusCode(context.Exception),
                Title = GetErrorTitle(context.Exception),
                Detail = GetUserFriendlyErrorMessage(context.Exception),
                Instance = context.HttpContext.Request.Path
            };

            // Add additional info in development
            if (_hostingEnvironment.IsDevelopment())
            {
                problemDetails.Extensions["exception"] = new
                {
                    exceptionType = context.Exception.GetType().Name,
                    stackTrace = context.Exception.StackTrace
                };
            }

            context.Result = new ObjectResult(problemDetails)
            {
                StatusCode = problemDetails.Status
            };

            context.ExceptionHandled = true;
        }

        private void HandleRegularException(ExceptionContext context)
        {
            // For regular requests, show the error view
            var result = new ViewResult
            {
                ViewName = "Error",
                ViewData = new ViewDataDictionary(_modelMetadataProvider, context.ModelState)
                {
                    ["ErrorMessage"] = GetUserFriendlyErrorMessage(context.Exception),
                    ["ExceptionType"] = context.Exception.GetType().Name
                }
            };

            // Add exception details in development
            if (_hostingEnvironment.IsDevelopment())
            {
                result.ViewData["Exception"] = context.Exception;
                result.ViewData["StackTrace"] = context.Exception.StackTrace;
            }

            // Set TempData for success/error messages if applicable
            if (context.HttpContext.Items.TryGetValue("OperationResult", out var operationResult) &&
                operationResult is bool success)
            {
                var controller = context.Controller as Controller;
                if (controller != null)
                {
                    if (success)
                    {
                        controller.TempData["Success"] = context.HttpContext.Items.TryGetValue("SuccessMessage", out var successMessage)
                                                       ? successMessage?.ToString()
                                                       : "Operación completada correctamente.";
                    }
                    else
                    {
                        controller.TempData["Error"] = context.HttpContext.Items.TryGetValue("ErrorMessage", out var errorMessage)
                                                     ? errorMessage?.ToString()
                                                     : GetUserFriendlyErrorMessage(context.Exception);
                    }
                }
            }

            context.Result = result;
            context.ExceptionHandled = true;
        }

        private HttpStatusCode GetStatusCode(Exception exception)
        {
            // Map exception types to appropriate HTTP status codes
            return exception switch
            {
                ArgumentException => HttpStatusCode.BadRequest,
                InvalidOperationException => HttpStatusCode.BadRequest,
                UnauthorizedAccessException => HttpStatusCode.Unauthorized,
                KeyNotFoundException => HttpStatusCode.NotFound,
                NotImplementedException => HttpStatusCode.NotImplemented,
                _ => HttpStatusCode.InternalServerError
            };
        }

        private string GetErrorTitle(Exception exception)
        {
            // Map exception types to user-friendly titles
            return exception switch
            {
                ArgumentException => "Datos Inválidos",
                InvalidOperationException => "Operación Inválida",
                UnauthorizedAccessException => "Acceso No Autorizado",
                KeyNotFoundException => "Recurso No Encontrado",
                NotImplementedException => "Funcionalidad No Implementada",
                _ => "Error del Servidor"
            };
        }

        private string GetUserFriendlyErrorMessage(Exception exception)
        {
            // Give friendly messages for known exception types
            var message = exception switch
            {
                ArgumentException => exception.Message,
                InvalidOperationException => exception.Message,
                UnauthorizedAccessException => "No tiene permiso para realizar esta operación.",
                KeyNotFoundException => "El recurso solicitado no fue encontrado.",
                NotImplementedException => "Esta funcionalidad aún no está implementada.",
                _ => _hostingEnvironment.IsDevelopment()
                    ? exception.Message
                    : "Ha ocurrido un error en el servidor. Por favor, inténtelo nuevamente más tarde."
            };

            // Sanitize message to prevent XSS in the error view
            return System.Net.WebUtility.HtmlEncode(message);
        }

        private int GetErrorCode(Exception exception)
        {
            // Map exceptions to numeric error codes for client-side handling
            return exception switch
            {
                ArgumentException => 400100,
                InvalidOperationException => 400200,
                UnauthorizedAccessException => 401100,
                KeyNotFoundException => 404100,
                NotImplementedException => 501100,
                _ => 500100
            };
        }
    }
}