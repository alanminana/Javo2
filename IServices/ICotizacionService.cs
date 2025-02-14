// File: IServices/ICotizacionService.cs
using Javo2.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Javo2.IServices
{
    public interface ICotizacionService
    {
        Task<IEnumerable<Venta>> GetAllCotizacionesAsync();
        Task<Venta?> GetCotizacionByIDAsync(int id);
        Task CreateCotizacionAsync(Venta cotizacion);
        Task UpdateCotizacionAsync(Venta cotizacion);
        Task DeleteCotizacionAsync(int id);
        Task<string> GenerarNumeroCotizacionAsync();
    }
}