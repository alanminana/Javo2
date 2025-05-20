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
        // CRUD básico
        Task<IEnumerable<Venta>> GetAllVentasAsync();
        Task<Venta?> GetVentaByIDAsync(int id);
        Task CreateVentaAsync(Venta venta);
        Task UpdateVentaAsync(Venta venta);
        Task DeleteVentaAsync(int id);

        // Consultas específicas
        Task<IEnumerable<Venta>> GetVentasByEstadoAsync(EstadoVenta estado);
        Task<IEnumerable<Venta>> GetVentasByFechaAsync(DateTime? fechaInicio, DateTime? fechaFin);
        Task<IEnumerable<Venta>> GetVentasByClienteIDAsync(int clienteID);
        Task<IEnumerable<Venta>> GetVentasFilteredAsync(VentaFilterDto filterDto);
        Task<IEnumerable<Venta>> GetVentasAsync(VentaFilterDto filter);
        Task<IEnumerable<Venta>> GetVentasPendientesDeEntregaAsync();

        // Generación de números
        Task<string> GenerarNumeroFacturaAsync();

        // Datos auxiliares
        Task<IEnumerable<FormaPago>> GetFormasPagoAsync();
        Task<IEnumerable<Banco>> GetBancosAsync();

        // Flujo de autorización y entrega
        Task AutorizarVentaAsync(int ventaID, string usuario);
        Task RechazarVentaAsync(int ventaID, string usuario);
        Task MarcarVentaComoEntregadaAsync(int ventaID, string usuario);

        // Procesamiento
        Task ProcessVentaAsync(int ventaID);
        Task UpdateEstadoVentaAsync(int id, EstadoVenta estado);

        // SelectLists
        IEnumerable<SelectListItem> GetFormasPagoSelectList();
        IEnumerable<SelectListItem> GetBancosSelectList();
        IEnumerable<SelectListItem> GetTipoTarjetaSelectList();
        IEnumerable<SelectListItem> GetCuotasSelectList();
        IEnumerable<SelectListItem> GetEntidadesElectronicasSelectList();
        IEnumerable<SelectListItem> GetPlanesFinanciamientoSelectList();

        // Alias
        // Añadir a IServices/IVentaService.cs
        Task<Venta> CrearVentaCreditoAsync(Venta venta, int numeroCuotas, DateTime primerVencimiento);
        Task<bool> ProcesarPagoCuotaAsync(int ventaId, int cuotaId, decimal monto, string formaPago, string referencia);
    }
}