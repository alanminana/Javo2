// Archivo: IServices/IProductoService.cs
using Javo2.DTOs;
using Javo2.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Javo2.IServices
{
    public interface IProductoService
    {
        Task<IEnumerable<Producto>> GetAllProductosAsync();
        Task<Producto?> GetProductoByIDAsync(int id);
        Task CreateProductoAsync(Producto producto);
        Task UpdateProductoAsync(Producto producto);
        Task DeleteProductoAsync(int id);

        // Métodos de filtrado
        Task<IEnumerable<Producto>> FilterProductosAsync(ProductoFilterDto filters);

        // Ajustes de Precios
        Task AdjustPricesAsync(IEnumerable<int> productIDs, decimal porcentaje, bool isAumento);

        // Búsquedas
        Task<Producto?> GetProductoByCodigoAsync(string codigo);
        Task<Producto?> GetProductoByNombreAsync(string nombre);
        Task<IEnumerable<Producto>> GetProductosByRubroAsync(string rubro);

        // Generadores
        string GenerarProductoIDAlfa();
        string GenerarCodBarraProducto();

        // Autocomplete
        Task<IEnumerable<string>> GetRubrosAutocompleteAsync(string term);
        Task<IEnumerable<string>> GetMarcasAutocompleteAsync(string term);
        Task<IEnumerable<string>> GetProductosAutocompleteAsync(string term);

        Task<IEnumerable<Producto>> GetProductosByTermAsync(string term);

        Task<(Dictionary<int, int> rubrosStock, Dictionary<int, int> marcasStock)> GetRubrosMarcasStockAsync();
    }
}
