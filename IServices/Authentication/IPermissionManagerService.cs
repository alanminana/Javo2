// IServices/Authentication/IPermissionManagerService.cs
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Javo2.IServices.Authentication
{
    public interface IPermissionManagerService
    {
        Task<bool> UserHasPermissionAsync(ClaimsPrincipal user, string permissionCode);
        Task<IEnumerable<string>> GetAllPermissionCodesAsync();
        Task<bool> RegisterPermissionPolicyAsync(string permissionCode);
    }
}