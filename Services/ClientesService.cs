using Javo2.IServices;
using Javo2.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Javo2.Helpers;
using System.IO;
using System.Threading;

namespace Javo2.Services
{
    public class ClienteService : IClienteService, IClienteSearchService
    {
        private readonly ILogger<ClienteService> _logger;
        private readonly IAuditoriaService? _auditoriaService;
        private static List<Cliente> _clientes = new();
        private static ReaderWriterLockSlim _lock = new();
        private static int _nextID = 1;
        private readonly string _jsonFilePath = "Data/clientes.json";
        private readonly string _backupDirectory = "Data/Backups";

        private static readonly List<Provincia> _provincias = new()
        {
            new Provincia { ProvinciaID = 1, Nombre = "Buenos Aires" },
            new Provincia { ProvinciaID = 2, Nombre = "Córdoba" },
            new Provincia { ProvinciaID = 3, Nombre = "Santa Fe" }
        };

        private static readonly List<Ciudad> _ciudades = new()
        {
            new Ciudad { CiudadID = 1, Nombre = "La Plata", ProvinciaID = 1 },
            new Ciudad { CiudadID = 2, Nombre = "Mar del Plata", ProvinciaID = 1 },
            new Ciudad { CiudadID = 3, Nombre = "Córdoba Capital", ProvinciaID = 2 },
            new Ciudad { CiudadID = 4, Nombre = "Rosario", ProvinciaID = 3 }
        };

        public ClienteService(ILogger<ClienteService> logger, IAuditoriaService? auditoriaService = null)
        {
            _logger = logger;
            _auditoriaService = auditoriaService;
            InitializeAsync().GetAwaiter().GetResult();
        }

        private async Task InitializeAsync()
        {
            Directory.CreateDirectory(_backupDirectory);
            await CargarDesdeJsonAsync();
        }

