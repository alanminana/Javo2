using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Javo2.infraestructura.Extensions
{
    public static class SecurityExtensions
    {
        public static IServiceCollection AddJavoSecurity(this IServiceCollection services)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        // TODO: configura Authority, Audience, etc.
                    });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy =>
                    policy.RequireClaim("role", "Admin"));
                // Agrega más políticas según necesidad
            });

            return services;
        }

        public static IApplicationBuilder UseJavoSecurity(this IApplicationBuilder app)
        {
            app.UseAuthentication();
            app.UseAuthorization();
            return app;
        }
    }
}
