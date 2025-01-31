using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Javo2.IServices;
using Javo2.Models;
using Javo2.ViewModels.Operaciones.Catalogo;
using System.Linq;
using System.Threading.Tasks;
using Javo2.Controllers.Base;
using AutoMapper;
using Javo2.Helpers;

namespace Javo2.Controllers
{
    public class CatalogoController : BaseController
    {
        private readonly ICatalogoService _catalogoService;
        private readonly IProductoService _productoService;
        private readonly IMapper _mapper;

        public CatalogoController(
            ICatalogoService catalogoService,
            IProductoService productoService,
            IMapper mapper,
            ILogger<CatalogoController> logger)
            : base(logger)
        {
            _catalogoService = catalogoService;
            _productoService = productoService;
            _mapper = mapper;
        }

        private async Task<CatalogoIndexViewModel> GetCatalogoIndexViewModelAsync(IEnumerable<Rubro>? rubros = null, IEnumerable<Marca>? marcas = null)
        {
            rubros ??= await _catalogoService.GetRubrosAsync();
            marcas ??= await _catalogoService.GetMarcasAsync();

            var model = new CatalogoIndexViewModel
            {
                Rubros = _mapper.Map<List<RubroViewModel>>(rubros),
                Marcas = _mapper.Map<List<MarcaViewModel>>(marcas)
            };

            return model;
        }

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Index action called");
            var rubros = await _catalogoService.GetRubrosAsync();
            var marcas = await _catalogoService.GetMarcasAsync();

            var model = new CatalogoIndexViewModel
            {
                Rubros = _mapper.Map<List<RubroViewModel>>(rubros),
                Marcas = _mapper.Map<List<MarcaViewModel>>(marcas)
            };

            _logger.LogInformation("Rubros and Marcas retrieved");
            await PopulateTotalStockForRubrosAndMarcas(model);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Filter(CatalogoFilterDto filters)
        {
            _logger.LogInformation("Filter action called with filters: {Filters}", filters);
            var rubros = await _catalogoService.FilterRubrosAsync(filters);
            var marcas = await _catalogoService.FilterMarcasAsync(filters);

            var model = new CatalogoIndexViewModel
            {
                Rubros = _mapper.Map<List<RubroViewModel>>(rubros),
                Marcas = _mapper.Map<List<MarcaViewModel>>(marcas)
            };

            await PopulateTotalStockForRubrosAndMarcas(model);

            return View("Index", model);
        }

