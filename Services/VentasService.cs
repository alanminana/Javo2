// File: Services/VentaService.cs
using Javo2.IServices;
using Javo2.Models;
using Javo2.ViewModels.Operaciones.Ventas;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Javo2.Helpers;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Javo2.Services
{
    public class VentaService : IVentaService
    {
        private readonly ILogger<VentaService> _logger;
        private readonly IAuditoriaService _auditoriaService;

        private static List<Venta> _ventas = new List<Venta>();
        private static int _nextVentaID = 1;
        private readonly string _jsonFilePath = "Data/ventas.json";
        private static readonly object _lock = new object();

        public VentaService(ILogger<VentaService> logger, IAuditoriaService auditoriaService)
        {
            _logger = logger;
            _auditoriaService = auditoriaService;
            // Cargar ventas de forma asíncrona utilizando el método asíncrono del helper
            CargarDesdeJsonAsync().GetAwaiter().GetResult();
        }

        public async Task<IEnumerable<Venta>> GetAllVentasAsync()
        {
            lock (_lock)
            {
                return _ventas.ToList();
            }
        }

        public Task<Venta?> GetVentaByIDAsync(int id)
        {
            lock (_lock)
            {
                var venta = _ventas.FirstOrDefault(v => v.VentaID == id);
                return Task.FromResult<Venta?>(venta);
            }
        }

        public async Task CreateVentaAsync(Venta venta)
        {
            lock (_lock)
            {
                venta.VentaID = _nextVentaID++;
                _ventas.Add(venta);
            }
            await GuardarEnJsonAsync();
            _logger.LogInformation("Venta creada: {@Venta}", venta);
        }

        public async Task UpdateVentaAsync(Venta venta)
        {
            lock (_lock)
            {
                var existing = _ventas.FirstOrDefault(v => v.VentaID == venta.VentaID);
                if (existing != null)
                {
                    // Actualiza campos relevantes
                    existing.FechaVenta = venta.FechaVenta;
                    existing.NumeroFactura = venta.NumeroFactura;
                    existing.NombreCliente = venta.NombreCliente;
                    existing.TelefonoCliente = venta.TelefonoCliente;
                    existing.DomicilioCliente = venta.DomicilioCliente;
                    existing.LocalidadCliente = venta.LocalidadCliente;
                    existing.CelularCliente = venta.CelularCliente;
                    existing.LimiteCreditoCliente = venta.LimiteCreditoCliente;
                    existing.SaldoCliente = venta.SaldoCliente;
                    existing.SaldoDisponibleCliente = venta.SaldoDisponibleCliente;
                    existing.FormaPagoID = venta.FormaPagoID;
                    existing.BancoID = venta.BancoID;
                    existing.TipoTarjeta = venta.TipoTarjeta;
                    existing.Cuotas = venta.Cuotas;
                    existing.EntidadElectronica = venta.EntidadElectronica;
                    existing.PlanFinanciamiento = venta.PlanFinanciamiento;
                    existing.Observaciones = venta.Observaciones;
                    existing.Condiciones = venta.Condiciones;
                    existing.Credito = venta.Credito;
                    existing.ProductosPresupuesto = venta.ProductosPresupuesto;
                    existing.PrecioTotal = venta.PrecioTotal;
                    existing.TotalProductos = venta.TotalProductos;
                    existing.AdelantoDinero = venta.AdelantoDinero;
                    existing.DineroContado = venta.DineroContado;
                    existing.MontoCheque = venta.MontoCheque;
                    existing.NumeroCheque = venta.NumeroCheque;
                    existing.Estado = venta.Estado;
                }
            }
            await GuardarEnJsonAsync();
            _logger.LogInformation("Venta ID={ID} actualizada", venta.VentaID);
        }

        public async Task DeleteVentaAsync(int id)
        {
            lock (_lock)
            {
                var existing = _ventas.FirstOrDefault(v => v.VentaID == id);
                if (existing != null)
                {
                    _ventas.Remove(existing);
                }
            }
            await GuardarEnJsonAsync();
            _logger.LogInformation("Venta ID={ID} eliminada", id);
        }

        public Task<IEnumerable<Venta>> GetVentasByEstadoAsync(EstadoVenta estado)
        {
            lock (_lock)
            {
                var result = _ventas.Where(v => v.Estado == estado);
                return Task.FromResult(result);
            }
        }

        public Task<IEnumerable<Venta>> GetVentasByFechaAsync(DateTime? fechaInicio, DateTime? fechaFin)
        {
            lock (_lock)
            {
                var result = _ventas.AsEnumerable();
                if (fechaInicio.HasValue)
                    result = result.Where(v => v.FechaVenta >= fechaInicio.Value);
                if (fechaFin.HasValue)
                    result = result.Where(v => v.FechaVenta <= fechaFin.Value);
                return Task.FromResult(result);
            }
        }

        public Task<IEnumerable<Venta>> GetVentasByClienteIDAsync(int clienteID)
        {
            lock (_lock)
            {
                var result = _ventas.Where(v => v.DniCliente == clienteID);
                return Task.FromResult(result);
            }
        }

        public Task<IEnumerable<Venta>> GetVentasFilteredAsync(VentaFilterDto filterDto)
        {
            lock (_lock)
            {
                var query = _ventas.AsEnumerable();
                if (!string.IsNullOrEmpty(filterDto.NombreCliente))
                    query = query.Where(v => v.NombreCliente.Contains(filterDto.NombreCliente, StringComparison.OrdinalIgnoreCase));
                if (filterDto.FechaInicio.HasValue)
                    query = query.Where(v => v.FechaVenta >= filterDto.FechaInicio.Value);
                if (filterDto.FechaFin.HasValue)
                    query = query.Where(v => v.FechaVenta <= filterDto.FechaFin.Value);
                if (!string.IsNullOrEmpty(filterDto.NumeroFactura))
                    query = query.Where(v => v.NumeroFactura.Contains(filterDto.NumeroFactura, StringComparison.OrdinalIgnoreCase));
                return Task.FromResult(query);
            }
        }

        public Task<IEnumerable<Venta>> GetVentasAsync(VentaFilterDto filter)
        {
            return GetVentasFilteredAsync(filter);
        }

        public Task<string> GenerarNumeroFacturaAsync()
        {
            lock (_lock)
            {
                var numero = $"FAC-{DateTime.Now:yyyyMMdd}-{_nextVentaID}";
                return Task.FromResult(numero);
            }
        }

        public Task<IEnumerable<Venta>> GetVentasPendientesDeEntregaAsync()
        {
            lock (_lock)
            {
                var result = _ventas.Where(v => v.Estado == EstadoVenta.PendienteDeEntrega);
                return Task.FromResult(result);
            }
        }

        public Task<IEnumerable<FormaPago>> GetFormasPagoAsync()
        {
            var formas = new List<FormaPago>
            {
                new FormaPago { FormaPagoID = 1, Nombre = "Contado" },
                new FormaPago { FormaPagoID = 2, Nombre = "Tarjeta de Crédito" },
                new FormaPago { FormaPagoID = 3, Nombre = "Tarjeta de Débito" },
                new FormaPago { FormaPagoID = 4, Nombre = "Transferencia" },
                new FormaPago { FormaPagoID = 5, Nombre = "Pago Virtual" },
                new FormaPago { FormaPagoID = 6, Nombre = "Crédito Personal" }
            };
            return Task.FromResult<IEnumerable<FormaPago>>(formas);
        }

        public Task<IEnumerable<Banco>> GetBancosAsync()
        {
            var bancos = new List<Banco>
            {
                new Banco { BancoID = 1, Nombre = "Banco Santander" },
                new Banco { BancoID = 2, Nombre = "BBVA" },
                new Banco { BancoID = 3, Nombre = "Banco Galicia" }
            };
            return Task.FromResult<IEnumerable<Banco>>(bancos);
        }

        public Task ProcessVentaAsync(int VentaID)
        {
            lock (_lock)
            {
                var venta = _ventas.FirstOrDefault(v => v.VentaID == VentaID);
                if (venta == null)
                {
                    throw new ArgumentException("Venta no encontrada");
                }
                venta.Estado = EstadoVenta.PendienteDeEntrega;
                GuardarEnJson();
                _logger.LogInformation("Venta ID={ID} procesada -> PendienteDeEntrega", VentaID);
            }
            return Task.CompletedTask;
        }

        public Task UpdateEstadoVentaAsync(int id, EstadoVenta estado)
        {
            lock (_lock)
            {
                var venta = _ventas.FirstOrDefault(v => v.VentaID == id);
                if (venta != null)
                {
                    venta.Estado = estado;
                    GuardarEnJson();
                    _logger.LogInformation("Venta ID={ID} estado actualizado a {Estado}", id, estado);
                }
            }
            return Task.CompletedTask;
        }

        public IEnumerable<SelectListItem> GetFormasPagoSelectList()
        {
            var formas = GetFormasPagoAsync().Result;
            return formas.Select(fp => new SelectListItem { Value = fp.FormaPagoID.ToString(), Text = fp.Nombre });
        }

        public IEnumerable<SelectListItem> GetBancosSelectList()
        {
            var bancos = GetBancosAsync().Result;
            return bancos.Select(b => new SelectListItem { Value = b.BancoID.ToString(), Text = b.Nombre });
        }

        public IEnumerable<SelectListItem> GetTipoTarjetaSelectList()
        {
            var tipos = new List<string> { "Visa", "MasterCard", "Amex" };
            return tipos.Select(t => new SelectListItem { Value = t, Text = t });
        }

        public IEnumerable<SelectListItem> GetCuotasSelectList()
        {
            var cuotas = Enumerable.Range(1, 12).Select(c => c.ToString()).ToList();
            return cuotas.Select(c => new SelectListItem { Value = c, Text = $"{c} cuotas" });
        }

        public IEnumerable<SelectListItem> GetEntidadesElectronicasSelectList()
        {
            var entidades = new List<string> { "PayPal", "MercadoPago", "Stripe" };
            return entidades.Select(e => new SelectListItem { Value = e, Text = e });
        }

        public IEnumerable<SelectListItem> GetPlanesFinanciamientoSelectList()
        {
            var planes = new List<string> { "Plan A", "Plan B", "Plan C" };
            return planes.Select(p => new SelectListItem { Value = p, Text = p });
        }

        public Task<Venta?> GetVentaByIdAsync(int id)
        {
            return GetVentaByIDAsync(id);
        }

        private async Task CargarDesdeJsonAsync()
        {
            try
            {
                var data = await JsonFileHelper.LoadFromJsonFileAsync<List<Venta>>(_jsonFilePath);
                lock (_lock)
                {
                    _ventas = data ?? new List<Venta>();
                    if (_ventas.Any())
                    {
                        _nextVentaID = _ventas.Max(v => v.VentaID) + 1;
                    }
                    _logger.LogInformation("VentaService: {Count} ventas cargadas desde {File}", _ventas.Count, _jsonFilePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar ventas desde JSON");
                lock (_lock)
                {
                    _ventas = new List<Venta>();
                    _nextVentaID = 1;
                }
            }
        }

        private async Task GuardarEnJsonAsync()
        {
            List<Venta> snapshot;
            lock (_lock)
            {
                snapshot = _ventas.ToList();
            }
            try
            {
                await JsonFileHelper.SaveToJsonFileAsync(_jsonFilePath, snapshot);
                _logger.LogInformation("VentaService: guardados {Count} ventas en {File}", snapshot.Count, _jsonFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar ventas en JSON");
            }
        }

        // Método síncrono para uso en métodos que lo requieran.
        private void GuardarEnJson()
        {
            lock (_lock)
            {
                try
                {
                    JsonFileHelper.SaveToJsonFile(_jsonFilePath, _ventas);
                    _logger.LogInformation("VentaService: guardados {Count} ventas en {File}", _ventas.Count, _jsonFilePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al guardar ventas en JSON");
                }
            }
        }
    }
}
