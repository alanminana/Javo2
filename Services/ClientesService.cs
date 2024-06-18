
using AutoMapper;
using Javo2.ViewModels.Operaciones.Clientes;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Javo2.Services
{
    public class ClienteService : IClienteService
    {
        private readonly IMapper _mapper;
        private readonly ILogger<ClienteService> _logger;

        private static readonly ConcurrentDictionary<int, ClientesViewModel> _clientes = new()
        {
            [1] = new ClientesViewModel { ClienteID = 1, Nombre = "Juan", Apellido = "Perez", DNI = 12345678, Email = "juan.perez@example.com" },
            [2] = new ClientesViewModel { ClienteID = 2, Nombre = "Maria", Apellido = "Gomez", DNI = 87654321, Email = "maria.gomez@example.com" }
        };

        public ClienteService(IMapper mapper, ILogger<ClienteService> logger)
        {
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<ClientesViewModel>> GetAllClientesAsync()
        {
            _logger.LogInformation("GetAllClientesAsync called");
            return await Task.FromResult(_clientes.Values);
        }

        public async Task<ClientesViewModel?> GetClienteByIdAsync(int id)
        {
            _logger.LogInformation("GetClienteByIdAsync called with ID: {Id}", id);
            _clientes.TryGetValue(id, out var cliente);
            return await Task.FromResult(cliente);
        }

        public async Task<ClientesViewModel?> GetClienteByDniAsync(int dni)
        {
            _logger.LogInformation("GetClienteByDniAsync called with DNI: {Dni}", dni);
            var cliente = _clientes.Values.FirstOrDefault(p => p.DNI == dni);
            return await Task.FromResult(cliente);
        }

        public async Task CreateClienteAsync(ClientesViewModel clienteViewModel)
        {
            _logger.LogInformation("CreateClienteAsync called with Cliente: {Cliente}", clienteViewModel.Nombre);
            clienteViewModel.ClienteID = _clientes.Keys.DefaultIfEmpty(0).Max() + 1;
            _clientes[clienteViewModel.ClienteID] = clienteViewModel;
            await Task.CompletedTask;
        }

        public async Task UpdateClienteAsync(ClientesViewModel clienteViewModel)
        {
            _logger.LogInformation("UpdateClienteAsync called with Cliente: {Cliente}", clienteViewModel.Nombre);
            if (_clientes.ContainsKey(clienteViewModel.ClienteID))
            {
                _clientes[clienteViewModel.ClienteID] = clienteViewModel;
            }
            await Task.CompletedTask;
        }

        public async Task DeleteClienteAsync(int id)
        {
            _logger.LogInformation("DeleteClienteAsync called with ID: {Id}", id);
            _clientes.TryRemove(id, out _);
            await Task.CompletedTask;
        }

        public async Task<IEnumerable<ClientesViewModel>> FilterClientesAsync(ClienteFilterDto filters)
        {
            _logger.LogInformation("FilterClientesAsync called with filters");
            var clientes = _clientes.Values.AsQueryable();

            if (!string.IsNullOrEmpty(filters.Nombre))
                clientes = clientes.Where(c => c.Nombre.Contains(filters.Nombre, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrEmpty(filters.Apellido))
                clientes = clientes.Where(c => c.Apellido.Contains(filters.Apellido, StringComparison.OrdinalIgnoreCase));
            if (filters.Dni.HasValue)
                clientes = clientes.Where(c => c.DNI == filters.Dni.Value);
            if (!string.IsNullOrEmpty(filters.Email))
                clientes = clientes.Where(c => c.Email.Contains(filters.Email, StringComparison.OrdinalIgnoreCase));

            return await Task.FromResult(clientes.ToList());
        }
    }
}