using javo2.ViewModels.Operaciones.Productos;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace javo2.IServices
{
    public interface IProductoService
    {
        Task<IEnumerable<ProductosViewModel>> GetAllProductosAsync();
        Task<ProductosViewModel?> GetProductoByIdAsync(int id);
        Task CreateProductoAsync(ProductosViewModel productoViewModel);
        Task UpdateProductoAsync(ProductosViewModel productoViewModel);
        Task DeleteProductoAsync(int id);
        Task<ProductosViewModel?> GetProductoByCodigoAsync(string codigo);
        Task<ProductosViewModel?> GetProductoByNombreAsync(string nombre);
        Task<IEnumerable<ProductosViewModel>> GetProductosByRubroAsync(string rubro);
        Task<IEnumerable<string>> GetRubrosAutocompleteAsync(string term);
        Task<IEnumerable<SelectListItem>> GetRubrosAsync();
        Task<IEnumerable<SelectListItem>> GetSubRubrosAsync();
        Task<IEnumerable<SelectListItem>> GetMarcasAsync();
        Task<IEnumerable<string>> GetMarcasAutocompleteAsync(string term);
        Task<IEnumerable<string>> GetProductosAutocompleteAsync(string term);
        string GenerarProductoIDAlfa();
        string GenerarCodBarraProducto();
    }
}
