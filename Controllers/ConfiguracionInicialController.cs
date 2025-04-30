// Controllers/ConfiguracionInicialController.cs
using Javo2.Controllers.Base;
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

namespace Javo2.Controllers
{
    [Authorize]
    public class ConfiguracionInicialController : BaseController
    {
        private readonly IUsuarioService _usuarioService;
        private readonly IRolService _rolService;
        private readonly IPermisoService _permisoService;

        public ConfiguracionInicialController(
            IUsuarioService usuarioService,
            IRolService rolService,
            IPermisoService permisoService,
            ILogger<ConfiguracionInicialController> logger)
            : base(logger)
        {
            _usuarioService = usuarioService;
            _rolService = rolService;
            _permisoService = permisoService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var usuarios = await _usuarioService.GetAllUsuariosAsync();
            if (usuarios.Any())
            {
                return RedirectToAction("Login", "Auth");
            }
            return View(new ConfiguracionInicialViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ConfiguracionInicialViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var usuarios = await _usuarioService.GetAllUsuariosAsync();
                if (usuarios.Any())
                {
                    return RedirectToAction("Login", "Auth");
                }

                var admin = new Usuario
                {
                    NombreUsuario = model.NombreUsuario,
                    Nombre = model.Nombre,
                    Apellido = model.Apellido,
                    Email = model.Email,
                    CreadoPor = "Sistema",
                    Activo = true
                };

                await _usuarioService.CreateUsuarioAsync(admin, model.Contraseña);

                var roles = await _rolService.GetAllRolesAsync();
                var rolAdmin = roles.FirstOrDefault(r =>
                    r.Nombre.Equals("Administrador", StringComparison.OrdinalIgnoreCase));

                if (rolAdmin != null)
                {
                    await _usuarioService.AsignarRolAsync(admin.UsuarioID, rolAdmin.RolID);
                }

                return RedirectToAction("ConfiguracionCompletada");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en la configuración inicial");
                ModelState.AddModelError(string.Empty,
                    "Ocurrió un error al crear el usuario administrador: " + ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ConfiguracionCompletada()
        {
            return View();
        }
    }    }