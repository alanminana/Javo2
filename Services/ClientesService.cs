// Archivo: Services/ClienteService.cs
using Javo2.IServices;
using Javo2.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Javo2.Services
{
    public class ClienteService : IClienteService
    {
        private readonly ILogger<ClienteService> _logger;

        // == LISTAS ESTÁTICAS EN MEMORIA (sin EF) ==
        private static readonly List<Cliente> _clientes = new List<Cliente>();

        private static readonly List<Provincia> _provincias = new List<Provincia>
        {
            new Provincia { ProvinciaID = 1, Nombre = "Buenos Aires" },
            new Provincia { ProvinciaID = 2, Nombre = "Córdoba" },
            new Provincia { ProvinciaID = 3, Nombre = "Santa Fe" }
        };

        private static readonly List<Ciudad> _ciudades = new List<Ciudad>
        {
            new Ciudad { CiudadID = 1, Nombre = "La Plata",       ProvinciaID = 1 },
            new Ciudad { CiudadID = 2, Nombre = "Mar del Plata",  ProvinciaID = 1 },
            new Ciudad { CiudadID = 3, Nombre = "Córdoba Capital",ProvinciaID = 2 },
            new Ciudad { CiudadID = 4, Nombre = "Rosario",        ProvinciaID = 3 }
        };

        public ClienteService(ILogger<ClienteService> logger)
        {
            _logger = logger;
        }

        // Listar todos los clientes
        public Task<IEnumerable<Cliente>> GetAllClientesAsync()
        {
            _logger.LogInformation("GetAllClientesAsync llamado. Cantidad actual de clientes en memoria: {Count}", _clientes.Count);
            return Task.FromResult(_clientes.AsEnumerable());
        }

        // Buscar un cliente por ID
        public Task<Cliente> GetClienteByIDAsync(int id)
        {
            var cliente = _clientes.FirstOrDefault(c => c.ClienteID == id);
            if (cliente == null)
            {
                _logger.LogWarning("GetClienteByIDAsync: cliente con ID={ID} no encontrado", id);
            }
            else
            {
                _logger.LogInformation("GetClienteByIDAsync: cliente {Nombre} {Apellido}, ID={ID}, CiudadID={CiudadID}",
                    cliente.Nombre, cliente.Apellido, cliente.ClienteID, cliente.CiudadID);
            }
            return Task.FromResult(cliente);
        }

        // Crear un nuevo cliente
        public Task CreateClienteAsync(Cliente cliente)
        {
            if (_clientes.Count == 0)
                cliente.ClienteID = 1;
            else
                cliente.ClienteID = _clientes.Max(c => c.ClienteID) + 1;

            _clientes.Add(cliente);

            _logger.LogInformation("CreateClienteAsync: nuevo cliente creado con ID={ID}, Nombre={Nombre}, Apellido={Apellido}, CiudadID={CiudadID}",
                cliente.ClienteID, cliente.Nombre, cliente.Apellido, cliente.CiudadID);

            return Task.CompletedTask;
        }

        // Actualizar un cliente
        public Task UpdateClienteAsync(Cliente cliente)
        {
            var existing = _clientes.FirstOrDefault(c => c.ClienteID == cliente.ClienteID);
            if (existing != null)
            {
                var index = _clientes.IndexOf(existing);
                _clientes[index] = cliente;

                _logger.LogInformation("UpdateClienteAsync: cliente ID={ID} actualizado. Nombre={Nombre}, CiudadID={CiudadID}",
                    cliente.ClienteID, cliente.Nombre, cliente.CiudadID);
            }
            else
            {
                _logger.LogWarning("UpdateClienteAsync: no se encontró cliente con ID={ID} para actualizar", cliente.ClienteID);
            }
            return Task.CompletedTask;
        }

        // Eliminar un cliente
        public Task DeleteClienteAsync(int id)
        {
            var existing = _clientes.FirstOrDefault(c => c.ClienteID == id);
            if (existing != null)
            {
                _clientes.Remove(existing);
                _logger.LogInformation("DeleteClienteAsync: cliente ID={ID} eliminado", id);
            }
            else
            {
                _logger.LogWarning("DeleteClienteAsync: cliente ID={ID} no encontrado", id);
            }
            return Task.CompletedTask;
        }

        // Retornar Provincias "mock"
        public Task<IEnumerable<Provincia>> GetProvinciasAsync()
        {
            _logger.LogInformation("GetProvinciasAsync: retornando {Count} provincias", _provincias.Count);
            return Task.FromResult(_provincias.AsEnumerable());
        }

        // Retornar Ciudades por Provincia (en memoria)
        public Task<IEnumerable<Ciudad>> GetCiudadesByProvinciaAsync(int provinciaID)
        {
            var ciudadesFiltradas = _ciudades.Where(c => c.ProvinciaID == provinciaID).ToList();
            _logger.LogInformation("GetCiudadesByProvinciaAsync: ProvinciaID={ProvinciaID}, encontradas {Count} ciudades",
                provinciaID, ciudadesFiltradas.Count);
            return Task.FromResult(ciudadesFiltradas.AsEnumerable());
        }

        // Obtener un cliente por DNI
        public Task<Cliente> GetClienteByDNIAsync(int dni)
        {
            var cliente = _clientes.FirstOrDefault(c => c.DNI == dni && c.Activo);
            if (cliente == null)
            {
                _logger.LogWarning("GetClienteByDNIAsync: no se encontró cliente con DNI={Dni}", dni);
            }
            else
            {
                _logger.LogInformation("GetClienteByDNIAsync: encontrado cliente ID={ID}, Nombre={Nombre}",
                    cliente.ClienteID, cliente.Nombre);
            }
            return Task.FromResult(cliente);
        }

        // Agregar compra al historial de un cliente
        public Task AgregarCompraAsync(int clienteID, Compra compra)
        {
            var cliente = _clientes.FirstOrDefault(c => c.ClienteID == clienteID);
            if (cliente != null)
            {
                if (cliente.Compras == null)
                {
                    cliente.Compras = new List<Compra>();
                }
                cliente.Compras.Add(compra);

                _logger.LogInformation("AgregarCompraAsync: Agregada compra ID={CompraID} al cliente ID={ClienteID}",
                    compra.CompraID, clienteID);
            }
            else
            {
                _logger.LogWarning("AgregarCompraAsync: no se encontró cliente ID={ID} para agregar la compra", clienteID);
            }
            return Task.CompletedTask;
        }
    }
}
