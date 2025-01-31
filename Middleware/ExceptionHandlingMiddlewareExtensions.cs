// Archivo: Middleware/ExceptionHandlingMiddlewareExtensions.cs
using Microsoft.AspNetCore.Builder;

namespace Javo2.Middleware
{
    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomExceptionHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}
