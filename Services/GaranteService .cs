// Services/GaranteService.cs

using Javo2.Helpers;
using Javo2.IServices;
using Javo2.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Javo2.Services
{
    public class GaranteService : IGaranteService
    {
        private readonly ILogger<GaranteService> _logger;
        private static List<Garante> _garantes = new List<Garante>();
        private static int _nextId = 1;
        private static readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private readonly string _jsonFilePath = "Data/garantes.json";

        public GaranteService(ILogger<GaranteService> logger)
        {
            _logger = logger;
            InitializeAsync().GetAwaiter().GetResult();
        }

        private async Task InitializeAsync()
        {
            await CargarDesdeJsonAsync();
        }

        private async Task CargarDesdeJsonAsync()
        {
            try
            {
                var data = await JsonFileHelper.LoadFromJsonFileAsync<List<Garante>>(_jsonFilePath);
                _lock.EnterWriteLock();
                try
                {
                    _garantes = data ?? new List<Garante>();
                    if (_garantes.Any())
                    {
                        _nextId = _garantes.Max(g => g.GaranteID) + 1;
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
                _nextId = 1;
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
                await JsonFileHelper.SaveToJsonFileAsync(_jsonFilePath, snapshot);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar garantes en JSON");
                throw;
            }
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

        public Task<Garante?> GetGaranteByIdAsync(int id)
        {
            _lock.EnterReadLock();
            try
            {
                return Task.FromResult(_garantes.FirstOrDefault(g => g.GaranteID == id));
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
                garante.GaranteID = _nextId++;
                garante.FechaCreacion = DateTime.UtcNow;
                _garantes.Add(garante);
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            await GuardarEnJsonAsync();
            _logger.LogInformation("Garante creado: ID={ID}, Nombre={Nombre}", garante.GaranteID, $"{garante.Nombre} {garante.Apellido}");
            return garante;
        }

        public async Task UpdateGaranteAsync(Garante garante)
        {
            _lock.EnterWriteLock();
            try
            {
                var existing = _garantes.FirstOrDefault(g => g.GaranteID == garante.GaranteID);
                if (existing == null)
                {
                    throw new KeyNotFoundException($"Garante con ID {garante.GaranteID} no encontrado.");
                }

                // Actualizar propiedades
                existing.Nombre = garante.Nombre;
                existing.Apellido = garante.Apellido;
                existing.DNI = garante.DNI;
                existing.Email = garante.Email;
                existing.Telefono = garante.Telefono;
                existing.Celular = garante.Celular;
                existing.Calle = garante.Calle;
                existing.NumeroCalle = garante.NumeroCalle;
                existing.NumeroPiso = garante.NumeroPiso;
                existing.Dpto = garante.Dpto;
                existing.Localidad = garante.Localidad;
                existing.CodigoPostal = garante.CodigoPostal;
                existing.ProvinciaID = garante.ProvinciaID;
                existing.CiudadID = garante.CiudadID;
                existing.LugarTrabajo = garante.LugarTrabajo;
                existing.IngresosMensuales = garante.IngresosMensuales;
                existing.RelacionCliente = garante.RelacionCliente;
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            await GuardarEnJsonAsync();
            _logger.LogInformation("Garante actualizado: ID={ID}", garante.GaranteID);
        }

        public async Task DeleteGaranteAsync(int id)
        {
            _lock.EnterWriteLock();
            try
            {
                var garante = _garantes.FirstOrDefault(g => g.GaranteID == id);
                if (garante == null)
                {
                    throw new KeyNotFoundException($"Garante con ID {id} no encontrado.");
                }

                _garantes.Remove(garante);
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            await GuardarEnJsonAsync();
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
    }
}