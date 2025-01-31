using Javo2.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Javo2.IServices
{
    public interface IClienteService
    {
        Task<IEnumerable<Cliente>> GetAllClientesAsync();
        Task<Cliente> GetClienteByIDAsync(int id);
        Task CreateClienteAsync(Cliente cliente);
        Task UpdateClienteAsync(Cliente cliente);
        Task DeleteClienteAsync(int id);

        Task<IEnumerable<Provincia>> GetProvinciasAsync();
        Task<IEnumerable<Ciudad>> GetCiudadesByProvinciaAsync(int provinciaID);

        Task<Cliente> GetClienteByDNIAsync(int dni);
        Task AgregarCompraAsync(int clienteID, Compra compra);
    }
}
