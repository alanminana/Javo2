// Controllers/Catalog/CatalogBaseController.cs
using AutoMapper;
using Javo2.Controllers.Base;
using Javo2.DTOs;
using Javo2.IServices;
using Javo2.Models;
using Javo2.Services.Catalog;
using Javo2.ViewModels.Operaciones.Catalogo;
using Javo2.ViewModels.Operaciones.Productos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Javo2.Controllers.Catalog
{
    public abstract class CatalogBaseController : BaseController
    {
        protected readonly ICatalogoService _catalogoService;
        protected readonly IProductSearchService _productSearchService;
        protected readonly IProductoService _productoService;
        protected readonly IAuditoriaService _auditoriaService;
        protected readonly IMapper _mapper;

        public CatalogBaseController(
            IProductoService productoService,
            ICatalogoService catalogoService,
            IProductSearchService productSearchService,
            IAuditoriaService auditoriaService,
            IMapper mapper,
            ILogger logger) : base(logger)
        {
            _productoService = productoService;
            _catalogoService = catalogoService;
            _productSearchService = productSearchService;
            _auditoriaService = auditoriaService;
            _mapper = mapper;
        }

        #region Métodos comunes de filtrado

        // Métodos de filtrado para rubros
        protected async Task<IActionResult> FilterRubrosAsync(string term)
        {
            try
            {
                var rubros = await _productSearchService.FilterRubrosAsync(term);
                var rubrosVm = _mapper.Map<IEnumerable<RubroViewModel>>(rubros);
                return PartialView("~/Views/Catalogo/_RubrosTable.cshtml", rubrosVm);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error en FilterRubros");
                return Json(new { error = "Error al filtrar rubros" });
            }
        }

        // Métodos de filtrado para marcas
        protected async Task<IActionResult> FilterMarcasAsync(string term)
        {
            try
            {
                var marcas = await _productSearchService.FilterMarcasAsync(term);
                var marcasVm = _mapper.Map<IEnumerable<MarcaViewModel>>(marcas);
                return PartialView("~/Views/Catalogo/_MarcasTable.cshtml", marcasVm);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error en FilterMarcas");
                return Json(new { error = "Error al filtrar marcas" });
            }
        }

        // Método para filtrado de productos por diversos criterios
        protected async Task<IActionResult> FilterProductsAsync(ProductoFilterDto filters)
        {
            try
            {
                var productos = await _productSearchService.FilterProductsAsync(filters);
                var productosVM = _mapper.Map<IEnumerable<ProductosViewModel>>(productos);
                return PartialView("_ProductosTable", productosVM);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error en FilterProducts");
                return PartialView("_ProductosTable", new List<ProductosViewModel>());
            }
        }

        // Métodos para cargar dropdown de subrubros
        protected async Task<IActionResult> GetSubRubrosAsync(int rubroId)
        {
            try
            {
                if (rubroId <= 0)
                {
                    LogWarning("GetSubRubros: rubroId inválido: {0}", rubroId);
                    return Json(new List<SelectListItem>());
                }

                var items = await _productSearchService.GetSubRubrosSelectListAsync(rubroId);
                return Json(items);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error obteniendo subrubros para rubroId {0}", rubroId);
                return Json(new List<SelectListItem>());
            }
        }

        // Método para búsqueda de productos por código
        protected async Task<IActionResult> BuscarProductoPorCodigoAsync(string codigoProducto)
        {
            try
            {
                var producto = await _productSearchService.GetProductByCodeAsync(codigoProducto);

                if (producto == null)
                {
                    // Intentar buscar por término si no se encuentra por código exacto
                    var productos = await _productSearchService.SearchProductsByTermAsync(codigoProducto);
                    producto = productos.FirstOrDefault();

                    if (producto == null)
                        return JsonError("Producto no encontrado.");
                }

                return JsonSuccess(null, new
                {
                    productoID = producto.ProductoID,
                    codigoBarra = producto.CodigoBarra,
                    codigoAlfa = producto.CodigoAlfa,
                    nombre = producto.Nombre,
                    descripcion = producto.Descripcion,
                    pCosto = producto.PCosto,
                    pContado = producto.PContado,
                    pLista = producto.PLista,
                    marcaId = producto.MarcaID,
                    marca = producto.Marca?.Nombre ?? "",
                    rubroId = producto.RubroID,
                    rubro = producto.Rubro?.Nombre ?? "",
                    subRubroId = producto.SubRubroID,
                    subRubro = producto.SubRubro?.Nombre ?? ""
                });
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al buscar producto por código: {Codigo}", codigoProducto);
                return JsonError("Error al buscar el producto.");
            }
        }

        #endregion

        #region Métodos comunes de auditoría

        // Registrar auditoría para operaciones de productos
        protected async Task RegistrarAuditoriaProductoAsync(string accion, int productoId, string nombre, string detalleExtra = null)
        {
            var detalle = $"Producto: {nombre}";
            if (!string.IsNullOrEmpty(detalleExtra))
            {
                detalle += $", {detalleExtra}";
            }

            await _auditoriaService.RegistrarCambioAsync(new AuditoriaRegistro
            {
                FechaHora = DateTime.Now,
                Usuario = User.Identity?.Name ?? "Sistema",
                Entidad = "Producto",
                Accion = accion,
                LlavePrimaria = productoId.ToString(),
                Detalle = detalle
            });
        }

        // Registrar auditoría para operaciones en catálogo (rubros, marcas, etc.)
        protected async Task RegistrarAuditoriaCatalogoAsync(string entidad, string accion, int id, string nombre, string detalleExtra = null)
        {
            var detalle = $"{entidad}: {nombre}";
            if (!string.IsNullOrEmpty(detalleExtra))
            {
                detalle += $", {detalleExtra}";
            }

            await _auditoriaService.RegistrarCambioAsync(new AuditoriaRegistro
            {
                FechaHora = DateTime.Now,
                Usuario = User.Identity?.Name ?? "Sistema",
                Entidad = entidad,
                Accion = accion,
                LlavePrimaria = id.ToString(),
                Detalle = detalle
            });
        }

        // Registrar auditoría para ajustes de precios
        protected async Task RegistrarAuditoriaAjustePrecioAsync(string accion, int[] productosIds, bool esAumento, decimal porcentaje, string descripcion)
        {
            await _auditoriaService.RegistrarCambioAsync(new AuditoriaRegistro
            {
                FechaHora = DateTime.Now,
                Usuario = User.Identity?.Name ?? "Sistema",
                Entidad = "Producto",
                Accion = "UpdatePrices",
                LlavePrimaria = string.Join(',', productosIds),
                Detalle = $"{(esAumento ? "Aumento" : "Descuento")} {porcentaje}%, {descripcion}"
            });
        }

        #endregion

        #region Métodos comunes para gestión de stock

        // Método para manejar actualizaciones de stock
        protected async Task<IActionResult> HandleStockUpdateAsync(int productoId, int cantidadCambio, string motivo)
        {
            try
            {
                await _productSearchService.UpdateStockAsync(productoId, cantidadCambio, motivo);

                var producto = await _productoService.GetProductoByIDAsync(productoId);
                string accion = cantidadCambio > 0 ? "Entrada" : "Salida";
                await RegistrarAuditoriaProductoAsync(
                    $"Stock{accion}",
                    productoId,
                    producto?.Nombre ?? "Desconocido",
                    $"{Math.Abs(cantidadCambio)} unidades, {motivo}"
                );

                SetSuccessMessage("Stock actualizado correctamente.");
                return RedirectToAction("Index", "Productos");
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al actualizar stock: {ProductoId}, {Cambio}, {Motivo}",
                    productoId, cantidadCambio, motivo);
                SetErrorMessage("Error al actualizar stock: " + ex.Message);
                return RedirectToAction("Index", "Productos");
            }
        }

        #endregion

        #region Métodos comunes de carga de opciones

        // Método común para cargar dropdowns en productos
        protected async Task PopulateProductDropdownsAsync(ProductosViewModel model)
        {
            // Cargar rubros
            model.Rubros = await _productSearchService.GetRubrosSelectListAsync();

            // Cargar marcas
            model.Marcas = await _productSearchService.GetMarcasSelectListAsync();

            // Cargar subrubros del rubro seleccionado
            if (model.SelectedRubroID > 0)
            {
                LogInfo($"Cargando subrubros para RubroID: {model.SelectedRubroID}");
                model.SubRubros = await _productSearchService.GetSubRubrosSelectListAsync(model.SelectedRubroID);
                LogInfo($"SubRubros cargados: {model.SubRubros.Count()}");
            }
            else if (model.Rubros.Any())
            {
                // Si no hay rubro seleccionado pero hay rubros disponibles, seleccionar el primero
                model.SelectedRubroID = int.Parse(model.Rubros.First().Value);
                model.SubRubros = await _productSearchService.GetSubRubrosSelectListAsync(model.SelectedRubroID);
            }
            else
            {
                model.SubRubros = new List<SelectListItem>();
            }
        }

        #endregion
    }
}