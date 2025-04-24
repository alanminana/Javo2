// Archivo: Controllers/CatalogoController.cs
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
using System;
using System.Collections.Generic;

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

        // GET: Catalogo
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("CatalogoController: Index GET");
                var model = await GetCatalogoIndexViewModelAsync();
                await PopulateTotalStockForRubrosAndMarcas(model);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Index de Catalogo");
                return View("Error");
            }
        }

        // GET: Catalogo/CreateRubro
        public IActionResult CreateRubro()
        {
            return View(new RubroViewModel());
        }

        // POST: Catalogo/CreateRubro
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRubro(RubroViewModel model)
        {
            if (!ModelState.IsValid)
            {
                LogModelStateErrors();
                return View(model);
            }

            try
            {
                var rubro = _mapper.Map<Rubro>(model);
                await _catalogoService.CreateRubroAsync(rubro);
                _logger.LogInformation("Rubro creado: {Nombre}", model.Nombre);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear Rubro");
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        // GET: Catalogo/EditRubro/5
        public async Task<IActionResult> EditRubro(int id)
        {
            try
            {
                var rubro = await _catalogoService.GetRubroByIDAsync(id);
                if (rubro == null)
                    return NotFound();

                var model = _mapper.Map<RubroViewModel>(rubro);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en EditRubro GET");
                return View("Error");
            }
        }

        // POST: Catalogo/EditRubro/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRubro(RubroViewModel model)
        {
            if (!ModelState.IsValid)
            {
                LogModelStateErrors();
                return View(model);
            }

            try
            {
                var rubro = _mapper.Map<Rubro>(model);
                await _catalogoService.UpdateRubroAsync(rubro);
                _logger.LogInformation("Rubro actualizado: ID={ID}", model.ID);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar Rubro");
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        // GET: Catalogo/EditSubRubros/5
        public async Task<IActionResult> EditSubRubros(int rubroId)
        {
            try
            {
                var rubro = await _catalogoService.GetRubroByIDAsync(rubroId);
                if (rubro == null)
                    return NotFound();

                var model = new EditSubRubrosViewModel
                {
                    RubroID = rubro.ID,
                    RubroNombre = rubro.Nombre,
                    SubRubros = _mapper.Map<List<SubRubroEditViewModel>>(rubro.SubRubros)
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en EditSubRubros GET");
                return View("Error");
            }
        }

        // POST: Catalogo/EditSubRubros
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
                await _catalogoService.UpdateSubRubrosAsync(model);
                _logger.LogInformation("SubRubros actualizados para RubroID: {ID}", model.RubroID);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar SubRubros");
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        // GET: Catalogo/CreateMarca
        public IActionResult CreateMarca()
        {
            return View(new MarcaViewModel());
        }

        // POST: Catalogo/CreateMarca
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMarca(MarcaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                LogModelStateErrors();
                return View(model);
            }

            try
            {
                var marca = _mapper.Map<Marca>(model);
                await _catalogoService.CreateMarcaAsync(marca);
                _logger.LogInformation("Marca creada: {Nombre}", model.Nombre);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear Marca");
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        // GET: Catalogo/EditMarca/5
        public async Task<IActionResult> EditMarca(int id)
        {
            try
            {
                var marca = await _catalogoService.GetMarcaByIDAsync(id);
                if (marca == null)
                    return NotFound();

                var model = _mapper.Map<MarcaViewModel>(marca);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en EditMarca GET");
                return View("Error");
            }
        }

        // POST: Catalogo/EditMarca/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMarca(MarcaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                LogModelStateErrors();
                return View(model);
            }

            try
            {
                var marca = _mapper.Map<Marca>(model);
                await _catalogoService.UpdateMarcaAsync(marca);
                _logger.LogInformation("Marca actualizada: ID={ID}", model.ID);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar Marca");
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        // GET: Catalogo/DeleteRubro/5
        public async Task<IActionResult> DeleteRubro(int id)
        {
            try
            {
                var rubro = await _catalogoService.GetRubroByIDAsync(id);
                if (rubro == null)
                    return NotFound();

                var model = _mapper.Map<RubroViewModel>(rubro);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en DeleteRubro GET");
                return View("Error");
            }
        }

        // POST: Catalogo/DeleteRubro/5
        [HttpPost, ActionName("DeleteRubro")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRubroConfirmed(int id)
        {
            try
            {
                await _catalogoService.DeleteRubroAsync(id);
                _logger.LogInformation("Rubro eliminado: ID={ID}", id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar Rubro");
                TempData["Error"] = $"Error al eliminar: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Catalogo/DeleteMarca/5
        public async Task<IActionResult> DeleteMarca(int id)
        {
            try
            {
                var marca = await _catalogoService.GetMarcaByIDAsync(id);
                if (marca == null)
                    return NotFound();

                var model = _mapper.Map<MarcaViewModel>(marca);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en DeleteMarca GET");
                return View("Error");
            }
        }

        // POST: Catalogo/DeleteMarca/5
        [HttpPost, ActionName("DeleteMarca")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMarcaConfirmed(int id)
        {
            try
            {
                await _catalogoService.DeleteMarcaAsync(id);
                _logger.LogInformation("Marca eliminada: ID={ID}", id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar Marca");
                TempData["Error"] = $"Error al eliminar: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Catalogo/FilterAsync
        [HttpGet]
        public async Task<IActionResult> FilterAsync([FromQuery] CatalogoFilterDto filters)
        {
            try
            {
                _logger.LogInformation("FilterAsync: {@Filters}", filters);
                var rubros = await _catalogoService.FilterRubrosAsync(filters);
                var marcas = await _catalogoService.FilterMarcasAsync(filters);
                var response = await GenerateRubrosMarcasPartialsAsync(rubros, marcas);
                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en FilterAsync");
                return Json(new { rubrosPartial = "", marcasPartial = "" });
            }
        }

        // Métodos auxiliares
        private async Task<CatalogoIndexViewModel> GetCatalogoIndexViewModelAsync(
            IEnumerable<Rubro>? rubros = null,
            IEnumerable<Marca>? marcas = null)
        {
            rubros ??= await _catalogoService.GetRubrosAsync();
            marcas ??= await _catalogoService.GetMarcasAsync();

            return new CatalogoIndexViewModel
            {
                Rubros = _mapper.Map<List<RubroViewModel>>(rubros),
                Marcas = _mapper.Map<List<MarcaViewModel>>(marcas)
            };
        }
        // Para crear Rubro vía AJAX
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRubroAjax([FromForm] string Nombre)
        {
            if (string.IsNullOrWhiteSpace(Nombre))
            {
                return Json(new { success = false, message = "El nombre es obligatorio" });
            }

            try
            {
                var rubro = new Rubro { Nombre = Nombre };
                await _catalogoService.CreateRubroAsync(rubro);
                _logger.LogInformation("Rubro creado vía AJAX: {Nombre}", Nombre);
                return Json(new { success = true, id = rubro.ID, name = rubro.Nombre });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear Rubro vía AJAX");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Para crear SubRubro vía AJAX
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSubRubroAjax([FromForm] string Nombre, [FromForm] int RubroID)
        {
            if (string.IsNullOrWhiteSpace(Nombre))
            {
                return Json(new { success = false, message = "El nombre es obligatorio" });
            }

            try
            {
                var subRubro = new SubRubro { Nombre = Nombre, RubroID = RubroID };
                await _catalogoService.CreateSubRubroAsync(subRubro);
                _logger.LogInformation("SubRubro creado vía AJAX: {Nombre} para RubroID {RubroID}", Nombre, RubroID);
                return Json(new { success = true, id = subRubro.ID, name = subRubro.Nombre });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear SubRubro vía AJAX");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Para crear Marca vía AJAX
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMarcaAjax([FromForm] string Nombre)
        {
            if (string.IsNullOrWhiteSpace(Nombre))
            {
                return Json(new { success = false, message = "El nombre es obligatorio" });
            }

            try
            {
                var marca = new Marca { Nombre = Nombre };
                await _catalogoService.CreateMarcaAsync(marca);
                _logger.LogInformation("Marca creada vía AJAX: {Nombre}", Nombre);
                return Json(new { success = true, id = marca.ID, name = marca.Nombre });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear Marca vía AJAX");
                return Json(new { success = false, message = ex.Message });
            }
        }
        private async Task PopulateTotalStockForRubrosAndMarcas(CatalogoIndexViewModel model)
        {
            var (rubrosStock, marcasStock) = await _productoService.GetRubrosMarcasStockAsync();

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
        }

        private async Task<object> GenerateRubrosMarcasPartialsAsync(
            IEnumerable<Rubro> rubros,
            IEnumerable<Marca> marcas)
        {
            var model = await GetCatalogoIndexViewModelAsync(rubros, marcas);
            await PopulateTotalStockForRubrosAndMarcas(model);

            var rubrosPartial = await this.RenderViewAsync("_RubrosTable", model.Rubros, true);
            var marcasPartial = await this.RenderViewAsync("_MarcasTable", model.Marcas, true);

            return new { rubrosPartial, marcasPartial };
        }
    }
}