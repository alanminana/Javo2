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
    [Authorize(Policy = "Permission:usuarios.ver")]
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
        public async Task<IActionResult> Index(UsuarioFilterViewModel filtro = null)
        {
            try
            {
                // Preparar filtro
                if (filtro == null)
                {
                    filtro = new UsuarioFilterViewModel();
                }

                // Preparar roles para el filtro
                var roles = await _rolService.GetAllRolesAsync();
                ViewBag.Roles = roles.Select(r => new SelectListItem
                {
                    Value = r.RolID.ToString(),
                    Text = r.Nombre
                });

                // Filtrar usuarios
                IEnumerable<Usuario> usuarios;
                if (!string.IsNullOrEmpty(filtro.Termino))
                {
                    usuarios = await _usuarioService.BuscarUsuariosAsync(filtro.Termino);
                }
                else
                {
                    usuarios = await _usuarioService.GetAllUsuariosAsync();
                }

                // Aplicar filtro por estado
                if (filtro.Activo.HasValue)
                {
                    usuarios = usuarios.Where(u => u.Activo == filtro.Activo.Value);
                }

                // Aplicar filtro por rol
                if (filtro.RolID > 0)
                {
                    usuarios = usuarios.Where(u =>
                        u.Roles.Any(r => r.RolID == filtro.RolID));
                }

                return View(usuarios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la lista de usuarios");
                return View("Error");
            }
        }

        // GET: Usuarios/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var usuario = await _usuarioService.GetUsuarioByIDAsync(id);
                if (usuario == null)
                {
                    return NotFound();
                }

                // Obtener roles del usuario
                var roles = new List<Rol>();
                foreach (var usuarioRol in usuario.Roles)
                {
                    var rol = await _rolService.GetRolByIDAsync(usuarioRol.RolID);
                    if (rol != null)
                    {
                        roles.Add(rol);
                    }
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
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                _logger.LogInformation("Preparando formulario de creación de usuario");

                // Cargar roles desde el servicio
                var roles = await _rolService.GetAllRolesAsync();
                _logger.LogInformation("Roles cargados: {Count}", roles.Count());

                var rolesItems = roles.Select(r => new SelectListItem
                {
                    Value = r.RolID.ToString(),
                    Text = r.Nombre
                }).ToList();

                // Crear modelo con usuario nuevo y roles cargados
                var model = new UsuarioFormViewModel
                {
                    Usuario = new Usuario { Activo = true },
                    RolesDisponibles = rolesItems,
                    RolesSeleccionados = new List<int>(),
                    EsEdicion = false,
                    Contraseña = string.Empty, // Inicializar para que no sea null
                    ConfirmarContraseña = string.Empty // Inicializar para que no sea null
                };

                return View("Form", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al preparar formulario de creación de usuario");
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UsuarioFormViewModel model, List<int> RolesSeleccionados)
        {
            try
            {
                _logger.LogInformation("Iniciando creación de usuario: {NombreUsuario}", model.Usuario?.NombreUsuario);
                _logger.LogDebug("Roles seleccionados: {Roles}", string.Join(", ", RolesSeleccionados ?? new List<int>()));

                // Siempre cargar los roles para tenerlos disponibles en caso de error
                var roles = await _rolService.GetAllRolesAsync();
                var rolesItems = roles.Select(r => new SelectListItem
                {
                    Value = r.RolID.ToString(),
                    Text = r.Nombre
                }).ToList();

                model.RolesDisponibles = rolesItems;
                model.RolesSeleccionados = RolesSeleccionados ?? new List<int>();

                // Validar manualmente algunos campos
                bool modelValid = true;

                // Validar contraseña
                if (string.IsNullOrEmpty(model.Contraseña))
                {
                    ModelState.AddModelError(nameof(model.Contraseña), "La contraseña es obligatoria");
                    _logger.LogWarning("ERROR DE VALIDACIÓN: Propiedad: {Prop}, Error: {Error}",
                        nameof(model.Contraseña), "La contraseña es obligatoria");
                    modelValid = false;
                }

                // Si hay errores en el modelo
                if (!ModelState.IsValid || !modelValid)
                {
                    _logger.LogWarning("VALIDACIÓN FALLIDA: Errores en el ModelState");

                    // Loguear todos los errores para diagnóstico
                    foreach (var state in ModelState)
                    {
                        foreach (var error in state.Value.Errors)
                        {
                            _logger.LogWarning("ERROR DE VALIDACIÓN: Propiedad: {Prop}, Error: {Error}",
                                state.Key, error.ErrorMessage);
                        }
                    }

                    _logger.LogInformation("RETORNANDO: Vista Form con modelo invalidado");
                    return View("Form", model);
                }

                // Establecer creado por
                model.Usuario.CreadoPor = User.Identity?.Name ?? "Sistema";

                // Crear usuario
                _logger.LogInformation("Creando usuario en la base de datos");
                var result = await _usuarioService.CreateUsuarioAsync(model.Usuario, model.Contraseña);
                if (!result)
                {
                    throw new Exception("No se pudo crear el usuario");
                }

                _logger.LogInformation("Usuario creado con ID: {UsuarioID}", model.Usuario.UsuarioID);

                // Asignar roles
                if (RolesSeleccionados != null && RolesSeleccionados.Any())
                {
                    _logger.LogInformation("Asignando {Count} roles al usuario", RolesSeleccionados.Count);
                    foreach (var rolId in RolesSeleccionados)
                    {
                        _logger.LogInformation("Asignando rol ID: {RolID}", rolId);
                        await _usuarioService.AsignarRolAsync(model.Usuario.UsuarioID, rolId);
                    }
                }
                else
                {
                    _logger.LogWarning("No se seleccionaron roles para el usuario");
                }

                TempData["Success"] = "Usuario creado correctamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario: {Message}", ex.Message);
                ModelState.AddModelError(string.Empty, "Error al crear usuario: " + ex.Message);

                // Recargar roles
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
        [Authorize(Policy = "Permission:usuarios.editar")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var usuario = await _usuarioService.GetUsuarioByIDAsync(id);
                if (usuario == null)
                {
                    return NotFound();
                }

                // Obtener roles
                var roles = await _rolService.GetAllRolesAsync();
                var rolesItems = roles.Select(r => new SelectListItem
                {
                    Value = r.RolID.ToString(),
                    Text = r.Nombre
                }).ToList();

                // Obtener roles seleccionados
                var rolesSeleccionados = usuario.Roles.Select(r => r.RolID).ToList();

                var model = new UsuarioFormViewModel
                {
                    Usuario = usuario,
                    RolesDisponibles = rolesItems,
                    RolesSeleccionados = rolesSeleccionados,
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
            {
                return NotFound();
            }

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

                // Actualizar usuario
                var result = await _usuarioService.UpdateUsuarioAsync(model.Usuario, model.Contraseña);
                if (!result)
                {
                    throw new Exception("No se pudo actualizar el usuario");
                }

                // Actualizar roles
                // 1. Obtener roles actuales
                var usuario = await _usuarioService.GetUsuarioByIDAsync(id);
                var rolesActuales = usuario.Roles.Select(r => r.RolID).ToList();

                // 2. Calcular roles a eliminar y a agregar
                var rolesSeleccionados = RolesSeleccionados ?? new List<int>();
                var rolesEliminar = rolesActuales.Except(rolesSeleccionados).ToList();
                var rolesAgregar = rolesSeleccionados.Except(rolesActuales).ToList();

                // 3. Eliminar roles
                foreach (var rolId in rolesEliminar)
                {
                    await _usuarioService.QuitarRolAsync(id, rolId);
                }

                // 4. Agregar roles
                foreach (var rolId in rolesAgregar)
                {
                    await _usuarioService.AsignarRolAsync(id, rolId);
                }

                TempData["Success"] = "Usuario actualizado correctamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar usuario");
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
        [Authorize(Policy = "Permission:usuarios.eliminar")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var usuario = await _usuarioService.GetUsuarioByIDAsync(id);
                if (usuario == null)
                {
                    return NotFound();
                }

                // Obtener roles del usuario
                var roles = new List<Rol>();
                foreach (var usuarioRol in usuario.Roles)
                {
                    var rol = await _rolService.GetRolByIDAsync(usuarioRol.RolID);
                    if (rol != null)
                    {
                        roles.Add(rol);
                    }
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
                {
                    throw new Exception("No se pudo eliminar el usuario");
                }

                TempData["Success"] = "Usuario eliminado correctamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar usuario");
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
                {
                    return Json(new { success = false, message = "No se pudo actualizar el estado del usuario" });
                }

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