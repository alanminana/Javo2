// IServices/IDevolucionGarantiaService.cs
using Javo2.Models;
using Javo2.ViewModels.Operaciones.DevolucionGarantia;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Javo2.IServices
{
    public interface IDevolucionGarantiaService
    {
        Task<IEnumerable<DevolucionGarantia>> GetAllAsync();
        Task<DevolucionGarantia> GetByIDAsync(int id);
        Task<IEnumerable<DevolucionGarantia>> GetByVentaIDAsync(int ventaID);
        Task<IEnumerable<DevolucionGarantia>> GetByEstadoAsync(EstadoCaso estado);

        Task<int> CreateAsync(DevolucionGarantia devolucion);
        Task UpdateAsync(DevolucionGarantia devolucion);
        Task DeleteAsync(int id);

        // Métodos específicos para cada flujo
        Task ProcesarDevolucionAsync(int devolucionID);
        Task ProcesarCambioAsync(int devolucionID);
        Task EnviarGarantiaAsync(int devolucionID, string destinatario, string trackingNumber);
        Task CompletarGarantiaAsync(int devolucionID, bool exitoso, string resultado);

        // Para actualizar stock
        Task ActualizarStockDevolucionAsync(int devolucionID);
    }
}