// Archivo: Controllers/ProductosController.cs
using AutoMapper;
using Javo2.Controllers.Base;
using Javo2.DTOs;
using Javo2.IServices;
using Javo2.IServices.Common;
using Javo2.Models;
using Javo2.Services;
using Javo2.ViewModels.Operaciones.Productos;
using Javo2.ViewModels.Operaciones.Stock;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Javo2.Controllers
{
    [Authorize(Policy = "PermisoPolitica")]

    public class ProductosController : BaseController
    {
        private readonly IProductoService _productoService;
        private readonly IDropdownService _dropdownService;
        private readonly ICatalogoService _catalogoService;
        private readonly IStockService _stockService;
        private readonly IMapper _mapper;
        private readonly IAuditoriaService _auditoriaService;
        private readonly IAjustePrecioService _ajustePrecioService;
        // Cambiar el campo _logger para que sea readonly y eliminar la asignación redundante en el constructor.

        private readonly ILogger<ProductosController> _logger; // Ya es readonly, no necesita asignación adicional en el constructor.

        public ProductosController(
     IProductoService productoService,
     IDropdownService dropdownService,
     ICatalogoService catalogoService,
     IStockService stockService,
     IMapper mapper,
     IAuditoriaService auditoriaService,
     IAjustePrecioService ajustePrecioService,  // Añadir esto
     ILogger<ProductosController> logger
 ) : base(logger)
        {
            _productoService = productoService;
            _dropdownService = dropdownService;
            _catalogoService = catalogoService;
            _stockService = stockService;
            _mapper = mapper;
            _auditoriaService = auditoriaService;
            _ajustePrecioService = ajustePrecioService;  // Asignar esto
            _logger = logger;
        }

        // GET: Productos
        [Authorize(Policy = "Permission:productos.ver")]

        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("ProductosController: Index GET");
                var productos = await _productoService.GetAllProductosAsync();
                var model = _mapper.Map<List<ProductosViewModel>>(productos);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Index de Productos");
                return View("Error");
            }
        }

        // GET: Productos/Details/5
        [Authorize(Policy = "Permission:productos.ver")]

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                _logger.LogInformation("ProductosController: Details GET con ID={ID}", id);
                var producto = await _productoService.GetProductoByIDAsync(id);
                if (producto == null) return NotFound();

                var model = _mapper.Map<ProductosViewModel>(producto);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Details de Productos");
                return View("Error");
            }
        }
        // GET: Productos/Create
        [HttpGet]
        [Authorize(Policy = "Permission:productos.crear")]

        public async Task<IActionResult> Create()
        {
            try
            {
                var model = new ProductosViewModel();
                model.Rubros = await _dropdownService.GetRubrosAsync();

                // Seleccionar explícitamente el primer rubro
                if (model.Rubros.Any())
                {
                    model.SelectedRubroID = int.Parse(model.Rubros.First().Value);
                    // Cargar explícitamente los subrubros del primer rubro
                    model.SubRubros = await _dropdownService.GetSubRubrosAsync(model.SelectedRubroID);
                }

                model.Marcas = await _dropdownService.GetMarcasAsync();
                return View("Form", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Create GET de Productos");
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
            ModelState.Remove(nameof(model.PContado)); // No validar estos campos
            ModelState.Remove(nameof(model.PLista));   // ya que se calculan automáticamente

            if (!ModelState.IsValid)
            {
                LogModelStateErrors();
                await PopulateDropdownsAsync(model);
                return View("Form", model);
            }

            try
            {
                var marca = await _catalogoService.GetMarcaByIDAsync(model.SelectedMarcaID);
                var rubro = await _catalogoService.GetRubroByIDAsync(model.SelectedRubroID);
                var sub = await _catalogoService.GetSubRubroByIDAsync(model.SelectedSubRubroID);

                if (marca == null || rubro == null || sub == null)
                {
                    if (marca == null) ModelState.AddModelError(string.Empty, "Marca inválida.");
                    if (rubro == null) ModelState.AddModelError(string.Empty, "Rubro inválido.");
                    if (sub == null) ModelState.AddModelError(string.Empty, "SubRubro inválido.");
                    await PopulateDropdownsAsync(model);
                    return View("Form", model);
                }

                // Solo mapear PCosto, los demás precios se calculan por el servicio
                var producto = new Producto
                {
                    CodigoAlfa = _productoService.GenerarProductoIDAlfa(),
                    CodigoBarra = _productoService.GenerarCodBarraProducto(),
                    Nombre = model.Nombre,
                    Descripcion = model.Descripcion,
                    PCosto = model.PCosto,
                    PorcentajeIva = model.PorcentajeIva,
                    RubroID = rubro.ID,
                    SubRubroID = sub.ID,
                    MarcaID = marca.ID,
                    FechaMod = DateTime.Now,
                    FechaModPrecio = DateTime.Now,
                    ModificadoPor = model.ModificadoPor,
                    Estado = Producto.EstadoProducto.Activo
                };

                // El servicio aplicará las reglas de precios automáticamente
                await _productoService.CreateProductoAsync(producto);

                if (model.StockInicial > 0)
                {
                    var stockItem = new StockItem { ProductoID = producto.ProductoID, CantidadDisponible = model.StockInicial };
                    await _stockService.CreateStockItemAsync(stockItem);
                    await _stockService.RegistrarMovimientoAsync(new MovimientoStock
                    {
                        ProductoID = producto.ProductoID,
                        Fecha = DateTime.Now,
                        TipoMovimiento = "Entrada",
                        Cantidad = model.StockInicial,
                        Motivo = "Stock inicial"
                    });
                }

                await _auditoriaService.RegistrarCambioAsync(new AuditoriaRegistro
                {
                    FechaHora = DateTime.Now,
                    Usuario = User.Identity?.Name ?? "Sistema",
                    Entidad = "Producto",
                    Accion = "Create",
                    LlavePrimaria = producto.ProductoID.ToString(),
                    Detalle = producto.Nombre
                });

                TempData["Success"] = "Producto creado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear producto");
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateDropdownsAsync(model);
                return View("Form", model);
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
            ModelState.Remove(nameof(model.PContado)); // No validar estos campos
            ModelState.Remove(nameof(model.PLista));   // ya que se calculan automáticamente

            if (!ModelState.IsValid)
            {
                LogModelStateErrors();
                await PopulateDropdownsAsync(model);
                return View("Form", model);
            }

            try
            {
                var existing = await _productoService.GetProductoByIDAsync(model.ProductoID);
                if (existing == null) return NotFound();

                // Solo actualizar los campos que el usuario debe controlar
                existing.Nombre = model.Nombre;
                existing.Descripcion = model.Descripcion;
                existing.PCosto = model.PCosto;  // Solo actualizamos PCosto
                existing.PorcentajeIva = model.PorcentajeIva;
                existing.RubroID = model.SelectedRubroID;
                existing.SubRubroID = model.SelectedSubRubroID;
                existing.MarcaID = model.SelectedMarcaID;
                existing.FechaMod = DateTime.Now;
                existing.ModificadoPor = model.ModificadoPor;

                // El servicio aplicará automáticamente las reglas para actualizar PContado y PLista
                await _productoService.UpdateProductoAsync(existing);

                if (model.StockInicial != 0)
                {
                    var stockItem = await _stockService.GetStockItemByProductoIDAsync(model.ProductoID);
                    if (stockItem == null)
                    {
                        stockItem = new StockItem { ProductoID = model.ProductoID, CantidadDisponible = Math.Max(0, model.StockInicial) };
                        await _stockService.CreateStockItemAsync(stockItem);
                    }
                    else
                    {
                        stockItem.CantidadDisponible = Math.Max(0, stockItem.CantidadDisponible + model.StockInicial);
                        await _stockService.UpdateStockItemAsync(stockItem);
                    }
                    await _stockService.RegistrarMovimientoAsync(new MovimientoStock
                    {
                        ProductoID = model.ProductoID,
                        Fecha = DateTime.Now,
                        TipoMovimiento = model.StockInicial > 0 ? "Entrada" : "Salida",
                        Cantidad = Math.Abs(model.StockInicial),
                        Motivo = "Ajuste manual"
                    });
                }

                await _auditoriaService.RegistrarCambioAsync(new AuditoriaRegistro
                {
                    FechaHora = DateTime.Now,
                    Usuario = model.ModificadoPor,
                    Entidad = "Producto",
                    Accion = "Update",
                    LlavePrimaria = model.ProductoID.ToString(),
                    Detalle = model.Nombre
                });

                TempData["Success"] = "Producto actualizado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar producto");
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateDropdownsAsync(model);
                return View("Form", model);
            }
        }

        // GET: Productos/Delete/5
        [Authorize(Policy = "Permission:productos.eliminar")]

        public async Task<IActionResult> Delete(int id)
        {
            var producto = await _productoService.GetProductoByIDAsync(id);
            if (producto == null) return NotFound();
            var model = _mapper.Map<ProductosViewModel>(producto);
            return View(model);
        }

        // POST: Productos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:productos.eliminar")]

        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _productoService.DeleteProductoAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetSubRubros(int rubroId)
        {
            _logger.LogInformation("GetSubRubros recibió petición para rubroId: {0}", rubroId);

            if (rubroId <= 0)
            {
                _logger.LogWarning("GetSubRubros: rubroId inválido: {0}", rubroId);
                return Json(new List<SelectListItem>());
            }

            try
            {
                var items = await _dropdownService.GetSubRubrosAsync(rubroId);
                _logger.LogInformation("GetSubRubros: Obtenidos {0} subrubros para rubroId {1}: {2}",
                    items.Count(),
                    rubroId,
                    string.Join(", ", items.Select(i => i.Text)));
                return Json(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo subrubros para rubroId {0}", rubroId);
                return Json(new List<SelectListItem>());
            }
        }

        // POST: Productos/IncrementarPrecios
        // POST: Productos/IncrementarPrecios
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IncrementarPrecios(string ProductoIDs, decimal porcentaje, bool isAumento, string descripcion = "")
        {
            if (string.IsNullOrEmpty(ProductoIDs))
                return Json(new { success = false, message = "Seleccione productos." });

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

                await _auditoriaService.RegistrarCambioAsync(new AuditoriaRegistro
                {
                    FechaHora = DateTime.Now,
                    Usuario = User.Identity?.Name ?? "Sistema",
                    Entidad = "Producto",
                    Accion = "UpdatePrices",
                    LlavePrimaria = string.Join(',', ids),
                    Detalle = $"{(isAumento ? "Aumento" : "Descuento")} {porcentaje}%"
                });

                return Json(new
                {
                    success = true,
                    message = $"Ajuste de precios aplicado correctamente a {ids.Count} productos.",
                    ajusteId = ajusteId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al aplicar ajuste rápido de precios");
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // GET: Productos/AjusteStock/5
        [HttpGet]
        public async Task<IActionResult> AjusteStock(int id)
        {
            var producto = await _productoService.GetProductoByIDAsync(id);
            if (producto == null) return NotFound();
            var item = await _stockService.GetStockItemByProductoIDAsync(id);
            var model = new AjusteStockViewModel
            {
                ProductoID = id,
                CantidadActual = item?.CantidadDisponible ?? 0
            };
            return View(model);
        }
        // GET: Productos/Edit/5
        [HttpGet]
        [Authorize(Policy = "Permission:productos.editar")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                _logger.LogInformation("ProductosController: Edit GET con ID={ID}", id);
                var producto = await _productoService.GetProductoByIDAsync(id);
                if (producto == null) return NotFound();

                var model = _mapper.Map<ProductosViewModel>(producto);
                await PopulateDropdownsAsync(model);
                return View("Form", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Edit GET de Productos");
                return View("Error");
            }
        }
        // POST: Productos/AjusteStock/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AjusteStock(AjusteStockViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var stockItem = await _stockService.GetStockItemByProductoIDAsync(model.ProductoID) ?? new StockItem { ProductoID = model.ProductoID };
            int diff = model.NuevaCantidad - stockItem.CantidadDisponible;
            stockItem.CantidadDisponible = model.NuevaCantidad;
            if (stockItem.StockItemID == 0) await _stockService.CreateStockItemAsync(stockItem);
            else await _stockService.UpdateStockItemAsync(stockItem);

            if (diff != 0)
            {
                await _stockService.RegistrarMovimientoAsync(new MovimientoStock
                {
                    ProductoID = model.ProductoID,
                    Fecha = DateTime.Now,
                    TipoMovimiento = diff > 0 ? "Entrada" : "Salida",
                    Cantidad = Math.Abs(diff),
                    Motivo = model.Motivo
                });
            }

            TempData["Success"] = "Stock actualizado.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Productos/Filter
        [HttpGet]
        public async Task<IActionResult> Filter(ProductoFilterDtoViewModel filters)
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
            var productos = await _productoService.FilterProductosAsync(dto);
            var model = _mapper.Map<List<ProductosViewModel>>(productos);
            return PartialView("_ProductosTable", model);
        }

        // GET: Productos/MovimientosStock/5
        [HttpGet]
        public async Task<IActionResult> MovimientosStock(int id)
        {
            var prod = await _productoService.GetProductoByIDAsync(id);
            if (prod == null) return NotFound();
            var item = await _stockService.GetStockItemByProductoIDAsync(id);
            var movs = await _stockService.GetMovimientosByProductoIDAsync(id);
            var model = new StockItemViewModel
            {
                StockItemID = item?.StockItemID ?? 0,
                ProductoID = id,
                NombreProducto = prod.Nombre,
                CantidadDisponible = item?.CantidadDisponible ?? 0,
                Movimientos = _mapper.Map<IEnumerable<MovimientoStockViewModel>>(movs)
            };
            return View(model);
        }

        // Auxiliar: poblado de dropdowns
        private async Task PopulateDropdownsAsync(ProductosViewModel model)
        {
            // Cargar rubros si no existen
            model.Rubros = await _dropdownService.GetRubrosAsync();

            // Cargar marcas
            model.Marcas = await _dropdownService.GetMarcasAsync();

            // Cargar subrubros del rubro seleccionado
            if (model.SelectedRubroID > 0)
            {
                _logger.LogInformation($"Cargando subrubros para RubroID: {model.SelectedRubroID}");
                model.SubRubros = await _dropdownService.GetSubRubrosAsync(model.SelectedRubroID);
                _logger.LogInformation($"SubRubros cargados: {model.SubRubros.Count()}");
            }
            else if (model.Rubros.Any())
            {
                // Si no hay rubro seleccionado pero hay rubros disponibles, seleccionar el primero
                model.SelectedRubroID = int.Parse(model.Rubros.First().Value);
                model.SubRubros = await _dropdownService.GetSubRubrosAsync(model.SelectedRubroID);
            }
            else
            {
                model.SubRubros = new List<SelectListItem>();
            }
        }
    }
}
