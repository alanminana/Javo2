// Ruta: Services/VentaService.cs
using Javo2.IServices;
using Javo2.Models;
using Javo2.ViewModels.Operaciones.Ventas;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Javo2.Services
{
    public class VentaService : IVentaService
    {
        private static readonly List<Venta> _ventas = new();  // Lista estática en memoria

        private readonly ILogger<VentaService> _logger;
        private readonly IStockService _stockService;

        // Datos simulados para combos
        private static readonly List<FormaPago> _formasPago = new()
        {
            new FormaPago { FormaPagoID = 1, Nombre = "Efectivo" },
            new FormaPago { FormaPagoID = 2, Nombre = "Tarjeta de crédito" },
            new FormaPago { FormaPagoID = 3, Nombre = "Tarjeta de débito" },
            new FormaPago { FormaPagoID = 4, Nombre = "Transferencia" },
            new FormaPago { FormaPagoID = 5, Nombre = "Pago Virtual" },
            new FormaPago { FormaPagoID = 6, Nombre = "Crédito Personal" }
        };

        private static readonly List<Banco> _bancos = new()
        {
            new Banco { BancoID = 1, Nombre = "Banco Santander" },
            new Banco { BancoID = 2, Nombre = "BBVA" },
            new Banco { BancoID = 3, Nombre = "Banco Galicia" }
        };

        private static readonly List<SelectListItem> _tipoTarjetaOptions = new()
        {
            new SelectListItem { Value = "Visa", Text = "Visa" },
            new SelectListItem { Value = "MasterCard", Text = "MasterCard" },
            new SelectListItem { Value = "Amex", Text = "American Express" }
        };

        private static readonly List<SelectListItem> _cuotasOptions = Enumerable.Range(1, 12)
            .Select(i => new SelectListItem { Value = i.ToString(), Text = $"{i} Cuotas" })
            .ToList();

        private static readonly List<SelectListItem> _entidadesElectronicas = new()
        {
            new SelectListItem { Value = "MercadoPago", Text = "MercadoPago" },
            new SelectListItem { Value = "PayPal", Text = "PayPal" },
            new SelectListItem { Value = "Stripe", Text = "Stripe" }
        };

        private static readonly List<SelectListItem> _planesFinanciamiento = new()
        {
            new SelectListItem { Value = "Plan A", Text = "Plan A" },
            new SelectListItem { Value = "Plan B", Text = "Plan B" },
            new SelectListItem { Value = "Plan C", Text = "Plan C" }
        };

        public VentaService(ILogger<VentaService> logger, IStockService stockService)
        {
            _logger = logger;
            _stockService = stockService;
        }

        // ===================== Lógica extra =====================
        public async Task<IEnumerable<Venta>> GetVentasAsync(VentaFilterDto filter)
        {
            _logger.LogInformation("GetVentasAsync con filtro: {@Filter}", filter);
            return await GetVentasFilteredAsync(filter);
        }

        public async Task<string> GenerarNumeroFacturaAsync()
        {
            var correlativo = _ventas.Count + 1;
            var numero = $"FAC-{correlativo:00000}";
            _logger.LogInformation("GenerarNumeroFactura => {Numero}", numero);
            return await Task.FromResult(numero);
        }

        public async Task<IEnumerable<Venta>> GetVentasPendientesDeEntregaAsync()
        {
            return await GetVentasByEstadoAsync(EstadoVenta.PendienteDeEntrega);
        }

        public async Task<IEnumerable<FormaPago>> GetFormasPagoAsync()
        {
            return await Task.FromResult(_formasPago.AsEnumerable());
        }

        public async Task<IEnumerable<Banco>> GetBancosAsync()
        {
            return await Task.FromResult(_bancos.AsEnumerable());
        }

        // ===================== CRUD =====================
        public async Task<IEnumerable<Venta>> GetAllVentasAsync()
        {
            return await Task.FromResult(_ventas);
        }

        public async Task<Venta?> GetVentaByIDAsync(int id)
        {
            var venta = _ventas.FirstOrDefault(v => v.VentaID == id);
            return await Task.FromResult(venta);
        }

        public async Task CreateVentaAsync(Venta venta)
        {
            _logger.LogInformation("[VentaService] CreateVentaAsync llamado con Venta={Venta}", venta);

            venta.VentaID = _ventas.Any() ? _ventas.Max(v => v.VentaID) + 1 : 1;
            _ventas.Add(venta);
            await Task.CompletedTask;
        }

        public async Task UpdateVentaAsync(Venta venta)
        {
            var existing = _ventas.FirstOrDefault(v => v.VentaID == venta.VentaID);
            if (existing != null)
            {
                // Actualizamos campos relevantes
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
                existing.EntidadElectronica = venta.EntidadElectronica;
                existing.PlanFinanciamiento = venta.PlanFinanciamiento;
                existing.AdelantoDinero = venta.AdelantoDinero;
                existing.DineroContado = venta.DineroContado;
                existing.MontoCheque = venta.MontoCheque;
                existing.NumeroCheque = venta.NumeroCheque;
                existing.Observaciones = venta.Observaciones;
                existing.Condiciones = venta.Condiciones;
                existing.Credito = venta.Credito;
                existing.Estado = venta.Estado;
                existing.EstadoEntrega = venta.EstadoEntrega;
                existing.ProductosPresupuesto = venta.ProductosPresupuesto;
                existing.PrecioTotal = venta.PrecioTotal;
                existing.FechaModificacion = DateTime.UtcNow;
            }
            await Task.CompletedTask;
        }

        public async Task DeleteVentaAsync(int id)
        {
            var venta = _ventas.FirstOrDefault(v => v.VentaID == id);
            if (venta != null) _ventas.Remove(venta);
            await Task.CompletedTask;
        }

        // ===================== FILTRADO =====================
        public async Task<IEnumerable<Venta>> GetVentasByEstadoAsync(EstadoVenta estado)
        {
            var result = _ventas.Where(v => v.Estado == estado);
            return await Task.FromResult(result);
        }

        public async Task<IEnumerable<Venta>> GetVentasByFechaAsync(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var result = _ventas.AsEnumerable();
            if (fechaInicio.HasValue)
                result = result.Where(v => v.FechaVenta.Date >= fechaInicio.Value.Date);
            if (fechaFin.HasValue)
                result = result.Where(v => v.FechaVenta.Date <= fechaFin.Value.Date);

            return await Task.FromResult(result);
        }

        public async Task<IEnumerable<Venta>> GetVentasByClienteIDAsync(int clienteID)
        {
            var result = _ventas.Where(v => v.ClienteID == clienteID);
            return await Task.FromResult(result);
        }

        public async Task<IEnumerable<Venta>> GetVentasFilteredAsync(VentaFilterDto filterDto)
        {
            var result = _ventas.AsEnumerable();
            if (!string.IsNullOrEmpty(filterDto.NombreCliente))
            {
                result = result.Where(v => v.NombreCliente.Contains(filterDto.NombreCliente, StringComparison.OrdinalIgnoreCase));
            }

            if (filterDto.FechaInicio.HasValue)
            {
                result = result.Where(v => v.FechaVenta.Date >= filterDto.FechaInicio.Value.Date);
            }

            if (filterDto.FechaFin.HasValue)
            {
                result = result.Where(v => v.FechaVenta.Date <= filterDto.FechaFin.Value.Date);
            }

            if (!string.IsNullOrEmpty(filterDto.NumeroFactura))
            {
                result = result.Where(v => v.NumeroFactura.Contains(filterDto.NumeroFactura, StringComparison.OrdinalIgnoreCase));
            }

            return await Task.FromResult(result);
        }

        // ===================== PROCESAMIENTO =====================
        public async Task ProcessVentaAsync(int VentaID)
        {
            var venta = _ventas.FirstOrDefault(v => v.VentaID == VentaID);
            if (venta == null) return;

            // Descontar stock, etc. (depende de tu lógica con _stockService)
            foreach (var item in venta.ProductosPresupuesto)
            {
                var movimiento = new MovimientoStock
                {
                    ProductoID = item.ProductoID,
                    Cantidad = item.Cantidad,
                    TipoMovimiento = "Salida",
                    Motivo = $"Venta #{venta.VentaID}"
                };
                await _stockService.RegistrarMovimientoAsync(movimiento);
            }

            venta.Estado = EstadoVenta.Completada;
            await Task.CompletedTask;
        }

        public async Task UpdateEstadoVentaAsync(int id, EstadoVenta estado)
        {
            var venta = _ventas.FirstOrDefault(v => v.VentaID == id);
            if (venta != null)
            {
                venta.Estado = estado;
            }
            await Task.CompletedTask;
        }

        // ===================== COMBOS (Opcional) =====================
        public IEnumerable<SelectListItem> GetFormasPagoSelectList()
        {
            return _formasPago.Select(fp => new SelectListItem
            {
                Value = fp.FormaPagoID.ToString(),
                Text = fp.Nombre
            });
        }

        public IEnumerable<SelectListItem> GetBancosSelectList()
        {
            return _bancos.Select(b => new SelectListItem
            {
                Value = b.BancoID.ToString(),
                Text = b.Nombre
            });
        }

        public IEnumerable<SelectListItem> GetTipoTarjetaSelectList()
        {
            return _tipoTarjetaOptions;
        }

        public IEnumerable<SelectListItem> GetCuotasSelectList()
        {
            return _cuotasOptions;
        }

        public IEnumerable<SelectListItem> GetEntidadesElectronicasSelectList()
        {
            return _entidadesElectronicas;
        }

        public IEnumerable<SelectListItem> GetPlanesFinanciamientoSelectList()
        {
            return _planesFinanciamiento;
        }
        public async Task<Venta?> GetVentaByIdAsync(int id)
        {
            return await GetVentaByIDAsync(id);
        }
    }
}