// Helpers/ClaimsPrincipalExtensions.cs
using System.Security.Claims;

namespace Javo2.Helpers
{
    public static class ClaimsPrincipalExtensions
    {
        public static bool HasPermission(this ClaimsPrincipal user, string permissionCode)
        {
            if (string.IsNullOrEmpty(permissionCode))
                return true;

            return user.HasClaim(c => c.Type == "Permission" && c.Value == permissionCode);
        }

        public static bool HasAnyPermission(this ClaimsPrincipal user, params string[] permissionCodes)
        {
            if (permissionCodes == null || permissionCodes.Length == 0)
                return false;

            foreach (var code in permissionCodes)
            {
                if (user.HasPermission(code))
                    return true;
            }

            return false;
        }

        public static bool HasAllPermissions(this ClaimsPrincipal user, params string[] permissionCodes)
        {
            if (permissionCodes == null || permissionCodes.Length == 0)
                return true;

            foreach (var code in permissionCodes)
            {
                if (!user.HasPermission(code))
                    return false;
            }

            return true;
        }

        public static bool IsInRole(this ClaimsPrincipal user, params string[] roles)
        {
            if (roles == null || roles.Length == 0)
                return false;

            foreach (var role in roles)
            {
                if (user.IsInRole(role))
                    return true;
            }

            return false;
        }
    }
}