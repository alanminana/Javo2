using javo2.ViewModels.Operaciones.Clientes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace javo2.Services
{
    public interface IClienteService
    {
        Task<IEnumerable<ClientesViewModel>> GetAllClientesAsync();
        Task<ClientesViewModel?> GetClienteByIdAsync(int id);
        Task<ClientesViewModel?> GetClienteByDniAsync(int dni);
        Task CreateClienteAsync(ClientesViewModel clienteViewModel);
        Task UpdateClienteAsync(ClientesViewModel clienteViewModel);
        Task DeleteClienteAsync(int id);
    }
}
