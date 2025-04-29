using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Javo2.Middleware
{
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;
        public SecurityHeadersMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Append("X-Frame-Options", "DENY");
            context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
            context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
            context.Response.Headers.Append("Content-Security-Policy",
                "default-src 'self'; " +
                "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdn.jsdelivr.net https://code.jquery.com; " +
                "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; " +
                "img-src 'self' data:; " +
                "font-src 'self' https://cdn.jsdelivr.net; " +
                "connect-src 'self' ws: wss: http: https:;");

            await _next(context);
        }
    }
}
