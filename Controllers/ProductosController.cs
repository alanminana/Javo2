﻿// Archivo: Controllers/ProductosController.cs
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
            IAuditoriaService auditoriaService,
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
            _logger.LogInformation("ProductosController: Index GET");
            var productos = await _productoService.GetAllProductosAsync();
            var model = _mapper.Map<List<ProductosViewModel>>(productos);
            return View(model);
        }

        // GET: Productos/Details/5
        public async Task<IActionResult> Details(int id)
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

        // GET: Productos/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            _logger.LogInformation("ProductosController: Create GET");
            var model = new ProductosViewModel();
            await PopulateDropdownsAsync(model);
            return View("Form", model);
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
            ModelState.Remove(nameof(model.ModificadoPor)); // Para no invalidar

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState inválido en Create => Errores: {Errors}",
                    string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));

                await PopulateDropdownsAsync(model);
                return View("Form", model);
            }

            // Validar marca/rubro/subrubro
            var marca = await _catalogoService.GetMarcaByIDAsync(model.SelectedMarcaID);
            var rubro = await _catalogoService.GetRubroByIDAsync(model.SelectedRubroID);
            var subRubro = await _catalogoService.GetSubRubroByIDAsync(model.SelectedSubRubroID);

            if (marca == null)
            {
                _logger.LogWarning("Marca con ID={ID} no encontrada", model.SelectedMarcaID);
                ModelState.AddModelError(string.Empty, "Marca inválida.");
                await PopulateDropdownsAsync(model);
                return View("Form", model);
            }
            if (rubro == null)
            {
                _logger.LogWarning("Rubro con ID={ID} no encontrado", model.SelectedRubroID);
                ModelState.AddModelError(string.Empty, "Rubro inválido.");
                await PopulateDropdownsAsync(model);
                return View("Form", model);
            }
            if (subRubro == null)
            {
                _logger.LogWarning("SubRubro con ID={ID} no encontrado", model.SelectedSubRubroID);
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
                _logger.LogWarning("ModelState inválido al Editar => Errores: {Errors}",
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
                await _productoService.DeleteProductoAsync(id);
                _logger.LogInformation("Producto ID={ID} eliminado con éxito", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar producto ID={ID}", id);
                ModelState.AddModelError(string.Empty, $"Error al eliminar producto: {ex.Message}");
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetSubRubros(int rubroId)
        {
            _logger.LogInformation("GetSubRubros GET => rubroId={rubroId}", rubroId);
            var subRubros = await _dropdownService.GetSubRubrosAsync(rubroId);
            _logger.LogInformation("SubRubros encontrados: {Count}", subRubros.Count);
            return Json(subRubros);
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
    }
}
