// Filters/PermissionActionFilter.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using Javo2.Helpers;

namespace Javo2.Filters
{
    public class PermissionSeeder : IActionFilter
    {
        private readonly ILogger<PermissionSeeder> _logger;

        public PermissionSeeder(ILogger<PermissionSeeder> logger)
        {
            _logger = logger;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.HttpContext.User.Identity.IsAuthenticated)
                return; // La autenticación se maneja en otro lugar

            // Obtener el controlador y la acción
            var controllerActionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
            if (controllerActionDescriptor == null)
                return;

            string controllerName = controllerActionDescriptor.ControllerName.ToLower();
            string actionName = controllerActionDescriptor.ActionName.ToLower();

            // Ignorar controladores específicos
            if (controllerName == "auth" || controllerName == "home" ||
                controllerName == "resetpassword" || controllerName == "configuracioninicial")
                return;

            // Determinar el permiso requerido según la acción
            string requiredPermission = null;

            // Verificar si ya tiene un atributo de autorización de permiso
            var hasPermissionAttribute = controllerActionDescriptor.MethodInfo
                .GetCustomAttributes(inherit: true)
                .Any(a => a.GetType().Name == "RequirePermissionAttribute" ||
                         (a.GetType().Name == "AuthorizeAttribute" &&
                          a.GetType().GetProperty("Policy")?.GetValue(a)?.ToString().StartsWith("Permission:") == true));

            if (hasPermissionAttribute)
                return; // Ya hay una validación de permiso

            // Determinar el permiso según la acción
            if (actionName.StartsWith("edit") || actionName.StartsWith("update"))
            {
                requiredPermission = $"{controllerName}.editar";
            }
            else if (actionName.StartsWith("delet") || actionName.StartsWith("remove"))
            {
                requiredPermission = $"{controllerName}.eliminar";
            }
            else if (actionName.StartsWith("creat") || actionName.StartsWith("add") || actionName.StartsWith("new"))
            {
                requiredPermission = $"{controllerName}.crear";
            }
            else if (actionName.StartsWith("detail") || actionName.StartsWith("view") || actionName.Equals("index"))
            {
                requiredPermission = $"{controllerName}.ver";
            }

            // Si se determinó un permiso y el usuario no lo tiene, denegar acceso
            if (!string.IsNullOrEmpty(requiredPermission) &&
                !context.HttpContext.User.HasPermission(requiredPermission))
            {
                _logger.LogWarning("Acceso denegado: Usuario {User} intenta acceder a {Controller}/{Action} " +
                                  "sin permiso {Permission}",
                                  context.HttpContext.User.Identity.Name,
                                  controllerName, actionName, requiredPermission);

                context.Result = new RedirectToActionResult("AccessDenied", "Auth", null);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // No se requiere ninguna acción después de ejecutar la acción
        }
    }
}