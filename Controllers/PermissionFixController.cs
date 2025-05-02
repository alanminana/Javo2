// Controllers/PermissionFixController.cs
using Javo2.Controllers.Base;
using Javo2.IServices.Authentication;
using Javo2.Models.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Javo2.Controllers
{
    [AllowAnonymous] // Esto permite acceder sin verificar permisos
    public class PermissionFixController : BaseController
    {
        private readonly IPermisoService _permisoService;
        private readonly IRolService _rolService;
        private readonly IUsuarioService _usuarioService;

        public PermissionFixController(
            IPermisoService permisoService,
            IRolService rolService,
            IUsuarioService usuarioService,
            ILogger<PermissionFixController> logger) : base(logger)
        {
            _permisoService = permisoService;
            _rolService = rolService;
            _usuarioService = usuarioService;
        }

        // GET: /PermissionFix/FixAdminPermissions
        public async Task<IActionResult> FixAdminPermissions()
        {
            try
            {
                var resultados = new List<string>();
                // 1. Encontrar el rol Administrador
                var roles = await _rolService.GetAllRolesAsync();
                var rolAdmin = roles.FirstOrDefault(r => r.Nombre.Equals("Administrador", StringComparison.OrdinalIgnoreCase));

                if (rolAdmin == null)
                {
                    return Content("Error: No se encontró el rol Administrador");
                }

                resultados.Add($"Rol Administrador encontrado con ID: {rolAdmin.RolID}");

                // 2. Obtener todos los permisos disponibles
                var permisos = await _permisoService.GetAllPermisosAsync();
                resultados.Add($"Encontrados {permisos.Count()} permisos en total");

                // 3. Obtener permisos actuales del administrador
                var permisosAdmin = rolAdmin.Permisos?.Select(p => p.PermisoID).ToList() ?? new List<int>();
                resultados.Add($"El administrador tiene actualmente {permisosAdmin.Count} permisos asignados");

                // 4. Encontrar permisos faltantes
                var permisosFaltantes = permisos
                    .Where(p => !permisosAdmin.Contains(p.PermisoID))
                    .ToList();

                resultados.Add($"Permisos faltantes: {permisosFaltantes.Count}");

                // 5. Asignar todos los permisos faltantes
                foreach (var permiso in permisosFaltantes)
                {
                    await _rolService.AsignarPermisoAsync(rolAdmin.RolID, permiso.PermisoID);
                    resultados.Add($"Asignado permiso: {permiso.Codigo} ({permiso.Nombre})");
                }

                // 6. Verificar el permiso específico de autorización de ventas
                var permisoAutorizarVentas = permisos.FirstOrDefault(p => p.Codigo == "ventas.autorizar");
                if (permisoAutorizarVentas != null)
                {
                    if (!permisosAdmin.Contains(permisoAutorizarVentas.PermisoID))
                    {
                        await _rolService.AsignarPermisoAsync(rolAdmin.RolID, permisoAutorizarVentas.PermisoID);
                        resultados.Add($"Asignado explícitamente el permiso crítico: ventas.autorizar");
                    }
                    else
                    {
                        resultados.Add("El permiso ventas.autorizar ya estaba asignado");
                    }
                }
                else
                {
                    // Crear el permiso si no existe
                    permisoAutorizarVentas = new Permiso
                    {
                        Codigo = "ventas.autorizar",
                        Nombre = "Autorizar ventas",
                        Grupo = "Ventas",
                        Descripcion = "Permite autorizar ventas pendientes",
                        Activo = true,
                        EsSistema = true
                    };

                    await _permisoService.CreatePermisoAsync(permisoAutorizarVentas);

                    // Obtener el ID del permiso recién creado
                    permisoAutorizarVentas = await _permisoService.GetPermisoByCodigo("ventas.autorizar");

                    // Asignar al rol Administrador
                    await _rolService.AsignarPermisoAsync(rolAdmin.RolID, permisoAutorizarVentas.PermisoID);
                    resultados.Add("Creado y asignado el permiso ventas.autorizar");
                }

                // 7. Verificar el permiso específico de rechazo de ventas
                var permisoRechazarVentas = permisos.FirstOrDefault(p => p.Codigo == "ventas.rechazar");
                if (permisoRechazarVentas != null)
                {
                    if (!permisosAdmin.Contains(permisoRechazarVentas.PermisoID))
                    {
                        await _rolService.AsignarPermisoAsync(rolAdmin.RolID, permisoRechazarVentas.PermisoID);
                        resultados.Add($"Asignado explícitamente el permiso crítico: ventas.rechazar");
                    }
                    else
                    {
                        resultados.Add("El permiso ventas.rechazar ya estaba asignado");
                    }
                }
                else
                {
                    // Crear el permiso si no existe
                    permisoRechazarVentas = new Permiso
                    {
                        Codigo = "ventas.rechazar",
                        Nombre = "Rechazar ventas",
                        Grupo = "Ventas",
                        Descripcion = "Permite rechazar ventas pendientes",
                        Activo = true,
                        EsSistema = true
                    };

                    await _permisoService.CreatePermisoAsync(permisoRechazarVentas);

                    // Obtener el ID del permiso recién creado
                    permisoRechazarVentas = await _permisoService.GetPermisoByCodigo("ventas.rechazar");

                    // Asignar al rol Administrador
                    await _rolService.AsignarPermisoAsync(rolAdmin.RolID, permisoRechazarVentas.PermisoID);
                    resultados.Add("Creado y asignado el permiso ventas.rechazar");
                }

                // 8. Verificar el permiso de entrega de productos
                var permisoEntregaProductos = permisos.FirstOrDefault(p => p.Codigo == "ventas.entregaProductos");
                if (permisoEntregaProductos != null)
                {
                    if (!permisosAdmin.Contains(permisoEntregaProductos.PermisoID))
                    {
                        await _rolService.AsignarPermisoAsync(rolAdmin.RolID, permisoEntregaProductos.PermisoID);
                        resultados.Add($"Asignado explícitamente el permiso crítico: ventas.entregaProductos");
                    }
                    else
                    {
                        resultados.Add("El permiso ventas.entregaProductos ya estaba asignado");
                    }
                }
                else
                {
                    // Crear el permiso si no existe
                    permisoEntregaProductos = new Permiso
                    {
                        Codigo = "ventas.entregaProductos",
                        Nombre = "Entrega de productos",
                        Grupo = "Ventas",
                        Descripcion = "Permite gestionar la entrega de productos",
                        Activo = true,
                        EsSistema = true
                    };

                    await _permisoService.CreatePermisoAsync(permisoEntregaProductos);

                    // Obtener el ID del permiso recién creado
                    permisoEntregaProductos = await _permisoService.GetPermisoByCodigo("ventas.entregaProductos");

                    // Asignar al rol Administrador
                    await _rolService.AsignarPermisoAsync(rolAdmin.RolID, permisoEntregaProductos.PermisoID);
                    resultados.Add("Creado y asignado el permiso ventas.entregaProductos");
                }

                // 9. Verificar que existe el permiso securitydashboard.ver
                var permisoSecurityDashboard = permisos.FirstOrDefault(p => p.Codigo == "securitydashboard.ver");
                if (permisoSecurityDashboard != null)
                {
                    if (!permisosAdmin.Contains(permisoSecurityDashboard.PermisoID))
                    {
                        await _rolService.AsignarPermisoAsync(rolAdmin.RolID, permisoSecurityDashboard.PermisoID);
                        resultados.Add($"Asignado explícitamente el permiso: securitydashboard.ver");
                    }
                    else
                    {
                        resultados.Add("El permiso securitydashboard.ver ya estaba asignado");
                    }
                }
                else
                {
                    var nuevoPermiso = new Permiso
                    {
                        Codigo = "securitydashboard.ver",
                        Nombre = "Ver Dashboard de Seguridad",
                        Grupo = "Seguridad",
                        Descripcion = "Permite ver el panel de control de seguridad",
                        Activo = true,
                        EsSistema = true
                    };

                    await _permisoService.CreatePermisoAsync(nuevoPermiso);
                    permisoSecurityDashboard = await _permisoService.GetPermisoByCodigo("securitydashboard.ver");

                    if (permisoSecurityDashboard != null)
                    {
                        await _rolService.AsignarPermisoAsync(rolAdmin.RolID, permisoSecurityDashboard.PermisoID);
                        resultados.Add("Creado y asignado el permiso securitydashboard.ver");
                    }
                }

                // 10. Verificar si existe usuario admin y regenerar claims
                var usuarioAdmin = await _usuarioService.GetUsuarioByNombreUsuarioAsync("admin");
                if (usuarioAdmin != null)
                {
                    resultados.Add($"Encontrado usuario admin con ID: {usuarioAdmin.UsuarioID}");
                    resultados.Add("IMPORTANTE: Es necesario cerrar sesión y volver a iniciar para aplicar los cambios de permisos");
                }
                else
                {
                    resultados.Add("No se encontró un usuario con nombre 'admin'");
                }

                return Content("<h1>Resultados de la reparación de permisos</h1><ul>" +
                    string.Join("", resultados.Select(r => $"<li>{r}</li>")) +
                    "</ul><p><strong>IMPORTANTE:</strong> Es necesario cerrar sesión y volver a iniciar para aplicar los cambios de permisos</p>",
                    "text/html");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al reparar permisos de administrador");
                return Content($"Error: {ex.Message}<br/>{ex.StackTrace}");
            }
        }
    }
}