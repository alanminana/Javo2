// Extensions/AuthenticationStartupExtensions.cs
using Javo2.IServices.Authentication;
using Javo2.Services.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Javo2.Extensions
{
    public static class AuthenticationStartupExtensions
    {
        public static IServiceCollection AddAuthenticationServices(this IServiceCollection services)
        {
            // Registrar servicios
            services.AddScoped<IUsuarioService, UsuarioService>();
            services.AddScoped<IRolService, RolService>();
            services.AddScoped<IPermisoService, PermisoService>();

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

                // Crear una política para cada permiso
                // Nota: Esto debe ser ampliado para incluir todos los permisos necesarios
                options.AddPolicy("Permission:roles.ver", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == "Permission" && c.Value == "roles.ver")));

                options.AddPolicy("Permission:roles.crear", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == "Permission" && c.Value == "roles.crear")));

                options.AddPolicy("Permission:roles.editar", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == "Permission" && c.Value == "roles.editar")));

                options.AddPolicy("Permission:roles.eliminar", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == "Permission" && c.Value == "roles.eliminar")));

                options.AddPolicy("Permission:usuarios.ver", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == "Permission" && c.Value == "usuarios.ver")));

                options.AddPolicy("Permission:usuarios.crear", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == "Permission" && c.Value == "usuarios.crear")));

                options.AddPolicy("Permission:usuarios.editar", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == "Permission" && c.Value == "usuarios.editar")));

                options.AddPolicy("Permission:usuarios.eliminar", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == "Permission" && c.Value == "usuarios.eliminar")));

                options.AddPolicy("Permission:permisos.ver", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == "Permission" && c.Value == "permisos.ver")));

                options.AddPolicy("Permission:permisos.crear", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == "Permission" && c.Value == "permisos.crear")));

                options.AddPolicy("Permission:permisos.editar", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == "Permission" && c.Value == "permisos.editar")));

                options.AddPolicy("Permission:permisos.eliminar", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == "Permission" && c.Value == "permisos.eliminar")));

                // Políticas para ventas
                options.AddPolicy("Permission:ventas.ver", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == "Permission" && c.Value == "ventas.ver")));

                options.AddPolicy("Permission:ventas.crear", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == "Permission" && c.Value == "ventas.crear")));

                options.AddPolicy("Permission:ventas.editar", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == "Permission" && c.Value == "ventas.editar")));

                options.AddPolicy("Permission:ventas.eliminar", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == "Permission" && c.Value == "ventas.eliminar")));

                options.AddPolicy("Permission:ventas.autorizar", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == "Permission" && c.Value == "ventas.autorizar")));

                options.AddPolicy("Permission:ventas.rechazar", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == "Permission" && c.Value == "ventas.rechazar")));

                // Políticas para productos
                options.AddPolicy("Permission:productos.ver", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == "Permission" && c.Value == "productos.ver")));

                options.AddPolicy("Permission:productos.crear", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == "Permission" && c.Value == "productos.crear")));

                options.AddPolicy("Permission:productos.editar", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == "Permission" && c.Value == "productos.editar")));

                options.AddPolicy("Permission:productos.eliminar", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == "Permission" && c.Value == "productos.eliminar")));

                options.AddPolicy("Permission:productos.ajustarprecios", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == "Permission" && c.Value == "productos.ajustarprecios")));

                // Políticas para clientes
                options.AddPolicy("Permission:clientes.ver", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == "Permission" && c.Value == "clientes.ver")));

                options.AddPolicy("Permission:clientes.crear", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == "Permission" && c.Value == "clientes.crear")));

                options.AddPolicy("Permission:clientes.editar", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == "Permission" && c.Value == "clientes.editar")));

                options.AddPolicy("Permission:clientes.eliminar", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == "Permission" && c.Value == "clientes.eliminar")));

                // Políticas para reportes
                options.AddPolicy("Permission:reportes.ver", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == "Permission" && c.Value == "reportes.ver")));

                options.AddPolicy("Permission:reportes.exportar", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == "Permission" && c.Value == "reportes.exportar")));

                // Políticas para configuración
                options.AddPolicy("Permission:configuracion.ver", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == "Permission" && c.Value == "configuracion.ver")));

                options.AddPolicy("Permission:configuracion.editar", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == "Permission" && c.Value == "configuracion.editar")));
            });

            return services;
        }

        public static IApplicationBuilder UseAuthenticationConfig(this IApplicationBuilder app)
        {
            app.UseAuthentication();
            app.UseAuthorization();

            return app;
        }
    }
}