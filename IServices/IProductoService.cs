// Archivo: IServices/IProductoService.cs
using Javo2.DTOs;
using Javo2.Models;
using Javo2.ViewModels.Operaciones.Productos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Javo2.IServices
{
    public interface IProductoService
    {
        Task<IEnumerable<Producto>> GetAllProductosAsync();
        Task<Producto?> GetProductoByIdAsync(int id);
        Task CreateProductoAsync(Producto producto);
        Task UpdateProductoAsync(Producto producto);
        Task DeleteProductoAsync(int id);
        Task<IEnumerable<Producto>> FilterProductosAsync(ProductoFilterDto filters);
        Task<Producto?> GetProductoByCodigoAsync(string codigo);
        Task<Producto?> GetProductoByNombreAsync(string nombre);
        Task<IEnumerable<Producto>> GetProductosByRubroAsync(string rubro);
        Task<IEnumerable<string>> GetRubrosAutocompleteAsync(string term);
        Task<IEnumerable<string>> GetMarcasAutocompleteAsync(string term);
        Task<IEnumerable<string>> GetProductosAutocompleteAsync(string term);
        string GenerarProductoIDAlfa();
        string GenerarCodBarraProducto();
    }
}
