// Controllers/ProveedoresController.cs
using AutoMapper;
using Javo2.Controllers.Base;
using Javo2.IServices;
using Javo2.IServices.Common;
using Javo2.Models;
using Javo2.ViewModels.Operaciones.Proveedores;
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
    [Authorize]
    public class ProveedoresController : BaseController
    {
        private readonly IProveedorService _proveedorService;
        private readonly IDropdownService _dropdownService;
        private readonly IProductoService _productoService;
        private readonly IStockService _stockService;
        private readonly IVentaService _ventaService;
        private readonly IMapper _mapper;

        public ProveedoresController(
            IProveedorService proveedorService,
            IDropdownService dropdownService,
            IProductoService productoService,
            IStockService stockService,
            IVentaService ventaService,
            IMapper mapper,
            ILogger<ProveedoresController> logger)
            : base(logger)
        {
            _proveedorService = proveedorService;
            _dropdownService = dropdownService;
            _productoService = productoService;
            _stockService = stockService;
            _ventaService = ventaService;
            _mapper = mapper;
        }

        #region Proveedores CRUD

        [Authorize(Policy = "Permission:proveedores.ver")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var proveedores = await _proveedorService.GetProveedoresAsync();
                var vm = _mapper.Map<IEnumerable<ProveedoresViewModel>>(proveedores);
                await PopulateProductosAsignadosInfo(vm);
                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Index");
                return View("Error");
            }
        }

        [HttpGet]
        [Authorize(Policy = "Permission:proveedores.ver")]
        public async Task<IActionResult> Filter(string filterField, string filterValue)
        {
            try
            {
                var proveedores = await _proveedorService.GetProveedoresAsync();
                var vm = _mapper.Map<IEnumerable<ProveedoresViewModel>>(proveedores);
                await PopulateProductosAsignadosInfo(vm);

                if (!string.IsNullOrEmpty(filterValue))
                    vm = ApplyFilter(vm, filterField, filterValue);

                return PartialView("_ProveedoresTable", vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Filter");
                return PartialView("_ProveedoresTable", Array.Empty<ProveedoresViewModel>());
            }
        }

        [Authorize(Policy = "Permission:proveedores.crear")]
        public async Task<IActionResult> Create()
        {
            try
            {
                var vm = await InitializeProveedorViewModelAsync();
                return View("Form", vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Create GET");
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:proveedores.crear")]
        public async Task<IActionResult> Create(ProveedoresViewModel vm)
        {
            await PopulateDropDownListsAsync(vm);
            if (!ModelState.IsValid)
            {
                LogModelStateErrors();
                return View("Form", vm);
            }

            try
            {
                var entidad = _mapper.Map<Proveedor>(vm);
                await _proveedorService.CreateProveedorAsync(entidad);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando proveedor");
                ModelState.AddModelError(string.Empty, "Ocurrió un error al crear el proveedor.");
                return View("Form", vm);
            }
        }

        [Authorize(Policy = "Permission:proveedores.editar")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var entidad = await _proveedorService.GetProveedorByIDAsync(id);
                if (entidad == null) return NotFound();

                var vm = _mapper.Map<ProveedoresViewModel>(entidad);
                await PopulateProveedorViewModelAsync(vm, entidad);
                return View("Form", vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Edit GET");
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:proveedores.editar")]
        public async Task<IActionResult> Edit(ProveedoresViewModel vm)
        {
            await PopulateDropDownListsAsync(vm);
            if (!ModelState.IsValid)
            {
                LogModelStateErrors();
                return View("Form", vm);
            }

            try
            {
                var entidad = _mapper.Map<Proveedor>(vm);
                await _proveedorService.UpdateProveedorAsync(entidad);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando proveedor");
                ModelState.AddModelError(string.Empty, "Ocurrió un error al actualizar el proveedor.");
                return View("Form", vm);
            }
        }

        [Authorize(Policy = "Permission:proveedores.ver")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var entidad = await _proveedorService.GetProveedorByIDAsync(id);
                if (entidad == null) return NotFound();

                var vm = _mapper.Map<ProveedoresViewModel>(entidad);
                await PopulateProveedorViewModelAsync(vm, entidad);
                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Details");
                return View("Error");
            }
        }

        [Authorize(Policy = "Permission:proveedores.eliminar")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var entidad = await _proveedorService.GetProveedorByIDAsync(id);
                if (entidad == null) return NotFound();

                var vm = _mapper.Map<ProveedoresViewModel>(entidad);
                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Delete GET");
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
                _logger.LogError(ex, "Error eliminando proveedor");
                ModelState.AddModelError(string.Empty, "Ocurrió un error al eliminar el proveedor.");
                var entidad = await _proveedorService.GetProveedorByIDAsync(id);
                var vm = _mapper.Map<ProveedoresViewModel>(entidad);
                return View(vm);
            }
        }

        [HttpGet]
        public async Task<IActionResult> SearchProducts(string term)
        {
            try
            {
                var productos = await _productoService.GetProductosByTermAsync(term);
                var result = productos.Select(p => new {
                    label = $"{p.Nombre} - {p.Marca?.Nombre ?? "Sin Marca"} {p.SubRubro?.Nombre ?? "Sin SubRubro"}",
                    value = p.ProductoID
                });
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en SearchProducts");
                return Json(Array.Empty<object>());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarCompra(int proveedorID, int ProductoID, int cantidad)
        {
            try
            {
                await _proveedorService.RegistrarCompraAsync(proveedorID, ProductoID, cantidad);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registrando compra rápida");
                ModelState.AddModelError(string.Empty, "Ocurrió un error al registrar la compra.");
            }
            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region Compras de Proveedor

        [HttpGet]
        [Authorize(Policy = "Permission:proveedores.ver")]
        public async Task<IActionResult> Compras()
        {
            try
            {
                var compras = await _proveedorService.GetComprasAsync();
                var vm = _mapper.Map<IEnumerable<CompraProveedorViewModel>>(compras);
                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Compras");
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

                var vm = _mapper.Map<CompraProveedorViewModel>(compra);
                var prov = await _proveedorService.GetProveedorByIDAsync(compra.ProveedorID);
                vm.NombreProveedor = prov?.Nombre;
                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en DetallesCompra");
                return View("Error");
            }
        }

        [HttpGet]
        [Authorize(Policy = "Permission:proveedores.crear")]
        public async Task<IActionResult> CrearCompra(int? proveedorId)
        {
            try
            {
                var vm = new CompraProveedorViewModel
                {
                    FechaCompra = DateTime.Now,
                    NumeroFactura = await _proveedorService.GenerarNumeroFacturaCompraAsync(),
                    Usuario = User.Identity?.Name ?? "Desconocido"
                };
                if (proveedorId.HasValue)
                {
                    var p = await _proveedorService.GetProveedorByIDAsync(proveedorId.Value);
                    if (p != null)
                    {
                        vm.ProveedorID = p.ProveedorID;
                        vm.NombreProveedor = p.Nombre;
                    }
                }
                await CargarOpcionesParaCompraAsync(vm);
                return View("FormCompra", vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en CrearCompra GET");
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:proveedores.crear")]
        public async Task<IActionResult> CrearCompra(CompraProveedorViewModel vm)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    LogModelStateErrors();
                    await CargarOpcionesParaCompraAsync(vm);
                    return View("FormCompra", vm);
                }
                if (vm.ProductosCompra == null || !vm.ProductosCompra.Any())
                {
                    ModelState.AddModelError("", "Debe agregar al menos un producto.");
                    await CargarOpcionesParaCompraAsync(vm);
                    return View("FormCompra", vm);
                }

                var entidad = _mapper.Map<CompraProveedor>(vm);
                entidad.TotalProductos = entidad.ProductosCompra.Sum(d => d.Cantidad);
                entidad.PrecioTotal = entidad.ProductosCompra.Sum(d => d.PrecioTotal);
                entidad.Usuario = User.Identity?.Name ?? "Desconocido";
                entidad.Estado = Enum.Parse<EstadoCompra>(vm.Estado);

                await _proveedorService.CreateCompraAsync(entidad);
                TempData["Success"] = "Compra creada.";
                return RedirectToAction(nameof(Compras));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en CrearCompra POST");
                ModelState.AddModelError(string.Empty, "Ocurrió un error: " + ex.Message);
                await CargarOpcionesParaCompraAsync(vm);
                return View("FormCompra", vm);
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
                if (compra.Estado == EstadoCompra.Completada || compra.Estado == EstadoCompra.Cancelada)
                {
                    TempData["Error"] = "No se puede editar.";
                    return RedirectToAction(nameof(DetallesCompra), new { id });
                }

                var vm = _mapper.Map<CompraProveedorViewModel>(compra);
                var p = await _proveedorService.GetProveedorByIDAsync(compra.ProveedorID);
                vm.NombreProveedor = p?.Nombre;
                await CargarOpcionesParaCompraAsync(vm);
                return View("FormCompra", vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en EditarCompra GET");
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:proveedores.editar")]
        public async Task<IActionResult> EditarCompra(CompraProveedorViewModel vm)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    LogModelStateErrors();
                    await CargarOpcionesParaCompraAsync(vm);
                    return View("FormCompra", vm);
                }
                if (vm.ProductosCompra == null || !vm.ProductosCompra.Any())
                {
                    ModelState.AddModelError("", "Debe agregar al menos un producto.");
                    await CargarOpcionesParaCompraAsync(vm);
                    return View("FormCompra", vm);
                }

                var orig = await _proveedorService.GetCompraByIDAsync(vm.CompraID);
                if (orig == null) return NotFound();
                if (orig.Estado == EstadoCompra.Completada || orig.Estado == EstadoCompra.Cancelada)
                {
                    TempData["Error"] = "No se puede editar.";
                    return RedirectToAction(nameof(DetallesCompra), new { id = vm.CompraID });
                }

                var entidad = _mapper.Map<CompraProveedor>(vm);
                entidad.TotalProductos = entidad.ProductosCompra.Sum(d => d.Cantidad);
                entidad.PrecioTotal = entidad.ProductosCompra.Sum(d => d.PrecioTotal);
                entidad.Usuario = User.Identity?.Name ?? "Desconocido";
                entidad.Estado = Enum.Parse<EstadoCompra>(vm.Estado);

                await _proveedorService.UpdateCompraAsync(entidad);
                TempData["Success"] = "Compra actualizada.";
                return RedirectToAction(nameof(Compras));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en EditarCompra POST");
                ModelState.AddModelError(string.Empty, "Ocurrió un error: " + ex.Message);
                await CargarOpcionesParaCompraAsync(vm);
                return View("FormCompra", vm);
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

                var vm = _mapper.Map<CompraProveedorViewModel>(compra);
                var p = await _proveedorService.GetProveedorByIDAsync(compra.ProveedorID);
                vm.NombreProveedor = p?.Nombre;
                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en EliminarCompra GET");
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
                TempData["Success"] = "Compra eliminada.";
                return RedirectToAction(nameof(Compras));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en EliminarCompra POST");
                TempData["Error"] = "Ocurrió un error: " + ex.Message;
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
                TempData["Success"] = "Compra procesada.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando compra");
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(DetallesCompra), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:proveedores.editar")]
        public async Task<IActionResult> CompletarCompra(int id)
        {
            try
            {
                await _proveedorService.CompletarCompraAsync(id);
                TempData["Success"] = "Compra completada.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completando compra");
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(DetallesCompra), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:proveedores.editar")]
        public async Task<IActionResult> CancelarCompra(int id)
        {
            try
            {
                await _proveedorService.CancelarCompraAsync(id);
                TempData["Success"] = "Compra cancelada.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelando compra");
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(DetallesCompra), new { id });
        }

        [HttpPost]
        [Authorize(Policy = "Permission:proveedores.ver")]
        public async Task<IActionResult> BuscarProducto(string codigoProducto)
        {
            try
            {
                var p = await _productoService.GetProductoByCodigoAsync(codigoProducto);
                if (p == null) return Json(new { success = false, message = "Producto no encontrado." });

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        productoID = p.ProductoID,
                        codigoBarra = p.CodigoBarra,
                        codigoAlfa = p.CodigoAlfa,
                        nombreProducto = p.Nombre,
                        marca = p.Marca?.Nombre ?? "",
                        cantidad = 1,
                        precioUnitario = p.PCosto,
                        precioTotal = p.PCosto
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en BuscarProducto");
                return Json(new { success = false, message = "Error al buscar el producto." });
            }
        }

        #endregion

        #region Helpers

        private async Task PopulateDropDownListsAsync(ProveedoresViewModel vm)
            => vm.ProductosDisponibles = await _dropdownService.GetProductosAsync();

        private async Task<ProveedoresViewModel> InitializeProveedorViewModelAsync()
            => new ProveedoresViewModel
            {
                ProductosDisponibles = await _dropdownService.GetProductosAsync()
            };

        private async Task PopulateProductosAsignadosInfo(IEnumerable<ProveedoresViewModel> list)
        {
            foreach (var vm in list)
            {
                vm.ProductosAsignadosNombres = new List<string>();
                vm.ProductosAsignadosStocks = new List<int>();
                foreach (var pid in vm.ProductosAsignados)
                {
                    var prod = await _productoService.GetProductoByIDAsync(pid);
                    vm.ProductosAsignadosNombres.Add(prod?.Nombre ?? "‑");
                    var stk = await _stockService.GetStockItemByProductoIDAsync(pid);
                    vm.ProductosAsignadosStocks.Add(stk?.CantidadDisponible ?? 0);
                }
            }
        }

        private async Task PopulateProveedorViewModelAsync(ProveedoresViewModel vm, Proveedor prov)
        {
            vm.ProductosAsignadosNombres = new List<string>();
            foreach (var pid in prov.ProductosAsignados)
            {
                var prod = await _productoService.GetProductoByIDAsync(pid);
                vm.ProductosAsignadosNombres.Add(prod?.Nombre ?? "‑");
            }
            await PopulateDropDownListsAsync(vm);
            await PopulateProductosAsignadosStocks(new[] { vm });
        }

        private async Task PopulateProductosAsignadosStocks(IEnumerable<ProveedoresViewModel> list)
        {
            foreach (var vm in list)
            {
                vm.ProductosAsignadosStocks = new List<int>();
                foreach (var pid in vm.ProductosAsignados)
                {
                    var stk = await _stockService.GetStockItemByProductoIDAsync(pid);
                    vm.ProductosAsignadosStocks.Add(stk?.CantidadDisponible ?? 0);
                }
            }
        }

        private IEnumerable<ProveedoresViewModel> ApplyFilter(IEnumerable<ProveedoresViewModel> items, string field, string value)
        {
            return field switch
            {
                "nombre" => items.Where(x => x.Nombre.Contains(value, StringComparison.OrdinalIgnoreCase)),
                "producto" => items.Where(x => x.ProductosAsignadosNombres.Any(n => n.Contains(value, StringComparison.OrdinalIgnoreCase))),
                "marca" => items.Where(x => x.ProductosAsignadosMarcas.Any(m => m.Contains(value, StringComparison.OrdinalIgnoreCase))),
                "submarca" => items.Where(x => x.ProductosAsignadosSubMarcas.Any(sm => sm.Contains(value, StringComparison.OrdinalIgnoreCase))),
                _ => items
            };
        }

        private async Task CargarOpcionesParaCompraAsync(CompraProveedorViewModel vm)
        {
            // Formas de pago, bancos, tarjetas...
            vm.FormasPago = _ventaService.GetFormasPagoSelectList();
            vm.Bancos = _ventaService.GetBancosSelectList();
            vm.TipoTarjetaOptions = _ventaService.GetTipoTarjetaSelectList();
            vm.CuotasOptions = _ventaService.GetCuotasSelectList();
            vm.EntidadesElectronicas = _ventaService.GetEntidadesElectronicasSelectList();

            // Lista de proveedores
            var provs = await _proveedorService.GetProveedoresAsync();
            vm.Proveedores = provs.Select(p => new SelectListItem
            {
                Value = p.ProveedorID.ToString(),
                Text = p.Nombre
            });
        }

        #endregion
    }
}
