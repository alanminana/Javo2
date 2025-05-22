// Services/ConfiguracionService.cs
using Javo2.Helpers;
using Javo2.IServices;
using Javo2.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Javo2.Services.System
{
    public class ConfiguracionService : IConfiguracionService
    {
        private readonly ILogger<ConfiguracionService> _logger;
        private static List<ConfiguracionSistema> _configuraciones = new();
        private static int _nextId = 1;
        private static readonly object _lock = new();
        private readonly string _jsonFilePath = "Data/configuracion.json";

        public ConfiguracionService(ILogger<ConfiguracionService> logger)
        {
            _logger = logger;
            CargarDesdeJsonAsync().GetAwaiter().GetResult();

            // Inicializar configuraciones por defecto si no existen
            InitializeDefaultConfigAsync().GetAwaiter().GetResult();
        }

        private async Task CargarDesdeJsonAsync()
        {
            try
            {
                var data = await JsonFileHelper.LoadFromJsonFileAsync<List<ConfiguracionSistema>>(_jsonFilePath);
                lock (_lock)
                {
                    _configuraciones = data ?? new List<ConfiguracionSistema>();

                    if (_configuraciones.Any())
                    {
                        _nextId = _configuraciones.Max(c => c.ConfiguracionID) + 1;
                    }
                }
                _logger.LogInformation("{Count} configuraciones cargadas", _configuraciones.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar configuraciones");
                _configuraciones = new List<ConfiguracionSistema>();
            }
        }

        private async Task GuardarEnJsonAsync()
        {
            try
            {
                List<ConfiguracionSistema> configuraciones;
                lock (_lock)
                {
                    configuraciones = _configuraciones.ToList();
                }
                await JsonFileHelper.SaveToJsonFileAsync(_jsonFilePath, configuraciones);
                _logger.LogInformation("Configuraciones guardadas: {Count}", configuraciones.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar configuraciones");
            }
        }

        private async Task InitializeDefaultConfigAsync()
        {
            // Configuraciones para Productos
            await EnsureConfigExistsAsync("Productos", "PorcentajeGananciaPLista", "84",
                "Porcentaje de ganancia para calcular el Precio de Lista a partir del Precio de Costo", "decimal");

            await EnsureConfigExistsAsync("Productos", "PorcentajeGananciaPContado", "50",
                "Porcentaje de ganancia para calcular el Precio de Contado a partir del Precio de Costo", "decimal");

            // Configuraciones para Clientes
            await EnsureConfigExistsAsync("Clientes", "LimiteCreditoDefault", "10000",
                "Límite de crédito por defecto para nuevos clientes", "decimal");

            // Configuraciones para Ventas
            await EnsureConfigExistsAsync("Ventas", "DiasVencimientoFactura", "30",
                "Días para vencimiento de facturas", "int");
        }

        private async Task EnsureConfigExistsAsync(string modulo, string clave, string valorDefault, string descripcion, string tipoDato)
        {
            var config = await GetByClaveAsync(modulo, clave);
            if (config == null)
            {
                await SaveValorAsync(modulo, clave, valorDefault, descripcion, tipoDato);
                _logger.LogInformation("Configuración creada: {Modulo}.{Clave}={Valor}",
                    modulo, clave, valorDefault);
            }
        }

        public async Task<IEnumerable<ConfiguracionSistema>> GetAllAsync()
        {
            lock (_lock)
            {
                return _configuraciones.ToList();
            }
        }

        public async Task<IEnumerable<ConfiguracionSistema>> GetByModuloAsync(string modulo)
        {
            lock (_lock)
            {
                return _configuraciones.Where(c => c.Modulo == modulo).ToList();
            }
        }

        public async Task<ConfiguracionSistema> GetByClaveAsync(string modulo, string clave)
        {
            lock (_lock)
            {
                return _configuraciones.FirstOrDefault(c => c.Modulo == modulo && c.Clave == clave);
            }
        }

        public async Task<T> GetValorAsync<T>(string modulo, string clave, T valorPorDefecto)
        {
            var config = await GetByClaveAsync(modulo, clave);
            if (config == null)
                return valorPorDefecto;

            try
            {
                if (typeof(T) == typeof(int))
                    return (T)(object)int.Parse(config.Valor);

                if (typeof(T) == typeof(decimal))
                    return (T)(object)decimal.Parse(config.Valor);

                if (typeof(T) == typeof(bool))
                    return (T)(object)bool.Parse(config.Valor);

                return (T)(object)config.Valor;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al convertir configuración {Modulo}.{Clave}", modulo, clave);
                return valorPorDefecto;
            }
        }

        public async Task SaveAsync(ConfiguracionSistema configuracion)
        {
            lock (_lock)
            {
                var existing = _configuraciones.FirstOrDefault(c =>
                    c.Modulo == configuracion.Modulo && c.Clave == configuracion.Clave);

                if (existing != null)
                {
                    existing.Valor = configuracion.Valor;
                    existing.Descripcion = configuracion.Descripcion;
                    existing.TipoDato = configuracion.TipoDato;
                }
                else
                {
                    configuracion.ConfiguracionID = _nextId++;
                    _configuraciones.Add(configuracion);
                }
            }

            await GuardarEnJsonAsync();
        }

        public async Task SaveValorAsync(string modulo, string clave, string valor,
            string descripcion = "", string tipoDato = "string")
        {
            var config = new ConfiguracionSistema
            {
                Modulo = modulo,
                Clave = clave,
                Valor = valor,
                Descripcion = descripcion,
                TipoDato = tipoDato
            };

            await SaveAsync(config);
        }
    }
}