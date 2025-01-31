using AutoMapper;
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
    public class VentasController : Controller
    {
        private readonly IVentaService _ventaService;
        private readonly IMapper _mapper;
        private readonly ILogger<VentasController> _logger;
        private readonly IClienteService _clienteService;
        private readonly IProductoService _productoService;

        // NUEVO: Para registrar auditoría de ventas
        private readonly IAuditoriaService _auditoriaService;

        public VentasController(
            IVentaService ventaService,
            IMapper mapper,
            ILogger<VentasController> logger,
            IClienteService clienteService,
            IProductoService productoService,
            IAuditoriaService auditoriaService    // <-- se inyecta la auditoría
        )
        {
            _ventaService = ventaService;
            _mapper = mapper;
            _logger = logger;
            _clienteService = clienteService;
            _productoService = productoService;
            _auditoriaService = auditoriaService; // <-- asignación
        }

        // GET: Ventas/Index
        [HttpGet]
        public async Task<IActionResult> Index(VentaFilterDto filter)
        {
            _logger.LogInformation("Entrando a Ventas/Index con filtro: {@Filter}", filter);

            var ventas = await _ventaService.GetVentasAsync(filter);
            var model = _mapper.Map<IEnumerable<VentaListViewModel>>(ventas);

            _logger.LogInformation("Se mapearon {Count} ventas al ViewModel", model.Count());
            return View(model);
        }

        // GET: Ventas/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            _logger.LogInformation("Entrando a Ventas/Create (GET)");

            var viewModel = new VentaFormViewModel
            {
                FechaVenta = DateTime.Now,
                NumeroFactura = await _ventaService.GenerarNumeroFacturaAsync(),
                Usuario = User.Identity?.Name ?? "Desconocido",
                Vendedor = User.Identity?.Name ?? "Desconocido",

                FormasPago = await ObtenerFormasPago(),
                Bancos = await ObtenerBancos(),
                TipoTarjetaOptions = await ObtenerTipoTarjetaOptions(),
                CuotasOptions = await ObtenerCuotasOptions(),
                EntidadesElectronicas = await ObtenerEntidadesElectronicas(),
                PlanesFinanciamiento = await ObtenerPlanesFinanciamiento()
            };

            _logger.LogInformation("VentaFormViewModel inicializado: {@ViewModel}", viewModel);
            return View("Form", viewModel);
        }

        // POST: Ventas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VentaFormViewModel model, string Finalizar)
        {
            _logger.LogInformation("[VentasController] Create POST => Finalizar={Finalizar}, Model={@Model}", Finalizar, model);

            // Validación condicional
            if (model.FormaPagoID == 2 && string.IsNullOrEmpty(model.TipoTarjeta))
            {
                ModelState.AddModelError(nameof(model.TipoTarjeta), "El campo 'TipoTarjeta' es requerido para Tarjeta de Crédito.");
            }
            if (model.FormaPagoID == 5 && string.IsNullOrEmpty(model.EntidadElectronica))
            {
                ModelState.AddModelError(nameof(model.EntidadElectronica), "El campo 'EntidadElectronica' es requerido para Pago Virtual.");
            }
            if (model.FormaPagoID == 6 && string.IsNullOrEmpty(model.PlanFinanciamiento))
            {
                ModelState.AddModelError(nameof(model.PlanFinanciamiento), "El campo 'PlanFinanciamiento' es requerido para Crédito Personal.");
            }

            if (!ModelState.IsValid)
            {
                // Recargar combos si falla la validación
                model.FormasPago = await ObtenerFormasPago();
                model.Bancos = await ObtenerBancos();
                model.TipoTarjetaOptions = await ObtenerTipoTarjetaOptions();
                model.CuotasOptions = await ObtenerCuotasOptions();
                model.EntidadesElectronicas = await ObtenerEntidadesElectronicas();
                model.PlanesFinanciamiento = await ObtenerPlanesFinanciamiento();

                // Log de cada error
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        _logger.LogWarning("Error en el campo {Field}: {Message}", state.Key, error.ErrorMessage);
                    }
                }

                return View("Form", model);
            }

            var venta = _mapper.Map<Venta>(model);
            venta.Usuario = User.Identity?.Name ?? "Desconocido";
            venta.Vendedor = User.Identity?.Name ?? "Desconocido";

            // Calcular total
            venta.PrecioTotal = venta.ProductosPresupuesto.Sum(p => p.PrecioTotal);
            _logger.LogInformation("[VentasController] PrecioTotal calculado: {Total}", venta.PrecioTotal);

            // Estado de la venta según si se finaliza
            if (!string.IsNullOrEmpty(Finalizar) && Finalizar.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                venta.Estado = EstadoVenta.PendienteDeAutorizacion;
                _logger.LogInformation("[VentasController] Venta finalizada => PendienteDeAutorizacion");
            }
            else
            {
                venta.Estado = EstadoVenta.Borrador;
                _logger.LogInformation("[VentasController] Venta en estado Borrador");
            }

            // Crear en servicio
            await _ventaService.CreateVentaAsync(venta);
            _logger.LogInformation("[VentasController] Venta creada con ID={ID}", venta.VentaID);

            // -----------------------------------
            // Registrar auditoría de la creación
            // -----------------------------------
            await _auditoriaService.RegistrarCambioAsync(new AuditoriaRegistro
            {
                FechaHora = DateTime.Now,
                Usuario = User.Identity?.Name ?? "Desconocido",
                Entidad = "Venta",
                Accion = "Create",
                LlavePrimaria = venta.VentaID.ToString(),
                Detalle = $"Cliente={venta.NombreCliente}, Total={venta.PrecioTotal}, Estado={venta.Estado}"
            });

            return RedirectToAction(nameof(Index));
        }

        // Nuevo ejemplo: Acción para “procesar” la venta (descontar stock, cambiar estado, etc.)
        [HttpPost]
        public async Task<IActionResult> Process(int id)
        {
            _logger.LogInformation("[VentasController] Process => ID={ID}", id);
            try
            {
                await _ventaService.ProcessVentaAsync(id);
                _logger.LogInformation("[VentasController] Venta ID={ID} procesada (stock descontado, etc.)", id);

                // Registrar auditoría
                await _auditoriaService.RegistrarCambioAsync(new AuditoriaRegistro
                {
                    FechaHora = DateTime.Now,
                    Usuario = User.Identity?.Name ?? "Desconocido",
                    Entidad = "Venta",
                    Accion = "Process",
                    LlavePrimaria = id.ToString(),
                    Detalle = "Venta completada y stock descontado"
                });

                TempData["Success"] = $"La venta {id} fue procesada correctamente.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[VentasController] Error al procesar la venta ID={ID}", id);
                TempData["Error"] = "No se pudo procesar la venta.";
            }

            return RedirectToAction(nameof(Index));
        }

        // ============== Acciones AJAX para buscar clientes/productos ==============
        [HttpPost]
        public async Task<IActionResult> BuscarClientePorDNI(int dni)
        {
            _logger.LogInformation("[VentasController] BuscarClientePorDNI => DNI={Dni}", dni);

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
                    calle = cliente.Calle,
                    localidad = cliente.Localidad,
                    celular = cliente.Celular,
                    limiteCredito = cliente.LimiteCreditoInicial,
                    saldo = cliente.Saldo,
                    saldoDisponible = cliente.SaldoDisponible
                }
            });
        }

        [HttpPost]
        public async Task<IActionResult> BuscarProducto(string codigoProducto)
        {
            _logger.LogInformation("[VentasController] BuscarProducto => codigoProducto={Codigo}", codigoProducto);

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

        [HttpPost]
        public async Task<IActionResult> BuscarProductoPorNombre(string nombreProducto)
        {
            _logger.LogInformation("[VentasController] BuscarProductoPorNombre => nombreProducto={Nombre}", nombreProducto);

            var prod = await _productoService.GetProductoByNombreAsync(nombreProducto);
            if (prod == null)
            {
                return Json(new { success = false, message = "No se encontró producto con ese nombre." });
            }

            return Json(new
            {
                success = true,
                data = new[] {
                    new {
                        productoID = prod.ProductoID,
                        codigoBarra = prod.CodigoBarra,
                        codigoAlfa = prod.CodigoAlfa,
                        nombreProducto = prod.Nombre,
                        marca = prod.Marca?.Nombre ?? "",
                        cantidad = 1,
                        precioUnitario = prod.PContado,
                        precioLista = prod.PLista,
                        precioTotal = prod.PContado
                    }
                }
            });
        }

        [HttpPost]
        public async Task<IActionResult> BuscarProductosPorRubro(string rubroProducto)
        {
            _logger.LogInformation("[VentasController] BuscarProductosPorRubro => rubro={Rubro}", rubroProducto);

            var productos = await _productoService.GetProductosByRubroAsync(rubroProducto);
            if (!productos.Any())
            {
                return Json(new { success = false, message = "No se encontraron productos para ese rubro." });
            }

            var dataList = productos.Select(p => new
            {
                productoID = p.ProductoID,
                codigoBarra = p.CodigoBarra,
                codigoAlfa = p.CodigoAlfa,
                nombreProducto = p.Nombre,
                marca = p.Marca?.Nombre ?? "",
                cantidad = 1,
                precioUnitario = p.PContado,
                precioLista = p.PLista,
                precioTotal = p.PContado
            }).ToList();

            return Json(new { success = true, data = dataList });
        }

        // Otras funciones auxiliares para combos
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

        private async Task<IEnumerable<SelectListItem>> ObtenerTipoTarjetaOptions()
        {
            var tipos = new List<SelectListItem>
            {
                new SelectListItem { Value = "Visa", Text = "Visa" },
                new SelectListItem { Value = "MasterCard", Text = "MasterCard" },
                new SelectListItem { Value = "Amex", Text = "Amex" },
            };
            return await Task.FromResult(tipos);
        }

        private async Task<IEnumerable<SelectListItem>> ObtenerCuotasOptions()
        {
            var list = new List<SelectListItem>();
            for (int i = 1; i <= 12; i++)
            {
                list.Add(new SelectListItem { Value = i.ToString(), Text = $"{i} Cuotas" });
            }
            return await Task.FromResult(list);
        }

        private async Task<IEnumerable<SelectListItem>> ObtenerEntidadesElectronicas()
        {
            var entidades = new List<SelectListItem>
            {
                new SelectListItem { Value = "PayPal", Text = "PayPal" },
                new SelectListItem { Value = "MercadoPago", Text = "MercadoPago" },
                new SelectListItem { Value = "Stripe", Text = "Stripe" },
            };
            return await Task.FromResult(entidades);
        }

        private async Task<IEnumerable<SelectListItem>> ObtenerPlanesFinanciamiento()
        {
            var planes = new List<SelectListItem>
            {
                new SelectListItem { Value = "Plan A", Text = "Plan A" },
                new SelectListItem { Value = "Plan B", Text = "Plan B" },
                new SelectListItem { Value = "Plan C", Text = "Plan C" },
            };
            return await Task.FromResult(planes);
        }

        // GET: Ventas/Reimprimir
        [HttpGet]
        public async Task<IActionResult> Reimprimir(int id)
        {
            var venta = await _ventaService.GetVentaByIDAsync(id);
            if (venta == null)
                return NotFound();

            // Lógica para reimprimir
            return View("Reimprimir", venta);
        }
    }
}
