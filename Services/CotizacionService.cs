// File: Services/CotizacionService.cs
using Javo2.IServices;
using Javo2.Models;
using Microsoft.Extensions.Logging;
using Javo2.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Javo2.Services
{
    public class CotizacionService : ICotizacionService
    {
        private readonly ILogger<CotizacionService> _logger;
        private static List<Venta> _cotizaciones = new List<Venta>();
        private static int _nextCotizacionID = 1;
        private readonly string _jsonFilePath = "Data/cotizaciones.json";
        private static readonly object _lock = new object();

        public CotizacionService(ILogger<CotizacionService> logger)
        {
            _logger = logger;
            CargarDesdeJson();
        }

        private void CargarDesdeJson()
        {
            lock (_lock)
            {
                try
                {
                    var data = JsonFileHelper.LoadFromJsonFile<List<Venta>>(_jsonFilePath);
                    _cotizaciones = data ?? new List<Venta>();
                    if (_cotizaciones.Any())
                    {
                        _nextCotizacionID = _cotizaciones.Max(c => c.VentaID) + 1;
                    }
                    _logger.LogInformation("CotizacionService: {Count} cotizaciones cargadas desde {File}", _cotizaciones.Count, _jsonFilePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al cargar cotizaciones desde JSON");
                    _cotizaciones = new List<Venta>();
                    _nextCotizacionID = 1;
                }
            }
        }

        private void GuardarEnJson()
        {
            lock (_lock)
            {
                try
                {
                    JsonFileHelper.SaveToJsonFile(_jsonFilePath, _cotizaciones);
                    _logger.LogInformation("CotizacionService: {Count} cotizaciones guardadas en {File}", _cotizaciones.Count, _jsonFilePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al guardar cotizaciones en JSON");
                }
            }
        }

        public Task<IEnumerable<Venta>> GetAllCotizacionesAsync()
        {
            lock (_lock)
            {
                return Task.FromResult(_cotizaciones.AsEnumerable());
            }
        }

        public Task<Venta?> GetCotizacionByIDAsync(int id)
        {
            lock (_lock)
            {
                var cotizacion = _cotizaciones.FirstOrDefault(c => c.VentaID == id);
                return Task.FromResult(cotizacion);
            }
        }

        public Task CreateCotizacionAsync(Venta cotizacion)
        {
            lock (_lock)
            {
                cotizacion.VentaID = _nextCotizacionID++;
                cotizacion.NumeroFactura = $"COT-{DateTime.Now:yyyyMMdd}-{cotizacion.VentaID}";
                // Se asigna estado "Borrador" para cotización; se podría definir un estado específico si se extiende el enum.
                cotizacion.Estado = EstadoVenta.Borrador;
                _cotizaciones.Add(cotizacion);
                GuardarEnJson();
                _logger.LogInformation("Cotización creada: ID={ID}, Cliente={Cliente}", cotizacion.VentaID, cotizacion.NombreCliente);
            }
            return Task.CompletedTask;
        }

        public Task UpdateCotizacionAsync(Venta cotizacion)
        {
            lock (_lock)
            {
                var existing = _cotizaciones.FirstOrDefault(c => c.VentaID == cotizacion.VentaID);
                if (existing != null)
                {
                    existing.FechaVenta = cotizacion.FechaVenta;
                    existing.NumeroFactura = cotizacion.NumeroFactura;
                    existing.NombreCliente = cotizacion.NombreCliente;
                    existing.TelefonoCliente = cotizacion.TelefonoCliente;
                    existing.DomicilioCliente = cotizacion.DomicilioCliente;
                    existing.LocalidadCliente = cotizacion.LocalidadCliente;
                    existing.CelularCliente = cotizacion.CelularCliente;
                    existing.LimiteCreditoCliente = cotizacion.LimiteCreditoCliente;
                    existing.SaldoCliente = cotizacion.SaldoCliente;
                    existing.SaldoDisponibleCliente = cotizacion.SaldoDisponibleCliente;
                    existing.FormaPagoID = cotizacion.FormaPagoID;
                    existing.BancoID = cotizacion.BancoID;
                    existing.TipoTarjeta = cotizacion.TipoTarjeta;
                    existing.Cuotas = cotizacion.Cuotas;
                    existing.EntidadElectronica = cotizacion.EntidadElectronica;
                    existing.PlanFinanciamiento = cotizacion.PlanFinanciamiento;
                    existing.Observaciones = cotizacion.Observaciones;
                    existing.Condiciones = cotizacion.Condiciones;
                    existing.Credito = cotizacion.Credito;
                    existing.ProductosPresupuesto = cotizacion.ProductosPresupuesto;
                    existing.PrecioTotal = cotizacion.PrecioTotal;
                    existing.TotalProductos = cotizacion.TotalProductos;
                    GuardarEnJson();
                    _logger.LogInformation("Cotización actualizada: ID={ID}", cotizacion.VentaID);
                }
            }
            return Task.CompletedTask;
        }

        public Task DeleteCotizacionAsync(int id)
        {
            lock (_lock)
            {
                var existing = _cotizaciones.FirstOrDefault(c => c.VentaID == id);
                if (existing != null)
                {
                    _cotizaciones.Remove(existing);
                    GuardarEnJson();
                    _logger.LogInformation("Cotización ID={ID} eliminada", id);
                }
            }
            return Task.CompletedTask;
        }

        public Task<string> GenerarNumeroCotizacionAsync()
        {
            lock (_lock)
            {
                var numero = $"COT-{DateTime.Now:yyyyMMdd}-{_nextCotizacionID}";
                return Task.FromResult(numero);
            }
        }
    }
}
