// Controllers/Catalog/CatalogoController.cs
using AutoMapper;
using Javo2.Controllers.Base;
using Javo2.Helpers;
using Javo2.IServices;
using Javo2.Models;
using Javo2.Services;
using Javo2.ViewModels.Operaciones.Catalogo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Javo2.Controllers.Catalog
{
    [Authorize(Policy = "PermisoPolitica")]
    public class CatalogoController : BaseController
    {
        private readonly ICatalogoService _catalogoService;
        private readonly IProductSearchService _productSearchService;
        private readonly IMapper _mapper;

        public CatalogoController(
            ICatalogoService catalogoService,
            IProductSearchService productSearchService,
            IMapper mapper,
            ILogger<CatalogoController> logger)
            : base(logger)
        {
            _catalogoService = catalogoService;
            _productSearchService = productSearchService;
            _mapper = mapper;
        }

        #region Vistas Principales

        // GET: Catalogo
        [Authorize(Policy = "Permission:catalogo.ver")]
        public async Task<IActionResult> Index()
        {
            try
            {
                LogInfo("CatalogoController: Index GET");
                var model = await GetCatalogoIndexViewModelAsync();
                await PopulateTotalStockForRubrosAndMarcas(model);
                return View(model);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error en Index de Catalogo");
                return View("Error");
            }
        }

        #endregion

        #region Gestión de Rubros

        // GET: Catalogo/CreateRubro
        [Authorize(Policy = "Permission:catalogo.crear")]
        public IActionResult CreateRubro()
        {
            return View(new RubroViewModel());
        }

        // POST: Catalogo/CreateRubro
        [HttpPost]
        [Authorize(Policy = "Permission:catalogo.crear")]
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
                LogInfo("Rubro creado: {Nombre}", model.Nombre);
                SetSuccessMessage("Rubro creado correctamente");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al crear Rubro");
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        // GET: Catalogo/EditRubro/5
        [Authorize(Policy = "Permission:catalogo.editar")]
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
                LogError(ex, "Error en EditRubro GET");
                return View("Error");
            }
        }

        // POST: Catalogo/EditRubro/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:catalogo.editar")]
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
                LogInfo("Rubro actualizado: ID={ID}", model.ID);
                SetSuccessMessage("Rubro actualizado correctamente");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al actualizar Rubro");
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        // GET: Catalogo/DeleteRubro/5
        [Authorize(Policy = "Permission:catalogo.eliminar")]
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
                LogError(ex, "Error en DeleteRubro GET");
                return View("Error");
            }
        }

        // POST: Catalogo/DeleteRubro/5
        [HttpPost, ActionName("DeleteRubro")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:catalogo.eliminar")]
        public async Task<IActionResult> DeleteRubroConfirmed(int id)
        {
            try
            {
                await _catalogoService.DeleteRubroAsync(id);
                LogInfo("Rubro eliminado: ID={ID}", id);
                SetSuccessMessage("Rubro eliminado correctamente");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al eliminar Rubro");
                SetErrorMessage($"Error al eliminar: {ex.Message}");
                return RedirectToAction(nameof(Index));
            }
        }

        #endregion

        #region Gestión de SubRubros

        // GET: Catalogo/EditSubRubros/5
        [Authorize(Policy = "Permission:catalogo.editar")]
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
                LogError(ex, "Error en EditSubRubros GET");
                return View("Error");
            }
        }

        // POST: Catalogo/EditSubRubros
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:catalogo.editar")]
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
                LogInfo("SubRubros actualizados para RubroID: {ID}", model.RubroID);
                SetSuccessMessage("SubRubros actualizados correctamente");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al actualizar SubRubros");
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        #endregion

        #region Gestión de Marcas

        // GET: Catalogo/CreateMarca
        [Authorize(Policy = "Permission:catalogo.crear")]
        public IActionResult CreateMarca()
        {
            return View(new MarcaViewModel());
        }

        // POST: Catalogo/CreateMarca
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:catalogo.crear")]
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
                LogInfo("Marca creada: {Nombre}", model.Nombre);
                SetSuccessMessage("Marca creada correctamente");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al crear Marca");
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        // GET: Catalogo/EditMarca/5
        [Authorize(Policy = "Permission:catalogo.editar")]
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
                LogError(ex, "Error en EditMarca GET");
                return View("Error");
            }
        }

        // POST: Catalogo/EditMarca/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:catalogo.editar")]
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
                LogInfo("Marca actualizada: ID={ID}", model.ID);
                SetSuccessMessage("Marca actualizada correctamente");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al actualizar Marca");
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        // GET: Catalogo/DeleteMarca/5
        [Authorize(Policy = "Permission:catalogo.eliminar")]
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
                LogError(ex, "Error en DeleteMarca GET");
                return View("Error");
            }
        }

        // POST: Catalogo/DeleteMarca/5
        [HttpPost, ActionName("DeleteMarca")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:catalogo.eliminar")]
        public async Task<IActionResult> DeleteMarcaConfirmed(int id)
        {
            try
            {
                await _catalogoService.DeleteMarcaAsync(id);
                LogInfo("Marca eliminada: ID={ID}", id);
                SetSuccessMessage("Marca eliminada correctamente");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al eliminar Marca");
                SetErrorMessage($"Error al eliminar: {ex.Message}");
                return RedirectToAction(nameof(Index));
            }
        }

        #endregion

        #region Filtros AJAX

        // GET: Catalogo/FilterAsync
        [HttpGet]
        public async Task<IActionResult> FilterAsync([FromQuery] CatalogoFilterDto filters)
        {
            try
            {
                LogInfo("FilterAsync: {@Filters}", filters);
                var rubros = await _catalogoService.FilterRubrosAsync(filters);
                var marcas = await _catalogoService.FilterMarcasAsync(filters);
                var response = await GenerateRubrosMarcasPartialsAsync(rubros, marcas);
                return Json(response);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error en FilterAsync");
                return Json(new { rubrosPartial = "", marcasPartial = "" });
            }
        }

        // Para crear Rubro vía AJAX
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:catalogo.crear")]
        public async Task<IActionResult> CreateRubroAjax([FromForm] string Nombre)
        {
            LogInfo("CatalogoController: CreateRubroAjax con Nombre={Nombre}", Nombre);

            if (string.IsNullOrWhiteSpace(Nombre))
            {
                LogWarning("CatalogoController: CreateRubroAjax recibió un nombre vacío");
                return JsonError("El nombre es obligatorio");
            }

            try
            {
                var rubro = new Rubro { Nombre = Nombre };
                await _catalogoService.CreateRubroAsync(rubro);
                LogInfo("CatalogoController: Rubro creado vía AJAX con ID={ID}, Nombre={Nombre}",
                    rubro.ID, Nombre);
                return JsonSuccess(null, new { id = rubro.ID, name = rubro.Nombre });
            }
            catch (Exception ex)
            {
                LogError(ex, "CatalogoController: Error al crear Rubro vía AJAX con Nombre={Nombre}", Nombre);
                return JsonError(ex.Message);
            }
        }

        // Para crear SubRubro vía AJAX
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:catalogo.crear")]
        public async Task<IActionResult> CreateSubRubroAjax([FromForm] string Nombre, [FromForm] int RubroID)
        {
            LogInfo("CatalogoController: CreateSubRubroAjax con Nombre={Nombre}, RubroID={RubroID}",
                Nombre, RubroID);

            if (string.IsNullOrWhiteSpace(Nombre))
            {
                LogWarning("CatalogoController: CreateSubRubroAjax recibió un nombre vacío");
                return JsonError("El nombre es obligatorio");
            }

            if (RubroID <= 0)
            {
                LogWarning("CatalogoController: CreateSubRubroAjax recibió un RubroID inválido: {RubroID}", RubroID);
                return JsonError("El Rubro es obligatorio");
            }

            try
            {
                var rubro = await _catalogoService.GetRubroByIDAsync(RubroID);
                if (rubro == null)
                {
                    LogWarning("CatalogoController: CreateSubRubroAjax no encontró el Rubro con ID={RubroID}", RubroID);
                    return JsonError("El Rubro seleccionado no existe");
                }

                var subRubro = new SubRubro { Nombre = Nombre, RubroID = RubroID };
                await _catalogoService.CreateSubRubroAsync(subRubro);
                LogInfo("CatalogoController: SubRubro creado vía AJAX con ID={ID}, Nombre={Nombre}, RubroID={RubroID}",
                    subRubro.ID, Nombre, RubroID);
                return JsonSuccess(null, new { id = subRubro.ID, name = subRubro.Nombre });
            }
            catch (Exception ex)
            {
                LogError(ex, "CatalogoController: Error al crear SubRubro vía AJAX con Nombre={Nombre}, RubroID={RubroID}",
                    Nombre, RubroID);
                return JsonError(ex.Message);
            }
        }

        // Para crear Marca vía AJAX
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:catalogo.crear")]
        public async Task<IActionResult> CreateMarcaAjax([FromForm] string Nombre)
        {
            if (string.IsNullOrWhiteSpace(Nombre))
            {
                return JsonError("El nombre es obligatorio");
            }

            try
            {
                var marca = new Marca { Nombre = Nombre };
                await _catalogoService.CreateMarcaAsync(marca);
                LogInfo("Marca creada vía AJAX: {Nombre}", Nombre);
                return JsonSuccess(null, new { id = marca.ID, name = marca.Nombre });
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al crear Marca vía AJAX");
                return JsonError(ex.Message);
            }
        }

        #endregion

        #region Métodos Auxiliares

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

        private async Task PopulateTotalStockForRubrosAndMarcas(CatalogoIndexViewModel model)
        {
            var (rubrosStock, marcasStock) = await _productSearchService.GetStockByRubroMarcaAsync();

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

        #endregion
    }
}