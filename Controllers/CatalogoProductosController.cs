// Controllers/CatalogoProductosController.cs
using AutoMapper;
using Javo2.Controllers.Base;
using Javo2.DTOs;
using Javo2.IServices;
using Javo2.ViewModels;
using Javo2.ViewModels.Operaciones.Catalogo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Javo2.Controllers
{
    [Authorize(Policy = "PermisoPolitica")]
    public class CatalogoProductosController : BaseController
    {
        private readonly IProductoService _productoService;
        private readonly ICatalogoService _catalogoService;
        private readonly IMapper _mapper;

        public CatalogoProductosController(
            IProductoService productoService,
            ICatalogoService catalogoService,
            IMapper mapper,
            ILogger<CatalogoProductosController> logger)
            : base(logger)
        {
            _productoService = productoService;
            _catalogoService = catalogoService;
            _mapper = mapper;
        }

        // GET: CatalogoProductos
        [Authorize(Policy = "Permission:productos.ver")]
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("CatalogoProductosController: Index GET");
                var productos = await _productoService.GetAllProductosAsync();
                var rubros = await _catalogoService.GetRubrosAsync();
                var marcas = await _catalogoService.GetMarcasAsync();

                // Cargar totales de stock para rubros y marcas
                var (rubrosStock, marcasStock) = await _productoService.GetRubrosMarcasStockAsync();

                var model = new CatalogoProductosViewModel
                {
                    Productos = _mapper.Map<IEnumerable<ViewModels.Operaciones.Productos.ProductosViewModel>>(productos),
                    Rubros = _mapper.Map<IEnumerable<ViewModels.Operaciones.Catalogo.RubroViewModel>>(rubros),
                    Marcas = _mapper.Map<IEnumerable<ViewModels.Operaciones.Catalogo.MarcaViewModel>>(marcas)
                };

                // Asignar totales de stock a los ViewModels
                foreach (var rubroVm in model.Rubros)
                {
                    rubroVm.TotalStock = rubrosStock.TryGetValue(rubroVm.ID, out int totalRubroStock)
                        ? totalRubroStock : 0;
                }

                foreach (var marcaVm in model.Marcas)
                {
                    marcaVm.TotalStock = marcasStock.TryGetValue(marcaVm.ID, out int totalMarcaStock)
                        ? totalMarcaStock : 0;
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Index de CatalogoProductos");
                return View("Error");
            }
        }

        // GET: CatalogoProductos/FilterProductos
        [HttpGet]
        [Authorize(Policy = "Permission:productos.ver")]
        public async Task<IActionResult> FilterProductos(string filterType, string filterValue)
        {
            try
            {
                var filters = new ProductoFilterDto();

                switch (filterType)
                {
                    case "Nombre":
                        filters.Nombre = filterValue;
                        break;
                    case "Codigo":
                        filters.Codigo = filterValue;
                        break;
                    case "Rubro":
                        filters.Rubro = filterValue;
                        break;
                    case "SubRubro":
                        filters.SubRubro = filterValue;
                        break;
                    case "Marca":
                        filters.Marca = filterValue;
                        break;
                }

                var productos = await _productoService.FilterProductosAsync(filters);
                var productosVM = _mapper.Map<IEnumerable<ViewModels.Operaciones.Productos.ProductosViewModel>>(productos);

                // Usar la ruta completa a la vista parcial existente
                return PartialView("~/Views/Productos/_ProductosTable.cshtml", productosVM);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en FilterProductos");
                return Json(new { error = "Error al filtrar productos" });
            }
        }

        // GET: CatalogoProductos/FilterRubros
        [HttpGet]
        [Authorize(Policy = "Permission:catalogo.ver")]
        public async Task<IActionResult> FilterRubros(string term)
        {
            try
            {
                // Estrategia 1: Intentar pasar el término directamente si existe sobrecarga
                var rubros = await _catalogoService.GetRubrosAsync();

                // Filtrado manual si la API no lo soporta directamente
                if (!string.IsNullOrEmpty(term))
                {
                    term = term.ToLower();
                    rubros = rubros.Where(r => r.Nombre.ToLower().Contains(term)).ToList();
                }

                var rubrosVm = _mapper.Map<IEnumerable<ViewModels.Operaciones.Catalogo.RubroViewModel>>(rubros);
                return PartialView("~/Views/Catalogo/_RubrosTable.cshtml", rubrosVm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en FilterRubros");
                return Json(new { error = "Error al filtrar rubros" });
            }
        }

        // GET: CatalogoProductos/FilterMarcas
        [HttpGet]
        [Authorize(Policy = "Permission:catalogo.ver")]
        public async Task<IActionResult> FilterMarcas(string term)
        {
            try
            {
                // Estrategia 1: Intentar pasar el término directamente si existe sobrecarga
                var marcas = await _catalogoService.GetMarcasAsync();

                // Filtrado manual si la API no lo soporta directamente
                if (!string.IsNullOrEmpty(term))
                {
                    term = term.ToLower();
                    marcas = marcas.Where(m => m.Nombre.ToLower().Contains(term)).ToList();
                }

                var marcasVm = _mapper.Map<IEnumerable<ViewModels.Operaciones.Catalogo.MarcaViewModel>>(marcas);
                return PartialView("~/Views/Catalogo/_MarcasTable.cshtml", marcasVm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en FilterMarcas");
                return Json(new { error = "Error al filtrar marcas" });
            }
        }

        // POST: CatalogoProductos/IncrementarPrecios
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:productos.ajustarprecios")]
        public async Task<IActionResult> IncrementarPrecios(string ProductoIDs, decimal porcentaje, bool isAumento, string descripcion = "")
        {
            if (string.IsNullOrEmpty(ProductoIDs))
                return Json(new { success = false, message = "Seleccione productos." });

            var ids = ProductoIDs.Split(',').Select(int.Parse).ToList();

            try
            {
                // Llamar al servicio para ajustar precios
                await _productoService.AdjustPricesAsync(ids, porcentaje, isAumento);

                return Json(new
                {
                    success = true,
                    message = $"Ajuste de precios aplicado correctamente a {ids.Count} productos."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al aplicar ajuste rápido de precios");
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // GET: CatalogoProductos/GetSubRubros
        [HttpGet]
        public async Task<IActionResult> GetSubRubros(int rubroId)
        {
            _logger.LogInformation("GetSubRubros recibido para rubroId: {0}", rubroId);

            if (rubroId <= 0)
            {
                _logger.LogWarning("GetSubRubros: rubroId inválido: {0}", rubroId);
                return Json(new List<SelectListItem>());
            }

            try
            {
                var subRubros = await _catalogoService.GetSubRubrosByRubroIDAsync(rubroId);
                var items = subRubros.Select(sr => new { value = sr.ID.ToString(), text = sr.Nombre }).ToList();

                _logger.LogInformation("GetSubRubros: Obtenidos {0} subrubros", items.Count);
                return Json(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo subrubros para rubroId {0}", rubroId);
                return Json(new List<object>());
            }
        }
    }
}