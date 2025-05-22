// Services/ProductSearchService.cs
using Javo2.DTOs;
using Javo2.IServices;
using Javo2.IServices.Common;
using Javo2.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Javo2.Services.Catalog
{
    public class ProductSearchService : IProductSearchService
    {
        private readonly IProductoService _productoService;
        private readonly ICatalogoService _catalogoService;
        private readonly IStockService _stockService;
        private readonly IDropdownService _dropdownService;
        private readonly ILogger<ProductSearchService> _logger;

        public ProductSearchService(
            IProductoService productoService,
            ICatalogoService catalogoService,
            IStockService stockService,
            IDropdownService dropdownService,
            ILogger<ProductSearchService> logger)
        {
            _productoService = productoService;
            _catalogoService = catalogoService;
            _stockService = stockService;
            _dropdownService = dropdownService;
            _logger = logger;
        }

        #region Filtrado y Búsqueda

        public async Task<IEnumerable<Producto>> FilterProductsAsync(ProductoFilterDto filters)
        {
            try
            {
                return await _productoService.FilterProductosAsync(filters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al filtrar productos con filtros: {@Filters}", filters);
                throw;
            }
        }

        public async Task<IEnumerable<Producto>> SearchProductsByTermAsync(string term)
        {
            try
            {
                // Si el término está vacío o es nulo, retornar una lista vacía
                if (string.IsNullOrWhiteSpace(term))
                {
                    return Enumerable.Empty<Producto>();
                }

                return await _productoService.GetProductosByTermAsync(term);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar productos por término: {Term}", term);
                throw;
            }
        }

        public async Task<Producto> GetProductByCodeAsync(string code)
        {
            try
            {
                // Si el código está vacío o es nulo, retornar null
                if (string.IsNullOrWhiteSpace(code))
                {
                    return null;
                }

                return await _productoService.GetProductoByCodigoAsync(code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener producto por código: {Code}", code);
                throw;
            }
        }

        #endregion

        #region Filtros para Catálogo

        public async Task<(IEnumerable<Rubro> Rubros, IEnumerable<Marca> Marcas)> GetCatalogFiltersAsync()
        {
            try
            {
                var rubros = await _catalogoService.GetRubrosAsync();
                var marcas = await _catalogoService.GetMarcasAsync();
                return (rubros, marcas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener filtros para catálogo");
                throw;
            }
        }

        public async Task<IEnumerable<Rubro>> FilterRubrosAsync(string term)
        {
            try
            {
                var rubros = await _catalogoService.GetRubrosAsync();

                // Filtrar por término si existe
                if (!string.IsNullOrWhiteSpace(term))
                {
                    var termLower = term.ToLower();
                    rubros = rubros.Where(r => r.Nombre.ToLower().Contains(termLower)).ToList();
                }

                return rubros;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al filtrar rubros por término: {Term}", term);
                throw;
            }
        }

        public async Task<IEnumerable<Marca>> FilterMarcasAsync(string term)
        {
            try
            {
                var marcas = await _catalogoService.GetMarcasAsync();

                // Filtrar por término si existe
                if (!string.IsNullOrWhiteSpace(term))
                {
                    var termLower = term.ToLower();
                    marcas = marcas.Where(m => m.Nombre.ToLower().Contains(termLower)).ToList();
                }

                return marcas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al filtrar marcas por término: {Term}", term);
                throw;
            }
        }

        public async Task<(Dictionary<int, int> RubrosStock, Dictionary<int, int> MarcasStock)> GetStockByRubroMarcaAsync()
        {
            try
            {
                return await _productoService.GetRubrosMarcasStockAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener stock por rubro y marca");
                throw;
            }
        }

        #endregion

        #region Dropdowns y SelectLists

        public async Task<IEnumerable<SelectListItem>> GetRubrosSelectListAsync()
        {
            try
            {
                return await _dropdownService.GetRubrosAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener lista desplegable de rubros");
                throw;
            }
        }

        public async Task<IEnumerable<SelectListItem>> GetSubRubrosSelectListAsync(int rubroId)
        {
            try
            {
                return await _dropdownService.GetSubRubrosAsync(rubroId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener lista desplegable de subrubros para rubroId: {RubroId}", rubroId);
                throw;
            }
        }

        public async Task<IEnumerable<SelectListItem>> GetMarcasSelectListAsync()
        {
            try
            {
                return await _dropdownService.GetMarcasAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener lista desplegable de marcas");
                throw;
            }
        }

        #endregion

        #region Validación y Preparación de Productos

        public async Task<bool> ValidateProductAsync(Producto producto)
        {
            try
            {
                // Validar relaciones
                if (producto.RubroID <= 0)
                {
                    _logger.LogWarning("Validación: RubroID inválido en producto: {ProductoID}", producto.ProductoID);
                    return false;
                }

                if (producto.SubRubroID <= 0)
                {
                    _logger.LogWarning("Validación: SubRubroID inválido en producto: {ProductoID}", producto.ProductoID);
                    return false;
                }

                if (producto.MarcaID <= 0)
                {
                    _logger.LogWarning("Validación: MarcaID inválido en producto: {ProductoID}", producto.ProductoID);
                    return false;
                }

                // Verificar si existen las entidades relacionadas
                var rubro = await _catalogoService.GetRubroByIDAsync(producto.RubroID);
                if (rubro == null)
                {
                    _logger.LogWarning("Validación: Rubro no encontrado: {RubroID}", producto.RubroID);
                    return false;
                }

                var subRubro = await _catalogoService.GetSubRubroByIDAsync(producto.SubRubroID);
                if (subRubro == null)
                {
                    _logger.LogWarning("Validación: SubRubro no encontrado: {SubRubroID}", producto.SubRubroID);
                    return false;
                }

                var marca = await _catalogoService.GetMarcaByIDAsync(producto.MarcaID);
                if (marca == null)
                {
                    _logger.LogWarning("Validación: Marca no encontrada: {MarcaID}", producto.MarcaID);
                    return false;
                }

                // Validar que el subrubro pertenezca al rubro
                if (subRubro.RubroID != producto.RubroID)
                {
                    _logger.LogWarning("Validación: SubRubro {SubRubroID} no pertenece al Rubro {RubroID}",
                        producto.SubRubroID, producto.RubroID);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar producto: {ProductoID}", producto.ProductoID);
                throw;
            }
        }

        public async Task<Producto> PrepareNewProductAsync()
        {
            try
            {
                return new Producto
                {
                    CodigoAlfa = await _productoService.GenerarProductoIDAlfa(),
                    CodigoBarra = await _productoService.GenerarCodBarraProducto(),
                    FechaMod = DateTime.Now
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al preparar nuevo producto");
                throw;
            }
        }

        #endregion

        #region Stock

        public async Task<StockItem> GetStockItemAsync(int productoId)
        {
            try
            {
                return await _stockService.GetStockItemByProductoIDAsync(productoId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener stock para producto: {ProductoID}", productoId);
                throw;
            }
        }

        public async Task<IEnumerable<MovimientoStock>> GetStockMovementsAsync(int productoId)
        {
            try
            {
                return await _stockService.GetMovimientosByProductoIDAsync(productoId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener movimientos de stock para producto: {ProductoID}", productoId);
                throw;
            }
        }

        public async Task UpdateStockAsync(int productoId, int differenceAmount, string reason)
        {
            try
            {
                if (differenceAmount == 0)
                {
                    _logger.LogInformation("No hay cambio en el stock para producto: {ProductoID}", productoId);
                    return;
                }

                var stockItem = await _stockService.GetStockItemByProductoIDAsync(productoId);

                if (stockItem == null)
                {
                    // Crear nuevo stock item si no existe
                    stockItem = new StockItem
                    {
                        ProductoID = productoId,
                        CantidadDisponible = Math.Max(0, differenceAmount) // No permitir negativos para nuevo stock
                    };
                    await _stockService.CreateStockItemAsync(stockItem);
                }
                else
                {
                    // Actualizar stock existente
                    stockItem.CantidadDisponible = Math.Max(0, stockItem.CantidadDisponible + differenceAmount);
                    await _stockService.UpdateStockItemAsync(stockItem);
                }

                // Registrar movimiento
                await _stockService.RegistrarMovimientoAsync(new MovimientoStock
                {
                    ProductoID = productoId,
                    Fecha = DateTime.Now,
                    TipoMovimiento = differenceAmount > 0 ? "Entrada" : "Salida",
                    Cantidad = Math.Abs(differenceAmount),
                    Motivo = reason
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar stock para producto: {ProductoID}, cambio: {Change}, motivo: {Reason}",
                    productoId, differenceAmount, reason);
                throw;
            }
        }

        #endregion
    }
}