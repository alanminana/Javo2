// Archivo: Services/ProductoService.cs
using Javo2.DTOs;
using Javo2.IServices;
using Javo2.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Javo2.Services
{
    public class ProductoService : IProductoService
    {
        private readonly ILogger<ProductoService> _logger;
        private readonly ICatalogoService _catalogoService;
        private readonly IStockService _stockService;
        private readonly IPromocionesService _promocionesService;

        private static readonly List<Producto> _productos = new();
        private int _nextProductoID = 1;

        public ProductoService(
            ILogger<ProductoService> logger,
            ICatalogoService catalogoService,
            IStockService stockService,
            IPromocionesService promocionesService
        )
        {
            _logger = logger;
            _catalogoService = catalogoService;
            _stockService = stockService;
            _promocionesService = promocionesService;

            _logger.LogInformation("ProductoService constructor: inicializando datos de prueba...");
            SeedDataAsync().GetAwaiter().GetResult();
        }

        private async Task SeedDataAsync()
        {
            try
            {
                _logger.LogInformation("SeedDataAsync: Cargando rubros y marcas...");
                var marcas = (await _catalogoService.GetMarcasAsync()).ToList();
                var rubros = (await _catalogoService.GetRubrosAsync()).ToList();

                if (!marcas.Any() || !rubros.Any())
                {
                    _logger.LogWarning("No se encontraron marcas o rubros. No se inicializarán productos en SeedData.");
                    return;
                }

                var marca = marcas.First();
                var rubro = rubros.First();
                var subRubros = await _catalogoService.GetSubRubrosByRubroIDAsync(rubro.ID);
                var subRubro = subRubros.FirstOrDefault();
                if (subRubro == null)
                {
                    _logger.LogWarning("El rubro no tiene subRubros. No se inicializan productos en SeedData.");
                    return;
                }

                var producto = new Producto
                {
                    ProductoID = _nextProductoID++,
                    CodigoAlfa = "P001",
                    CodigoBarra = GenerarCodBarraProducto(),
                    Nombre = "Producto Inicial",
                    Descripcion = "Descripción del producto inicial",
                    PCosto = 100,
                    PContado = 150,
                    PLista = 200,
                    PorcentajeIva = 21,
                    RubroID = rubro.ID,
                    SubRubroID = subRubro.ID,
                    MarcaID = marca.ID,
                    Rubro = rubro,
                    SubRubro = subRubro,
                    Marca = marca,
                    FechaMod = DateTime.Now,
                    FechaModPrecio = DateTime.Now,
                    Usuario = "TestUser",
                    ModificadoPor = "TestUser",
                    EstadoComentario = "Activo",
                    DeudaTotal = 0
                };

                _productos.Add(producto);
                _logger.LogInformation("SeedDataAsync: Agregado producto inicial: {Nombre}", producto.Nombre);

                var stockItem = new StockItem
                {
                    ProductoID = producto.ProductoID,
                    CantidadDisponible = 10,
                    Producto = producto
                };
                await _stockService.CreateStockItemAsync(stockItem);
                _logger.LogInformation("SeedDataAsync: Stock inicial creado para producto ID={ID}", producto.ProductoID);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en SeedDataAsync");
            }
        }

        public async Task<IEnumerable<Producto>> GetProductosByTermAsync(string term)
        {
            var result = _productos
                .Where(p => p.Nombre.Contains(term, StringComparison.OrdinalIgnoreCase)
                         || (p.Marca != null && p.Marca.Nombre.Contains(term, StringComparison.OrdinalIgnoreCase)));
            return await Task.FromResult(result);
        }

        // Ajuste de precios
        public async Task AdjustPricesAsync(IEnumerable<int> ProductoIDs, decimal porcentaje, bool isAumento)
        {
            _logger.LogInformation("AdjustPricesAsync llamado. ProductoIDs={@IDs}, porcentaje={Porcentaje}, isAumento={IsAumento}",
                ProductoIDs, porcentaje, isAumento);

            foreach (var id in ProductoIDs)
            {
                var producto = _productos.FirstOrDefault(p => p.ProductoID == id);
                if (producto != null)
                {
                    decimal factor = isAumento ? (1 + porcentaje / 100m)
                                               : (1 - porcentaje / 100m);

                    _logger.LogInformation("Ajustando precios de producto ID={ID}, factor={Factor}", id, factor);

                    producto.PCosto *= factor;
                    producto.PContado *= factor;
                    producto.PLista *= factor;
                    producto.FechaModPrecio = DateTime.Now;
                }
                else
                {
                    _logger.LogWarning("AdjustPricesAsync: Producto ID={ID} no encontrado para ajuste", id);
                }
            }
            _logger.LogInformation("Ajuste masivo de precios completado. CantProductos: {Count}", ProductoIDs.Count());
            await Task.CompletedTask;
        }

        public async Task<IEnumerable<Producto>> GetAllProductosAsync()
        {
            _logger.LogInformation("GetAllProductosAsync: retornando {_Count} productos", _productos.Count);

            foreach (var producto in _productos)
            {
                // Asignar stock item
                producto.StockItem = await _stockService.GetStockItemByProductoIDAsync(producto.ProductoID);

                // Aplicar promociones si las hay
                var promos = await _promocionesService.GetPromocionesAplicablesAsync(producto);
                decimal pListaOriginal = producto.PLista;
                foreach (var promo in promos)
                {
                    if (promo.EsAumento)
                        pListaOriginal += pListaOriginal * (promo.Porcentaje / 100m);
                    else
                        pListaOriginal -= pListaOriginal * (promo.Porcentaje / 100m);
                }
                producto.PLista = pListaOriginal;
            }
            return _productos.AsEnumerable();
        }

        public async Task<Producto?> GetProductoByIDAsync(int id)
        {
            _logger.LogInformation("GetProductoByIDAsync llamado con ID={ID}", id);
            var producto = _productos.FirstOrDefault(p => p.ProductoID == id);
            if (producto != null)
            {
                producto.StockItem = await _stockService.GetStockItemByProductoIDAsync(producto.ProductoID);
            }
            else
            {
                _logger.LogWarning("GetProductoByIDAsync: Producto ID={ID} no encontrado", id);
            }
            return producto;
        }

        public async Task CreateProductoAsync(Producto producto)
        {
            _logger.LogInformation("CreateProductoAsync llamado para {Nombre}", producto.Nombre);
            ValidateProducto(producto);

            producto.ProductoID = _nextProductoID++;
            producto.FechaMod = DateTime.Now;
            producto.FechaModPrecio = DateTime.Now;

            _productos.Add(producto);
            _logger.LogInformation("Producto creado con ID={ID}, Nombre={Nombre}", producto.ProductoID, producto.Nombre);

            var stockItem = new StockItem
            {
                ProductoID = producto.ProductoID,
                CantidadDisponible = 0,
                Producto = producto
            };
            await _stockService.CreateStockItemAsync(stockItem);
            _logger.LogInformation("StockItem creado para producto ID={ID}", producto.ProductoID);
        }

        public async Task UpdateProductoAsync(Producto producto)
        {
            _logger.LogInformation("UpdateProductoAsync llamado para producto ID={ID}, Nombre={Nombre}",
                producto.ProductoID, producto.Nombre);
            ValidateProducto(producto);

            var existingProducto = _productos.FirstOrDefault(p => p.ProductoID == producto.ProductoID);
            if (existingProducto == null)
            {
                _logger.LogWarning("UpdateProductoAsync: Producto ID={ID} no encontrado", producto.ProductoID);
                throw new KeyNotFoundException($"Producto con ID {producto.ProductoID} no encontrado.");
            }

            existingProducto.Nombre = producto.Nombre;
            existingProducto.Descripcion = producto.Descripcion;
            existingProducto.PCosto = producto.PCosto;
            existingProducto.PContado = producto.PContado;
            existingProducto.PLista = producto.PLista;
            existingProducto.PorcentajeIva = producto.PorcentajeIva;
            existingProducto.RubroID = producto.RubroID;
            existingProducto.SubRubroID = producto.SubRubroID;
            existingProducto.MarcaID = producto.MarcaID;
            existingProducto.FechaMod = DateTime.Now;
            existingProducto.ModificadoPor = producto.ModificadoPor;
            existingProducto.EstadoComentario = producto.EstadoComentario;

            // Actualizar StockItem
            existingProducto.StockItem = await _stockService.GetStockItemByProductoIDAsync(existingProducto.ProductoID);

            _logger.LogInformation("Producto ID={ID} actualizado correctamente", producto.ProductoID);
        }

        public async Task DeleteProductoAsync(int id)
        {
            _logger.LogInformation("DeleteProductoAsync llamado con ID={ID}", id);
            var producto = _productos.FirstOrDefault(p => p.ProductoID == id);
            if (producto == null)
            {
                _logger.LogWarning("DeleteProductoAsync: Producto ID={ID} no encontrado", id);
                throw new KeyNotFoundException($"Producto con ID {id} no encontrado.");
            }

            _productos.Remove(producto);
            _logger.LogInformation("Producto ID={ID} eliminado.", id);

            var stockItem = await _stockService.GetStockItemByProductoIDAsync(id);
            if (stockItem != null)
            {
                await _stockService.DeleteStockItemAsync(stockItem.StockItemID);
                _logger.LogInformation("StockItem para producto ID={ID} también fue eliminado.", id);
            }
        }

        public async Task<IEnumerable<Producto>> FilterProductosAsync(ProductoFilterDto filters)
        {
            _logger.LogInformation("FilterProductosAsync llamado con filtros: {@Filters}", filters);
            var query = _productos.AsQueryable();

            if (!string.IsNullOrEmpty(filters.Nombre))
                query = query.Where(p => p.Nombre.Contains(filters.Nombre, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(filters.Codigo))
                query = query.Where(p => (p.CodigoBarra != null && p.CodigoBarra.Contains(filters.Codigo, StringComparison.OrdinalIgnoreCase))
                                      || (p.CodigoAlfa != null && p.CodigoAlfa.Contains(filters.Codigo, StringComparison.OrdinalIgnoreCase)));

            if (!string.IsNullOrEmpty(filters.Marca))
                query = query.Where(p => p.Marca != null
                                      && p.Marca.Nombre.Contains(filters.Marca, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(filters.Rubro))
                query = query.Where(p => p.Rubro != null
                                      && p.Rubro.Nombre.Contains(filters.Rubro, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(filters.SubRubro))
                query = query.Where(p => p.SubRubro != null
                                      && p.SubRubro.Nombre.Contains(filters.SubRubro, StringComparison.OrdinalIgnoreCase));

            return await Task.FromResult(query.AsEnumerable());
        }

        public Task<Producto?> GetProductoByCodigoAsync(string codigo)
        {
            _logger.LogInformation("GetProductoByCodigoAsync llamado con Codigo={Codigo}", codigo);
            var producto = _productos.FirstOrDefault(p => (p.CodigoBarra != null && p.CodigoBarra.Contains(codigo))
                                                       || (p.CodigoAlfa != null && p.CodigoAlfa.Contains(codigo)));
            return Task.FromResult(producto);
        }

        public Task<Producto?> GetProductoByNombreAsync(string nombre)
        {
            _logger.LogInformation("GetProductoByNombreAsync llamado con Nombre={Nombre}", nombre);
            var producto = _productos.FirstOrDefault(p => p.Nombre.Contains(nombre, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(producto);
        }

        public Task<IEnumerable<Producto>> GetProductosByRubroAsync(string rubro)
        {
            _logger.LogInformation("GetProductosByRubroAsync llamado con Rubro={Rubro}", rubro);
            var productos = _productos.Where(p => p.Rubro != null
                                               && p.Rubro.Nombre.Equals(rubro, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(productos.AsEnumerable());
        }

        public string GenerarProductoIDAlfa()
        {
            _logger.LogInformation("GenerarProductoIDAlfa llamado.");
            var maxID = _productos.Any() ? _productos.Max(p => p.ProductoID) : 0;
            return $"P{maxID + 1:D3}";
        }

        public string GenerarCodBarraProducto()
        {
            _logger.LogInformation("GenerarCodBarraProducto llamado.");
            var random = new Random();
            return random.Next(100000000, 999999999).ToString();
        }

        public async Task<IEnumerable<string>> GetRubrosAutocompleteAsync(string term)
        {
            _logger.LogInformation("GetRubrosAutocompleteAsync llamado con Term={Term}", term);
            var rubros = _productos
                .Select(p => p.Rubro?.Nombre ?? "")
                .Distinct()
                .Where(r => r != null && r.Contains(term, StringComparison.OrdinalIgnoreCase));
            return await Task.FromResult(rubros);
        }

        public async Task<IEnumerable<string>> GetMarcasAutocompleteAsync(string term)
        {
            _logger.LogInformation("GetMarcasAutocompleteAsync llamado con Term={Term}", term);
            var marcas = _productos
                .Select(p => p.Marca?.Nombre ?? "")
                .Distinct()
                .Where(m => m != null && m.Contains(term, StringComparison.OrdinalIgnoreCase));
            return await Task.FromResult(marcas);
        }

        public async Task<IEnumerable<string>> GetProductosAutocompleteAsync(string term)
        {
            _logger.LogInformation("GetProductosAutocompleteAsync llamado con Term={Term}", term);
            var productos = _productos
                .Where(p => p.Nombre.Contains(term, StringComparison.OrdinalIgnoreCase))
                .Select(p => p.Nombre)
                .Distinct();
            return await Task.FromResult(productos);
        }

        public async Task<(Dictionary<int, int> rubrosStock, Dictionary<int, int> marcasStock)> GetRubrosMarcasStockAsync()
        {
            _logger.LogInformation("GetRubrosMarcasStockAsync llamado.");
            var productos = await GetAllProductosAsync();

            var rubroStocks = productos
                .Where(p => p.Rubro != null && p.StockItem != null)
                .GroupBy(p => p.RubroID)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.StockItem.CantidadDisponible));

            var marcaStocks = productos
                .Where(p => p.Marca != null && p.StockItem != null)
                .GroupBy(p => p.MarcaID)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.StockItem.CantidadDisponible));

            return (rubroStocks, marcaStocks);
        }

        private void ValidateProducto(Producto producto)
        {
            _logger.LogInformation("ValidateProducto llamado para Nombre={Nombre}, PCosto={PCosto}, PContado={PContado}, PLista={PLista}",
                producto.Nombre, producto.PCosto, producto.PContado, producto.PLista);

            if (string.IsNullOrWhiteSpace(producto.Nombre))
            {
                var msg = "El nombre del producto no puede estar vacío.";
                _logger.LogError(msg);
                throw new ArgumentException(msg);
            }
            if (producto.PCosto < 0 || producto.PContado < 0 || producto.PLista < 0)
            {
                var msg = "Los precios no pueden ser negativos.";
                _logger.LogError(msg);
                throw new ArgumentException(msg);
            }
        }
    }
}
