// Archivo: Controllers/ProductosController.cs
using AutoMapper;
using Javo2.Controllers.Base;
using Javo2.DTOs;
using Javo2.IServices;
using Javo2.IServices.Common;
using Javo2.Models;
using Javo2.ViewModels.Operaciones.Productos;
using Javo2.ViewModels.Operaciones.Stock;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Javo2.Controllers
{
    public class ProductosController : BaseController
    {
        private readonly IProductoService _productoService;
        private readonly IDropdownService _dropdownService;
        private readonly ICatalogoService _catalogoService;
        private readonly IStockService _stockService;
        private readonly IMapper _mapper;
        private readonly IAuditoriaService _auditoriaService;

        public ProductosController(
            IProductoService productoService,
            IDropdownService dropdownService,
            ICatalogoService catalogoService,
            IStockService stockService,
            IMapper mapper,
            IAuditoriaService auditoriaService,
            ILogger<ProductosController> logger
        ) : base(logger)
        {
            _productoService = productoService;
            _dropdownService = dropdownService;
            _catalogoService = catalogoService;
            _stockService = stockService;
            _mapper = mapper;
            _auditoriaService = auditoriaService;
        }

        // GET: Productos
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
        public async Task<IActionResult> Create()
        {
            try
            {
                _logger.LogInformation("ProductosController: Create GET");
                var model = new ProductosViewModel();
                model.Rubros = await _dropdownService.GetRubrosAsync();
                if (model.Rubros.Any()) model.SelectedRubroID = int.Parse(model.Rubros.First().Value);
                await PopulateDropdownsAsync(model);
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductosViewModel model)
        {
            model.ModificadoPor = User.Identity?.Name ?? "Sistema";
            ModelState.Remove(nameof(model.ModificadoPor));

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

                var producto = new Producto
                {
                    CodigoAlfa = _productoService.GenerarProductoIDAlfa(),
                    CodigoBarra = _productoService.GenerarCodBarraProducto(),
                    Nombre = model.Nombre,
                    Descripcion = model.Descripcion,
                    PCosto = model.PCosto,
                    PContado = model.PContado,
                    PLista = model.PLista,
                    PorcentajeIva = model.PorcentajeIva,
                    RubroID = rubro.ID,
                    SubRubroID = sub.ID,
                    MarcaID = marca.ID,
                    FechaMod = DateTime.Now,
                    FechaModPrecio = DateTime.Now,
                    ModificadoPor = model.ModificadoPor,
                    Estado = Producto.EstadoProducto.Activo
                };

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

        // GET: Productos/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var producto = await _productoService.GetProductoByIDAsync(id);
            if (producto == null) return NotFound();
            var model = _mapper.Map<ProductosViewModel>(producto);
            model.StockInicial = 0;
            await PopulateDropdownsAsync(model);
            return View("Form", model);
        }

        // POST: Productos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductosViewModel model)
        {
            model.ModificadoPor = User.Identity?.Name ?? "Sistema";
            ModelState.Remove(nameof(model.ModificadoPor));
            if (!ModelState.IsValid)
            {
                LogModelStateErrors();
                await PopulateDropdownsAsync(model);
                return View("Form", model);
            }
            var existing = await _productoService.GetProductoByIDAsync(model.ProductoID);
            if (existing == null) return NotFound();

            existing.Nombre = model.Nombre;
            existing.Descripcion = model.Descripcion;
            existing.PCosto = model.PCosto;
            existing.PContado = model.PContado;
            existing.PLista = model.PLista;
            existing.PorcentajeIva = model.PorcentajeIva;
            existing.RubroID = model.SelectedRubroID;
            existing.SubRubroID = model.SelectedSubRubroID;
            existing.MarcaID = model.SelectedMarcaID;
            existing.FechaMod = DateTime.Now;
            existing.ModificadoPor = model.ModificadoPor;

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

            return RedirectToAction(nameof(Index));
        }

        // GET: Productos/Delete/5
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
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _productoService.DeleteProductoAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // AJAX: Obtener SubRubros
        [HttpGet]
        public async Task<IActionResult> GetSubRubros(int rubroId)
        {
            if (rubroId <= 0) return Json(new List<SelectListItem>());
            var items = await _dropdownService.GetSubRubrosAsync(rubroId);
            return Json(items);
        }

        // POST: Productos/IncrementarPrecios
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IncrementarPrecios(string ProductoIDs, decimal porcentaje, bool isAumento)
        {
            if (string.IsNullOrEmpty(ProductoIDs)) return Json(new { success = false, message = "Seleccione productos." });
            var ids = ProductoIDs.Split(',').Select(int.Parse).ToList();
            await _productoService.AdjustPricesAsync(ids, porcentaje, isAumento);
            await _auditoriaService.RegistrarCambioAsync(new AuditoriaRegistro
            {
                FechaHora = DateTime.Now,
                Usuario = User.Identity?.Name ?? "Sistema",
                Entidad = "Producto",
                Accion = "UpdatePrices",
                LlavePrimaria = string.Join(',', ids),
                Detalle = $"{(isAumento ? "Aumento" : "Descuento")} {porcentaje}%"
            });
            return Json(new { success = true });
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
            model.Rubros ??= await _dropdownService.GetRubrosAsync();
            model.Marcas = await _dropdownService.GetMarcasAsync();
            model.SubRubros = model.SelectedRubroID > 0
                ? await _dropdownService.GetSubRubrosAsync(model.SelectedRubroID)
                : new List<SelectListItem>();
        }
    }
}
