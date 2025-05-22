// Services/CotizacionService.cs
using Javo2.IServices;
using Javo2.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Javo2.Helpers;

namespace Javo2.Services.Operations
{
    public class CotizacionService : ICotizacionService
    {
        private readonly ILogger<CotizacionService> _logger;
        private static List<Cotizacion> _cotizaciones = new();
        private static int _nextId = 1;
        private readonly string _jsonFilePath = "Data/cotizaciones.json";
        private static readonly object _lock = new();

        public CotizacionService(ILogger<CotizacionService> logger)
        {
            _logger = logger;
            CargarDesdeJson();
        }

        public Task<IEnumerable<Cotizacion>> GetAllCotizacionesAsync()
        {
            lock (_lock)
            {
                return Task.FromResult(_cotizaciones.AsEnumerable());
            }
        }

        public Task<Cotizacion?> GetCotizacionByIDAsync(int id)
        {
            lock (_lock)
            {
                return Task.FromResult(_cotizaciones.FirstOrDefault(c => c.CotizacionID == id));
            }
        }

        public Task<string> GenerarNumeroCotizacionAsync()
        {
            lock (_lock)
            {
                var numero = $"COT-{DateTime.Now:yyyyMMdd}-{_nextId}";
                return Task.FromResult(numero);
            }
        }

        public Task CreateCotizacionAsync(Cotizacion cotizacion)
        {
            lock (_lock)
            {
                // Asignar ID y número
                cotizacion.CotizacionID = _nextId++;
                cotizacion.NumeroCotizacion = $"COT-{DateTime.Now:yyyyMMdd}-{cotizacion.CotizacionID}";
                cotizacion.EstadoCotizacion = "Borrador";

                // Calcular fechas
                cotizacion.FechaCotizacion = DateTime.Now;
                cotizacion.FechaVencimiento = cotizacion.FechaCotizacion
                                               .AddDays(cotizacion.DiasVigencia);

                // Recalcular totales
                cotizacion.TotalProductos = cotizacion.ProductosPresupuesto?
                    .Sum(p => p.Cantidad) ?? 0;
                cotizacion.PrecioTotal = cotizacion.ProductosPresupuesto?
                    .Sum(p => p.PrecioTotal) ?? 0m;

                _cotizaciones.Add(cotizacion);
                GuardarEnJson();

                _logger.LogInformation("Cotización creada: ID={ID}, Vence={Vence}",
                    cotizacion.CotizacionID, cotizacion.FechaVencimiento.ToShortDateString());
            }
            return Task.CompletedTask;
        }

        public Task UpdateCotizacionAsync(Cotizacion cotizacion)
        {
            lock (_lock)
            {
                var existing = _cotizaciones
                    .FirstOrDefault(c => c.CotizacionID == cotizacion.CotizacionID);
                if (existing != null)
                {
                    // Actualizar campos básicos
                    existing.FechaCotizacion = cotizacion.FechaCotizacion;
                    existing.DiasVigencia = cotizacion.DiasVigencia;
                    existing.FechaVencimiento = cotizacion.FechaCotizacion
                                                    .AddDays(cotizacion.DiasVigencia);
                    existing.NombreCliente = cotizacion.NombreCliente;
                    existing.DniCliente = cotizacion.DniCliente;
                    existing.TelefonoCliente = cotizacion.TelefonoCliente;
                    existing.EmailCliente = cotizacion.EmailCliente;
                    existing.Observaciones = cotizacion.Observaciones;
                    existing.EstadoCotizacion = cotizacion.EstadoCotizacion;

                    // Actualizar lista de productos y totales
                    existing.ProductosPresupuesto = cotizacion.ProductosPresupuesto;
                    existing.TotalProductos = cotizacion.ProductosPresupuesto?
                        .Sum(p => p.Cantidad) ?? 0;
                    existing.PrecioTotal = cotizacion.ProductosPresupuesto?
                        .Sum(p => p.PrecioTotal) ?? 0m;

                    GuardarEnJson();
                    _logger.LogInformation("Cotización actualizada ID={ID}", cotizacion.CotizacionID);
                }
            }
            return Task.CompletedTask;
        }

        public Task DeleteCotizacionAsync(int id)
        {
            lock (_lock)
            {
                var existing = _cotizaciones.FirstOrDefault(c => c.CotizacionID == id);
                if (existing != null)
                {
                    _cotizaciones.Remove(existing);
                    GuardarEnJson();
                    _logger.LogInformation("Cotización eliminada ID={ID}", id);
                }
            }
            return Task.CompletedTask;
        }

        private void CargarDesdeJson()
        {
            try
            {
                if (!File.Exists(_jsonFilePath))
                    File.WriteAllText(_jsonFilePath, "[]");

                var data = JsonFileHelper
                    .LoadFromJsonFileAsync<List<Cotizacion>>(_jsonFilePath)
                    .GetAwaiter().GetResult();

                lock (_lock)
                {
                    _cotizaciones = data ?? new List<Cotizacion>();
                    _nextId = _cotizaciones.Any()
                        ? _cotizaciones.Max(c => c.CotizacionID) + 1
                        : 1;
                }

                _logger.LogInformation("Cargadas {Count} cotizaciones desde JSON", _cotizaciones.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cargando cotizaciones desde JSON");
                _cotizaciones = new();
            }
        }

        private void GuardarEnJson()
        {
            try
            {
                JsonFileHelper.SaveToJsonFile(_jsonFilePath, _cotizaciones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error guardando cotizaciones en JSON");
            }
        }
    }
}
