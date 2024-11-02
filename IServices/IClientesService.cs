// IServices/IClienteService.cs
using Javo2.Models;
using Javo2.ViewModels.Operaciones.Clientes;

namespace Javo2.IServices
{
    public interface IClienteService
    {
        /// Obtiene todos los clientes.
        Task<IEnumerable<Cliente>> GetAllClientesAsync();
        /// Obtiene un cliente por su ID.
        Task<Cliente?> GetClienteByIdAsync(int id);
        Task<Cliente?> GetClienteByDniAsync(int dni);
        Task CreateClienteAsync(Cliente cliente);
        Task UpdateClienteAsync(Cliente cliente);
        Task DeleteClienteAsync(int id);
        Task<IEnumerable<Cliente>> FilterClientesAsync(ClienteFilterDtoViewModel filters);
    }
}
