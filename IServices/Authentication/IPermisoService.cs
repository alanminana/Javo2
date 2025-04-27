// IServices/Authentication/IPermisoService.cs
using Javo2.Models.Authentication;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Javo2.IServices.Authentication
{
    public interface IPermisoService
    {
        /// <summary>
        /// Obtiene todos los permisos del sistema
        /// </summary>
        Task<IEnumerable<Permiso>> GetAllPermisosAsync();

        /// <summary>
        /// Obtiene un permiso por su ID
        /// </summary>
        Task<Permiso> GetPermisoByIDAsync(int id);

        /// <summary>
        /// Obtiene un permiso por su código único
        /// </summary>
        Task<Permiso> GetPermisoByCodigo(string codigo);

        /// <summary>
        /// Crea un nuevo permiso en el sistema
        /// </summary>
        Task<bool> CreatePermisoAsync(Permiso permiso);

        /// <summary>
        /// Actualiza un permiso existente
        /// </summary>
        Task<bool> UpdatePermisoAsync(Permiso permiso);

        /// <summary>
        /// Elimina un permiso del sistema
        /// </summary>
        Task<bool> DeletePermisoAsync(int id);
    }
}