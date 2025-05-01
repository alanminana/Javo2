using Javo2.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Javo2.IServices
{
    public interface IGaranteService
    {
        Task<IEnumerable<Garante>> GetAllGarantesAsync();
        Task<Garante> GetGaranteByIdAsync(int id);
        Task<Garante> GetGaranteByDniAsync(int dni);
        Task<Garante> CreateGaranteAsync(Garante garante);
        Task UpdateGaranteAsync(Garante garante);
        Task DeleteGaranteAsync(int id);
        Task<IEnumerable<Garante>> GetGarantesByClienteIdAsync(int clienteId);
    }
}