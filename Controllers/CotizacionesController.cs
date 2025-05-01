// Controllers/CotizacionesController.cs
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
using System.Threading.Tasks;

namespace Javo2.Controllers
{
    [Authorize]
    public class CotizacionesController : BaseController
    {
        private readonly ICotizacionService _cotizacionService;
        private readonly IMapper _mapper;

        public CotizacionesController(
            ICotizacionService cotizacionService,
            IMapper mapper,
            ILogger<CotizacionesController> logger)
            : base(logger)
        {
            _cotizacionService = cotizacionService;
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
                var model = _mapper.Map<IEnumerable<VentaListViewModel>>(cotizaciones);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las cotizaciones");
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
                if (cotizacion == null) return NotFound();

                var model = _mapper.Map<VentaFormViewModel>(cotizacion);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la cotización");
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
                    return BadRequest("No se recibieron datos de cotización");
                }

                // Deserializar datos de formulario a diccionario
                var formData = JsonConvert.DeserializeObject<Dictionary<string, string>>(cotizacionData);

                // Convertir a modelo de Venta para cotización
                var cotizacion = new Venta
                {
                    FechaVenta = DateTime.Now,
                    NumeroFactura = await _cotizacionService.GenerarNumeroCotizacionAsync(),
                    Usuario = User.Identity?.Name ?? "Sistema",
                    Vendedor = User.Identity?.Name ?? "Sistema",

                    NombreCliente = formData["NombreCliente"],
                    TelefonoCliente = formData.ContainsKey("TelefonoCliente") ? formData["TelefonoCliente"] : string.Empty,
                    DomicilioCliente = formData.ContainsKey("DomicilioCliente") ? formData["DomicilioCliente"] : string.Empty,
                    LocalidadCliente = formData.ContainsKey("LocalidadCliente") ? formData["LocalidadCliente"] : string.Empty,
                    CelularCliente = formData.ContainsKey("CelularCliente") ? formData["CelularCliente"] : string.Empty,

                    Observaciones = formData.ContainsKey("Observaciones") ? formData["Observaciones"] : string.Empty,
                    Condiciones = formData.ContainsKey("Condiciones") ? formData["Condiciones"] : string.Empty,

                    ProductosPresupuesto = new List<DetalleVenta>(),

                    Estado = EstadoVenta.Borrador
                };

                // Procesar productos
                // (Extraer productos del formData y convertirlos a DetalleVenta)

                await _cotizacionService.CreateCotizacionAsync(cotizacion);

                TempData["Success"] = "Cotización creada correctamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear la cotización");
                TempData["Error"] = "Error al crear la cotización: " + ex.Message;
                return RedirectToAction("Create", "Ventas");
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
                if (cotizacion == null) return NotFound();

                var model = _mapper.Map<VentaFormViewModel>(cotizacion);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar la impresión de cotización");
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
                if (cotizacion == null) return NotFound();

                // Pasar el ID de cotización a TempData para que VentasController pueda leerlo
                TempData["CotizacionID"] = id;

                return RedirectToAction("CreateFromCotizacion", "Ventas");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al convertir cotización a venta");
                TempData["Error"] = "Error al convertir cotización a venta: " + ex.Message;
                return RedirectToAction(nameof(Index));
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
                if (cotizacion == null) return NotFound();

                var model = _mapper.Map<VentaListViewModel>(cotizacion);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar cotización para eliminar");
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
                await _cotizacionService.DeleteCotizacionAsync(id);
                TempData["Success"] = "Cotización eliminada correctamente";
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