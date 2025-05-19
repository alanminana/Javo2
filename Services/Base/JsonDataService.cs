// Archivo: Services/Base/JsonDataService.cs
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Javo2.Helpers;

namespace Javo2.Services.Base
{
    public abstract class JsonDataService<T> where T : class
    {
        protected readonly ILogger _logger;
        protected List<T> _items = new();
        protected readonly object _lock = new();
        protected readonly string _jsonFilePath;
        protected readonly string _backupDirectory = "Data/Backups";
        protected static int _nextID = 1;

        protected JsonDataService(ILogger logger, string jsonFilePath)
        {
            _logger = logger;
            _jsonFilePath = jsonFilePath;
            Directory.CreateDirectory(_backupDirectory);
            InitializeAsync().GetAwaiter().GetResult();
        }

        private async Task InitializeAsync() => await CargarDesdeJsonAsync();

        protected virtual async Task CargarDesdeJsonAsync()
        {
            try
            {
                var data = await JsonFileHelper.LoadFromJsonFileAsync<List<T>>(_jsonFilePath);
                lock (_lock)
                {
                    _items = data ?? new List<T>();
                }
                _logger.LogInformation("Cargados {Count} elementos desde {FilePath}", _items.Count, _jsonFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar datos desde {FilePath}", _jsonFilePath);
                _items = new List<T>();
            }
        }

        protected virtual async Task GuardarEnJsonAsync()
        {
            List<T> snapshot;
            lock (_lock)
            {
                snapshot = _items.ToList();
            }

            try
            {
                // Backup automático
                if (File.Exists(_jsonFilePath))
                {
                    var backupPath = Path.Combine(_backupDirectory,
                        $"{Path.GetFileNameWithoutExtension(_jsonFilePath)}_backup_{DateTime.Now:yyyyMMddHHmmss}.json");
                    File.Copy(_jsonFilePath, backupPath, true);

                    // Mantener solo los últimos 10 backups
                    var backupFiles = Directory.GetFiles(_backupDirectory,
                        $"{Path.GetFileNameWithoutExtension(_jsonFilePath)}_backup_*.json")
                        .OrderByDescending(f => File.GetCreationTime(f))
                        .Skip(10);

                    foreach (var file in backupFiles)
                    {
                        try { File.Delete(file); } catch { }
                    }
                }

                await JsonFileHelper.SaveToJsonFileAsync(_jsonFilePath, snapshot);
                _logger.LogInformation("Guardados {Count} elementos en {FilePath}", snapshot.Count, _jsonFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar datos en {FilePath}", _jsonFilePath);
                throw;
            }
        }

        // Métodos importados desde BaseJsonService
        protected void CargarDesdeJson()
        {
            try
            {
                var data = JsonFileHelper.LoadFromJsonFile<List<T>>(_jsonFilePath);
                lock (_lock)
                {
                    _items = data ?? new List<T>();
                }
                _logger.LogInformation("Cargados {Count} elementos desde {FilePath}", _items.Count, _jsonFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar datos desde {FilePath}", _jsonFilePath);
                _items = new List<T>();
            }
        }

        protected void GuardarEnJson()
        {
            try
            {
                lock (_lock)
                {
                    JsonFileHelper.SaveToJsonFile(_jsonFilePath, _items);
                }
                _logger.LogInformation("Guardados {Count} elementos en {FilePath}", _items.Count, _jsonFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar datos en {FilePath}", _jsonFilePath);
            }
        }
    }
}