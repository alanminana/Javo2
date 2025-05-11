using Javo2.IServices;
using Javo2.Models;
using Javo2.ViewModels.Operaciones.Productos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Javo2.Controllers
{
    [Authorize]
    public class AjustePreciosController : Controller
    {
        private readonly IAjustePrecioService _ajustePrecioService;
        private readonly IProductoService _productoService;
        private readonly IAuditoriaService _auditoriaService;
        private readonly IMapper _mapper;
        private readonly ILogger<AjustePreciosController> _logger;

        public AjustePreciosController(
            IAjustePrecioService ajustePrecioService,
            IProductoService productoService,
            IAuditoriaService auditoriaService,
            IMapper mapper,
            ILogger<AjustePreciosController> logger)
        {
            _ajustePrecioService = ajustePrecioService;
            _productoService = productoService;
            _auditoriaService = auditoriaService;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: AjustePrecios/Index
        [HttpGet]
        [Authorize(Policy = "Permission:productos.ajustarprecios")]
        public async Task<IActionResult> Index()
        {
            var historial = await _ajustePrecioService.ObtenerHistorialAjustesAsync();
            var viewModel = _mapper.Map<List<AjustePrecioHistoricoViewModel>>(historial);
            return View(viewModel);
        }

        // GET: AjustePrecios/Form
        [HttpGet]
        [Authorize(Policy = "Permission:productos.ajustarprecios")]
        public async Task<IActionResult> Form()
        {
            var productos = await _productoService.GetAllProductosAsync();
            var viewModel = new AjustePrecioFormViewModel
            {
                Productos = productos.Select(p => new ProductoAjusteViewModel
                {
                    ProductoID = p.ProductoID,
                    Nombre = p.Nombre,
                    PCosto = p.PCosto,
                    PContado = p.PContado,
                    PLista = p.PLista,
                    Seleccionado = false
                }).ToList()
            };
            return View(viewModel);
        }

        // POST: AjustePrecios/Form
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:productos.ajustarprecios")]
        public async Task<IActionResult> Form(AjustePrecioFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var productosIDs = model.Productos
                    .Where(p => p.Seleccionado)
                    .Select(p => p.ProductoID)
                    .ToList();

                if (!productosIDs.Any())
                {
                    ModelState.AddModelError("", "Debe seleccionar al menos un producto.");
                    return View(model);
                }

                var ajusteID = await _ajustePrecioService.AjustarPreciosAsync(
                    productosIDs,
                    model.Porcentaje,
                    model.EsAumento,
                    model.Descripcion);

                await _auditoriaService.RegistrarCambioAsync(new AuditoriaRegistro
                {
                    FechaHora = DateTime.Now,
                    Usuario = User.Identity?.Name ?? "Sistema",
                    Entidad = "AjustePrecio",
                    Accion = "CrearAjuste",
                    LlavePrimaria = ajusteID.ToString(),
                    Detalle = $"Ajuste permanente {(model.EsAumento ? "aumento" : "descuento")} del {model.Porcentaje}%"
                });

                TempData["Success"] = "Ajuste de precios aplicado correctamente.";
                return RedirectToAction(nameof(Details), new { id = ajusteID });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al aplicar ajuste de precios");
                ModelState.AddModelError("", $"Error: {ex.Message}");
                return View(model);
            }
        }

        // GET: AjustePrecios/Details/5
        [HttpGet]
        [Authorize(Policy = "Permission:productos.ajustarprecios")]
        public async Task<IActionResult> Details(int id)
        {
            var ajuste = await _ajustePrecioService.ObtenerAjusteHistoricoAsync(id);
            if (ajuste == null)
            {
                return NotFound();
            }

            var viewModel = _mapper.Map<AjustePrecioHistoricoViewModel>(ajuste);
            return View(viewModel);
        }

        // POST: AjustePrecios/Revert/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:productos.ajustarprecios")]
        public async Task<IActionResult> Revert(int id)
        {
            try
            {
                await _ajustePrecioService.RevertirAjusteAsync(id, User.Identity?.Name ?? "Sistema");

                await _auditoriaService.RegistrarCambioAsync(new AuditoriaRegistro
                {
                    FechaHora = DateTime.Now,
                    Usuario = User.Identity?.Name ?? "Sistema",
                    Entidad = "AjustePrecio",
                    Accion = "RevertirAjuste",
                    LlavePrimaria = id.ToString(),
                    Detalle = $"Reversión manual de ajuste de precios"
                });

                TempData["Success"] = "Ajuste de precios revertido correctamente.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al revertir ajuste de precios");
                TempData["Error"] = $"Error: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        [HttpGet]
        [Authorize(Policy = "Permission:productos.ajustarprecios")]
        public IActionResult FormSelector()
        {
            return View();
        }

        // GET: AjustePrecios/FormTemporal
        [HttpGet]
        [Authorize(Policy = "Permission:productos.ajustarprecios")]
        public async Task<IActionResult> FormTemporal()
        {
            var viewModel = new AjusteTemporalFormViewModel
            {
                FechaInicio = DateTime.Now.AddHours(1).Date.AddHours(DateTime.Now.Hour + 1),
                FechaFin = DateTime.Now.AddDays(7).Date.AddHours(DateTime.Now.Hour + 1),
                EsAumento = false, // Por defecto es descuento para ajustes temporales
            };

            await CargarProductosFormularioTemporal(viewModel);
            return View(viewModel);
        }

        // Método auxiliar para cargar productos y opciones de ajustes temporales
        private async Task CargarProductosFormularioTemporal(AjusteTemporalFormViewModel model)
        {
            var productos = await _productoService.GetAllProductosAsync();
            model.Productos = productos.Select(p => new ProductoAjusteViewModel
            {
                ProductoID = p.ProductoID,
                Nombre = p.Nombre,
                PCosto = p.PCosto,
                PContado = p.PContado,
                PLista = p.PLista,
                Seleccionado = model.Productos?.FirstOrDefault(m => m.ProductoID == p.ProductoID)?.Seleccionado ?? false
            }).ToList();

            model.TiposDeAjuste = new List<SelectListItem>
            {
                new SelectListItem { Value = "Promoción", Text = "Promoción" },
                new SelectListItem { Value = "Hot Sale", Text = "Hot Sale" },
                new SelectListItem { Value = "Oferta Especial", Text = "Oferta Especial" },
                new SelectListItem { Value = "Liquidación", Text = "Liquidación" },
                new SelectListItem { Value = "Descuento Temporal", Text = "Descuento Temporal" },
                new SelectListItem { Value = "Black Friday", Text = "Black Friday" },
                new SelectListItem { Value = "Cyber Monday", Text = "Cyber Monday" },
                new SelectListItem { Value = "Navidad", Text = "Navidad" },
                new SelectListItem { Value = "Aniversario", Text = "Aniversario" }
            };
        }

        // POST: AjustePrecios/FormTemporal
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:productos.ajustarprecios")]
        public async Task<IActionResult> FormTemporal(AjusteTemporalFormViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await CargarProductosFormularioTemporal(model);
                    return View(model);
                }

                // Validaciones básicas
                if (model.Porcentaje <= 0 || model.Porcentaje > 100)
                {
                    ModelState.AddModelError("Porcentaje", "El porcentaje debe estar entre 0.01 y 100");
                    await CargarProductosFormularioTemporal(model);
                    return View(model);
                }

                if (model.FechaInicio >= model.FechaFin)
                {
                    ModelState.AddModelError("FechaFin", "La fecha de finalización debe ser posterior a la fecha de inicio");
                    await CargarProductosFormularioTemporal(model);
                    return View(model);
                }

                if (string.IsNullOrEmpty(model.TipoAjuste))
                {
                    ModelState.AddModelError("TipoAjuste", "Debe seleccionar un tipo de ajuste");
                    await CargarProductosFormularioTemporal(model);
                    return View(model);
                }

                // Obtener productos seleccionados
                var productosSeleccionados = model.Productos
                    .Where(p => p.Seleccionado)
                    .Select(p => p.ProductoID)
                    .ToList();

                if (!productosSeleccionados.Any())
                {
                    ModelState.AddModelError("", "Debe seleccionar al menos un producto");
                    await CargarProductosFormularioTemporal(model);
                    return View(model);
                }

                // Redondear fechas a minutos para evitar problemas con segundos/milisegundos
                var fechaInicio = new DateTime(
                    model.FechaInicio.Value.Year,
                    model.FechaInicio.Value.Month,
                    model.FechaInicio.Value.Day,
                    model.FechaInicio.Value.Hour,
                    model.FechaInicio.Value.Minute,
                    0
                );

                var fechaFin = new DateTime(
                    model.FechaFin.Value.Year,
                    model.FechaFin.Value.Month,
                    model.FechaFin.Value.Day,
                    model.FechaFin.Value.Hour,
                    model.FechaFin.Value.Minute,
                    0
                );

                // Crear el ajuste temporal - Asegurando estado programado
                var ajusteID = await _ajustePrecioService.CrearAjusteTemporalAsync(
                    productosSeleccionados,
                    model.Porcentaje,
                    model.EsAumento,
                    fechaInicio,
                    fechaFin,
                    model.TipoAjuste,
                    model.Descripcion ?? $"Ajuste temporal {(model.EsAumento ? "aumento" : "descuento")} del {model.Porcentaje}%"
                );

                await _auditoriaService.RegistrarCambioAsync(new AuditoriaRegistro
                {
                    FechaHora = DateTime.Now,
                    Usuario = User.Identity?.Name ?? "Sistema",
                    Entidad = "AjustePrecio",
                    Accion = "CrearAjusteTemporal",
                    LlavePrimaria = ajusteID.ToString(),
                    Detalle = $"Ajuste temporal {model.TipoAjuste}, {(model.EsAumento ? "aumento" : "descuento")} del {model.Porcentaje}%, Vigencia: {fechaInicio:dd/MM/yyyy HH:mm} - {fechaFin:dd/MM/yyyy HH:mm}"
                });

                TempData["Success"] = $"Ajuste temporal creado correctamente. Se aplicará el {fechaInicio:dd/MM/yyyy HH:mm} y finalizará el {fechaFin:dd/MM/yyyy HH:mm}";
                return RedirectToAction("DetailsTemporal", new { id = ajusteID });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear ajuste temporal: {Message}", ex.Message);
                ModelState.AddModelError("", $"Ocurrió un error al crear el ajuste temporal: {ex.Message}");
                await CargarProductosFormularioTemporal(model);
                return View(model);
            }
        }

        // GET: AjustePrecios/DetailsTemporal/5
        [HttpGet]
        [Authorize(Policy = "Permission:productos.ajustarprecios")]
        public async Task<IActionResult> DetailsTemporal(int id)
        {
            var ajuste = await _ajustePrecioService.ObtenerAjusteHistoricoAsync(id);
            if (ajuste == null)
            {
                return NotFound();
            }

            if (!ajuste.EsTemporal)
            {
                return RedirectToAction("Details", new { id });
            }

            var viewModel = _mapper.Map<AjusteTemporalViewModel>(ajuste);
            return View(viewModel);
        }

        // GET: AjustePrecios/IndexTemporales
        [HttpGet]
        [Authorize(Policy = "Permission:productos.ajustarprecios")]
        public async Task<IActionResult> IndexTemporales()
        {
            var activos = await _ajustePrecioService.ObtenerAjustesTemporalesPorEstadoAsync(EstadoAjusteTemporal.Activo);
            var programados = await _ajustePrecioService.ObtenerAjustesTemporalesPorEstadoAsync(EstadoAjusteTemporal.Programado);
            var finalizados = await _ajustePrecioService.ObtenerAjustesTemporalesPorEstadoAsync(EstadoAjusteTemporal.Finalizado);

            var viewModel = new AjustesTemporalesIndexViewModel
            {
                AjustesActivos = _mapper.Map<List<AjusteTemporalViewModel>>(activos),
                AjustesProgramados = _mapper.Map<List<AjusteTemporalViewModel>>(programados),
                AjustesFinalizados = _mapper.Map<List<AjusteTemporalViewModel>>(finalizados)
            };

            return View(viewModel);
        }

        // POST: AjustePrecios/ActivarTemporal/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:productos.ajustarprecios")]
        public async Task<IActionResult> ActivarTemporal(int id)
        {
            try
            {
                await _ajustePrecioService.ActivarAjusteTemporalAsync(id);

                await _auditoriaService.RegistrarCambioAsync(new AuditoriaRegistro
                {
                    FechaHora = DateTime.Now,
                    Usuario = User.Identity?.Name ?? "Sistema",
                    Entidad = "AjustePrecio",
                    Accion = "ActivarAjusteTemporal",
                    LlavePrimaria = id.ToString(),
                    Detalle = "Activación manual de ajuste temporal"
                });

                TempData["Success"] = "Ajuste temporal activado con éxito.";
                return RedirectToAction("DetailsTemporal", new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al activar ajuste temporal.");
                TempData["Error"] = "Ocurrió un error al activar el ajuste temporal: " + ex.Message;
                return RedirectToAction("DetailsTemporal", new { id });
            }
        }

        // POST: AjustePrecios/FinalizarTemporal/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:productos.ajustarprecios")]
        public async Task<IActionResult> FinalizarTemporal(int id)
        {
            try
            {
                await _ajustePrecioService.FinalizarAjusteTemporalAsync(id, User.Identity?.Name ?? "Sistema");

                await _auditoriaService.RegistrarCambioAsync(new AuditoriaRegistro
                {
                    FechaHora = DateTime.Now,
                    Usuario = User.Identity?.Name ?? "Sistema",
                    Entidad = "AjustePrecio",
                    Accion = "FinalizarAjusteTemporal",
                    LlavePrimaria = id.ToString(),
                    Detalle = "Finalización manual de ajuste temporal"
                });

                TempData["Success"] = "Ajuste temporal finalizado con éxito.";
                return RedirectToAction("DetailsTemporal", new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al finalizar ajuste temporal.");
                TempData["Error"] = "Ocurrió un error al finalizar el ajuste temporal: " + ex.Message;
                return RedirectToAction("DetailsTemporal", new { id });
            }
        }

        // AJAX: AjustePrecios/SimularAjusteTemporal
        [HttpPost]
        [Authorize(Policy = "Permission:productos.ajustarprecios")]
        public async Task<IActionResult> SimularAjusteTemporal([FromBody] SimulacionAjusteTemporalViewModel model)
        {
            try
            {
                var productos = await _productoService.GetAllProductosAsync();
                var productosSeleccionados = productos
                    .Where(p => model.ProductoIDs.Contains(p.ProductoID))
                    .ToList();

                var factor = model.EsAumento ? (1 + model.Porcentaje / 100m) : (1 - model.Porcentaje / 100m);

                var resultado = productosSeleccionados.Select(p => new {
                    id = p.ProductoID,
                    nombre = p.Nombre,
                    costoActual = p.PCosto,
                    contadoActual = p.PContado,
                    listaActual = p.PLista,
                    costoNuevo = Math.Round(p.PCosto * factor, 2),
                    contadoNuevo = Math.Round(p.PContado * factor, 2),
                    listaNuevo = Math.Round(p.PLista * factor, 2)
                }).ToList();

                // Formatear fechas sin segundos
                var fechaInicio = new DateTime(
                    model.FechaInicio.Year,
                    model.FechaInicio.Month,
                    model.FechaInicio.Day,
                    model.FechaInicio.Hour,
                    model.FechaInicio.Minute,
                    0
                );

                var fechaFin = new DateTime(
                    model.FechaFin.Year,
                    model.FechaFin.Month,
                    model.FechaFin.Day,
                    model.FechaFin.Hour,
                    model.FechaFin.Minute,
                    0
                );

                return Json(new
                {
                    success = true,
                    productos = resultado,
                    fechaInicio = fechaInicio.ToString("dd/MM/yyyy HH:mm"),
                    fechaFin = fechaFin.ToString("dd/MM/yyyy HH:mm"),
                    tipoAjuste = model.TipoAjuste,
                    duracion = Math.Ceiling((fechaFin - fechaInicio).TotalDays)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al simular ajuste temporal: {Message}", ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
        }

        // AJAX: AjustePrecios/SimularAjuste (para ajustes permanentes)
        [HttpPost]
        [Authorize(Policy = "Permission:productos.ajustarprecios")]
        public async Task<IActionResult> SimularAjuste([FromBody] SimulacionAjusteViewModel model)
        {
            try
            {
                var productos = await _productoService.GetAllProductosAsync();
                var productosSeleccionados = productos
                    .Where(p => model.ProductoIDs.Contains(p.ProductoID))
                    .ToList();

                var factor = model.EsAumento ? (1 + model.Porcentaje / 100m) : (1 - model.Porcentaje / 100m);

                var resultado = productosSeleccionados.Select(p => new {
                    id = p.ProductoID,
                    nombre = p.Nombre,
                    costoActual = p.PCosto,
                    contadoActual = p.PContado,
                    listaActual = p.PLista,
                    costoNuevo = Math.Round(p.PCosto * factor, 2),
                    contadoNuevo = Math.Round(p.PContado * factor, 2),
                    listaNuevo = Math.Round(p.PLista * factor, 2)
                }).ToList();

                return Json(new
                {
                    success = true,
                    productos = resultado
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al simular ajuste permanente: {Message}", ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
        }
    }

    
}