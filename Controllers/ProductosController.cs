using javo2.IServices;
using javo2.ViewModels.Operaciones.Productos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace javo2.Controllers
{
    public class ProductosController : Controller
    {
        private readonly IProductoService _productoService;
        private readonly ICatalogoService _catalogoService;
        private readonly ILogger<ProductosController> _logger;

        public ProductosController(IProductoService productoService, ICatalogoService catalogoService, ILogger<ProductosController> logger)
        {
            _productoService = productoService;
            _catalogoService = catalogoService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Index action called");
            var productos = await _productoService.GetAllProductosAsync();
            _logger.LogInformation("Productos retrieved: {ProductosCount}", productos.Count());
            return View(productos);
        }

        public async Task<IActionResult> Create()
        {
            _logger.LogInformation("Create GET action called");
            var model = await InitializeNewProductViewModelAsync();
            _logger.LogInformation("Initial model data: {@Model}", model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductosViewModel model)
        {
            _logger.LogInformation("Create POST action called with Producto: {Producto}", model.Nombre);
            _logger.LogInformation("Model data received: {@Model}", model);

            // Asignar valores generados antes de la validación del modelo
            model.ProductoIDAlfa = _productoService.GenerarProductoIDAlfa();
            model.CodBarra = _productoService.GenerarCodBarraProducto();

            _logger.LogInformation("Generated values: ProductoIDAlfa: {ProductoIDAlfa}, CodBarra: {CodBarra}", model.ProductoIDAlfa, model.CodBarra);

            // Inicializar listas desplegables
            await PopulateDropDownListsAsync(model);

            if (ModelState.IsValid)
            {
                try
                {
                    _logger.LogInformation("Model state is valid. Proceeding to create product.");
                    await _productoService.CreateProductoAsync(model);
                    _logger.LogInformation("Producto created successfully");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error creating Producto: {Error}", ex.Message);
                    ModelState.AddModelError(string.Empty, "An error occurred while creating the product.");
                }
            }

            _logger.LogWarning("Model state is invalid. Errors: {Errors}", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            LogModelStateErrors();
            return View(model);
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

            await PopulateDropDownListsAsync(producto ?? new ProductosViewModel());
            _logger.LogInformation("Model data for Edit GET: {@Model}", producto);
            return View(producto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductosViewModel model)
        {
            _logger.LogInformation("Edit POST action called with Producto: {Producto}", model.Nombre);
            _logger.LogInformation("Model data received: {@Model}", model);

            // Inicializar listas desplegables
            await PopulateDropDownListsAsync(model);

            if (ModelState.IsValid)
            {
                try
                {
                    await _productoService.UpdateProductoAsync(model);
                    _logger.LogInformation("Producto updated successfully");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error updating Producto: {Error}", ex.Message);
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the product.");
                }
            }

            _logger.LogWarning("Model state is invalid. Errors: {Errors}", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            LogModelStateErrors();
            return View(model);
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
            return View(producto);
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
            return View(producto);
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
            }
            catch (Exception ex)
            {
                _logger.LogError("Error deleting Producto: {Error}", ex.Message);
                ModelState.AddModelError(string.Empty, "An error occurred while deleting the product.");
                var producto = await _productoService.GetProductoByIdAsync(id);
                return View(producto);
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropDownListsAsync(ProductosViewModel model)
        {
            model.Rubros = await _catalogoService.GetRubrosAsync();
            model.SubRubros = await _catalogoService.GetSubRubrosAsync();
            model.Marcas = await _catalogoService.GetMarcasAsync();
            _logger.LogInformation("PopulateDropDownListsAsync called. Rubros: {RubrosCount}, SubRubros: {SubRubrosCount}, Marcas: {MarcasCount}",
                model.Rubros.Count(), model.SubRubros.Count(), model.Marcas.Count());
        }

        private void LogModelStateErrors()
        {
            foreach (var key in ModelState.Keys)
            {
                var state = ModelState[key];
                if (state?.Errors != null)
                {
                    var errors = state.Errors.Select(e => e.ErrorMessage).ToList();
                    if (errors.Count > 0)
                    {
                        _logger.LogWarning("Model state errors for key '{Key}': {Errors}", key, string.Join(", ", errors));
                    }
                }
            }
        }

        private void LogModelData(ProductosViewModel model)
        {
            _logger.LogInformation("Model data: {Model}", new
            {
                model.ProductoID,
                model.ProductoIDAlfa,
                model.CodBarra,
                model.Nombre,
                model.Descripcion,
                model.PCosto,
                model.PContado,
                model.PLista,
                model.PorcentajeIva,
                model.SelectedRubroId,
                model.SelectedSubRubroId,
                model.SelectedMarcaId,
                model.Rubros,
                model.SubRubros,
                model.Marcas
            });
        }

        private async Task<ProductosViewModel> InitializeNewProductViewModelAsync()
        {
            var model = new ProductosViewModel
            {
                ProductoIDAlfa = _productoService.GenerarProductoIDAlfa(),
                CodBarra = _productoService.GenerarCodBarraProducto(),
                PorcentajeIva = 21,
                Usuario = "cosmefulanito",
                EstadoComentario = "test",
                DeudaTotal = 0,
                ModificadoPor = "cosmefulanito",
                Rubros = await _catalogoService.GetRubrosAsync(),
                SubRubros = await _catalogoService.GetSubRubrosAsync(),
                Marcas = await _catalogoService.GetMarcasAsync()
            };
            _logger.LogInformation("Initialized new product view model: {@Model}", model);
            return model;
        }
    }
}
