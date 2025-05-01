using AutoMapper;
using Javo2.Controllers.Base;
using Javo2.IServices;
using Javo2.Models;
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
    public class CotizacionController : BaseController
    {
        private readonly ICotizacionService _cotizacionService;
        private readonly IVentaService _ventaService;
        private readonly IAuditoriaService _auditoriaService;
        private readonly IMapper _mapper;

        public CotizacionController(
            ICotizacionService cotizacionService,
            IVentaService ventaService,
            IAuditoriaService auditoriaService,
            IMapper mapper,
            ILogger<CotizacionController> logger) : base(logger)
        {
            _cotizacionService = cotizacionService;
            _ventaService = ventaService;
            _auditoriaService = auditoriaService;
            _mapper = mapper;
        }

        // GET: Cotizacion/Index
        [HttpGet]
        [Authorize(Policy = "Permission:ventas.ver")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var cotizaciones = await _cotizacionService.GetAllCotizacionesAsync();
                var model = _mapper.Map<IEnumerable<Javo2.ViewModels.Operaciones.Ventas.VentaListViewModel>>(cotizaciones);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la lista de cotizaciones");
                return View("Error");
            }
        }

        // GET: Cotizacion/Detalles/5
        [HttpGet]
        [Authorize(Policy = "Permission:ventas.ver")]
        public async Task<IActionResult> Detalles(int id)
        {
            try
            {
                var cotizacion = await _cotizacionService.GetCotizacionByIDAsync(id);
                if (cotizacion == null)
                    return NotFound();

                return View(cotizacion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener detalles de la cotización");
                return View("Error");
            }
        }

        // GET: Cotizacion/Print/5
        [HttpGet]
        [Authorize(Policy = "Permission:ventas.ver")]
        public async Task<IActionResult> Print(int id)
        {
            try
            {
                var cotizacion = await _cotizacionService.GetCotizacionByIDAsync(id);
                if (cotizacion == null)
                    return NotFound();

                return View(cotizacion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar impresión de cotización");
                return View("Error");
            }
        }

        // POST: Cotizacion/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:ventas.crear")]
        public async Task<IActionResult> Create(string cotizacionData)
        {
            try
            {
                // Deserializar los datos del formulario
                if (string.IsNullOrEmpty(cotizacionData))
                {
                    return BadRequest("No se proporcionaron datos para la cotización");
                }

                var formData = JsonSerializer.Deserialize<List<KeyValuePair<string, string>>>(cotizacionData);
                var venta = new Venta
                {
                    // Mapear los campos básicos
                    NumeroFactura = await _cotizacionService.GenerarNumeroCotizacionAsync(),
                    FechaVenta = DateTime.Now,
                    Usuario = User.Identity?.Name ?? "Desconocido",
                    Vendedor = User.Identity?.Name ?? "Desconocido",
                    Estado = EstadoVenta.Borrador
                };

                // Procesar los datos del formulario
                foreach (var item in formData)
                {
                    switch (item.Key)
                    {
                        case "NombreCliente":
                            venta.NombreCliente = item.Value;
                            break;
                        case "TelefonoCliente":
                            venta.TelefonoCliente = item.Value;
                            break;
                        case "DomicilioCliente":
                            venta.DomicilioCliente = item.Value;
                            break;
                        case "LocalidadCliente":
                            venta.LocalidadCliente = item.Value;
                            break;
                        case "CelularCliente":
                            venta.CelularCliente = item.Value;
                            break;
                        case "DniCliente":
                            if (int.TryParse(item.Value, out int dni))
                                venta.DniCliente = dni;
                            break;
                        case "FormaPagoID":
                            if (int.TryParse(item.Value, out int formaPagoId))
                                venta.FormaPagoID = formaPagoId;
                            break;
                        case "Observaciones":
                            venta.Observaciones = item.Value;
                            break;
                        case "Condiciones":
                            venta.Condiciones = item.Value;
                            break;
                    }
                }

                // Procesar productos
                venta.ProductosPresupuesto = new List<DetalleVenta>();
                for (int i = 0; i < formData.Count; i++)
                {
                    // Buscar si hay un elemento ProductosPresupuesto[i].ProductoID
                    var productoIdKey = $"ProductosPresupuesto[{i}].ProductoID";
                    var productoIdItem = formData.FirstOrDefault(x => x.Key == productoIdKey);
                    if (!string.IsNullOrEmpty(productoIdItem.Value))
                    {
                        if (int.TryParse(productoIdItem.Value, out int productoId))
                        {
                            var producto = new DetalleVenta
                            {
                                ProductoID = productoId
                            };

                            // Buscar otros campos del producto
                            foreach (var campo in new[] { "NombreProducto", "CodigoAlfa", "CodigoBarra", "Marca" })
                            {
                                var key = $"ProductosPresupuesto[{i}].{campo}";
                                var item = formData.FirstOrDefault(x => x.Key == key);
                                if (!string.IsNullOrEmpty(item.Value))
                                {
                                    switch (campo)
                                    {
                                        case "NombreProducto":
                                            producto.NombreProducto = item.Value;
                                            break;
                                        case "CodigoAlfa":
                                            producto.CodigoAlfa = item.Value;
                                            break;
                                        case "CodigoBarra":
                                            producto.CodigoBarra = item.Value;
                                            break;
                                        case "Marca":
                                            producto.Marca = item.Value;
                                            break;
                                    }
                                }
                            }

                            // Campos numéricos
                            foreach (var campo in new[] { "Cantidad", "PrecioUnitario", "PrecioTotal", "PrecioLista" })
                            {
                                var key = $"ProductosPresupuesto[{i}].{campo}";
                                var item = formData.FirstOrDefault(x => x.Key == key);
                                if (!string.IsNullOrEmpty(item.Value))
                                {
                                    if (decimal.TryParse(item.Value, out decimal valor))
                                    {
                                        switch (campo)
                                        {
                                            case "Cantidad":
                                                producto.Cantidad = (int)valor;
                                                break;
                                            case "PrecioUnitario":
                                                producto.PrecioUnitario = valor;
                                                break;
                                            case "PrecioTotal":
                                                producto.PrecioTotal = valor;
                                                break;
                                            case "PrecioLista":
                                                producto.PrecioLista = valor;
                                                break;
                                        }
                                    }
                                }
                            }

                            venta.ProductosPresupuesto.Add(producto);
                        }
                    }
                }

                // Calcular totales
                venta.TotalProductos = venta.ProductosPresupuesto.Sum(p => p.Cantidad);
                venta.PrecioTotal = venta.ProductosPresupuesto.Sum(p => p.PrecioTotal);

                // Guardar la cotización
                await _cotizacionService.CreateCotizacionAsync(venta);

                // Registrar en auditoría
                await _auditoriaService.RegistrarCambioAsync(new AuditoriaRegistro
                {
                    FechaHora = DateTime.Now,
                    Usuario = User.Identity?.Name ?? "Desconocido",
                    Entidad = "Cotizacion",
                    Accion = "Create",
                    LlavePrimaria = venta.VentaID.ToString(),
                    Detalle = $"Cotización creada: Cliente={venta.NombreCliente}, Total={venta.PrecioTotal}"
                });

                TempData["Success"] = "Cotización creada exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear cotización");
                TempData["Error"] = "Error al crear cotización: " + ex.Message;
                return RedirectToAction("Create", "Ventas");
            }
        }

        // GET: Cotizacion/ConvertirAVenta/5
        [HttpGet]
        [Authorize(Policy = "Permission:ventas.crear")]
        public async Task<IActionResult> ConvertirAVenta(int id)
        {
            try
            {
                var cotizacion = await _cotizacionService.GetCotizacionByIDAsync(id);
                if (cotizacion == null)
                    return NotFound();

                // Almacenar el ID de la cotización en TempData
                TempData["CotizacionID"] = id;

                // Redireccionar a la creación de venta desde cotización
                return RedirectToAction("CreateFromCotizacion", "Ventas");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al convertir cotización a venta");
                TempData["Error"] = "Error al convertir cotización a venta: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Cotizacion/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:ventas.eliminar")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _cotizacionService.DeleteCotizacionAsync(id);

                // Registrar en auditoría
                await _auditoriaService.RegistrarCambioAsync(new AuditoriaRegistro
                {
                    FechaHora = DateTime.Now,
                    Usuario = User.Identity?.Name ?? "Desconocido",
                    Entidad = "Cotizacion",
                    Accion = "Delete",
                    LlavePrimaria = id.ToString(),
                    Detalle = $"Cotización eliminada: ID={id}"
                });

                TempData["Success"] = "Cotización eliminada exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar cotización");
                TempData["Error"] = "Error al eliminar cotización: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}