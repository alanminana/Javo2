using AutoMapper;
using Javo2.Controllers.Base;
using Javo2.IServices;
using Javo2.Models;
using Javo2.ViewModels.Operaciones.Ventas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Javo2.Controllers
{
    [Authorize]
    public class CotizacionesController : BaseController
    {
        private readonly ICotizacionService _cotizacionService;
        private readonly IClienteService _clienteService;
        private readonly IProductoService _productoService;
        private readonly IMapper _mapper;
        private readonly IAuditoriaService _auditoriaService;

        public CotizacionesController(
            ICotizacionService cotizacionService,
            IClienteService clienteService,
            IProductoService productoService,
            IMapper mapper,
            IAuditoriaService auditoriaService,
            ILogger<CotizacionesController> logger) : base(logger)
        {
            _cotizacionService = cotizacionService;
            _clienteService = clienteService;
            _productoService = productoService;
            _mapper = mapper;
            _auditoriaService = auditoriaService;
        }

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
                _logger.LogError(ex, "Error al obtener las cotizaciones");
                return View("Error");
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
                {
                    return NotFound();
                }

                var model = _mapper.Map<VentaFormViewModel>(cotizacion);
                // Cargar datos adicionales si es necesario
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los detalles de la cotización {ID}", id);
                return View("Error");
            }
        }

        [HttpPost]
        [Authorize(Policy = "Permission:ventas.crear")]
        public async Task<IActionResult> Create(string cotizacionData)
        {
            try
            {
                if (string.IsNullOrEmpty(cotizacionData))
                {
                    return BadRequest("No se recibieron datos para la cotización");
                }

                // Deserializar los datos de la cotización
                var formValues = JsonConvert.DeserializeObject<List<FormValue>>(cotizacionData);
                if (formValues == null)
                {
                    return BadRequest("Formato de datos inválido");
                }

                // Crear objeto de cotización
                var cotizacion = new Venta
                {
                    FechaVenta = DateTime.Now,
                    NumeroFactura = await _cotizacionService.GenerarNumeroCotizacionAsync(),
                    Usuario = User.Identity?.Name ?? "Desconocido",
                    Vendedor = User.Identity?.Name ?? "Desconocido",
                    Estado = EstadoVenta.Borrador, // Las cotizaciones siempre quedan en estado Borrador
                    ProductosPresupuesto = new List<DetalleVenta>()
                };

                // Procesar datos del cliente
                cotizacion.NombreCliente = GetFormValue(formValues, "NombreCliente") ?? "Cliente sin nombre";
                cotizacion.DniCliente = int.TryParse(GetFormValue(formValues, "DniCliente"), out int dni) ? dni : 0;
                cotizacion.TelefonoCliente = GetFormValue(formValues, "TelefonoCliente") ?? "";
                cotizacion.DomicilioCliente = GetFormValue(formValues, "DomicilioCliente") ?? "";
                cotizacion.LocalidadCliente = GetFormValue(formValues, "LocalidadCliente") ?? "";
                cotizacion.CelularCliente = GetFormValue(formValues, "CelularCliente") ?? "";

                // Procesar productos
                var productosDict = new Dictionary<string, string>();
                foreach (var item in formValues.Where(v => v.Name.StartsWith("ProductosPresupuesto[")))
                {
                    productosDict[item.Name] = item.Value;
                }

                // Agrupar por índice
                Dictionary<int, Dictionary<string, string>> productosPorIndice = new();
                foreach (var kvp in productosDict)
                {
                    // Extraer índice del nombre del campo: ProductosPresupuesto[0].ProductoID -> 0
                    var indexStart = kvp.Key.IndexOf('[') + 1;
                    var indexEnd = kvp.Key.IndexOf(']');
                    if (indexStart > 0 && indexEnd > indexStart)
                    {
                        var indexStr = kvp.Key.Substring(indexStart, indexEnd - indexStart);
                        if (int.TryParse(indexStr, out int index))
                        {
                            if (!productosPorIndice.ContainsKey(index))
                            {
                                productosPorIndice[index] = new Dictionary<string, string>();
                            }

                            // Extraer nombre de la propiedad: ProductosPresupuesto[0].ProductoID -> ProductoID
                            var propName = kvp.Key.Substring(indexEnd + 2); // +2 para saltar ].
                            productosPorIndice[index][propName] = kvp.Value;
                        }
                    }
                }

                // Crear objetos DetalleVenta
                foreach (var productoData in productosPorIndice.Values)
                {
                    if (int.TryParse(productoData.GetValueOrDefault("ProductoID"), out int productoID) && productoID > 0)
                    {
                        var detalle = new DetalleVenta
                        {
                            ProductoID = productoID,
                            NombreProducto = productoData.GetValueOrDefault("NombreProducto") ?? "",
                            CodigoBarra = productoData.GetValueOrDefault("CodigoBarra") ?? "",
                            CodigoAlfa = productoData.GetValueOrDefault("CodigoAlfa") ?? "",
                            Marca = productoData.GetValueOrDefault("Marca") ?? "",
                            Cantidad = int.TryParse(productoData.GetValueOrDefault("Cantidad"), out int cantidad) ? cantidad : 1,
                            PrecioUnitario = decimal.TryParse(productoData.GetValueOrDefault("PrecioUnitario"), out decimal precio) ? precio : 0,
                            PrecioTotal = decimal.TryParse(productoData.GetValueOrDefault("PrecioTotal"), out decimal total) ? total : 0,
                            PrecioLista = decimal.TryParse(productoData.GetValueOrDefault("PrecioLista"), out decimal lista) ? lista : 0
                        };

                        cotizacion.ProductosPresupuesto.Add(detalle);
                    }
                }

                // Calcular totales
                cotizacion.TotalProductos = cotizacion.ProductosPresupuesto.Sum(p => p.Cantidad);
                cotizacion.PrecioTotal = cotizacion.ProductosPresupuesto.Sum(p => p.PrecioTotal);

                // Guardar cotización
                await _cotizacionService.CreateCotizacionAsync(cotizacion);

                // Registrar en auditoría
                await _auditoriaService.RegistrarCambioAsync(new AuditoriaRegistro
                {
                    FechaHora = DateTime.Now,
                    Usuario = User.Identity?.Name ?? "Desconocido",
                    Entidad = "Cotizacion",
                    Accion = "Create",
                    LlavePrimaria = cotizacion.VentaID.ToString(),
                    Detalle = $"Cotización creada para {cotizacion.NombreCliente}, Total: {cotizacion.PrecioTotal:C}"
                });

                return RedirectToAction(nameof(Print), new { id = cotizacion.VentaID });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear cotización");
                return View("Error");
            }
        }

        [HttpGet]
        [Authorize(Policy = "Permission:ventas.ver")]
        public async Task<IActionResult> Print(int id)
        {
            try
            {
                var cotizacion = await _cotizacionService.GetCotizacionByIDAsync(id);
                if (cotizacion == null)
                {
                    return NotFound();
                }

                var model = _mapper.Map<VentaFormViewModel>(cotizacion);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al imprimir cotización {ID}", id);
                return View("Error");
            }
        }

        [HttpGet]
        [Authorize(Policy = "Permission:ventas.editar")]
        public async Task<IActionResult> ConvertirAVenta(int id)
        {
            try
            {
                var cotizacion = await _cotizacionService.GetCotizacionByIDAsync(id);
                if (cotizacion == null)
                {
                    return NotFound();
                }

                // Redirigir al controlador de ventas para crear una nueva venta con base en la cotización
                TempData["CotizacionID"] = id;
                return RedirectToAction("CreateFromCotizacion", "Ventas");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al convertir cotización {ID} a venta", id);
                return View("Error");
            }
        }

        [HttpGet]
        [Authorize(Policy = "Permission:ventas.eliminar")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var cotizacion = await _cotizacionService.GetCotizacionByIDAsync(id);
                if (cotizacion == null)
                {
                    return NotFound();
                }

                var model = _mapper.Map<VentaListViewModel>(cotizacion);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar cotización {ID} para eliminar", id);
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

                // Registrar en auditoría
                await _auditoriaService.RegistrarCambioAsync(new AuditoriaRegistro
                {
                    FechaHora = DateTime.Now,
                    Usuario = User.Identity?.Name ?? "Desconocido",
                    Entidad = "Cotizacion",
                    Accion = "Delete",
                    LlavePrimaria = id.ToString(),
                    Detalle = "Cotización eliminada"
                });

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar cotización {ID}", id);
                return View("Error");
            }
        }

        // Utilidad para obtener valor de un formulario
        private string? GetFormValue(List<FormValue> formValues, string name)
        {
            return formValues.FirstOrDefault(v => v.Name == name)?.Value;
        }

        // Clase para deserializar los valores del formulario
        public class FormValue
        {
            public string Name { get; set; } = "";
            public string Value { get; set; } = "";
        }
    }
}