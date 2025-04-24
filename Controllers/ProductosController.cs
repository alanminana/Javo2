// Archivo: Controllers/ProductosController.cs con manejo de stock
using AutoMapper;
using Javo2.Controllers.Base;
using Javo2.DTOs;
using Javo2.IServices;
using Javo2.IServices.Common;
using Javo2.Models;
using Javo2.ViewModels.Operaciones.Productos;
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
                if (producto == null)
                {
                    _logger.LogWarning("Producto con ID={ID} no encontrado en Details", id);
                    return NotFound();
                }
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
            _logger.LogInformation("ProductosController: Create POST => Nombre={Nombre}", model.Nombre);

            // Asignar ModificadoPor y remover del ModelState
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
                // Validar entidades relacionadas
                var marca = await _catalogoService.GetMarcaByIDAsync(model.SelectedMarcaID);
                var rubro = await _catalogoService.GetRubroByIDAsync(model.SelectedRubroID);
                var subRubro = await _catalogoService.GetSubRubroByIDAsync(model.SelectedSubRubroID);

                if (marca == null || rubro == null || subRubro == null)
                {
                    if (marca == null) ModelState.AddModelError(string.Empty, "Marca inválida.");
                    if (rubro == null) ModelState.AddModelError(string.Empty, "Rubro inválido.");
                    if (subRubro == null) ModelState.AddModelError(string.Empty, "SubRubro inválido.");

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
                    RubroID = model.SelectedRubroID,
                    SubRubroID = model.SelectedSubRubroID,
                    MarcaID = model.SelectedMarcaID,
                    FechaMod = DateTime.Now,
                    FechaModPrecio = DateTime.Now,
                    ModificadoPor = model.ModificadoPor,
                    Estado = Producto.EstadoProducto.Activo
                };

                await _productoService.CreateProductoAsync(producto);
                _logger.LogInformation("Producto creado exitosamente con ID={ID}", producto.ProductoID);

                // Crear StockItem con el stock inicial
                if (model.StockInicial > 0)
                {
                    var stockItem = new StockItem
                    {
                        ProductoID = producto.ProductoID,
                        CantidadDisponible = model.StockInicial
                    };

                    await _stockService.CreateStockItemAsync(stockItem);

                    // Registrar movimiento de stock
                    var movimiento = new MovimientoStock
                    {
                        ProductoID = producto.ProductoID,
                        Fecha = DateTime.Now,
                        TipoMovimiento = "Entrada",
                        Cantidad = model.StockInicial,
                        Motivo = "Stock inicial al crear producto"
                    };

                    await _stockService.RegistrarMovimientoAsync(movimiento);
                }

                // Auditoría
                await _auditoriaService.RegistrarCambioAsync(new AuditoriaRegistro
                {
                    FechaHora = DateTime.Now,
                    Usuario = User.Identity?.Name ?? "Sistema",
                    Entidad = "Producto",
                    Accion = "Create",
                    LlavePrimaria = producto.ProductoID.ToString(),
                    Detalle = $"Nombre={producto.Nombre}, Rubro={rubro.Nombre}, Marca={marca.Nombre}"
                });

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear producto");
                ModelState.AddModelError(string.Empty, $"Error al crear producto: {ex.Message}");
                await PopulateDropdownsAsync(model);
                return View("Form", model);
            }
        }

        // GET: Productos/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                _logger.LogInformation("ProductosController: Edit GET con ID={ID}", id);
                var producto = await _productoService.GetProductoByIDAsync(id);
                if (producto == null)
                {
                    _logger.LogWarning("Producto ID={ID} no encontrado", id);
                    return NotFound();
                }

                var model = _mapper.Map<ProductosViewModel>(producto);

                // El stock inicial es 0 para edición (se usará para ajustes)
                model.StockInicial = 0;

                await PopulateDropdownsAsync(model);
                return View("Form", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Edit GET de Productos");
                return View("Error");
            }
        }

        // POST: Productos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductosViewModel model)
        {
            _logger.LogInformation("ProductosController: Edit POST => ProductoID={ID}", model.ProductoID);

            // Asignar ModificadoPor y remover del ModelState
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
                var existingProducto = await _productoService.GetProductoByIDAsync(model.ProductoID);
                if (existingProducto == null)
                {
                    return NotFound();
                }

                // Guardar stock actual antes de actualizar el producto
                int stockAnterior = existingProducto.StockItem?.CantidadDisponible ?? 0;

                existingProducto.Nombre = model.Nombre;
                existingProducto.Descripcion = model.Descripcion;
                existingProducto.PCosto = model.PCosto;
                existingProducto.PContado = model.PContado;
                existingProducto.PLista = model.PLista;
                existingProducto.PorcentajeIva = model.PorcentajeIva;
                existingProducto.RubroID = model.SelectedRubroID;
                existingProducto.SubRubroID = model.SelectedSubRubroID;
                existingProducto.MarcaID = model.SelectedMarcaID;
                existingProducto.FechaMod = DateTime.Now;
                existingProducto.ModificadoPor = model.ModificadoPor;

                await _productoService.UpdateProductoAsync(existingProducto);
                _logger.LogInformation("Producto ID={ID} actualizado correctamente", model.ProductoID);

                // Actualizar stock si hay un ajuste
                if (model.StockInicial != 0)
                {
                    var stockItem = await _stockService.GetStockItemByProductoIDAsync(model.ProductoID);
                    if (stockItem == null)
                    {
                        // Crear stock si no existe
                        stockItem = new StockItem
                        {
                            ProductoID = model.ProductoID,
                            CantidadDisponible = model.StockInicial > 0 ? model.StockInicial : 0 // Asegurar que no sea negativo
                        };
                        await _stockService.CreateStockItemAsync(stockItem);
                    }
                    else
                    {
                        // Actualizar stock existente
                        int nuevaCantidad = stockItem.CantidadDisponible + model.StockInicial;
                        stockItem.CantidadDisponible = nuevaCantidad >= 0 ? nuevaCantidad : 0;

                        // Si el resultado sería negativo, ajustar la cantidad del movimiento
                        if (nuevaCantidad < 0)
                        {
                            model.StockInicial = -stockAnterior; // Ajustar para el registro de movimiento
                        }

                        await _stockService.UpdateStockItemAsync(stockItem);
                    }

                    // Registrar movimiento de stock
                    var movimiento = new MovimientoStock
                    {
                        ProductoID = model.ProductoID,
                        Fecha = DateTime.Now,
                        TipoMovimiento = model.StockInicial > 0 ? "Entrada" : "Salida",
                        Cantidad = Math.Abs(model.StockInicial),
                        Motivo = $"Ajuste manual de stock - {model.ModificadoPor}"
                    };

                    await _stockService.RegistrarMovimientoAsync(movimiento);
                }

                // Auditoría
                await _auditoriaService.RegistrarCambioAsync(new AuditoriaRegistro
                {
                    FechaHora = DateTime.Now,
                    Usuario = User.Identity?.Name ?? "Sistema",
                    Entidad = "Producto",
                    Accion = "Update",
                    LlavePrimaria = existingProducto.ProductoID.ToString(),
                    Detalle = $"Nombre={existingProducto.Nombre}"
                });

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar producto");
                ModelState.AddModelError(string.Empty, $"Error al actualizar producto: {ex.Message}");
                await PopulateDropdownsAsync(model);
                return View("Form", model);
            }
        }