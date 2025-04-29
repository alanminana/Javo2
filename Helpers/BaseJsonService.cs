using Javo2.Helpers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Javo2.Services.Base
{
    public abstract class PermissionTagHelper<T> where T : class
    {
        protected readonly ILogger _logger;
        protected List<T> _items = new();
        protected readonly object _lock = new();
        protected readonly string _jsonFilePath;

        protected PermissionTagHelper(ILogger logger, string jsonFilePath)
        {
            _logger = logger;
            _jsonFilePath = jsonFilePath;
            CargarDesdeJson();
        }

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

        // Versiones asíncronas
        protected async Task CargarDesdeJsonAsync()
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

        protected async Task GuardarEnJsonAsync()
        {
            try
            {
                List<T> itemsToSave;
                lock (_lock)
                {
                    itemsToSave = _items.ToList();
                }
                await JsonFileHelper.SaveToJsonFileAsync(_jsonFilePath, itemsToSave);
                _logger.LogInformation("Guardados {Count} elementos en {FilePath}", itemsToSave.Count, _jsonFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar datos en {FilePath}", _jsonFilePath);
            }
        }
    }
}