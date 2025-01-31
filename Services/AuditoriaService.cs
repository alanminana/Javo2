// Services/AuditoriaService.cs
using Javo2.IServices;
using Javo2.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Javo2.Helpers; // Para usar JsonFileHelper

namespace Javo2.Services
{
    public class AuditoriaService : IAuditoriaService
    {
        private readonly ILogger<AuditoriaService> _logger;

        // Lista en memoria de registros de auditoría
        private static List<AuditoriaRegistro> _registros = new();
        
        // Counter para asignar ID autoincrementable
        private static int _nextID = 1;

        // Ruta donde guardamos los registros de auditoría en JSON
        private readonly string _jsonFilePath = "Data/auditoria.json";

        // Lock para concurrencia
        private static readonly object _lock = new();

        public AuditoriaService(ILogger<AuditoriaService> logger)
        {
            _logger = logger;

            // Al instanciar el servicio, cargamos lo que haya en el archivo JSON
            CargarDesdeJson();
        }

        public Task RegistrarCambioAsync(AuditoriaRegistro registro)
        {
            lock (_lock)
            {
                registro.ID = _nextID++;
                registro.FechaHora = DateTime.Now;

                _registros.Add(registro);
                _logger.LogInformation("Se registró auditoría: {@Registro}", registro);

                // Guardamos inmediatamente en JSON (opcional)
                GuardarEnJson();
            }
            return Task.CompletedTask;
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
            // Permite forzar manualmente el guardado
            lock (_lock)
            {
                GuardarEnJson();
            }
            return Task.CompletedTask;
        }

        // =====================
        // Métodos internos
        // =====================
        private void CargarDesdeJson()
        {
            lock (_lock)
            {
                try
                {
                    _registros = JsonFileHelper.LoadFromJsonFile<List<AuditoriaRegistro>>(_jsonFilePath);
                    // Si la lista viene vacía, _registros se mantiene new() o con data
                    if (_registros.Count > 0)
                    {
                        // Ajustamos _nextID al valor más alto actual + 1
                        _nextID = _registros.Max(r => r.ID) + 1;
                    }
                    _logger.LogInformation("AuditoriaService: cargados {Count} registros de {File}",
                        _registros.Count, _jsonFilePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al cargar auditoría desde JSON");
                    _registros = new List<AuditoriaRegistro>();
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
                    JsonFileHelper.SaveToJsonFile(_jsonFilePath, _registros);
                    _logger.LogInformation("AuditoriaService: guardados {Count} registros en {File}",
                        _registros.Count, _jsonFilePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al guardar auditoría en JSON");
                }
            }
        }
    }
}
