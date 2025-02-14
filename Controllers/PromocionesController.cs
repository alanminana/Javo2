// File: Controllers/PromocionesController.cs
using AutoMapper;
using Javo2.Controllers.Base;
using Javo2.IServices;
using Javo2.Models;
using Javo2.ViewModels.Operaciones.Promociones;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Javo2.Controllers
{
    public class PromocionesController : BaseController
    {
        private readonly IPromocionesService _promocionesService;
        private readonly IMapper _mapper;

        public PromocionesController(IPromocionesService promocionesService, IMapper mapper, ILogger<PromocionesController> logger)
            : base(logger)
        {
            _promocionesService = promocionesService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var promos = await _promocionesService.GetPromocionesAsync();
            var model = promos.Select(p => _mapper.Map<PromocionViewModel>(p));
            return View(model);
        }

        public IActionResult Create()
        {
            return View("Form", new PromocionViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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

        public async Task<IActionResult> Edit(int id)
        {
            var promo = await _promocionesService.GetPromocionByIDAsync(id);
            if (promo == null) return NotFound();

            var model = _mapper.Map<PromocionViewModel>(promo);
            return View("Form", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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

        public async Task<IActionResult> Delete(int id)
        {
            var promo = await _promocionesService.GetPromocionByIDAsync(id);
            if (promo == null) return NotFound();

            var model = _mapper.Map<PromocionViewModel>(promo);
            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
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
