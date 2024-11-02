// Models/CatalogoController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Javo2.IServices;
using Javo2.Models;
using Javo2.ViewModels.Operaciones.Catalogo;
using System.Linq;
using System.Threading.Tasks;
using Javo2.Controllers.Base;
using AutoMapper;

namespace Javo2.Controllers
{
    public class CatalogoController : BaseController
    {
        private readonly ICatalogoService _catalogoService;
        private readonly IMapper _mapper;

        public CatalogoController(
            ICatalogoService catalogoService,
            IMapper mapper,
            ILogger<CatalogoController> logger)
            : base(logger)
        {
            _catalogoService = catalogoService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Index action called");
            var rubros = await _catalogoService.GetRubrosAsync();
            var marcas = await _catalogoService.GetMarcasAsync();

            var model = new CatalogoIndexViewModel
            {
                Rubros = rubros.Select(r => new RubroViewModel
                {
                    Id = r.Id,
                    Nombre = r.Nombre,
                    SubRubros = r.SubRubros.Select(sr => new SubRubroViewModel
                    {
                        Id = sr.Id,
                        Nombre = sr.Nombre,
                        RubroId = sr.RubroId
                    }).ToList()
                }).ToList(),
                Marcas = marcas.Select(m => new MarcaViewModel
                {
                    Id = m.Id,
                    Nombre = m.Nombre
                }).ToList()
            };

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
            if (!ModelState.IsValid)
            {
                LogModelStateErrors(); // Asegúrate de implementar este método si lo necesitas
                return View(model);
            }

            await _catalogoService.UpdateSubRubrosAsync(model);
            return RedirectToAction(nameof(Index));
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

            var rubro = new Rubro
            {
                Nombre = model.Nombre
            };

            try
            {
                await _catalogoService.CreateRubroAsync(rubro);
                return RedirectToAction(nameof(Index));
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error al crear el Rubro");
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

            var marca = new Marca
            {
                Nombre = model.Nombre
            };

            await _catalogoService.CreateMarcaAsync(marca);
            return RedirectToAction(nameof(Index));
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

            var model = new RubroViewModel
            {
                Id = rubro.Id,
                Nombre = rubro.Nombre
            };

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

            var rubro = new Rubro
            {
                Id = model.Id,
                Nombre = model.Nombre
            };

            await _catalogoService.UpdateRubroAsync(rubro);
            return RedirectToAction(nameof(Index));
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

            var model = new MarcaViewModel
            {
                Id = marca.Id,
                Nombre = marca.Nombre
            };

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

            var marca = new Marca
            {
                Id = model.Id,
                Nombre = model.Nombre
            };

            await _catalogoService.UpdateMarcaAsync(marca);
            return RedirectToAction(nameof(Index));
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

            var model = new RubroViewModel
            {
                Id = rubro.Id,
                Nombre = rubro.Nombre
            };

            return View(model);
        }

        [HttpPost, ActionName("DeleteRubro")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRubroConfirmed(int id)
        {
            try
            {
                await _catalogoService.DeleteRubroAsync(id);
                _logger.LogInformation("Rubro with ID {Id} deleted successfully", id);
                return RedirectToAction(nameof(Index));
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el Rubro con ID {Id}", id);
                ModelState.AddModelError(string.Empty, "Ocurrió un error al eliminar el rubro.");
                return RedirectToAction("DeleteRubro", new { id });
            }
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

            var model = new MarcaViewModel
            {
                Id = marca.Id,
                Nombre = marca.Nombre
            };

            return View(model);
        }

        [HttpPost, ActionName("DeleteMarca")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMarcaConfirmed(int id)
        {
            try
            {
                await _catalogoService.DeleteMarcaAsync(id);
                _logger.LogInformation("Marca with ID {Id} deleted successfully", id);
                return RedirectToAction(nameof(Index));
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar la Marca con ID {Id}", id);
                ModelState.AddModelError(string.Empty, "Ocurrió un error al eliminar la marca.");
                return RedirectToAction("DeleteMarca", new { id });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Filter(CatalogoFilterDto filters)
        {
            _logger.LogInformation("Filter action called with filters: {Filters}", filters);
            var rubros = await _catalogoService.FilterRubrosAsync(filters);
            var marcas = await _catalogoService.FilterMarcasAsync(filters);

            var rubrosViewModel = rubros.Select(r => new RubroViewModel
            {
                Id = r.Id,
                Nombre = r.Nombre,
                SubRubros = r.SubRubros.Select(sr => new SubRubroViewModel
                {
                    Id = sr.Id,
                    Nombre = sr.Nombre,
                    RubroId = sr.RubroId
                }).ToList()
            }).ToList();

            var marcasViewModel = marcas.Select(m => new MarcaViewModel
            {
                Id = m.Id,
                Nombre = m.Nombre
            }).ToList();

            var model = new CatalogoIndexViewModel
            {
                Rubros = rubrosViewModel,
                Marcas = marcasViewModel
            };

            // Devolvemos la vista parcial con el modelo filtrado
            return PartialView("_CatalogoTable", model);
        }
    }
}
