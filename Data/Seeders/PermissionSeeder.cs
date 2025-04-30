// Data/Seeders/PermissionSeeder.cs
using Javo2.IServices.Authentication;
using Javo2.Models.Authentication;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Javo2.Data.Seeders
{
    public class PermissionSeeder
    {
        private readonly IPermisoService _permisoService;
        private readonly IRolService _rolService;
        private readonly ILogger<PermissionSeeder> _logger;

        public PermissionSeeder(
            IPermisoService permisoService,
            IRolService rolService,
            ILogger<PermissionSeeder> logger)
        {
            _permisoService = permisoService;
            _rolService = rolService;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            try
            {
                // Lista de todos los permisos del sistema
                var permisosNecesarios = new List<(string Codigo, string Nombre, string Grupo, string Descripcion)>
                {
                    // SecurityDashboard
                    ("securitydashboard.ver", "Ver Dashboard de Seguridad", "Seguridad", "Permite ver el panel de control de seguridad"),
                    ("securitydashboard.crear", "Crear elementos en Dashboard", "Seguridad", "Permite crear elementos en el panel de seguridad"),
                    ("securitydashboard.editar", "Editar elementos en Dashboard", "Seguridad", "Permite editar elementos en el panel de seguridad"),
                    ("securitydashboard.eliminar", "Eliminar elementos en Dashboard", "Seguridad", "Permite eliminar elementos en el panel de seguridad"),
                };

                // Permisos actuales
                var permisosActuales = await _permisoService.GetAllPermisosAsync();
                var codigosExistentes = permisosActuales.Select(p => p.Codigo).ToList();

                // Crear permisos que no existen
                foreach (var perm in permisosNecesarios)
                {
                    if (!codigosExistentes.Contains(perm.Codigo))
                    {
                        var nuevoPermiso = new Permiso
                        {
                            Codigo = perm.Codigo,
                            Nombre = perm.Nombre,
                            Grupo = perm.Grupo,
                            Descripcion = perm.Descripcion,
                            Activo = true,
                            EsSistema = true
                        };

                        await _permisoService.CreatePermisoAsync(nuevoPermiso);
                        _logger.LogInformation("Permiso creado: {Codigo}", perm.Codigo);
                    }
                }

                // Asegurarse de que el rol Administrador tenga todos los permisos
                var roles = await _rolService.GetAllRolesAsync();
                var rolAdmin = roles.FirstOrDefault(r => r.Nombre.Equals("Administrador", StringComparison.OrdinalIgnoreCase));

                if (rolAdmin != null)
                {
                    permisosActuales = await _permisoService.GetAllPermisosAsync(); // Actualizar con los nuevos permisos
                    var permisosAdmin = rolAdmin.Permisos.Select(p => p.PermisoID).ToList();

                    foreach (var permiso in permisosActuales)
                    {
                        if (!permisosAdmin.Contains(permiso.PermisoID))
                        {
                            await _rolService.AsignarPermisoAsync(rolAdmin.RolID, permiso.PermisoID);
                            _logger.LogInformation("Permiso {Codigo} asignado al rol Administrador", permiso.Codigo);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al sembrar permisos");
            }
        }
    }
}