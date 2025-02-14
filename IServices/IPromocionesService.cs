// File: IServices/IPromocionesService.cs
using Javo2.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Javo2.IServices
{
    public interface IPromocionesService
    {
        Task<IEnumerable<Promocion>> GetPromocionesAsync();
        Task<Promocion?> GetPromocionByIDAsync(int id);
        Task CreatePromocionAsync(Promocion promocion);
        Task UpdatePromocionAsync(Promocion promocion);
        Task DeletePromocionAsync(int id);
        Task<IEnumerable<Promocion>> GetPromocionesAplicablesAsync(Producto producto);
    }
}
