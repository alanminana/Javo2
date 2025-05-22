// Controllers/Security/UsuariosController.cs
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

namespace Javo2.Controllers.Security
{
    [Authorize]
    public class UsuariosController : SecurityBaseController
    {
        public UsuariosController(
            IUsuarioService usuarioService,
            IRolService rolService,
            IPermisoService permisoService,
            IPermissionManagerService permissionManager,
            ILogger<UsuariosController> logger)
            : base(usuarioService, rolService, permisoService, permissionManager, logger)
        {
        }

        // GET: Usuarios
        [HttpGet]
        [Authorize(Policy = "Permission:usuarios.ver")]
        public async Task<IActionResult> Index(UsuarioFilterViewModel filtro = null)
        {
            try
            {
                if (filtro == null)
                    filtro = new UsuarioFilterViewModel();

                var roles = await _rolService.GetAllRolesAsync();
                ViewBag.Roles = roles.Select(r => new SelectListItem
                {
                    Value = r.RolID.ToString(),
                    Text = r.Nombre
                });

                IEnumerable<Usuario> usuarios;
                if (!string.IsNullOrEmpty(filtro.Termino))
                    usuarios = await _usuarioService.BuscarUsuariosAsync(filtro.Termino);
                else
                    usuarios = await _usuarioService.GetAllUsuariosAsync();

                if (filtro.Activo.HasValue)
                    usuarios = usuarios.Where(u => u.Activo == filtro.Activo.Value);

                if (filtro.RolID > 0)
                    usuarios = usuarios.Where(u => u.Roles.Any(r => r.RolID == filtro.RolID));

                return View(usuarios);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al obtener la lista de usuarios");
                return View("Error");
            }
        }

        // GET: Usuarios/Details/5
        [HttpGet]
        [Authorize(Policy = "Permission:usuarios.ver")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var usuario = await _usuarioService.GetUsuarioByIDAsync(id);
                if (usuario == null)
                    return NotFound();

                var roles = new List<Rol>();
                foreach (var usuarioRol in usuario.Roles)
                {
                    var rol = await _rolService.GetRolByIDAsync(usuarioRol.RolID);
                    if (rol != null)
                        roles.Add(rol);
                }

                // Usar método común para crear ViewModel
                var model = CrearUsuarioDetailsViewModel(usuario, roles);
                return View(model);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al obtener detalles del usuario");
                return View("Error");
            }
        }

        // GET: Usuarios/Create
        [HttpGet]
        [Authorize(Policy = "Permission:usuarios.crear")]
        public async Task<IActionResult> Create()
        {
            try
            {
                LogInfo("Preparando formulario de creación de usuario");

                // Usar método común para preparar formulario
                var model = await PrepararFormularioUsuarioAsync();
                return View("Form", model);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al preparar formulario de creación de usuario");
                return View("Error");
            }
        }

        // POST: Usuarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:usuarios.crear")]
        public async Task<IActionResult> Create(UsuarioFormViewModel model, List<int> RolesSeleccionados)
        {
            try
            {
                LogInfo("Iniciando creación de usuario: {NombreUsuario}", model.Usuario?.NombreUsuario);

                // Usar método común de validación
                if (!await ValidarDatosUsuarioAsync(model, esEdicion: false))
                {
                    model = await PrepararFormularioUsuarioAsync(model.Usuario);
                    model.RolesSeleccionados = RolesSeleccionados ?? new List<int>();
                    return View("Form", model);
                }

                model.Usuario.CreadoPor = User.Identity?.Name ?? "Sistema";
                var result = await _usuarioService.CreateUsuarioAsync(model.Usuario, model.Contraseña);
                if (!result)
                    throw new Exception("No se pudo crear el usuario");

                // Usar método común para asignar roles
                if (RolesSeleccionados != null && RolesSeleccionados.Any())
                {
                    await ProcesarAsignacionRolesUsuarioAsync(model.Usuario.UsuarioID, RolesSeleccionados);
                }

                SetSuccessMessage("Usuario creado correctamente");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al crear usuario: {Message}", ex.Message);
                ModelState.AddModelError(string.Empty, "Error al crear usuario: " + ex.Message);

                // Recargar formulario usando método común
                model = await PrepararFormularioUsuarioAsync(model.Usuario);
                model.RolesSeleccionados = RolesSeleccionados ?? new List<int>();

                return View("Form", model);
            }
        }

