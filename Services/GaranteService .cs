// Archivo: Services/GaranteService.cs
using Javo2.Helpers;
using Javo2.IServices;
using Javo2.Models;
using Javo2.Services.Base;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Javo2.Services
{
    public class GaranteService : JsonDataService<Garante>, IGaranteService
    {
        private readonly ILogger<GaranteService> _logger;
        private static List<Garante> _garantes = new();
        private static int _nextID = 1;
        private static readonly object _lock = new();
        private readonly string _jsonFilePath = "Data/garantes.json";

        public GaranteService(ILogger<GaranteService> logger)
        {
            _logger = logger;
            CargarDesdeJsonAsync().GetAwaiter().GetResult();
        }

        private async Task CargarDesdeJsonAsync()
        {
            try
            {
                var data = await JsonFileHelper.LoadFromJsonFileAsync<List<Garante>>(_jsonFilePath);
                lock (_lock)
                {
                    _garantes = data ?? new List<Garante>();

                    if (_garantes.Any())
                    {
                        _nextID = _garantes.Max(g => g.GaranteID) + 1;
                    }
                }
                _logger.LogInformation("Garantes cargados: {Count}", _garantes.Count);
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
            lock (_lock)
            {
                snapshot = _garantes.ToList();
            }

            try
            {
                await JsonFileHelper.SaveToJsonFileAsync(_jsonFilePath, snapshot);
                _logger.LogInformation("Garantes guardados: {Count}", snapshot.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar garantes en JSON");
                throw;
            }
        }

        public async Task<Garante?> GetGaranteByIdAsync(int id)
        {
            lock (_lock)
            {
                return _garantes.FirstOrDefault(g => g.GaranteID == id);
            }
        }

        public async Task<Garante?> GetGaranteByDniAsync(int dni)
        {
            lock (_lock)
            {
                return _garantes.FirstOrDefault(g => g.DNI == dni);
            }
        }

        public async Task<Garante> CreateGaranteAsync(Garante garante)
        {
            lock (_lock)
            {
                garante.GaranteID = _nextID++;
                garante.FechaCreacion = DateTime.UtcNow;
                _garantes.Add(garante);
            }

            await GuardarEnJsonAsync();

            _logger.LogInformation("Garante creado: ID={ID}, Nombre={Nombre} {Apellido}",
                garante.GaranteID, garante.Nombre, garante.Apellido);

            return garante;
        }

        public async Task<bool> UpdateGaranteAsync(Garante garante)
        {
            lock (_lock)
            {
                var existing = _garantes.FirstOrDefault(g => g.GaranteID == garante.GaranteID);
                if (existing == null)
                {
                    return false;
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

            await GuardarEnJsonAsync();

            _logger.LogInformation("Garante actualizado: ID={ID}", garante.GaranteID);

            return true;
        }

        public async Task<bool> DeleteGaranteAsync(int id)
        {
            lock (_lock)
            {
                var garante = _garantes.FirstOrDefault(g => g.GaranteID == id);
                if (garante == null)
                {
                    return false;
                }

                _garantes.Remove(garante);
            }

            await GuardarEnJsonAsync();

            _logger.LogInformation("Garante eliminado: ID={ID}", id);

            return true;
        }

        public async Task<IEnumerable<Garante>> GetAllGarantesAsync()
        {
            lock (_lock)
            {
                return _garantes.ToList();
            }
        }
    }
}