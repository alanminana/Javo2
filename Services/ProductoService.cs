// Archivo: Services/ProductoService.cs
using Javo2.DTOs;
using Javo2.IServices;
using Javo2.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Javo2.Helpers;

namespace Javo2.Services
{
    public class ProductoService : IProductoService
    {
        private readonly IConfiguracionService _configuracionService;
        private readonly ILogger<ProductoService> _logger;
        private readonly ICatalogoService _catalogoService;
        private readonly IStockService _stockService;
        private readonly IPromocionesService _promocionesService;
        private readonly string _jsonFilePath = "Data/productos.json";

        private static List<Producto> _productos = new();
        private static int _nextProductoID = 1;
        private static readonly object _lock = new();
        private readonly string _serviceId;

        public ProductoService(
            ILogger<ProductoService> logger,
            ICatalogoService catalogoService,
            IStockService stockService,
            IPromocionesService promocionesService,
            IConfiguracionService configuracionService
        )
        {
            _serviceId = Guid.NewGuid().ToString();
            _logger = logger;

            _logger.LogWarning("DEPURACIÓN: ProductoService creado - ID: {ID}", _serviceId);
            _catalogoService = catalogoService;
            _stockService = stockService;
            _promocionesService = promocionesService;
            _configuracionService = configuracionService;

            _logger.LogInformation("ProductoService: Inicializando servicio");
            CargarDesdeJsonAsync().GetAwaiter().GetResult();

            if (!_productos.Any())
            {
                SeedDataAsync().GetAwaiter().GetResult();
            }
        }

