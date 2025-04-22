// Archivo: Controllers/ProveedoresController.cs
// Optimizaciones realizadas:
// - Se consolidó la lógica repetida en métodos helper
// - Se mejoró el manejo de excepciones
// - Se eliminó código duplicado

using AutoMapper;
using Javo2.Controllers.Base;
using Javo2.IServices;
using Javo2.IServices.Common;
using Javo2.Models;
using Javo2.ViewModels.Operaciones.Proveedores;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Javo2.Controllers
{
    public class ProveedoresController : BaseController
    {
        private readonly IProveedorService _proveedorService;
        private readonly IDropdownService _dropdownService;
        private readonly IProductoService _productoService;
        private readonly IStockService _stockService;
        private readonly IMapper _mapper;

        public ProveedoresController(
            IProveedorService proveedorService,
            IDropdownService dropdownService,
            IProductoService productoService,
            IStockService stockService,
            IMapper mapper,
            ILogger<ProveedoresController> logger)
            : base(logger)
        {
            _proveedorService = proveedorService;
            _dropdownService = dropdownService;
            _productoService = productoService;
            _stockService = stockService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Index action called");
                var proveedores = await _proveedorService.GetProveedoresAsync();
                var proveedoresViewModel = _mapper.Map<IEnumerable<ProveedoresViewModel>>(proveedores);

                await PopulateProductosAsignadosInfo(proveedoresViewModel);

                return View(proveedoresViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Index action of ProveedoresController");
                return View("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Filter(string filterField, string filterValue)
        {
            try
            {
                _logger.LogInformation("Filter action called with filterField: {FilterField}, filterValue: {FilterValue}", filterField, filterValue);
                var proveedores = await _proveedorService.GetProveedoresAsync();
                var proveedoresViewModel = _mapper.Map<IEnumerable<ProveedoresViewModel>>(proveedores);

                await PopulateProductosAsignadosInfo(proveedoresViewModel);

                if (!string.IsNullOrEmpty(filterValue))
                {
                    proveedoresViewModel = ApplyFilter(proveedoresViewModel, filterField, filterValue);
                }

                return PartialView("_ProveedoresTable", proveedoresViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Filter action of ProveedoresController");
                return PartialView("_ProveedoresTable", new List<ProveedoresViewModel>());
            }
        }

        public async Task<IActionResult> Create()
        {
            try
            {
                _logger.LogInformation("Create GET action called");
                var viewModel = await InitializeProveedorViewModelAsync();
                return View("Form", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Create GET action of ProveedoresController");
                return View("Error");
            }
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
                return View("Form", proveedorViewModel);
            }

            try
            {
                var proveedor = _mapper.Map<Proveedor>(proveedorViewModel);
                _logger.LogInformation("Mapped Proveedor ID before creation: {ProveedorID}", proveedor.ProveedorID);

                await _proveedorService.CreateProveedorAsync(proveedor);

                _logger.LogInformation("Proveedor created successfully with ID: {ProveedorID}", proveedor.ProveedorID);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Proveedor");
                ModelState.AddModelError(string.Empty, "Ocurrió un error al crear el proveedor.");
                return View("Form", proveedorViewModel);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                _logger.LogInformation("Edit GET action called with ID: {ID}", id);
                var proveedor = await _proveedorService.GetProveedorByIDAsync(id);
                if (proveedor == null)
                {
                    _logger.LogWarning("Proveedor with ID {ID} not found", id);
                    return NotFound();
                }

                var proveedorViewModel = _mapper.Map<ProveedoresViewModel>(proveedor);
                await PopulateProveedorViewModelAsync(proveedorViewModel, proveedor);

                return View("Form", proveedorViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Edit GET action of ProveedoresController");
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProveedoresViewModel proveedorViewModel)
        {
            _logger.LogInformation("Edit POST action called with ProveedorID: {ProveedorID}", proveedorViewModel.ProveedorID);

            await PopulateDropDownListsAsync(proveedorViewModel);

            if (!ModelState.IsValid)
            {
                LogModelStateErrors();
                return View("Form", proveedorViewModel);
            }

            try
            {
                var proveedor = _mapper.Map<Proveedor>(proveedorViewModel);
                _logger.LogInformation("Mapped Proveedor ID: {ProveedorID}", proveedor.ProveedorID);

                await _proveedorService.UpdateProveedorAsync(proveedor);
                _logger.LogInformation("Proveedor updated successfully with ID: {ProveedorID}", proveedor.ProveedorID);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Proveedor");
                ModelState.AddModelError(string.Empty, "Ocurrió un error al actualizar el proveedor.");
                return View("Form", proveedorViewModel);
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                _logger.LogInformation("Details action called with ID: {ID}", id);
                var proveedor = await _proveedorService.GetProveedorByIDAsync(id);
                if (proveedor == null)
                {
                    _logger.LogWarning("Proveedor with ID {ID} not found", id);
                    return NotFound();
                }

                var proveedorViewModel = _mapper.Map<ProveedoresViewModel>(proveedor);
                await PopulateProveedorViewModelAsync(proveedorViewModel, proveedor);

                return View(proveedorViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Details GET action of ProveedoresController");
                return View("Error");
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _logger.LogInformation("Delete GET action called with ID: {ID}", id);
                var proveedor = await _proveedorService.GetProveedorByIDAsync(id);
                if (proveedor == null)
                {
                    _logger.LogWarning("Proveedor with ID {ID} not found", id);
                    return NotFound();
                }

                var proveedorViewModel = _mapper.Map<ProveedoresViewModel>(proveedor);
                return View(proveedorViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Delete GET action of ProveedoresController");
                return View("Error");
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            _logger.LogInformation("Delete POST action called with ID: {ID}", id);
            try
            {
                await _proveedorService.DeleteProveedorAsync(id);
                _logger.LogInformation("Proveedor deleted successfully with ID: {ID}", id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Proveedor");
                ModelState.AddModelError(string.Empty, "Ocurrió un error al eliminar el proveedor.");
                var proveedor = await _proveedorService.GetProveedorByIDAsync(id);
                var proveedorViewModel = _mapper.Map<ProveedoresViewModel>(proveedor);
                return View(proveedorViewModel);
            }
        }

        [HttpGet]
        public async Task<IActionResult> SearchProducts(string term)
        {
            try
            {
                var products = await _productoService.GetProductosByTermAsync(term);
                var result = products.Select(p => new
                {
                    label = $"{p.Nombre} - {p.Marca?.Nombre ?? "Sin Marca"} {p.SubRubro?.Nombre ?? "Sin SubRubro"}",
                    value = p.ProductoID
                }).ToList();
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SearchProducts action of ProveedoresController");
                return Json(new List<object>());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarCompra(int proveedorID, int ProductoID, int cantidad)
        {
            _logger.LogInformation("RegistrarCompra POST called with ProveedorID={ProveedorID}, ProductoID={ProductoID}, Cantidad={Cantidad}",
                proveedorID, ProductoID, cantidad);

            try
            {
                await _proveedorService.RegistrarCompraAsync(proveedorID, ProductoID, cantidad);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registrando la compra");
                ModelState.AddModelError(string.Empty, "Ocurrió un error al registrar la compra.");
                return RedirectToAction(nameof(Index));
            }
        }

        // Métodos auxiliares
        private async Task PopulateDropDownListsAsync(ProveedoresViewModel model)
        {
            model.ProductosDisponibles = await _dropdownService.GetProductosAsync();
        }

        private async Task<ProveedoresViewModel> InitializeProveedorViewModelAsync()
        {
            var model = new ProveedoresViewModel
            {
                ProductosDisponibles = await _dropdownService.GetProductosAsync()
            };
            return model;
        }

        private async Task PopulateProductosAsignadosInfo(IEnumerable<ProveedoresViewModel> proveedoresViewModel)
        {
            foreach (var proveedorViewModel in proveedoresViewModel)
            {
                proveedorViewModel.ProductosAsignadosNombres = new List<string>();
                proveedorViewModel.ProductosAsignadosStocks = new List<int>();

                foreach (var ProductoID in proveedorViewModel.ProductosAsignados)
                {
                    var producto = await _productoService.GetProductoByIDAsync(ProductoID);
                    if (producto != null)
                    {
                        proveedorViewModel.ProductosAsignadosNombres.Add(producto.Nombre);

                        var stockItem = await _stockService.GetStockItemByProductoIDAsync(ProductoID);
                        int stockDisponible = stockItem != null ? stockItem.CantidadDisponible : 0;
                        proveedorViewModel.ProductosAsignadosStocks.Add(stockDisponible);
                    }
                    else
                    {
                        _logger.LogWarning("Producto con ID {ProductoID} no encontrado", ProductoID);
                        proveedorViewModel.ProductosAsignadosNombres.Add("Producto no encontrado");
                        proveedorViewModel.ProductosAsignadosStocks.Add(0);
                    }
                }
            }
        }

        private async Task PopulateProveedorViewModelAsync(ProveedoresViewModel viewModel, Proveedor proveedor)
        {
            viewModel.ProductosAsignadosNombres = new List<string>();
            foreach (var ProductoID in proveedor.ProductosAsignados)
            {
                var producto = await _productoService.GetProductoByIDAsync(ProductoID);
                if (producto != null)
                {
                    viewModel.ProductosAsignadosNombres.Add(producto.Nombre);
                }
                else
                {
                    _logger.LogWarning("Producto con ID {ProductoID} no encontrado", ProductoID);
                }
            }

            await PopulateDropDownListsAsync(viewModel);
            await PopulateProductosAsignadosStocks(new List<ProveedoresViewModel> { viewModel });
        }

        private async Task PopulateProductosAsignadosStocks(IEnumerable<ProveedoresViewModel> proveedoresViewModel)
        {
            foreach (var proveedorViewModel in proveedoresViewModel)
            {
                proveedorViewModel.ProductosAsignadosStocks = new List<int>();
                foreach (var ProductoID in proveedorViewModel.ProductosAsignados)
                {
                    var stockItem = await _stockService.GetStockItemByProductoIDAsync(ProductoID);
                    int stockDisponible = stockItem != null ? stockItem.CantidadDisponible : 0;
                    proveedorViewModel.ProductosAsignadosStocks.Add(stockDisponible);
                }
            }
        }

        private IEnumerable<ProveedoresViewModel> ApplyFilter(IEnumerable<ProveedoresViewModel> proveedores, string filterField, string filterValue)
        {
            switch (filterField)
            {
                case "nombre":
                    return proveedores.Where(p => p.Nombre.Contains(filterValue, StringComparison.OrdinalIgnoreCase));
                case "producto":
                    return proveedores.Where(p => p.ProductosAsignadosNombres.Any(n => n.Contains(filterValue, StringComparison.OrdinalIgnoreCase)));
                case "marca":
                    return proveedores.Where(p => p.ProductosAsignadosMarcas.Any(m => m.Contains(filterValue, StringComparison.OrdinalIgnoreCase)));
                case "submarca":
                    return proveedores.Where(p => p.ProductosAsignadosSubMarcas.Any(sm => sm.Contains(filterValue, StringComparison.OrdinalIgnoreCase)));
                default:
                    return proveedores;
            }
        }
    }
}