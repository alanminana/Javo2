// Archivo: Services/VentaService.cs
using AutoMapper;
using Javo2.IServices;
using Javo2.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Javo2.Services
{
    public class VentaService : IVentaService
    {
        private static readonly List<Venta> _ventas = new()
        {
            new Venta
            {
                VentaID = 1,
                FechaVenta = DateTime.Now,
                NumeroFactura = "F001",
                NombreCliente = "Cliente A",
                PrecioTotal = 1000,
                Estado = EstadoVenta.PendienteDeAutorizacion
            }
        };

        private readonly IMapper _mapper;
        private readonly ILogger<VentaService> _logger;

        public VentaService(IMapper mapper, ILogger<VentaService> logger)
        {
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<Venta>> GetAllVentasAsync()
        {
            _logger.LogInformation("GetAllVentasAsync called");
            return await Task.FromResult(_ventas);
        }

        public async Task<Venta?> GetVentaByIdAsync(int id)
        {
            _logger.LogInformation("GetVentaByIdAsync called with ID: {Id}", id);
            var venta = _ventas.FirstOrDefault(v => v.VentaID == id);
            return await Task.FromResult(venta);
        }

        public async Task CreateVentaAsync(Venta venta)
        {
            _logger.LogInformation("CreateVentaAsync called with Venta: {Venta}", venta);
            venta.VentaID = _ventas.Any() ? _ventas.Max(v => v.VentaID) + 1 : 1;
            _ventas.Add(venta);
            _logger.LogInformation("Venta created with ID: {Id}", venta.VentaID);
            await Task.CompletedTask;
        }

        public async Task UpdateVentaAsync(Venta venta)
        {
            _logger.LogInformation("UpdateVentaAsync called with Venta: {Venta}", venta);
            var existingVenta = _ventas.FirstOrDefault(v => v.VentaID == venta.VentaID);
            if (existingVenta != null)
            {
                _mapper.Map(venta, existingVenta);
                _logger.LogInformation("Venta updated with ID: {Id}", venta.VentaID);
            }
            else
            {
                _logger.LogWarning("Venta with ID: {Id} not found", venta.VentaID);
            }
            await Task.CompletedTask;
        }

        public async Task DeleteVentaAsync(int id)
        {
            _logger.LogInformation("DeleteVentaAsync called with ID: {Id}", id);
            var venta = _ventas.FirstOrDefault(v => v.VentaID == id);
            if (venta != null)
            {
                _ventas.Remove(venta);
                _logger.LogInformation("Venta deleted with ID: {Id}", id);
            }
            else
            {
                _logger.LogWarning("Venta with ID: {Id} not found", id);
            }
            await Task.CompletedTask;
        }

        public async Task<IEnumerable<Venta>> GetVentasByEstadoAsync(EstadoVenta estado)
        {
            _logger.LogInformation("GetVentasByEstadoAsync called with estado: {Estado}", estado);
            var ventas = _ventas.Where(v => v.Estado == estado);
            return await Task.FromResult(ventas);
        }

        public async Task UpdateEstadoVentaAsync(int id, EstadoVenta estado)
        {
            _logger.LogInformation("UpdateEstadoVentaAsync called with ID: {Id}, Estado: {Estado}", id, estado);
            var venta = _ventas.FirstOrDefault(v => v.VentaID == id);
            if (venta != null)
            {
                venta.Estado = estado;
                _logger.LogInformation("Estado of Venta ID: {Id} updated to: {Estado}", id, estado);
            }
            else
            {
                _logger.LogWarning("Venta with ID: {Id} not found", id);
            }
            await Task.CompletedTask;
        }

        public async Task<IEnumerable<Venta>> GetVentasByFechaAsync(DateTime? fechaInicio, DateTime? fechaFin)
        {
            _logger.LogInformation("GetVentasByFechaAsync called with fechaInicio: {FechaInicio}, fechaFin: {FechaFin}", fechaInicio, fechaFin);
            var ventas = _ventas.AsEnumerable();

            if (fechaInicio.HasValue)
            {
                ventas = ventas.Where(v => v.FechaVenta >= fechaInicio.Value);
            }

            if (fechaFin.HasValue)
            {
                ventas = ventas.Where(v => v.FechaVenta <= fechaFin.Value);
            }

            return await Task.FromResult(ventas);
        }
    }
}
