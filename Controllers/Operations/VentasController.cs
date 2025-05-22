// Controllers/Operations/VentasController.cs
using AutoMapper;
using Javo2.Helpers;
using Javo2.IServices;
using Javo2.IServices.Common;
using Javo2.Models;
using Javo2.Services.Catalog;
using Javo2.Services.Operations;
using Javo2.ViewModels.Operaciones.DevolucionGarantia;
using Javo2.ViewModels.Operaciones.Ventas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Javo2.Controllers.Operations
{
    [Authorize]
    public class VentasController : OperationsBaseController
    {
        private readonly IVentaService _ventaService;
        private readonly ICotizacionService _cotizacionService;
        private readonly IDevolucionGarantiaService _devolucionService;
        private readonly ICreditoService _creditoService;
        private readonly IProductSearchService _productSearchService;
        private readonly VentaWorkflowStateManager _workflowManager;

        public VentasController(
            IVentaService ventaService,
            ICotizacionService cotizacionService,
            IProductoService productoService,
            IClienteService clienteService,
            IAuditoriaService auditoriaService,
            IDevolucionGarantiaService devolucionService,
            ICreditoService creditoService,
            IProductSearchService productSearchService,
            IDropdownService dropdownService,
            VentaWorkflowStateManager workflowManager,
            IMapper mapper,
            ILogger<VentasController> logger
      ) : base(productoService, clienteService, auditoriaService, dropdownService, mapper, logger)
        {
            _ventaService = ventaService;
            _cotizacionService = cotizacionService;
            _devolucionService = devolucionService;
            _creditoService = creditoService;
            _productSearchService = productSearchService;
            _workflowManager = workflowManager;
        }

        #region Ventas

        // GET: Ventas/Index
        public async Task<IActionResult> Index(VentaFilterDto filter, string activeTab = "ventas")
        {
            try
            {
                var viewModel = new VentasIndexViewModel();

                // Cargar ventas
                var ventas = await _ventaService.GetVentasAsync(filter);
                viewModel.Ventas = _mapper.Map<IEnumerable<VentaListViewModel>>(ventas);

                // Cargar cotizaciones
                var cotizaciones = await _cotizacionService.GetAllCotizacionesAsync();
                viewModel.Cotizaciones = _mapper.Map<IEnumerable<CotizacionListViewModel>>(cotizaciones);

                // Cargar devoluciones
                var devoluciones = await _devolucionService.GetAllAsync();
                viewModel.Devoluciones = _mapper.Map<IEnumerable<DevolucionGarantiaListViewModel>>(devoluciones);

                ViewBag.ActiveTab = activeTab;

                return View(viewModel);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al obtener la lista de ventas y cotizaciones");
                return View("Error");
            }
        }

        // GET: Ventas/Create
        [HttpGet]
        [Authorize(Policy = "Permission:ventas.crear")]
        public async Task<IActionResult> Create()
        {
            try
            {
                LogInfo("Create GET");

                var model = new VentaFormViewModel
                {
                    FechaVenta = DateTime.Today,
                    NumeroFactura = await _ventaService.GenerarNumeroFacturaAsync(),
                    Usuario = User.Identity?.Name ?? "Desconocido",
                    Vendedor = User.Identity?.Name ?? "Desconocido",
                    ProductosPresupuesto = new List<DetalleVentaViewModel>(),
                    Estado = EstadoVenta.Borrador.ToString()
                };

                // Usar método común para cargar opciones
                await CargarOpcionesFormasPagoAsync(model);

                return View("Form", model);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al inicializar formulario de venta");
                return View("Error");
            }
        }

        // POST: Ventas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:ventas.crear")]
        public async Task<IActionResult> Create(VentaFormViewModel model, string Finalizar)
        {
            try
            {
                LogInfo("Create POST => Finalizar={Finalizar}", Finalizar);

                // Validar forma de pago utilizando PaymentValidator
                if (!PaymentValidator.ValidatePaymentMethod(model, ModelState))
                {
                    await CargarOpcionesFormasPagoAsync(model);
                    return View("Form", model);
                }

                // Establecer valores por defecto
                model.Observaciones ??= "Sin observaciones";
                model.Condiciones ??= "Condiciones estándar";

                if (!ModelState.IsValid)
                {
                    LogModelStateErrors();
                    await CargarOpcionesFormasPagoAsync(model);
                    return View("Form", model);
                }

                // Usar método común de validación
                if (!ValidarProductosEnOperacion(model.ProductosPresupuesto, "venta"))
                {
                    await CargarOpcionesFormasPagoAsync(model);
                    return View("Form", model);
                }

                // Convertir viewmodel a venta
                var venta = _mapper.Map<Venta>(model);
                venta.TotalProductos = CalcularCantidadTotalProductos(venta.ProductosPresupuesto, p => p.Cantidad);
                venta.PrecioTotal = CalcularTotalOperacion(venta.ProductosPresupuesto, p => p.PrecioTotal);
                venta.Usuario = User.Identity?.Name ?? "Desconocido";
                venta.Vendedor = User.Identity?.Name ?? "Desconocido";

                // Definir estado según finalizar
                bool finalizar = !string.IsNullOrEmpty(Finalizar) && Finalizar.Equals("true", StringComparison.OrdinalIgnoreCase);
                venta.Estado = finalizar ? EstadoVenta.PendienteDeAutorizacion : EstadoVenta.Borrador;

                // Crear venta
                await _ventaService.CreateVentaAsync(venta);

                // Usar método común de auditoría
                await RegistrarAuditoriaOperacionAsync(
                    "Venta",
                    "Create",
                    venta.VentaID,
                    $"Cliente={venta.NombreCliente}, Total={venta.PrecioTotal}, Estado={venta.Estado}"
                );

                string mensaje = finalizar ?
                    "Venta enviada para autorización exitosamente." :
                    "Borrador de venta guardado exitosamente.";

                SetSuccessMessage(mensaje);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al crear venta");
                ModelState.AddModelError(string.Empty, "Ocurrió un error al crear la venta: " + ex.Message);
                await CargarOpcionesFormasPagoAsync(model);
                return View("Form", model);
            }
        }

        // GET: Ventas/Edit/5
        [HttpGet]
        [Authorize(Policy = "Permission:ventas.editar")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                LogInfo("Edit GET => VentaID={ID}", id);
                var venta = await _ventaService.GetVentaByIDAsync(id);
                if (venta == null) return NotFound();

                // Usar método común para validar estado
                if (!ValidarEstadoParaEdicion(venta.Estado, new[] { EstadoVenta.Borrador }, "venta"))
                {
                    return RedirectToAction(nameof(Details), new { id });
                }

                var model = _mapper.Map<VentaFormViewModel>(venta);
                await CargarOpcionesFormasPagoAsync(model);
                return View("Form", model);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al cargar la venta para edición");
                return View("Error");
            }
        }

        // POST: Ventas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:ventas.editar")]
        public async Task<IActionResult> Edit(VentaFormViewModel model)
        {
            try
            {
                LogInfo("Edit POST => VentaID={ID}", model.VentaID);

                // Validar forma de pago
                if (!PaymentValidator.ValidatePaymentMethod(model, ModelState))
                {
                    await CargarOpcionesFormasPagoAsync(model);
                    return View("Form", model);
                }

                if (!ModelState.IsValid)
                {
                    LogModelStateErrors();
                    await CargarOpcionesFormasPagoAsync(model);
                    return View("Form", model);
                }

                // Usar método común de validación
                if (!ValidarProductosEnOperacion(model.ProductosPresupuesto, "venta"))
                {
                    await CargarOpcionesFormasPagoAsync(model);
                    return View("Form", model);
                }

                // Obtener venta original para verificar estado
                var ventaOriginal = await _ventaService.GetVentaByIDAsync(model.VentaID);
                if (ventaOriginal == null) return NotFound();

                // Usar método común para validar estado
                if (!ValidarEstadoParaEdicion(ventaOriginal.Estado, new[] { EstadoVenta.Borrador }, "venta"))
                {
                    return RedirectToAction(nameof(Details), new { id = model.VentaID });
                }

                // Convertir viewmodel a venta
                var venta = _mapper.Map<Venta>(model);
                venta.TotalProductos = CalcularCantidadTotalProductos(venta.ProductosPresupuesto, p => p.Cantidad);
                venta.PrecioTotal = CalcularTotalOperacion(venta.ProductosPresupuesto, p => p.PrecioTotal);

                // Conservar algunos valores originales
                venta.FechaVenta = ventaOriginal.FechaVenta;
                venta.NumeroFactura = ventaOriginal.NumeroFactura;
                venta.Usuario = ventaOriginal.Usuario;
                venta.Estado = ventaOriginal.Estado;

                // Actualizar la venta
                await _ventaService.UpdateVentaAsync(venta);

                // Usar método común de auditoría
                await RegistrarAuditoriaOperacionAsync(
                    "Venta",
                    "Update",
                    venta.VentaID,
                    $"Cliente={venta.NombreCliente}, Total={venta.PrecioTotal}, Estado={venta.Estado}"
                );

                SetSuccessMessage("Venta actualizada exitosamente.");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al actualizar la venta");
                ModelState.AddModelError(string.Empty, "Ocurrió un error al actualizar la venta.");
                await CargarOpcionesFormasPagoAsync(model);
                return View("Form", model);
            }
        }

        [HttpGet]
        [Authorize(Policy = "Permission:ventas.ver")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var venta = await _ventaService.GetVentaByIDAsync(id);
                if (venta == null)
                    return NotFound();

                return View(venta);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al obtener detalles de venta");
                return View("Error");
            }
        }

        [HttpGet]
        [Authorize(Policy = "Permission:ventas.eliminar")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var venta = await _ventaService.GetVentaByIDAsync(id);
                if (venta == null)
                    return NotFound();

                // Usar método común para validar estado
                if (!ValidarEstadoParaEliminacion(venta.Estado, new[] { EstadoVenta.Borrador, EstadoVenta.Rechazada }, "venta"))
                {
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = _mapper.Map<VentaListViewModel>(venta);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al cargar venta para eliminar");
                return View("Error");
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:ventas.eliminar")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var venta = await _ventaService.GetVentaByIDAsync(id);
                if (venta == null)
                    return NotFound();

                // Usar método común para validar estado
                if (!ValidarEstadoParaEliminacion(venta.Estado, new[] { EstadoVenta.Borrador, EstadoVenta.Rechazada }, "venta"))
                {
                    return RedirectToAction(nameof(Index));
                }

                await _ventaService.DeleteVentaAsync(id);

                // Usar método común de auditoría
                await RegistrarAuditoriaOperacionAsync("Venta", "Delete", id, $"Venta eliminada: {venta.NumeroFactura}");

                SetSuccessMessage("Venta eliminada exitosamente");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al eliminar venta");
                SetErrorMessage("Error al eliminar la venta");
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Ventas/Autorizaciones
        [HttpGet]
        [Authorize(Policy = "Permission:ventas.autorizar")]
        public async Task<IActionResult> Autorizaciones()
        {
            try
            {
                var ventas = await _ventaService.GetVentasByEstadoAsync(EstadoVenta.PendienteDeAutorizacion);
                var model = _mapper.Map<IEnumerable<VentaListViewModel>>(ventas);
                return View(model);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al cargar ventas pendientes de autorización");
                return View("Error");
            }
        }

        // POST: Ventas/Autorizar
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:ventas.autorizar")]
        public async Task<IActionResult> Autorizar(int id)
        {
            return await CambiarEstadoOperacionAsync(
                id,
                EstadoVenta.Autorizada,
                _ventaService.GetVentaByIDAsync,
                async (venta, estado) => {
                    await _ventaService.AutorizarVentaAsync(id, User.Identity?.Name ?? "Desconocido");
                    return true;
                },
                "Venta"
            );
        }

        // POST: Ventas/Rechazar
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:ventas.rechazar")]
        public async Task<IActionResult> Rechazar(int id)
        {
            return await CambiarEstadoOperacionAsync(
                id,
                EstadoVenta.Rechazada,
                _ventaService.GetVentaByIDAsync,
                async (venta, estado) => {
                    await _ventaService.RechazarVentaAsync(id, User.Identity?.Name ?? "Desconocido");
                    return true;
                },
                "Venta"
            );
        }

        // GET: Ventas/EntregaProductos
        [HttpGet]
        [Authorize(Policy = "Permission:ventas.entregaProductos")]
        public async Task<IActionResult> EntregaProductos()
        {
            try
            {
                var ventas = await _ventaService.GetVentasPendientesDeEntregaAsync();
                var model = _mapper.Map<IEnumerable<VentaListViewModel>>(ventas);
                return View(model);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al cargar ventas pendientes de entrega");
                return View("Error");
            }
        }

        // POST: Ventas/MarcarEntregada
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:ventas.entregaProductos")]
        public async Task<IActionResult> MarcarEntregada(int id)
        {
            return await CambiarEstadoOperacionAsync(
                id,
                EstadoVenta.Entregada,
                _ventaService.GetVentaByIDAsync,
                async (venta, estado) => {
                    await _ventaService.MarcarVentaComoEntregadaAsync(id, User.Identity?.Name ?? "Desconocido");
                    return true;
                },
                "Venta"
            );
        }

        #endregion

        #region Cotizaciones

        [HttpGet]
        [Authorize(Policy = "Permission:ventas.ver")]
        public async Task<IActionResult> ListaCotizaciones()
        {
            try
            {
                var cotizaciones = await _cotizacionService.GetAllCotizacionesAsync();
                var viewModel = _mapper.Map<IEnumerable<CotizacionListViewModel>>(cotizaciones);
                return View("~/Views/Cotizacion/Index.cshtml", viewModel);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al obtener cotizaciones");
                return View("Error");
            }
        }

        [HttpGet]
        [Authorize(Policy = "Permission:ventas.ver")]
        public async Task<IActionResult> CrearCotizacion()
        {
            try
            {
                var viewModel = new CotizacionViewModel
                {
                    FechaCotizacion = DateTime.Today,
                    NumeroCotizacion = await _cotizacionService.GenerarNumeroCotizacionAsync(),
                    DiasVigencia = 15,
                    ProductosPresupuesto = new List<DetalleVentaViewModel>()
                };

                // Usar método común para cargar opciones
                await CargarOpcionesFormasPagoAsync(viewModel);

                return View("~/Views/Cotizacion/Create.cshtml", viewModel);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al inicializar formulario de cotización");
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:ventas.ver")]
        public async Task<IActionResult> CrearCotizacion(CotizacionViewModel model)
        {
            try
            {
                LogInfo("Create cotización POST - Total productos: {Count}, Precio total: {Price}",
                    model.ProductosPresupuesto?.Count ?? 0, model.PrecioTotal);

                if (!ModelState.IsValid)
                {
                    await CargarOpcionesFormasPagoAsync(model);
                    return View("~/Views/Cotizacion/Create.cshtml", model);
                }

                var cotizacion = _mapper.Map<Cotizacion>(model);
                cotizacion.FechaCotizacion = DateTime.Now;
                cotizacion.FechaVencimiento = DateTime.Now.AddDays(model.DiasVigencia);
                cotizacion.Usuario = User.Identity?.Name ?? "Sistema";

                // Usar métodos comunes de cálculo
                cotizacion.TotalProductos = CalcularCantidadTotalProductos(cotizacion.ProductosPresupuesto, p => p.Cantidad);
                cotizacion.PrecioTotal = CalcularTotalOperacion(cotizacion.ProductosPresupuesto, p => p.Cantidad * p.PrecioUnitario);

                // Actualizar precio total de cada item
                foreach (var item in cotizacion.ProductosPresupuesto)
                {
                    item.PrecioTotal = item.Cantidad * item.PrecioUnitario;
                }

                await _cotizacionService.CreateCotizacionAsync(cotizacion);

                // Usar método común de auditoría
                await RegistrarAuditoriaOperacionAsync(
                    "Cotizacion",
                    "Create",
                    cotizacion.CotizacionID,
                    $"Cliente={cotizacion.NombreCliente}, Total={cotizacion.PrecioTotal}"
                );

                SetSuccessMessage("Cotización creada exitosamente");
                return RedirectToAction(nameof(ListaCotizaciones));
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al crear cotización");
                ModelState.AddModelError(string.Empty, "Error al crear la cotización: " + ex.Message);
                await CargarOpcionesFormasPagoAsync(model);
                return View("~/Views/Cotizacion/Create.cshtml", model);
            }
        }

        #endregion

 

        #endregion

        #region Créditos

        [HttpGet]
        [Authorize(Policy = "Permission:ventas.crear")]
        public async Task<IActionResult> CalcularCuotas(int id)
        {
            var venta = await _ventaService.GetVentaByIDAsync(id);
            if (venta == null)
                return NotFound();

            if (venta.Estado != EstadoVenta.Autorizada)
            {
                SetErrorMessage("Solo se pueden calcular cuotas para ventas autorizadas");
                return RedirectToAction(nameof(Details), new { id });
            }

            var cliente = await _clienteService.GetClienteByIDAsync(venta.DniCliente);
            if (cliente == null || !cliente.AptoCredito)
            {
                SetErrorMessage("El cliente no es apto para crédito");
                return RedirectToAction(nameof(Details), new { id });
            }

            var viewModel = new VentaCreditoViewModel
            {
                VentaID = venta.VentaID,
                NumeroFactura = venta.NumeroFactura,
                FechaVenta = venta.FechaVenta,
                ClienteNombre = venta.NombreCliente,
                MontoTotal = venta.PrecioTotal,
                FechaVencimiento = DateTime.Now.AddDays(30),
                NumeroCuotas = 3,
                ScoreCliente = cliente.ScoreCredito,
                RequiereGarante = cliente.RequiereGarante,
                TieneGarante = cliente.GaranteID.HasValue
            };

            var criterio = await _creditoService.GetCriterioByScoreAsync(cliente.ScoreCredito);
            if (criterio != null)
            {
                viewModel.PlazoMaximo = criterio.PlazoMaximo;
                viewModel.PlazosDisponibles = Enumerable.Range(1, criterio.PlazoMaximo)
                    .Select(p => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Value = p.ToString(),
                        Text = $"{p} {(p == 1 ? "cuota" : "cuotas")}"
                    });
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:ventas.crear")]
        public async Task<IActionResult> CalcularCuotas(VentaCreditoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var cliente = await _clienteService.GetClienteByIDAsync(model.ClienteID);
                var criterio = await _creditoService.GetCriterioByScoreAsync(cliente?.ScoreCredito);
                if (criterio != null)
                {
                    model.PlazoMaximo = criterio.PlazoMaximo;
                    model.PlazosDisponibles = Enumerable.Range(1, criterio.PlazoMaximo)
                        .Select(p => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                        {
                            Value = p.ToString(),
                            Text = $"{p} {(p == 1 ? "cuota" : "cuotas")}"
                        });
                }
                return View(model);
            }

            try
            {
                var venta = await _ventaService.GetVentaByIDAsync(model.VentaID);
                if (venta == null)
                    return NotFound();

                venta = await _ventaService.CrearVentaCreditoAsync(venta, model.NumeroCuotas, model.FechaVencimiento);

                // Usar método común de auditoría
                await RegistrarAuditoriaOperacionAsync(
                    "Venta",
                    "ConvertirACredito",
                    venta.VentaID,
                    $"Venta convertida a crédito: {model.NumeroCuotas} cuotas"
                );

                SetSuccessMessage($"Venta procesada a crédito en {model.NumeroCuotas} cuotas");
                return RedirectToAction(nameof(Details), new { id = venta.VentaID });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        #endregion
    }
}