using Javo2.IServices;
using Javo2.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Javo2.Services
{
    public class ProveedorService : IProveedorService
    {
        private readonly ILogger<ProveedorService> _logger;
        private static readonly List<Proveedor> _proveedores = new();
        private readonly IStockService _stockService;
        private readonly IProductoService _productoService;
        private static readonly object _lock = new();

        public ProveedorService(ILogger<ProveedorService> logger, IStockService stockService, IProductoService productoService)
        {
            _logger = logger;
            _stockService = stockService;
            _productoService = productoService;

            if (!_proveedores.Any())
            {
                SeedData();
            }
        }

        private void SeedData()
        {
            lock (_lock)
            {
                _proveedores.AddRange(new List<Proveedor>
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
                });
                _logger.LogInformation("Seed data added for Proveedores");
            }
        }

        public async Task<IEnumerable<Proveedor>> GetProveedoresAsync()
        {
            _logger.LogInformation("GetProveedoresAsync called");
            List<Proveedor> proveedoresCopy;
            lock (_lock)
            {
                proveedoresCopy = _proveedores.ToList();
            }
            return await Task.FromResult(proveedoresCopy);
        }

        public async Task<Proveedor?> GetProveedorByIDAsync(int id)
        {
            _logger.LogInformation("GetProveedorByIDAsync called with ID: {ID}", id);
            Proveedor? proveedor;
            lock (_lock)
            {
                proveedor = _proveedores.FirstOrDefault(p => p.ProveedorID == id);
            }
            if (proveedor == null)
            {
                _logger.LogWarning("Proveedor with ID {ID} not found", id);
            }
            return await Task.FromResult(proveedor);
        }

        public async Task CreateProveedorAsync(Proveedor proveedor)
        {
            ValidateProveedor(proveedor);
            _logger.LogInformation("CreateProveedorAsync called with Proveedor: {Proveedor}", proveedor.Nombre);
            lock (_lock)
            {
                proveedor.ProveedorID = _proveedores.Any() ? _proveedores.Max(p => p.ProveedorID) + 1 : 1;
                _logger.LogInformation("Assigned ProveedorID: {ProveedorID}", proveedor.ProveedorID);
                _proveedores.Add(proveedor);
                _logger.LogInformation("Proveedor created with ID: {ID}", proveedor.ProveedorID);
            }
            await Task.CompletedTask;
        }

        public async Task UpdateProveedorAsync(Proveedor proveedor)
        {
            ValidateProveedor(proveedor);
            _logger.LogInformation("UpdateProveedorAsync called with ProveedorID: {ProveedorID}", proveedor.ProveedorID);
            lock (_lock)
            {
                var existingProveedor = _proveedores.FirstOrDefault(p => p.ProveedorID == proveedor.ProveedorID);
                if (existingProveedor == null)
                {
                    _logger.LogWarning("Proveedor with ID {ID} not found", proveedor.ProveedorID);
                    throw new KeyNotFoundException($"Proveedor con ID {proveedor.ProveedorID} no encontrado.");
                }

                existingProveedor.Nombre = proveedor.Nombre;
                existingProveedor.Direccion = proveedor.Direccion;
                existingProveedor.Telefono = proveedor.Telefono;
                existingProveedor.Email = proveedor.Email;
                existingProveedor.CondicionesPago = proveedor.CondicionesPago;
                existingProveedor.ProductosAsignados = proveedor.ProductosAsignados;
                _logger.LogInformation("Proveedor updated with ID: {ID}", proveedor.ProveedorID);
            }
            await Task.CompletedTask;
        }

        public async Task DeleteProveedorAsync(int id)
        {
            _logger.LogInformation("DeleteProveedorAsync called with ID: {ID}", id);
            lock (_lock)
            {
                var proveedor = _proveedores.FirstOrDefault(p => p.ProveedorID == id);
                if (proveedor == null)
                {
                    _logger.LogWarning("Proveedor with ID {ID} not found", id);
                    throw new KeyNotFoundException($"Proveedor con ID {id} no encontrado.");
                }

                _proveedores.Remove(proveedor);
                _logger.LogInformation("Proveedor deleted with ID: {ID}", id);
            }
            await Task.CompletedTask;
        }


        // Archivo: Services/ProveedorService.cs
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