        public Task<IEnumerable<Cliente>> GetAllClientesAsync()
        {
            _lock.EnterReadLock();
            try
            {
                return Task.FromResult(_clientes.AsEnumerable());
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public Task<Cliente?> GetClienteByIDAsync(int id)
        {
            _lock.EnterReadLock();
            try
            {
                return Task.FromResult(_clientes.FirstOrDefault(c => c.ClienteID == id));
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public async Task CreateClienteAsync(Cliente cliente)
        {
            _lock.EnterWriteLock();
            try
            {
                cliente.ClienteID = _nextID++;
                cliente.FechaCreacion = DateTime.UtcNow;
                _clientes.Add(cliente);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
            
            await GuardarEnJsonAsync();
            _logger.LogInformation("Cliente creado: ID={ID}, Nombre={Nombre}", cliente.ClienteID, cliente.Nombre);
        }

        public async Task UpdateClienteAsync(Cliente cliente)
        {
            _lock.EnterWriteLock();
            try
            {
                var existing = _clientes.FirstOrDefault(c => c.ClienteID == cliente.ClienteID);
                if (existing != null)
                {
                    var index = _clientes.IndexOf(existing);
                    cliente.FechaModificacion = DateTime.UtcNow;
                    _clientes[index] = cliente;
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
            
            await GuardarEnJsonAsync();
            _logger.LogInformation("Cliente actualizado: ID={ID}", cliente.ClienteID);
        }

        public async Task DeleteClienteAsync(int id)
        {
            _lock.EnterWriteLock();
            try
            {
                var existing = _clientes.FirstOrDefault(c => c.ClienteID == id);
                if (existing != null)
                {
                    _clientes.Remove(existing);
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
            
            await GuardarEnJsonAsync();
            _logger.LogInformation("Cliente eliminado: ID={ID}", id);
        }

        public async Task<(IEnumerable<Cliente> Clientes, int TotalCount)> SearchClientesAsync(
            string? searchTerm = null,
            int? page = null,
            int? pageSize = null,
            string? orderBy = null,
            bool? ascending = true)
        {
            _lock.EnterReadLock();
            try
            {
                var query = _clientes.AsQueryable();

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    searchTerm = searchTerm.ToLower();
                    query = query.Where(c => 
                        c.Nombre.ToLower().Contains(searchTerm) ||
                        c.Apellido.ToLower().Contains(searchTerm) ||
                        c.DNI.ToString().Contains(searchTerm) ||
                        c.Email.ToLower().Contains(searchTerm) ||
                        c.Telefono.Contains(searchTerm) ||
                        c.Celular.Contains(searchTerm));
                }

                var totalCount = query.Count();

                switch (orderBy?.ToLower())
                {
                    case "nombre":
                        query = ascending.GetValueOrDefault() ? 
                            query.OrderBy(c => c.Nombre) : 
                            query.OrderByDescending(c => c.Nombre);
                        break;
                    case "apellido":
                        query = ascending.GetValueOrDefault() ? 
                            query.OrderBy(c => c.Apellido) : 
                            query.OrderByDescending(c => c.Apellido);
                        break;
                    case "dni":
                        query = ascending.GetValueOrDefault() ? 
                            query.OrderBy(c => c.DNI) : 
                            query.OrderByDescending(c => c.DNI);
                        break;
                    default:
                        query = query.OrderByDescending(c => c.FechaCreacion);
                        break;
                }

                if (page.HasValue && pageSize.HasValue)
                {
                    query = query.Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value);
                }

                return (query.ToList(), totalCount);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public Task<IEnumerable<Provincia>> GetProvinciasAsync() => Task.FromResult(_provincias.AsEnumerable());

        public Task<IEnumerable<Ciudad>> GetCiudadesByProvinciaAsync(int provinciaID)
        {
            return Task.FromResult(_ciudades.Where(c => c.ProvinciaID == provinciaID).AsEnumerable());
        }

        public Task<Cliente?> GetClienteByDNIAsync(int dni)
        {
            _lock.EnterReadLock();
            try
            {
                return Task.FromResult(_clientes.FirstOrDefault(c => c.DNI == dni && c.Activo));
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public async Task AgregarCompraAsync(int clienteID, Compra compra)
        {
            _lock.EnterWriteLock();
            try
            {
                var cliente = _clientes.FirstOrDefault(c => c.ClienteID == clienteID);
                if (cliente != null)
                {
                    cliente.Compras ??= new List<Compra>();
                    cliente.Compras.Add(compra);
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
            
            await GuardarEnJsonAsync();
        }

        private async Task CargarDesdeJsonAsync()
        {
            try
            {
                if (!File.Exists(_jsonFilePath))
                {
                    _clientes = new List<Cliente>();
                    return;
                }

                var data = await JsonFileHelper.LoadFromJsonFileAsync<List<Cliente>>(_jsonFilePath);
                _lock.EnterWriteLock();
                try
                {
                    _clientes = data ?? new List<Cliente>();
                    if (_clientes.Any())
                    {
                        _nextID = _clientes.Max(c => c.ClienteID) + 1;
                    }
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar clientes desde JSON");
                _clientes = new List<Cliente>();
                _nextID = 1;
            }
        }

        private async Task GuardarEnJsonAsync()
        {
            List<Cliente> snapshot;
            _lock.EnterReadLock();
            try
            {
                snapshot = _clientes.ToList();
            }
            finally
            {
                _lock.ExitReadLock();
            }

            try
            {
                // Backup automático
                if (File.Exists(_jsonFilePath))
                {
                    var backupPath = Path.Combine(_backupDirectory, 
                        $"clientes_backup_{DateTime.Now:yyyyMMddHHmmss}.json");
                    File.Copy(_jsonFilePath, backupPath, true);

                    // Mantener solo los últimos 10 backups
                    var backupFiles = Directory.GetFiles(_backupDirectory, "clientes_backup_*.json")
                        .OrderByDescending(f => File.GetCreationTime(f))
                        .Skip(10);
                    
                    foreach (var file in backupFiles)
                    {
                        try { File.Delete(file); } catch { }
                    }
                }

                await JsonFileHelper.SaveToJsonFileAsync(_jsonFilePath, snapshot);
                _logger.LogInformation("Clientes guardados: {Count}", snapshot.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar clientes en JSON");
                throw;
            }
        }
    }
}