using javo2.ViewModels.Operaciones.Proveedores;
using javo2.ViewModels.Operaciones.Productos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace javo2.Services
{
    public interface IProveedorService
    {
        Task<IEnumerable<ProveedoresViewModel>> GetProveedoresAsync();
        Task<ProveedoresViewModel?> GetProveedorByIdAsync(int id);
        Task CreateProveedorAsync(ProveedoresViewModel proveedorViewModel);
        Task UpdateProveedorAsync(ProveedoresViewModel proveedorViewModel);
        Task DeleteProveedorAsync(int id);
        Task<IEnumerable<ProductosViewModel>> GetProductosDisponiblesAsync();
    }
}
