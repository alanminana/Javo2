// Controllers/Catalog/ProductosBaseController.cs
using AutoMapper;
using Javo2.Controllers.Base;
using Javo2.DTOs;
using Javo2.IServices;
using Javo2.Models;
using Javo2.Services;
using Javo2.ViewModels.Operaciones.Productos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Javo2.Controllers.Catalog
{
    [Authorize(Policy = "PermisoPolitica")]
    public abstract class ProductosBaseController : BaseController
    {
        protected readonly IProductoService _productoService;
        protected readonly IProductSearchService _productSearchService;
        protected readonly ICatalogoService _catalogoService;
        protected readonly IAuditoriaService _auditoriaService;
        protected readonly IMapper _mapper;

        public ProductosBaseController(
            IProductoService productoService,
            IProductSearchService productSearchService,
            ICatalogoService catalogoService,
            IAuditoriaService auditoriaService,
            IMapper mapper,
            ILogger logger) : base(logger)
        {
            _productoService = productoService;
            _productSearchService = productSearchService;
            _catalogoService = catalogoService;
            _auditoriaService = auditoriaService;
            _mapper = mapper;
        }

        #region Métodos Comunes API

        [HttpGet]
        public async Task<IActionResult> GetSubRubros(int rubroId)
        {
            LogInfo("GetSubRubros recibido para rubroId: {0}", rubroId);

            if (rubroId <= 0)
            {
                LogWarning("GetSubRubros: rubroId inválido: {0}", rubroId);
                return Json(new List<SelectListItem>());
            }

            try
            {
                var items = await _productSearchService.GetSubRubrosSelectListAsync(rubroId);
                LogInfo("GetSubRubros: Obtenidos {0} subrubros", items.Count());
                return Json(items);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error obteniendo subrubros para rubroId {0}", rubroId);
                return Json(new List<SelectListItem>());
            }
        }

        [HttpPost]
        [Authorize(Policy = "Permission:productos.ver")]
        public async Task<IActionResult> BuscarProducto(string codigoProducto)
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

        [HttpGet]
        [Authorize(Policy = "Permission:productos.ver")]
        public async Task<IActionResult> Filter(ProductoFilterDtoViewModel filters)
        {
            try
            {
                var dto = new ProductoFilterDto
                {
                    Nombre = filters.Nombre,
                    Categoria = filters.Categoria,
                    PrecioMinimo = filters.PrecioMinimo,
                    PrecioMaximo = filters.PrecioMaximo,
                    Codigo = filters.Codigo,
                    Rubro = filters.Rubro,
                    SubRubro = filters.SubRubro,
                    Marca = filters.Marca
                };

                var productos = await _productSearchService.FilterProductsAsync(dto);
                return PartialView("_ProductosTable", _mapper.Map<List<ProductosViewModel>>(productos));
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al filtrar productos: {Filtros}", filters);
                return PartialView("_ProductosTable", new List<ProductosViewModel>());
            }
        }

        #endregion

        #region Métodos Comunes Auditoría

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

        #region Métodos Comunes Stock

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
    }
}