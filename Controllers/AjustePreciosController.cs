// Controllers/AjustePreciosController.cs
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
        private readonly IMapper _mapper;
        private readonly ILogger<AjustePreciosController> _logger;

        public AjustePreciosController(
            IAjustePrecioService ajustePrecioService,
            IProductoService productoService,
            IMapper mapper,
            ILogger<AjustePreciosController> logger)
        {
            _ajustePrecioService = ajustePrecioService;
            _productoService = productoService;
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
            {
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

                var ajusteID = await _ajustePrecioService.AjustarPreciosAsync(
                    productosSeleccionados,
                    model.Porcentaje,
                    model.EsAumento,
                    model.Descripcion);

                TempData["Success"] = "Ajuste de precios realizado con éxito.";
                return RedirectToAction("Details", new { id = ajusteID });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al ajustar precios.");
                ModelState.AddModelError("", "Ocurrió un error al ajustar los precios: " + ex.Message);
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
                await _ajustePrecioService.RevertirAjusteAsync(id, User.Identity.Name);
                TempData["Success"] = "Ajuste de precios revertido con éxito.";
                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al revertir ajuste de precios.");
                TempData["Error"] = "Ocurrió un error al revertir el ajuste: " + ex.Message;
                return RedirectToAction("Details", new { id });
            }
        }

        // AJAX: AjustePrecios/SimularAjuste
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

                return Json(new { success = true, productos = resultado });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al simular ajuste de precios.");
                return Json(new { success = false, message = ex.Message });
            }
        }
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

    public class ProductoAjusteViewModel
    {
        public int ProductoID { get; set; }
        public string Nombre { get; set; }
        public decimal PCosto { get; set; }
        public decimal PContado { get; set; }
        public decimal PLista { get; set; }
        public bool Seleccionado { get; set; }
    }

    public class SimulacionAjusteViewModel
    {
        public List<int> ProductoIDs { get; set; }
        public decimal Porcentaje { get; set; }
        public bool EsAumento { get; set; }
    }
}