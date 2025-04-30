// Controllers/ConfiguracionController.cs
using Javo2.Controllers.Base;
using Javo2.IServices;
using Javo2.IServices.Authentication;
using Javo2.Models;
using Javo2.ViewModels.Configuracion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Javo2.Controllers
{
    [Authorize]  // Fuerza que el usuario esté autenticado
    public class ConfiguracionController : BaseController
    {
        private readonly IConfiguracionService _configuracionService;
        private readonly IUsuarioService _usuarioService;
        private readonly IRolService _rolService;
        private readonly IPermisoService _permisoService;

        public ConfiguracionController(
            IConfiguracionService configuracionService,
            IUsuarioService usuarioService,
            IRolService rolService,
            IPermisoService permisoService,
            ILogger<ConfiguracionController> logger) : base(logger)
        {
            _configuracionService = configuracionService;
            _usuarioService = usuarioService;
            _rolService = rolService;
            _permisoService = permisoService;
        }

        // GET: Configuracion
        [HttpGet]
        [Authorize(Policy = "Permission:configuracion.ver")]
        public async Task<IActionResult> Index(string modulo = null)
        {
            try
            {
                var configuraciones = string.IsNullOrEmpty(modulo) ?
                    await _configuracionService.GetAllAsync() :
                    await _configuracionService.GetByModuloAsync(modulo);

                var viewModel = new ConfiguracionIndexViewModel
                {
                    Configuraciones = configuraciones.ToList(),
                    ModuloSeleccionado = modulo
                };

                var modulos = (await _configuracionService.GetAllAsync())
                    .Select(c => c.Modulo)
                    .Distinct()
                    .ToList();

                viewModel.Modulos = modulos;

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar configuraciones");
                return View("Error");
            }
        }

        // GET: Configuracion/Edit/5
        [HttpGet]
        [Authorize(Policy = "Permission:configuracion.editar")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var configuracion = (await _configuracionService.GetAllAsync())
                    .FirstOrDefault(c => c.ConfiguracionID == id);

                if (configuracion == null)
                    return NotFound();

                return View(configuracion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar configuración para editar");
                return View("Error");
            }
        }

        // POST: Configuracion/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:configuracion.editar")]
        public async Task<IActionResult> Edit(int id, ConfiguracionSistema configuracion)
        {
            if (id != configuracion.ConfiguracionID)
                return NotFound();

            if (!ModelState.IsValid)
                return View(configuracion);

            try
            {
                await _configuracionService.SaveAsync(configuracion);
                TempData["Success"] = "Configuración actualizada correctamente.";
                return RedirectToAction(nameof(Index), new { modulo = configuracion.Modulo });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar configuración");
                ModelState.AddModelError("", "Ocurrió un error al guardar la configuración.");
                return View(configuracion);
            }
        }

        // GET: Configuracion/Seguridad
        [HttpGet]
        [Authorize(Policy = "Permission:configuracion.seguridad")]
        public async Task<IActionResult> Seguridad()
        {
            try
            {
                var usuarios = await _usuarioService.GetAllUsuariosAsync();
                ViewBag.UsuariosActivos = usuarios.Count(u => u.Activo);

                var roles = await _rolService.GetAllRolesAsync();
                ViewBag.RolesCount = roles.Count();

                var permisos = await _permisoService.GetAllPermisosAsync();
                ViewBag.PermisosCount = permisos.Count();

                if (User.Identity.IsAuthenticated)
                {
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                    if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                    {
                        var usuario = await _usuarioService.GetUsuarioByIDAsync(userId);
                        ViewBag.UltimoAcceso = usuario?.UltimoAcceso;
                    }
                }

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar la página de seguridad");
                return View("Error");
            }
        }
    }
}
