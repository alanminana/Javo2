using AutoMapper;
using Javo2.ViewModels.Operaciones.Ventas;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Javo2.Services
{
    public class VentaService : IVentaService
    {
        private static readonly List<VentasViewModel> _ventas = new()
        {
            new() { VentaID = 1, FechaVenta = DateTime.Now, NumeroFactura = "F001", NombreCliente = "Cliente A", PrecioTotal = 1000, Estado = EstadoVenta.PendienteDeAutorizacion }
        };

        private readonly IMapper _mapper;
        private readonly ILogger<VentaService> _logger;

        public VentaService(IMapper mapper, ILogger<VentaService> logger)
        {
            _mapper = mapper;
            _logger = logger;
        }

        // Método para obtener todas las ventas
        public async Task<IEnumerable<VentasViewModel>> GetAllVentasAsync()
        {
            _logger.LogInformation("GetAllVentasAsync called");
            return await Task.FromResult(_ventas);
        }

        // Método para obtener ventas por estado
        public async Task<IEnumerable<VentasViewModel>> GetVentasByEstadoAsync(EstadoVenta estado)
        {
            _logger.LogInformation("GetVentasByEstadoAsync called with estado: {Estado}", estado);
            return await Task.FromResult(_ventas.Where(v => v.Estado == estado));
        }

        // Método para obtener una venta por ID
        public async Task<VentasViewModel?> GetVentaByIdAsync(int id)
        {
            _logger.LogInformation("GetVentaByIdAsync called with ID: {Id}", id);
            return await Task.FromResult(_ventas.FirstOrDefault(v => v.VentaID == id));
        }

        // Método para obtener ventas por rango de fechas
        public async Task<IEnumerable<VentasViewModel>> GetVentasByFechaAsync(DateTime? fechaInicio, DateTime? fechaFin)
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

            return await Task.FromResult(ventas.ToList());
        }

        // Método para crear una nueva venta
        public async Task CreateVentaAsync(VentasViewModel ventaViewModel)
        {
            _logger.LogInformation("CreateVentaAsync called with Venta: {Venta}", ventaViewModel);
            ventaViewModel.VentaID = _ventas.Count > 0 ? _ventas.Max(v => v.VentaID) + 1 : 1;
            _ventas.Add(ventaViewModel);
            _logger.LogInformation("Venta created with ID: {Id}", ventaViewModel.VentaID);
            await Task.CompletedTask;
        }

        // Método para actualizar una venta existente
        public async Task UpdateVentaAsync(VentasViewModel ventaViewModel)
        {
            _logger.LogInformation("UpdateVentaAsync called with Venta: {Venta}", ventaViewModel);
            var venta = _ventas.FirstOrDefault(v => v.VentaID == ventaViewModel.VentaID);
            if (venta != null)
            {
                _mapper.Map(ventaViewModel, venta);
                _logger.LogInformation("Venta updated with ID: {Id}", ventaViewModel.VentaID);
            }
            else
            {
                _logger.LogWarning("Venta with ID: {Id} not found", ventaViewModel.VentaID);
            }
            await Task.CompletedTask;
        }

        // Método para actualizar el estado de una venta
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

        // Método para eliminar una venta
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
    }
}
