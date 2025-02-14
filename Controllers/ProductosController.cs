// Archivo: Controllers/ProductosController.cs
// Cambios realizados:
// - Se agregó manejo de excepciones (try/catch) en los métodos GET (Index, Details, Create, Edit y GetSubRubros)
//   para mejorar la robustez del controlador.
// - Se agregó el retorno de una vista de error ("Error") en caso de excepción.
// - Se mantuvieron los try/catch existentes en los métodos POST y se agregó manejo en GetSubRubros.

using AutoMapper;
using Javo2.Controllers.Base;
using Javo2.DTOs;
using Javo2.IServices;
using Javo2.IServices.Common;
using Javo2.Models;
using Javo2.ViewModels.Operaciones.Productos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Javo2.Controllers
{
    public class ProductosController : BaseController
    {
        private readonly IProductoService _productoService;
        private readonly IDropdownService _dropdownService;
        private readonly ICatalogoService _catalogoService;
        private readonly IMapper _mapper;
        private readonly IAuditoriaService _auditoriaService;

        public ProductosController(
            IProductoService productoService,
            IDropdownService dropdownService,
            ICatalogoService catalogoService,
            IMapper mapper,
            IAuditoriaService auditoriaService,     // Inyección de auditoría
            ILogger<ProductosController> logger
        ) : base(logger)
        {
            _productoService = productoService;
            _dropdownService = dropdownService;
            _catalogoService = catalogoService;
            _mapper = mapper;
            _auditoriaService = auditoriaService;
        }

        // GET: Productos
        public async Task<IActionResult> Index()
        {
            try // Cambio: Agregado manejo de excepciones en GET Index
            {
                _logger.LogInformation("ProductosController: Index GET");
                var productos = await _productoService.GetAllProductosAsync();
                var model = _mapper.Map<List<ProductosViewModel>>(productos);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Index GET de Productos");
                return View("Error");
            }
        }

        // GET: Productos/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try // Cambio: Agregado manejo de excepciones en GET Details
            {
                _logger.LogInformation("ProductosController: Details GET con ID={ID}", id);
                var producto = await _productoService.GetProductoByIDAsync(id);
                if (producto == null)
                {
                    _logger.LogWarning("Producto con ID={ID} no encontrado en Details", id);
                    return NotFound();
                }
                var model = _mapper.Map<ProductosViewModel>(producto);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Details GET de Productos");
                return View("Error");
            }
        }

        // GET: Productos/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try // Cambio: Agregado manejo de excepciones en GET Create
            {
                _logger.LogInformation("ProductosController: Create GET");
                var model = new ProductosViewModel();
                await PopulateDropdownsAsync(model);
                return View("Form", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Create GET de Productos");
                return View("Error");
            }
        }

        // POST: Productos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductosViewModel model)
        {
            _logger.LogInformation("ProductosController: Create POST => Nombre={Nombre}, RubroID={RubroID}, SubRubroID={SubRubroID}, MarcaID={MarcaID}",
                model.Nombre, model.SelectedRubroID, model.SelectedSubRubroID, model.SelectedMarcaID);

            // Rellenar usuario
            model.ModificadoPor = User.Identity?.Name ?? "UsuarioDesconocido";
            ModelState.Remove(nameof(model.ModificadoPor)); // Evita invalidación por este campo

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState inválido en Create POST => Errores: {Errors}",
                    string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                await PopulateDropdownsAsync(model);
                return View("Form", model);
            }

            // Validar marca/rubro/subrubro
            var marca = await _catalogoService.GetMarcaByIDAsync(model.SelectedMarcaID);
            var rubro = await _catalogoService.GetRubroByIDAsync(model.SelectedRubroID);
            var subRubro = await _catalogoService.GetSubRubroByIDAsync(model.SelectedSubRubroID);

            if (marca == null || rubro == null || subRubro == null)
            {
                if (marca == null)
                    ModelState.AddModelError(string.Empty, "Marca inválida.");
                if (rubro == null)
                    ModelState.AddModelError(string.Empty, "Rubro inválido.");
                if (subRubro == null)
                    ModelState.AddModelError(string.Empty, "SubRubro inválido.");

                await PopulateDropdownsAsync(model);
                return View("Form", model);
            }

            var producto = _mapper.Map<Producto>(model);
            producto.Marca = marca;
            producto.Rubro = rubro;
            producto.SubRubro = subRubro;

            _logger.LogInformation("Creando nuevo producto => Nombre={Nombre}, Marca={Marca}, Rubro={Rubro}, SubRubro={SubRubro}",
                producto.Nombre, producto.Marca?.Nombre, producto.Rubro?.Nombre, producto.SubRubro?.Nombre);

            try
            {
                await _productoService.CreateProductoAsync(producto);
                _logger.LogInformation("Producto creado exitosamente con ID={ID}", producto.ProductoID);

                // Registro de auditoría: Creación
                await _auditoriaService.RegistrarCambioAsync(new AuditoriaRegistro
                {
                    FechaHora = DateTime.Now,
                    Usuario = User.Identity?.Name ?? "Desconocido",
                    Entidad = "Producto",
                    Accion = "Create",
                    LlavePrimaria = producto.ProductoID.ToString(),
                    Detalle = $"Nombre={producto.Nombre}, Rubro={rubro.Nombre}, Marca={marca.Nombre}"
                });

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción al crear producto: {Message}", ex.Message);
                ModelState.AddModelError(string.Empty, $"Error al crear producto: {ex.Message}");
                await PopulateDropdownsAsync(model);
                return View("Form", model);
            }
        }

        // GET: Productos/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try // Cambio: Agregado manejo de excepciones en GET Edit
            {
                _logger.LogInformation("ProductosController: Edit GET con ID={ID}", id);
                var producto = await _productoService.GetProductoByIDAsync(id);
                if (producto == null)
                {
                    _logger.LogWarning("Producto ID={ID} no encontrado al Editar", id);
                    return NotFound();
                }

                var model = _mapper.Map<ProductosViewModel>(producto);
                await PopulateDropdownsAsync(model);
                return View("Form", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Edit GET de Productos");
                return View("Error");
            }
        }

        // POST: Productos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductosViewModel model)
        {
            _logger.LogInformation("ProductosController: Edit POST => ProductoID={ProductoID}, Nombre={Nombre}",
                model.ProductoID, model.Nombre);

            model.ModificadoPor = User.Identity?.Name ?? "UsuarioDesconocido";
            ModelState.Remove(nameof(model.ModificadoPor));

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState inválido al Editar POST => Errores: {Errors}",
                    string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                await PopulateDropdownsAsync(model);
                return View("Form", model);
            }

            try
            {
                var producto = _mapper.Map<Producto>(model);

                _logger.LogInformation("Actualizando producto ID={ID}, Nombre={Nombre}",
                    producto.ProductoID, producto.Nombre);

                await _productoService.UpdateProductoAsync(producto);
                _logger.LogInformation("Producto ID={ID} actualizado correctamente", producto.ProductoID);

                // Registro de auditoría: Update
                await _auditoriaService.RegistrarCambioAsync(new AuditoriaRegistro
                {
                    FechaHora = DateTime.Now,
                    Usuario = User.Identity?.Name ?? "Desconocido",
                    Entidad = "Producto",
                    Accion = "Update",
                    LlavePrimaria = producto.ProductoID.ToString(),
                    Detalle = $"Nombre={producto.Nombre}"
                });

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción al actualizar producto: {Message}", ex.Message);
                ModelState.AddModelError(string.Empty, $"Error al actualizar producto: {ex.Message}");
                await PopulateDropdownsAsync(model);
                return View("Form", model);
            }
        }

        // POST: Productos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            _logger.LogInformation("ProductosController: Delete POST => ID={ID}", id);
            try
            {
                var producto = await _productoService.GetProductoByIDAsync(id);
                if (producto == null)
                {
                    _logger.LogWarning("Producto ID={ID} no encontrado para eliminar", id);
                    return NotFound();
                }

                await _productoService.DeleteProductoAsync(id);
                _logger.LogInformation("Producto ID={ID} eliminado con éxito", id);

                // Registro de auditoría: Delete
                await _auditoriaService.RegistrarCambioAsync(new AuditoriaRegistro
                {
                    FechaHora = DateTime.Now,
                    Usuario = User.Identity?.Name ?? "Desconocido",
                    Entidad = "Producto",
                    Accion = "Delete",
                    LlavePrimaria = producto.ProductoID.ToString(),
                    Detalle = $"Nombre={producto.Nombre}"
                });

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar producto ID={ID}", id);
                ModelState.AddModelError(string.Empty, $"Error al eliminar producto: {ex.Message}");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSubRubros(int rubroId)
        {
            try // Cambio: Agregado manejo de excepciones en GetSubRubros
            {
                _logger.LogInformation("GetSubRubros GET => rubroId={rubroId}", rubroId);
                var subRubros = await _dropdownService.GetSubRubrosAsync(rubroId);
                _logger.LogInformation("SubRubros encontrados: {Count}", subRubros.Count);
                return Json(subRubros);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetSubRubros GET");
                return Json(new List<SelectListItem>());
            }
        }

        private async Task PopulateDropdownsAsync(ProductosViewModel model)
        {
            _logger.LogInformation("PopulateDropdownsAsync => Cargando Rubros, Marcas, SubRubros para la vista");
            model.Rubros = await _dropdownService.GetRubrosAsync();
            model.Marcas = await _dropdownService.GetMarcasAsync();

            if (model.SelectedRubroID > 0)
            {
                model.SubRubros = await _dropdownService.GetSubRubrosAsync(model.SelectedRubroID);
                _logger.LogInformation("Cargando subRubros para RubroID={ID}, encontrados {Count}",
                    model.SelectedRubroID, model.SubRubros.Count());
            }
            else
            {
                model.SubRubros = new List<SelectListItem>();
            }
        }

        private void LogModelStateErrors()
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            _logger.LogWarning("ModelState inválido => {Errors}", string.Join(" | ", errors));
        }

        // EJEMPLO OPCIONAL: Ajustar precios en lote
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdjustPrices(int[] productIDs, decimal porcentaje, bool isAumento)
        {
            _logger.LogInformation("AdjustPrices POST => ProductIDs={@ProductIDs}, Porcentaje={Porc}, Aumento={IsAumento}",
                productIDs, porcentaje, isAumento);
            try
            {
                await _productoService.AdjustPricesAsync(productIDs, porcentaje, isAumento);

                // Registrar auditoría de ajuste masivo
                await _auditoriaService.RegistrarCambioAsync(new AuditoriaRegistro
                {
                    FechaHora = DateTime.Now,
                    Usuario = User.Identity?.Name ?? "Desconocido",
                    Entidad = "Producto",
                    Accion = "UpdatePrices",
                    LlavePrimaria = string.Join(",", productIDs),
                    Detalle = $"Ajuste de precios en {porcentaje}% (Aumento={isAumento})"
                });

                TempData["Success"] = "Precios ajustados exitosamente.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al ajustar precios masivamente");
                TempData["Error"] = "No se pudo ajustar los precios.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
