using AutoMapper;
using javo2.ViewModels.Operaciones.Clientes;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace javo2.Services
{
    public class ClienteService : IClienteService
    {
        private readonly IMapper _mapper;
        private readonly ILogger<ClienteService> _logger;

        // Lista estática para simular una base de datos
        private static readonly List<ClientesViewModel> _clientes = new()
        {
            new() { ClienteID = 1, Nombre = "Juan", Apellido = "Perez", DNI = 12345678 },
            new() { ClienteID = 2, Nombre = "Maria", Apellido = "Gomez", DNI = 87654321 }
        };

        public ClienteService(IMapper mapper, ILogger<ClienteService> logger)
        {
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<ClientesViewModel>> GetAllClientesAsync()
        {
            _logger.LogInformation("GetAllClientesAsync called");
            return await Task.FromResult(_clientes);
        }

        public async Task<ClientesViewModel?> GetClienteByIdAsync(int id)
        {
            _logger.LogInformation("GetClienteByIdAsync called with ID: {Id}", id);
            var cliente = _clientes.FirstOrDefault(p => p.ClienteID == id);
            return await Task.FromResult(cliente);
        }

        public async Task<ClientesViewModel?> GetClienteByDniAsync(int dni)
        {
            _logger.LogInformation("GetClienteByDniAsync called with DNI: {Dni}", dni);
            var cliente = _clientes.FirstOrDefault(p => p.DNI == dni);
            return await Task.FromResult(cliente);
        }

        public async Task CreateClienteAsync(ClientesViewModel clienteViewModel)
        {
            _logger.LogInformation("CreateClienteAsync called with Cliente: {Cliente}", clienteViewModel.Nombre);
            clienteViewModel.ClienteID = _clientes.Count > 0 ? _clientes.Max(p => p.ClienteID) + 1 : 1;
            _clientes.Add(clienteViewModel);
            _logger.LogInformation("Cliente created with ID: {Id}", clienteViewModel.ClienteID);
            await Task.CompletedTask;
        }

        public async Task UpdateClienteAsync(ClientesViewModel clienteViewModel)
        {
            _logger.LogInformation("UpdateClienteAsync called with Cliente: {Cliente}", clienteViewModel.Nombre);
            var cliente = _clientes.FirstOrDefault(p => p.ClienteID == clienteViewModel.ClienteID);
            if (cliente != null)
            {
                _mapper.Map(clienteViewModel, cliente);
                _logger.LogInformation("Cliente updated with ID: {Id}", clienteViewModel.ClienteID);
            }
            await Task.CompletedTask;
        }

        public async Task DeleteClienteAsync(int id)
        {
            _logger.LogInformation("DeleteClienteAsync called with ID: {Id}", id);
            var cliente = _clientes.FirstOrDefault(p => p.ClienteID == id);
            if (cliente != null)
            {
                _clientes.Remove(cliente);
                _logger.LogInformation("Cliente deleted with ID: {Id}", id);
            }
            await Task.CompletedTask;
        }
    }
}
