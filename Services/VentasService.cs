// Ruta: Services/VentaService.cs
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

        private static List<Venta> _ventas = new();
        private static int _nextVentaID = 1;
        private readonly string _jsonFilePath = "Data/ventas.json";
        private static readonly object _lock = new();

        public VentaService(ILogger<VentaService> logger, IAuditoriaService auditoriaService)
        {
            _logger = logger;
            _auditoriaService = auditoriaService;
            CargarDesdeJson();
        }

        // =============== Métodos CRUD ===============
        public Task<IEnumerable<Venta>> GetAllVentasAsync()
        {
            lock (_lock)
            {
                return Task.FromResult(_ventas.AsEnumerable());
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

        public Task CreateVentaAsync(Venta venta)
        {
            lock (_lock)
            {
                venta.VentaID = _nextVentaID++;
                _ventas.Add(venta);
                GuardarEnJson();
                _logger.LogInformation("Venta creada: {@Venta}", venta);
            }
            return Task.CompletedTask;
        }

        public Task UpdateVentaAsync(Venta venta)
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

                    GuardarEnJson();
                    _logger.LogInformation("Venta ID={ID} actualizada", venta.VentaID);
                }
            }
            return Task.CompletedTask;
        }

        public Task DeleteVentaAsync(int id)
        {
            lock (_lock)
            {
                var existing = _ventas.FirstOrDefault(v => v.VentaID == id);
                if (existing != null)
                {
                    _ventas.Remove(existing);
                    GuardarEnJson();
                    _logger.LogInformation("Venta ID={ID} eliminada", id);
                }
            }
            return Task.CompletedTask;
        }

        // =============== Métodos de Filtrado ===============
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
                // asumiendo que almacenarías un ClienteID en la venta
                var result = _ventas.Where(v => v.ClienteID == clienteID);
                return Task.FromResult(result);
            }
        }

        public Task<IEnumerable<Venta>> GetVentasFilteredAsync(VentaFilterDto filterDto)
        {
            // depende de cómo quieras filtrar...
            lock (_lock)
            {
                var query = _ventas.AsEnumerable();
                if (!string.IsNullOrEmpty(filterDto.NombreCliente))
                {
                    query = query.Where(v => v.NombreCliente.Contains(filterDto.NombreCliente, StringComparison.OrdinalIgnoreCase));
                }
                if (filterDto.FechaInicio.HasValue)
                {
                    query = query.Where(v => v.FechaVenta >= filterDto.FechaInicio.Value);
                }
                if (filterDto.FechaFin.HasValue)
                {
                    query = query.Where(v => v.FechaVenta <= filterDto.FechaFin.Value);
                }
                if (!string.IsNullOrEmpty(filterDto.NumeroFactura))
                {
                    query = query.Where(v => v.NumeroFactura.Contains(filterDto.NumeroFactura, StringComparison.OrdinalIgnoreCase));
                }
                return Task.FromResult(query);
            }
        }

        // =============== Método específico: GetVentasAsync ===============
        public Task<IEnumerable<Venta>> GetVentasAsync(VentaFilterDto filter)
        {
            // Reutiliza la lógica de GetVentasFilteredAsync o similar
            return GetVentasFilteredAsync(filter);
        }

        // =============== Generar Número de Factura ===============
        public Task<string> GenerarNumeroFacturaAsync()
        {
            lock (_lock)
            {
                var numero = $"FAC-{DateTime.Now:yyyyMMdd}-{_nextVentaID}";
                return Task.FromResult(numero);
            }
        }

        // =============== Ventas Pendientes de Entrega ===============
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

        // =============== Procesamiento de la Venta ===============
        public Task ProcessVentaAsync(int VentaID)
        {
            lock (_lock)
            {
                var venta = _ventas.FirstOrDefault(v => v.VentaID == VentaID);
                if (venta == null)
                {
                    throw new ArgumentException("Venta no encontrada");
                }

                // Lógica de verificación de stock, etc.
                // Cambiar estado a PendienteDeEntrega, etc.
                // Actualizar y guardar
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

        // =============== Listas para combos (si se usan) ===============
        // Estos métodos devuelven opciones de SelectListItem, si tu MVC lo requiere.
        public IEnumerable<SelectListItem> GetFormasPagoSelectList()
        {
            var formas = GetFormasPagoAsync().Result; // Ojo con .Result en prod
            return formas.Select(fp => new SelectListItem { Value = fp.FormaPagoID.ToString(), Text = fp.Nombre });
        }

        public IEnumerable<SelectListItem> GetBancosSelectList()
        {
            var bancos = GetBancosAsync().Result;
            return bancos.Select(b => new SelectListItem { Value = b.BancoID.ToString(), Text = b.Nombre });
        }

        public IEnumerable<SelectListItem> GetTipoTarjetaSelectList()
        {
            // Ejemplo de tipos de tarjeta
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

        // Extra: Método duplicado en la interfaz, lo mantengo por compatibilidad
        public Task<Venta?> GetVentaByIdAsync(int id)
        {
            // Reutilizamos
            return GetVentaByIDAsync(id);
        }

        // =============== Métodos internos ===============
        private void CargarDesdeJson()
        {
            lock (_lock)
            {
                try
                {
                    var data = JsonFileHelper.LoadFromJsonFile<List<Venta>>(_jsonFilePath);
                    _ventas = data ?? new List<Venta>();
                    if (_ventas.Any())
                    {
                        _nextVentaID = _ventas.Max(v => v.VentaID) + 1;
                    }
                    _logger.LogInformation("VentaService: {Count} ventas cargadas desde {File}", _ventas.Count, _jsonFilePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al cargar ventas desde JSON");
                    _ventas = new List<Venta>();
                    _nextVentaID = 1;
                }
            }
        }

        private void GuardarEnJson()
        {
            lock (_lock)
            {
                try
                {
                    JsonFileHelper.SaveToJsonFile(_jsonFilePath, _ventas);
                    _logger.LogInformation("VentaService: {Count} ventas guardadas en {File}", _ventas.Count, _jsonFilePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al guardar ventas en JSON");
                }
            }
        }
    }
}
