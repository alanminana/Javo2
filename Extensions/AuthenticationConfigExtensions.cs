// Extensions/AuthenticationConfigExtensions.cs
using Javo2.IServices.Authentication;
using Javo2.Services.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Javo2.Extensions
{
    public static class AuthenticationConfigExtensions
    {
        public static IServiceCollection AddAuthenticationServices(this IServiceCollection services)
        {
            // Registrar servicios de autenticación
            services.AddScoped<IUsuarioService, UsuarioService>();
            services.AddScoped<IRolService, RolService>();
            services.AddScoped<IPermisoService, PermisoService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IResetPasswordService, ResetPasswordService>();
            services.AddScoped<IPermissionManagerService, PermissionManagerService>();

            // Configurar autenticación con cookies
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

            // Configurar políticas de autorización
            services.AddAuthorization(options =>
            {
                // Política base para todos los permisos
                options.AddPolicy("PermisoPolitica", policy =>
                {
                    policy.RequireAuthenticatedUser();
                });

                // Definir políticas para cada módulo
                DefineModulePolicies(options, "usuarios");
                DefineModulePolicies(options, "roles");
                DefineModulePolicies(options, "permisos");
                DefineModulePolicies(options, "ventas");
                DefineModulePolicies(options, "productos");
                DefineModulePolicies(options, "clientes");
                DefineModulePolicies(options, "reportes");
                DefineModulePolicies(options, "configuracion");
                DefineModulePolicies(options, "proveedores");
                DefineModulePolicies(options, "promociones");
                DefineModulePolicies(options, "devolucionGarantia");
                DefineModulePolicies(options, "catalogo");
                DefineModulePolicies(options, "ajustePrecios");
                DefineModulePolicies(options, "auditoria");
                DefineModulePolicies(options, "diagnostic");
                DefineModulePolicies(options, "perfil");
                DefineModulePolicies(options, "configuracionInicial");

                // Permisos especiales para ventas
                options.AddPolicy("Permission:ventas.autorizar", policy => policy.RequireClaim("Permission", "ventas.autorizar"));
                options.AddPolicy("Permission:ventas.rechazar", policy => policy.RequireClaim("Permission", "ventas.rechazar"));
                options.AddPolicy("Permission:ventas.entrega", policy => policy.RequireClaim("Permission", "ventas.entrega"));
                options.AddPolicy("Permission:ventas.reimprimir", policy => policy.RequireClaim("Permission", "ventas.reimprimir"));
                options.AddPolicy("Permission:ventas.entregaProductos", policy => policy.RequireClaim("Permission", "ventas.entregaProductos"));
                options.AddPolicy("Permission:ventas.autorizaciones", policy => policy.RequireClaim("Permission", "ventas.autorizaciones"));

                // Permisos especiales para DevolucionGarantia
                options.AddPolicy("Permission:devolucionGarantia.procesar", policy => policy.RequireClaim("Permission", "devolucionGarantia.procesar"));
                options.AddPolicy("Permission:devolucionGarantia.enviarGarantia", policy => policy.RequireClaim("Permission", "devolucionGarantia.enviarGarantia"));
                options.AddPolicy("Permission:devolucionGarantia.completarGarantia", policy => policy.RequireClaim("Permission", "devolucionGarantia.completarGarantia"));

                // Permisos especiales para auditoria
                options.AddPolicy("Permission:auditoria.rollback", policy => policy.RequireClaim("Permission", "auditoria.rollback"));

                // Permisos especiales para productos
                options.AddPolicy("Permission:productos.ajustarprecios", policy => policy.RequireClaim("Permission", "productos.ajustarprecios"));
            });

            return services;
        }

        public static IApplicationBuilder UseAuthenticationConfig(this IApplicationBuilder app)
        {
            app.UseAuthentication();
            app.UseAuthorization();
            return app;
        }

        private static void DefineModulePolicies(AuthorizationOptions options, string module)
        {
            // Políticas CRUD estándar
            options.AddPolicy($"Permission:{module}.ver", policy => policy.RequireClaim("Permission", $"{module}.ver"));
            options.AddPolicy($"Permission:{module}.crear", policy => policy.RequireClaim("Permission", $"{module}.crear"));
            options.AddPolicy($"Permission:{module}.editar", policy => policy.RequireClaim("Permission", $"{module}.editar"));
            options.AddPolicy($"Permission:{module}.eliminar", policy => policy.RequireClaim("Permission", $"{module}.eliminar"));
        }
    }
}