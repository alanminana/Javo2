

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
        Task<bool> CreateRolAsync(Rol rol);
        Task<bool> UpdateRolAsync(Rol rol);
        Task<bool> DeleteRolAsync(int id);
        Task<bool> AsignarPermisoAsync(int rolID, int permisoID);
        Task<bool> QuitarPermisoAsync(int rolID, int permisoID);
        Task<IEnumerable<Permiso>> GetPermisosByRolIDAsync(int rolID);
        Task<IEnumerable<Usuario>> GetUsuariosByRolIDAsync(int rolID);
    }
}