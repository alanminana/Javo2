using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;

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