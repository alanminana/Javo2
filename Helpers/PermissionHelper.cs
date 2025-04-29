// Helpers/PermissionHelper.cs
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Javo2.Helpers
{
    public static class PermissionHelper
    {
        public static bool HasPermission(this ClaimsPrincipal user, string permissionCode)
        {
            if (string.IsNullOrEmpty(permissionCode))
                return true;

            return user.HasClaim(c => c.Type == "Permission" && c.Value == permissionCode);
        }
    }
}