// Controllers/AjustePreciosController.cs (Extendido)
using Javo2.IServices;
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
            var productos = await _productoService.GetAllProductosAsync();
            var viewModel = new AjusteTemporalFormViewModel
            {
                Productos = productos.Select(p => new ProductoAjusteViewModel
                {
                    ProductoID = p.ProductoID,
                    Nombre = p.Nombre,
                    PCosto = p.PCosto,
                    PContado = p.PContado,
                    PLista = p.PLista,
                    Seleccionado = false
                }).ToList(),
                TiposDeAjuste = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>
                {
                    new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = "Promoción", Text = "Promoción" },
                    new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = "Hot Sale", Text = "Hot Sale" },
                    new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = "Oferta Especial", Text = "Oferta Especial" },
                    new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = "Liquidación", Text = "Liquidación" },
                    new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = "Descuento Temporal", Text = "Descuento Temporal" }
                }
            };
            return View(viewModel);
        }

        // POST: AjustePrecios/FormTemporal
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:productos.ajustarprecios")]
        public async Task<IActionResult> FormTemporal(AjusteTemporalFormViewModel model)
        {
            if (!ModelState.IsValid)
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

                model.TiposDeAjuste = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>
                {
                    new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = "Promoción", Text = "Promoción" },
                    new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = "Hot Sale", Text = "Hot Sale" },
                    new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = "Oferta Especial", Text = "Oferta Especial" },
                    new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = "Liquidación", Text = "Liquidación" },
                    new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = "Descuento Temporal", Text = "Descuento Temporal" }
                };

                return View(model);
            }

            try
            {
                var productosSeleccionados = model.Productos
                    .Where(p => p.Seleccionado)
                    .Select(p => p.ProductoID)
                    .ToList();

                if (!productosSeleccionados.Any())
                {
                    ModelState.AddModelError("", "Debe seleccionar al menos un producto.");
                    return View(model);
                }

                if (model.FechaFin <= model.FechaInicio)
                {
                    ModelState.AddModelError("FechaFin", "La fecha de fin debe ser posterior a la fecha de inicio.");
                    return View(model);
                }

                var ajusteID = await _ajustePrecioService.CrearAjusteTemporalAsync(
                    productosSeleccionados,
                    model.Porcentaje,
                    model.EsAumento,
                    model.FechaInicio,
                    model.FechaFin,
                    model.TipoAjuste,
                    model.Descripcion);

                await _auditoriaService.RegistrarCambioAsync(new Models.AuditoriaRegistro
                {
                    FechaHora = DateTime.Now,
                    Usuario = User.Identity?.Name ?? "Sistema",
                    Entidad = "AjustePrecio",
                    Accion = "CrearAjusteTemporal",
                    LlavePrimaria = ajusteID.ToString(),
                    Detalle = $"Ajuste temporal {model.TipoAjuste}, {(model.EsAumento ? "aumento" : "descuento")} del {model.Porcentaje}%, Vigencia: {model.FechaInicio.ToShortDateString()} - {model.FechaFin.ToShortDateString()}"
                });

                TempData["Success"] = "Ajuste temporal creado exitosamente.";
                return RedirectToAction("DetailsTemporal", new { id = ajusteID });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear ajuste temporal.");
                ModelState.AddModelError("", "Ocurrió un error al crear el ajuste temporal: " + ex.Message);

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
            var activos = await _ajustePrecioService.ObtenerAjustesTemporalesPorEstadoAsync(Models.EstadoAjusteTemporal.Activo);
            var programados = await _ajustePrecioService.ObtenerAjustesTemporalesPorEstadoAsync(Models.EstadoAjusteTemporal.Programado);
            var finalizados = await _ajustePrecioService.ObtenerAjustesTemporalesPorEstadoAsync(Models.EstadoAjusteTemporal.Finalizado);

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

                await _auditoriaService.RegistrarCambioAsync(new Models.AuditoriaRegistro
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

                await _auditoriaService.RegistrarCambioAsync(new Models.AuditoriaRegistro
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

                return Json(new
                {
                    success = true,
                    productos = resultado,
                    fechaInicio = model.FechaInicio.ToString("dd/MM/yyyy"),
                    fechaFin = model.FechaFin.ToString("dd/MM/yyyy"),
                    tipoAjuste = model.TipoAjuste,
                    duracion = (model.FechaFin - model.FechaInicio).TotalDays
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al simular ajuste temporal.");
                return Json(new { success = false, message = ex.Message });
            }
        }
    }

    public class AjusteTemporalFormViewModel
    {
        [Required(ErrorMessage = "El porcentaje es obligatorio")]
        [Range(0.01, 100, ErrorMessage = "El porcentaje debe estar entre 0.01 y 100")]
        [Display(Name = "Porcentaje")]
        public decimal Porcentaje { get; set; }

        [Display(Name = "Tipo de Ajuste")]
        public bool EsAumento { get; set; } = true;

        [Required(ErrorMessage = "El tipo de ajuste es obligatorio")]
        [Display(Name = "Motivo del Ajuste")]
        public string TipoAjuste { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es obligatoria")]
        [Display(Name = "Fecha de Inicio")]
        public DateTime FechaInicio { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "La fecha de finalización es obligatoria")]
        [Display(Name = "Fecha de Finalización")]
        public DateTime FechaFin { get; set; } = DateTime.Now.AddDays(7);

        [Display(Name = "Descripción")]
        [StringLength(250, ErrorMessage = "La descripción no puede exceder los 250 caracteres")]
        public string Descripcion { get; set; }

        public List<ProductoAjusteViewModel> Productos { get; set; } = new List<ProductoAjusteViewModel>();
        public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> TiposDeAjuste { get; set; } = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>();
    }
    public class AjustePrecioFormViewModel
    {
        [Required(ErrorMessage = "El porcentaje es obligatorio")]
        [Range(0.01, 100, ErrorMessage = "El porcentaje debe estar entre 0.01 y 100")]
        [Display(Name = "Porcentaje")]
        public decimal Porcentaje { get; set; }

        [Display(Name = "Tipo de Ajuste")]
        public bool EsAumento { get; set; } = true;

        [Display(Name = "Descripción")]
        [StringLength(250, ErrorMessage = "La descripción no puede exceder los 250 caracteres")]
        public string Descripcion { get; set; }

        public List<ProductoAjusteViewModel> Productos { get; set; } = new List<ProductoAjusteViewModel>();
    }

    public class SimulacionAjusteTemporalViewModel
    {
        public List<int> ProductoIDs { get; set; }
        public decimal Porcentaje { get; set; }
        public bool EsAumento { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string TipoAjuste { get; set; }
    }

    public class AjusteTemporalViewModel
    {
        public int AjusteHistoricoID { get; set; }
        public DateTime FechaAjuste { get; set; }
        public string UsuarioAjuste { get; set; }
        public decimal Porcentaje { get; set; }
        public bool EsAumento { get; set; }
        public string Descripcion { get; set; }
        public List<AjustePrecioDetalleViewModel> Detalles { get; set; } = new List<AjustePrecioDetalleViewModel>();
        public bool Revertido { get; set; }
        public DateTime? FechaReversion { get; set; }
        public string UsuarioReversion { get; set; }

        // Propiedades específicas para ajustes temporales
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string TipoAjusteTemporal { get; set; }
        public string EstadoTemporal { get; set; }

        public bool PuedeActivar => EstadoTemporal == "Programado" && !Revertido;
        public bool PuedeFinalizar => EstadoTemporal == "Activo" && !Revertido;
        public TimeSpan DuracionTotal => FechaFin.HasValue && FechaInicio.HasValue ? FechaFin.Value - FechaInicio.Value : TimeSpan.Zero;
        public int DiasRestantes => FechaFin.HasValue && DateTime.Now < FechaFin.Value ? (FechaFin.Value - DateTime.Now).Days : 0;
    }

    public class AjustesTemporalesIndexViewModel
    {
        public List<AjusteTemporalViewModel> AjustesActivos { get; set; } = new List<AjusteTemporalViewModel>();
        public List<AjusteTemporalViewModel> AjustesProgramados { get; set; } = new List<AjusteTemporalViewModel>();
        public List<AjusteTemporalViewModel> AjustesFinalizados { get; set; } = new List<AjusteTemporalViewModel>();
    }
}