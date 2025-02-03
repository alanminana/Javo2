// Ruta: IServices/IClienteService.cs
using Javo2.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Javo2.IServices
{
    public interface IClienteService
    {
        Task<IEnumerable<Cliente>> GetAllClientesAsync();
        Task<Cliente?> GetClienteByIDAsync(int id);
        Task CreateClienteAsync(Cliente cliente);
        Task UpdateClienteAsync(Cliente cliente);
        Task DeleteClienteAsync(int id);

        // Retorna Provincias "mock" o de JSON
        Task<IEnumerable<Provincia>> GetProvinciasAsync();
        Task<IEnumerable<Ciudad>> GetCiudadesByProvinciaAsync(int provinciaID);

        // Búsqueda por DNI
        Task<Cliente?> GetClienteByDNIAsync(int dni);

        // Historial de compras (opcional)
        Task AgregarCompraAsync(int clienteID, Compra compra);
    }
}
