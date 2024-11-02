// Archivo: Controllers/VentasController.cs
using AutoMapper;
using Javo2.Controllers.Base;
using Javo2.IServices;
using Javo2.Models;
using Javo2.ViewModels.Operaciones.Ventas;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Javo2.Controllers
{
    public class VentasController : BaseController
    {
        private readonly IVentaService _ventaService;
        private readonly IClienteService _clienteService;
        private readonly IProductoService _productoService;
        private readonly IMapper _mapper;

        public VentasController(
            IVentaService ventaService,
            IClienteService clienteService,
            IProductoService productoService,
            IMapper mapper,
            ILogger<VentasController> logger)
            : base(logger)
        {
            _ventaService = ventaService;
            _clienteService = clienteService;
            _productoService = productoService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index(DateTime? fechaInicio, DateTime? fechaFin)
        {
            _logger.LogInformation("Index action called with fechaInicio: {FechaInicio}, fechaFin: {FechaFin}", fechaInicio, fechaFin);
            var ventas = await _ventaService.GetVentasByFechaAsync(fechaInicio, fechaFin);
            var ventasViewModel = _mapper.Map<IEnumerable<VentasViewModel>>(ventas);
            _logger.LogInformation("Ventas retrieved: {VentasCount}", ventasViewModel.Count());
            return View(ventasViewModel);
        }

        public async Task<IActionResult> Create()
        {
            _logger.LogInformation("Create GET action called");
            var model = new VentasViewModel
            {
                FechaVenta = DateTime.Now,
                NumeroFactura = GenerateNumeroFactura(),
                Usuario = "cosmefulanito",
                Vendedor = "cosmefulanito"
            };
            await PopulateDropdownsAsync(model);
            return View(model);
        }

        private string GenerateNumeroFactura()
        {
            // Genera un número de factura único
            var numeroFactura = $"F{DateTime.Now:yyyyMMddHHmmss}";
            return numeroFactura;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VentasViewModel model)
        {
            _logger.LogInformation("Create POST action called with Venta: {Venta}", model);
            if (ModelState.IsValid)
            {
                var venta = _mapper.Map<Venta>(model);
                await _ventaService.CreateVentaAsync(venta);
                _logger.LogInformation("Venta created successfully");
                return RedirectToAction(nameof(Index));
            }

            _logger.LogWarning("Model state is invalid. Errors: {Errors}", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            await PopulateDropdownsAsync(model);
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            _logger.LogInformation("Edit GET action called with ID: {Id}", id);
            var venta = await _ventaService.GetVentaByIdAsync(id);
            if (venta == null)
            {
                _logger.LogWarning("Venta with ID {Id} not found", id);
                return NotFound();
            }
            var model = _mapper.Map<VentasViewModel>(venta);
            await PopulateDropdownsAsync(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(VentasViewModel model)
        {
            _logger.LogInformation("Edit POST action called with Venta: {Venta}", model);
            if (ModelState.IsValid)
            {
                var venta = _mapper.Map<Venta>(model);
                await _ventaService.UpdateVentaAsync(venta);
                _logger.LogInformation("Venta updated successfully");
                return RedirectToAction(nameof(Index));
            }

            _logger.LogWarning("Model state is invalid. Errors: {Errors}", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            await PopulateDropdownsAsync(model);
            return View(model);
        }

        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Delete GET action called with ID: {Id}", id);
            var venta = await _ventaService.GetVentaByIdAsync(id);
            if (venta == null)
            {
                _logger.LogWarning("Venta with ID {Id} not found", id);
                return NotFound();
            }
            var model = _mapper.Map<VentasViewModel>(venta);
            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            _logger.LogInformation("Delete POST action called with ID: {Id}", id);
            await _ventaService.DeleteVentaAsync(id);
            _logger.LogInformation("Venta deleted successfully");
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            _logger.LogInformation("Details action called with ID: {Id}", id);
            var venta = await _ventaService.GetVentaByIdAsync(id);
            if (venta == null)
            {
                _logger.LogWarning("Venta with ID {Id} not found", id);
                return NotFound();
            }
            var model = _mapper.Map<VentasViewModel>(venta);
            return View(model);
        }

        // Métodos para buscar clientes y productos permanecen igual, utilizando los servicios correspondientes

        // ... código restante ...

        private async Task PopulateDropdownsAsync(VentasViewModel model)
        {
            // Aquí puedes cargar listas desde la base de datos o definirlas estáticamente
            model.FormasPago = new List<SelectListItem>
            {
                new() { Text = "Tarjeta de crédito", Value = "Tarjeta de crédito" },
                new() { Text = "Efectivo", Value = "Efectivo" },
                new() { Text = "Transferencia", Value = "Transferencia" },
                new() { Text = "Pago Virtual", Value = "Pago Virtual" },
                new() { Text = "Débito", Value = "Débito" },
                new() { Text = "Crédito Personal", Value = "Crédito Personal" }
            };

            model.Bancos = new List<SelectListItem>
            {
                new() { Text = "HSBC", Value = "HSBC" },
                new() { Text = "BBVA", Value = "BBVA" },
                new() { Text = "Santander", Value = "Santander" }
            };

            model.TipoTarjeta = new List<SelectListItem>
            {
                new() { Text = "AMEX", Value = "AMEX" },
                new() { Text = "VISA", Value = "VISA" },
                new() { Text = "MASTERCARD", Value = "MASTERCARD" }
            };

            model.Cuotas = new List<SelectListItem>
            {
                new() { Text = "1 cuota", Value = "1" },
                new() { Text = "3 cuotas", Value = "3" },
                new() { Text = "6 cuotas", Value = "6" },
                new() { Text = "12 cuotas", Value = "12" }
            };

            model.EntidadesElectronicas = new List<SelectListItem>
            {
                new() { Text = "Personal Pay", Value = "Personal Pay" },
                new() { Text = "Mercado Pago", Value = "Mercado Pago" }
            };

            model.PlanesFinanciamiento = new List<SelectListItem>
            {
                new() { Text = "Ahora 12", Value = "Ahora 12" },
                new() { Text = "Plan 18", Value = "Plan 18" }
            };

            model.TipoEntregas = new List<SelectListItem>
            {
                new() { Text = "Envío a domicilio", Value = "Envío a domicilio" },
                new() { Text = "Retiro en tienda", Value = "Retiro en tienda" }
            };

            await Task.CompletedTask;
        }
    }
}
