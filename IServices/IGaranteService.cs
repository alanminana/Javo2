// Archivo: IServices/IGaranteService.cs
using Javo2.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Javo2.IServices
{
    public interface IGaranteService
    {
        Task<Garante?> GetGaranteByIdAsync(int id);
        Task<Garante?> GetGaranteByDniAsync(int dni);
        Task<Garante> CreateGaranteAsync(Garante garante);
        Task<bool> UpdateGaranteAsync(Garante garante);
        Task<bool> DeleteGaranteAsync(int id);
        Task<IEnumerable<Garante>> GetAllGarantesAsync();
    }
}