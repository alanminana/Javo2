using Javo2.Helpers;
using Javo2.IServices;
using Javo2.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Javo2.Services
{
    public class GaranteService : IGaranteService
    {
        private readonly ILogger<GaranteService> _logger;
        private readonly IAuditoriaService _auditoriaService;
        private static List<Garante> _garantes = new();
        private static ReaderWriterLockSlim _lock = new();
        private static int _nextID = 1;
        private readonly string _jsonFilePath = "Data/garantes.json";
        private readonly string _backupDirectory = "Data/Backups";

        public GaranteService(ILogger<GaranteService> logger, IAuditoriaService auditoriaService)
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

        public Task<IEnumerable<Garante>> GetAllGarantesAsync()
        {
            _lock.EnterReadLock();
            try
            {
                return Task.FromResult(_garantes.AsEnumerable());
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public Task<Garante> GetGaranteByIdAsync(int id)
        {
            _lock.EnterReadLock();
            try
            {
                var garante = _garantes.FirstOrDefault(g => g.GaranteID == id);
                return Task.FromResult(garante);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public Task<Garante> GetGaranteByDniAsync(int dni)
        {
            _lock.EnterReadLock();
            try
            {
                var garante = _garantes.FirstOrDefault(g => g.DNI == dni);
                return Task.FromResult(garante);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public async Task<Garante> CreateGaranteAsync(Garante garante)
        {
            _lock.EnterWriteLock();
            try
            {
                garante.GaranteID = _nextID++;
                garante.FechaCreacion = DateTime.UtcNow;
                _garantes.Add(garante);
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            await GuardarEnJsonAsync();

            await _auditoriaService.RegistrarCambioAsync(new AuditoriaRegistro
            {
                FechaHora = DateTime.Now,
                Usuario = "Sistema",
                Entidad = "Garante",
                Accion = "Create",
                LlavePrimaria = garante.GaranteID.ToString(),
                Detalle = $"Creación de garante: {garante.Nombre} {garante.Apellido} (DNI: {garante.DNI})"
            });

            _logger.LogInformation("Garante creado: ID={ID}, Nombre={Nombre}", garante.GaranteID, $"{garante.Nombre} {garante.Apellido}");
            return garante;
        }

        public async Task UpdateGaranteAsync(Garante garante)
        {
            _lock.EnterWriteLock();
            try
            {
                var existing = _garantes.FirstOrDefault(g => g.GaranteID == garante.GaranteID);
                if (existing != null)
                {
                    var index = _garantes.IndexOf(existing);
                    _garantes[index] = garante;
                }
                else
                {
                    throw new KeyNotFoundException($"No se encontró el garante con ID {garante.GaranteID}");
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            await GuardarEnJsonAsync();

            await _auditoriaService.RegistrarCambioAsync(new AuditoriaRegistro
            {
                FechaHora = DateTime.Now,
                Usuario = "Sistema",
                Entidad = "Garante",
                Accion = "Update",
                LlavePrimaria = garante.GaranteID.ToString(),
                Detalle = $"Actualización de garante: {garante.Nombre} {garante.Apellido} (DNI: {garante.DNI})"
            });

            _logger.LogInformation("Garante actualizado: ID={ID}", garante.GaranteID);
        }

        public async Task DeleteGaranteAsync(int id)
        {
            Garante garante;
            _lock.EnterWriteLock();
            try
            {
                garante = _garantes.FirstOrDefault(g => g.GaranteID == id);
                if (garante != null)
                {
                    _garantes.Remove(garante);
                }
                else
                {
                    throw new KeyNotFoundException($"No se encontró el garante con ID {id}");
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            await GuardarEnJsonAsync();

            await _auditoriaService.RegistrarCambioAsync(new AuditoriaRegistro
            {
                FechaHora = DateTime.Now,
                Usuario = "Sistema",
                Entidad = "Garante",
                Accion = "Delete",
                LlavePrimaria = id.ToString(),
                Detalle = $"Eliminación de garante: {garante.Nombre} {garante.Apellido} (DNI: {garante.DNI})"
            });

            _logger.LogInformation("Garante eliminado: ID={ID}", id);
        }

        public Task<IEnumerable<Garante>> GetGarantesByClienteIdAsync(int clienteId)
        {
            _lock.EnterReadLock();
            try
            {
                var garantes = _garantes.Where(g => g.ClienteID == clienteId);
                return Task.FromResult(garantes);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        private async Task CargarDesdeJsonAsync()
        {
            try
            {
                if (!File.Exists(_jsonFilePath))
                {
                    _garantes = new List<Garante>();
                    await JsonFileHelper.SaveToJsonFileAsync(_jsonFilePath, _garantes);
                    return;
                }

                var data = await JsonFileHelper.LoadFromJsonFileAsync<List<Garante>>(_jsonFilePath);
                _lock.EnterWriteLock();
                try
                {
                    _garantes = data ?? new List<Garante>();
                    if (_garantes.Any())
                    {
                        _nextID = _garantes.Max(g => g.GaranteID) + 1;
                    }
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar garantes desde JSON");
                _garantes = new List<Garante>();
                _nextID = 1;
            }
        }

        private async Task GuardarEnJsonAsync()
        {
            List<Garante> snapshot;
            _lock.EnterReadLock();
            try
            {
                snapshot = _garantes.ToList();
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
                        $"garantes_backup_{DateTime.Now:yyyyMMddHHmmss}.json");
                    File.Copy(_jsonFilePath, backupPath, true);

                    // Mantener solo los últimos 10 backups
                    var backupFiles = Directory.GetFiles(_backupDirectory, "garantes_backup_*.json")
                        .OrderByDescending(f => File.GetCreationTime(f))
                        .Skip(10);

                    foreach (var file in backupFiles)
                    {
                        try { File.Delete(file); } catch { }
                    }
                }

                await JsonFileHelper.SaveToJsonFileAsync(_jsonFilePath, snapshot);
                _logger.LogInformation("Garantes guardados: {Count}", snapshot.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar garantes en JSON");
                throw;
            }
        }
    }
}