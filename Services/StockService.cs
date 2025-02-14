// File: Services/StockService.cs
using Javo2.IServices;
using Javo2.Models;
using Microsoft.Extensions.Logging;
using Javo2.Helpers; // Para utilizar JsonFileHelper
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Javo2.Services
{
    public class StockService : IStockService
    {
        private readonly ILogger<StockService> _logger;
        private List<StockItem> _stockItems;
        private List<MovimientoStock> _movimientos;
        private static readonly object _lock = new();

        private readonly string _stockFilePath = "Data/stock.json";
        private readonly string _movimientosFilePath = "Data/movimientosStock.json";

        public StockService(ILogger<StockService> logger)
        {
            _logger = logger;
            LoadData();
        }

        private void LoadData()
        {
            lock (_lock)
            {
                _stockItems = JsonFileHelper.LoadFromJsonFile<List<StockItem>>(_stockFilePath) ?? new List<StockItem>();
                _movimientos = JsonFileHelper.LoadFromJsonFile<List<MovimientoStock>>(_movimientosFilePath) ?? new List<MovimientoStock>();
                _logger.LogInformation("Stock data loaded: {Count} items and {MovCount} movimientos.", _stockItems.Count, _movimientos.Count);
            }
        }

        private void SaveStockData()
        {
            try
            {
                JsonFileHelper.SaveToJsonFile(_stockFilePath, _stockItems);
                _logger.LogInformation("Stock data saved to {File}.", _stockFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving stock data.");
            }
        }

        private void SaveMovimientosData()
        {
            try
            {
                JsonFileHelper.SaveToJsonFile(_movimientosFilePath, _movimientos);
                _logger.LogInformation("Movimientos data saved to {File}.", _movimientosFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving movimientos data.");
            }
        }

        public Task<StockItem?> GetStockItemByProductoIDAsync(int ProductoID)
        {
            lock (_lock)
            {
                var stockItem = _stockItems.FirstOrDefault(s => s.ProductoID == ProductoID);
                return Task.FromResult(stockItem);
            }
        }

        public Task CreateStockItemAsync(StockItem stockItem)
        {
            lock (_lock)
            {
                if (stockItem.StockItemID == 0)
                {
                    stockItem.StockItemID = _stockItems.Any() ? _stockItems.Max(s => s.StockItemID) + 1 : 1;
                }
                _stockItems.Add(stockItem);
                _logger.LogInformation("StockItem creado para ProductoID {ProductoID} con Cantidad {Cantidad}", stockItem.ProductoID, stockItem.CantidadDisponible);
                SaveStockData();
            }
            return Task.CompletedTask;
        }

        public Task UpdateStockItemAsync(StockItem stockItem)
        {
            lock (_lock)
            {
                var existing = _stockItems.FirstOrDefault(s => s.StockItemID == stockItem.StockItemID);
                if (existing == null)
                {
                    throw new KeyNotFoundException($"StockItem con ID {stockItem.StockItemID} no encontrado.");
                }
                existing.CantidadDisponible = stockItem.CantidadDisponible;
                _logger.LogInformation("StockItem con ID {StockItemID} actualizado. Nueva Cantidad: {Cantidad}", stockItem.StockItemID, stockItem.CantidadDisponible);
                SaveStockData();
            }
            return Task.CompletedTask;
        }

        public Task DeleteStockItemAsync(int stockItemID)
        {
            lock (_lock)
            {
                var existing = _stockItems.FirstOrDefault(s => s.StockItemID == stockItemID);
                if (existing == null)
                {
                    throw new KeyNotFoundException($"StockItem con ID {stockItemID} no encontrado.");
                }
                _stockItems.Remove(existing);
                _logger.LogInformation("StockItem con ID {StockItemID} eliminado.", stockItemID);
                SaveStockData();
            }
            return Task.CompletedTask;
        }

        public Task RegistrarMovimientoAsync(MovimientoStock movimiento)
        {
            lock (_lock)
            {
                var stockItem = _stockItems.FirstOrDefault(s => s.ProductoID == movimiento.ProductoID);
                if (stockItem == null)
                {
                    throw new KeyNotFoundException($"No se encontró StockItem para ProductoID {movimiento.ProductoID}");
                }

                // Actualizar CantidadDisponible según el tipo de movimiento
                if (movimiento.TipoMovimiento.Equals("Entrada", StringComparison.OrdinalIgnoreCase))
                {
                    stockItem.CantidadDisponible += movimiento.Cantidad;
                }
                else if (movimiento.TipoMovimiento.Equals("Salida", StringComparison.OrdinalIgnoreCase))
                {
                    if (stockItem.CantidadDisponible < movimiento.Cantidad)
                    {
                        throw new InvalidOperationException("No hay stock suficiente para la salida solicitada.");
                    }
                    stockItem.CantidadDisponible -= movimiento.Cantidad;
                }
                else if (movimiento.TipoMovimiento.Equals("Ajuste", StringComparison.OrdinalIgnoreCase))
                {
                    stockItem.CantidadDisponible += movimiento.Cantidad;
                    if (stockItem.CantidadDisponible < 0)
                    {
                        throw new InvalidOperationException("El ajuste resultó en stock negativo, lo cual no es válido.");
                    }
                }

                movimiento.MovimientoStockID = _movimientos.Any() ? _movimientos.Max(m => m.MovimientoStockID) + 1 : 1;
                movimiento.Fecha = DateTime.UtcNow;
                _movimientos.Add(movimiento);

                _logger.LogInformation("Movimiento de Stock registrado: {Tipo}, ProductoID {ProductoID}, Cantidad {Cantidad}", movimiento.TipoMovimiento, movimiento.ProductoID, movimiento.Cantidad);

                SaveStockData();
                SaveMovimientosData();
            }
            return Task.CompletedTask;
        }

        public Task<IEnumerable<MovimientoStock>> GetMovimientosByProductoIDAsync(int ProductoID)
        {
            IEnumerable<MovimientoStock> result;
            lock (_lock)
            {
                result = _movimientos.Where(m => m.ProductoID == ProductoID).ToList();
            }
            return Task.FromResult(result);
        }
    }
}
