// File: Controllers/PromocionesController.cs
using AutoMapper;
using Javo2.Controllers.Base;
using Javo2.IServices;
using Javo2.Models;
using Javo2.ViewModels.Operaciones.Promociones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Javo2.Controllers
{
    [Authorize]  // Fuerza que el usuario esté autenticado
    public class PromocionesController : BaseController
    {
        private readonly IPromocionesService _promocionesService;
        private readonly IMapper _mapper;

        public PromocionesController(
            IPromocionesService promocionesService,
            IMapper mapper,
            ILogger<PromocionesController> logger)
            : base(logger)
        {
            _promocionesService = promocionesService;
            _mapper = mapper;
        }

        // GET: Promociones
        [HttpGet]
        [Authorize(Policy = "Permission:promociones.ver")]
        public async Task<IActionResult> Index()
        {
            var promos = await _promocionesService.GetPromocionesAsync();
            var model = promos.Select(p => _mapper.Map<PromocionViewModel>(p));
            return View(model);
        }

        // GET: Promociones/Create
        [HttpGet]
        [Authorize(Policy = "Permission:promociones.crear")]
        public IActionResult Create()
        {
            return View("Form", new PromocionViewModel());
        }

        // POST: Promociones/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:promociones.crear")]
        public async Task<IActionResult> Create(PromocionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                LogModelStateErrors();
                return View("Form", model);
            }

            var promo = _mapper.Map<Promocion>(model);
            try
            {
                await _promocionesService.CreatePromocionAsync(promo);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear la Promoción");
                ModelState.AddModelError(string.Empty, "Ocurrió un error al crear la promoción.");
                return View("Form", model);
            }
        }

        // GET: Promociones/Edit/5
        [HttpGet]
        [Authorize(Policy = "Permission:promociones.editar")]
        public async Task<IActionResult> Edit(int id)
        {
            var promo = await _promocionesService.GetPromocionByIDAsync(id);
            if (promo == null) return NotFound();

            var model = _mapper.Map<PromocionViewModel>(promo);
            return View("Form", model);
        }

        // POST: Promociones/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:promociones.editar")]
        public async Task<IActionResult> Edit(PromocionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                LogModelStateErrors();
                return View("Form", model);
            }

            var promo = _mapper.Map<Promocion>(model);
            try
            {
                await _promocionesService.UpdatePromocionAsync(promo);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar la Promoción");
                ModelState.AddModelError(string.Empty, "Ocurrió un error al actualizar la promoción.");
                return View("Form", model);
            }
        }

        // GET: Promociones/Delete/5
        [HttpGet]
        [Authorize(Policy = "Permission:promociones.eliminar")]
        public async Task<IActionResult> Delete(int id)
        {
            var promo = await _promocionesService.GetPromocionByIDAsync(id);
            if (promo == null) return NotFound();

            var model = _mapper.Map<PromocionViewModel>(promo);
            return View(model);
        }

        // POST: Promociones/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:promociones.eliminar")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _promocionesService.DeletePromocionAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar la Promoción");
                ModelState.AddModelError(string.Empty, "Ocurrió un error al eliminar la promoción.");
                var promo = await _promocionesService.GetPromocionByIDAsync(id);
                var model = _mapper.Map<PromocionViewModel>(promo);
                return View(model);
            }
        }
    }
}
