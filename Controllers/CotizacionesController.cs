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
    [Authorize(Policy = "PermisoPolitica")]
    public class CotizacionesController : BaseController
    {
        private readonly ICotizacionService _cotizacionService;
        private readonly IVentaService _ventaService;
        private readonly IMapper _mapper;
        private readonly IAuditoriaService _auditoriaService;

        public CotizacionesController(
            ICotizacionService cotizacionService,
            IVentaService ventaService,
            IMapper mapper,
            IAuditoriaService auditoriaService,
            ILogger<CotizacionesController> logger) : base(logger)
        {
            _cotizacionService = cotizacionService;
            _ventaService = ventaService;
            _mapper = mapper;
            _auditoriaService = auditoriaService;
        }

        // GET: Cotizaciones
        [HttpGet]
        [Authorize(Policy = "Permission:ventas.ver")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var cotizaciones = await _cotizacionService.GetAllCotizacionesAsync();
                var model = _mapper.Map<IEnumerable<VentaListViewModel>>(cotizaciones);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar lista de cotizaciones");
                return View("Error");
            }
        }

        // GET: Cotizaciones/Details/5
        [HttpGet]
        [Authorize(Policy = "Permission:ventas.ver")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var cotizacion = await _cotizacionService.GetCotizacionByIDAsync(id);
                if (cotizacion == null)
                    return NotFound();

                return View("Detalles", cotizacion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar detalles de cotización: {ID}", id);
                return View("Error");
            }
        }

        // POST: Cotizaciones/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:ventas.crear")]
        public async Task<IActionResult> Create(string cotizacionData)
        {
            try
            {
                if (string.IsNullOrEmpty(cotizacionData))
                {
                    TempData["Error"] = "No se recibieron datos válidos para crear la cotización.";
                    return RedirectToAction("Create", "Ventas");
                }

                // Deserializar el formulario serializado
                var formData = JsonSerializer.Deserialize<List<KeyValuePair<string, string>>>(cotizacionData);

                // Crear una nueva venta/cotización
                var cotizacion = new Venta
                {
                    FechaVenta = DateTime.Now,
                    Usuario = User.Identity?.Name ?? "Desconocido",
                    Vendedor = User.Identity?.Name ?? "Desconocido",
                    NumeroFactura = $"COT-{DateTime.Now:yyyyMMdd}-",  // Se completará en el servicio
                    Estado = EstadoVenta.Borrador,
                    ProductosPresupuesto = new List<DetalleVenta>()
                };

                // Extraer datos del formulario
                foreach (var item in formData)
                {
                    switch (item.Key)
                    {
                        case "NombreCliente":
                            cotizacion.NombreCliente = item.Value;
                            break;
                        case "DniCliente":
                            if (int.TryParse(item.Value, out int dni))
                                cotizacion.DniCliente = dni;
                            break;
                        case "TelefonoCliente":
                            cotizacion.TelefonoCliente = item.Value;
                            break;
                        case "DomicilioCliente":
                            cotizacion.DomicilioCliente = item.Value;
                            break;
                        case "LocalidadCliente":
                            cotizacion.LocalidadCliente = item.Value;
                            break;
                        case "CelularCliente":
                            cotizacion.CelularCliente = item.Value;
                            break;
                        case "Observaciones":
                            cotizacion.Observaciones = item.Value;
                            break;
                        case "Condiciones":
                            cotizacion.Condiciones = item.Value;
                            break;
                    }
                }

                // Extraer productos
                var productos = new List<DetalleVenta>();
                var index = 0;
                while (true)
                {
                    string productoIdKey = $"ProductosPresupuesto[{index}].ProductoID";
                    string cantidadKey = $"ProductosPresupuesto[{index}].Cantidad";

                    var productoIdItem = formData.FirstOrDefault(i => i.Key == productoIdKey);
                    var cantidadItem = formData.FirstOrDefault(i => i.Key == cantidadKey);

                    if (productoIdItem.Key == null || cantidadItem.Key == null)
                        break;

                    if (int.TryParse(productoIdItem.Value, out int productoId) &&
                        int.TryParse(cantidadItem.Value, out int cantidad))
                    {
                        var producto = new DetalleVenta
                        {
                            ProductoID = productoId,
                            Cantidad = cantidad,
                            NombreProducto = formData.FirstOrDefault(i => i.Key == $"ProductosPresupuesto[{index}].NombreProducto").Value ?? "",
                            CodigoAlfa = formData.FirstOrDefault(i => i.Key == $"ProductosPresupuesto[{index}].CodigoAlfa").Value ?? "",
                            CodigoBarra = formData.FirstOrDefault(i => i.Key == $"ProductosPresupuesto[{index}].CodigoBarra").Value ?? "",
                            Marca = formData.FirstOrDefault(i => i.Key == $"ProductosPresupuesto[{index}].Marca").Value ?? ""
                        };

                        if (decimal.TryParse(formData.FirstOrDefault(i => i.Key == $"ProductosPresupuesto[{index}].PrecioUnitario").Value, out decimal precioUnit))
                            producto.PrecioUnitario = precioUnit;

                        if (decimal.TryParse(formData.FirstOrDefault(i => i.Key == $"ProductosPresupuesto[{index}].PrecioTotal").Value, out decimal precioTotal))
                            producto.PrecioTotal = precioTotal;

                        if (decimal.TryParse(formData.FirstOrDefault(i => i.Key == $"ProductosPresupuesto[{index}].PrecioLista").Value, out decimal precioLista))
                            producto.PrecioLista = precioLista;

                        productos.Add(producto);
                    }

                    index++;
                }

                cotizacion.ProductosPresupuesto = productos;
                cotizacion.TotalProductos = productos.Sum(p => p.Cantidad);
                cotizacion.PrecioTotal = productos.Sum(p => p.PrecioTotal);

                // Guardar la cotización
                await _cotizacionService.CreateCotizacionAsync(cotizacion);

                // Registrar en auditoría
                await _auditoriaService.RegistrarCambioAsync(new AuditoriaRegistro
                {
                    FechaHora = DateTime.Now,
                    Usuario = User.Identity?.Name ?? "Desconocido",
                    Entidad = "Cotizacion",
                    Accion = "Create",
                    LlavePrimaria = cotizacion.VentaID.ToString(),
                    Detalle = $"Cliente={cotizacion.NombreCliente}, Total={cotizacion.PrecioTotal}"
                });

                TempData["Success"] = "Cotización creada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear cotización");
                TempData["Error"] = "Ocurrió un error al crear la cotización.";
                return RedirectToAction("Create", "Ventas");
            }
        }

        // GET: Cotizaciones/Delete/5
        [HttpGet]
        [Authorize(Policy = "Permission:ventas.eliminar")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var cotizacion = await _cotizacionService.GetCotizacionByIDAsync(id);
                if (cotizacion == null)
                    return NotFound();

                var model = _mapper.Map<VentaListViewModel>(cotizacion);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar cotización para eliminación: {ID}", id);
                return View("Error");
            }
        }

        // POST: Cotizaciones/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:ventas.eliminar")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var cotizacion = await _cotizacionService.GetCotizacionByIDAsync(id);
                if (cotizacion == null)
                    return NotFound();

                await _cotizacionService.DeleteCotizacionAsync(id);

                // Registrar en auditoría
                await _auditoriaService.RegistrarCambioAsync(new AuditoriaRegistro
                {
                    FechaHora = DateTime.Now,
                    Usuario = User.Identity?.Name ?? "Desconocido",
                    Entidad = "Cotizacion",
                    Accion = "Delete",
                    LlavePrimaria = id.ToString(),
                    Detalle = $"Cotización eliminada: Cliente={cotizacion.NombreCliente}, Total={cotizacion.PrecioTotal}"
                });

                TempData["Success"] = "Cotización eliminada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar cotización: {ID}", id);
                TempData["Error"] = "Ocurrió un error al eliminar la cotización.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Cotizaciones/Print/5
        [HttpGet]
        [Authorize(Policy = "Permission:ventas.ver")]
        public async Task<IActionResult> Print(int id)
        {
            try
            {
                var cotizacion = await _cotizacionService.GetCotizacionByIDAsync(id);
                if (cotizacion == null)
                    return NotFound();

                // Registrar en auditoría
                await _auditoriaService.RegistrarCambioAsync(new AuditoriaRegistro
                {
                    FechaHora = DateTime.Now,
                    Usuario = User.Identity?.Name ?? "Desconocido",
                    Entidad = "Cotizacion",
                    Accion = "Print",
                    LlavePrimaria = id.ToString(),
                    Detalle = $"Impresión de cotización: {cotizacion.NumeroFactura}"
                });

                return View(cotizacion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al imprimir cotización: {ID}", id);
                return View("Error");
            }
        }

        // GET: Cotizaciones/ConvertirAVenta/5
        [HttpGet]
        [Authorize(Policy = "Permission:ventas.crear")]
        public async Task<IActionResult> ConvertirAVenta(int id)
        {
            try
            {
                var cotizacion = await _cotizacionService.GetCotizacionByIDAsync(id);
                if (cotizacion == null)
                    return NotFound();

                // Guardar ID en TempData para ser utilizado por VentasController
                TempData["CotizacionID"] = id;

                // Registrar en auditoría
                await _auditoriaService.RegistrarCambioAsync(new AuditoriaRegistro
                {
                    FechaHora = DateTime.Now,
                    Usuario = User.Identity?.Name ?? "Desconocido",
                    Entidad = "Cotizacion",
                    Accion = "ConvertirAVenta",
                    LlavePrimaria = id.ToString(),
                    Detalle = $"Conversión de cotización a venta: {cotizacion.NumeroFactura}"
                });

                return RedirectToAction("CreateFromCotizacion", "Ventas");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al convertir cotización a venta: {ID}", id);
                TempData["Error"] = "Ocurrió un error al convertir la cotización a venta.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}