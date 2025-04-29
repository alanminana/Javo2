using Javo2.IServices.Authentication;
using Javo2.Services.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Javo2.Extensions
{
    public static class AuthenticationExtensions
    {
        /// <summary>
        /// Registra servicios de autenticación, cookie auth y políticas de permisos.
        /// </summary>
        public static IServiceCollection AddAuthenticationServices(this IServiceCollection services)
        {
            // 1) Servicios de dominio
            services.AddScoped<IUsuarioService, UsuarioService>();
            services.AddScoped<IRolService, RolService>();
            services.AddScoped<IPermisoService, PermisoService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IResetPasswordService, ResetPasswordService>();

            // 2) Cookie Authentication
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Auth/Login";
                    options.LogoutPath = "/Auth/Logout";
                    options.AccessDeniedPath = "/Auth/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromHours(3);
                    options.SlidingExpiration = true;
                    options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
                });

            // 3) Políticas de autorización
            services.AddAuthorization(options =>
            {
                options.AddPolicy("PermisoPolitica", policy =>
                    policy.RequireAuthenticatedUser());

                CreatePermissionPolicies(options, new[]
                {
                    "usuarios.ver","usuarios.crear","usuarios.editar","usuarios.eliminar",
                    "roles.ver","roles.crear","roles.editar","roles.eliminar",
                    "permisos.ver","permisos.crear","permisos.editar","permisos.eliminar",
                    "ventas.ver","ventas.crear","ventas.editar","ventas.eliminar",
                    "ventas.autorizar","ventas.rechazar",
                    "productos.ver","productos.crear","productos.editar","productos.eliminar",
                    "productos.ajustarprecios",
                    "clientes.ver","clientes.crear","clientes.editar","clientes.eliminar",
                    "reportes.ver","reportes.exportar",
                    "configuracion.ver","configuracion.editar"
                });
            });

            return services;
        }

        /// <summary>
        /// Inserta la autenticación y autorización en el pipeline.
        /// </summary>
        public static IApplicationBuilder UseAuthenticationConfig(this IApplicationBuilder app)
        {
            app.UseAuthentication();
            app.UseAuthorization();
            return app;
        }

        private static void CreatePermissionPolicies(AuthorizationOptions options, string[] codes)
        {
            foreach (var code in codes)
            {
                options.AddPolicy($"Permission:{code}", policy =>
                    policy.RequireClaim("Permission", code));
            }
        }
    }
}
