// Controllers/ProveedoresController.cs
using Microsoft.AspNetCore.Mvc;
using Javo2.IServices;
using Javo2.IServices.Common;
using Javo2.Models;
using Javo2.ViewModels.Operaciones.Proveedores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Javo2.Controllers.Base;
using Microsoft.AspNetCore.Mvc.Rendering;
using Javo2.IServices.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Javo2.Controllers
{
    [Authorize]
    public class ProveedoresController : BaseController
    {
        private readonly ILogger<ProveedoresController> _logger;
        private readonly IProveedorService _proveedorService;
        private readonly IProductoService _productoService;
        private readonly IMapper _mapper;
        private readonly IDropdownService _dropdownService;

        public ProveedoresController(
            ILogger<ProveedoresController> logger,
            IPermissionManagerService permissionManager,
            IProveedorService proveedorService,
            IProductoService productoService,
            IMapper mapper,
            IDropdownService dropdownService)
            : base(logger, permissionManager)
        {
            _logger = logger;
            _proveedorService = proveedorService;
            _productoService = productoService;
            _mapper = mapper;
            _dropdownService = dropdownService;
        }

        #region Métodos de Proveedores

        [HttpGet]
        [Authorize(Policy = "Permission:proveedores.ver")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var proveedores = await _proveedorService.GetProveedoresAsync();
                var viewModels = _mapper.Map<IEnumerable<ProveedoresViewModel>>(proveedores);
                return View(viewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar proveedores");
                return View("Error");
            }
        }

        [HttpGet]
        [Authorize(Policy = "Permission:proveedores.ver")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var proveedor = await _proveedorService.GetProveedorByIDAsync(id);
                if (proveedor == null) return NotFound();

                var viewModel = _mapper.Map<ProveedoresViewModel>(proveedor);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar detalles del proveedor");
                return View("Error");
            }
        }

        [HttpGet]
        [Authorize(Policy = "Permission:proveedores.crear")]
        public IActionResult Create()
        {
            var viewModel = new ProveedoresViewModel();
            return View("Form", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:proveedores.crear")]
        public async Task<IActionResult> Create(ProveedoresViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                LogModelStateErrors();
                return View("Form", viewModel);
            }

            try
            {
                var proveedor = _mapper.Map<Proveedor>(viewModel);
                await _proveedorService.CreateProveedorAsync(proveedor);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear proveedor");
                ModelState.AddModelError(string.Empty, $"Error al crear el proveedor: {ex.Message}");
                return View("Form", viewModel);
            }
        }

        [HttpGet]
        [Authorize(Policy = "Permission:proveedores.editar")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var proveedor = await _proveedorService.GetProveedorByIDAsync(id);
                if (proveedor == null) return NotFound();

                var viewModel = _mapper.Map<ProveedoresViewModel>(proveedor);
                return View("Form", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar proveedor para editar");
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:proveedores.editar")]
        public async Task<IActionResult> Edit(ProveedoresViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                LogModelStateErrors();
                return View("Form", viewModel);
            }

            try
            {
                var proveedor = _mapper.Map<Proveedor>(viewModel);
                await _proveedorService.UpdateProveedorAsync(proveedor);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar proveedor");
                ModelState.AddModelError(string.Empty, $"Error al actualizar el proveedor: {ex.Message}");
                return View("Form", viewModel);
            }
        }

        [HttpGet]
        [Authorize(Policy = "Permission:proveedores.eliminar")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var proveedor = await _proveedorService.GetProveedorByIDAsync(id);
                if (proveedor == null) return NotFound();

                var viewModel = _mapper.Map<ProveedoresViewModel>(proveedor);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar proveedor para eliminar");
                return View("Error");
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:proveedores.eliminar")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _proveedorService.DeleteProveedorAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar proveedor");
                return View("Error");
            }
        }

        #endregion

        #region Métodos para Compras

        [HttpGet]
        [Authorize(Policy = "Permission:proveedores.ver")]
        public async Task<IActionResult> Compras()
        {
            try
            {
                var compras = await _proveedorService.GetComprasAsync();
                var model = _mapper.Map<IEnumerable<CompraProveedorViewModel>>(compras);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar compras");
                return View("Error");
            }
        }

        [HttpGet]
        [Authorize(Policy = "Permission:proveedores.ver")]
        public async Task<IActionResult> DetallesCompra(int id)
        {
            try
            {
                var compra = await _proveedorService.GetCompraByIDAsync(id);
                if (compra == null) return NotFound();

                var model = _mapper.Map<CompraProveedorViewModel>(compra);

                // Cargar nombre del proveedor
                var proveedor = await _proveedorService.GetProveedorByIDAsync(compra.ProveedorID);
                if (proveedor != null)
                {
                    model.NombreProveedor = proveedor.Nombre;
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar detalles de compra");
                return View("Error");
            }
        }

        [HttpGet]
        [Authorize(Policy = "Permission:proveedores.crear")]
        public async Task<IActionResult> CrearCompra(int? proveedorId)
        {
            try
            {
                var model = new CompraProveedorViewModel
                {
                    FechaCompra = DateTime.Now,
                    NumeroFactura = await _proveedorService.GenerarNumeroFacturaCompraAsync(),
                    Usuario = User.Identity?.Name ?? "Desconocido"
                };

                if (proveedorId.HasValue)
                {
                    var proveedor = await _proveedorService.GetProveedorByIDAsync(proveedorId.Value);
                    if (proveedor != null)
                    {
                        model.ProveedorID = proveedor.ProveedorID;
                        model.NombreProveedor = proveedor.Nombre;
                    }
                }

                // Cargar listas de opciones
                await CargarOpcionesParaCompraAsync(model);

                return View("FormCompra", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al inicializar formulario de compra");
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:proveedores.crear")]
        public async Task<IActionResult> CrearCompra(CompraProveedorViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    LogModelStateErrors();
                    await CargarOpcionesParaCompraAsync(model);
                    return View("FormCompra", model);
                }

                // Validar que haya productos
                if (model.ProductosCompra == null || !model.ProductosCompra.Any())
                {
                    ModelState.AddModelError("", "Debe agregar al menos un producto a la compra");
                    await CargarOpcionesParaCompraAsync(model);
                    return View("FormCompra", model);
                }

                // Convertir ViewModel a modelo
                var compra = _mapper.Map<CompraProveedor>(model);
                compra.TotalProductos = compra.ProductosCompra.Sum(p => p.Cantidad);
                compra.PrecioTotal = compra.ProductosCompra.Sum(p => p.PrecioTotal);
                compra.Usuario = User.Identity?.Name ?? "Desconocido";
                compra.Estado = Enum.Parse<EstadoCompra>(model.Estado);

                // Crear compra
                await _proveedorService.CreateCompraAsync(compra);

                TempData["Success"] = "Compra creada exitosamente.";
                return RedirectToAction(nameof(Compras));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear compra");
                ModelState.AddModelError(string.Empty, "Ocurrió un error al crear la compra: " + ex.Message);
                await CargarOpcionesParaCompraAsync(model);
                return View("FormCompra", model);
            }
        }

        [HttpGet]
        [Authorize(Policy = "Permission:proveedores.editar")]
        public async Task<IActionResult> EditarCompra(int id)
        {
            try
            {
                var compra = await _proveedorService.GetCompraByIDAsync(id);
                if (compra == null) return NotFound();

                var model = _mapper.Map<CompraProveedorViewModel>(compra);

                // Verificar si la compra está en estado Completada o Cancelada
                if (compra.Estado == EstadoCompra.Completada || compra.Estado == EstadoCompra.Cancelada)
                {
                    TempData["Error"] = "No se puede editar una compra completada o cancelada.";
                    return RedirectToAction(nameof(DetallesCompra), new { id });
                }

                // Cargar nombre del proveedor
                var proveedor = await _proveedorService.GetProveedorByIDAsync(compra.ProveedorID);
                if (proveedor != null)
                {
                    model.NombreProveedor = proveedor.Nombre;
                }

                await CargarOpcionesParaCompraAsync(model);

                return View("FormCompra", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar compra para edición");
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:proveedores.editar")]
        public async Task<IActionResult> EditarCompra(CompraProveedorViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    LogModelStateErrors();
                    await CargarOpcionesParaCompraAsync(model);
                    return View("FormCompra", model);
                }

                // Validar que haya productos
                if (model.ProductosCompra == null || !model.ProductosCompra.Any())
                {
                    ModelState.AddModelError("", "Debe agregar al menos un producto a la compra");
                    await CargarOpcionesParaCompraAsync(model);
                    return View("FormCompra", model);
                }

                // Verificar estado de la compra
                var compraOriginal = await _proveedorService.GetCompraByIDAsync(model.CompraID);
                if (compraOriginal == null) return NotFound();

                if (compraOriginal.Estado == EstadoCompra.Completada || compraOriginal.Estado == EstadoCompra.Cancelada)
                {
                    TempData["Error"] = "No se puede editar una compra completada o cancelada.";
                    return RedirectToAction(nameof(DetallesCompra), new { id = model.CompraID });
                }

                // Convertir ViewModel a modelo
                var compra = _mapper.Map<CompraProveedor>(model);
                compra.TotalProductos = compra.ProductosCompra.Sum(p => p.Cantidad);
                compra.PrecioTotal = compra.ProductosCompra.Sum(p => p.PrecioTotal);
                compra.Usuario = User.Identity?.Name ?? "Desconocido";
                compra.Estado = Enum.Parse<EstadoCompra>(model.Estado);

                // Actualizar compra
                await _proveedorService.UpdateCompraAsync(compra);

                TempData["Success"] = "Compra actualizada exitosamente.";
                return RedirectToAction(nameof(Compras));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar compra");
                ModelState.AddModelError(string.Empty, "Ocurrió un error al actualizar la compra: " + ex.Message);
                await CargarOpcionesParaCompraAsync(model);
                return View("FormCompra", model);
            }
        }

        [HttpGet]
        [Authorize(Policy = "Permission:proveedores.eliminar")]
        public async Task<IActionResult> EliminarCompra(int id)
        {
            try
            {
                var compra = await _proveedorService.GetCompraByIDAsync(id);
                if (compra == null) return NotFound();

                var model = _mapper.Map<CompraProveedorViewModel>(compra);

                // Cargar nombre del proveedor
                var proveedor = await _proveedorService.GetProveedorByIDAsync(compra.ProveedorID);
                if (proveedor != null)
                {
                    model.NombreProveedor = proveedor.Nombre;
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar compra para eliminación");
                return View("Error");
            }
        }

        [HttpPost, ActionName("EliminarCompra")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:proveedores.eliminar")]
        public async Task<IActionResult> EliminarCompraConfirmado(int id)
        {
            try
            {
                await _proveedorService.DeleteCompraAsync(id);
                TempData["Success"] = "Compra eliminada exitosamente.";
                return RedirectToAction(nameof(Compras));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar compra");
                TempData["Error"] = "Ocurrió un error al eliminar la compra: " + ex.Message;
                return RedirectToAction(nameof(Compras));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:proveedores.editar")]
        public async Task<IActionResult> ProcesarCompra(int id)
        {
            try
            {
                await _proveedorService.ProcesarCompraAsync(id);
                TempData["Success"] = "Compra procesada exitosamente.";
                return RedirectToAction(nameof(DetallesCompra), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar compra");
                TempData["Error"] = "Ocurrió un error al procesar la compra: " + ex.Message;
                return RedirectToAction(nameof(DetallesCompra), new { id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:proveedores.editar")]
        public async Task<IActionResult> CompletarCompra(int id)
        {
            try
            {
                await _proveedorService.CompletarCompraAsync(id);
                TempData["Success"] = "Compra completada exitosamente.";
                return RedirectToAction(nameof(DetallesCompra), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al completar compra");
                TempData["Error"] = "Ocurrió un error al completar la compra: " + ex.Message;
                return RedirectToAction(nameof(DetallesCompra), new { id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:proveedores.editar")]
        public async Task<IActionResult> CancelarCompra(int id)
        {
            try
            {
                await _proveedorService.CancelarCompraAsync(id);
                TempData["Success"] = "Compra cancelada exitosamente.";
                return RedirectToAction(nameof(DetallesCompra), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cancelar compra");
                TempData["Error"] = "Ocurrió un error al cancelar la compra: " + ex.Message;
                return RedirectToAction(nameof(DetallesCompra), new { id });
            }
        }

        [HttpPost]
        [Authorize(Policy = "Permission:proveedores.ver")]
        public async Task<IActionResult> BuscarProducto(string codigoProducto)
        {
            try
            {
                var producto = await _productoService.GetProductoByCodigoAsync(codigoProducto);
                if (producto == null)
                    return Json(new { success = false, message = "Producto no encontrado." });

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        productoID = producto.ProductoID,
                        codigoBarra = producto.CodigoBarra,
                        codigoAlfa = producto.CodigoAlfa,
                        nombreProducto = producto.Nombre,
                        marca = producto.Marca?.Nombre ?? "",
                        cantidad = 1,
                        precioUnitario = producto.PCosto,
                        precioTotal = producto.PCosto
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar producto");
                return Json(new { success = false, message = "Error al buscar el producto." });
            }
        }

        #endregion
        [HttpPost]
        [Authorize(Policy = "Permission:proveedores.editar")]
        public async Task<IActionResult> SearchProductsForPurchase(string term)
        {
            try
            {
                if (string.IsNullOrEmpty(term) || term.Length < 2)
                    return Json(new List<object>());

                // Buscar productos que coincidan con el término
                var productos = await _productoService.GetAllProductosAsync();
                var filteredProducts = productos
                    .Where(p =>
                        p.Nombre.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                        p.CodigoAlfa.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                        p.CodigoBarra.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                        (p.Marca != null && p.Marca.Nombre.Contains(term, StringComparison.OrdinalIgnoreCase)))
                    .Take(20)
                    .Select(p => new
                    {
                        id = p.ProductoID,
                        name = p.Nombre,
                        codigo = p.CodigoAlfa,
                        marca = p.Marca?.Nombre ?? "Sin marca",
                        precio = p.PCosto
                    });

                return Json(filteredProducts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar productos para compra");
                return Json(new List<object>());
            }
        }
        #region Métodos de ayuda

        [HttpPost]
        public async Task<IActionResult> SearchProducts(string term)
        {
            try
            {
                // Vamos a usar directamente el método GetProductoByCodigoAsync, 
                // ya que parece no existir otro método específico de búsqueda
                var producto = await _productoService.GetProductoByCodigoAsync(term);

                var results = new List<object>();
                if (producto != null)
                {
                    results.Add(new
                    {
                        label = producto.Nombre,
                        value = producto.ProductoID
                    });
                }

                return Json(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar productos");
                return Json(new List<object>());
            }
        }

        // Método auxiliar para cargar opciones de compra
        private async Task CargarOpcionesParaCompraAsync(CompraProveedorViewModel model)
        {
            // Formas de pago (usamos lista estática para mantener la simplicidad)
            model.FormasPago = new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "Contado" },
                new SelectListItem { Value = "2", Text = "Tarjeta de Crédito" },
                new SelectListItem { Value = "3", Text = "Tarjeta de Débito" },
                new SelectListItem { Value = "4", Text = "Transferencia" },
                new SelectListItem { Value = "5", Text = "Pago Virtual" },
                new SelectListItem { Value = "6", Text = "Crédito Personal" }
            };

            // Bancos (usamos lista estática porque parece que IDropdownService no tiene GetBancosAsync)
            model.Bancos = new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "Banco Nación" },
                new SelectListItem { Value = "2", Text = "Banco Provincia" },
                new SelectListItem { Value = "3", Text = "Banco Santander" },
                new SelectListItem { Value = "4", Text = "Banco Galicia" },
                new SelectListItem { Value = "5", Text = "BBVA" },
                new SelectListItem { Value = "6", Text = "HSBC" }
            };

            // Tipos de tarjeta
            model.TipoTarjetaOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "Visa", Text = "Visa" },
                new SelectListItem { Value = "MasterCard", Text = "MasterCard" },
                new SelectListItem { Value = "American Express", Text = "American Express" },
                new SelectListItem { Value = "Naranja", Text = "Naranja" }
            };

            // Cuotas
            model.CuotasOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "1 cuota" },
                new SelectListItem { Value = "3", Text = "3 cuotas" },
                new SelectListItem { Value = "6", Text = "6 cuotas" },
                new SelectListItem { Value = "12", Text = "12 cuotas" }
            };

            // Entidades electrónicas
            model.EntidadesElectronicas = new List<SelectListItem>
            {
                new SelectListItem { Value = "MercadoPago", Text = "MercadoPago" },
                new SelectListItem { Value = "Todo Pago", Text = "Todo Pago" },
                new SelectListItem { Value = "PayPal", Text = "PayPal" }
            };

            // Cargar lista de proveedores
            var proveedores = await _proveedorService.GetProveedoresAsync();
            model.Proveedores = proveedores.Select(p => new SelectListItem
            {
                Value = p.ProveedorID.ToString(),
                Text = p.Nombre
            });
        }

        [HttpPost]
        public async Task<IActionResult> Filter(string filterField, string filterValue)
        {
            try
            {
                var proveedores = await _proveedorService.GetProveedoresAsync();
                var viewModels = _mapper.Map<IEnumerable<ProveedoresViewModel>>(proveedores);

                if (!string.IsNullOrEmpty(filterValue))
                {
                    viewModels = filterField switch
                    {
                        "nombre" => viewModels.Where(p => p.Nombre.Contains(filterValue, StringComparison.OrdinalIgnoreCase)),
                        "producto" => viewModels.Where(p => p.ProductosAsignadosNombres != null &&
                                                          p.ProductosAsignadosNombres.Any(n => n.Contains(filterValue, StringComparison.OrdinalIgnoreCase))),
                        "marca" => viewModels.Where(p => p.ProductosAsignadosMarcas != null &&
                                                       p.ProductosAsignadosMarcas.Any(m => m.Contains(filterValue, StringComparison.OrdinalIgnoreCase))),
                        _ => viewModels,
                    };
                }

                return PartialView("_ProveedoresTable", viewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al filtrar proveedores");
                return PartialView("_ProveedoresTable", new List<ProveedoresViewModel>());
            }
        }

        #endregion
    }
}