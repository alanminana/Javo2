// Controllers/Operations/ProveedoresController.cs
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
using Microsoft.AspNetCore.Mvc.Rendering;
using Javo2.Services.Operations;

namespace Javo2.Controllers.Operations
{
    [Authorize]
    public class ProveedoresController : OperationsBaseController
    {
        private readonly IProveedorService _proveedorService;
        private readonly CompraWorkflowStateManager _workflowManager;

        public ProveedoresController(
            IProveedorService proveedorService,
            IProductoService productoService,
            IClienteService clienteService,
            IAuditoriaService auditoriaService,
            IDropdownService dropdownService,
            CompraWorkflowStateManager workflowManager,
            IMapper mapper,
            ILogger<ProveedoresController> logger)
            : base(productoService, clienteService, auditoriaService, dropdownService, mapper, logger)
        {
            _proveedorService = proveedorService;
            _workflowManager = workflowManager;
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
                LogError(ex, "Error al cargar proveedores");
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
                LogError(ex, "Error al cargar detalles del proveedor");
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

                // Usar método común de auditoría
                await RegistrarAuditoriaOperacionAsync(
                    "Proveedor",
                    "Create",
                    proveedor.ProveedorID,
                    $"Proveedor creado: {proveedor.Nombre}"
                );

                SetSuccessMessage("Proveedor creado exitosamente");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al crear proveedor");
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
                LogError(ex, "Error al cargar proveedor para editar");
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

                // Usar método común de auditoría
                await RegistrarAuditoriaOperacionAsync(
                    "Proveedor",
                    "Update",
                    proveedor.ProveedorID,
                    $"Proveedor actualizado: {proveedor.Nombre}"
                );

                SetSuccessMessage("Proveedor actualizado exitosamente");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al actualizar proveedor");
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
                LogError(ex, "Error al cargar proveedor para eliminar");
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
                var proveedor = await _proveedorService.GetProveedorByIDAsync(id);
                if (proveedor == null) return NotFound();

                string nombre = proveedor.Nombre;
                await _proveedorService.DeleteProveedorAsync(id);

                // Usar método común de auditoría
                await RegistrarAuditoriaOperacionAsync("Proveedor", "Delete", id, $"Proveedor eliminado: {nombre}");

                SetSuccessMessage("Proveedor eliminado exitosamente");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al eliminar proveedor");
                SetErrorMessage("Error al eliminar proveedor: " + ex.Message);
                return RedirectToAction(nameof(Index));
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
                LogError(ex, "Error al cargar compras");
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
                LogError(ex, "Error al cargar detalles de compra");
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

                // Usar método común para cargar opciones
                await CargarOpcionesCompraAsync(model);

                return View("FormCompra", model);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al inicializar formulario de compra");
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
                var (isValid, compra) = await PrepararCompraAsync(model);
                if (!isValid || compra == null)
                    return View("FormCompra", model);

                await _proveedorService.CreateCompraAsync(compra);

                // Usar método común de auditoría
                await RegistrarAuditoriaOperacionAsync(
                    "Compra",
                    "Create",
                    compra.CompraID,
                    $"Compra creada: {compra.NumeroFactura}, Total: {compra.PrecioTotal}"
                );

                SetSuccessMessage("Compra creada exitosamente.");
                return RedirectToAction(nameof(Compras));
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al crear compra");
                ModelState.AddModelError(string.Empty, "Ocurrió un error al crear la compra: " + ex.Message);
                await CargarOpcionesCompraAsync(model);
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

                // Usar método común para validar estado
                if (!ValidarEstadoParaEdicion(compra.Estado, new[] { EstadoCompra.Borrador, EstadoCompra.Confirmada }, "compra"))
                {
                    return RedirectToAction(nameof(DetallesCompra), new { id });
                }

                // Cargar nombre del proveedor
                var proveedor = await _proveedorService.GetProveedorByIDAsync(compra.ProveedorID);
                if (proveedor != null)
                {
                    model.NombreProveedor = proveedor.Nombre;
                }

                await CargarOpcionesCompraAsync(model);

                return View("FormCompra", model);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al cargar compra para edición");
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
                    await CargarOpcionesCompraAsync(model);
                    return View("FormCompra", model);
                }

