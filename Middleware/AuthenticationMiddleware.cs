using Javo2.IServices.Authentication;

public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthenticationMiddleware> _logger;
    private readonly string[] _allowedPaths = new[] {
        "/Auth/Login",
        "/Auth/Logout",
        "/Auth/AccessDenied",
        "/ResetPassword/OlvideContraseña",
        "/ResetPassword/ResetearContraseña",
        "/ResetPassword/TokenInvalido",
        "/ResetPassword/ResetExitoso",
        "/ConfiguracionInicial",
        "/ConfiguracionInicial/Index",
        "/css/",
        "/js/",
        "/lib/",
        "/img/"
    };

    public AuthenticationMiddleware(RequestDelegate next, ILogger<AuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Rutas raíz o vacías
            if (string.IsNullOrEmpty(context.Request.Path.Value) || context.Request.Path.Value == "/")
            {
                var usuarioService = context.RequestServices.GetRequiredService<IUsuarioService>();
                var usuarios = await usuarioService.GetAllUsuariosAsync();

                if (!usuarios.Any())
                {
                    context.Response.Redirect("/ConfiguracionInicial");
                    return;
                }

                // Si ya hay usuarios, pasar al siguiente middleware
                await _next(context);
                return;
            }

            // Rutas públicas
            if (_allowedPaths.Any(p => context.Request.Path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase)))
            {
                await _next(context);
                return;
            }

            // Verificación de autenticación
            if (context.User?.Identity?.IsAuthenticated != true)
            {
                context.Response.Redirect("/Auth/Login");
                return;
            }

            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error en middleware de autenticación. Ruta: {context.Request.Path}");
            context.Response.Redirect("/Auth/Login");
        }
    }
}