        public async Task<IActionResult> EditSubRubros(int rubroId)
        {
            _logger.LogInformation("EditSubRubros action called with RubroID: {RubroID}", rubroId);
            var rubro = await _catalogoService.GetRubroByIDAsync(rubroId);
            if (rubro == null)
            {
                _logger.LogWarning("Rubro with ID {RubroID} not found", rubroId);
                return NotFound();
            }

            var model = new EditSubRubrosViewModel
            {
                RubroID = rubro.ID,
                RubroNombre = rubro.Nombre,
                SubRubros = _mapper.Map<List<SubRubroEditViewModel>>(rubro.SubRubros)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSubRubros(EditSubRubrosViewModel model)
        {
            if (!ModelState.IsValid)
            {
                LogModelStateErrors();
                return View(model);
            }

            try
            {
                _logger.LogInformation("Updating SubRubros for RubroID: {RubroID}", model.RubroID);
                await _catalogoService.UpdateSubRubrosAsync(model);
                _logger.LogInformation("SubRubros updated successfully for RubroID: {RubroID}", model.RubroID);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar los SubRubros para RubroID: {RubroID}", model.RubroID);
                ModelState.AddModelError(string.Empty, "Ocurrió un error al actualizar los subrubros.");
                return View(model);
            }
        }

        public IActionResult CreateRubro()
        {
            return View(new RubroViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRubro(RubroViewModel model)
        {
            if (!ModelState.IsValid)
            {
                LogModelStateErrors();
                return View(model);
            }

            var rubro = _mapper.Map<Rubro>(model);

            try
            {
                _logger.LogInformation("Creating new Rubro with Name: {Nombre}", model.Nombre);
                await _catalogoService.CreateRubroAsync(rubro);
                _logger.LogInformation("Rubro created successfully with Name: {Nombre}", model.Nombre);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el Rubro con Name: {Nombre}", model.Nombre);
                ModelState.AddModelError(string.Empty, "Ocurrió un error al crear el rubro.");
                return View(model);
            }
        }

        public IActionResult CreateMarca()
        {
            return View(new MarcaViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMarca(MarcaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                LogModelStateErrors();
                return View(model);
            }

            var marca = _mapper.Map<Marca>(model);

            try
            {
                _logger.LogInformation("Creating new Marca with Name: {Nombre}", model.Nombre);
                await _catalogoService.CreateMarcaAsync(marca);
                _logger.LogInformation("Marca created successfully with Name: {Nombre}", model.Nombre);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear la Marca con Name: {Nombre}", model.Nombre);
                ModelState.AddModelError(string.Empty, "Ocurrió un error al crear la marca.");
                return View(model);
            }
        }

        public async Task<IActionResult> EditRubro(int id)
        {
            _logger.LogInformation("EditRubro action called with ID: {ID}", id);
            var rubro = await _catalogoService.GetRubroByIDAsync(id);
            if (rubro == null)
            {
                _logger.LogWarning("Rubro with ID {ID} not found", id);
                return NotFound();
            }

            var model = _mapper.Map<RubroViewModel>(rubro);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRubro(RubroViewModel model)
        {
            if (!ModelState.IsValid)
            {
                LogModelStateErrors();
                return View(model);
            }

            var rubro = _mapper.Map<Rubro>(model);

            try
            {
                _logger.LogInformation("Updating Rubro with ID: {ID}", model.ID);
                await _catalogoService.UpdateRubroAsync(rubro);
                _logger.LogInformation("Rubro updated successfully with ID: {ID}", model.ID);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el Rubro con ID {ID}", model.ID);
                ModelState.AddModelError(string.Empty, "Ocurrió un error al actualizar el rubro.");
                return View(model);
            }
        }

        public async Task<IActionResult> EditMarca(int id)
        {
            _logger.LogInformation("EditMarca action called with ID: {ID}", id);
            var marca = await _catalogoService.GetMarcaByIDAsync(id);
            if (marca == null)
            {
                _logger.LogWarning("Marca with ID {ID} not found", id);
                return NotFound();
            }

            var model = _mapper.Map<MarcaViewModel>(marca);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMarca(MarcaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                LogModelStateErrors();
                return View(model);
            }

            var marca = _mapper.Map<Marca>(model);

            try
            {
                _logger.LogInformation("Updating Marca with ID: {ID}", model.ID);
                await _catalogoService.UpdateMarcaAsync(marca);
                _logger.LogInformation("Marca updated successfully with ID: {ID}", model.ID);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar la Marca con ID {ID}", model.ID);
                ModelState.AddModelError(string.Empty, "Ocurrió un error al actualizar la marca.");
                return View(model);
            }
        }

        public async Task<IActionResult> DeleteRubro(int id)
        {
            _logger.LogInformation("DeleteRubro action called with ID: {ID}", id);
            var rubro = await _catalogoService.GetRubroByIDAsync(id);
            if (rubro == null)
            {
                _logger.LogWarning("Rubro with ID {ID} not found", id);
                return NotFound();
            }

            var model = _mapper.Map<RubroViewModel>(rubro);
            return View(model);
        }

        [HttpPost, ActionName("DeleteRubro")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRubroConfirmed(int id)
        {
            try
            {
                _logger.LogInformation("Deleting Rubro with ID: {ID}", id);
                await _catalogoService.DeleteRubroAsync(id);
                _logger.LogInformation("Rubro deleted successfully with ID: {ID}", id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el Rubro con ID {ID}", id);
                ModelState.AddModelError(string.Empty, "Ocurrió un error al eliminar el rubro.");
                return RedirectToAction(nameof(DeleteRubro), new { id });
            }
        }

        public async Task<IActionResult> DeleteMarca(int id)
        {
            _logger.LogInformation("DeleteMarca action called with ID: {ID}", id);
            var marca = await _catalogoService.GetMarcaByIDAsync(id);
            if (marca == null)
            {
                _logger.LogWarning("Marca with ID {ID} not found", id);
                return NotFound();
            }

            var model = _mapper.Map<MarcaViewModel>(marca);
            return View(model);
        }

        [HttpPost, ActionName("DeleteMarca")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMarcaConfirmed(int id)
        {
            try
            {
                _logger.LogInformation("Deleting Marca with ID: {ID}", id);
                await _catalogoService.DeleteMarcaAsync(id);
                _logger.LogInformation("Marca deleted successfully with ID: {ID}", id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar la Marca con ID {ID}", id);
                ModelState.AddModelError(string.Empty, "Ocurrió un error al eliminar la marca.");
                return RedirectToAction(nameof(DeleteMarca), new { id });
            }
        }

        [HttpGet]
        public async Task<IActionResult> FilterAsync([FromQuery] CatalogoFilterDto filters)
        {
            _logger.LogInformation("Filter action called with filters: {@Filters}", filters);

            var rubros = await _catalogoService.FilterRubrosAsync(filters);
            var marcas = await _catalogoService.FilterMarcasAsync(filters);

            var partials = await GenerateRubrosMarcasPartialsAsync(rubros, marcas);

            return Json(partials);
        }

        private async Task PopulateTotalStockForRubrosAndMarcas(CatalogoIndexViewModel model)
        {
            var (rubrosStock, marcasStock) = await _productoService.GetRubrosMarcasStockAsync();

            foreach (var rubroVm in model.Rubros)
            {
                rubroVm.TotalStock = rubrosStock.TryGetValue(rubroVm.ID, out int totalRubroStock) ? totalRubroStock : 0;
            }

            foreach (var marcaVm in model.Marcas)
            {
                marcaVm.TotalStock = marcasStock.TryGetValue(marcaVm.ID, out int totalMarcaStock) ? totalMarcaStock : 0;
            }
        }

        private async Task<object> GenerateRubrosMarcasPartialsAsync(IEnumerable<Rubro> rubros, IEnumerable<Marca> marcas)
        {
            var model = await GetCatalogoIndexViewModelAsync(rubros, marcas);

            var rubrosPartial = await this.RenderViewAsync("_RubrosTable", model.Rubros, true);
            var marcasPartial = await this.RenderViewAsync("_MarcasTable", model.Marcas, true);

            return new
            {
                rubrosPartial,
                marcasPartial
            };
        }
    }
}