                // Usar método común de validación
                if (!ValidarProductosEnOperacion(model.ProductosCompra, "compra"))
                {
                    await CargarOpcionesCompraAsync(model);
                    return View("FormCompra", model);
                }

                // Verificar estado de la compra
                var compraOriginal = await _proveedorService.GetCompraByIDAsync(model.CompraID);
                if (compraOriginal == null) return NotFound();

                // Usar método común para validar estado
                if (!ValidarEstadoParaEdicion(compraOriginal.Estado, new[] { EstadoCompra.Borrador, EstadoCompra.Confirmada }, "compra"))
                {
                    return RedirectToAction(nameof(DetallesCompra), new { id = model.CompraID });
                }

                // Convertir ViewModel a modelo
                var compra = _mapper.Map<CompraProveedor>(model);
                compra.TotalProductos = CalcularCantidadTotalProductos(compra.ProductosCompra, p => p.Cantidad);
                compra.PrecioTotal = CalcularTotalOperacion(compra.ProductosCompra, p => p.PrecioTotal);
                compra.Usuario = User.Identity?.Name ?? "Desconocido";
                compra.Estado = Enum.Parse<EstadoCompra>(model.Estado);

                // Actualizar compra
                await _proveedorService.UpdateCompraAsync(compra);

                // Usar método común de auditoría
                await RegistrarAuditoriaOperacionAsync(
                    "Compra",
                    "Update",
                    compra.CompraID,
                    $"Compra actualizada: {compra.NumeroFactura}, Total: {compra.PrecioTotal}"
                );

                SetSuccessMessage("Compra actualizada exitosamente.");
                return RedirectToAction(nameof(Compras));
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al actualizar compra");
                ModelState.AddModelError(string.Empty, "Ocurrió un error al actualizar la compra: " + ex.Message);
                await CargarOpcionesCompraAsync(model);
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

                // Usar método común para validar estado
                if (!ValidarEstadoParaEliminacion(compra.Estado, new[] { EstadoCompra.Borrador }, "compra"))
                {
                    return RedirectToAction(nameof(DetallesCompra), new { id });
                }

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
                LogError(ex, "Error al cargar compra para eliminación");
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
                var compra = await _proveedorService.GetCompraByIDAsync(id);
                if (compra == null) return NotFound();

                // Usar método común para validar estado
                if (!ValidarEstadoParaEliminacion(compra.Estado, new[] { EstadoCompra.Borrador }, "compra"))
                {
                    return RedirectToAction(nameof(DetallesCompra), new { id });
                }

                string numeroFactura = compra.NumeroFactura;
                await _proveedorService.DeleteCompraAsync(id);

                // Usar método común de auditoría
                await RegistrarAuditoriaOperacionAsync("Compra", "Delete", id, $"Compra eliminada: {numeroFactura}");

