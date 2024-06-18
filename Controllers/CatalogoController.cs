using Javo2.IServices;
using Javo2.ViewModels.Operaciones.Catalogo;
using Javo2.ViewModels.Operaciones.Catalogo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Javo2.Controllers
{
    public class CatalogoController : Controller
    {
        private readonly ICatalogoService _catalogoService;
        private readonly ILogger<CatalogoController> _logger;

        public CatalogoController(ICatalogoService catalogoService, ILogger<CatalogoController> logger)
        {
            _catalogoService = catalogoService;
            _logger = logger;
        }

        private void LogModelStateErrors()
        {
            foreach (var key in ModelState.Keys)
            {
                var state = ModelState[key];
                if (state?.Errors != null)
                {
                    var errors = state.Errors.Select(e => e.ErrorMessage).ToList();
                    if (errors.Count > 0)
                    {
                        _logger.LogWarning("Model state errors for key '{Key}': {Errors}", key, string.Join(", ", errors));
                    }
                }
            }
        }

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Index action called");
            var rubros = await _catalogoService.GetRubroViewModelsAsync();
            var marcas = await _catalogoService.GetMarcaViewModelsAsync();
            var model = new Tuple<IEnumerable<RubroViewModel>, IEnumerable<MarcaViewModel>>(rubros, marcas);
            _logger.LogInformation("Rubros and Marcas retrieved");
            return View(model);
        }

        public async Task<IActionResult> EditSubRubros(int rubroId)
        {
            _logger.LogInformation("EditSubRubros action called with RubroId: {RubroId}", rubroId);
            var rubro = await _catalogoService.GetRubroByIdAsync(rubroId);
            if (rubro == null)
            {
                _logger.LogWarning("Rubro with ID {RubroId} not found", rubroId);
                return NotFound();
            }

            var model = new EditSubRubrosViewModel
            {
                RubroId = rubro.Id,
                RubroNombre = rubro.Nombre,
                SubRubros = rubro.SubRubros.Select(sr => new SubRubroEditViewModel
                {
                    Id = sr.Id,
                    Nombre = sr.Nombre
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSubRubros(EditSubRubrosViewModel model)
        {
            if (ModelState.IsValid)
            {
                foreach (var subRubro in model.SubRubros)
                {
                    if (subRubro.IsDeleted)
                    {
                        await _catalogoService.DeleteSubRubroAsync(subRubro.Id);
                    }
                    else if (subRubro.Id == 0)
                    {
                        await CreateSubRubroAsync(subRubro.Nombre, model.RubroNombre);
                    }
                    else
                    {
                        await _catalogoService.UpdateSubRubroAsync(new SubRubroViewModel
                        {
                            Id = subRubro.Id,
                            Nombre = subRubro.Nombre,
                            RubroNombre = model.RubroNombre
                        });
                    }
                }

                if (!string.IsNullOrWhiteSpace(model.NewSubRubroNombre))
                {
                    await CreateSubRubroAsync(model.NewSubRubroNombre, model.RubroNombre);
                }

                return RedirectToAction(nameof(Index));
            }

            LogModelStateErrors();
            return View(model);
        }

        private async Task CreateSubRubroAsync(string nombre, string rubroNombre)
        {
            await _catalogoService.CreateSubRubroAsync(new SubRubroViewModel
            {
                Nombre = nombre,
                RubroNombre = rubroNombre
            });
        }

        public IActionResult CreateRubro()
        {
            return View(new RubroViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRubro(RubroViewModel model)
        {
            if (ModelState.IsValid)
            {
                await _catalogoService.CreateRubroAsync(model);
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public IActionResult CreateMarca()
        {
            return View(new MarcaViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMarca(MarcaViewModel model)
        {
            if (ModelState.IsValid)
            {
                await _catalogoService.CreateMarcaAsync(model);
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public async Task<IActionResult> EditRubro(int id)
        {
            _logger.LogInformation("EditRubro action called with ID: {Id}", id);
            var rubro = await _catalogoService.GetRubroByIdAsync(id);
            if (rubro == null)
            {
                _logger.LogWarning("Rubro with ID {Id} not found", id);
                return NotFound();
            }
            return View(rubro);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRubro(RubroViewModel model)
        {
            if (ModelState.IsValid)
            {
                await _catalogoService.UpdateRubroAsync(model);
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public async Task<IActionResult> EditMarca(int id)
        {
            _logger.LogInformation("EditMarca action called with ID: {Id}", id);
            var marca = await _catalogoService.GetMarcaByIdAsync(id);
            if (marca == null)
            {
                _logger.LogWarning("Marca with ID {Id} not found", id);
                return NotFound();
            }
            return View(marca);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMarca(MarcaViewModel model)
        {
            if (ModelState.IsValid)
            {
                await _catalogoService.UpdateMarcaAsync(model);
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public async Task<IActionResult> DeleteRubro(int id)
        {
            _logger.LogInformation("DeleteRubro action called with ID: {Id}", id);
            var rubro = await _catalogoService.GetRubroByIdAsync(id);
            if (rubro == null)
            {
                _logger.LogWarning("Rubro with ID {Id} not found", id);
                return NotFound();
            }
            return View(rubro);
        }

        [HttpPost, ActionName("DeleteRubro")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRubroConfirmed(int id)
        {
            await _catalogoService.DeleteRubroAsync(id);
            _logger.LogInformation("Rubro with ID {Id} deleted successfully", id);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DeleteMarca(int id)
        {
            _logger.LogInformation("DeleteMarca action called with ID: {Id}", id);
            var marca = await _catalogoService.GetMarcaByIdAsync(id);
            if (marca == null)
            {
                _logger.LogWarning("Marca with ID {Id} not found", id);
                return NotFound();
            }
            return View(marca);
        }

        [HttpPost, ActionName("DeleteMarca")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMarcaConfirmed(int id)
        {
            await _catalogoService.DeleteMarcaAsync(id);
            _logger.LogInformation("Marca with ID {Id} deleted successfully", id);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Filter(CatalogoFilterDto filters)
        {
            _logger.LogInformation("Filter action called with filters: {Filters}", filters);
            var rubros = await _catalogoService.FilterRubrosAsync(filters);
            var marcas = await _catalogoService.FilterMarcasAsync(filters);
            var model = new Tuple<IEnumerable<RubroViewModel>, IEnumerable<MarcaViewModel>>(rubros, marcas);
            return PartialView("_CatalogoTable", model);
        }
    }
}
