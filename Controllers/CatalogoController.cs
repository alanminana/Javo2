// Archivo: Controllers/CatalogoController.cs
// Cambios realizados:
// - Se ha envuelto la mayoría de las acciones (GET) en bloques try/catch para capturar y registrar excepciones, 
//   retornando una vista de error ("Error") en caso de fallo.
// - Se han agregado try/catch en acciones que retornan datos (como Filter y EditSubRubros GET) para una mayor robustez.

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
            try // Se agregó try/catch para manejo de excepciones en Index
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
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error in Index action of CatalogoController");
                return View("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Filter(CatalogoFilterDto filters)
        {
            try // Se agregó try/catch en Filter
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
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error in Filter action of CatalogoController");
                return View("Error");
            }
        }

        public async Task<IActionResult> EditSubRubros(int rubroId)
        {
            try // Se agregó try/catch en EditSubRubros GET
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
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error in EditSubRubros GET action of CatalogoController");
                return View("Error");
            }
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
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error updating SubRubros for RubroID: {RubroID}", model.RubroID);
                ModelState.AddModelError(string.Empty, "Ocurrió un error al actualizar los subrubros.");
                return View(model);
            }
        }

        public IActionResult CreateRubro()
        {
            // Simple action, no se requiere try/catch
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
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error creating Rubro with Name: {Nombre}", model.Nombre);
                ModelState.AddModelError(string.Empty, "Ocurrió un error al crear el rubro.");
                return View(model);
            }
        }

        public IActionResult CreateMarca()
        {
            // Simple action, no se requiere try/catch
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
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error creating Marca with Name: {Nombre}", model.Nombre);
                ModelState.AddModelError(string.Empty, "Ocurrió un error al crear la marca.");
                return View(model);
            }
        }

        public async Task<IActionResult> EditRubro(int id)
        {
            try // Se agregó try/catch en EditRubro GET
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
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error in EditRubro GET action of CatalogoController");
                return View("Error");
            }
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
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error updating Rubro with ID: {ID}", model.ID);
                ModelState.AddModelError(string.Empty, "Ocurrió un error al actualizar el rubro.");
                return View(model);
            }
        }

        public async Task<IActionResult> EditMarca(int id)
        {
            try // Se agregó try/catch en EditMarca GET
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
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error in EditMarca GET action of CatalogoController");
                return View("Error");
            }
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
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error updating Marca with ID: {ID}", model.ID);
                ModelState.AddModelError(string.Empty, "Ocurrió un error al actualizar la marca.");
                return View(model);
            }
        }

        public async Task<IActionResult> DeleteRubro(int id)
        {
            try // Se agregó try/catch en DeleteRubro GET
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
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteRubro GET action of CatalogoController");
                return View("Error");
            }
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
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error deleting Rubro with ID: {ID}", id);
                ModelState.AddModelError(string.Empty, "Ocurrió un error al eliminar el rubro.");
                return RedirectToAction(nameof(DeleteRubro), new { id });
            }
        }

        public async Task<IActionResult> DeleteMarca(int id)
        {
            try // Se agregó try/catch en DeleteMarca GET
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
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteMarca GET action of CatalogoController");
                return View("Error");
            }
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
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error deleting Marca with ID: {ID}", id);
                ModelState.AddModelError(string.Empty, "Ocurrió un error al eliminar la marca.");
                return RedirectToAction(nameof(DeleteMarca), new { id });
            }
        }

        [HttpGet]
        public async Task<IActionResult> FilterAsync([FromQuery] CatalogoFilterDto filters)
        {
            try // Se agregó try/catch en FilterAsync GET
            {
                _logger.LogInformation("FilterAsync action called with filters: {@Filters}", filters);

                var rubros = await _catalogoService.FilterRubrosAsync(filters);
                var marcas = await _catalogoService.FilterMarcasAsync(filters);

                var partials = await GenerateRubrosMarcasPartialsAsync(rubros, marcas);

                return Json(partials);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error in FilterAsync action of CatalogoController");
                return Json(new { rubrosPartial = "", marcasPartial = "" });
            }
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
