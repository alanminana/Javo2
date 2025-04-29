// Middleware/AuthenticationMiddleware.cs
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using System;

namespace Javo2.Middleware
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string[] _allowedPaths = new[] {
            "/Auth/Login",
            "/Auth/Logout",
            "/Auth/AccessDenied",
            "/ResetPassword/OlvideContraseña",
            "/ResetPassword/ResetearContraseña",
            "/ResetPassword/TokenInvalido",
            "/ResetPassword/ResetExitoso",
            "/ConfiguracionInicial",
            "/css/",
            "/js/",
            "/lib/",
            "/img/"
        };

        public AuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Permitir acceso a recursos públicos
            if (_allowedPaths.Any(p => context.Request.Path.StartsWithSegments(p)))
            {
                await _next(context);
                return;
            }

            // Verificar si el usuario está autenticado
            if (!context.User.Identity.IsAuthenticated)
            {
                // Redirigir a la página de login
                context.Response.Redirect("/Auth/Login");
                return;
            }

            // Para peticiones AJAX, verificar también la autenticación
            if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest" && !context.User.Identity.IsAuthenticated)
            {
                context.Response.StatusCode = 401; // Unauthorized
                return;
            }

            // Verificar permisos para acciones específicas
            string path = context.Request.Path.Value.ToLower();
            string controller = "";
            string action = "";

            // Extraer controller y action del path
            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length >= 1)
            {
                controller = segments[0].ToLower();
            }
            if (segments.Length >= 2)
            {
                action = segments[1].ToLower();
            }

            // Verificar permisos según el método y la acción
            if (!string.IsNullOrEmpty(controller) && !string.IsNullOrEmpty(action))
            {
                string requiredPermission = null;

                // Determinar el permiso necesario según la acción
                if (action.StartsWith("edit") || action.StartsWith("update"))
                {
                    requiredPermission = $"{controller}.editar";
                }
                else if (action.StartsWith("delet") || action.StartsWith("remove"))
                {
                    requiredPermission = $"{controller}.eliminar";
                }
                else if (action.StartsWith("creat") || action.StartsWith("add") || action.StartsWith("new"))
                {
                    requiredPermission = $"{controller}.crear";
                }
                else if (action.StartsWith("detail") || action.StartsWith("view") || action.Equals("index"))
                {
                    requiredPermission = $"{controller}.ver";
                }

                // Verificar si el usuario tiene el permiso necesario
                if (!string.IsNullOrEmpty(requiredPermission) &&
                    !context.User.HasClaim(c => c.Type == "Permission" && c.Value == requiredPermission))
                {
                    // AJAX request
                    if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        context.Response.StatusCode = 403; // Forbidden
                        return;
                    }

                    // Normal request
                    context.Response.Redirect("/Auth/AccessDenied");
                    return;
                }
            }

            // Continuar con la solicitud
            await _next(context);
        }
    }

    // Extensión para facilitar la configuración en Program.cs
    public static class AuthenticationMiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomAuthentication(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthenticationMiddleware>();
        }
    }
}