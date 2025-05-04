// IServices/IProveedorService.cs (actualizado)
using Javo2.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Javo2.IServices
{
    public interface IProveedorService
    {
        // Métodos existentes
        Task<IEnumerable<Proveedor>> GetProveedoresAsync();
        Task<Proveedor?> GetProveedorByIDAsync(int id);
        Task CreateProveedorAsync(Proveedor proveedor);
        Task UpdateProveedorAsync(Proveedor proveedor);
        Task DeleteProveedorAsync(int id);

        // Nuevos métodos para compras
        Task<IEnumerable<CompraProveedor>> GetComprasAsync();
        Task<CompraProveedor?> GetCompraByIDAsync(int id);
        Task<IEnumerable<CompraProveedor>> GetComprasByProveedorIDAsync(int proveedorID);
        Task<string> GenerarNumeroFacturaCompraAsync();
        Task CreateCompraAsync(CompraProveedor compra);
        Task UpdateCompraAsync(CompraProveedor compra);
        Task DeleteCompraAsync(int id);
        Task ProcesarCompraAsync(int compraID);
        Task CompletarCompraAsync(int compraID);
        Task CancelarCompraAsync(int compraID);
    }
}