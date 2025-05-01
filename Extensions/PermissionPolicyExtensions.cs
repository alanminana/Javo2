// Extensions/PermissionPolicyExtensions.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;

namespace Javo2.Extensions
{
    public static class PermissionPolicyExtensions
    {
        public static IServiceCollection AddPermissionPolicies(this IServiceCollection services)
        {
            // Define la estructura de permisos
            var actionPermissions = new Dictionary<string, string[]>
            {
                {"ver", new[] {"Index", "Details", "Get", "List"}},
                {"crear", new[] {"Create", "Add", "New"}},
                {"editar", new[] {"Edit", "Update", "Modify"}},
                {"eliminar", new[] {"Delete", "Remove"}}
            };

            // Lista de controladores
            var controllers = new[]
            {
                "Usuarios", "Roles", "Permisos", "Ventas", "Productos",
                "Clientes", "Reportes", "Configuracion", "Proveedores",
                "Promociones", "DevolucionGarantia", "Catalogo", "SecurityDashboard",
                "SecurityTools", "Auditoria", "Perfil"
            };

            // Registro dinámico de políticas
            services.AddAuthorization(options =>
            {
                // Política genérica para todos los permisos
                options.AddPolicy("PermisoPolitica", policy =>
                {
                    policy.RequireAuthenticatedUser();
                });

                // Crear políticas para cada controlador y acción
                foreach (var controller in controllers)
                {
                    foreach (var action in actionPermissions)
                    {
                        string policyName = $"{controller.ToLower()}.{action.Key}";
                        options.AddPolicy($"Permission:{policyName}", policy =>
                            policy.RequireClaim("Permission", policyName));
                    }
                }

                // Añadir políticas especiales
                options.AddPolicy("Permission:ventas.autorizar", policy =>
                    policy.RequireClaim("Permission", "ventas.autorizar"));

                options.AddPolicy("Permission:ventas.rechazar", policy =>
                    policy.RequireClaim("Permission", "ventas.rechazar"));

                options.AddPolicy("Permission:productos.ajustarprecios", policy =>
                    policy.RequireClaim("Permission", "productos.ajustarprecios"));

                options.AddPolicy("Permission:reportes.exportar", policy =>
                    policy.RequireClaim("Permission", "reportes.exportar"));

                options.AddPolicy("Permission:auditoria.rollback", policy =>
                    policy.RequireClaim("Permission", "auditoria.rollback"));
            });

            return services;
        }
    }
}