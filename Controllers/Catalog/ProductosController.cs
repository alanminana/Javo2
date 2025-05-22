// Controllers/Catalog/ProductosController.cs
using AutoMapper;
using Javo2.Controllers.Base;
using Javo2.DTOs;
using Javo2.IServices;
using Javo2.Models;
using Javo2.Services.Catalog;
using Javo2.Services.Reporting;
using Javo2.ViewModels.Operaciones.Catalogo;
using Javo2.ViewModels.Operaciones.Productos;
using Javo2.ViewModels.Operaciones.Stock;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Javo2.Controllers.Catalog
{
    [Authorize(Policy = "PermisoPolitica")]
    public class ProductosController : ValidationBaseController
    {
        private readonly IStockService _stockService;
        private readonly IAjustePrecioService _ajustePrecioService;
        private readonly IProductoService _productoService;
        private readonly IProductSearchService _productSearchService;
        private readonly ICatalogoService _catalogoService;
        private readonly IStockService _stockService;
        private readonly IAuditoriaService _auditoriaService;
        private readonly IAjustePrecioService _ajustePrecioService;
        private readonly IMapper _mapper;
        public ProductosController(
         IProductoService productoService,
         IProductSearchService productSearchService,
         ICatalogoService catalogoService,
         IStockService stockService,
         IAuditoriaService auditoriaService,
         IAjustePrecioService ajustePrecioService,
         IMapper mapper,
         ILogger<ProductosController> logger
     ) : base(logger)
        {
            _productoService = productoService;
            _productSearchService = productSearchService;
            _catalogoService = catalogoService;
            _stockService = stockService;
            _auditoriaService = auditoriaService;
            _ajustePrecioService = ajustePrecioService;
            _mapper = mapper;
        }
        #region Listado y Búsqueda de Productos

        // GET: Productos
        [Authorize(Policy = "Permission:productos.ver")]
        public async Task<IActionResult> Index()
        {
            try
            {
                LogInfo("ProductosController: Index GET");
                var productos = await _productoService.GetAllProductosAsync();
                var model = _mapper.Map<List<ProductosViewModel>>(productos);
                return View(model);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error en Index de Productos");
                return View("Error");
            }
        }

        // GET: Productos/Details/5
        [Authorize(Policy = "Permission:productos.ver")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                LogInfo("ProductosController: Details GET con ID={ID}", id);
                var producto = await _productoService.GetProductoByIDAsync(id);
                if (producto == null) return NotFound();

                var model = _mapper.Map<ProductosViewModel>(producto);
                return View(model);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error en Details de Productos");
                return View("Error");
            }
        }

        #endregion

        #region CRUD de Productos

        // GET: Productos/Create
        [HttpGet]
        [Authorize(Policy = "Permission:productos.crear")]
        public async Task<IActionResult> Create()
        {
            try
            {
                var model = new ProductosViewModel();

                // Usar el método común para cargar dropdowns
                await PopulateProductDropdownsAsync(model);

                return View("Form", model);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error en Create GET de Productos");
                return View("Error");
            }
        }

        // POST: Productos/Create
        [HttpPost]
        [Authorize(Policy = "Permission:productos.crear")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductosViewModel model)
        {
            model.ModificadoPor = User.Identity?.Name ?? "Sistema";
            ModelState.Remove(nameof(model.ModificadoPor));
            ModelState.Remove(nameof(model.PContado));
            ModelState.Remove(nameof(model.PLista));

            if (!ModelState.IsValid)
            {
                LogModelStateErrors();
                await PopulateProductDropdownsAsync(model);
                return View("Form", model);
            }

            try
            {
                // Preparar el producto
                var producto = new Producto
                {
                    Nombre = model.Nombre,
                    Descripcion = model.Descripcion,
                    PCosto = model.PCosto,
                    PorcentajeIva = model.PorcentajeIva,
                    RubroID = model.SelectedRubroID,
                    SubRubroID = model.SelectedSubRubroID,
                    MarcaID = model.SelectedMarcaID,
                    FechaMod = DateTime.Now,
                    ModificadoPor = model.ModificadoPor,
                    CodigoAlfa = await _productoService.GenerarProductoIDAlfa(),
                    CodigoBarra = await _productoService.GenerarCodBarraProducto()
                };

                // Validar relaciones
                if (!await _productSearchService.ValidateProductAsync(producto))
                {
                    ModelState.AddModelError("", "Por favor, verifique que el rubro, subrubro y marca sean válidos");
                    await PopulateProductDropdownsAsync(model);
                    return View("Form", model);
                }

                // Crear producto
                await _productoService.CreateProductoAsync(producto);

                // Manejar stock inicial
                if (model.StockInicial > 0)
                {
                    await _productSearchService.UpdateStockAsync(
                        producto.ProductoID,
                        model.StockInicial,
                        "Stock inicial"
                    );
                }

                // Usar método común de auditoría
                await RegistrarAuditoriaProductoAsync(
                    "Create",
                    producto.ProductoID,
                    producto.Nombre,
                    $"PCosto: {producto.PCosto:C}, Stock inicial: {model.StockInicial}"
                );

                SetSuccessMessage("Producto creado exitosamente");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al crear producto");
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateProductDropdownsAsync(model);
                return View("Form", model);
            }
        }

        // GET: Productos/Edit/5
        [HttpGet]
        [Authorize(Policy = "Permission:productos.editar")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                LogInfo("ProductosController: Edit GET con ID={ID}", id);
                var producto = await _productoService.GetProductoByIDAsync(id);
                if (producto == null) return NotFound();

                var model = _mapper.Map<ProductosViewModel>(producto);
                await PopulateProductDropdownsAsync(model);
                return View("Form", model);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error en Edit GET de Productos");
                return View("Error");
            }
        }

        // POST: Productos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:productos.editar")]
        public async Task<IActionResult> Edit(ProductosViewModel model)
        {
            model.ModificadoPor = User.Identity?.Name ?? "Sistema";
            ModelState.Remove(nameof(model.ModificadoPor));
            ModelState.Remove(nameof(model.PContado));
            ModelState.Remove(nameof(model.PLista));

            if (!ModelState.IsValid)
            {
                LogModelStateErrors();
                await PopulateProductDropdownsAsync(model);
                return View("Form", model);
            }

            try
            {
                // Obtener producto existente
                var producto = await _productoService.GetProductoByIDAsync(model.ProductoID);
                if (producto == null)
                {
                    return NotFound();
                }

                // Actualizar propiedades
                producto.Nombre = model.Nombre;
                producto.Descripcion = model.Descripcion;
                producto.PCosto = model.PCosto;
                producto.PorcentajeIva = model.PorcentajeIva;
                producto.RubroID = model.SelectedRubroID;
                producto.SubRubroID = model.SelectedSubRubroID;
                producto.MarcaID = model.SelectedMarcaID;
                producto.FechaMod = DateTime.Now;
                producto.ModificadoPor = model.ModificadoPor;

                // Validar relaciones
                if (!await _productSearchService.ValidateProductAsync(producto))
                {
                    ModelState.AddModelError("", "Por favor, verifique que el rubro, subrubro y marca sean válidos");
                    await PopulateProductDropdownsAsync(model);
                    return View("Form", model);
                }

                // Actualizar producto
                await _productoService.UpdateProductoAsync(producto);

                // Manejar cambio de stock
                if (model.StockInicial != 0)
                {
                    await _productSearchService.UpdateStockAsync(
                        model.ProductoID,
                        model.StockInicial,
                        "Ajuste manual"
                    );
                }

                // Usar método común de auditoría
                await RegistrarAuditoriaProductoAsync(
                    "Update",
                    model.ProductoID,
                    model.Nombre,
                    $"PCosto: {model.PCosto:C}"
                );

                SetSuccessMessage("Producto actualizado exitosamente");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al actualizar producto");
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateProductDropdownsAsync(model);
                return View("Form", model);
            }
        }

        // GET: Productos/Delete/5
        [Authorize(Policy = "Permission:productos.eliminar")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var producto = await _productoService.GetProductoByIDAsync(id);
                if (producto == null) return NotFound();

                var model = _mapper.Map<ProductosViewModel>(producto);
                return View(model);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al preparar eliminación de producto");
                return View("Error");
            }
        }

        // POST: Productos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:productos.eliminar")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var producto = await _productoService.GetProductoByIDAsync(id);
                if (producto == null) return NotFound();

                string nombre = producto.Nombre;

                await _productoService.DeleteProductoAsync(id);

                // Usar método común de auditoría
                await RegistrarAuditoriaProductoAsync("Delete", id, nombre);

                SetSuccessMessage("Producto eliminado exitosamente");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al eliminar producto");
                SetErrorMessage("Error al eliminar el producto: " + ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        #endregion

        #region Catálogo de Productos

        // GET: Productos/Catalogo
        [Authorize(Policy = "Permission:productos.ver")]
        public async Task<IActionResult> Catalogo()
        {
            try
            {
                LogInfo("ProductosController: Catalogo GET");

                // Obtener productos, rubros y marcas
                var productos = await _productoService.GetAllProductosAsync();
                var (rubros, marcas) = await _productSearchService.GetCatalogFiltersAsync();

                // Cargar totales de stock
                var (rubrosStock, marcasStock) = await _productSearchService.GetStockByRubroMarcaAsync();

                var model = new CatalogoProductosViewModel
                {
                    Productos = _mapper.Map<IEnumerable<ProductosViewModel>>(productos),
                    Rubros = _mapper.Map<IEnumerable<RubroViewModel>>(rubros),
                    Marcas = _mapper.Map<IEnumerable<MarcaViewModel>>(marcas)
                };

                // Asignar totales de stock a los ViewModels
                foreach (var rubroVm in model.Rubros)
                {
                    rubroVm.TotalStock = rubrosStock.TryGetValue(rubroVm.ID, out int totalRubroStock)
                        ? totalRubroStock : 0;
                }

                foreach (var marcaVm in model.Marcas)
                {
                    marcaVm.TotalStock = marcasStock.TryGetValue(marcaVm.ID, out int totalMarcaStock)
                        ? totalMarcaStock : 0;
                }

                return View(model);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error en Catalogo de Productos");
                return View("Error");
            }
        }

        // GET: Productos/FilterProductos
        // GET: Productos/FilterProductos
        [HttpGet]
        [Authorize(Policy = "Permission:productos.ver")]
        public async Task<IActionResult> FilterProductos(string filterType, string filterValue)
        {
            try
            {
                var filters = new ProductoFilterDto();

                switch (filterType)
                {
                    case "Nombre":
                        filters.Nombre = filterValue;
                        break;
                    case "Codigo":
                        filters.Codigo = filterValue;
                        break;
                    case "Rubro":
                        filters.Rubro = filterValue;
                        break;
                    case "SubRubro":
                        filters.SubRubro = filterValue;
                        break;
                    case "Marca":
                        filters.Marca = filterValue;
                        break;
                }

                return await FilterProductsAsync(filters);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error en FilterProductos");
                return Json(new { error = "Error al filtrar productos" });
            }
        }

        // GET: Productos/FilterRubros
        [HttpGet]
        [Authorize(Policy = "Permission:catalogo.ver")]
        public async Task<IActionResult> FilterRubros(string term)
        {
            return await FilterRubrosAsync(term);
        }

        // GET: Productos/FilterMarcas
        [HttpGet]
        [Authorize(Policy = "Permission:catalogo.ver")]
        public async Task<IActionResult> FilterMarcas(string term)
        {
            return await FilterMarcasAsync(term);
        }

        #endregion

        #region Operaciones de Stock

        // GET: Productos/AjusteStock/5
        [HttpGet]
        [Authorize(Policy = "Permission:productos.editar")]
        public async Task<IActionResult> AjusteStock(int id)
        {
            try
            {
                var producto = await _productoService.GetProductoByIDAsync(id);
                if (producto == null) return NotFound();

                var stockItem = await _productSearchService.GetStockItemAsync(id);

                var model = new AjusteStockViewModel
                {
                    ProductoID = id,
                    CantidadActual = stockItem?.CantidadDisponible ?? 0,
                    NombreProducto = producto.Nombre
                };

                return View(model);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al cargar formulario de ajuste de stock");
                return View("Error");
            }
        }

        // POST: Productos/AjusteStock/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:productos.editar")]
        public async Task<IActionResult> AjusteStock(AjusteStockViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // Determinar la diferencia de stock
            var stockItem = await _productSearchService.GetStockItemAsync(model.ProductoID);
            int cantidadActual = stockItem?.CantidadDisponible ?? 0;
            int diferencia = model.NuevaCantidad - cantidadActual;

            // Usar método común de la clase base
            return await HandleStockUpdateAsync(model.ProductoID, diferencia, model.Motivo);
        }

        // GET: Productos/MovimientosStock/5
        [HttpGet]
        [Authorize(Policy = "Permission:productos.ver")]
        public async Task<IActionResult> MovimientosStock(int id)
        {
            try
            {
                var producto = await _productoService.GetProductoByIDAsync(id);
                if (producto == null) return NotFound();

                var stockItem = await _productSearchService.GetStockItemAsync(id);
                var movimientos = await _productSearchService.GetStockMovementsAsync(id);

                var model = new StockItemViewModel
                {
                    StockItemID = stockItem?.StockItemID ?? 0,
                    ProductoID = id,
                    NombreProducto = producto.Nombre,
                    CantidadDisponible = stockItem?.CantidadDisponible ?? 0,
                    Movimientos = _mapper.Map<IEnumerable<MovimientoStockViewModel>>(movimientos)
                };

                return View(model);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al cargar movimientos de stock");
                return View("Error");
            }
        }

        #endregion

        #region Ajuste de Precios

        // POST: Productos/IncrementarPrecios
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:productos.ajustarprecios")]
        public async Task<IActionResult> IncrementarPrecios(string ProductoIDs, decimal porcentaje, bool isAumento, string descripcion = "")
        {
            if (string.IsNullOrEmpty(ProductoIDs))
                return JsonError("Seleccione productos.");

            var ids = ProductoIDs.Split(',').Select(int.Parse).ToList();

            try
            {
                // Usar IAjustePrecioService en lugar de IProductoService
                var ajusteId = await _ajustePrecioService.AjustarPreciosAsync(
                    ids,
                    porcentaje,
                    isAumento,
                    descripcion ?? "Ajuste rápido desde listado de productos"
                );

                // Usar método común de auditoría
                await RegistrarAuditoriaAjustePrecioAsync(
                    "UpdatePrices",
                    ids.ToArray(),
                    isAumento,
                    porcentaje,
                    descripcion ?? "Ajuste rápido desde listado de productos"
                );

                return JsonSuccess(
                    $"Ajuste de precios aplicado correctamente a {ids.Count} productos.",
                    new { ajusteId = ajusteId }
                );
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al aplicar ajuste rápido de precios");
                return JsonError($"Error: {ex.Message}");
            }
        }

        #endregion

        #region Métodos API comunes

        [HttpGet]
        public async Task<IActionResult> GetSubRubros(int rubroId)
        {
            return await GetSubRubrosAsync(rubroId);
        }

        [HttpPost]
        [Authorize(Policy = "Permission:productos.ver")]
        public async Task<IActionResult> BuscarProducto(string codigoProducto)
        {
            return await BuscarProductoPorCodigoAsync(codigoProducto);
        }

        [HttpGet]
        [Authorize(Policy = "Permission:productos.ver")]
        public async Task<IActionResult> Filter(ProductoFilterDtoViewModel filters)
        {
            try
            {
                var dto = new ProductoFilterDto
                {
                    Nombre = filters.Nombre,
                    Categoria = filters.Categoria,
                    PrecioMinimo = filters.PrecioMinimo,
                    PrecioMaximo = filters.PrecioMaximo,
                    Codigo = filters.Codigo,
                    Rubro = filters.Rubro,
                    SubRubro = filters.SubRubro,
                    Marca = filters.Marca
                };

                return await FilterProductsAsync(dto);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al filtrar productos: {Filtros}", filters);
                return PartialView("_ProductosTable", new List<ProductosViewModel>());
            }
        }

        #endregion
    }
}