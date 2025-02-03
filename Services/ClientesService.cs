// Ruta: Services/ClienteService.cs
using Javo2.IServices;
using Javo2.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Javo2.Helpers; // Para JsonFileHelper si lo tienes
using System.IO;

namespace Javo2.Services
{
    public class ClienteService : IClienteService
    {
        private readonly ILogger<ClienteService> _logger;
        private readonly IAuditoriaService? _auditoriaService; // Opcional, inyectar para registrar auditoría

        private static List<Cliente> _clientes = new();
        private static readonly object _lock = new();
        private static int _nextID = 1;

        private readonly string _jsonFilePath = "Data/clientes.json"; // Ruta para persistir

        // Provincias y Ciudades "mock"
        private static readonly List<Provincia> _provincias = new()
        {
            new Provincia { ProvinciaID = 1, Nombre = "Buenos Aires" },
            new Provincia { ProvinciaID = 2, Nombre = "Córdoba" },
            new Provincia { ProvinciaID = 3, Nombre = "Santa Fe" }
        };

        private static readonly List<Ciudad> _ciudades = new()
        {
            new Ciudad { CiudadID = 1, Nombre = "La Plata",       ProvinciaID = 1 },
            new Ciudad { CiudadID = 2, Nombre = "Mar del Plata",  ProvinciaID = 1 },
            new Ciudad { CiudadID = 3, Nombre = "Córdoba Capital",ProvinciaID = 2 },
            new Ciudad { CiudadID = 4, Nombre = "Rosario",        ProvinciaID = 3 }
        };

        public ClienteService(
            ILogger<ClienteService> logger,
            IAuditoriaService? auditoriaService = null // inyectar si lo deseas
        )
        {
            _logger = logger;
            _auditoriaService = auditoriaService;
            CargarDesdeJson();
        }

        // ========================================
        // Métodos CRUD
        // ========================================
        public Task<IEnumerable<Cliente>> GetAllClientesAsync()
        {
            lock (_lock)
            {
                _logger.LogInformation("GetAllClientesAsync: {Count} clientes en memoria", _clientes.Count);
                return Task.FromResult(_clientes.AsEnumerable());
            }
        }

        public Task<Cliente?> GetClienteByIDAsync(int id)
        {
            lock (_lock)
            {
                var cliente = _clientes.FirstOrDefault(c => c.ClienteID == id);
                return Task.FromResult(cliente);
            }
        }

        public Task CreateClienteAsync(Cliente cliente)
        {
            lock (_lock)
            {
                cliente.ClienteID = _nextID++;
                _clientes.Add(cliente);
                GuardarEnJson();

                _logger.LogInformation("CreateClienteAsync: nuevo cliente ID={ID}, Nombre={Nombre}", cliente.ClienteID, cliente.Nombre);

                // Auditoría opcional
                _auditoriaService?.RegistrarCambioAsync(new AuditoriaRegistro
                {
                    FechaHora = DateTime.Now,
                    Usuario = "Sistema",
                    Entidad = "Cliente",
                    Accion = "Create",
                    LlavePrimaria = cliente.ClienteID.ToString(),
                    Detalle = $"Nombre={cliente.Nombre}, Apellido={cliente.Apellido}"
                });
            }
            return Task.CompletedTask;
        }

        public Task UpdateClienteAsync(Cliente cliente)
        {
            lock (_lock)
            {
                var existing = _clientes.FirstOrDefault(c => c.ClienteID == cliente.ClienteID);
                if (existing != null)
                {
                    // Actualiza propiedades
                    var index = _clientes.IndexOf(existing);
                    _clientes[index] = cliente;
                    GuardarEnJson();

                    _logger.LogInformation("UpdateClienteAsync: cliente ID={ID} actualizado", cliente.ClienteID);

                    _auditoriaService?.RegistrarCambioAsync(new AuditoriaRegistro
                    {
                        FechaHora = DateTime.Now,
                        Usuario = "Sistema",
                        Entidad = "Cliente",
                        Accion = "Edit",
                        LlavePrimaria = cliente.ClienteID.ToString(),
                        Detalle = $"Nombre={cliente.Nombre}, Apellido={cliente.Apellido}"
                    });
                }
                else
                {
                    _logger.LogWarning("UpdateClienteAsync: no se encontró cliente ID={ID} para actualizar", cliente.ClienteID);
                }
            }
            return Task.CompletedTask;
        }

