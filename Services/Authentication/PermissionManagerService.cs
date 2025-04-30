// Services/Authentication/PermissionManagerService.cs
using Javo2.IServices.Authentication;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Javo2.Services.Authentication
{
    public class PermissionManagerService : IPermissionManagerService
    {
        private readonly IPermisoService _permisoService;
        private readonly ILogger<PermissionManagerService> _logger;
        private readonly HashSet<string> _registeredPermissions = new HashSet<string>();

        public PermissionManagerService(
            IPermisoService permisoService,
            ILogger<PermissionManagerService> logger)
        {
            _permisoService = permisoService;
            _logger = logger;
        }

        public async Task<bool> UserHasPermissionAsync(ClaimsPrincipal user, string permissionCode)
        {
            if (string.IsNullOrEmpty(permissionCode) || !user.Identity.IsAuthenticated)
                return false;

            return user.HasClaim(c => c.Type == "Permission" && c.Value == permissionCode);
        }

        public async Task<IEnumerable<string>> GetAllPermissionCodesAsync()
        {
            var permisos = await _permisoService.GetAllPermisosAsync();
            return permisos.Where(p => p.Activo).Select(p => p.Codigo);
        }

        public Task<bool> RegisterPermissionPolicyAsync(string permissionCode)
        {
            if (string.IsNullOrEmpty(permissionCode))
                return Task.FromResult(false);

            bool added = _registeredPermissions.Add(permissionCode);
            return Task.FromResult(added);
        }
    }
}