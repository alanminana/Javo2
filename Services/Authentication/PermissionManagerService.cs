// Services/Authentication/PermissionManagerService.cs
using Javo2.IServices.Authentication;
using Javo2.Models.Authentication;
using Microsoft.Extensions.Caching.Memory;
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
        private readonly IRolService _rolService;
        private readonly IUsuarioService _usuarioService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<PermissionManagerService> _logger;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(30);

        public PermissionManagerService(
            IPermisoService permisoService,
            IRolService rolService,
            IUsuarioService usuarioService,
            IMemoryCache cache,
            ILogger<PermissionManagerService> logger)
        {
            _permisoService = permisoService;
            _rolService = rolService;
            _usuarioService = usuarioService;
            _cache = cache;
            _logger = logger;
        }

        #region Verificación de Permisos

        public async Task<bool> UserHasPermissionAsync(ClaimsPrincipal user, string permissionCode)
        {
            // Verificar si el usuario está autenticado
            if (!user.Identity.IsAuthenticated)
                return false;

            // Obtener el ID del usuario
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return false;

            // Verificar el permiso en los claims del usuario (más rápido)
            if (user.HasClaim("Permission", permissionCode))
                return true;

            // Verificar el permiso en la base de datos usando caché
            return await UserHasPermissionInDatabaseAsync(userId, permissionCode);
        }

        private async Task<bool> UserHasPermissionInDatabaseAsync(int userId, string permissionCode)
        {
            // Verificar en caché
            string cacheKey = $"UserPermissions_{userId}";
            if (!_cache.TryGetValue(cacheKey, out HashSet<string> userPermissions))
            {
                // Si no está en caché, obtener de la base de datos
                userPermissions = await GetUserPermissionsAsync(userId);

                // Guardar en caché
                _cache.Set(cacheKey, userPermissions, _cacheDuration);
            }

            return userPermissions.Contains(permissionCode);
        }

        private async Task<HashSet<string>> GetUserPermissionsAsync(int userId)
        {
            try
            {
                var permisos = await _usuarioService.GetPermisosUsuarioAsync(userId);
                return new HashSet<string>(permisos.Where(p => p.Activo).Select(p => p.Codigo));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener permisos del usuario ID: {UserId}", userId);
                return new HashSet<string>();
            }
        }

        #endregion

        #region Administración de Permisos

        public async Task<IEnumerable<Permiso>> GetAllPermissionsAsync()
        {
            string cacheKey = "AllPermissions";
            if (!_cache.TryGetValue(cacheKey, out IEnumerable<Permiso> permisos))
            {
                permisos = await _permisoService.GetAllPermisosAsync();
                _cache.Set(cacheKey, permisos, _cacheDuration);
            }
            return permisos;
        }

        public async Task<IEnumerable<Permiso>> GetGroupedPermissionsAsync()
        {
            var permisos = await GetAllPermissionsAsync();
            return permisos.OrderBy(p => p.Grupo).ThenBy(p => p.Nombre);
        }

        public async Task<Dictionary<string, List<Permiso>>> GetPermissionsByGroupAsync()
        {
            var permisos = await GetAllPermissionsAsync();
            return permisos
                .GroupBy(p => p.Grupo ?? "General")
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.OrderBy(p => p.Nombre).ToList());
        }

        public async Task<bool> CreateOrUpdatePermissionAsync(Permiso permiso)
        {
            try
            {
                if (permiso.PermisoID > 0)
                {
                    var result = await _permisoService.UpdatePermisoAsync(permiso);
                    if (result)
                        InvalidateCache();
                    return result;
                }
                else
                {
                    var result = await _permisoService.CreatePermisoAsync(permiso);
                    if (result)
                        InvalidateCache();
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear/actualizar permiso: {Name}", permiso.Nombre);
                return false;
            }
        }

        public async Task<bool> DeletePermissionAsync(int id)
        {
            try
            {
                var result = await _permisoService.DeletePermisoAsync(id);
                if (result)
                    InvalidateCache();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar permiso ID: {Id}", id);
                return false;
            }
        }

        public async Task<bool> TogglePermissionStatusAsync(int id)
        {
            try
            {
                var permiso = await _permisoService.GetPermisoByIDAsync(id);
                if (permiso == null)
                    return false;

                permiso.Activo = !permiso.Activo;
                var result = await _permisoService.UpdatePermisoAsync(permiso);
                if (result)
                    InvalidateCache();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar estado del permiso ID: {Id}", id);
                return false;
            }
        }

        #endregion

        #region Administración de Roles

        public async Task<IEnumerable<Rol>> GetAllRolesAsync()
        {
            string cacheKey = "AllRoles";
            if (!_cache.TryGetValue(cacheKey, out IEnumerable<Rol> roles))
            {
                roles = await _rolService.GetAllRolesAsync();
                _cache.Set(cacheKey, roles, _cacheDuration);
            }
            return roles;
        }

        public async Task<Rol> GetRoleByIdAsync(int id)
        {
            string cacheKey = $"Role_{id}";
            if (!_cache.TryGetValue(cacheKey, out Rol rol))
            {
                rol = await _rolService.GetRolByIDAsync(id);
                if (rol != null)
                    _cache.Set(cacheKey, rol, _cacheDuration);
            }
            return rol;
        }

        public async Task<bool> CreateOrUpdateRoleAsync(Rol rol)
        {
            try
            {
                if (rol.RolID > 0)
                {
                    var result = await _rolService.UpdateRolAsync(rol);
                    if (result)
                        InvalidateCache();
                    return result;
                }
                else
                {
                    var rolId = await _rolService.CreateRolAsync(rol);
                    if (rolId > 0)
                        InvalidateCache();
                    return rolId > 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear/actualizar rol: {Name}", rol.Nombre);
                return false;
            }
        }

        public async Task<bool> DeleteRoleAsync(int id)
        {
            try
            {
                var result = await _rolService.DeleteRolAsync(id);
                if (result)
                    InvalidateCache();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar rol ID: {Id}", id);
                return false;
            }
        }

        public async Task<bool> AssignPermissionsToRoleAsync(int roleId, IEnumerable<int> permissionIds)
        {
            try
            {
                // Obtener rol actual y sus permisos
                var rol = await _rolService.GetRolByIDAsync(roleId);
                if (rol == null)
                    return false;

                var permisosActuales = rol.Permisos.Select(p => p.PermisoID).ToList();
                var permisosSeleccionados = permissionIds.ToList();

                // Calcular permisos a eliminar y a agregar
                var permisosEliminar = permisosActuales.Except(permisosSeleccionados).ToList();
                var permisosAgregar = permisosSeleccionados.Except(permisosActuales).ToList();

                // Eliminar permisos
                foreach (var permisoID in permisosEliminar)
                {
                    await _rolService.QuitarPermisoAsync(roleId, permisoID);
                }

                // Agregar permisos
                foreach (var permisoID in permisosAgregar)
                {
                    await _rolService.AsignarPermisoAsync(roleId, permisoID);
                }

                // Invalidar caché
                InvalidateCache();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al asignar permisos al rol ID: {Id}", roleId);
                return false;
            }
        }

        #endregion

        #region Caché

        private void InvalidateCache()
        {
            // Invalidar todas las claves de caché relacionadas con permisos y roles
            _cache.Remove("AllPermissions");
            _cache.Remove("AllRoles");

            // Eliminar caché de permisos de usuario
            var cacheKeys = _cache.GetKeys<string>().Where(k => k.StartsWith("UserPermissions_")).ToList();
            foreach (var key in cacheKeys)
            {
                _cache.Remove(key);
            }

            // Eliminar caché de roles específicos
            var roleCacheKeys = _cache.GetKeys<string>().Where(k => k.StartsWith("Role_")).ToList();
            foreach (var key in roleCacheKeys)
            {
                _cache.Remove(key);
            }
        }

        #endregion
    }

    // Extensión para obtener todas las claves de cache
    public static class MemoryCacheExtensions
    {
        public static IEnumerable<T> GetKeys<T>(this IMemoryCache cache)
        {
            var field = typeof(MemoryCache).GetProperty("EntriesCollection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var collection = field.GetValue(cache) as System.Collections.ICollection;

            // Si no se puede obtener la colección, devolver una lista vacía
            if (collection == null)
                return Enumerable.Empty<T>();

            // De lo contrario, extraer las claves
            var keys = new List<T>();
            foreach (var item in collection)
            {
                var methodInfo = item.GetType().GetProperty("Key");
                var key = methodInfo.GetValue(item);
                if (key is T typedKey)
                {
                    keys.Add(typedKey);
                }
            }
            return keys;
        }
    }
}