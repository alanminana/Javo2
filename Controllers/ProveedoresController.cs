using javo2.Services;
using javo2.ViewModels.Operaciones.Proveedores;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace javo2.Controllers
{
    public class ProveedoresController : Controller
    {
        private readonly IProveedorService _proveedorService;
        private readonly ILogger<ProveedoresController> _logger;

        public ProveedoresController(IProveedorService proveedorService, ILogger<ProveedoresController> logger)
        {
            _proveedorService = proveedorService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Index action called");
            var proveedores = await _proveedorService.GetProveedoresAsync();
            _logger.LogInformation("Proveedores retrieved: {ProveedoresCount}", proveedores.Count());
            return View(proveedores);
        }

        public async Task<IActionResult> Create()
        {
            _logger.LogInformation("Create GET action called");
            var viewModel = await CreateViewModelWithProductosDisponiblesAsync();
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProveedoresViewModel proveedorViewModel)
        {
            _logger.LogInformation("Create POST action called with Proveedor: {Proveedor}", proveedorViewModel);
            if (ModelState.IsValid)
            {
                await _proveedorService.CreateProveedorAsync(proveedorViewModel);
                _logger.LogInformation("Proveedor created successfully");
                return RedirectToAction(nameof(Index));
            }
            _logger.LogWarning("Model state is invalid");
            proveedorViewModel.ProductosDisponibles = (await _proveedorService.GetProductosDisponiblesAsync()).ToList();
            return View(proveedorViewModel);
        }

        public async Task<IActionResult> Edit(int id)
        {
            _logger.LogInformation("Edit GET action called with ID: {Id}", id);
            var proveedor = await _proveedorService.GetProveedorByIdAsync(id);
            if (proveedor == null)
            {
                _logger.LogWarning("Proveedor with ID {Id} not found", id);
                return NotFound();
            }
            proveedor.ProductosDisponibles = (await _proveedorService.GetProductosDisponiblesAsync()).ToList();
            return View(proveedor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProveedoresViewModel proveedorViewModel)
        {
            _logger.LogInformation("Edit POST action called with Proveedor: {Proveedor}", proveedorViewModel);
            if (id != proveedorViewModel.ProveedorID)
            {
                _logger.LogWarning("Proveedor ID mismatch");
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await _proveedorService.UpdateProveedorAsync(proveedorViewModel);
                _logger.LogInformation("Proveedor updated successfully");
                return RedirectToAction(nameof(Index));
            }
            _logger.LogWarning("Model state is invalid");
            proveedorViewModel.ProductosDisponibles = (await _proveedorService.GetProductosDisponiblesAsync()).ToList();
            return View(proveedorViewModel);
        }

        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Delete GET action called with ID: {Id}", id);
            var proveedor = await _proveedorService.GetProveedorByIdAsync(id);
            if (proveedor == null)
            {
                _logger.LogWarning("Proveedor with ID {Id} not found", id);
                return NotFound();
            }
            return View(proveedor);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            _logger.LogInformation("Delete POST action called with ID: {Id}", id);
            await _proveedorService.DeleteProveedorAsync(id);
            _logger.LogInformation("Proveedor deleted successfully");
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            _logger.LogInformation("Details action called with ID: {Id}", id);
            var proveedor = await _proveedorService.GetProveedorByIdAsync(id);
            if (proveedor == null)
            {
                _logger.LogWarning("Proveedor with ID {Id} not found", id);
                return NotFound();
            }
            return View(proveedor);
        }

        private async Task<ProveedoresViewModel> CreateViewModelWithProductosDisponiblesAsync()
        {
            _logger.LogInformation("Creating view model with productos disponibles");
            var viewModel = new ProveedoresViewModel
            {
                ProductosDisponibles = (await _proveedorService.GetProductosDisponiblesAsync()).ToList()
            };
            _logger.LogInformation("View model created with {ProductosCount} productos disponibles", viewModel.ProductosDisponibles.Count);
            return viewModel;
        }
    }
}
