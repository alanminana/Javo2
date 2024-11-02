// Archivo: IServices/IVentaService.cs
using Javo2.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Javo2.IServices
{
    public interface IVentaService
    {
        Task<IEnumerable<Venta>> GetAllVentasAsync();
        Task<Venta?> GetVentaByIdAsync(int id);
        Task CreateVentaAsync(Venta venta);
        Task UpdateVentaAsync(Venta venta);
        Task DeleteVentaAsync(int id);
        Task<IEnumerable<Venta>> GetVentasByEstadoAsync(EstadoVenta estado);
        Task UpdateEstadoVentaAsync(int id, EstadoVenta estado);
        Task<IEnumerable<Venta>> GetVentasByFechaAsync(DateTime? fechaInicio, DateTime? fechaFin);
    }
}
