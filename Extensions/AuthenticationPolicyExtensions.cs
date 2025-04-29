using Javo2.IServices.Authentication;
using Javo2.Services.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Javo2.Extensions
{
    public static class AuthenticationPolicyExtensions
    {
        public static IServiceCollection AddAuthenticationServices(this IServiceCollection services)
        {
            // Registrar servicios
            services.AddScoped<IUsuarioService, UsuarioService>();
            services.AddScoped<IRolService, RolService>();
            services.AddScoped<IPermisoService, PermisoService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IResetPasswordService, ResetPasswordService>();

            // Configurar autenticación con cookies
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Auth/Login";
                    options.LogoutPath = "/Auth/Logout";
                    options.AccessDeniedPath = "/Auth/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromHours(3);
                    options.SlidingExpiration = true;
                });

            // Configurar políticas de autorización
            services.AddAuthorization(options =>
            {
                // Política para verificar permisos específicos
                options.AddPolicy("PermisoPolitica", policy =>
                {
                    policy.RequireAuthenticatedUser();
                });

                // Crear políticas para cada permiso común
                CreatePermissionPolicies(options, new[]
                {
                    "usuarios.ver", "usuarios.crear", "usuarios.editar", "usuarios.eliminar",
                    "roles.ver", "roles.crear", "roles.editar", "roles.eliminar",
                    "permisos.ver", "permisos.crear", "permisos.editar", "permisos.eliminar",
                    "ventas.ver", "ventas.crear", "ventas.editar", "ventas.eliminar", "ventas.autorizar", "ventas.rechazar",
                    "productos.ver", "productos.crear", "productos.editar", "productos.eliminar", "productos.ajustarprecios",
                    "clientes.ver", "clientes.crear", "clientes.editar", "clientes.eliminar",
                    "reportes.ver", "reportes.exportar",
                    "configuracion.ver", "configuracion.editar"
                });
            });

            return services;
        }

        private static void CreatePermissionPolicies(AuthorizationOptions options, string[] permissionCodes)
        {
            foreach (var code in permissionCodes)
            {
                options.AddPolicy($"Permission:{code}", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == "Permission" && c.Value == code)));
            }
        }

        public static IApplicationBuilder UseAuthenticationConfig(this IApplicationBuilder app)
        {
            app.UseAuthentication();
            app.UseAuthorization();

            return app;
        }
    }
}