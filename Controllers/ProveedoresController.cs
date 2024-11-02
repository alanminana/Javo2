// Archivo: Controllers/ProveedoresController.cs
using AutoMapper;
using Javo2.Controllers.Base;
using Javo2.IServices;
using Javo2.IServices.Common;
using Javo2.Models;
using Javo2.ViewModels.Operaciones.Proveedores;
using Microsoft.AspNetCore.Mvc;

namespace Javo2.Controllers
{
    public class ProveedoresController : BaseController
    {
        private readonly IProveedorService _proveedorService;
        private readonly IDropdownService _dropdownService;
        private readonly IProductoService _productoService;
        private readonly IMapper _mapper;

        public ProveedoresController(
            IProveedorService proveedorService,
            IDropdownService dropdownService,
            IProductoService productoService,
            IMapper mapper,
            ILogger<ProveedoresController> logger)
            : base(logger)
        {
            _proveedorService = proveedorService;
            _dropdownService = dropdownService;
            _productoService = productoService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Index action called");
            var proveedores = await _proveedorService.GetProveedoresAsync();
            var proveedoresViewModel = _mapper.Map<IEnumerable<ProveedoresViewModel>>(proveedores);
            _logger.LogInformation("Proveedores retrieved: {ProveedoresCount}", proveedores.Count());
            return View(proveedoresViewModel);
        }

        public async Task<IActionResult> Create()
        {
            _logger.LogInformation("Create GET action called");
            var viewModel = await InitializeProveedorViewModelAsync();
            _logger.LogInformation("Initial model data: {@Model}", viewModel);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProveedoresViewModel proveedorViewModel)
        {
            _logger.LogInformation("Create POST action called with Proveedor: {Proveedor}", proveedorViewModel.Nombre);

            await PopulateDropDownListsAsync(proveedorViewModel);

            if (!ModelState.IsValid)
            {
                LogModelStateErrors();
                return View(proveedorViewModel);
            }

            try
            {
                var proveedor = _mapper.Map<Proveedor>(proveedorViewModel);
                await _proveedorService.CreateProveedorAsync(proveedor);
                _logger.LogInformation("Proveedor created successfully");
                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                _logger.LogError("Error creating Proveedor: {Error}", ex.Message);
                ModelState.AddModelError(string.Empty, "Ocurrió un error al crear el proveedor.");
                return View(proveedorViewModel);
            }
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

            var proveedorViewModel = _mapper.Map<ProveedoresViewModel>(proveedor);

            // Inicializar listas desplegables
            await PopulateDropDownListsAsync(proveedorViewModel);

            return View(proveedorViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProveedoresViewModel proveedorViewModel)
        {
            _logger.LogInformation("Edit POST action called with Proveedor: {Proveedor}", proveedorViewModel.Nombre);
            _logger.LogInformation("Model data received: {@Model}", proveedorViewModel);

            // Inicializar listas desplegables
            await PopulateDropDownListsAsync(proveedorViewModel);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state is invalid. Errors: {Errors}", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                LogModelStateErrors();
                return View(proveedorViewModel);
            }

            try
            {
                var proveedor = _mapper.Map<Proveedor>(proveedorViewModel);
                await _proveedorService.UpdateProveedorAsync(proveedor);
                _logger.LogInformation("Proveedor updated successfully");
                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                _logger.LogError("Error updating Proveedor: {Error}", ex.Message);
                ModelState.AddModelError(string.Empty, "Ocurrió un error al actualizar el proveedor.");
                return View(proveedorViewModel);
            }
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

            var proveedorViewModel = _mapper.Map<ProveedoresViewModel>(proveedor);

            // Obtener nombres de productos asignados
            proveedorViewModel.ProductosAsignadosNombres = new List<string>();
            foreach (var productoId in proveedor.ProductosAsignados)
            {
                var producto = await _productoService.GetProductoByIdAsync(productoId);
                if (producto != null)
                {
                    proveedorViewModel.ProductosAsignadosNombres.Add(producto.Nombre);
                }
            }

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

            var proveedorViewModel = _mapper.Map<ProveedoresViewModel>(proveedor);

            return View(proveedorViewModel);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            _logger.LogInformation("Delete POST action called with ID: {Id}", id);
            try
            {
                await _proveedorService.DeleteProveedorAsync(id);
                _logger.LogInformation("Proveedor deleted successfully");
                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                _logger.LogError("Error deleting Proveedor: {Error}", ex.Message);
                ModelState.AddModelError(string.Empty, "Ocurrió un error al eliminar el proveedor.");
                var proveedor = await _proveedorService.GetProveedorByIdAsync(id);
                var proveedorViewModel = _mapper.Map<ProveedoresViewModel>(proveedor);
                return View(proveedorViewModel);
            }
        }

        private async Task PopulateDropDownListsAsync(ProveedoresViewModel model)
        {
            model.ProductosDisponibles = await _dropdownService.GetProductosAsync();
            _logger.LogInformation("PopulateDropDownListsAsync called. ProductosDisponibles: {ProductosCount}", model.ProductosDisponibles.Count());
        }

        private async Task<ProveedoresViewModel> InitializeProveedorViewModelAsync()
        {
            var model = new ProveedoresViewModel
            {
                ProductosDisponibles = await _dropdownService.GetProductosAsync()
            };
            _logger.LogInformation("Initialized new supplier view model: {@Model}", model);
            return model;
        }
    }
}
