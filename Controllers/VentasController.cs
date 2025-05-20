// Controllers/VentasController.cs
using AutoMapper;
using Javo2.Controllers.Base;
using Javo2.Helpers;
using Javo2.IServices;
using Javo2.Models;
using Javo2.Services;
using Javo2.ViewModels.Operaciones.DevolucionGarantia;
using Javo2.ViewModels.Operaciones.Ventas;
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
    [Authorize]  // Fuerza que el usuario esté autenticado
    public class VentasController : BaseController
    {
        private readonly IVentaService _ventaService;
        private readonly ICotizacionService _cotizacionService;
        private readonly IMapper _mapper;
        private readonly IClienteService _clienteService;
        private readonly IProductoService _productoService;
        private readonly IAuditoriaService _auditoriaService;
        private readonly IDevolucionGarantiaService _devolucionService;
        private readonly ICreditoService _creditoService;
        private readonly string _controllerId;


        public VentasController(
            IVentaService ventaService,
            ICotizacionService cotizacionService,
            IMapper mapper,
              ILogger<VentasController> logger,
            IClienteService clienteService,
            IProductoService productoService,
            IAuditoriaService auditoriaService,
            IDevolucionGarantiaService devolucionService, ICreditoService creditoService
        ) : base(logger)
        {
            _controllerId = Guid.NewGuid().ToString();
            _logger.LogWarning("DEPURACIÓN: VentasController creado - ID: {ID}", _controllerId);
            _ventaService = ventaService;
       
            _cotizacionService = cotizacionService;
            _mapper = mapper;
            _clienteService = clienteService;
            _productoService = productoService;
            _auditoriaService = auditoriaService;
            _devolucionService = devolucionService; // Inicializar el servicio
            _creditoService = creditoService;


        }

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
                var devoluciones = await _devolucionService.GetAllAsync(); // Añadir esta línea
                viewModel.Devoluciones = _mapper.Map<IEnumerable<DevolucionGarantiaListViewModel>>(devoluciones); // Añadir esta línea

                ViewBag.ActiveTab = activeTab;

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la lista de ventas y cotizaciones");
                return View("Error");
            }
        }

        // GET: Ventas/ConvertirCotizacionAVenta/{id}
        [HttpGet]
        [Authorize(Policy = "Permission:ventas.crear")]
        public async Task<IActionResult> ConvertirCotizacionAVenta(int id)
        {
            try
            {
                var cotizacion = await _cotizacionService.GetCotizacionByIDAsync(id);
                if (cotizacion == null)
                    return NotFound();

                // Crear VentaFormViewModel a partir de la cotización
                var ventaViewModel = new VentaFormViewModel
                {
                    FechaVenta = DateTime.Today,
                    NumeroFactura = await _ventaService.GenerarNumeroFacturaAsync(),
                    Usuario = User.Identity?.Name ?? "Desconocido",
                    Vendedor = User.Identity?.Name ?? "Desconocido",
                    DniCliente = cotizacion.DniCliente,
                    NombreCliente = cotizacion.NombreCliente,
                    TelefonoCliente = cotizacion.TelefonoCliente,
                    DomicilioCliente = cotizacion.DomicilioCliente ?? string.Empty,
                    LocalidadCliente = cotizacion.LocalidadCliente ?? string.Empty,
                    CelularCliente = cotizacion.CelularCliente ?? string.Empty,
                    ProductosPresupuesto = _mapper.Map<List<DetalleVentaViewModel>>(cotizacion.ProductosPresupuesto),
                    Estado = EstadoVenta.Borrador.ToString(),
                    Observaciones = cotizacion.Observaciones,
                    Condiciones = $"Generado desde cotización #{cotizacion.CotizacionID} - {cotizacion.NumeroCotizacion}"
                };

                // Cargar combos para el formulario
                await CargarCombosAsync(ventaViewModel);

                return View("Form", ventaViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear venta desde cotización");
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
                _logger.LogInformation("Create GET");

                // Initialize new view model
                var model = new VentaFormViewModel
                {
                    FechaVenta = DateTime.Today,
                    NumeroFactura = await _ventaService.GenerarNumeroFacturaAsync(),
                    Usuario = User.Identity?.Name ?? "Desconocido",
                    Vendedor = User.Identity?.Name ?? "Desconocido",
                    ProductosPresupuesto = new List<DetalleVentaViewModel>(),
                    Estado = EstadoVenta.Borrador.ToString()
                };

                // Load form combos
                await CargarCombosAsync(model);

                return View("Form", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al inicializar formulario de venta");
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
                _logger.LogInformation("Create POST => Finalizar={Finalizar}, Model={@Model}", Finalizar, model);

                // Validar forma de pago utilizando el nuevo PaymentValidator
                if (!PaymentValidator.ValidatePaymentMethod(model, ModelState))
                {
                    await CargarCombosAsync(model);
                    return View("Form", model);
                }

                // Establecer valores por defecto
                if (string.IsNullOrEmpty(model.Observaciones))
                    model.Observaciones = "Sin observaciones";

                if (string.IsNullOrEmpty(model.Condiciones))
                    model.Condiciones = "Condiciones estándar";

                // Validaciones
                if (!ModelState.IsValid)
                {
                    // Registrar errores para depuración
                    LogModelStateErrors();
                    await CargarCombosAsync(model);
                    return View("Form", model);
                }

                // Validar que haya productos
                if (model.ProductosPresupuesto == null || !model.ProductosPresupuesto.Any())
                {
                    ModelState.AddModelError("", "Debe agregar al menos un producto a la venta");
                    await CargarCombosAsync(model);
                    return View("Form", model);
                }

                // Convertir viewmodel a venta
                var venta = _mapper.Map<Venta>(model);
                venta.TotalProductos = venta.ProductosPresupuesto.Sum(p => p.Cantidad);
                venta.PrecioTotal = venta.ProductosPresupuesto.Sum(p => p.PrecioTotal);
                venta.Usuario = User.Identity?.Name ?? "Desconocido";
                venta.Vendedor = User.Identity?.Name ?? "Desconocido";

                // Definir estado según finalizar
                bool finalizar = !string.IsNullOrEmpty(Finalizar) && Finalizar.Equals("true", StringComparison.OrdinalIgnoreCase);
                venta.Estado = finalizar ? EstadoVenta.PendienteDeAutorizacion : EstadoVenta.Borrador;

                // Crear venta
                await _ventaService.CreateVentaAsync(venta);

                // Registrar en auditoría
                await _auditoriaService.RegistrarCambioAsync(new AuditoriaRegistro
                {
                    FechaHora = DateTime.Now,
                    Usuario = venta.Usuario,
                    Entidad = "Venta",
                    Accion = "Create",
                    LlavePrimaria = venta.VentaID.ToString(),
                    Detalle = $"Cliente={venta.NombreCliente}, Total={venta.PrecioTotal}, Estado={venta.Estado}"
                });

                string mensaje = finalizar ?
                    "Venta enviada para autorización exitosamente." :
                    "Borrador de venta guardado exitosamente.";

                TempData["Success"] = mensaje;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear venta");
                ModelState.AddModelError(string.Empty, "Ocurrió un error al crear la venta: " + ex.Message);
                await CargarCombosAsync(model);
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
                _logger.LogInformation("Edit GET => VentaID={ID}", id);
                var venta = await _ventaService.GetVentaByIDAsync(id);
                if (venta == null) return NotFound();

                // Verificar si la venta está en un estado que permite edición
                if (venta.Estado != EstadoVenta.Borrador)
                {
                    TempData["Error"] = "Solo se pueden editar ventas en estado Borrador.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                var model = _mapper.Map<VentaFormViewModel>(venta);
                await CargarCombosAsync(model);
                return View("Form", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar la venta para edición");
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
                _logger.LogInformation("Edit POST => VentaID={ID}", model.VentaID);

                // Validar forma de pago utilizando el nuevo PaymentValidator
                if (!PaymentValidator.ValidatePaymentMethod(model, ModelState))
                {
                    await CargarCombosAsync(model);
                    return View("Form", model);
                }

                // Validaciones
                if (!ModelState.IsValid)
                {
                    LogModelStateErrors();
                    await CargarCombosAsync(model);
                    return View("Form", model);
                }

                // Validar que haya productos
                if (model.ProductosPresupuesto == null || model.ProductosPresupuesto.Count == 0)
                {
                    ModelState.AddModelError("", "Debe agregar al menos un producto a la venta");
                    await CargarCombosAsync(model);
                    return View("Form", model);
                }

                // Obtener venta original para verificar estado
                var ventaOriginal = await _ventaService.GetVentaByIDAsync(model.VentaID);
                if (ventaOriginal == null) return NotFound();

                // Verificar que la venta esté en estado Borrador
                if (ventaOriginal.Estado != EstadoVenta.Borrador)
                {
                    TempData["Error"] = "Solo se pueden editar ventas en estado Borrador.";
                    return RedirectToAction(nameof(Details), new { id = model.VentaID });
                }

                // Convertir viewmodel a venta
                var venta = _mapper.Map<Venta>(model);
                venta.TotalProductos = venta.ProductosPresupuesto.Sum(p => p.Cantidad);
                venta.PrecioTotal = venta.ProductosPresupuesto.Sum(p => p.PrecioTotal);

                // Conservar algunos valores originales
                venta.FechaVenta = ventaOriginal.FechaVenta;
                venta.NumeroFactura = ventaOriginal.NumeroFactura;
                venta.Usuario = ventaOriginal.Usuario;
                venta.Estado = ventaOriginal.Estado;

                // Actualizar la venta
                await _ventaService.UpdateVentaAsync(venta);

                // Registrar en auditoría
                await _auditoriaService.RegistrarCambioAsync(new AuditoriaRegistro
                {
                    FechaHora = DateTime.Now,
                    Usuario = User.Identity?.Name ?? "Desconocido",
                    Entidad = "Venta",
                    Accion = "Update",
                    LlavePrimaria = venta.VentaID.ToString(),
                    Detalle = $"Cliente={venta.NombreCliente}, Total={venta.PrecioTotal}, Estado={venta.Estado}"
                });

                TempData["Success"] = "Venta actualizada exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar la venta");
                ModelState.AddModelError(string.Empty, "Ocurrió un error al actualizar la venta.");
                await CargarCombosAsync(model);
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
                _logger.LogError(ex, "Error al obtener detalles de venta");
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

                if (venta.Estado != EstadoVenta.Borrador && venta.Estado != EstadoVenta.Rechazada)
                {
                    TempData["Error"] = "Solo se pueden eliminar ventas en estado Borrador o Rechazada.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = _mapper.Map<VentaListViewModel>(venta);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar venta para eliminar");
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

                if (venta.Estado != EstadoVenta.Borrador && venta.Estado != EstadoVenta.Rechazada)
                {
                    TempData["Error"] = "Solo se pueden eliminar ventas en estado Borrador o Rechazada.";
                    return RedirectToAction(nameof(Index));
                }

                await _ventaService.DeleteVentaAsync(id);
                TempData["Success"] = "Venta eliminada exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar venta");
                TempData["Error"] = "Error al eliminar la venta";
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
                _logger.LogError(ex, "Error al cargar ventas pendientes de autorización");
                return View("Error");
            }
        }

        // POST: Ventas/Autorizar
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:ventas.autorizar")]
        public async Task<IActionResult> Autorizar(int id)
        {
            try
            {
                _logger.LogInformation("Autorizar POST => VentaID={ID}", id);
                await _ventaService.AutorizarVentaAsync(id, User.Identity?.Name ?? "Desconocido");
                return Json(new { success = true, message = "Venta autorizada correctamente." });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Venta no encontrada al intentar autorizar ID: {ID}", id);
                return Json(new { success = false, message = "Venta no encontrada." });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Estado incorrecto al intentar autorizar venta ID: {ID}", id);
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al autorizar venta ID: {ID}", id);
                return Json(new { success = false, message = "Error al procesar la solicitud." });
            }
        }

        // POST: Ventas/Rechazar
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:ventas.rechazar")]
        public async Task<IActionResult> Rechazar(int id)
        {
            try
            {
                _logger.LogInformation("Rechazar POST => VentaID={ID}", id);
                await _ventaService.RechazarVentaAsync(id, User.Identity?.Name ?? "Desconocido");
                return Json(new { success = true, message = "Venta rechazada correctamente." });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Venta no encontrada al intentar rechazar ID: {ID}", id);
                return Json(new { success = false, message = "Venta no encontrada." });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Estado incorrecto al intentar rechazar venta ID: {ID}", id);
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al rechazar venta ID: {ID}", id);
                return Json(new { success = false, message = "Error al procesar la solicitud." });
            }
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
                _logger.LogError(ex, "Error al cargar ventas pendientes de entrega");
                return View("Error");
            }
        }

        // POST: Ventas/MarcarEntregada
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:ventas.entregaProductos")]
        public async Task<IActionResult> MarcarEntregada(int id)
        {
            try
            {
                _logger.LogInformation("MarcarEntregada POST => VentaID={ID}", id);
                await _ventaService.MarcarVentaComoEntregadaAsync(id, User.Identity?.Name ?? "Desconocido");
                return Json(new { success = true, message = "Venta marcada como entregada correctamente." });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Venta no encontrada al marcar como entregada ID: {ID}", id);
                return Json(new { success = false, message = "Venta no encontrada." });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Estado incorrecto al marcar venta como entregada ID: {ID}", id);
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al marcar venta como entregada ID: {ID}", id);
                return Json(new { success = false, message = "Error al procesar la solicitud." });
            }
        }

        // GET: Ventas/Reimprimir/5
        [HttpGet]
        [Authorize(Policy = "Permission:ventas.ver")]
        public async Task<IActionResult> Reimprimir(int id)
        {
            try
            {
                var venta = await _ventaService.GetVentaByIDAsync(id);
                if (venta == null) return NotFound();

                // Registrar en auditoría
                await _auditoriaService.RegistrarCambioAsync(new AuditoriaRegistro
                {
                    FechaHora = DateTime.Now,
                    Usuario = User.Identity?.Name ?? "Desconocido",
                    Entidad = "Venta",
                    Accion = "Reimprimir",
                    LlavePrimaria = id.ToString(),
                    Detalle = $"Reimpresión de venta: {venta.NumeroFactura}"
                });

                return View(venta);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al reimprimir venta ID: {ID}", id);
                return View("Error");
            }
        }

        #region Métodos AJAX

        [HttpPost]
        [Authorize(Policy = "Permission:ventas.ver")]
        public async Task<IActionResult> BuscarClientePorDNI(int dni)
        {
            try
            {
                _logger.LogInformation("BuscarClientePorDNI => DNI={Dni}", dni);
                if (await _clienteService.GetClienteByDNIAsync(dni) is not { } cliente)
                    return Json(new { success = false, message = "Cliente no encontrado con ese DNI." });

                // Determinar si el cliente puede usar crédito
                decimal saldoDisponible = cliente.AptoCredito ? cliente.SaldoDisponible : 0;

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        nombre = $"{cliente.Nombre} {cliente.Apellido}",
                        telefono = cliente.Telefono,
                        domicilio = $"{cliente.Calle} {cliente.NumeroCalle}",
                        localidad = cliente.Localidad,
                        celular = cliente.Celular,
                        limiteCredito = cliente.LimiteCreditoInicial,
                        saldo = cliente.Saldo,
                        saldoDisponible = saldoDisponible,
                        aptoCredito = cliente.AptoCredito
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar cliente por DNI");
                return Json(new { success = false, message = "Error al buscar el cliente." });
            }
        }
        private static int _contadorBusquedas = 0;

        public async Task<IActionResult> BuscarProducto(string codigoProducto)
        {
            int contador = Interlocked.Increment(ref _contadorBusquedas);
            _logger.LogWarning("DEPURACIÓN: BuscarProducto - Contador: {Contador}", contador);
            try
            {
                _logger.LogWarning("DEPURACIÓN: BuscarProducto llamado - ControllerID: {CtrlID}, Código: {Codigo}",
    _controllerId, codigoProducto);
                var producto = await _productoService.GetProductoByCodigoAsync(codigoProducto);

                _logger.LogWarning("DEPURACIÓN: Producto encontrado: {EncontradoSiNo}", producto != null ? "Sí" : "No");

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
                        precioUnitario = Math.Round(producto.PContado, 2),
                        precioLista = Math.Round(producto.PLista, 2)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar producto");
                return Json(new { success = false, message = "Error al buscar el producto." });
            }
        }

        private Task CargarCombosAsync(VentaFormViewModel model)
        {
            model.FormasPago = _ventaService.GetFormasPagoSelectList();
            model.Bancos = _ventaService.GetBancosSelectList();
            model.TipoTarjetaOptions = _ventaService.GetTipoTarjetaSelectList();
            model.CuotasOptions = _ventaService.GetCuotasSelectList();
            model.EntidadesElectronicas = _ventaService.GetEntidadesElectronicasSelectList();
            model.PlanesFinanciamiento = _ventaService.GetPlanesFinanciamientoSelectList();

            // Return a completed task since no asynchronous operations are performed
            return Task.CompletedTask;
        }

        #endregion
    // GET: Ventas/CalcularCuotas/5
    [HttpGet]
        [Authorize(Policy = "Permission:ventas.crear")]
        public async Task<IActionResult> CalcularCuotas(int id)
        {
            var venta = await _ventaService.GetVentaByIDAsync(id);
            if (venta == null)
                return NotFound();

            // Verificar que la venta esté en estado autorizada
            if (venta.Estado != EstadoVenta.Autorizada)
            {
                TempData["Error"] = "Solo se pueden calcular cuotas para ventas autorizadas";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Verificar si el cliente califica para crédito
            var cliente = await _clienteService.GetClienteByIDAsync(venta.DniCliente);
            if (cliente == null || !cliente.AptoCredito)
            {
                TempData["Error"] = "El cliente no es apto para crédito";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Crear view model
            var viewModel = new VentaCreditoViewModel
            {
                VentaID = venta.VentaID,
                NumeroFactura = venta.NumeroFactura,
                FechaVenta = venta.FechaVenta,
                ClienteNombre = venta.NombreCliente,
                MontoTotal = venta.PrecioTotal,
                FechaVencimiento = DateTime.Now.AddDays(30),
                NumeroCuotas = 3, // Valor por defecto
                ScoreCliente = cliente.ScoreCredito,
                RequiereGarante = cliente.RequiereGarante,
                TieneGarante = cliente.GaranteID.HasValue
            };

            // Obtener plazos disponibles según calificación del cliente
            var criterio = await _creditoService.GetCriterioByScoreAsync(cliente.ScoreCredito);
            if (criterio != null)
            {
                viewModel.PlazoMaximo = criterio.PlazoMaximo;
                viewModel.PlazosDisponibles = Enumerable.Range(1, criterio.PlazoMaximo)
                    .Select(p => new SelectListItem
                    {
                        Value = p.ToString(),
                        Text = $"{p} {(p == 1 ? "cuota" : "cuotas")}"
                    });
            }

            return View(viewModel);
        }

        // POST: Ventas/CalcularCuotas
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:ventas.crear")]
        public async Task<IActionResult> CalcularCuotas(VentaCreditoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Recargar plazos disponibles
                var cliente = await _clienteService.GetClienteByIDAsync(model.ClienteID);
                var criterio = await _creditoService.GetCriterioByScoreAsync(cliente?.ScoreCredito);
                if (criterio != null)
                {
                    model.PlazoMaximo = criterio.PlazoMaximo;
                    model.PlazosDisponibles = Enumerable.Range(1, criterio.PlazoMaximo)
                        .Select(p => new SelectListItem
                        {
                            Value = p.ToString(),
                            Text = $"{p} {(p == 1 ? "cuota" : "cuotas")}"
                        });
                }
                return View(model);
            }

            try
            {
                // Obtener venta
                var venta = await _ventaService.GetVentaByIDAsync(model.VentaID);
                if (venta == null)
                    return NotFound();

                // Crear venta a crédito
                venta = await _ventaService.CrearVentaCreditoAsync(venta, model.NumeroCuotas, model.FechaVencimiento);

                TempData["Success"] = $"Venta procesada a crédito en {model.NumeroCuotas} cuotas";
                return RedirectToAction(nameof(Details), new { id = venta.VentaID });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        // GET: Ventas/PlanPago/5
        [HttpGet]
        [Authorize(Policy = "Permission:ventas.ver")]
        public async Task<IActionResult> PlanPago(int id)
        {
            var venta = await _ventaService.GetVentaByIDAsync(id);
            if (venta == null)
                return NotFound();

            if (!venta.EsCredito)
            {
                TempData["Error"] = "Esta venta no fue realizada a crédito";
                return RedirectToAction(nameof(Details), new { id });
            }

            return View(venta);
        }

        // GET: Ventas/RegistrarPago/5
        [HttpGet]
        [Authorize(Policy = "Permission:ventas.editar")]
        public async Task<IActionResult> RegistrarPago(int id, int cuotaId)
        {
            var venta = await _ventaService.GetVentaByIDAsync(id);
            if (venta == null)
                return NotFound();

            var cuota = venta.CuotasPagas.FirstOrDefault(c => c.CuotaID == cuotaId);
            if (cuota == null)
                return NotFound();

            if (cuota.Pagada)
            {
                TempData["Error"] = "Esta cuota ya fue pagada";
                return RedirectToAction(nameof(PlanPago), new { id });
            }

            var viewModel = new PagoCuotaViewModel
            {
                VentaID = venta.VentaID,
                CuotaID = cuota.CuotaID,
                NumeroCuota = cuota.NumeroCuota,
                FechaVencimiento = cuota.FechaVencimiento,
                MontoCuota = cuota.ImporteCuota,
                FechaPago = DateTime.Now,
                FormaPago = "Efectivo"
            };

            // Calcular mora si aplica
            if (DateTime.Now > cuota.FechaVencimiento)
            {
                int diasAtraso = (int)(DateTime.Now - cuota.FechaVencimiento).TotalDays;
                decimal montoMora = await _creditoService.CalcularInteresDeAtrasoAsync(diasAtraso, cuota.ImporteCuota);
                viewModel.DiasAtraso = diasAtraso;
                viewModel.MontoMora = montoMora;
                viewModel.MontoTotal = cuota.ImporteCuota + montoMora;
            }
            else
            {
                viewModel.MontoTotal = cuota.ImporteCuota;
            }

            return View(viewModel);
        }

        // POST: Ventas/RegistrarPago
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:ventas.editar")]
        public async Task<IActionResult> RegistrarPago(PagoCuotaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                bool resultado = await _ventaService.ProcesarPagoCuotaAsync(
                    model.VentaID,
                    model.CuotaID,
                    model.MontoTotal,
                    model.FormaPago,
                    model.Referencia);

                if (resultado)
                {
                    TempData["Success"] = $"Pago de cuota {model.NumeroCuota} registrado correctamente";
                    return RedirectToAction(nameof(PlanPago), new { id = model.VentaID });
                }
                else
                {
                    ModelState.AddModelError("", "No se pudo procesar el pago");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }
    }
}