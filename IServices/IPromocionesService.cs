using Javo2.Models;

namespace Javo2.IServices
{
    public interface IPromocionesService
    {
        Task<IEnumerable<Promocion>> GetPromocionesAsync();
        Task<Promocion?> GetPromocionByIDAsync(int id);
        Task CreatePromocionAsync(Promocion promocion);
        Task UpdatePromocionAsync(Promocion promocion);
        Task DeletePromocionAsync(int id);

        // Método para obtener todas las promociones activas aplicables a un producto dado
        Task<IEnumerable<Promocion>> GetPromocionesAplicablesAsync(Producto producto);
    }
}
