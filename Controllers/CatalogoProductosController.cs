// Controllers/CatalogoProductosController.cs
using System.Threading.Tasks;
using AutoMapper;
using Javo2.IServices;
using Javo2.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Javo2.Controllers
{
    [Authorize]
    public class CatalogoProductosController : Controller
    {
        private readonly IProductoService _productoService;
        private readonly ICatalogoService _catalogoService;
        private readonly IMapper _mapper;
        private readonly ILogger<CatalogoProductosController> _logger;

        public CatalogoProductosController(
            IProductoService productoService,
            ICatalogoService catalogoService,
            IMapper mapper,
            ILogger<CatalogoProductosController> logger)
        {
            _productoService = productoService;
            _catalogoService = catalogoService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var productos = await _productoService.GetAllProductosAsync();
            var rubros = await _catalogoService.GetRubrosAsync();
            var marcas = await _catalogoService.GetMarcasAsync();

            var viewModel = new CatalogoProductosViewModel
            {
                Productos = _mapper.Map<IEnumerable<ViewModels.Operaciones.Productos.ProductosViewModel>>(productos),
                Rubros = _mapper.Map<IEnumerable<ViewModels.Operaciones.Catalogo.RubroViewModel>>(rubros),
                Marcas = _mapper.Map<IEnumerable<ViewModels.Operaciones.Catalogo.MarcaViewModel>>(marcas)
            };

            return View(viewModel);
        }

        // Action para filtrar productos (AJAX)
        [HttpGet]
        public async Task<IActionResult> FilterProductos(string filterType, string filterValue)
        {
            var filters = new Javo2.DTOs.ProductoFilterDto();

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

            return PartialView("_ProductosTable", productosVM);
        }
    }
}