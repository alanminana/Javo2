// Archivo: Filters/ClientesExceptionFilter.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;

namespace Javo2.Filters
{
    public class ClientesExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<ClientesExceptionFilter> _logger;

        public ClientesExceptionFilter(ILogger<ClientesExceptionFilter> logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            _logger.LogError(context.Exception, "Error en {Controller}/{Action}: {Message}",
                context.RouteData.Values["controller"],
                context.RouteData.Values["action"],
                context.Exception.Message);

            // Determinar el mensaje a mostrar
            string mensaje = "Ha ocurrido un error al procesar la solicitud.";

            if (context.Exception is KeyNotFoundException)
            {
                mensaje = "El registro solicitado no existe.";
                context.Result = new NotFoundObjectResult(mensaje);
            }
            else if (context.Exception is InvalidOperationException)
            {
                mensaje = context.Exception.Message;
                context.Result = new BadRequestObjectResult(mensaje);
            }
            else
            {
                // Guardar mensaje en TempData para mostrar en la vista
                if (context.HttpContext.Request.Method == "POST")
                {
                    var controller = context.Controller as Controller;
                    if (controller != null)
                    {
                        controller.TempData["Error"] = mensaje;
                        context.Result = new RedirectToActionResult("Index", "Clientes", null);
                    }
                }
                else
                {
                    context.Result = new ViewResult { ViewName = "Error" };
                }
            }

            context.ExceptionHandled = true;
        }
    }
}