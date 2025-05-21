// Controllers/Security/SecurityController.cs
using Javo2.Controllers.Base;
using Javo2.Data.Seeders;
using Javo2.Filters.ExceptionHandling;
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
    [Authorize]
    public class SecurityController : BaseController
    {
        private readonly IPermissionManagerService _permissionManager;
        private readonly IUsuarioService _usuarioService;

        public SecurityController(
            IPermissionManagerService permissionManager,
            IUsuarioService usuarioService,
            ILogger<SecurityController> logger) : base(logger)
        {
            _permissionManager = permissionManager;
            _usuarioService = usuarioService;
        }

        #region Dashboard de Seguridad

        [Authorize(Policy = "Permission:securitydashboard.ver")]
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                // Obtener estadísticas para el dashboard
                var usuarios = await _usuarioService.GetAllUsuariosAsync();
                var roles = await _permissionManager.GetAllRolesAsync();
                var permisos = await _permissionManager.GetAllPermissionsAsync();

                var viewModel = new SecurityDashboardViewModel
                {
                    TotalUsuarios = usuarios.Count(),
                    UsuariosActivos = usuarios.Count(u => u.Activo),
                    TotalRoles = roles.Count(),
                    TotalPermisos = permisos.Count(),
                    UltimosUsuariosRegistrados = usuarios
                        .OrderByDescending(u => u.FechaCreacion)
                        .Take(5)
                        .Select(u => new UsuarioSimpleViewModel
                        {
                            UsuarioID = u.UsuarioID,
                            NombreUsuario = u.NombreUsuario,
                            NombreCompleto = $"{u.Nombre} {u.Apellido}",
                            FechaCreacion = u.FechaCreacion
                        })
                        .ToList(),
                    UltimosAccesos = usuarios
                        .Where(u => u.UltimoAcceso.HasValue)
                        .OrderByDescending(u => u.UltimoAcceso)
                        .Take(5)
                        .Select(u => new UsuarioSimpleViewModel
                        {
                            UsuarioID = u.UsuarioID,
                            NombreUsuario = u.NombreUsuario,
                            NombreCompleto = $"{u.Nombre} {u.Apellido}",
                            UltimoAcceso = u.UltimoAcceso
                        })
                        .ToList()
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
            var permisos = await _permissionManager.GetAllPermissionsAsync();
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

            var permisos = await _permissionManager.GetAllPermissionsAsync();
            var existente = permisos.FirstOrDefault(p => p.Codigo == permiso.Codigo);
            if (existente != null)
            {
                ModelState.AddModelError("Codigo", "Ya existe un permiso con este código");
                return View("FormPermiso", permiso);
            }

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
            var permisos = await _permissionManager.GetAllPermissionsAsync();
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

            var permisos = await _permissionManager.GetAllPermissionsAsync();
            var original = permisos.FirstOrDefault(p => p.PermisoID == id);
            if (original == null) return NotFound();
            if (original.EsSistema)
            {
                SetErrorMessage("No se pueden editar permisos del sistema");
                return RedirectToAction(nameof(Permisos));
            }

            var existente = permisos.FirstOrDefault(p => p.Codigo == permiso.Codigo && p.PermisoID != id);
            if (existente != null)
            {
                ModelState.AddModelError("Codigo", "Ya existe un permiso con este código");
                return View("FormPermiso", permiso);
            }

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
            var permisos = await _permissionManager.GetAllPermissionsAsync();
            var permiso = permisos.FirstOrDefault(p => p.PermisoID == id);
            if (permiso == null) return NotFound();
            if (permiso.EsSistema)
            {
                SetErrorMessage("No se pueden eliminar permisos del sistema");
                return RedirectToAction(nameof(Permisos));
            }
            return View(permiso);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:permisos.eliminar")]
        public async Task<IActionResult> EliminarPermisoConfirmado(int id)
        {
            var permisos = await _permissionManager.GetAllPermissionsAsync();
            var permiso = permisos.FirstOrDefault(p => p.PermisoID == id);
            if (permiso == null) return NotFound();
            if (permiso.EsSistema)
            {
                SetErrorMessage("No se pueden eliminar permisos del sistema");
                return RedirectToAction(nameof(Permisos));
            }

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

            var permisos = await _permissionManager.GetAllPermissionsAsync();
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
            var roles = await _permissionManager.GetAllRolesAsync();
            return View(roles);
        }

        [HttpGet]
        [Authorize(Policy = "Permission:roles.ver")]
        public async Task<IActionResult> DetallesRol(int id)
        {
            var rol = await _permissionManager.GetRoleByIdAsync(id);
            if (rol == null)
                return NotFound();

            var permisos = await _permissionManager.GetAllPermissionsAsync();
            var permisosRol = new List<Permiso>();

            foreach (var rolPermiso in rol.Permisos)
            {
                var permiso = permisos.FirstOrDefault(p => p.PermisoID == rolPermiso.PermisoID);
                if (permiso != null)
                {
                    permisosRol.Add(permiso);
                }
            }

            var model = new RolDetailsViewModel
            {
                Rol = rol,
                Permisos = permisosRol
            };

            return View(model);
        }

        [HttpGet]
        [Authorize(Policy = "Permission:roles.crear")]
        public async Task<IActionResult> CrearRol()
        {
            var permisos = await _permissionManager.GetAllPermissionsAsync();
            var permisosActivos = permisos.Where(p => p.Activo).ToList();

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

                var permisos = await _permissionManager.GetAllPermissionsAsync();
                var permisosActivos = permisos.Where(p => p.Activo).ToList();
                var gruposPermisos = permisosActivos
                    .GroupBy(p => p.Grupo ?? "General")
                    .ToDictionary(g => g.Key, g => g.ToList());

                model.GruposPermisos = gruposPermisos;
                model.PermisosSeleccionados = PermisosSeleccionados ?? new List<int>();
                return View("FormRol", model);
            }

            // Crear rol
            var rol = new Rol
            {
                Nombre = model.Rol.Nombre,
                Descripcion = model.Rol.Descripcion,
                EsSistema = false // Los roles creados manualmente nunca son del sistema
            };

            var result = await _permissionManager.CreateOrUpdateRoleAsync(rol);
            if (!result)
            {
                ModelState.AddModelError(string.Empty, "Error al crear el rol");

                var permisos = await _permissionManager.GetAllPermissionsAsync();
                var permisosActivos = permisos.Where(p => p.Activo).ToList();
                var gruposPermisos = permisosActivos
                    .GroupBy(p => p.Grupo ?? "General")
                    .ToDictionary(g => g.Key, g => g.ToList());

                model.GruposPermisos = gruposPermisos;
                model.PermisosSeleccionados = PermisosSeleccionados ?? new List<int>();
                return View("FormRol", model);
            }

            // Obtener el ID del rol recién creado
            var roles = await _permissionManager.GetAllRolesAsync();
            var rolCreado = roles.FirstOrDefault(r => r.Nombre == rol.Nombre);
            if (rolCreado != null && PermisosSeleccionados != null && PermisosSeleccionados.Any())
            {
                await _permissionManager.AssignPermissionsToRoleAsync(rolCreado.RolID, PermisosSeleccionados);
            }

            SetSuccessMessage("Rol creado correctamente");
            return RedirectToAction(nameof(Roles));
        }

        [HttpGet]
        [Authorize(Policy = "Permission:roles.editar")]
        public async Task<IActionResult> EditarRol(int id)
        {
            var rol = await _permissionManager.GetRoleByIdAsync(id);
            if (rol == null)
                return NotFound();

            // Obtener permisos asignados
            var permisosIds = rol.Permisos.Select(p => p.PermisoID).ToList();

            // Obtener todos los permisos
            var permisos = await _permissionManager.GetAllPermissionsAsync();
            var permisosActivos = permisos.Where(p => p.Activo).ToList();

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

                var permisos = await _permissionManager.GetAllPermissionsAsync();
                var permisosActivos = permisos.Where(p => p.Activo).ToList();
                var gruposPermisos = permisosActivos
                    .GroupBy(p => p.Grupo ?? "General")
                    .ToDictionary(g => g.Key, g => g.ToList());

                model.GruposPermisos = gruposPermisos;
                model.PermisosSeleccionados = PermisosSeleccionados ?? new List<int>();
                model.EsEdicion = true;
                return View("FormRol", model);
            }

            var originalRol = await _permissionManager.GetRoleByIdAsync(id);
            if (originalRol == null)
                return NotFound();

            // Si es un rol del sistema, solo permitir cambiar la descripción
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
            var result = await _permissionManager.CreateOrUpdateRoleAsync(originalRol);
            if (!result)
            {
                ModelState.AddModelError(string.Empty, "Error al actualizar el rol");

                var permisos = await _permissionManager.GetAllPermissionsAsync();
                var permisosActivos = permisos.Where(p => p.Activo).ToList();
                var gruposPermisos = permisosActivos
                    .GroupBy(p => p.Grupo ?? "General")
                    .ToDictionary(g => g.Key, g => g.ToList());

                model.GruposPermisos = gruposPermisos;
                model.PermisosSeleccionados = PermisosSeleccionados ?? new List<int>();
                model.EsEdicion = true;
                return View("FormRol", model);
            }

            // Actualizar permisos
            await _permissionManager.AssignPermissionsToRoleAsync(id, PermisosSeleccionados ?? new List<int>());

            SetSuccessMessage("Rol actualizado correctamente");
            return RedirectToAction(nameof(Roles));
        }

        [HttpGet]
        [Authorize(Policy = "Permission:roles.eliminar")]
        public async Task<IActionResult> EliminarRol(int id)
        {
            var rol = await _permissionManager.GetRoleByIdAsync(id);
            if (rol == null)
                return NotFound();

            // No permitir eliminar roles del sistema
            if (rol.EsSistema)
            {
                SetErrorMessage("No se pueden eliminar roles del sistema");
                return RedirectToAction(nameof(Roles));
            }

            // Obtener permisos del rol
            var permisos = await _permissionManager.GetAllPermissionsAsync();
            var permisosRol = new List<Permiso>();

            foreach (var rolPermiso in rol.Permisos)
            {
                var permiso = permisos.FirstOrDefault(p => p.PermisoID == rolPermiso.PermisoID);
                if (permiso != null)
                {
                    permisosRol.Add(permiso);
                }
            }

            var model = new RolDetailsViewModel
            {
                Rol = rol,
                Permisos = permisosRol
            };

            return View(model);
        }

        [HttpPost, ActionName("EliminarRol")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:roles.eliminar")]
        public async Task<IActionResult> EliminarRolConfirmado(int id)
        {
            var rol = await _permissionManager.GetRoleByIdAsync(id);
            if (rol == null)
                return NotFound();

            // No permitir eliminar roles del sistema
            if (rol.EsSistema)
            {
                SetErrorMessage("No se pueden eliminar roles del sistema");
                return RedirectToAction(nameof(Roles));
            }

            var result = await _permissionManager.DeleteRoleAsync(id);
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
                // Obtener las dependencias del contenedor
                var permisoService = HttpContext.RequestServices.GetRequiredService<IPermisoService>();
                var rolService = HttpContext.RequestServices.GetRequiredService<IRolService>();
                var loggerFactory = HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
                var permissionLogger = loggerFactory.CreateLogger<PermissionSeeder>();

                var seeder = new PermissionSeeder(permisoService, rolService, permissionLogger);
                await seeder.SeedAsync();

                SetSuccessMessage("Permisos recargados correctamente");
                return RedirectToAction(nameof(Dashboard));
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al recargar permisos");
                SetErrorMessage("Error al recargar permisos: " + ex.Message);
                return RedirectToAction(nameof(Dashboard));
            }
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
            try
            {
                var username = User.Identity.Name;
                if (string.IsNullOrEmpty(username))
                {
                    return Json(new { success = false, message = "Usuario no autenticado" });
                }

                var usuario = await _usuarioService.GetUsuarioByNombreUsuarioAsync(username);
                if (usuario == null)
                {
                    return Json(new { success = false, message = "Usuario no encontrado" });
                }

                var roles = new List<string>();
                foreach (var usuarioRol in usuario.Roles)
                {
                    var rol = await _permissionManager.GetRoleByIdAsync(usuarioRol.RolID);
                    if (rol != null)
                    {
                        roles.Add(rol.Nombre);
                    }
                }

                var permisos = await _usuarioService.GetPermisosUsuarioAsync(usuario.UsuarioID);
                var permisosLista = permisos.Select(p => p.Codigo).ToList();

                var tienePermisoDashboard = permisosLista.Contains("securitydashboard.ver");

                return Json(new
                {
                    success = true,
                    usuario = new
                    {
                        username = usuario.NombreUsuario,
                        nombre = $"{usuario.Nombre} {usuario.Apellido}",
                        roles = roles,
                        tieneRolAdmin = roles.Any(r => r.Equals("Administrador", StringComparison.OrdinalIgnoreCase)),
                        permisos = permisosLista,
                        tienePermisoDashboard = tienePermisoDashboard
                    }
                });
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al verificar permisos");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [AllowAnonymous]
        public async Task<IActionResult> RepararPermisos()
        {
            try
            {
                var resultados = new List<string>();

                // 1. Encontrar el rol Administrador
                var roles = await _permissionManager.GetAllRolesAsync();
                var rolAdmin = roles.FirstOrDefault(r => r.Nombre.Equals("Administrador", StringComparison.OrdinalIgnoreCase));

                if (rolAdmin == null)
                {
                    return Content("Error: No se encontró el rol Administrador");
                }

                resultados.Add($"Rol Administrador encontrado con ID: {rolAdmin.RolID}");

                // 2. Obtener todos los permisos disponibles
                var permisos = await _permissionManager.GetAllPermissionsAsync();
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
                if (permisosFaltantes.Any())
                {
                    await _permissionManager.AssignPermissionsToRoleAsync(
                        rolAdmin.RolID,
                        permisosFaltantes.Select(p => p.PermisoID)
                    );

                    resultados.Add($"Se han asignado {permisosFaltantes.Count} permisos faltantes");
                }

                resultados.Add("IMPORTANTE: Es necesario cerrar sesión y volver a iniciar para aplicar los cambios de permisos");

                return Content("<h1>Resultados de la reparación de permisos</h1><ul>" +
                    string.Join("", resultados.Select(r => $"<li>{r}</li>")) +
                    "</ul><p><strong>IMPORTANTE:</strong> Es necesario cerrar sesión y volver a iniciar para aplicar los cambios de permisos</p>",
                    "text/html");
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