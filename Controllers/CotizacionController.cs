// Controllers/CotizacionesController.cs
using AutoMapper;
using Javo2.Controllers.Base;
using Javo2.IServices;
using Javo2.Models;
using Javo2.ViewModels.Operaciones.Ventas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Javo2.Controllers
{
    [Authorize]
    public class CotizacionesController : BaseController
    {
        private readonly ICotizacionService _cotizacionService;
        private readonly IVentaService _ventaService;
        private readonly IProductoService _productoService;
        private readonly IClienteService _clienteService;
        private readonly IMapper _mapper;

        public CotizacionesController(
            ICotizacionService cotizacionService,
            IVentaService ventaService,
            IProductoService productoService,
            IClienteService clienteService,
            IMapper mapper,
            ILogger<CotizacionesController> logger) : base(logger)
        {
            _cotizacionService = cotizacionService;
            _ventaService = ventaService;
            _productoService = productoService;
            _clienteService = clienteService;
            _mapper = mapper;
        }

        // GET: Cotizaciones
        [HttpGet]
        [Authorize(Policy = "Permission:ventas.ver")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var cotizaciones = await _cotizacionService.GetAllCotizacionesAsync();
                var viewModel = _mapper.Map<IEnumerable<CotizacionListViewModel>>(cotizaciones);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cotizaciones");
                return View("Error");
            }
        }

        // GET: Cotizaciones/Create
        [HttpGet]
        [Authorize(Policy = "Permission:ventas.crear")]
        public async Task<IActionResult> Create()
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

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al inicializar formulario de cotización");
                return View("Error");
            }
        }

        // POST: Cotizaciones/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:ventas.crear")]
        public async Task<IActionResult> Create(CotizacionViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Mapear a modelo de dominio Cotizacion (no directamente a Venta)
                var cotizacion = _mapper.Map<Cotizacion>(model);
                cotizacion.FechaCotizacion = DateTime.Now;
                cotizacion.FechaVencimiento = DateTime.Now.AddDays(model.DiasVigencia);
                cotizacion.Usuario = User.Identity?.Name ?? "Sistema";

                // Calcular totales
                if (cotizacion.ProductosPresupuesto != null && cotizacion.ProductosPresupuesto.Any())
                {
                    cotizacion.TotalProductos = cotizacion.ProductosPresupuesto.Sum(p => p.Cantidad);
                    cotizacion.PrecioTotal = cotizacion.ProductosPresupuesto.Sum(p => p.PrecioTotal);
                }

                // Guardar - El servicio se encargará de la conversión a Venta si es necesario
                await _cotizacionService.CreateCotizacionAsync(cotizacion);

                TempData["Success"] = "Cotización creada exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear cotización");
                ModelState.AddModelError(string.Empty, "Error al crear la cotización: " + ex.Message);
                return View(model);
            }
        }

        // POST: Cotizaciones/CreateFromVenta
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:ventas.crear")]
        public async Task<IActionResult> CreateFromVenta(string cotizacionData)
        {
            try
            {
                if (string.IsNullOrEmpty(cotizacionData))
                {
                    return RedirectToAction(nameof(Create));
                }

                // Deserializar datos de formulario de venta
                var formData = JsonSerializer.Deserialize<Dictionary<string, string>>(cotizacionData);

                // Crear cotización desde datos de venta
                var cotizacionViewModel = new CotizacionViewModel
                {
                    FechaCotizacion = DateTime.Today,
                    NumeroCotizacion = await _cotizacionService.GenerarNumeroCotizacionAsync(),
                    DiasVigencia = 15,
                    NombreCliente = formData.ContainsKey("NombreCliente") ? formData["NombreCliente"] : string.Empty,
                    DniCliente = formData.ContainsKey("DniCliente") ? int.Parse(formData["DniCliente"]) : 0,
                    TelefonoCliente = formData.ContainsKey("TelefonoCliente") ? formData["TelefonoCliente"] : string.Empty,
                    Observaciones = formData.ContainsKey("Observaciones") ? formData["Observaciones"] : string.Empty,
                    ProductosPresupuesto = new List<DetalleVentaViewModel>()
                };

                // Obtener productos de la venta
                // Esta parte requeriría una implementación más compleja para extraer 
                // productos del formulario serializado

                return View("Create", cotizacionViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear cotización desde venta");
                return View("Error");
            }
        }

        // GET: Cotizaciones/ConvertToVenta/5
        [HttpGet]
        [Authorize(Policy = "Permission:ventas.crear")]
        public async Task<IActionResult> ConvertToVenta(int id)
        {
            try
            {
                var cotizacion = await _cotizacionService.GetCotizacionByIDAsync(id);
                if (cotizacion == null)
                {
                    return NotFound();
                }

                // Guardar ID en TempData para VentasController
                TempData["CotizacionID"] = id;

                return RedirectToAction("CreateFromCotizacion", "Ventas");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al convertir cotización a venta");
                return View("Error");
            }
        }

        // Métodos para búsqueda de cliente y productos
        [HttpPost]
        [Authorize(Policy = "Permission:ventas.ver")]
        public async Task<IActionResult> BuscarClientePorDNI(int dni)
        {
            try
            {
                var cliente = await _clienteService.GetClienteByDNIAsync(dni);
                if (cliente == null)
                    return Json(new { success = false, message = "Cliente no encontrado" });

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        nombre = $"{cliente.Nombre} {cliente.Apellido}",
                        telefono = cliente.Telefono,
                        email = cliente.Email
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar cliente");
                return Json(new { success = false, message = "Error al buscar cliente" });
            }
        }

        [HttpPost]
        [Authorize(Policy = "Permission:ventas.ver")]
        public async Task<IActionResult> BuscarProducto(string codigoProducto)
        {
            try
            {
                var producto = await _productoService.GetProductoByCodigoAsync(codigoProducto);
                if (producto == null)
                    return Json(new { success = false, message = "Producto no encontrado" });

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
                        precioUnitario = producto.PContado,
                        precioLista = producto.PLista
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar producto");
                return Json(new { success = false, message = "Error al buscar producto" });
            }
        }
    }
}