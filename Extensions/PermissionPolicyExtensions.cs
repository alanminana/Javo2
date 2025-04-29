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
                "Promociones", "DevolucionGarantia", "Catalogo"
            };

            // Registro dinámico de políticas
            services.AddAuthorization(options =>
            {
                foreach (var controller in controllers)
                {
                    foreach (var action in actionPermissions)
                    {
                        string policyName = $"{controller.ToLower()}.{action.Key}";
                        options.AddPolicy(policyName, policy =>
                            policy.RequireClaim("Permission", policyName));
                    }
                }
            });

            return services;
        }
    }
}