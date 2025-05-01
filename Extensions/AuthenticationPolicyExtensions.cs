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
        public static IServiceCollection AddAuthenticationPolicies(this IServiceCollection services)
        {
            // Configurar políticas de autorización
            services.AddAuthorization(options =>
            {
                // Excepciones para rutas que no requieren autenticación
                options.AddPolicy("AllowAnonymous", policy => policy.RequireAssertion(_ => true));

                // Política base para todos los permisos
                options.AddPolicy("PermisoPolitica", policy =>
                {
                    policy.RequireAuthenticatedUser();
                });
                options.AddPolicy("Permission:securitydashboard.ver", policy =>
            policy.RequireClaim("Permission", "securitydashboard.ver"));
                // Crear políticas para cada permiso específico
                CreatePermissionPolicies(options, new[]
                {
                    "usuarios.ver", "usuarios.crear", "usuarios.editar", "usuarios.eliminar",
                    "roles.ver", "roles.crear", "roles.editar", "roles.eliminar",
                    "permisos.ver", "permisos.crear", "permisos.editar", "permisos.eliminar",
                    "ventas.ver", "ventas.crear", "ventas.editar", "ventas.eliminar",
                    "ventas.autorizar", "ventas.rechazar",
                    "productos.ver", "productos.crear", "productos.editar", "productos.eliminar",
                    "productos.ajustarprecios",
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
                    policy.RequireClaim("Permission", code));
            }
        }
    }
}