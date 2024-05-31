using javo2.IServices;
using javo2.ViewModels.Operaciones.Productos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace javo2.Controllers
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

        public async Task<IActionResult> Index()
        {
            var rubros = await _catalogoService.GetRubroViewModelsAsync();
            var marcas = await _catalogoService.GetMarcaViewModelsAsync();
            var model = new Tuple<IEnumerable<RubroViewModel>, IEnumerable<MarcaViewModel>>(rubros, marcas);
            return View(model);
        }

        public async Task<IActionResult> EditSubRubros(int rubroId)
        {
            var rubro = await _catalogoService.GetRubroByIdAsync(rubroId);
            if (rubro == null)
            {
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
                        // Crear subrubro nuevo
                        await _catalogoService.CreateSubRubroAsync(new SubRubroViewModel
                        {
                            Nombre = subRubro.Nombre,
                            RubroNombre = model.RubroNombre
                        });
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
                    await _catalogoService.CreateSubRubroAsync(new SubRubroViewModel
                    {
                        Nombre = model.NewSubRubroNombre,
                        RubroNombre = model.RubroNombre
                    });
                }

                return RedirectToAction(nameof(Index));
            }

            LogModelStateErrors();
            return View(model);
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
            var rubro = await _catalogoService.GetRubroByIdAsync(id);
            if (rubro == null)
            {
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
            var marca = await _catalogoService.GetMarcaByIdAsync(id);
            if (marca == null)
            {
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
            var rubro = await _catalogoService.GetRubroByIdAsync(id);
            if (rubro == null)
            {
                return NotFound();
            }
            return View(rubro);
        }

        [HttpPost, ActionName("DeleteRubro")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRubroConfirmed(int id)
        {
            await _catalogoService.DeleteRubroAsync(id);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DeleteMarca(int id)
        {
            var marca = await _catalogoService.GetMarcaByIdAsync(id);
            if (marca == null)
            {
                return NotFound();
            }
            return View(marca);
        }

        [HttpPost, ActionName("DeleteMarca")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMarcaConfirmed(int id)
        {
            await _catalogoService.DeleteMarcaAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private async Task<SubRubroViewModel> InitializeNewSubRubroViewModelAsync()
        {
            var rubros = await _catalogoService.GetRubrosAsync();
            var model = new SubRubroViewModel
            {
                Rubros = rubros
            };
            return model;
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
    }
}
