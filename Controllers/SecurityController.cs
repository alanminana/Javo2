// Controllers/Security/SecurityController.cs
using Javo2.Controllers.Base;
using Javo2.Data.Seeders;
using Javo2.IServices.Authentication;
using Javo2.Models.Authentication;
using Javo2.ViewModels.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Javo2.Controllers.Security
{
    public class SecurityController : BaseController
    {
        private readonly IPermisoService _permisoService;
        private readonly IRolService _rolService;
        private readonly IUsuarioService _usuarioService;

        public SecurityController(
            IPermisoService permisoService,
            IRolService rolService,
            IUsuarioService usuarioService,
            ILogger<SecurityController> logger) : base(logger)
        {
            _permisoService = permisoService;
            _rolService = rolService;
            _usuarioService = usuarioService;
        }

        #region Herramientas de Seguridad

        [Authorize(Roles = "Administrador")]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> RecargarPermisos()
        {
            try
            {
                // Obtener las dependencias del contenedor
                var permisoService = HttpContext.RequestServices.GetRequiredService<IPermisoService>();
                var rolService = HttpContext.RequestServices.GetRequiredService<IRolService>();
                var loggerFactory = HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
                var permissionLogger = loggerFactory.CreateLogger<PermissionSeeder>();

                var seeder = new PermissionSeeder(permisoService, rolService, permissionLogger);
                await seeder.SeedAsync();

                TempData["Success"] = "Permisos recargados correctamente";
                return RedirectToAction("Index", "SecurityDashboard");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al recargar permisos");
                TempData["Error"] = "Error al recargar permisos: " + ex.Message;
                return RedirectToAction("Index", "SecurityDashboard");
            }
        }

        [Authorize(Roles = "Administrador")]
        public IActionResult CerrarSesiones()
        {
            // Implementación futura para cerrar sesiones activas
            TempData["Info"] = "Función no implementada todavía";
            return RedirectToAction("Index", "SecurityDashboard");
        }

        #endregion

        #region Reparación de Permisos

        [AllowAnonymous]
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

                // 8. Verificar el permiso específico de securitydashboard.ver
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

                // 9. Verificar usuario admin
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

        [AllowAnonymous]
        public async Task<IActionResult> FixAdmin()
        {
            try
            {
                // 1. Verificar si existe el permiso securitydashboard.ver
                var permisoSecurityDashboard = await _permisoService.GetPermisoByCodigo("securitydashboard.ver");

                // Si no existe, crear el permiso
                if (permisoSecurityDashboard == null)
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

                    if (permisoSecurityDashboard == null)
                    {
                        return Content("Error: No se pudo crear el permiso securitydashboard.ver");
                    }
                }

                // 2. Encontrar el rol Administrador
                var roles = await _rolService.GetAllRolesAsync();
                var rolAdmin = roles.FirstOrDefault(r => r.Nombre.Equals("Administrador", StringComparison.OrdinalIgnoreCase));

                if (rolAdmin == null)
                {
                    return Content("Error: No se encontró el rol Administrador");
                }

                // 3. Verificar si el rol ya tiene el permiso
                var permisosAdmin = rolAdmin.Permisos?.Select(p => p.PermisoID).ToList() ?? new List<int>();

                if (!permisosAdmin.Contains(permisoSecurityDashboard.PermisoID))
                {
                    // 4. Asignar el permiso al rol Administrador
                    await _rolService.AsignarPermisoAsync(rolAdmin.RolID, permisoSecurityDashboard.PermisoID);
                }

                return Content("¡Permisos corregidos! El administrador ahora tiene acceso al Dashboard de Seguridad. " +
                               "Ahora puedes iniciar sesión con el usuario admin y acceder a /SecurityDashboard/Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al corregir permisos de admin");
                return Content("Error: " + ex.Message);
            }
        }

        #endregion

        #region Gestión de Permisos

        [HttpGet]
        [Authorize(Policy = "Permission:permisos.ver")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var permisos = await _permisoService.GetAllPermisosAsync();
                var permisosAgrupados = permisos
                    .GroupBy(p => p.Grupo ?? "General")
                    .OrderBy(g => g.Key)
                    .ToDictionary(g => g.Key, g => g.OrderBy(p => p.Nombre).ToList());

                return View(permisosAgrupados);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la lista de permisos");
                return View("Error");
            }
        }

        [HttpGet]
        [Authorize(Policy = "Permission:permisos.ver")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var permiso = await _permisoService.GetPermisoByIDAsync(id);
                if (permiso == null) return NotFound();
                return View(permiso);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener detalles del permiso");
                return View("Error");
            }
        }

        [HttpGet]
        [Authorize(Policy = "Permission:permisos.crear")]
        public IActionResult Create()
        {
            try
            {
                var permiso = new Permiso { Activo = true };
                return View("Form", permiso);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al preparar formulario de creación de permiso");
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:permisos.crear")]
        public async Task<IActionResult> Create(Permiso permiso)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View("Form", permiso);

                var existente = await _permisoService.GetPermisoByCodigo(permiso.Codigo);
                if (existente != null)
                {
                    ModelState.AddModelError("Codigo", "Ya existe un permiso con este código");
                    return View("Form", permiso);
                }

                var result = await _permisoService.CreatePermisoAsync(permiso);
                if (!result)
                {
                    ModelState.AddModelError(string.Empty, "No se pudo crear el permiso");
                    return View("Form", permiso);
                }

                TempData["Success"] = "Permiso creado correctamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear permiso");
                ModelState.AddModelError(string.Empty, "Error al crear permiso: " + ex.Message);
                return View("Form", permiso);
            }
        }

        [HttpGet]
        [Authorize(Policy = "Permission:permisos.editar")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var permiso = await _permisoService.GetPermisoByIDAsync(id);
                if (permiso == null) return NotFound();
                if (permiso.EsSistema)
                {
                    TempData["Error"] = "No se pueden editar permisos del sistema";
                    return RedirectToAction(nameof(Index));
                }
                return View("Form", permiso);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al preparar formulario de edición de permiso");
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:permisos.editar")]
        public async Task<IActionResult> Edit(int id, Permiso permiso)
        {
            if (id != permiso.PermisoID) return NotFound();

            try
            {
                var original = await _permisoService.GetPermisoByIDAsync(id);
                if (original == null) return NotFound();
                if (original.EsSistema)
                {
                    TempData["Error"] = "No se pueden editar permisos del sistema";
                    return RedirectToAction(nameof(Index));
                }

                if (!ModelState.IsValid)
                    return View("Form", permiso);

                var existente = await _permisoService.GetPermisoByCodigo(permiso.Codigo);
                if (existente != null && existente.PermisoID != id)
                {
                    ModelState.AddModelError("Codigo", "Ya existe un permiso con este código");
                    return View("Form", permiso);
                }

                permiso.EsSistema = original.EsSistema;
                var result = await _permisoService.UpdatePermisoAsync(permiso);
                if (!result)
                {
                    ModelState.AddModelError(string.Empty, "No se pudo actualizar el permiso");
                    return View("Form", permiso);
                }

                TempData["Success"] = "Permiso actualizado correctamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar permiso");
                ModelState.AddModelError(string.Empty, "Error al actualizar permiso: " + ex.Message);
                return View("Form", permiso);
            }
        }

        [HttpGet]
        [Authorize(Policy = "Permission:permisos.eliminar")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var permiso = await _permisoService.GetPermisoByIDAsync(id);
                if (permiso == null) return NotFound();
                if (permiso.EsSistema)
                {
                    TempData["Error"] = "No se pueden eliminar permisos del sistema";
                    return RedirectToAction(nameof(Index));
                }
                return View(permiso);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al preparar eliminación de permiso");
                return View("Error");
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:permisos.eliminar")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var permiso = await _permisoService.GetPermisoByIDAsync(id);
                if (permiso == null) return NotFound();
                if (permiso.EsSistema)
                {
                    TempData["Error"] = "No se pueden eliminar permisos del sistema";
                    return RedirectToAction(nameof(Index));
                }

                var result = await _permisoService.DeletePermisoAsync(id);
                if (!result)
                {
                    TempData["Error"] = "No se pudo eliminar el permiso";
                    return RedirectToAction(nameof(Index));
                }

                TempData["Success"] = "Permiso eliminado correctamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar permiso");
                TempData["Error"] = "Error al eliminar permiso: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:permisos.editar")]
        public async Task<IActionResult> ToggleEstado(int id)
        {
            try
            {
                var permiso = await _permisoService.GetPermisoByIDAsync(id);
                if (permiso == null) return NotFound();
                if (permiso.EsSistema)
                    return Json(new { success = false, message = "No se puede cambiar el estado de permisos del sistema" });

                permiso.Activo = !permiso.Activo;
                var result = await _permisoService.UpdatePermisoAsync(permiso);
                if (!result)
                    return Json(new { success = false, message = "No se pudo actualizar el estado del permiso" });

                return Json(new
                {
                    success = true,
                    message = $"Permiso {(permiso.Activo ? "activado" : "desactivado")} correctamente",
                    estado = permiso.Activo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar estado de permiso");
                return Json(new { success = false, message = "Error al cambiar estado del permiso: " + ex.Message });
            }
        }

        #endregion
    }
}