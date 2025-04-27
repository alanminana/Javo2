// Controllers/Authentication/RolesController.cs
using Javo2.Controllers.Base;
using Javo2.IServices.Authentication;
using Javo2.Models.Authentication;
using Javo2.ViewModels.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Javo2.Controllers.Authentication
{
    [Authorize(Policy = "PermisoPolitica")]
    public class RolesController : BaseController
    {
        private readonly IRolService _rolService;
        private readonly IPermisoService _permisoService;

        public RolesController(
            IRolService rolService,
            IPermisoService permisoService,
            ILogger<RolesController> logger)
            : base(logger)
        {
            _rolService = rolService;
            _permisoService = permisoService;
        }

        // GET: Roles
        [Authorize(Policy = "Permission:roles.ver")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var roles = await _rolService.GetAllRolesAsync();
                return View(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la lista de roles");
                return View("Error");
            }
        }

        // GET: Roles/Details/5
        [Authorize(Policy = "Permission:roles.ver")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var rol = await _rolService.GetRolByIDAsync(id);
                if (rol == null)
                {
                    return NotFound();
                }

                // Obtener permisos del rol
                var permisosIds = rol.Permisos.Select(p => p.PermisoID).ToList();
                var permisos = new List<Permiso>();

                foreach (var permisoId in permisosIds)
                {
                    var permiso = await _permisoService.GetPermisoByIDAsync(permisoId);
                    if (permiso != null)
                    {
                        permisos.Add(permiso);
                    }
                }

                var model = new RolDetailsViewModel
                {
                    Rol = rol,
                    Permisos = permisos
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener detalles del rol");
                return View("Error");
            }
        }

        // GET: Roles/Create
        [Authorize(Policy = "Permission:roles.crear")]
        public async Task<IActionResult> Create()
        {
            try
            {
                var allPermisos = await _permisoService.GetAllPermisosAsync();
                var permisosActivos = allPermisos.Where(p => p.Activo).ToList();

                // Agrupar permisos por grupo
                var gruposPermisos = permisosActivos
                    .GroupBy(p => p.Grupo ?? "General")
                    .ToDictionary(g => g.Key, g => g.ToList());

                var model = new RolFormViewModel
                {
                    Rol = new Rol { EsSistema = false },
                    GruposPermisos = gruposPermisos,
                    PermisosSeleccionados = new List<int>(),
                    EsEdicion = false
                };

                return View("Form", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al preparar formulario de creación de rol");
                return View("Error");
            }
        }

        // POST: Roles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:roles.crear")]
        public async Task<IActionResult> Create(RolFormViewModel model, List<int> PermisosSeleccionados)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var allPermisos = await _permisoService.GetAllPermisosAsync();
                    var permisosActivos = allPermisos.Where(p => p.Activo).ToList();
                    var gruposPermisos = permisosActivos
                        .GroupBy(p => p.Grupo ?? "General")
                        .ToDictionary(g => g.Key, g => g.ToList());

                    model.GruposPermisos = gruposPermisos;
                    model.PermisosSeleccionados = PermisosSeleccionados ?? new List<int>();
                    model.EsEdicion = false;

                    return View("Form", model);
                }

                // Crear rol
                var rol = new Rol
                {
                    Nombre = model.Rol.Nombre,
                    Descripcion = model.Rol.Descripcion,
                    EsSistema = false // Los roles creados manualmente nunca son del sistema
                };

                var rolID = await _rolService.CreateRolAsync(rol);

                // Asignar permisos
                if (PermisosSeleccionados != null && PermisosSeleccionados.Any())
                {
                    foreach (var permisoID in PermisosSeleccionados)
                    {
                        await _rolService.AsignarPermisoAsync(rolID, permisoID);
                    }
                }

                TempData["Success"] = "Rol creado correctamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear rol");
                ModelState.AddModelError(string.Empty, "Error al crear rol: " + ex.Message);

                var allPermisos = await _permisoService.GetAllPermisosAsync();
                var permisosActivos = allPermisos.Where(p => p.Activo).ToList();
                var gruposPermisos = permisosActivos
                    .GroupBy(p => p.Grupo ?? "General")
                    .ToDictionary(g => g.Key, g => g.ToList());

                model.GruposPermisos = gruposPermisos;
                model.PermisosSeleccionados = PermisosSeleccionados ?? new List<int>();
                model.EsEdicion = false;

                return View("Form", model);
            }
        }

        // GET: Roles/Edit/5
        [Authorize(Policy = "Permission:roles.editar")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var rol = await _rolService.GetRolByIDAsync(id);
                if (rol == null)
                {
                    return NotFound();
                }

                // Obtener permisos asignados
                var permisosIds = rol.Permisos.Select(p => p.PermisoID).ToList();

                // Obtener todos los permisos
                var allPermisos = await _permisoService.GetAllPermisosAsync();
                var permisosActivos = allPermisos.Where(p => p.Activo).ToList();

                // Agrupar permisos por grupo
                var gruposPermisos = permisosActivos
                    .GroupBy(p => p.Grupo ?? "General")
                    .ToDictionary(g => g.Key, g => g.ToList());

                var model = new RolFormViewModel
                {
                    Rol = rol,
                    GruposPermisos = gruposPermisos,
                    PermisosSeleccionados = permisosIds,
                    EsEdicion = true
                };

                return View("Form", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al preparar formulario de edición de rol");
                return View("Error");
            }
        }

        // POST: Roles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:roles.editar")]
        public async Task<IActionResult> Edit(int id, RolFormViewModel model, List<int> PermisosSeleccionados)
        {
            if (id != model.Rol.RolID)
            {
                return NotFound();
            }

            try
            {
                var originalRol = await _rolService.GetRolByIDAsync(id);
                if (originalRol == null)
                {
                    return NotFound();
                }

                // Si es un rol del sistema, solo permitir cambiar la descripción
                if (originalRol.EsSistema)
                {
                    originalRol.Descripcion = model.Rol.Descripcion;
                }
                else
                {
                    originalRol.Nombre = model.Rol.Nombre;
                    originalRol.Descripcion = model.Rol.Descripcion;
                }

                // Actualizar rol
                await _rolService.UpdateRolAsync(originalRol);

                // Actualizar permisos
                // 1. Obtener permisos actuales
                var permisosActuales = originalRol.Permisos.Select(p => p.PermisoID).ToList();

                // 2. Calcular permisos a eliminar y a agregar
                var permisosSeleccionados = PermisosSeleccionados ?? new List<int>();
                var permisosEliminar = permisosActuales.Except(permisosSeleccionados).ToList();
                var permisosAgregar = permisosSeleccionados.Except(permisosActuales).ToList();

                // 3. Eliminar permisos
                foreach (var permisoID in permisosEliminar)
                {
                    await _rolService.QuitarPermisoAsync(id, permisoID);
                }

                // 4. Agregar permisos
                foreach (var permisoID in permisosAgregar)
                {
                    await _rolService.AsignarPermisoAsync(id, permisoID);
                }

                TempData["Success"] = "Rol actualizado correctamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar rol");
                ModelState.AddModelError(string.Empty, "Error al actualizar rol: " + ex.Message);

                var allPermisos = await _permisoService.GetAllPermisosAsync();
                var permisosActivos = allPermisos.Where(p => p.Activo).ToList();
                var gruposPermisos = permisosActivos
                    .GroupBy(p => p.Grupo ?? "General")
                    .ToDictionary(g => g.Key, g => g.ToList());

                model.GruposPermisos = gruposPermisos;
                model.PermisosSeleccionados = PermisosSeleccionados ?? new List<int>();
                model.EsEdicion = true;

                return View("Form", model);
            }
        }

        // GET: Roles/Delete/5
        [Authorize(Policy = "Permission:roles.eliminar")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var rol = await _rolService.GetRolByIDAsync(id);
                if (rol == null)
                {
                    return NotFound();
                }

                // No permitir eliminar roles del sistema
                if (rol.EsSistema)
                {
                    TempData["Error"] = "No se pueden eliminar roles del sistema";
                    return RedirectToAction(nameof(Index));
                }

                // Obtener permisos del rol
                var permisosIds = rol.Permisos.Select(p => p.PermisoID).ToList();
                var permisos = new List<Permiso>();

                foreach (var permisoId in permisosIds)
                {
                    var permiso = await _permisoService.GetPermisoByIDAsync(permisoId);
                    if (permiso != null)
                    {
                        permisos.Add(permiso);
                    }
                }

                var model = new RolDetailsViewModel
                {
                    Rol = rol,
                    Permisos = permisos
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al preparar eliminación de rol");
                return View("Error");
            }
        }

        // POST: Roles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:roles.eliminar")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var rol = await _rolService.GetRolByIDAsync(id);
                if (rol == null)
                {
                    return NotFound();
                }

                // No permitir eliminar roles del sistema
                if (rol.EsSistema)
                {
                    TempData["Error"] = "No se pueden eliminar roles del sistema";
                    return RedirectToAction(nameof(Index));
                }

                await _rolService.DeleteRolAsync(id);
                TempData["Success"] = "Rol eliminado correctamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar rol");
                TempData["Error"] = "Error al eliminar rol: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}