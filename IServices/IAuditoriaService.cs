// IServices/IAuditoriaService.cs
using Javo2.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Javo2.IServices
{
    public interface IAuditoriaService
    {
        Task RegistrarCambioAsync(AuditoriaRegistro registro);
        Task<IEnumerable<AuditoriaRegistro>> GetAllRegistrosAsync();
        Task<AuditoriaRegistro?> GetRegistroByIDAsync(int id);

        // Nuevo si quieres forzar guardado manual
        Task ForceSaveAsync();
    }
}
