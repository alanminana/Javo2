// Archivo: Controllers/ProductosController.cs
using Javo2.Controllers.Base;
using Javo2.DTOs;
using Javo2.Helpers;
using Javo2.IServices;
using Javo2.IServices.Common;
using Javo2.Models;
using Javo2.ViewModels.Operaciones.Productos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using System.Threading.Tasks;

namespace Javo2.Controllers
{
    public class ProductosController : BaseController
    {
        private readonly IProductoService _productoService;
        private readonly IDropdownService _dropdownService;

        public ProductosController(
            IProductoService productoService,
            IDropdownService dropdownService,
            ILogger<ProductosController> logger)
            : base(logger)
        {
            _productoService = productoService;
            _dropdownService = dropdownService;
        }

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Index action called");
            var productos = await _productoService.GetAllProductosAsync();
            _logger.LogInformation("Productos retrieved: {ProductosCount}", productos.Count());

            // Mapear entidades Producto a ProductosViewModel
            var productosViewModel = productos.Select(p => new ProductosViewModel
            {
                ProductoID = p.ProductoID,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                PCosto = p.PCosto,
                PContado = p.PContado,
                PLista = p.PLista,
                PorcentajeIva = p.PorcentajeIva,
                // Mapear otras propiedades según sea necesario
            }).ToList();

            return View(productosViewModel);
        }

        public async Task<IActionResult> Details(int id)
        {
            _logger.LogInformation("Details action called with ID: {Id}", id);
            var producto = await _productoService.GetProductoByIdAsync(id);
            if (producto == null)
            {
                _logger.LogWarning("Producto with ID {Id} not found", id);
                return NotFound();
            }

            // Mapear Producto a ProductosViewModel
            var productoViewModel = new ProductosViewModel
            {
                ProductoID = producto.ProductoID,
                Nombre = producto.Nombre,
                Descripcion = producto.Descripcion,
                PCosto = producto.PCosto,
                PContado = producto.PContado,
                PLista = producto.PLista,
                PorcentajeIva = producto.PorcentajeIva,
                // Mapear otras propiedades según sea necesario
            };

            return View(productoViewModel);
        }

