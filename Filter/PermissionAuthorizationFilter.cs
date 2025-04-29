// Filters/PermissionAuthorizationFilter.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Javo2.Helpers;

namespace Javo2.Filters
{
    public class PermissionAuthorizationFilter : IAuthorizationFilter
    {
        private readonly string _permissionCode;

        public PermissionAuthorizationFilter(string permissionCode)
        {
            _permissionCode = permissionCode;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (!context.HttpContext.User.HasPermission(_permissionCode))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Auth", null);
            }
        }
    }

    public class RequirePermissionAttribute : TypeFilterAttribute
    {
        public RequirePermissionAttribute(string permissionCode)
            : base(typeof(PermissionAuthorizationFilter))
        {
            Arguments = new object[] { permissionCode };
        }
    }
}