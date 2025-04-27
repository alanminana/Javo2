// IServices/Authentication/IRolService.cs
using Javo2.Models.Authentication;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Javo2.IServices.Authentication
{
    public interface IRolService
    {
        Task<IEnumerable<Rol>> GetAllRolesAsync();
        Task<Rol> GetRolByIDAsync(int id);
        Task<Rol> GetRolByNombreAsync(string nombre);
        Task<int> CreateRolAsync(Rol rol);
        Task<bool> UpdateRolAsync(Rol rol);
        Task<bool> DeleteRolAsync(int id);

        // Métodos para gestionar permisos asociados a roles
        Task<IEnumerable<Permiso>> GetPermisosByRolIDAsync(int rolID);
        Task<bool> AsignarPermisoAsync(int rolID, int permisoID);
        Task<bool> QuitarPermisoAsync(int rolID, int permisoID);

        // Métodos para verificar si un rol tiene un permiso específico
        Task<bool> TienePermisoAsync(int rolID, string codigoPermiso);
    }
}