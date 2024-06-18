using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Javo2.Middleware
{
    /// <summary>
    /// Middleware para manejar excepciones globales en la aplicación.
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        /// <summary>
        /// Inicializa una nueva instancia del middleware <see cref="ExceptionHandlingMiddleware"/>.
        /// </summary>
        /// <param name="next">El delegado de la siguiente solicitud.</param>
        /// <param name="logger">El logger para registrar errores.</param>
        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Invoca el middleware para manejar la solicitud HTTP.
        /// </summary>
        /// <param name="httpContext">El contexto de la solicitud HTTP.</param>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred");
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        /// <summary>
        /// Maneja la excepción y devuelve una respuesta de error en formato JSON.
        /// </summary>
        /// <param name="context">El contexto de la solicitud HTTP.</param>
        /// <param name="exception">La excepción que se produjo.</param>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var errorDetails = new ErrorDetails
            {
                StatusCode = context.Response.StatusCode,
                Message = exception.Message // Incluye el mensaje de la excepción para obtener más detalles
            };

            return context.Response.WriteAsync(errorDetails.ToString());
        }

        /// <summary>
        /// Clase para representar detalles de errores.
        /// </summary>
        public class ErrorDetails
        {
            /// <summary>
            /// Código de estado HTTP del error.
            /// </summary>
            public int StatusCode { get; set; }

            /// <summary>
            /// Mensaje de error.
            /// </summary>
            public string Message { get; set; }

            /// <summary>
            /// Convierte los detalles del error a una cadena JSON.
            /// </summary>
            /// <returns>Una cadena JSON que representa los detalles del error.</returns>
            public override string ToString()
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(this);
            }
        }
    }
}
