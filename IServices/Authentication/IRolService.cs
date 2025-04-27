// IServices/Authentication/IRolService.cs
using Javo2.Models.Authentication;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Javo2.IServices.Authentication
{
    public interface IRolService
    {
        /// <summary>
        /// Obtiene todos los roles del sistema
        /// </summary>
        Task<IEnumerable<Rol>> GetAllRolesAsync();

        /// <summary>
        /// Obtiene un rol por su ID
        /// </summary>
        Task<Rol> GetRolByIDAsync(int id);

        /// <summary>
        /// Obtiene un rol por su nombre
        /// </summary>
        Task<Rol> GetRolByNombreAsync(string nombre);

        /// <summary>
        /// Crea un nuevo rol en el sistema
        /// </summary>
        Task<bool> CreateRolAsync(Rol rol);

        /// <summary>
        /// Actualiza un rol existente
        /// </summary>
        Task<bool> UpdateRolAsync(Rol rol);

        /// <summary>
        /// Elimina un rol del sistema
        /// </summary>
        Task<bool> DeleteRolAsync(int id);

        /// <summary>
        /// Asigna un permiso a un rol
        /// </summary>
        Task<bool> AsignarPermisoAsync(int rolID, int permisoID);

        /// <summary>
        /// Quita un permiso de un rol
        /// </summary>
        Task<bool> QuitarPermisoAsync(int rolID, int permisoID);

        /// <summary>
        /// Obtiene todos los permisos asignados a un rol
        /// </summary>
        Task<IEnumerable<Permiso>> GetPermisosByRolIDAsync(int rolID);
    }
}