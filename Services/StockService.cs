using Javo2.IServices;
using Javo2.Models;
using Microsoft.Extensions.Logging;

namespace Javo2.Services
{
    public class StockService : IStockService
    {
        private readonly ILogger<StockService> _logger;
        private static readonly List<StockItem> _stockItems = new();
        private static readonly List<MovimientoStock> _movimientos = new();
        private static readonly object _lock = new();

        public StockService(ILogger<StockService> logger)
        {
            _logger = logger;
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
                // Asignar un ID único si es necesario
                if (stockItem.StockItemID == 0)
                {
                    stockItem.StockItemID = _stockItems.Any() ? _stockItems.Max(s => s.StockItemID) + 1 : 1;
                }
                _stockItems.Add(stockItem);
                _logger.LogInformation("StockItem creado para ProductoID {ProductoID} con Cantidad {Cantidad}", stockItem.ProductoID, stockItem.CantidadDisponible);
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
            }
            return Task.CompletedTask;
        }

        public Task RegistrarMovimientoAsync(MovimientoStock movimiento)
        {
            lock (_lock)
            {
                // Validar que exista el StockItem
                var stockItem = _stockItems.FirstOrDefault(s => s.ProductoID == movimiento.ProductoID);
                if (stockItem == null)
                {
                    throw new KeyNotFoundException($"No se encontró StockItem para ProductoID {movimiento.ProductoID}");
                }

                // Actualizar CantidadDisponible según el tipo de movimiento
                // Asumimos: "Entrada" suma, "Salida" resta, "Ajuste" puede ser más complejo
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
                    // Ajuste puede ser positivo o negativo, se espera que Cantidad sea positiva o negativa según corresponda
                    stockItem.CantidadDisponible += movimiento.Cantidad;
                    // Validar que no quede negativo
                    if (stockItem.CantidadDisponible < 0)
                    {
                        throw new InvalidOperationException("El ajuste resultó en stock negativo, lo cual no es válido.");
                    }
                }

                // Generar un ID para el movimiento
                movimiento.MovimientoStockID = _movimientos.Any() ? _movimientos.Max(m => m.MovimientoStockID) + 1 : 1;
                movimiento.Fecha = DateTime.UtcNow;
                _movimientos.Add(movimiento);

                _logger.LogInformation("Movimiento de Stock registrado: {Tipo}, ProductoID {ProductoID}, Cantidad {Cantidad}", movimiento.TipoMovimiento, movimiento.ProductoID, movimiento.Cantidad);
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
