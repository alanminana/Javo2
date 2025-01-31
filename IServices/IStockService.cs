using Javo2.Models;

namespace Javo2.IServices
{
    public interface IStockService
    {
        Task<StockItem?> GetStockItemByProductoIDAsync(int ProductoID);
        Task CreateStockItemAsync(StockItem stockItem);
        Task UpdateStockItemAsync(StockItem stockItem);
        Task DeleteStockItemAsync(int stockItemID);

        Task RegistrarMovimientoAsync(MovimientoStock movimiento);
        Task<IEnumerable<MovimientoStock>> GetMovimientosByProductoIDAsync(int ProductoID);
    }
}
