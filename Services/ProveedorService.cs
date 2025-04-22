// Archivo: Services/ProveedorService.cs
// Optimizaciones realizadas:
// - Se mejoró el manejo de persistencia con mejor manejo de errores
// - Se consolidó la lógica de carga y guardado

using Javo2.IServices;
using Javo2.Models;
using Microsoft.Extensions.Logging;
using Javo2.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Javo2.Services
{
    public class ProveedorService : IProveedorService
    {
        private readonly ILogger<ProveedorService> _logger;
        private readonly IStockService _stockService;
        private readonly IProductoService _productoService;
        private readonly string _jsonFilePath = "Data/proveedores.json";

        private List<Proveedor> _proveedores;
        private ProveedorData _proveedorData;
        private static readonly object _lock = new();

        public ProveedorService(
            ILogger<ProveedorService> logger,
            IStockService stockService,
            IProductoService productoService)
        {
            _logger = logger;
            _stockService = stockService;
            _productoService = productoService;

            InitializeData();
        }

        private void InitializeData()
        {
            try
            {
                _proveedorData = JsonFileHelper.LoadFromJsonFile<ProveedorData>(_jsonFilePath) ?? new ProveedorData();
                _proveedores = _proveedorData.Proveedores;

                if (!_proveedores.Any())
                {
                    SeedData();
                }

                _logger.LogInformation("Proveedor data initialized with {Count} providers", _proveedores.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing provider data");
                _proveedorData = new ProveedorData();
                _proveedores = new List<Proveedor>();
                SeedData();
            }
        }

        private void SeedData()
        {
            _proveedores = new List<Proveedor>
            {
                new Proveedor
                {
                    ProveedorID = 1,
                    Nombre = "Proveedor Ejemplo 1",
                    Direccion = "Calle Falsa 123",
                    Telefono = "123456789",
                    Email = "proveedor1@example.com",
                    CondicionesPago = "30 días",
                    ProductosAsignados = new List<int> { 1, 2 }
                },
                new Proveedor
                {
                    ProveedorID = 2,
                    Nombre = "Proveedor Ejemplo 2",
                    Direccion = "Avenida Siempre Viva 742",
                    Telefono = "987654321",
                    Email = "proveedor2@example.com",
                    CondicionesPago = "15 días",
                    ProductosAsignados = new List<int> { 3 }
                }
            };

            _proveedorData.Proveedores = _proveedores;
            SaveData();
            _logger.LogInformation("Seed data created for Proveedores");
        }

        private void SaveData()
        {
            try
            {
                JsonFileHelper.SaveToJsonFile(_jsonFilePath, _proveedorData);
                _logger.LogInformation("Proveedor data saved to {File}", _jsonFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving provider data");
                throw;
            }
        }

        public async Task<IEnumerable<Proveedor>> GetProveedoresAsync()
        {
            _logger.LogInformation("GetProveedoresAsync called");
            return await Task.FromResult(_proveedores.ToList());
        }

        public async Task<Proveedor?> GetProveedorByIDAsync(int id)
        {
            _logger.LogInformation("GetProveedorByIDAsync called with ID: {ID}", id);
            return await Task.FromResult(_proveedores.FirstOrDefault(p => p.ProveedorID == id));
        }

        public async Task CreateProveedorAsync(Proveedor proveedor)
        {
            ValidateProveedor(proveedor);

            lock (_lock)
            {
                proveedor.ProveedorID = _proveedores.Any() ? _proveedores.Max(p => p.ProveedorID) + 1 : 1;
                _proveedores.Add(proveedor);
                SaveData();
            }

            _logger.LogInformation("Proveedor created with ID: {ID}", proveedor.ProveedorID);
            await Task.CompletedTask;
        }

        public async Task UpdateProveedorAsync(Proveedor proveedor)
        {
            ValidateProveedor(proveedor);

            lock (_lock)
            {
                var existingProveedor = _proveedores.FirstOrDefault(p => p.ProveedorID == proveedor.ProveedorID);
                if (existingProveedor == null)
                {
                    throw new KeyNotFoundException($"Proveedor con ID {proveedor.ProveedorID} no encontrado.");
                }

                existingProveedor.Nombre = proveedor.Nombre;
                existingProveedor.Direccion = proveedor.Direccion;
                existingProveedor.Telefono = proveedor.Telefono;
                existingProveedor.Email = proveedor.Email;
                existingProveedor.CondicionesPago = proveedor.CondicionesPago;
                existingProveedor.ProductosAsignados = proveedor.ProductosAsignados;

                SaveData();
            }

            _logger.LogInformation("Proveedor updated with ID: {ID}", proveedor.ProveedorID);
            await Task.CompletedTask;
        }

        public async Task DeleteProveedorAsync(int id)
        {
            lock (_lock)
            {
                var proveedor = _proveedores.FirstOrDefault(p => p.ProveedorID == id);
                if (proveedor == null)
                {
                    throw new KeyNotFoundException($"Proveedor con ID {id} no encontrado.");
                }

                _proveedores.Remove(proveedor);
                SaveData();
            }

            _logger.LogInformation("Proveedor deleted with ID: {ID}", id);
            await Task.CompletedTask;
        }

        public async Task RegistrarCompraAsync(int proveedorID, int ProductoID, int cantidad)
        {
            _logger.LogInformation("RegistrarCompraAsync called with ProveedorID: {ProveedorID}, ProductoID: {ProductoID}, Cantidad: {Cantidad}",
                proveedorID, ProductoID, cantidad);

            var proveedor = await GetProveedorByIDAsync(proveedorID);
            if (proveedor == null)
            {
                throw new KeyNotFoundException($"Proveedor con ID {proveedorID} no encontrado.");
            }

            if (!proveedor.ProductosAsignados.Contains(ProductoID))
            {
                throw new InvalidOperationException($"El producto con ID {ProductoID} no está asignado al proveedor con ID {proveedorID}.");
            }

            var stockItem = await _stockService.GetStockItemByProductoIDAsync(ProductoID);
            if (stockItem == null)
            {
                throw new KeyNotFoundException($"StockItem para el producto con ID {ProductoID} no encontrado.");
            }

            var movimiento = new MovimientoStock
            {
                ProductoID = ProductoID,
                TipoMovimiento = "Entrada",
                Cantidad = cantidad,
                Motivo = $"Compra al proveedor {proveedor.Nombre} (ID: {proveedorID})"
            };

            await _stockService.RegistrarMovimientoAsync(movimiento);
            _logger.LogInformation("Compra registrada: ProductoID {ProductoID}, Cantidad {Cantidad}", ProductoID, cantidad);
        }

        private void ValidateProveedor(Proveedor proveedor)
        {
            if (string.IsNullOrWhiteSpace(proveedor.Nombre))
            {
                throw new ArgumentException("El nombre del proveedor no puede estar vacío.");
            }
        }
    }
}