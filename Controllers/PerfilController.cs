// Controllers/PerfilController.cs
using Javo2.Controllers.Base;
using Javo2.IServices.Authentication;
using Javo2.ViewModels.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Javo2.Controllers
{
    [Authorize]
    public class PerfilController : BaseController
    {
        private readonly IUsuarioService _usuarioService;
        private readonly IRolService _rolService;

        public PerfilController(
            IUsuarioService usuarioService,
            IRolService rolService,
            ILogger<PerfilController> logger)
            : base(logger)
        {
            _usuarioService = usuarioService;
            _rolService = rolService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    _logger.LogWarning("No se pudo obtener el ID del usuario desde los claims");
                    return RedirectToAction("Login", "Auth");
                }

                var usuario = await _usuarioService.GetUsuarioByIDAsync(userId);
                if (usuario == null)
                {
                    _logger.LogWarning("Usuario con ID {UserId} no encontrado", userId);
                    return RedirectToAction("Login", "Auth");
                }

                var model = new PerfilViewModel
                {
                    UsuarioID = usuario.UsuarioID,
                    NombreUsuario = usuario.NombreUsuario,
                    Nombre = usuario.Nombre,
                    Apellido = usuario.Apellido,
                    Email = usuario.Email,
                    FechaCreacion = usuario.FechaCreacion,
                    UltimoAcceso = usuario.UltimoAcceso
                };

                // Obtener roles del usuario
                foreach (var usuarioRol in usuario.Roles)
                {
                    var rol = await _rolService.GetRolByIDAsync(usuarioRol.RolID);
                    if (rol != null)
                    {
                        model.Roles.Add(new RolBasicoViewModel
                        {
                            RolID = rol.RolID,
                            Nombre = rol.Nombre,
                            Descripcion = rol.Descripcion
                        });
                    }
                }

                // Obtener permisos del usuario
                var permisos = await _usuarioService.GetPermisosUsuarioAsync(userId);
                foreach (var permiso in permisos)
                {
                    model.Permisos.Add(new PermisoBasicoViewModel
                    {
                        PermisoID = permiso.PermisoID,
                        Nombre = permiso.Nombre,
                        Codigo = permiso.Codigo,
                        Grupo = permiso.Grupo
                    });
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar el perfil del usuario");
                return View("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return RedirectToAction("Login", "Auth");
                }

                var usuario = await _usuarioService.GetUsuarioByIDAsync(userId);
                if (usuario == null)
                {
                    return RedirectToAction("Login", "Auth");
                }

                var model = new EditarPerfilViewModel
                {
                    UsuarioID = usuario.UsuarioID,
                    NombreUsuario = usuario.NombreUsuario,
                    Nombre = usuario.Nombre,
                    Apellido = usuario.Apellido,
                    Email = usuario.Email
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar formulario de edición de perfil");
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditarPerfilViewModel model)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId) || userId != model.UsuarioID)
                {
                    ModelState.AddModelError(string.Empty, "Error de autenticación");
                    return View(model);
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var usuario = await _usuarioService.GetUsuarioByIDAsync(userId);
                if (usuario == null)
                {
                    ModelState.AddModelError(string.Empty, "Usuario no encontrado");
                    return View(model);
                }

                // Actualizar solo los campos permitidos
                usuario.Nombre = model.Nombre;
                usuario.Apellido = model.Apellido;
                usuario.Email = model.Email;

                // Actualizar la contraseña solo si se proporciona
                string nuevaContraseña = null;
                if (!string.IsNullOrEmpty(model.ContraseñaNueva))
                {
                    if (string.IsNullOrEmpty(model.ContraseñaActual))
                    {
                        ModelState.AddModelError(nameof(model.ContraseñaActual), "Debe proporcionar la contraseña actual");
                        return View(model);
                    }

                    bool verificada = await _usuarioService.AutenticarAsync(usuario.NombreUsuario, model.ContraseñaActual);
                    if (!verificada)
                    {
                        ModelState.AddModelError(nameof(model.ContraseñaActual), "La contraseña actual es incorrecta");
                        return View(model);
                    }

                    nuevaContraseña = model.ContraseñaNueva;
                }

                await _usuarioService.UpdateUsuarioAsync(usuario, nuevaContraseña);

                TempData["Success"] = "Perfil actualizado correctamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar perfil del usuario");
                ModelState.AddModelError(string.Empty, "Error al actualizar el perfil: " + ex.Message);
                return View(model);
            }
        }
    }
}