using AutoMapper;
using javo2.ViewModels.Operaciones.Ventas;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace javo2.Services
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

        public async Task<IEnumerable<VentasViewModel>> GetAllVentasAsync()
        {
            _logger.LogInformation("GetAllVentasAsync called");
            return await Task.FromResult(_ventas);
        }

        public async Task<IEnumerable<VentasViewModel>> GetVentasByEstadoAsync(EstadoVenta estado)
        {
            _logger.LogInformation("GetVentasByEstadoAsync called with estado: {Estado}", estado);
            return await Task.FromResult(_ventas.Where(v => v.Estado == estado));
        }

        public async Task<VentasViewModel?> GetVentaByIdAsync(int id)
        {
            _logger.LogInformation("GetVentaByIdAsync called with ID: {Id}", id);
            return await Task.FromResult(_ventas.FirstOrDefault(v => v.VentaID == id));
        }

        public async Task CreateVentaAsync(VentasViewModel ventaViewModel)
        {
            _logger.LogInformation("CreateVentaAsync called with Venta: {Venta}", ventaViewModel);
            ventaViewModel.VentaID = _ventas.Count > 0 ? _ventas.Max(v => v.VentaID) + 1 : 1;
            _ventas.Add(ventaViewModel);
            _logger.LogInformation("Venta created with ID: {Id}", ventaViewModel.VentaID);
            await Task.CompletedTask;
        }

        public async Task UpdateVentaAsync(VentasViewModel ventaViewModel)
        {
            _logger.LogInformation("UpdateVentaAsync called with Venta: {Venta}", ventaViewModel);
            var venta = _ventas.FirstOrDefault(v => v.VentaID == ventaViewModel.VentaID);
            if (venta != null)
            {
                _mapper.Map(ventaViewModel, venta);
                _logger.LogInformation("Venta updated with ID: {Id}", ventaViewModel.VentaID);
            }
            await Task.CompletedTask;
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
            await Task.CompletedTask;
        }
    }
}