        public Task DeleteClienteAsync(int id)
        {
            lock (_lock)
            {
                var existing = _clientes.FirstOrDefault(c => c.ClienteID == id);
                if (existing != null)
                {
                    _clientes.Remove(existing);
                    GuardarEnJson();

                    _logger.LogInformation("DeleteClienteAsync: cliente ID={ID} eliminado", id);

                    _auditoriaService?.RegistrarCambioAsync(new AuditoriaRegistro
                    {
                        FechaHora = DateTime.Now,
                        Usuario = "Sistema",
                        Entidad = "Cliente",
                        Accion = "Delete",
                        LlavePrimaria = id.ToString(),
                        Detalle = $"Eliminado cliente {existing.Nombre} {existing.Apellido}"
                    });
                }
                else
                {
                    _logger.LogWarning("DeleteClienteAsync: cliente ID={ID} no encontrado", id);
                }
            }
            return Task.CompletedTask;
        }

        // ========================================
        // Métodos de Provincias / Ciudades
        // ========================================
        public Task<IEnumerable<Provincia>> GetProvinciasAsync()
        {
            lock (_lock)
            {
                return Task.FromResult(_provincias.AsEnumerable());
            }
        }

        public Task<IEnumerable<Ciudad>> GetCiudadesByProvinciaAsync(int provinciaID)
        {
            lock (_lock)
            {
                var lista = _ciudades.Where(c => c.ProvinciaID == provinciaID).ToList();
                return Task.FromResult(lista.AsEnumerable());
            }
        }

        // ========================================
        // Otros (buscar DNI, agregar compra)
        // ========================================
        public Task<Cliente?> GetClienteByDNIAsync(int dni)
        {
            lock (_lock)
            {
                var cliente = _clientes.FirstOrDefault(c => c.DNI == dni && c.Activo);
                return Task.FromResult(cliente);
            }
        }

        public Task AgregarCompraAsync(int clienteID, Compra compra)
        {
            lock (_lock)
            {
                var cliente = _clientes.FirstOrDefault(c => c.ClienteID == clienteID);
                if (cliente == null)
                {
                    _logger.LogWarning("AgregarCompraAsync: no se encontró cliente ID={ID}", clienteID);
                }
                else
                {
                    if (cliente.Compras == null)
                        cliente.Compras = new List<Compra>();

                    cliente.Compras.Add(compra);

                    GuardarEnJson();
                    _logger.LogInformation("AgregarCompraAsync: se agregó compra ID={CompraID} al cliente ID={ClienteID}", compra.CompraID, clienteID);

                    _auditoriaService?.RegistrarCambioAsync(new AuditoriaRegistro
                    {
                        FechaHora = DateTime.Now,
                        Usuario = "Sistema",
                        Entidad = "Cliente",
                        Accion = "AgregarCompra",
                        LlavePrimaria = clienteID.ToString(),
                        Detalle = $"Se registró compra ID={compra.CompraID}, Producto={compra.Producto}"
                    });
                }
            }
            return Task.CompletedTask;
        }

        // ========================================
        // Persistencia en JSON
        // ========================================
        private void CargarDesdeJson()
        {
            lock (_lock)
            {
                try
                {
                    if (!File.Exists(_jsonFilePath))
                    {
                        _clientes = new List<Cliente>();
                        return;
                    }
                    _clientes = JsonFileHelper.LoadFromJsonFile<List<Cliente>>(_jsonFilePath) ?? new List<Cliente>();
                    if (_clientes.Any())
                    {
                        _nextID = _clientes.Max(c => c.ClienteID) + 1;
                    }
                    _logger.LogInformation("ClienteService: {Count} clientes cargados desde {File}", _clientes.Count, _jsonFilePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al cargar clientes desde JSON");
                    _clientes = new List<Cliente>();
                    _nextID = 1;
                }
            }
        }

        private void GuardarEnJson()
        {
            lock (_lock)
            {
                try
                {
                    JsonFileHelper.SaveToJsonFile(_jsonFilePath, _clientes);
                    _logger.LogInformation("ClienteService: guardados {Count} clientes en {File}", _clientes.Count, _jsonFilePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al guardar clientes en JSON");
                }
            }
        }
    }
}
