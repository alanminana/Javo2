using AutoMapper;
using Javo2.Controllers.Base;
using Javo2.IServices;
using Javo2.Models;
using Javo2.ViewModels.Operaciones.Ventas;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Javo2.Controllers
{
    public class VentasController : BaseController
    {
        private readonly IVentaService _ventaService;
        private readonly IMapper _mapper;
        private readonly IClienteService _clienteService;
        private readonly IProductoService _productoService;
        private readonly IAuditoriaService _auditoriaService;

        public VentasController(
            IVentaService ventaService,
            IMapper mapper,
            ILogger<VentasController> logger,
            IClienteService clienteService,
            IProductoService productoService,
            IAuditoriaService auditoriaService
        ) : base(logger)
        {
            _ventaService = ventaService;
            _mapper = mapper;
            _clienteService = clienteService;
            _productoService = productoService;
            _auditoriaService = auditoriaService;
        }

        // GET: Ventas/Index
        [HttpGet]
        public async Task<IActionResult> Index(VentaFilterDto filter)
        {
            try
            {
                _logger.LogInformation("Index GET => Filtro: {@Filter}", filter);

                var ventas = await _ventaService.GetVentasAsync(filter);
                var model = _mapper.Map<IEnumerable<VentaListViewModel>>(ventas);

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la lista de ventas");
                return View("Error");
            }
        }

        // GET: Ventas/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                _logger.LogInformation("Create GET => Inicializando formulario de venta");

                var viewModel = new VentaFormViewModel
                {
                    FechaVenta = DateTime.Today,
                    NumeroFactura = await _ventaService.GenerarNumeroFacturaAsync(),
                    Usuario = User.Identity?.Name ?? "Desconocido",
                    Vendedor = User.Identity?.Name ?? "Desconocido",
                    ProductosPresupuesto = new List<DetalleVentaViewModel>(),
                    Estado = EstadoVenta.Borrador.ToString()
                };

                await CargarCombosAsync(viewModel);
                return View("Form", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al inicializar el formulario de venta");
                return View("Error");
            }
        }

        // POST: Ventas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VentaFormViewModel model, string Finalizar)
        {
            try
            {
                _logger.LogInformation("Create POST => Finalizar={Finalizar}, Model={@Model}", Finalizar, model);

                // Limpiar errores de ModelState para campos que no aplican según la forma de pago
                if (model.FormaPagoID != 2) // Si no es tarjeta de crédito
                {
                    ModelState.Remove(nameof(model.TipoTarjeta));
                    ModelState.Remove(nameof(model.Cuotas));
                }
                if (model.FormaPagoID != 5) // Si no es pago virtual
                {
                    ModelState.Remove(nameof(model.EntidadElectronica));
                }
                if (model.FormaPagoID != 6) // Si no es crédito personal
                {
                    ModelState.Remove(nameof(model.PlanFinanciamiento));
                }

                // Limpiar errores de campos opcionales en productos
                for (int i = 0; i < model.ProductosPresupuesto.Count; i++)
                {
                    ModelState.Remove($"ProductosPresupuesto[{i}].Marca");
                    ModelState.Remove($"ProductosPresupuesto[{i}].CodigoAlfa");
                    ModelState.Remove($"ProductosPresupuesto[{i}].CodigoBarra");
                }

                // Validación personalizada para campos condicionales
                if (!ValidarFormaPago(model))
                {
                    await CargarCombosAsync(model);
                    return View("Form", model);
                }

                // Asegurarse de que hay al menos un producto
                if (model.ProductosPresupuesto == null || !model.ProductosPresupuesto.Any())
                {
                    ModelState.AddModelError(string.Empty, "Debe agregar al menos un producto.");
                    await CargarCombosAsync(model);
                    return View("Form", model);
                }

                if (!ModelState.IsValid)
                {
                    LogModelStateErrors();
                    await CargarCombosAsync(model);
                    return View("Form", model);
                }

                // Mapear y configurar la venta
                var venta = _mapper.Map<Venta>(model);
                venta.Usuario = User.Identity?.Name ?? "Desconocido";
                venta.Vendedor = User.Identity?.Name ?? "Desconocido";
                venta.FechaVenta = DateTime.Today;

                // Calcular totales
                venta.TotalProductos = venta.ProductosPresupuesto.Sum(p => p.Cantidad);
                venta.PrecioTotal = venta.ProductosPresupuesto.Sum(p => p.PrecioTotal);

                // Determinar estado basado en el botón presionado
                venta.Estado = !string.IsNullOrEmpty(Finalizar) && Finalizar.Equals("true", StringComparison.OrdinalIgnoreCase)
                    ? EstadoVenta.PendienteDeAutorizacion
                    : EstadoVenta.Borrador;

                await _ventaService.CreateVentaAsync(venta);

                // Registrar auditoría
                await _auditoriaService.RegistrarCambioAsync(new AuditoriaRegistro
                {
                    FechaHora = DateTime.Now,
                    Usuario = User.Identity?.Name ?? "Desconocido",
                    Entidad = "Venta",
                    Accion = "Create",
                    LlavePrimaria = venta.VentaID.ToString(),
                    Detalle = $"Cliente={venta.NombreCliente}, Total={venta.PrecioTotal}, Estado={venta.Estado}"
                });

                TempData["Success"] = "Venta creada exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear la venta");
                ModelState.AddModelError(string.Empty, "Ocurrió un error al crear la venta.");
                await CargarCombosAsync(model);
                return View("Form", model);
            }
        }

        // GET: Ventas/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                _logger.LogInformation("Edit GET => VentaID={ID}", id);

                var venta = await _ventaService.GetVentaByIDAsync(id);
                if (venta == null)
                {
                    return NotFound();
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
        public async Task<IActionResult> Edit(VentaFormViewModel model)
        {
            try
            {
                _logger.LogInformation("Edit POST => VentaID={ID}", model.VentaID);

                // Limpiar errores de ModelState para campos que no aplican según la forma de pago
                if (model.FormaPagoID != 2) // Si no es tarjeta de crédito
                {
                    ModelState.Remove(nameof(model.TipoTarjeta));
                    ModelState.Remove(nameof(model.Cuotas));
                }
                if (model.FormaPagoID != 5) // Si no es pago virtual
                {
                    ModelState.Remove(nameof(model.EntidadElectronica));
                }
                if (model.FormaPagoID != 6) // Si no es crédito personal
                {
                    ModelState.Remove(nameof(model.PlanFinanciamiento));
                }

                // Limpiar errores de campos opcionales en productos
                for (int i = 0; i < model.ProductosPresupuesto.Count; i++)
                {
                    ModelState.Remove($"ProductosPresupuesto[{i}].Marca");
                    ModelState.Remove($"ProductosPresupuesto[{i}].CodigoAlfa");
                    ModelState.Remove($"ProductosPresupuesto[{i}].CodigoBarra");
                }

                if (!ValidarFormaPago(model))
                {
                    await CargarCombosAsync(model);
                    return View("Form", model);
                }

                if (!ModelState.IsValid)
                {
                    LogModelStateErrors();
                    await CargarCombosAsync(model);
                    return View("Form", model);
                }

                var venta = _mapper.Map<Venta>(model);

                // Recalcular totales
                venta.TotalProductos = venta.ProductosPresupuesto.Sum(p => p.Cantidad);
                venta.PrecioTotal = venta.ProductosPresupuesto.Sum(p => p.PrecioTotal);

                await _ventaService.UpdateVentaAsync(venta);

                // Registrar auditoría
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

        // GET: Ventas/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                _logger.LogInformation("Details GET => VentaID={ID}", id);

                var venta = await _ventaService.GetVentaByIDAsync(id);
                if (venta == null)
                {
                    return NotFound();
                }

                var model = _mapper.Map<VentaListViewModel>(venta);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los detalles de la venta");
                return View("Error");
            }
        }

        // GET: Ventas/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _logger.LogInformation("Delete GET => VentaID={ID}", id);

                var venta = await _ventaService.GetVentaByIDAsync(id);
                if (venta == null)
                {
                    return NotFound();
                }

                var model = _mapper.Map<VentaListViewModel>(venta);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar la venta para eliminación");
                return View("Error");
            }
        }

        // POST: Ventas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                _logger.LogInformation("DeleteConfirmed POST => VentaID={ID}", id);

                var venta = await _ventaService.GetVentaByIDAsync(id);
                if (venta == null)
                {
                    return NotFound();
                }

                await _ventaService.DeleteVentaAsync(id);

                // Registrar auditoría
                await _auditoriaService.RegistrarCambioAsync(new AuditoriaRegistro
                {
                    FechaHora = DateTime.Now,
                    Usuario = User.Identity?.Name ?? "Desconocido",
                    Entidad = "Venta",
                    Accion = "Delete",
                    LlavePrimaria = id.ToString(),
                    Detalle = $"Eliminada venta: Cliente={venta.NombreCliente}, Total={venta.PrecioTotal}"
                });

                TempData["Success"] = "Venta eliminada exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar la venta");
                TempData["Error"] = "Ocurrió un error al eliminar la venta.";
                return RedirectToAction(nameof(Index));
            }
        }

        #region Métodos AJAX

        [HttpPost]
        public async Task<IActionResult> BuscarClientePorDNI(int dni)
        {
            try
            {
                _logger.LogInformation("BuscarClientePorDNI => DNI={Dni}", dni);

                var cliente = await _clienteService.GetClienteByDNIAsync(dni);
                if (cliente == null)
                {
                    return Json(new { success = false, message = "Cliente no encontrado con ese DNI." });
                }

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
                        saldoDisponible = cliente.SaldoDisponible
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
        public async Task<IActionResult> BuscarProducto(string codigoProducto)
        {
            try
            {
                _logger.LogInformation("BuscarProducto => Código={Codigo}", codigoProducto);

                var producto = await _productoService.GetProductoByCodigoAsync(codigoProducto);
                if (producto == null)
                {
                    return Json(new { success = false, message = "Producto no encontrado." });
                }

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
                        precioTotal = producto.PContado
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

        #region Flujo de Autorización y Entrega

        // GET: Ventas/Autorizaciones
        [HttpGet]
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
        public async Task<IActionResult> Autorizar(int id)
        {
            try
            {
                _logger.LogInformation("Autorizar POST => VentaID={ID}", id);

                await _ventaService.AutorizarVentaAsync(id, User.Identity?.Name ?? "Desconocido");

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
        public async Task<IActionResult> Rechazar(int id)
        {
            try
            {
                _logger.LogInformation("Rechazar POST => VentaID={ID}", id);

                await _ventaService.RechazarVentaAsync(id, User.Identity?.Name ?? "Desconocido");

                return Json(new { success = true, message = "Venta rechazada correctamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al rechazar venta ID: {ID}", id);
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Ventas/EntregaProductos
        [HttpGet]
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
public async Task<IActionResult> MarcarEntregada(int id)
{
    try
    {
        _logger.LogInformation("MarcarEntregada POST => VentaID={ID}", id);

        await _ventaService.MarcarVentaComoEntregadaAsync(id, User.Identity?.Name ?? "Desconocido");

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
        public async Task<IActionResult> Reimprimir(int id)
        {
            try
            {
                var venta = await _ventaService.GetVentaByIDAsync(id);
                if (venta == null)
                {
                    return NotFound();
                }

                // Aquí se implementaría la lógica para generar el PDF o la vista de impresión
                return View(venta);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al reimprimir venta ID: {ID}", id);
                return View("Error");
            }
        }

        #endregion

        #region Métodos Privados

        private async Task CargarCombosAsync(VentaFormViewModel model)
        {
            model.FormasPago = await ObtenerFormasPago();
            model.Bancos = await ObtenerBancos();
            model.TipoTarjetaOptions = await ObtenerTipoTarjetaOptions();
            model.CuotasOptions = await ObtenerCuotasOptions();
            model.EntidadesElectronicas = await ObtenerEntidadesElectronicas();
            model.PlanesFinanciamiento = await ObtenerPlanesFinanciamiento();
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
                    break;

                case 5: // Pago Virtual
                    if (string.IsNullOrEmpty(model.EntidadElectronica))
                    {
                        ModelState.AddModelError(nameof(model.EntidadElectronica),
                            "Debe seleccionar una entidad electrónica para pagos virtuales.");
                        isValid = false;
                    }
                    break;

                case 6: // Crédito Personal
                    if (string.IsNullOrEmpty(model.PlanFinanciamiento))
                    {
                        ModelState.AddModelError(nameof(model.PlanFinanciamiento),
                            "Debe seleccionar un plan de financiamiento para crédito personal.");
                        isValid = false;
                    }
                    break;
            }

            return isValid;
        }

        private async Task<IEnumerable<SelectListItem>> ObtenerFormasPago()
        {
            var formasPago = await _ventaService.GetFormasPagoAsync();
            return formasPago.Select(fp => new SelectListItem
            {
                Value = fp.FormaPagoID.ToString(),
                Text = fp.Nombre
            });
        }

        private async Task<IEnumerable<SelectListItem>> ObtenerBancos()
        {
            var bancos = await _ventaService.GetBancosAsync();
            return bancos.Select(b => new SelectListItem
            {
                Value = b.BancoID.ToString(),
                Text = b.Nombre
            });
        }

        private Task<IEnumerable<SelectListItem>> ObtenerTipoTarjetaOptions()
        {
            var tipos = new List<SelectListItem>
            {
                new SelectListItem { Value = "Visa", Text = "Visa" },
                new SelectListItem { Value = "MasterCard", Text = "MasterCard" },
                new SelectListItem { Value = "Amex", Text = "Amex" },
                new SelectListItem { Value = "Naranja", Text = "Naranja" },
                new SelectListItem { Value = "Cabal", Text = "Cabal" }
            };
            return Task.FromResult<IEnumerable<SelectListItem>>(tipos);
        }

        private Task<IEnumerable<SelectListItem>> ObtenerCuotasOptions()
        {
            var cuotas = new List<SelectListItem>();
            for (int i = 1; i <= 24; i++)
            {
                cuotas.Add(new SelectListItem { Value = i.ToString(), Text = $"{i} Cuota{(i > 1 ? "s" : "")}" });
            }
            return Task.FromResult<IEnumerable<SelectListItem>>(cuotas);
        }

        private Task<IEnumerable<SelectListItem>> ObtenerEntidadesElectronicas()
        {
            var entidades = new List<SelectListItem>
            {
                new SelectListItem { Value = "MercadoPago", Text = "MercadoPago" },
                new SelectListItem { Value = "Modo", Text = "Modo" },
                new SelectListItem { Value = "BIMO", Text = "BIMO" },
                new SelectListItem { Value = "Cuenta DNI", Text = "Cuenta DNI" },
                new SelectListItem { Value = "QR", Text = "QR" }
            };
            return Task.FromResult<IEnumerable<SelectListItem>>(entidades);
        }

        private Task<IEnumerable<SelectListItem>> ObtenerPlanesFinanciamiento()
        {
            var planes = new List<SelectListItem>
            {
                new SelectListItem { Value = "Plan 6 cuotas", Text = "Plan 6 cuotas" },
                new SelectListItem { Value = "Plan 12 cuotas", Text = "Plan 12 cuotas" },
                new SelectListItem { Value = "Plan 18 cuotas", Text = "Plan 18 cuotas" },
                new SelectListItem { Value = "Plan 24 cuotas", Text = "Plan 24 cuotas" },
                new SelectListItem { Value = "Plan 36 cuotas", Text = "Plan 36 cuotas" }
            };
            return Task.FromResult<IEnumerable<SelectListItem>>(planes);
        }

        #endregion
    }
}