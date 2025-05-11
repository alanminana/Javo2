// IServices/ICotizacionService.cs
using Javo2.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Javo2.IServices
{
    public interface ICotizacionService
    {
        Task<IEnumerable<Cotizacion>> GetAllCotizacionesAsync();
        Task<Cotizacion?> GetCotizacionByIDAsync(int id);
        Task CreateCotizacionAsync(Cotizacion cotizacion);
        Task UpdateCotizacionAsync(Cotizacion cotizacion);
        Task DeleteCotizacionAsync(int id);
        Task<string> GenerarNumeroCotizacionAsync();
    }
}