        private async Task CargarDesdeJsonAsync()
        {
            try
            {
                var data = await JsonFileHelper.LoadFromJsonFileAsync<List<Producto>>(_jsonFilePath);
                lock (_lock)
                {
                    _productos = data ?? new List<Producto>();
                    if (_productos.Any())
                    {
                        _nextProductoID = _productos.Max(p => p.ProductoID) + 1;
                    }
                    _logger.LogInformation("ProductoService: {Count} productos cargados desde {File}", _productos.Count, _jsonFilePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar productos desde JSON");
                lock (_lock)
                {
                    _productos = new List<Producto>();
                    _nextProductoID = 1;
                }
            }
        }

        private async Task GuardarEnJsonAsync()
        {
            List<Producto> snapshot;
            lock (_lock)
            {
                snapshot = _productos.ToList();
            }
            try
            {
                await JsonFileHelper.SaveToJsonFileAsync(_jsonFilePath, snapshot);
                _logger.LogInformation("ProductoService: guardados {Count} productos en {File}", snapshot.Count, _jsonFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar productos en JSON");
            }
        }

        private async Task SeedDataAsync()
        {
            try
            {
                _logger.LogInformation("SeedDataAsync: Iniciando carga de datos");
                var marcas = (await _catalogoService.GetMarcasAsync()).ToList();
                var rubros = (await _catalogoService.GetRubrosAsync()).ToList();

                if (!marcas.Any() || !rubros.Any())
                {
                    _logger.LogWarning("No se encontraron marcas o rubros");
                    return;
                }

                var marca = marcas.First();
                var rubro = rubros.First();
                var subRubros = await _catalogoService.GetSubRubrosByRubroIDAsync(rubro.ID);
                var subRubro = subRubros.FirstOrDefault();

                if (subRubro == null)
                {
                    _logger.LogWarning("No hay subRubros para el rubro {RubroID}", rubro.ID);
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
                    FechaMod = DateTime.Now,
                    FechaModPrecio = DateTime.Now,
                    ModificadoPor = "Sistema",
                    Estado = Producto.EstadoProducto.Activo
                };

                _productos.Add(producto);
                await GuardarEnJsonAsync();
                _logger.LogInformation("Producto inicial creado: {Nombre}", producto.Nombre);

                // Crear stock inicial
                var stockItem = new StockItem
                {
                    ProductoID = producto.ProductoID,
                    CantidadDisponible = 10
                };
                await _stockService.CreateStockItemAsync(stockItem);
                _logger.LogInformation("Stock inicial creado para producto ID={ID}", producto.ProductoID);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en SeedDataAsync");
            }
        }

        public async Task<IEnumerable<Producto>> GetAllProductosAsync()
        {
            lock (_lock)
            {
                _logger.LogInformation("GetAllProductosAsync: Retornando {Count} productos", _productos.Count);
            }

            // Cargar stock y relaciones
            foreach (var producto in _productos)
            {
                producto.StockItem = await _stockService.GetStockItemByProductoIDAsync(producto.ProductoID);
                producto.Rubro = await _catalogoService.GetRubroByIDAsync(producto.RubroID);
                producto.SubRubro = await _catalogoService.GetSubRubroByIDAsync(producto.SubRubroID);
                producto.Marca = await _catalogoService.GetMarcaByIDAsync(producto.MarcaID);

                var promos = await _promocionesService.GetPromocionesAplicablesAsync(producto);
                if (promos.Any())
                {
                    decimal precioOriginal = producto.PLista;
                    foreach (var promo in promos)
                    {
                        if (promo.EsAumento)
                            precioOriginal += precioOriginal * (promo.Porcentaje / 100m);
                        else
                            precioOriginal -= precioOriginal * (promo.Porcentaje / 100m);
                    }
                    producto.PLista = precioOriginal;
                }
            }

            lock (_lock)
            {
                return _productos.AsEnumerable();
            }
        }

        public async Task<Producto?> GetProductoByIDAsync(int id)
        {
            _logger.LogInformation("GetProductoByIDAsync: ID={ID}", id);
            Producto? producto;

            lock (_lock)
            {
                producto = _productos.FirstOrDefault(p => p.ProductoID == id);
            }

            if (producto != null)
            {
                producto.StockItem = await _stockService.GetStockItemByProductoIDAsync(producto.ProductoID);
                producto.Rubro = await _catalogoService.GetRubroByIDAsync(producto.RubroID);
                producto.SubRubro = await _catalogoService.GetSubRubroByIDAsync(producto.SubRubroID);
                producto.Marca = await _catalogoService.GetMarcaByIDAsync(producto.MarcaID);
            }
            else
            {
                _logger.LogWarning("Producto ID={ID} no encontrado", id);
            }

            return producto;
        }
        private async Task AplicarReglasPrecios(Producto producto)
        {
            // Obtener porcentajes desde la configuración
            var porcentajeLista = await _configuracionService.GetValorAsync("Productos", "PorcentajeGananciaPLista", 84m);
            var porcentajeContado = await _configuracionService.GetValorAsync("Productos", "PorcentajeGananciaPContado", 50m);

            // Aplicar reglas
            producto.PContado = Math.Round(producto.PCosto * (1 + porcentajeContado / 100m), 2);
            producto.PLista = Math.Round(producto.PCosto * (1 + porcentajeLista / 100m), 2);
        }
        public async Task CreateProductoAsync(Producto producto)
        {
            _logger.LogInformation("CreateProductoAsync: {Nombre}", producto.Nombre);
            ValidateProducto(producto);

            // Aplicar reglas de precios antes de crear el producto
            await AplicarReglasPrecios(producto);

            lock (_lock)
            {
                producto.ProductoID = _nextProductoID++;
                producto.FechaMod = DateTime.Now;
                producto.FechaModPrecio = DateTime.Now;
                _productos.Add(producto);
            }

            await GuardarEnJsonAsync();
            _logger.LogInformation("Producto creado con ID={ID}", producto.ProductoID);

            // Crear stock inicial
            var stockItem = new StockItem
            {
                ProductoID = producto.ProductoID,
                CantidadDisponible = 0
            };
            await _stockService.CreateStockItemAsync(stockItem);
            _logger.LogInformation("StockItem creado para producto ID={ID}", producto.ProductoID);
        }

        public async Task UpdateProductoAsync(Producto producto)
        {
            _logger.LogInformation("UpdateProductoAsync: ID={ID}", producto.ProductoID);
            ValidateProducto(producto);

            // Aplicar reglas de precios antes de actualizar el producto
            await AplicarReglasPrecios(producto);

            lock (_lock)
            {
                var existingProducto = _productos.FirstOrDefault(p => p.ProductoID == producto.ProductoID);
                if (existingProducto == null)
                {
                    _logger.LogWarning("Producto ID={ID} no encontrado para actualizar", producto.ProductoID);
                    throw new KeyNotFoundException($"Producto con ID {producto.ProductoID} no encontrado.");
                }

                existingProducto.Nombre = producto.Nombre;
                existingProducto.Descripcion = producto.Descripcion;
                existingProducto.PCosto = producto.PCosto;
                existingProducto.PContado = producto.PContado;
                existingProducto.PLista = producto.PLista;  // Se ha aplicado la regla de precios
                existingProducto.PorcentajeIva = producto.PorcentajeIva;
                existingProducto.RubroID = producto.RubroID;
                existingProducto.SubRubroID = producto.SubRubroID;
                existingProducto.MarcaID = producto.MarcaID;
                existingProducto.FechaMod = DateTime.Now;
                existingProducto.ModificadoPor = producto.ModificadoPor;
            }

            await GuardarEnJsonAsync();
            _logger.LogInformation("Producto ID={ID} actualizado", producto.ProductoID);
        }

        public async Task DeleteProductoAsync(int id)
        {
            _logger.LogInformation("DeleteProductoAsync: ID={ID}", id);

            lock (_lock)
            {
                var producto = _productos.FirstOrDefault(p => p.ProductoID == id);
                if (producto == null)
                {
                    _logger.LogWarning("Producto ID={ID} no encontrado para eliminar", id);
                    throw new KeyNotFoundException($"Producto con ID {id} no encontrado.");
                }

                _productos.Remove(producto);
            }

            await GuardarEnJsonAsync();

            // Eliminar stock asociado
            var stockItem = await _stockService.GetStockItemByProductoIDAsync(id);
            if (stockItem != null)
            {
                await _stockService.DeleteStockItemAsync(stockItem.StockItemID);
                _logger.LogInformation("StockItem eliminado para producto ID={ID}", id);
            }

            _logger.LogInformation("Producto ID={ID} eliminado", id);
        }

        public async Task<IEnumerable<Producto>> FilterProductosAsync(ProductoFilterDto filters)
        {
            _logger.LogInformation("FilterProductosAsync: {@Filters}", filters);
            IQueryable<Producto> query;

            lock (_lock)
            {
                query = _productos.AsQueryable();
            }

            if (!string.IsNullOrEmpty(filters.Nombre))
                query = query.Where(p => p.Nombre.Contains(filters.Nombre, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(filters.Codigo))
                query = query.Where(p => (p.CodigoBarra != null && p.CodigoBarra.Contains(filters.Codigo, StringComparison.OrdinalIgnoreCase))
                                      || (p.CodigoAlfa != null && p.CodigoAlfa.Contains(filters.Codigo, StringComparison.OrdinalIgnoreCase)));

            if (!string.IsNullOrEmpty(filters.Marca))
                query = query.Where(p => p.Marca != null && p.Marca.Nombre.Contains(filters.Marca, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(filters.Rubro))
                query = query.Where(p => p.Rubro != null && p.Rubro.Nombre.Contains(filters.Rubro, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(filters.SubRubro))
                query = query.Where(p => p.SubRubro != null && p.SubRubro.Nombre.Contains(filters.SubRubro, StringComparison.OrdinalIgnoreCase));

            if (filters.PrecioMinimo.HasValue)
                query = query.Where(p => p.PLista >= filters.PrecioMinimo.Value);

            if (filters.PrecioMaximo.HasValue)
                query = query.Where(p => p.PLista <= filters.PrecioMaximo.Value);

            return await Task.FromResult(query.AsEnumerable());
        }

        public async Task AdjustPricesAsync(IEnumerable<int> productIDs, decimal porcentaje, bool isAumento)
        {
            _logger.LogInformation("AdjustPricesAsync: {Count} productos, {Porcentaje}% {Tipo}",
                productIDs.Count(), porcentaje, isAumento ? "Aumento" : "Descuento");

            decimal factor = isAumento ? (1 + porcentaje / 100m) : (1 - porcentaje / 100m);

            lock (_lock)
            {
                foreach (var id in productIDs)
                {
                    var producto = _productos.FirstOrDefault(p => p.ProductoID == id);
                    if (producto != null)
                    {
                        // Ajustar PCosto
                        producto.PCosto *= factor;

                        // Aplicar reglas de precios para recalcular PContado y PLista
                        AplicarReglasPrecios(producto).GetAwaiter().GetResult();
                        producto.FechaModPrecio = DateTime.Now;

                        _logger.LogInformation("Precio ajustado para producto ID={ID}", id);
                    }
                    else
                    {
                        _logger.LogWarning("Producto ID={ID} no encontrado para ajuste", id);
                    }
                }
            }

            await GuardarEnJsonAsync();
        }
        public Task<Producto?> GetProductoByCodigoAsync(string codigo)
        {
            _logger.LogWarning("DEPURACIÓN: GetProductoByCodigoAsync - ServiceID: {SvcID}, Código: {Codigo}",
        _serviceId, codigo);

            if (string.IsNullOrEmpty(codigo))
                return Task.FromResult<Producto?>(null);

            codigo = codigo.Trim().ToLower();

            lock (_lock)
            {
                // Primero buscar por código exacto
                var producto = _productos.FirstOrDefault(p =>
                    (p.CodigoAlfa != null && p.CodigoAlfa.ToLower() == codigo) ||
                    (p.CodigoBarra != null && p.CodigoBarra.ToLower() == codigo));

                // Solo si no se encuentra por código, intentar buscar por nombre
                if (producto == null)
                {
                    producto = _productos.FirstOrDefault(p =>
                        p.Nombre != null && p.Nombre.ToLower().Contains(codigo));
                }

                return Task.FromResult(producto);
            }
        }

        public Task<Producto?> GetProductoByNombreAsync(string nombre)
        {
            _logger.LogInformation("GetProductoByNombreAsync: {Nombre}", nombre);
            lock (_lock)
            {
                var producto = _productos.FirstOrDefault(p =>
                    p.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase));
                return Task.FromResult(producto);
            }
        }

        public Task<IEnumerable<Producto>> GetProductosByRubroAsync(string rubro)
        {
            _logger.LogInformation("GetProductosByRubroAsync: {Rubro}", rubro);
            lock (_lock)
            {
                var productos = _productos
                    .Where(p => p.Rubro != null && p.Rubro.Nombre.Equals(rubro, StringComparison.OrdinalIgnoreCase))
                    .AsEnumerable();
                return Task.FromResult(productos);
            }
        }

        public Task<IEnumerable<Producto>> GetProductosByTermAsync(string term)
        {
            lock (_lock)
            {
                var productos = _productos
                    .Where(p => p.Nombre.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                               (p.Marca != null && p.Marca.Nombre.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                               (p.Rubro != null && p.Rubro.Nombre.Contains(term, StringComparison.OrdinalIgnoreCase)))
                    .AsEnumerable();
                return Task.FromResult(productos);
            }
        }

        public string GenerarProductoIDAlfa()
        {
            _logger.LogInformation("GenerarProductoIDAlfa");
            lock (_lock)
            {
                var maxID = _productos.Any() ? _productos.Max(p => p.ProductoID) : 0;
                return $"P{maxID + 1:D3}";
            }
        }

        public string GenerarCodBarraProducto()
        {
            _logger.LogInformation("GenerarCodBarraProducto");
            var random = new Random();
            return random.Next(100000000, 999999999).ToString();
        }

        public Task<IEnumerable<string>> GetRubrosAutocompleteAsync(string term)
        {
            _logger.LogInformation("GetRubrosAutocompleteAsync: {Term}", term);
            lock (_lock)
            {
                var rubros = _productos
                    .Where(p => p.Rubro != null)
                    .Select(p => p.Rubro.Nombre)
                    .Distinct()
                    .Where(r => r.Contains(term, StringComparison.OrdinalIgnoreCase))
                    .AsEnumerable();
                return Task.FromResult(rubros);
            }
        }

        public Task<IEnumerable<string>> GetMarcasAutocompleteAsync(string term)
        {
            _logger.LogInformation("GetMarcasAutocompleteAsync: {Term}", term);
            lock (_lock)
            {
                var marcas = _productos
                    .Where(p => p.Marca != null)
                    .Select(p => p.Marca.Nombre)
                    .Distinct()
                    .Where(m => m.Contains(term, StringComparison.OrdinalIgnoreCase))
                    .AsEnumerable();
                return Task.FromResult(marcas);
            }
        }

        public Task<IEnumerable<string>> GetProductosAutocompleteAsync(string term)
        {
            _logger.LogInformation("GetProductosAutocompleteAsync: {Term}", term);
            lock (_lock)
            {
                var productos = _productos
                    .Where(p => p.Nombre.Contains(term, StringComparison.OrdinalIgnoreCase))
                    .Select(p => p.Nombre)
                    .Distinct()
                    .AsEnumerable();
                return Task.FromResult(productos);
            }
        }

        public async Task<(Dictionary<int, int> rubrosStock, Dictionary<int, int> marcasStock)> GetRubrosMarcasStockAsync()
        {
            _logger.LogInformation("GetRubrosMarcasStockAsync");
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
            _logger.LogInformation("ValidateProducto: {Nombre}", producto.Nombre);

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