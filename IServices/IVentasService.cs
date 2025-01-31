// Ruta: IServices/IVentaService.cs
using Javo2.Models;
using Javo2.ViewModels.Operaciones.Ventas;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Javo2.IServices
{
    public interface IVentaService
    {
        // Métodos CRUD
        Task<IEnumerable<Venta>> GetAllVentasAsync();
        Task<Venta?> GetVentaByIDAsync(int id);
        Task CreateVentaAsync(Venta venta);
        Task UpdateVentaAsync(Venta venta);
        Task DeleteVentaAsync(int id);

        // Métodos para filtrado
        Task<IEnumerable<Venta>> GetVentasByEstadoAsync(EstadoVenta estado);
        Task<IEnumerable<Venta>> GetVentasByFechaAsync(DateTime? fechaInicio, DateTime? fechaFin);
        Task<IEnumerable<Venta>> GetVentasByClienteIDAsync(int clienteID);
        Task<IEnumerable<Venta>> GetVentasFilteredAsync(VentaFilterDto filterDto);

        // Métodos específicos
        Task<IEnumerable<Venta>> GetVentasAsync(VentaFilterDto filter);
        Task<string> GenerarNumeroFacturaAsync();
        Task<IEnumerable<Venta>> GetVentasPendientesDeEntregaAsync();
        Task<IEnumerable<FormaPago>> GetFormasPagoAsync();
        Task<IEnumerable<Banco>> GetBancosAsync();

        // Procesamiento
        Task ProcessVentaAsync(int VentaID);
        Task UpdateEstadoVentaAsync(int id, EstadoVenta estado);

        // Listas para combos (si se usan)
        IEnumerable<SelectListItem> GetFormasPagoSelectList();
        IEnumerable<SelectListItem> GetBancosSelectList();
        IEnumerable<SelectListItem> GetTipoTarjetaSelectList();
        IEnumerable<SelectListItem> GetCuotasSelectList();
        IEnumerable<SelectListItem> GetEntidadesElectronicasSelectList();
        IEnumerable<SelectListItem> GetPlanesFinanciamientoSelectList();

        Task<Venta?> GetVentaByIdAsync(int id);
    }
}
