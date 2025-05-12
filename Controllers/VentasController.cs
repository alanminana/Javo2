// Controllers/VentasController.cs
using AutoMapper;
using Javo2.Controllers.Base;
using Javo2.IServices;
using Javo2.Models;
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

        public VentasController(
            IVentaService ventaService,
            ICotizacionService cotizacionService,
            IMapper mapper,
            ILogger<VentasController> logger,
            IClienteService clienteService,
            IProductoService productoService,
            IAuditoriaService auditoriaService
        ) : base(logger)
        {
            _ventaService = ventaService;
            _cotizacionService = cotizacionService;
            _mapper = mapper;
            _clienteService = clienteService;
            _productoService = productoService;
            _auditoriaService = auditoriaService;
        }

        // GET: Ventas/Index
        [HttpGet]
        [Authorize(Policy = "Permission:ventas.ver")]
        public async Task<IActionResult> Index(VentaFilterDto filter)
        {
            try
            {
                _logger.LogInformation("Index GET => Filtro: {@Filter}", filter);
                var ventas = await _ventaService.GetVentasAsync(filter);
                var model = _mapper.Map<IEnumerable<VentaListViewModel>>(ventas);

                // Guardar filtros en ViewBag para la vista
                ViewBag.FechaInicio = filter.FechaInicio;
                ViewBag.FechaFin = filter.FechaFin;
                ViewBag.NombreCliente = filter.NombreCliente;
                ViewBag.NumeroFactura = filter.NumeroFactura;

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la lista de ventas");
                return View("Error");
            }
        }
        // En VentasController.cs, actualiza el método CreateFromCotizacion
        [HttpGet]
        [Authorize(Policy = "Permission:ventas.crear")]
        public async Task<IActionResult> CreateFromCotizacion(int id)
        {
            try
            {
                var cotizacion = await _cotizacionService.GetCotizacionByIDAsync(id);
                if (cotizacion == null)
                    return NotFound();

                // Utilizar el mapeo para convertir la cotización en formulario de venta
                var ventaViewModel = _mapper.Map<VentaFormViewModel>(cotizacion);

                // Obtener número de factura nuevo
                ventaViewModel.NumeroFactura = await _ventaService.GenerarNumeroFacturaAsync();

                // Asignar usuario actual
                ventaViewModel.Usuario = User.Identity?.Name ?? "Desconocido";
                ventaViewModel.Vendedor = User.Identity?.Name ?? "Desconocido";

                // Asignar manualmente los datos faltantes
                ventaViewModel.NombreCliente = cotizacion.NombreCliente;
                ventaViewModel.DniCliente = cotizacion.DniCliente;
                ventaViewModel.TelefonoCliente = cotizacion.TelefonoCliente;

                // Usar valores por defecto para los campos faltantes
                ventaViewModel.DomicilioCliente = string.Empty;
                ventaViewModel.LocalidadCliente = string.Empty;
                ventaViewModel.CelularCliente = string.Empty;

                // Observaciones relacionadas con la cotización
                ventaViewModel.Observaciones = cotizacion.Observaciones;
                ventaViewModel.Condiciones = $"Generado desde cotización #{cotizacion.CotizacionID} - {cotizacion.NumeroCotizacion}";

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
        [HttpGet]
        [Authorize(Policy = "Permission:ventas.crear")]
        public async Task<IActionResult> ConvertirCotizacionAVenta(int id)
        {
            try
            {
                var cotizacion = await _cotizacionService.GetCotizacionByIDAsync(id);
                if (cotizacion == null)
                    return NotFound();

                // Crear VentaFormViewModel directamente sin mapeo automático
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

        // GET: Ventas/CreateFromCotizacion
        [HttpGet]
        [Authorize(Policy = "Permission:ventas.crear")]
        public async Task<IActionResult> CreateFromCotizacion()
        {
            try
            {
                // Obtener ID de cotización de TempData
                if (!TempData.ContainsKey("CotizacionID") || !(TempData["CotizacionID"] is int cotizacionID))
                {
                    return RedirectToAction(nameof(Create));
                }

                // Obtener cotización
                var cotizacion = await _cotizacionService.GetCotizacionByIDAsync(cotizacionID);
                if (cotizacion == null)
                {
                    return RedirectToAction(nameof(Create));
                }

                // Crear venta a partir de cotización
                var viewModel = new VentaFormViewModel
                {
                    FechaVenta = DateTime.Today,
                    NumeroFactura = await _ventaService.GenerarNumeroFacturaAsync(),
                    Usuario = User.Identity?.Name ?? "Desconocido",
                    Vendedor = User.Identity?.Name ?? "Desconocido",

                    // Datos del cliente de la cotización
                    NombreCliente = cotizacion.NombreCliente,
                    DniCliente = cotizacion.DniCliente,
                    TelefonoCliente = cotizacion.TelefonoCliente,
                    DomicilioCliente = cotizacion.DomicilioCliente,
                    LocalidadCliente = cotizacion.LocalidadCliente,
                    CelularCliente = cotizacion.CelularCliente,

                    // Productos de la cotización
                    ProductosPresupuesto = _mapper.Map<List<DetalleVentaViewModel>>(cotizacion.ProductosPresupuesto),

                    // Estado inicial
                    Estado = EstadoVenta.Borrador.ToString(),

                    // Observaciones relacionadas con la cotización
                    Observaciones = cotizacion.Observaciones,
                    Condiciones = $"Generado desde cotización #{cotizacion.VentaID} - {cotizacion.NumeroFactura}"
                };

                // Cargar combos y otros datos necesarios
                await CargarCombosAsync(viewModel);

                // Establecer algunos valores por defecto
                viewModel.FormaPagoID = 1; // Contado por defecto

                return View("Form", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear venta desde cotización");
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:ventas.crear")]
        public async Task<IActionResult> Create(VentaFormViewModel model, string Finalizar)
        {
            try
            {
                _logger.LogInformation("Create POST => Finalizar={Finalizar}, Model={@Model}", Finalizar, model);

                // Validar forma de pago primero para limpiar campos no necesarios
                ValidarFormaPago(model);
                if (string.IsNullOrEmpty(model.Observaciones))
                    model.Observaciones = "Sin observaciones";

                if (string.IsNullOrEmpty(model.Condiciones))
                    model.Condiciones = "Condiciones estándar";
                // Validaciones
                if (!ModelState.IsValid)
                {
                    // Registrar errores para depuración
                    foreach (var state in ModelState)
                    {
                        foreach (var error in state.Value.Errors)
                        {
                            _logger.LogError("ModelState Error en {Field}: {Error}", state.Key, error.ErrorMessage);
                        }
                    }

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
                bool finalizar = !string.IsNullOrEmpty(Finalizar) && Finalizar.ToLower() == "true";
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

                // Validaciones
                if (!ModelState.IsValid)
                {
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
                var cotizacion = await _cotizacionService.GetCotizacionByIDAsync(id);
                if (cotizacion == null)
                    return NotFound();

                return View("~/Views/Cotizacion/Detalles.cshtml", cotizacion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener detalles de cotización");
                return View("Error");
            }
        }// Agrega también un método Delete
        [HttpGet]
        [Authorize(Policy = "Permission:ventas.eliminar")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var cotizacion = await _cotizacionService.GetCotizacionByIDAsync(id);
                if (cotizacion == null)
                    return NotFound();

                var viewModel = _mapper.Map<CotizacionListViewModel>(cotizacion);
                return View("~/Views/Cotizacion/Delete.cshtml", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar cotización para eliminar");
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
                await _cotizacionService.DeleteCotizacionAsync(id);
                TempData["Success"] = "Cotización eliminada exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar cotización");
                TempData["Error"] = "Error al eliminar la cotización";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:ventas.eliminar")]
        public async Task<IActionResult> DeleteCotizacionConfirmed(int id)
        {
            try
            {
                await _cotizacionService.DeleteCotizacionAsync(id);
                TempData["Success"] = "Cotización eliminada exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar cotización");
                TempData["Error"] = "Error al eliminar la cotización";
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
                var venta = await _ventaService.GetVentaByIDAsync(id);
                if (venta == null || venta.Estado != EstadoVenta.PendienteDeAutorizacion)
                    return Json(new { success = false, message = "Venta no disponible para autorizar." });

                await _ventaService.AutorizarVentaAsync(id, User.Identity?.Name ?? "Desconocido");

                // Registrar en auditoría
                await _auditoriaService.RegistrarCambioAsync(new AuditoriaRegistro
                {
                    FechaHora = DateTime.Now,
                    Usuario = User.Identity?.Name ?? "Desconocido",
                    Entidad = "Venta",
                    Accion = "Autorizar",
                    LlavePrimaria = id.ToString(),
                    Detalle = $"Venta autorizada: Cliente={venta.NombreCliente}, Total={venta.PrecioTotal}"
                });

                return Json(new { success = true, message = "Venta autorizada correctamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al autorizar venta ID: {ID}", id);
                return Json(new { success = false, message = ex.Message });
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
                var venta = await _ventaService.GetVentaByIDAsync(id);
                if (venta == null || venta.Estado != EstadoVenta.PendienteDeAutorizacion)
                    return Json(new { success = false, message = "Venta no disponible para rechazar." });

                await _ventaService.RechazarVentaAsync(id, User.Identity?.Name ?? "Desconocido");

                // Registrar en auditoría
                await _auditoriaService.RegistrarCambioAsync(new AuditoriaRegistro
                {
                    FechaHora = DateTime.Now,
                    Usuario = User.Identity?.Name ?? "Desconocido",
                    Entidad = "Venta",
                    Accion = "Rechazar",
                    LlavePrimaria = id.ToString(),
                    Detalle = $"Venta rechazada: Cliente={venta.NombreCliente}, Total={venta.PrecioTotal}"
                });

                return Json(new { success = true, message = "Venta rechazada correctamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al rechazar venta ID: {ID}", id);
                return Json(new { success = false, message = ex.Message });
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
                var venta = await _ventaService.GetVentaByIDAsync(id);
                if (venta == null || venta.Estado != EstadoVenta.PendienteDeEntrega)
                    return Json(new { success = false, message = "Venta no disponible para entrega." });

                await _ventaService.MarcarVentaComoEntregadaAsync(id, User.Identity?.Name ?? "Desconocido");

                // Registrar en auditoría
                await _auditoriaService.RegistrarCambioAsync(new AuditoriaRegistro
                {
                    FechaHora = DateTime.Now,
                    Usuario = User.Identity?.Name ?? "Desconocido",
                    Entidad = "Venta",
                    Accion = "MarcarEntregada",
                    LlavePrimaria = id.ToString(),
                    Detalle = $"Venta marcada como entregada: Cliente={venta.NombreCliente}, Total={venta.PrecioTotal}"
                });

                return Json(new { success = true, message = "Venta marcada como entregada correctamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al marcar venta como entregada ID: {ID}", id);
                return Json(new { success = false, message = ex.Message });
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

        #region Métodos AJAX

        [HttpPost]
        [Authorize(Policy = "Permission:ventas.ver")]
        public async Task<IActionResult> BuscarClientePorDNI(int dni)
        {
            try
            {
                _logger.LogInformation("BuscarClientePorDNI => DNI={Dni}", dni);
                var cliente = await _clienteService.GetClienteByDNIAsync(dni);
                if (cliente == null)
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

        [HttpPost]
        [Authorize(Policy = "Permission:ventas.ver")]
        public async Task<IActionResult> BuscarProducto(string codigoProducto)
        {
            try
            {
                _logger.LogInformation("BuscarProducto => Código={Codigo}", codigoProducto);
                var producto = await _productoService.GetProductoByCodigoAsync(codigoProducto);
                if (producto == null)
                    return Json(new { success = false, message = "Producto no encontrado." });

                // Obtener stock disponible
                var stockItem = producto.StockItem;
                bool hayStock = stockItem != null && stockItem.CantidadDisponible > 0;

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
                        precioUnitario = producto.PContado,
                        precioLista = producto.PLista,
                        precioTotal = producto.PContado,
                        stockDisponible = stockItem?.CantidadDisponible ?? 0,
                        hayStock = hayStock
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar producto");
                return Json(new { success = false, message = "Error al buscar el producto." });
            }
        }

        // GET: Ventas/GetFormasPagoOptions
        [HttpGet]
        public async Task<IActionResult> GetFormasPagoOptions()
        {
            try
            {
                var formasPago = await _ventaService.GetFormasPagoAsync();
                var result = formasPago.Select(fp => new { value = fp.FormaPagoID, text = fp.Nombre });
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener formas de pago");
                return Json(new object[0]);
            }
        }

        // GET: Ventas/GetBancosOptions
        [HttpGet]
        public async Task<IActionResult> GetBancosOptions()
        {
            try
            {
                var bancos = await _ventaService.GetBancosAsync();
                var result = bancos.Select(b => new { value = b.BancoID, text = b.Nombre });
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener bancos");
                return Json(new object[0]);
            }
        }

        #endregion

        #region Métodos Auxiliares

        private async Task CargarCombosAsync(VentaFormViewModel model)
        {
            model.FormasPago = _ventaService.GetFormasPagoSelectList();
            model.Bancos = _ventaService.GetBancosSelectList();
            model.TipoTarjetaOptions = _ventaService.GetTipoTarjetaSelectList();
            model.CuotasOptions = _ventaService.GetCuotasSelectList();
            model.EntidadesElectronicas = _ventaService.GetEntidadesElectronicasSelectList();
            model.PlanesFinanciamiento = _ventaService.GetPlanesFinanciamientoSelectList();
        }

        private bool ValidarFormaPago(VentaFormViewModel model)
        {
            bool isValid = true;

            // Solo validar campos específicos según la forma de pago seleccionada
            switch (model.FormaPagoID)
            {
                case 2: // Tarjeta de Crédito
                        if (string.IsNullOrEmpty(model.TipoTarjeta))
                    {
                        ModelState.AddModelError(nameof(model.TipoTarjeta),
                            "Debe seleccionar un tipo de tarjeta para pagos con tarjeta de crédito.");
                        isValid = false;
                    }
                    if (!model.Cuotas.HasValue || model.Cuotas.Value <= 0)
                    {
                        ModelState.AddModelError(nameof(model.Cuotas),
                            "Debe especificar el número de cuotas.");
                        isValid = false;
                    }
                    // Limpiar validaciones de otros métodos de pago
                    ModelState.Remove(nameof(model.EntidadElectronica));
                    ModelState.Remove(nameof(model.PlanFinanciamiento));
                    break;

                case 5: // Pago Virtual
                    if (string.IsNullOrEmpty(model.EntidadElectronica))
                    {
                        ModelState.AddModelError(nameof(model.EntidadElectronica),
                            "Debe seleccionar una entidad electrónica para pagos virtuales.");
                        isValid = false;
                    }
                    // Limpiar validaciones de otros métodos de pago
                    ModelState.Remove(nameof(model.TipoTarjeta));
                    ModelState.Remove(nameof(model.Cuotas));
                    ModelState.Remove(nameof(model.PlanFinanciamiento));
                    break;

                case 6: // Crédito Personal
                    if (string.IsNullOrEmpty(model.PlanFinanciamiento))
                    {
                        ModelState.AddModelError(nameof(model.PlanFinanciamiento),
                            "Debe seleccionar un plan de financiamiento para crédito personal.");
                        isValid = false;
                    }
                    if (!model.Cuotas.HasValue || model.Cuotas.Value <= 0)
                    {
                        ModelState.AddModelError(nameof(model.Cuotas),
                            "Debe especificar el número de cuotas para crédito personal.");
                        isValid = false;
                    }
                    // Limpiar validaciones de otros métodos de pago
                    ModelState.Remove(nameof(model.TipoTarjeta));
                    ModelState.Remove(nameof(model.EntidadElectronica));
                    break;

                default: // Para otros métodos de pago (Contado, Débito, Transferencia)
                         // Limpiar todas las validaciones específicas
                    ModelState.Remove(nameof(model.TipoTarjeta));
                    ModelState.Remove(nameof(model.Cuotas));
                    ModelState.Remove(nameof(model.EntidadElectronica));
                    ModelState.Remove(nameof(model.PlanFinanciamiento));
                    break;
            }

            return isValid;
        }

        #endregion
    }
}