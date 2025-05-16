using Javo2.Helpers;
using Javo2.IServices;
using Javo2.Models;
using Javo2.Services.Base;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Javo2.Services
{
    public class ClienteService : JsonDataService<Cliente>, IClienteService, IClienteSearchService
    {

        private readonly IAuditoriaService? _auditoriaService;
        private readonly IGaranteService? _garanteService;
        private readonly ILogger<ClienteService> _logger;
    
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

        public ClienteService(
            ILogger<ClienteService> logger,
            IAuditoriaService? auditoriaService = null,
            IGaranteService? garanteService = null)
        {
            _logger = logger;
            _auditoriaService = auditoriaService;
            _garanteService = garanteService;
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

                // Si el cliente es apto para crédito, asegurarse de tener un límite de crédito
                if (cliente.AptoCredito && cliente.LimiteCreditoInicial <= 0)
                {
                    cliente.LimiteCreditoInicial = 10000; // Valor por defecto
                }

                // Inicializar saldos
                cliente.SaldoInicial = 0;
                cliente.Saldo = 0;
                cliente.SaldoDisponible = cliente.AptoCredito ? cliente.LimiteCreditoInicial : 0;

                _clientes.Add(cliente);
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            await GuardarEnJsonAsync();

            if (_auditoriaService != null)
            {
                await _auditoriaService.RegistrarCambioAsync(new AuditoriaRegistro
                {
                    FechaHora = DateTime.Now,
                    Usuario = cliente.ModificadoPor,
                    Entidad = "Cliente",
                    Accion = "Create",
                    LlavePrimaria = cliente.ClienteID.ToString(),
                    Detalle = $"Cliente creado: {cliente.Nombre} {cliente.Apellido}, DNI: {cliente.DNI}"
                });
            }

            _logger.LogInformation("Cliente creado: ID={ID}, Nombre={Nombre}", cliente.ClienteID, $"{cliente.Nombre} {cliente.Apellido}");
        }

        public async Task UpdateClienteAsync(Cliente cliente)
        {
            Cliente? existing;
            _lock.EnterWriteLock();
            try
            {
                existing = _clientes.FirstOrDefault(c => c.ClienteID == cliente.ClienteID);
                if (existing != null)
                {
                    // Guardar valores anteriores para auditoría
                    bool anteriorAptoCredito = existing.AptoCredito;
                    decimal anteriorLimiteCredito = existing.LimiteCreditoInicial;

                    // Actualizar campos básicos
                    existing.Nombre = cliente.Nombre;
                    existing.Apellido = cliente.Apellido;
                    existing.DNI = cliente.DNI;
                    existing.Email = cliente.Email;
                    existing.Telefono = cliente.Telefono;
                    existing.Celular = cliente.Celular;
                    existing.TelefonoTrabajo = cliente.TelefonoTrabajo;
                    existing.Calle = cliente.Calle;
                    existing.NumeroCalle = cliente.NumeroCalle;
                    existing.NumeroPiso = cliente.NumeroPiso;
                    existing.Dpto = cliente.Dpto;
                    existing.Localidad = cliente.Localidad;
                    existing.CodigoPostal = cliente.CodigoPostal;
                    existing.DescripcionDomicilio = cliente.DescripcionDomicilio;
                    existing.ProvinciaID = cliente.ProvinciaID;
                    existing.CiudadID = cliente.CiudadID;
                    existing.ModificadoPor = cliente.ModificadoPor;
                    existing.Activo = cliente.Activo;
                    existing.FechaModificacion = DateTime.UtcNow;

                    // Actualizar campos de crédito
                    existing.AptoCredito = cliente.AptoCredito;
                    existing.RequiereGarante = cliente.RequiereGarante;
                    existing.GaranteID = cliente.GaranteID;

                    // Lógica para cuando cambia el estado de crédito o el límite
                    if (cliente.AptoCredito)
                    {
                        // Si cambió el límite o es nuevo apto de crédito
                        if (!anteriorAptoCredito || cliente.LimiteCreditoInicial != anteriorLimiteCredito)
                        {
                            existing.LimiteCreditoInicial = cliente.LimiteCreditoInicial;

                            // Recalcular saldo disponible
                            existing.SaldoDisponible = existing.LimiteCreditoInicial - existing.DeudaTotal;
                        }
                    }
                    else
                    {
                        // Si ya no es apto para crédito
                        if (anteriorAptoCredito)
                        {
                            // Mantener el límite pero establecer saldo disponible a 0
                            existing.SaldoDisponible = 0;
                        }
                    }
                }
                else
                {
                    throw new KeyNotFoundException($"Cliente con ID {cliente.ClienteID} no encontrado");
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            await GuardarEnJsonAsync();

            if (_auditoriaService != null)
            {
                await _auditoriaService.RegistrarCambioAsync(new AuditoriaRegistro
                {
                    FechaHora = DateTime.Now,
                    Usuario = cliente.ModificadoPor,
                    Entidad = "Cliente",
                    Accion = "Update",
                    LlavePrimaria = cliente.ClienteID.ToString(),
                    Detalle = $"Cliente actualizado: {cliente.Nombre} {cliente.Apellido}, DNI: {cliente.DNI}"
                });
            }

            _logger.LogInformation("Cliente actualizado: ID={ID}", cliente.ClienteID);
        }

        public async Task DeleteClienteAsync(int id)
        {
            Cliente? cliente;
            _lock.EnterWriteLock();
            try
            {
                cliente = _clientes.FirstOrDefault(c => c.ClienteID == id);
                if (cliente != null)
                {
                    // Verificar si tiene deuda antes de eliminar
                    if (cliente.DeudaTotal > 0)
                    {
                        throw new InvalidOperationException($"No se puede eliminar el cliente ID {id} porque tiene una deuda pendiente de {cliente.DeudaTotal:C}");
                    }

                    _clientes.Remove(cliente);
                }
                else
                {
                    throw new KeyNotFoundException($"Cliente con ID {id} no encontrado");
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            await GuardarEnJsonAsync();

            if (_auditoriaService != null)
            {
                await _auditoriaService.RegistrarCambioAsync(new AuditoriaRegistro
                {
                    FechaHora = DateTime.Now,
                    Usuario = "Sistema",
                    Entidad = "Cliente",
                    Accion = "Delete",
                    LlavePrimaria = id.ToString(),
                    Detalle = $"Cliente eliminado: {cliente.Nombre} {cliente.Apellido}, DNI: {cliente.DNI}"
                });
            }

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
                else
                {
                    throw new KeyNotFoundException($"Cliente con ID {clienteID} no encontrado");
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            await GuardarEnJsonAsync();
        }

        public async Task<bool> AsignarGaranteAsync(int clienteID, int garanteID)
        {
            _lock.EnterWriteLock();
            try
            {
                var cliente = _clientes.FirstOrDefault(c => c.ClienteID == clienteID);
                if (cliente == null)
                {
                    return false;
                }

                // Verificar si el garante existe
                if (_garanteService != null)
                {
                    var garante = await _garanteService.GetGaranteByIdAsync(garanteID);
                    if (garante == null)
                    {
                        return false;
                    }
                }

                cliente.GaranteID = garanteID;
                cliente.FechaModificacion = DateTime.UtcNow;
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            await GuardarEnJsonAsync();

            if (_auditoriaService != null)
            {
                await _auditoriaService.RegistrarCambioAsync(new AuditoriaRegistro
                {
                    FechaHora = DateTime.Now,
                    Usuario = "Sistema",
                    Entidad = "Cliente",
                    Accion = "AsignarGarante",
                    LlavePrimaria = clienteID.ToString(),
                    Detalle = $"Garante (ID: {garanteID}) asignado al cliente ID: {clienteID}"
                });
            }

            _logger.LogInformation("Garante (ID: {GaranteID}) asignado al cliente ID: {ClienteID}", garanteID, clienteID);
            return true;
        }

        public async Task<bool> AjustarLimiteCreditoAsync(int clienteID, decimal nuevoLimite, string usuario)
        {
            decimal limiteAnterior;

            _lock.EnterWriteLock();
            try
            {
                var cliente = _clientes.FirstOrDefault(c => c.ClienteID == clienteID);
                if (cliente == null)
                {
                    return false;
                }

                // Guardar límite anterior para auditoría
                limiteAnterior = cliente.LimiteCreditoInicial;

                // Actualizar límite y saldo disponible
                cliente.LimiteCreditoInicial = nuevoLimite;
                cliente.SaldoDisponible = nuevoLimite - cliente.DeudaTotal;
                cliente.FechaModificacion = DateTime.UtcNow;
                cliente.ModificadoPor = usuario;
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            await GuardarEnJsonAsync();

            if (_auditoriaService != null)
            {
                await _auditoriaService.RegistrarCambioAsync(new AuditoriaRegistro
                {
                    FechaHora = DateTime.Now,
                    Usuario = usuario,
                    Entidad = "Cliente",
                    Accion = "AjusteCredito",
                    LlavePrimaria = clienteID.ToString(),
                    Detalle = $"Límite de crédito ajustado de {limiteAnterior:C} a {nuevoLimite:C}"
                });
            }

            _logger.LogInformation("Límite de crédito del cliente ID {ClienteID} ajustado a {NuevoLimite}", clienteID, nuevoLimite);
            return true;
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