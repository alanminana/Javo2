using Javo2.ViewModels.Operaciones.Proveedores;
using Javo2.ViewModels.Operaciones.Productos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Javo2.Services
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