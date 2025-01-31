// Archivo: IServices/IProveedorService.cs
using Javo2.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Javo2.IServices
{
    /// <summary>
    /// Interfaz para el servicio de proveedores.
    /// </summary>
    public interface IProveedorService
    {
        /// <summary>
        /// Obtiene todos los proveedores.
        /// </summary>
        Task<IEnumerable<Proveedor>> GetProveedoresAsync();

        /// <summary>
        /// Obtiene un proveedor por su ID.
        /// </summary>
        Task<Proveedor?> GetProveedorByIDAsync(int id);

        /// <summary>
        /// Crea un nuevo proveedor.
        /// </summary>
        Task CreateProveedorAsync(Proveedor proveedor);

        /// <summary>
        /// Actualiza un proveedor existente.
        /// </summary>
        Task UpdateProveedorAsync(Proveedor proveedor);

        /// <summary>
        /// Elimina un proveedor por su ID.
        /// </summary>
        Task DeleteProveedorAsync(int id);

        Task RegistrarCompraAsync(int proveedorID, int ProductoID, int cantidad);

    }
}