                SetSuccessMessage("Compra eliminada exitosamente.");
                return RedirectToAction(nameof(Compras));
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al eliminar compra");
                SetErrorMessage("Ocurrió un error al eliminar la compra: " + ex.Message);
                return RedirectToAction(nameof(Compras));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:proveedores.editar")]
        public async Task<IActionResult> ProcesarCompra(int id)
        {
            return await CambiarEstadoOperacionAsync(
                id,
                EstadoCompra.EnProceso,
                _proveedorService.GetCompraByIDAsync,
                async (compra, estado) => {
                    await _proveedorService.ProcesarCompraAsync(id);
                    return true;
                },
                "Compra"
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:proveedores.editar")]
        public async Task<IActionResult> CompletarCompra(int id)
        {
            return await CambiarEstadoOperacionAsync(
                id,
                EstadoCompra.Completada,
                _proveedorService.GetCompraByIDAsync,
                async (compra, estado) => {
                    await _proveedorService.CompletarCompraAsync(id);
                    return true;
                },
                "Compra"
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:proveedores.editar")]
        public async Task<IActionResult> CancelarCompra(int id)
        {
            return await CambiarEstadoOperacionAsync(
                id,
                EstadoCompra.Cancelada,
                _proveedorService.GetCompraByIDAsync,
                async (compra, estado) => {
                    await _proveedorService.CancelarCompraAsync(id);
                    return true;
                },
                "Compra"
            );
        }

        [HttpPost]
        [Authorize(Policy = "Permission:proveedores.ver")]
        public async Task<IActionResult> BuscarProducto(string codigoProducto)
        {
            // Usar método común de la clase base
            return await BuscarProductoPorCodigoAsync(codigoProducto);
        }

        [HttpPost]
        [Authorize(Policy = "Permission:proveedores.ver")]
        public async Task<IActionResult> SearchProducts(string term, bool forPurchase = false)
        {
            // Usar método común de la clase base
            return await BuscarProductosAsync(term, forPurchase);
        }

        [HttpPost]
        public async Task<IActionResult> Filter(string filterField, string filterValue)
        {
            return await FiltrarEntidadesAsync<Proveedor, ProveedoresViewModel>(
                _proveedorService.GetProveedoresAsync,
                (proveedores, field, value) => AplicarFiltroProveedores(proveedores, field, value),
                filterField,
                filterValue,
                "_ProveedoresTable"
            );
        }

        #endregion

        #region Métodos Auxiliares

        // Método común para validar y preparar compras
        private async Task<(bool isValid, CompraProveedor compra)> PrepararCompraAsync(CompraProveedorViewModel model)
        {
            if (!ModelState.IsValid)
            {
                LogModelStateErrors();
                await CargarOpcionesCompraAsync(model);
                return (false, null);
            }

            // Usar método común de validación
            if (!ValidarProductosEnOperacion(model.ProductosCompra, "compra"))
            {
                await CargarOpcionesCompraAsync(model);
                return (false, null);
            }

            var compra = _mapper.Map<CompraProveedor>(model);
            compra.TotalProductos = CalcularCantidadTotalProductos(compra.ProductosCompra, p => p.Cantidad);
            compra.PrecioTotal = CalcularTotalOperacion(compra.ProductosCompra, p => p.PrecioTotal);
            compra.Usuario = User.Identity?.Name ?? "Desconocido";
            compra.Estado = Enum.Parse<EstadoCompra>(model.Estado);

            return (true, compra);
        }

        // Método auxiliar para cargar opciones de compra
        private async Task CargarOpcionesCompraAsync(CompraProveedorViewModel model)
        {
            // Usar método común para cargar formas de pago
            await CargarOpcionesFormasPagoAsync(model);

            // Cargar lista de proveedores
            var proveedores = await _proveedorService.GetProveedoresAsync();
            model.Proveedores = proveedores.Select(p => new SelectListItem
            {
                Value = p.ProveedorID.ToString(),
                Text = p.Nombre
            });
        }

        private IEnumerable<Proveedor> AplicarFiltroProveedores(IEnumerable<Proveedor> proveedores, string filterField, string filterValue)
        {
            if (string.IsNullOrEmpty(filterValue))
                return proveedores;

            return filterField switch
            {
                "nombre" => proveedores.Where(p => p.Nombre.Contains(filterValue, StringComparison.OrdinalIgnoreCase)),
                "producto" => proveedores.Where(p => p.ProductosAsignadosNombres != null &&
                                                    p.ProductosAsignadosNombres.Any(n => n.Contains(filterValue, StringComparison.OrdinalIgnoreCase))),
                "marca" => proveedores.Where(p => p.ProductosAsignadosMarcas != null &&
                                                 p.ProductosAsignadosMarcas.Any(m => m.Contains(filterValue, StringComparison.OrdinalIgnoreCase))),
                _ => proveedores,
            };
        }

        #endregion
    }
}