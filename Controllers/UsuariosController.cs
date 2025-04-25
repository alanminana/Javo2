// Controllers/Authentication/UsuariosController.cs
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
        [Authorize(Policy = "Permission:usuarios.ver")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var usuarios = await _usuarioService.GetAllUsuariosAsync();
                return View(usuarios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la lista de usuarios");
                return View("Error");
            }
        }

        // GET: Usuarios/Details/5
        [Authorize(Policy = "Permission:usuarios.ver")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var usuario = await _usuarioService.GetUsuarioByIDAsync(id);
                if (usuario == null)
                {
                    return NotFound();
                }

                // Obtener roles asignados al usuario
                var rolesUsuario = new List<Rol>();
                foreach (var usuarioRol in usuario.Roles)
                {
                    var rol = await _rolService.GetRolByIDAsync(usuarioRol.RolID);
                    if (rol != null)
                    {
                        rolesUsuario.Add(rol);
                    }
                }

                var viewModel = new UsuarioDetailsViewModel
                {
                    Usuario = usuario,
                    Roles = rolesUsuario
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener detalles del usuario");
                return View("Error");
            }
        }

        // GET: Usuarios/Create
        [Authorize(Policy = "Permission:usuarios.crear")]
        public async Task<IActionResult> Create()
        {
            try
            {
                var viewModel = new UsuarioFormViewModel
                {
                    Usuario = new Usuario { Activo = true },
                    RolesDisponibles = await GetRolesSelectList(),
                    RolesSeleccionados = new List<int>()
                };

                return View("Form", viewModel);
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
        public async Task<IActionResult> Create(UsuarioFormViewModel viewModel)
        {
            try
            {
                // Validar modelo
                if (!ModelState.IsValid)
                {
                    viewModel.RolesDisponibles = await GetRolesSelectList();
                    return View("Form", viewModel);
                }

                // Crear usuario
                var usuario = viewModel.Usuario;

                var result = await _usuarioService.CreateUsuarioAsync(usuario, viewModel.Contraseña);
                if (!result)
                {
                    ModelState.AddModelError(string.Empty, "No se pudo crear el usuario");
                    viewModel.RolesDisponibles = await GetRolesSelectList();
                    return View("Form", viewModel);
                }

                // Asignar roles
                if (viewModel.RolesSeleccionados != null)
                {
                    foreach (var rolID in viewModel.RolesSeleccionados)
                    {
                        await _usuarioService.AsignarRolAsync(usuario.UsuarioID, rolID);
                    }
                }

                TempData["Success"] = "Usuario creado correctamente";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                viewModel.RolesDisponibles = await GetRolesSelectList();
                return View("Form", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario");
                ModelState.AddModelError(string.Empty, "Error al crear usuario");
                viewModel.RolesDisponibles = await GetRolesSelectList();
                return View("Form", viewModel);
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

                var rolesSeleccionados = usuario.Roles.Select(r => r.RolID).ToList();

                var viewModel = new UsuarioFormViewModel
                {
                    Usuario = usuario,
                    RolesDisponibles = await GetRolesSelectList(),
                    RolesSeleccionados = rolesSeleccionados,
                    EsEdicion = true
                };

                return View("Form", viewModel);
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
        public async Task<IActionResult> Edit(int id, UsuarioFormViewModel viewModel)
        {
            if (id != viewModel.Usuario.UsuarioID)
            {
                return NotFound();
            }

            try
            {
                // En edición no es obligatorio cambiar la contraseña
                ModelState.Remove("Contraseña");
                ModelState.Remove("ConfirmarContraseña");

                if (!ModelState.IsValid)
                {
                    viewModel.RolesDisponibles = await GetRolesSelectList();
                    viewModel.EsEdicion = true;
                    return View("Form", viewModel);
                }

                // Actualizar usuario
                var usuario = viewModel.Usuario;
                var result = await _usuarioService.UpdateUsuarioAsync(usuario, viewModel.Contraseña);
                if (!result)
                {
                    ModelState.AddModelError(string.Empty, "No se pudo actualizar el usuario");
                    viewModel.RolesDisponibles = await GetRolesSelectList();
                    viewModel.EsEdicion = true;
                    return View("Form", viewModel);
                }

                // Obtener roles actuales del usuario
                var usuarioActual = await _usuarioService.GetUsuarioByIDAsync(id);
                var rolesActuales = usuarioActual.Roles.Select(r => r.RolID).ToList();

                // Roles que se deben quitar (están en rolesActuales pero no en rolesSeleccionados)
                var rolesQuitar = rolesActuales.Except(viewModel.RolesSeleccionados ?? new List<int>()).ToList();
                foreach (var rolID in rolesQuitar)
                {
                    await _usuarioService.QuitarRolAsync(id, rolID);
                }

                // Roles que se deben agregar (están en rolesSeleccionados pero no en rolesActuales)
                var rolesAgregar = (viewModel.RolesSeleccionados ?? new List<int>()).Except(rolesActuales).ToList();
                foreach (var rolID in rolesAgregar)
                {
                    await _usuarioService.AsignarRolAsync(id, rolID);
                }

                TempData["Success"] = "Usuario actualizado correctamente";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                viewModel.RolesDisponibles = await GetRolesSelectList();
                viewModel.EsEdicion = true;
                return View("Form", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar usuario");
                ModelState.AddModelError(string.Empty, "Error al actualizar usuario");
                viewModel.RolesDisponibles = await GetRolesSelectList();
                viewModel.EsEdicion = true;
                return View("Form", viewModel);
            }
        }

        // GET: Usuarios/Delete