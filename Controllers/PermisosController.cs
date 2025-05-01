// Controllers/Authentication/PermisosController.cs
using Javo2.Controllers.Base;
using Javo2.IServices.Authentication;
using Javo2.Models.Authentication;
using Javo2.ViewModels.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Javo2.Controllers.Authentication
{
    [AllowAnonymous] // Esto permite acceder sin verificar permisos
    public class PermisoEmergenciaController : BaseController
    {
        private readonly IPermisoService _permisoService;
        private readonly IRolService _rolService;

        public PermisoEmergenciaController(
            IPermisoService permisoService,
            IRolService rolService,
            ILogger<PermisoEmergenciaController> logger) : base(logger)
        {
            _permisoService = permisoService;
            _rolService = rolService;
        }

        // GET: /PermisoEmergencia/FixAdmin
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

        // GET: Permisos
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

        // GET: Permisos/Details/5
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

        // GET: Permisos/Create
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

        // POST: Permisos/Create
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

        // GET: Permisos/Edit/5
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

        // POST: Permisos/Edit/5
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

        // GET: Permisos/Delete/5
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

        // POST: Permisos/Delete/5
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

        // POST: Permisos/ToggleEstado/5
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
    }
}
