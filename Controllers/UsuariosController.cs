// Controllers/UsuariosController.cs
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

namespace Javo2.Controllers
{
    [Authorize]  // Fuerza que el usuario esté autenticado
    public class UsuariosController : BaseController
    {
        private readonly IUsuarioService _usuarioService;
        private readonly IRolService _rolService;

        public UsuariosController(
            IUsuarioService usuarioService,
            IRolService rolService,
            ILogger<UsuariosController> logger)
            : base(logger)
        {
            _usuarioService = usuarioService;
            _rolService = rolService;
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
                _logger.LogError(ex, "Error al obtener la lista de usuarios");
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

                var model = new UsuarioDetailsViewModel
                {
                    Usuario = usuario,
                    Roles = roles
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener detalles del usuario");
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
                _logger.LogInformation("Preparando formulario de creación de usuario");

                var roles = await _rolService.GetAllRolesAsync();
                var rolesItems = roles.Select(r => new SelectListItem
                {
                    Value = r.RolID.ToString(),
                    Text = r.Nombre
                }).ToList();

                var model = new UsuarioFormViewModel
                {
                    Usuario = new Usuario { Activo = true },
                    RolesDisponibles = rolesItems,
                    RolesSeleccionados = new List<int>(),
                    EsEdicion = false
                };

                return View("Form", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al preparar formulario de creación de usuario");
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
                _logger.LogInformation("Iniciando creación de usuario: {NombreUsuario}", model.Usuario?.NombreUsuario);

                var roles = await _rolService.GetAllRolesAsync();
                var rolesItems = roles.Select(r => new SelectListItem
                {
                    Value = r.RolID.ToString(),
                    Text = r.Nombre
                }).ToList();

                model.RolesDisponibles = rolesItems;
                model.RolesSeleccionados = RolesSeleccionados ?? new List<int>();

                bool isValid = true;
                if (model.Usuario == null)
                {
                    ModelState.AddModelError("Usuario", "La información del usuario es obligatoria");
                    isValid = false;
                }
                else
                {
                    if (string.IsNullOrEmpty(model.Usuario.NombreUsuario))
                    {
                        ModelState.AddModelError("Usuario.NombreUsuario", "El nombre de usuario es obligatorio");
                        isValid = false;
                    }
                    if (string.IsNullOrEmpty(model.Usuario.Email))
                    {
                        ModelState.AddModelError("Usuario.Email", "El email es obligatorio");
                        isValid = false;
                    }
                }

                if (string.IsNullOrEmpty(model.Contraseña))
                {
                    ModelState.AddModelError("Contraseña", "La contraseña es obligatoria");
                    isValid = false;
                }
                else if (model.Contraseña.Length < 6)
                {
                    ModelState.AddModelError("Contraseña", "La contraseña debe tener al menos 6 caracteres");
                    isValid = false;
                }

                if (string.IsNullOrEmpty(model.ConfirmarContraseña))
                {
                    ModelState.AddModelError("ConfirmarContraseña", "La confirmación de contraseña es obligatoria");
                    isValid = false;
                }
                else if (model.Contraseña != model.ConfirmarContraseña)
                {
                    ModelState.AddModelError("ConfirmarContraseña", "Las contraseñas no coinciden");
                    isValid = false;
                }

                if (!isValid)
                    return View("Form", model);

                model.Usuario.CreadoPor = User.Identity?.Name ?? "Sistema";
                var result = await _usuarioService.CreateUsuarioAsync(model.Usuario, model.Contraseña);
                if (!result)
                    throw new Exception("No se pudo crear el usuario");

                if (RolesSeleccionados != null && RolesSeleccionados.Any())
                {
                    foreach (var rolId in RolesSeleccionados)
                        await _usuarioService.AsignarRolAsync(model.Usuario.UsuarioID, rolId);
                }

                TempData["Success"] = "Usuario creado correctamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario: {Message}", ex.Message);
                ModelState.AddModelError(string.Empty, "Error al crear usuario: " + ex.Message);

                var roles = await _rolService.GetAllRolesAsync();
                var rolesItems = roles.Select(r => new SelectListItem
                {
                    Value = r.RolID.ToString(),
                    Text = r.Nombre
                }).ToList();

                model.RolesDisponibles = rolesItems;
                model.RolesSeleccionados = RolesSeleccionados ?? new List<int>();
                model.EsEdicion = false;

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

                var roles = await _rolService.GetAllRolesAsync();
                var rolesItems = roles.Select(r => new SelectListItem
                {
                    Value = r.RolID.ToString(),
                    Text = r.Nombre
                }).ToList();

                var selected = usuario.Roles.Select(r => r.RolID).ToList();

                var model = new UsuarioFormViewModel
                {
                    Usuario = usuario,
                    RolesDisponibles = rolesItems,
                    RolesSeleccionados = selected,
                    EsEdicion = true
                };

                return View("Form", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al preparar formulario de edición de usuario");
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
                if (!ModelState.IsValid)
                {
                    var roles = await _rolService.GetAllRolesAsync();
                    var rolesItems = roles.Select(r => new SelectListItem
                    {
                        Value = r.RolID.ToString(),
                        Text = r.Nombre
                    }).ToList();

                    model.RolesDisponibles = rolesItems;
                    model.RolesSeleccionados = RolesSeleccionados ?? new List<int>();
                    model.EsEdicion = true;
                    return View("Form", model);
                }

                var result = await _usuarioService.UpdateUsuarioAsync(model.Usuario, model.Contraseña);
                if (!result)
                    throw new Exception("No se pudo actualizar el usuario");

                var usuario = await _usuarioService.GetUsuarioByIDAsync(id);
                var actuales = usuario.Roles.Select(r => r.RolID).ToList();
                var rolesAEliminar = actuales.Except(RolesSeleccionados ?? new List<int>()).ToList();
                var rolesAAgregar = (RolesSeleccionados ?? new List<int>()).Except(actuales).ToList();

                foreach (var rolId in rolesAEliminar)
                    await _usuarioService.QuitarRolAsync(id, rolId);
                foreach (var rolId in rolesAAgregar)
                    await _usuarioService.AsignarRolAsync(id, rolId);

                TempData["Success"] = "Usuario actualizado correctamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar usuario: {Message}", ex.Message);
                ModelState.AddModelError(string.Empty, "Error al actualizar usuario: " + ex.Message);

                var roles = await _rolService.GetAllRolesAsync();
                var rolesItems = roles.Select(r => new SelectListItem
                {
                    Value = r.RolID.ToString(),
                    Text = r.Nombre
                }).ToList();

                model.RolesDisponibles = rolesItems;
                model.RolesSeleccionados = RolesSeleccionados ?? new List<int>();
                model.EsEdicion = true;
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

                var model = new UsuarioDetailsViewModel
                {
                    Usuario = usuario,
                    Roles = roles
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al preparar eliminación de usuario");
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

                TempData["Success"] = "Usuario eliminado correctamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar usuario: {Message}", ex.Message);
                TempData["Error"] = "Error al eliminar usuario: " + ex.Message;
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
                    return Json(new { success = false, message = "No se pudo actualizar el estado del usuario" });

                var usuario = await _usuarioService.GetUsuarioByIDAsync(id);
                return Json(new
                {
                    success = true,
                    message = $"Usuario {(usuario.Activo ? "activado" : "desactivado")} correctamente",
                    estado = usuario.Activo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar estado de usuario");
                return Json(new { success = false, message = "Error al cambiar estado del usuario: " + ex.Message });
            }
        }
    }
}