        // GET: Usuarios/Edit/5
        [HttpGet]
        [Authorize(Policy = "Permission:usuarios.editar")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var usuario = await _usuarioService.GetUsuarioByIDAsync(id);
                if (usuario == null)
                    return NotFound();

                // Usar método común para preparar formulario
                var model = await PrepararFormularioUsuarioAsync(usuario, esEdicion: true);
                return View("Form", model);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al preparar formulario de edición de usuario");
                return View("Error");
            }
        }

        // POST: Usuarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:usuarios.editar")]
        public async Task<IActionResult> Edit(int id, UsuarioFormViewModel model, List<int> RolesSeleccionados)
        {
            if (id != model.Usuario.UsuarioID)
                return NotFound();

            try
            {
                // Usar método común de validación
                if (!await ValidarDatosUsuarioAsync(model, esEdicion: true))
                {
                    model = await PrepararFormularioUsuarioAsync(model.Usuario, esEdicion: true);
                    model.RolesSeleccionados = RolesSeleccionados ?? new List<int>();
                    return View("Form", model);
                }

                var result = await _usuarioService.UpdateUsuarioAsync(model.Usuario, model.Contraseña);
                if (!result)
                    throw new Exception("No se pudo actualizar el usuario");

                // Usar método común para actualizar roles
                await ProcesarAsignacionRolesUsuarioAsync(id, RolesSeleccionados ?? new List<int>());

                SetSuccessMessage("Usuario actualizado correctamente");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al actualizar usuario: {Message}", ex.Message);
                ModelState.AddModelError(string.Empty, "Error al actualizar usuario: " + ex.Message);

                // Recargar formulario usando método común
                model = await PrepararFormularioUsuarioAsync(model.Usuario, esEdicion: true);
                model.RolesSeleccionados = RolesSeleccionados ?? new List<int>();

                return View("Form", model);
            }
        }

        // GET: Usuarios/Delete/5
        [HttpGet]
        [Authorize(Policy = "Permission:usuarios.eliminar")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var usuario = await _usuarioService.GetUsuarioByIDAsync(id);
                if (usuario == null)
                    return NotFound();

                var roles = new List<Rol>();
                foreach (var usuarioRol in usuario.Roles)
                {
                    var rol = await _rolService.GetRolByIDAsync(usuarioRol.RolID);
                    if (rol != null)
                        roles.Add(rol);
                }

                // Usar método común para crear ViewModel
                var model = CrearUsuarioDetailsViewModel(usuario, roles);
                return View(model);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al preparar eliminación de usuario");
                return View("Error");
            }
        }

        // POST: Usuarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:usuarios.eliminar")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var result = await _usuarioService.DeleteUsuarioAsync(id);
                if (!result)
                    throw new Exception("No se pudo eliminar el usuario");

                SetSuccessMessage("Usuario eliminado correctamente");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al eliminar usuario: {Message}", ex.Message);
                SetErrorMessage("Error al eliminar usuario: " + ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Usuarios/ToggleEstado/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:usuarios.editar")]
        public async Task<IActionResult> ToggleEstado(int id)
        {
            try
            {
                var result = await _usuarioService.ToggleEstadoAsync(id);
                if (!result)
                    return JsonError("No se pudo actualizar el estado del usuario");

                var usuario = await _usuarioService.GetUsuarioByIDAsync(id);
                return JsonSuccess(
                    $"Usuario {(usuario.Activo ? "activado" : "desactivado")} correctamente",
                    new { estado = usuario.Activo }
                );
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al cambiar estado de usuario");
                return JsonError("Error al cambiar estado del usuario: " + ex.Message);
            }
        }
    }
}