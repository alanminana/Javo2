// Controllers/Security/SecurityController.cs
using Javo2.Data.Seeders;
using Javo2.Filters.ExceptionHandling;
using Javo2.IServices.Authentication;
using Javo2.Models.Authentication;
using Javo2.Services.Security;
using Javo2.ViewModels.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Javo2.Controllers.Security
{
    [Authorize]
    public class SecurityController : SecurityBaseController
    {
        private readonly ISecurityManagementService _securityManagementService;

        public SecurityController(
            IUsuarioService usuarioService,
            IRolService rolService,
            IPermisoService permisoService,
            IPermissionManagerService permissionManager,
            ISecurityManagementService securityManagementService,
            ILogger<SecurityController> logger)
            : base(usuarioService, rolService, permisoService, permissionManager, logger)
        {
            _securityManagementService = securityManagementService;
        }

        #region Dashboard de Seguridad

        [Authorize(Policy = "Permission:securitydashboard.ver")]
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var dashboardData = await _securityManagementService.ObtenerDatosDashboardAsync();

                var viewModel = new SecurityDashboardViewModel
                {
                    TotalUsuarios = dashboardData.TotalUsuarios,
                    UsuariosActivos = dashboardData.UsuariosActivos,
                    TotalRoles = dashboardData.TotalRoles,
                    TotalPermisos = dashboardData.TotalPermisos,
                    UltimosUsuariosRegistrados = dashboardData.UltimosUsuariosRegistrados
                        .Select(u => new UsuarioSimpleViewModel
                        {
                            UsuarioID = u.UsuarioID,
                            NombreUsuario = u.NombreUsuario,
                            NombreCompleto = u.NombreCompleto,
                            FechaCreacion = u.FechaCreacion
                        }).ToList(),
                    UltimosAccesos = dashboardData.UltimosAccesos
                        .Select(u => new UsuarioSimpleViewModel
                        {
                            UsuarioID = u.UsuarioID,
                            NombreUsuario = u.NombreUsuario,
                            NombreCompleto = u.NombreCompleto,
                            UltimoAcceso = u.UltimoAcceso
                        }).ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                throw new BusinessException("Error al cargar el dashboard de seguridad", "SEC001", ex);
            }
        }

        #endregion

        #region Gestión de Permisos

        [HttpGet]
        [Authorize(Policy = "Permission:permisos.ver")]
        public async Task<IActionResult> Permisos()
        {
            var permisosAgrupados = await _permissionManager.GetPermissionsByGroupAsync();
            return View(permisosAgrupados);
        }

        [HttpGet]
        [Authorize(Policy = "Permission:permisos.ver")]
        public async Task<IActionResult> DetallesPermiso(int id)
        {
            var permisos = await _permisoService.GetAllPermisosAsync();
            var permiso = permisos.FirstOrDefault(p => p.PermisoID == id);
            if (permiso == null) return NotFound();
            return View(permiso);
        }

        [HttpGet]
        [Authorize(Policy = "Permission:permisos.crear")]
        public IActionResult CrearPermiso()
        {
            var permiso = new Permiso { Activo = true };
            return View("FormPermiso", permiso);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:permisos.crear")]
        public async Task<IActionResult> CrearPermiso(Permiso permiso)
        {
            if (!ModelState.IsValid)
                return View("FormPermiso", permiso);

            // Usar método común de validación
            if (!await ValidarCodigoPermisoUnicoAsync(permiso.Codigo))
                return View("FormPermiso", permiso);

            var result = await _permissionManager.CreateOrUpdatePermissionAsync(permiso);
            if (!result)
            {
                ModelState.AddModelError(string.Empty, "No se pudo crear el permiso");
                return View("FormPermiso", permiso);
            }

            SetSuccessMessage("Permiso creado correctamente");
            return RedirectToAction(nameof(Permisos));
        }

        [HttpGet]
        [Authorize(Policy = "Permission:permisos.editar")]
        public async Task<IActionResult> EditarPermiso(int id)
        {
            var permisos = await _permisoService.GetAllPermisosAsync();
            var permiso = permisos.FirstOrDefault(p => p.PermisoID == id);
            if (permiso == null) return NotFound();

            if (permiso.EsSistema)
            {
                SetErrorMessage("No se pueden editar permisos del sistema");
                return RedirectToAction(nameof(Permisos));
            }
            return View("FormPermiso", permiso);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:permisos.editar")]
        public async Task<IActionResult> EditarPermiso(int id, Permiso permiso)
        {
            if (id != permiso.PermisoID) return NotFound();

            if (!ModelState.IsValid)
                return View("FormPermiso", permiso);

            var permisos = await _permisoService.GetAllPermisosAsync();
            var original = permisos.FirstOrDefault(p => p.PermisoID == id);
            if (original == null) return NotFound();

            if (original.EsSistema)
            {
                SetErrorMessage("No se pueden editar permisos del sistema");
                return RedirectToAction(nameof(Permisos));
            }

            // Usar método común de validación
            if (!await ValidarCodigoPermisoUnicoAsync(permiso.Codigo, id))
                return View("FormPermiso", permiso);

            permiso.EsSistema = original.EsSistema;
            var result = await _permissionManager.CreateOrUpdatePermissionAsync(permiso);
            if (!result)
            {
                ModelState.AddModelError(string.Empty, "No se pudo actualizar el permiso");
                return View("FormPermiso", permiso);
            }

            SetSuccessMessage("Permiso actualizado correctamente");
            return RedirectToAction(nameof(Permisos));
        }

        [HttpGet]
        [Authorize(Policy = "Permission:permisos.eliminar")]
        public async Task<IActionResult> EliminarPermiso(int id)
        {
            // Usar método común de validación
            if (!await ValidarPermisoParaEliminacionAsync(id))
                return RedirectToAction(nameof(Permisos));

            var permisos = await _permisoService.GetAllPermisosAsync();
            var permiso = permisos.FirstOrDefault(p => p.PermisoID == id);
            return View(permiso);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:permisos.eliminar")]
        public async Task<IActionResult> EliminarPermisoConfirmado(int id)
        {
            // Usar método común de validación
            if (!await ValidarPermisoParaEliminacionAsync(id))
                return RedirectToAction(nameof(Permisos));

            var result = await _permissionManager.DeletePermissionAsync(id);
            if (!result)
            {
                SetErrorMessage("No se pudo eliminar el permiso");
                return RedirectToAction(nameof(Permisos));
            }

            SetSuccessMessage("Permiso eliminado correctamente");
            return RedirectToAction(nameof(Permisos));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:permisos.editar")]
        public async Task<IActionResult> CambiarEstadoPermiso(int id)
        {
            var result = await _permissionManager.TogglePermissionStatusAsync(id);
            if (!result)
                return JsonError("No se pudo cambiar el estado del permiso");

            var permisos = await _permisoService.GetAllPermisosAsync();
            var permiso = permisos.FirstOrDefault(p => p.PermisoID == id);
            if (permiso == null)
                return JsonError("Permiso no encontrado");

            return JsonSuccess(
                $"Permiso {(permiso.Activo ? "activado" : "desactivado")} correctamente",
                new { estado = permiso.Activo }
            );
        }

        #endregion

        #region Gestión de Roles

        [HttpGet]
        [Authorize(Policy = "Permission:roles.ver")]
        public async Task<IActionResult> Roles()
        {
            var roles = await _rolService.GetAllRolesAsync();
            return View(roles);
        }

        [HttpGet]
        [Authorize(Policy = "Permission:roles.ver")]
        public async Task<IActionResult> DetallesRol(int id)
        {
            var rol = await _rolService.GetRolByIDAsync(id);
            if (rol == null)
                return NotFound();

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

        [HttpGet]
        [Authorize(Policy = "Permission:roles.crear")]
        public async Task<IActionResult> CrearRol()
        {
            // Usar método común para preparar formulario
            var model = await PrepararFormularioRolAsync();
            return View("FormRol", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:roles.crear")]
        public async Task<IActionResult> CrearRol(RolFormViewModel model, List<int> PermisosSeleccionados)
        {
            if (string.IsNullOrWhiteSpace(model.Rol?.Nombre))
            {
                ModelState.AddModelError("Rol.Nombre", "El nombre del rol es obligatorio");
                model = await PrepararFormularioRolAsync(model.Rol);
                model.PermisosSeleccionados = PermisosSeleccionados ?? new List<int>();
                return View("FormRol", model);
            }

            // Crear rol
            var rol = new Rol
            {
                Nombre = model.Rol.Nombre,
                Descripcion = model.Rol.Descripcion,
                EsSistema = false
            };

            var rolId = await _rolService.CreateRolAsync(rol);
            if (rolId <= 0)
            {
                ModelState.AddModelError(string.Empty, "Error al crear el rol");
                model = await PrepararFormularioRolAsync(model.Rol);
                model.PermisosSeleccionados = PermisosSeleccionados ?? new List<int>();
                return View("FormRol", model);
            }

            // Usar método común para asignar permisos
            if (PermisosSeleccionados != null && PermisosSeleccionados.Any())
            {
                await ProcesarAsignacionPermisosRolAsync(rolId, PermisosSeleccionados);
            }

            SetSuccessMessage("Rol creado correctamente");
            return RedirectToAction(nameof(Roles));
        }

        [HttpGet]
        [Authorize(Policy = "Permission:roles.editar")]
        public async Task<IActionResult> EditarRol(int id)
        {
            var rol = await _rolService.GetRolByIDAsync(id);
            if (rol == null)
                return NotFound();

            // Usar método común para preparar formulario
            var model = await PrepararFormularioRolAsync(rol, esEdicion: true);
            return View("FormRol", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:roles.editar")]
        public async Task<IActionResult> EditarRol(int id, RolFormViewModel model, List<int> PermisosSeleccionados)
        {
            if (id != model.Rol.RolID)
                return NotFound();

            if (string.IsNullOrWhiteSpace(model.Rol?.Nombre))
            {
                ModelState.AddModelError("Rol.Nombre", "El nombre del rol es obligatorio");
                model = await PrepararFormularioRolAsync(model.Rol, esEdicion: true);
                model.PermisosSeleccionados = PermisosSeleccionados ?? new List<int>();
                return View("FormRol", model);
            }

            var originalRol = await _rolService.GetRolByIDAsync(id);
            if (originalRol == null)
                return NotFound();

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
            var result = await _rolService.UpdateRolAsync(originalRol);
            if (!result)
            {
                ModelState.AddModelError(string.Empty, "Error al actualizar el rol");
                model = await PrepararFormularioRolAsync(model.Rol, esEdicion: true);
                model.PermisosSeleccionados = PermisosSeleccionados ?? new List<int>();
                return View("FormRol", model);
            }

            // Usar método común para actualizar permisos
            await ProcesarAsignacionPermisosRolAsync(id, PermisosSeleccionados ?? new List<int>());

            SetSuccessMessage("Rol actualizado correctamente");
            return RedirectToAction(nameof(Roles));
        }

        [HttpGet]
        [Authorize(Policy = "Permission:roles.eliminar")]
        public async Task<IActionResult> EliminarRol(int id)
        {
            // Usar método común de validación
            if (!await ValidarRolParaEliminacionAsync(id))
                return RedirectToAction(nameof(Roles));

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

        [HttpPost, ActionName("EliminarRol")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:roles.eliminar")]
        public async Task<IActionResult> EliminarRolConfirmado(int id)
        {
            // Usar método común de validación
            if (!await ValidarRolParaEliminacionAsync(id))
                return RedirectToAction(nameof(Roles));

            var result = await _rolService.DeleteRolAsync(id);
            if (!result)
            {
                SetErrorMessage("Error al eliminar el rol");
                return RedirectToAction(nameof(Roles));
            }

            SetSuccessMessage("Rol eliminado correctamente");
            return RedirectToAction(nameof(Roles));
        }

        #endregion

        #region Herramientas de Seguridad

        [Authorize(Roles = "Administrador")]
        public IActionResult Herramientas()
        {
            return View();
        }

        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> RecargarPermisos()
        {
            try
            {
                var result = await _securityManagementService.RecargarPermisosAsync();
                if (result)
                {
                    SetSuccessMessage("Permisos recargados correctamente");
                }
                else
                {
                    SetErrorMessage("Error al recargar permisos");
                }
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al recargar permisos");
                SetErrorMessage("Error al recargar permisos: " + ex.Message);
            }

            return RedirectToAction(nameof(Dashboard));
        }

        [Authorize(Roles = "Administrador")]
        public IActionResult CerrarSesiones()
        {
            // Implementación futura para cerrar sesiones activas
            SetInfoMessage("Función no implementada todavía");
            return RedirectToAction(nameof(Dashboard));
        }

        #endregion

        #region Reparación de Permisos

        [AllowAnonymous]
        public async Task<IActionResult> VerificarPermisos()
        {
            // Usar método común de verificación
            var result = await VerificarPermisosUsuarioAsync();

            if (!result.Success)
            {
                return Json(new { success = false, message = result.Message });
            }

            return Json(new
            {
                success = true,
                usuario = new
                {
                    username = result.Usuario.Username,
                    nombre = result.Usuario.Nombre,
                    roles = result.Usuario.Roles,
                    tieneRolAdmin = result.Usuario.TieneRolAdmin,
                    permisos = result.Usuario.Permisos,
                    tienePermisoDashboard = result.Usuario.TienePermisoDashboard
                }
            });
        }

        [AllowAnonymous]
        public async Task<IActionResult> RepararPermisos()
        {
            try
            {
                // Usar método común de reparación
                var result = await RepararPermisosAdministradorAsync();

                var content = "<h1>Resultados de la reparación de permisos</h1><ul>" +
                    string.Join("", result.Messages.Select(r => $"<li>{r}</li>")) +
                    "</ul>";

                if (result.Success)
                {
                    content += "<p><strong>IMPORTANTE:</strong> Es necesario cerrar sesión y volver a iniciar para aplicar los cambios de permisos</p>";
                }

                return Content(content, "text/html");
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al reparar permisos de administrador");
                return Content($"Error: {ex.Message}<br/>{ex.StackTrace}");
            }
        }

        #endregion
    }
}