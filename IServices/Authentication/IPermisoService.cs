// 6. IServices/Authentication/IPermisoService.cs
// Interfaz más completa para el servicio de permisos
using Javo2.Models.Authentication;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Javo2.IServices.Authentication
{
    public interface IPermisoService
    {
        Task<IEnumerable<Permiso>> GetAllPermisosAsync();
        Task<Permiso> GetPermisoByIDAsync(int id);
        Task<Permiso> GetPermisoByCodigo(string codigo);
        Task<bool> CreatePermisoAsync(Permiso permiso);
        Task<bool> UpdatePermisoAsync(Permiso permiso);
        Task<bool> DeletePermisoAsync(int id);
        Task<IEnumerable<Permiso>> GetPermisosByModuloAsync(string modulo);
        Task<bool> ToggleEstadoAsync(int id);
    }
}