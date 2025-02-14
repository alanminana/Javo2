// File: Controllers/CotizacionesController.cs
using AutoMapper;
using Javo2.IServices;
using Javo2.Models;
using Javo2.ViewModels.Operaciones.Ventas;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace Javo2.Controllers
{
    public class CotizacionesController : Controller
    {
        private readonly ICotizacionService _cotizacionService;
        private readonly IMapper _mapper;
        private readonly ILogger<CotizacionesController> _logger;

        public CotizacionesController(ICotizacionService cotizacionService, IMapper mapper, ILogger<CotizacionesController> logger)
        {
            _cotizacionService = cotizacionService;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: Cotizaciones/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var cotizaciones = await _cotizacionService.GetAllCotizacionesAsync();
            var model = cotizaciones.Select(c => _mapper.Map<VentaListViewModel>(c));
            return View(model);
        }

        // GET: Cotizaciones/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var viewModel = new VentaFormViewModel
            {
                FechaVenta = System.DateTime.Now,
                NumeroFactura = await _cotizacionService.GenerarNumeroCotizacionAsync(),
                Usuario = User.Identity?.Name ?? "Desconocido",
                Vendedor = User.Identity?.Name ?? "Desconocido"
                // Se pueden cargar otros combos si se requieren
            };
            return View("Form", viewModel);
        }

        // POST: Cotizaciones/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VentaFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Form", model);
            }
            var cotizacion = _mapper.Map<Venta>(model);
            cotizacion.Estado = EstadoVenta.Borrador;
            await _cotizacionService.CreateCotizacionAsync(cotizacion);
            return RedirectToAction(nameof(Index));
        }

        // GET: Cotizaciones/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var cotizacion = await _cotizacionService.GetCotizacionByIDAsync(id);
            if (cotizacion == null)
                return NotFound();

            var viewModel = _mapper.Map<VentaFormViewModel>(cotizacion);
            return View("Form", viewModel);
        }

        // POST: Cotizaciones/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(VentaFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Form", model);
            }
            var cotizacion = _mapper.Map<Venta>(model);
            await _cotizacionService.UpdateCotizacionAsync(cotizacion);
            return RedirectToAction(nameof(Index));
        }

        // GET: Cotizaciones/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var cotizacion = await _cotizacionService.GetCotizacionByIDAsync(id);
            if (cotizacion == null)
                return NotFound();
            var viewModel = _mapper.Map<VentaListViewModel>(cotizacion);
            return View(viewModel);
        }

        // POST: Cotizaciones/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _cotizacionService.DeleteCotizacionAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
