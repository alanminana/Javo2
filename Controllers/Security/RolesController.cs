// Controllers/Security/RolesController.cs
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
    [Authorize(Policy = "PermisoPolitica")]
    public class RolesController : SecurityBaseController
    {
        public RolesController(
            IUsuarioService usuarioService,
            IRolService rolService,
            IPermisoService permisoService,
            IPermissionManagerService permissionManager,
            ILogger<RolesController> logger)
            : base(usuarioService, rolService, permisoService, permissionManager, logger)
        {
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
                LogError(ex, "Error al obtener la lista de roles");
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

                // Usar método común para crear ViewModel
                var model = CrearRolDetailsViewModel(rol, permisos);
                return View(model);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al obtener detalles del rol");
                return View("Error");
            }
        }

        // GET: Roles/Create
        [Authorize(Policy = "Permission:roles.crear")]
        public async Task<IActionResult> Create()
        {
            try
            {
                LogInfo("Iniciando creación de rol - GET");

                // Usar método común para preparar formulario
                var model = await PrepararFormularioRolAsync();
                return View("Form", model);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al preparar formulario de creación de rol");
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
                LogInfo("Iniciando creación de rol - POST");
                LogInfo("Nombre del rol: {Nombre}", model.Rol?.Nombre);
                LogInfo("Permisos seleccionados: {Count}", PermisosSeleccionados?.Count ?? 0);

                if (string.IsNullOrWhiteSpace(model.Rol?.Nombre))
                {
                    ModelState.AddModelError("Rol.Nombre", "El nombre del rol es obligatorio");
                    LogWarning("Error de validación: El nombre del rol es obligatorio");

                    // Recargar formulario usando método común
                    model = await PrepararFormularioRolAsync(model.Rol);
                    model.PermisosSeleccionados = PermisosSeleccionados ?? new List<int>();
                    return View("Form", model);
                }

                LogInfo("Creando rol en la base de datos");

                // Crear rol
                var rol = new Rol
                {
                    Nombre = model.Rol.Nombre,
                    Descripcion = model.Rol.Descripcion,
                    EsSistema = false
                };

                var rolID = await _rolService.CreateRolAsync(rol);
                LogInfo("Rol creado con ID: {RolID}", rolID);

                // Usar método común para asignar permisos
                if (PermisosSeleccionados != null && PermisosSeleccionados.Any())
                {
                    LogInfo("Asignando {Count} permisos al rol", PermisosSeleccionados.Count);
                    await ProcesarAsignacionPermisosRolAsync(rolID, PermisosSeleccionados);
                }
                else
                {
                    LogWarning("No se seleccionaron permisos para el rol");
                }

                SetSuccessMessage("Rol creado correctamente");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al crear rol: {Message}", ex.Message);
                ModelState.AddModelError(string.Empty, "Error al crear rol: " + ex.Message);

                // Recargar formulario usando método común
                model = await PrepararFormularioRolAsync(model.Rol);
                model.PermisosSeleccionados = PermisosSeleccionados ?? new List<int>();

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

                // Usar método común para preparar formulario
                var model = await PrepararFormularioRolAsync(rol, esEdicion: true);
                return View("Form", model);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al preparar formulario de edición de rol");
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
                LogInfo("Iniciando edición de rol - POST, ID: {RolID}", id);
                LogInfo("Permisos seleccionados: {Count}", PermisosSeleccionados?.Count ?? 0);

                if (string.IsNullOrWhiteSpace(model.Rol?.Nombre))
                {
                    ModelState.AddModelError("Rol.Nombre", "El nombre del rol es obligatorio");
                    LogWarning("Error de validación: El nombre del rol es obligatorio");

                    // Recargar formulario usando método común
                    model = await PrepararFormularioRolAsync(model.Rol, esEdicion: true);
                    model.PermisosSeleccionados = PermisosSeleccionados ?? new List<int>();
                    return View("Form", model);
                }

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

                // Usar método común para actualizar permisos
                await ProcesarAsignacionPermisosRolAsync(id, PermisosSeleccionados ?? new List<int>());

                SetSuccessMessage("Rol actualizado correctamente");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al actualizar rol: {Message}", ex.Message);
                ModelState.AddModelError(string.Empty, "Error al actualizar rol: " + ex.Message);

                // Recargar formulario usando método común
                model = await PrepararFormularioRolAsync(model.Rol, esEdicion: true);
                model.PermisosSeleccionados = PermisosSeleccionados ?? new List<int>();

                return View("Form", model);
            }
        }

        // GET: Roles/Delete/5
        [Authorize(Policy = "Permission:roles.eliminar")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // Usar método común de validación
                if (!await ValidarRolParaEliminacionAsync(id))
                    return RedirectToAction(nameof(Index));

                var rol = await _rolService.GetRolByIDAsync(id);
                var permisos = await _permisoService.GetAllPermisosAsync();
                var permisosRol = new List<Permiso>();

                foreach (var rolPermiso in rol.Permisos)
                {
                    var permiso = permisos.FirstOrDefault(p => p.PermisoID == rolPermiso.PermisoID);
                    if (permiso != null)
                    {
                        permisosRol.Add(permiso);
                    }
                }

                // Usar método común para crear ViewModel
                var model = CrearRolDetailsViewModel(rol, permisosRol);
                return View(model);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al preparar eliminación de rol");
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
                // Usar método común de validación
                if (!await ValidarRolParaEliminacionAsync(id))
                    return RedirectToAction(nameof(Index));

                await _rolService.DeleteRolAsync(id);
                SetSuccessMessage("Rol eliminado correctamente");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al eliminar rol: {Message}", ex.Message);
                SetErrorMessage("Error al eliminar rol: " + ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }