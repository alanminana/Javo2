using Javo2.Controllers.Base;
using Javo2.IServices;
using Javo2.Models;
using Javo2.ViewModels.Configuracion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Javo2.Controllers
{
    [Authorize(Policy = "Permission:configuracion.ver")]
    public class ConfiguracionController : BaseController
    {
        private readonly IConfiguracionService _configuracionService;

        public ConfiguracionController(
            IConfiguracionService configuracionService,
            ILogger<ConfiguracionController> logger) : base(logger)
        {
            _configuracionService = configuracionService;
        }

        public async Task<IActionResult> Index(string modulo = null)
        {
            try
            {
                // Obtener todos los módulos primero
                var todasConfiguraciones = await _configuracionService.GetAllAsync();
                var modulos = todasConfiguraciones
                    .Select(c => c.Modulo)
                    .Distinct()
                    .OrderBy(m => m)
                    .ToList();

                // Si no se especificó un módulo pero hay módulos disponibles, seleccionar el primero
                if (string.IsNullOrEmpty(modulo) && modulos.Any())
                {
                    modulo = modulos.First();
                }

                // Obtener configuraciones para el módulo seleccionado
                var configuraciones = string.IsNullOrEmpty(modulo) ?
                    todasConfiguraciones.ToList() :
                    todasConfiguraciones.Where(c => c.Modulo == modulo).ToList();

                var viewModel = new ConfiguracionIndexViewModel
                {
                    Configuraciones = configuraciones,
                    ModuloSeleccionado = modulo,
                    Modulos = modulos
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar configuraciones");
                return View("Error");
            }
        }

        [HttpGet]
        [Authorize(Policy = "Permission:configuracion.editar")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var configuraciones = await _configuracionService.GetAllAsync();
                var configuracion = configuraciones.FirstOrDefault(c => c.ConfiguracionID == id);

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:configuracion.editar")]
        public async Task<IActionResult> Edit(ConfiguracionSistema configuracion)
        {
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
    }
}