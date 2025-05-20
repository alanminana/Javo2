// IServices/ICreditoService.cs
using Javo2.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Javo2.IServices
{
    public interface ICreditoService
    {
        // Configuración
        Task<ConfiguracionCredito> GetConfiguracionVigenteAsync();
        Task<ConfiguracionCredito> SaveConfiguracionAsync(ConfiguracionCredito config);

        // Criterios
        Task<CriteriosCalificacionCredito> GetCriterioByScoreAsync(string score);
        Task<IEnumerable<CriteriosCalificacionCredito>> GetAllCriteriosAsync();
        Task<CriteriosCalificacionCredito> SaveCriterioAsync(CriteriosCalificacionCredito criterio);

        // Cálculos
        Task<decimal> CalcularRecargoAsync(string scoreCredito, decimal montoTotal, int numeroCuotas);
        Task<decimal> CalcularInteresDeAtrasoAsync(int diasAtraso, decimal montoCuota);

        // Generación de cuotas
        Task<List<Cuota>> GenerarCuotasAsync(Venta venta, int numeroCuotas, DateTime primerVencimiento);

        // Pagos
        Task<bool> RegistrarPagoCuotaAsync(int cuotaId, decimal monto, string formaPago, string referencia, string usuario);
        Task<bool> AsignarCuotaAGaranteAsync(int cuotaId, string usuario);

        // Consultas
        Task<List<Cuota>> GetCuotasPendientesByClienteAsync(int clienteId);
        Task<List<Cuota>> GetCuotasVencidasAsync(int diasAtraso = 0);
        Task<bool> ClienteCalificaPorCreditoAsync(int clienteId, decimal montoTotal, int numeroCuotas);
    }
}