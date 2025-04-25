// Controllers/ConfiguracionController.cs
using Javo2.Controllers.Base;
using Javo2.IServices;
using Javo2.Models;
using Javo2.ViewModels.Configuracion;
using Microsoft.AspNetCore.Mvc;

namespace Javo2.Controllers
{
    public class ConfiguracionController : BaseController
    {
        private readonly IConfiguracionService _configuracionService;

        public ConfiguracionController(
            IConfiguracionService configuracionService,
            ILogger<ConfiguracionController> logger) : base(logger)
        {
            _configuracionService = configuracionService;
        }

        // GET: Configuracion
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

                // Obtener lista de módulos disponibles para el filtro
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
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var configuracion = (await _configuracionService.GetAllAsync())
                    .FirstOrDefault(c => c.ConfiguracionID == id);

                if (configuracion == null)
                {
                    return NotFound();
                }

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
        public async Task<IActionResult> Edit(int id, ConfiguracionSistema configuracion)
        {
            if (id != configuracion.ConfiguracionID)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(configuracion);
            }

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