        public async Task<IActionResult> Create()
        {
            _logger.LogInformation("Create GET action called");
            var producto = await InitializeNewProductAsync();

            // Mapear Producto a ProductosViewModel
            var model = new ProductosViewModel
            {
                PorcentajeIva = producto.PorcentajeIva,
                Rubros = await _dropdownService.GetRubrosAsync(),
                Marcas = await _dropdownService.GetMarcasAsync(),
                SubRubros = new List<SelectListItem>(),
                // Inicializar otras propiedades según sea necesario
            };

            _logger.LogInformation("Initial model data: {@Model}", model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductosViewModel model)
        {
            _logger.LogInformation("Create POST action called with Producto: {Producto}", model.Nombre);

            await DropdownHelper.PopulateProductDropdownsAsync(_dropdownService, model);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state is invalid. Errors: {Errors}",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return View(model);
            }

            try
            {
                // Mapear ProductosViewModel a entidad Producto
                var producto = new Producto
                {
                    Nombre = model.Nombre,
                    Descripcion = model.Descripcion,
                    PCosto = model.PCosto,
                    PContado = model.PContado,
                    PLista = model.PLista,
                    PorcentajeIva = model.PorcentajeIva,
                    RubroId = model.SelectedRubroId,
                    SubRubroId = model.SelectedSubRubroId,
                    MarcaId = model.SelectedMarcaId,
                    // Asignar otras propiedades según sea necesario
                };

                await _productoService.CreateProductoAsync(producto);
                _logger.LogInformation("Producto created successfully");
                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                _logger.LogError("Error creating Producto: {Error}", ex.Message);
                ModelState.AddModelError(string.Empty, "Ocurrió un error al crear el producto.");
                return View(model);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            _logger.LogInformation("Edit GET action called with ID: {Id}", id);
            var producto = await _productoService.GetProductoByIdAsync(id);
            if (producto == null)
            {
                _logger.LogWarning("Producto with ID {Id} not found", id);
                return NotFound();
            }

            // Mapear Producto a ProductosViewModel
            var model = new ProductosViewModel
            {
                ProductoID = producto.ProductoID,
                Nombre = producto.Nombre,
                Descripcion = producto.Descripcion,
                PCosto = producto.PCosto,
                PContado = producto.PContado,
                PLista = producto.PLista,
                PorcentajeIva = producto.PorcentajeIva,
                SelectedRubroId = producto.RubroId,
                SelectedSubRubroId = producto.SubRubroId,
                SelectedMarcaId = producto.MarcaId,
                Rubros = await _dropdownService.GetRubrosAsync(),
                Marcas = await _dropdownService.GetMarcasAsync(),
                SubRubros = await _dropdownService.GetSubRubrosAsync(producto.RubroId),
                // Mapear otras propiedades según sea necesario
            };

            _logger.LogInformation("Model data for Edit GET: {@Model}", model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductosViewModel model)
        {
            _logger.LogInformation("Edit POST action called with Producto: {Producto}", model.Nombre);

            await DropdownHelper.PopulateProductDropdownsAsync(_dropdownService, model);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state is invalid. Errors: {Errors}",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return View(model);
            }

            try
            {
                // Mapear ProductosViewModel a entidad Producto
                var producto = new Producto
                {
                    ProductoID = model.ProductoID,
                    Nombre = model.Nombre,
                    Descripcion = model.Descripcion,
                    PCosto = model.PCosto,
                    PContado = model.PContado,
                    PLista = model.PLista,
                    PorcentajeIva = model.PorcentajeIva,
                    RubroId = model.SelectedRubroId,
                    SubRubroId = model.SelectedSubRubroId,
                    MarcaId = model.SelectedMarcaId,
                    // Asignar otras propiedades según sea necesario
                };

                await _productoService.UpdateProductoAsync(producto);
                _logger.LogInformation("Producto updated successfully");
                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                _logger.LogError("Error updating Producto: {Error}", ex.Message);
                ModelState.AddModelError(string.Empty, "Ocurrió un error al actualizar el producto.");
                return View(model);
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Delete GET action called with ID: {Id}", id);
            var producto = await _productoService.GetProductoByIdAsync(id);
            if (producto == null)
            {
                _logger.LogWarning("Producto with ID {Id} not found", id);
                return NotFound();
            }

            // Mapear Producto a ProductosViewModel
            var model = new ProductosViewModel
            {
                ProductoID = producto.ProductoID,
                Nombre = producto.Nombre,
                Descripcion = producto.Descripcion,
                // Mapear otras propiedades según sea necesario
            };

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            _logger.LogInformation("Delete POST action called with ID: {Id}", id);
            try
            {
                await _productoService.DeleteProductoAsync(id);
                _logger.LogInformation("Producto deleted successfully");
                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                _logger.LogError("Error deleting Producto: {Error}", ex.Message);
                ModelState.AddModelError(string.Empty, "Ocurrió un error al eliminar el producto.");
                var producto = await _productoService.GetProductoByIdAsync(id);

                // Mapear Producto a ProductosViewModel
                var model = new ProductosViewModel
                {
                    ProductoID = producto.ProductoID,
                    Nombre = producto.Nombre,
                    Descripcion = producto.Descripcion,
                    // Mapear otras propiedades según sea necesario
                };

                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSubRubros(int rubroId)
        {
            _logger.LogInformation("GetSubRubros called with RubroId: {RubroId}", rubroId);
            var subRubros = await _dropdownService.GetSubRubrosAsync(rubroId);
            return Json(subRubros);
        }

        [HttpGet]
        public async Task<IActionResult> Filter(string Nombre, string Codigo, string Rubro, string SubRubro, string Marca)
        {
            _logger.LogInformation("Filter action called with parameters: Nombre={Nombre}, Codigo={Codigo}, Rubro={Rubro}, SubRubro={SubRubro}, Marca={Marca}", Nombre, Codigo, Rubro, SubRubro, Marca);

            var filters = new ProductoFilterDto
            {
                Nombre = Nombre,
                Codigo = Codigo,
                Rubro = Rubro,
                SubRubro = SubRubro,
                Marca = Marca
            };

            var productos = await _productoService.FilterProductosAsync(filters);

            // Mapear entidades Producto a ProductosViewModel
            var productosViewModel = productos.Select(p => new ProductosViewModel
            {
                ProductoID = p.ProductoID,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                PCosto = p.PCosto,
                PContado = p.PContado,
                PLista = p.PLista,
                PorcentajeIva = p.PorcentajeIva,
                // Mapear otras propiedades según sea necesario
            }).ToList();

            return PartialView("_ProductosTable", productosViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IncrementarPrecios(string productoIds, decimal porcentaje)
        {
            _logger.LogInformation("IncrementarPrecios called with ProductoIds: {ProductoIds} and Porcentaje: {Porcentaje}", productoIds, porcentaje);

            if (string.IsNullOrEmpty(productoIds))
            {
                return Json(new { success = false, message = "Datos inválidos." });
            }

            var ids = productoIds.Split(',').Select(int.Parse).ToArray();
            foreach (var id in ids)
            {
                var producto = await _productoService.GetProductoByIdAsync(id);
                if (producto != null)
                {
                    producto.PCosto += producto.PCosto * (porcentaje / 100);
                    producto.PContado += producto.PContado * (porcentaje / 100);
                    producto.PLista += producto.PLista * (porcentaje / 100);

                    await _productoService.UpdateProductoAsync(producto);
                }
            }

            return Json(new { success = true });
        }

        private async Task<Producto> InitializeNewProductAsync()
        {
            return await Task.FromResult(new Producto());
        }
    }
}
