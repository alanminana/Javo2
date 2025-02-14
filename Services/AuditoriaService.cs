// File: Services/AuditoriaService.cs
using Javo2.IServices;
using Javo2.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Javo2.Helpers;

namespace Javo2.Services
{
    public class AuditoriaService : IAuditoriaService
    {
        private readonly ILogger<AuditoriaService> _logger;
        private static List<AuditoriaRegistro> _registros = new();
        private static int _nextID = 1;
        private readonly string _jsonFilePath = "Data/auditoria.json";
        private static readonly object _lock = new();

        public AuditoriaService(ILogger<AuditoriaService> logger)
        {
            _logger = logger;
            CargarDesdeJsonAsync().GetAwaiter().GetResult();
        }

        public Task RegistrarCambioAsync(AuditoriaRegistro registro)
        {
            lock (_lock)
            {
                registro.ID = _nextID++;
                registro.FechaHora = DateTime.Now;
                _registros.Add(registro);
                _logger.LogInformation("Se registró auditoría: {@Registro}", registro);
            }
            // Guardamos de forma asíncrona
            return GuardarEnJsonAsync();
        }

        public Task<IEnumerable<AuditoriaRegistro>> GetAllRegistrosAsync()
        {
            lock (_lock)
            {
                return Task.FromResult<IEnumerable<AuditoriaRegistro>>(_registros.ToList());
            }
        }

        public Task<AuditoriaRegistro?> GetRegistroByIDAsync(int id)
        {
            lock (_lock)
            {
                var reg = _registros.FirstOrDefault(r => r.ID == id);
                return Task.FromResult<AuditoriaRegistro?>(reg);
            }
        }

        public Task ForceSaveAsync()
        {
            return GuardarEnJsonAsync();
        }

        private async Task CargarDesdeJsonAsync()
        {
            lock (_lock)
            {
                try
                {
                    _registros = JsonFileHelper.LoadFromJsonFile<List<AuditoriaRegistro>>(_jsonFilePath);
                    if (_registros == null)
                    {
                        _registros = new List<AuditoriaRegistro>();
                    }
                    if (_registros.Any())
                    {
                        _nextID = _registros.Max(r => r.ID) + 1;
                    }
                    _logger.LogInformation("AuditoriaService: cargados {Count} registros de {File}", _registros.Count, _jsonFilePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al cargar auditoría desde JSON");
                    _registros = new List<AuditoriaRegistro>();
                    _nextID = 1;
                }
            }
            await Task.CompletedTask;
        }

        private async Task GuardarEnJsonAsync()
        {
            lock (_lock)
            {
                try
                {
                    JsonFileHelper.SaveToJsonFile(_jsonFilePath, _registros);
                    _logger.LogInformation("AuditoriaService: guardados {Count} registros en {File}", _registros.Count, _jsonFilePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al guardar auditoría en JSON");
                }
            }
            await Task.CompletedTask;
        }
    }
}
