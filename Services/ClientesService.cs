using Javo2.IServices;
using Javo2.Models;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Javo2.ViewModels.Operaciones.Clientes;

namespace Javo2.Services
{
    public class ClienteService : IClienteService
    {
        private readonly ILogger<ClienteService> _logger;
        private readonly List<Cliente> _clientes;
        private readonly IMapper _mapper;

        public ClienteService(ILogger<ClienteService> logger, IMapper mapper)
        {
            _logger = logger;
            _mapper = mapper;
            _clientes = new List<Cliente>();

            // Datos de ejemplo
            SeedData();
        }

        private void SeedData()
        {
            _clientes.Add(new Cliente
            {
                ClienteID = 1,
                Nombre = "Juan",
                Apellido = "Perez",
                DNI = 12345678,
                Email = "juan.perez@example.com",
                Activo = true
            });

            _clientes.Add(new Cliente
            {
                ClienteID = 2,
                Nombre = "María",
                Apellido = "Gómez",
                DNI = 87654321,
                Email = "maria.gomez@example.com",
                Activo = true
            });
        }

        public Task<IEnumerable<Cliente>> GetAllClientesAsync()
        {
            return Task.FromResult(_clientes.AsEnumerable());
        }

        public Task<Cliente?> GetClienteByIdAsync(int id)
        {
            var cliente = _clientes.FirstOrDefault(c => c.ClienteID == id);
            return Task.FromResult(cliente);
        }

        public Task<Cliente?> GetClienteByDniAsync(int dni)
        {
            var cliente = _clientes.FirstOrDefault(c => c.DNI == dni);
            return Task.FromResult(cliente);
        }


        public Task<Cliente?> GetClienteByEmailAsync(string email)
        {
            var cliente = _clientes.FirstOrDefault(c => c.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(cliente);
        }
        public async Task CreateClienteAsync(Cliente cliente)
        {
            try
            {
                _logger.LogInformation("Creando cliente en el servicio: {@Cliente}", cliente);

                // Validar que no exista un cliente con el mismo DNI
                var existingByDni = await GetClienteByDniAsync(cliente.DNI);
                if (existingByDni != null)
                {
                    throw new ArgumentException($"Ya existe un cliente con el DNI {cliente.DNI}.");
                }

                // Validar que no exista un cliente con el mismo Email
                var existingByEmail = await GetClienteByEmailAsync(cliente.Email);
                if (existingByEmail != null)
                {
                    throw new ArgumentException($"Ya existe un cliente con el email {cliente.Email}.");
                }

                cliente.ClienteID = _clientes.Any() ? _clientes.Max(c => c.ClienteID) + 1 : 1;
                _clientes.Add(cliente);

                _logger.LogInformation("Cliente agregado a la lista. Total de clientes: {Count}", _clientes.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el Cliente: {ErrorMessage}", ex.Message);
                throw;
            }
        }


        public async Task UpdateClienteAsync(Cliente cliente)
        {
            try
            {
                var existingCliente = _clientes.FirstOrDefault(c => c.ClienteID == cliente.ClienteID);

                if (existingCliente == null)
                {
                    throw new KeyNotFoundException($"Cliente con ID {cliente.ClienteID} no encontrado.");
                }

                // Validar que no exista otro cliente con el mismo DNI
                var existingByDni = _clientes.FirstOrDefault(c => c.DNI == cliente.DNI && c.ClienteID != cliente.ClienteID);
                if (existingByDni != null)
                {
                    throw new ArgumentException($"Ya existe otro cliente con el DNI {cliente.DNI}.");
                }

                // Validar que no exista otro cliente con el mismo Email
                var existingByEmail = _clientes.FirstOrDefault(c => c.Email.Equals(cliente.Email, StringComparison.OrdinalIgnoreCase) && c.ClienteID != cliente.ClienteID);
                if (existingByEmail != null)
                {
                    throw new ArgumentException($"Ya existe otro cliente con el email {cliente.Email}.");
                }

                // Actualizar propiedades
                existingCliente.Nombre = cliente.Nombre;
                existingCliente.Apellido = cliente.Apellido;
                existingCliente.DNI = cliente.DNI;
                existingCliente.Email = cliente.Email;
                existingCliente.Telefono = cliente.Telefono;
                existingCliente.Celular = cliente.Celular;
                existingCliente.TelefonoTrabajo = cliente.TelefonoTrabajo;
                existingCliente.Calle = cliente.Calle;
                existingCliente.NumeroCalle = cliente.NumeroCalle;
                existingCliente.NumeroPiso = cliente.NumeroPiso;
                existingCliente.Dpto = cliente.Dpto;
                existingCliente.Localidad = cliente.Localidad;
                existingCliente.CodigoPostal = cliente.CodigoPostal;
                existingCliente.DescripcionDomicilio = cliente.DescripcionDomicilio;
                existingCliente.ProvinciaID = cliente.ProvinciaID;
                existingCliente.CiudadID = cliente.CiudadID;
                existingCliente.ModificadoPor = cliente.ModificadoPor;
                existingCliente.Activo = cliente.Activo;
                existingCliente.FechaModificacion = DateTime.UtcNow;

                // Actualizar otras propiedades según sea necesario
            }
            catch (Exception ex) when (ex is ArgumentException || ex is KeyNotFoundException)
            {
                _logger.LogError(ex, "Error al actualizar el Cliente: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public Task DeleteClienteAsync(int id)
        {
            var cliente = _clientes.FirstOrDefault(c => c.ClienteID == id);
            if (cliente == null)
            {
                throw new KeyNotFoundException($"Cliente con ID {id} no encontrado.");
            }

            _clientes.Remove(cliente);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<Cliente>> FilterClientesAsync(ClienteFilterDtoViewModel filters)
        {
            var clientes = _clientes.AsQueryable();

            if (!string.IsNullOrEmpty(filters.Nombre))
                clientes = clientes.Where(c => c.Nombre.Contains(filters.Nombre, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(filters.Apellido))
                clientes = clientes.Where(c => c.Apellido.Contains(filters.Apellido, StringComparison.OrdinalIgnoreCase));

            if (filters.Dni.HasValue)
                clientes = clientes.Where(c => c.DNI == filters.Dni.Value);

            if (!string.IsNullOrEmpty(filters.Email))
                clientes = clientes.Where(c => c.Email.Contains(filters.Email, StringComparison.OrdinalIgnoreCase));

            return Task.FromResult(clientes.AsEnumerable());
        }
    }
}
