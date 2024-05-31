using javo2.ViewModels.Operaciones.Ventas;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace javo2.Services
{
    public interface IVentaService
    {
        Task<IEnumerable<VentasViewModel>> GetAllVentasAsync();
        Task<VentasViewModel?> GetVentaByIdAsync(int id);
        Task CreateVentaAsync(VentasViewModel ventaViewModel);
        Task UpdateVentaAsync(VentasViewModel ventaViewModel);
        Task DeleteVentaAsync(int id);
        Task<IEnumerable<VentasViewModel>> GetVentasByEstadoAsync(EstadoVenta estado);
        Task UpdateEstadoVentaAsync(int id, EstadoVenta estado);
    }
}