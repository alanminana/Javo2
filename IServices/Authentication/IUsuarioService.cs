using Javo2.Models.Authentication;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Javo2.IServices.Authentication
{
    public interface IUsuarioService
    {
        Task<IEnumerable<Usuario>> GetAllUsuariosAsync();
        Task<Usuario> GetUsuarioByIDAsync(int id);
        Task<Usuario> GetUsuarioByNombreUsuarioAsync(string nombreUsuario);
        Task<Usuario> GetUsuarioByEmailAsync(string email);
        Task<bool> CreateUsuarioAsync(Usuario usuario, string contraseña);
        Task<bool> UpdateUsuarioAsync(Usuario usuario, string contraseñaNueva = null);
        Task<bool> DeleteUsuarioAsync(int id);
        Task<bool> CambiarContraseñaAsync(int usuarioID, string contraseñaActual, string contraseñaNueva);
        Task<bool> AsignarRolAsync(int usuarioID, int rolID);
        Task<bool> QuitarRolAsync(int usuarioID, int rolID);
        Task<bool> AutenticarAsync(string nombreUsuario, string contraseña);
        Task<IEnumerable<Permiso>> GetPermisosUsuarioAsync(int usuarioID);
        Task<bool> TienePermisoAsync(int usuarioID, string codigoPermiso);
        Task<bool> ToggleEstadoAsync(int id);
        Task<IEnumerable<Usuario>> BuscarUsuariosAsync(string termino);
        Task<IEnumerable<Usuario>> GetUsuariosByRolIDAsync(int rolID);
        Task<bool> ResetearContrasenaAsync(int usuarioID, string nuevaContraseña);
        Task<bool> IsUsernameUniqueAsync(string nombreUsuario);
        Task<bool> IsEmailUniqueAsync(string email);
        Task<IEnumerable<Usuario>> GetUsuariosActivosAsync();
        Task<IEnumerable<Usuario>> GetUsuariosRecentesAsync(int count);
        Task<IEnumerable<Usuario>> GetUltimosAccesosAsync(int count);
        Task<bool> ValidarContraseñaSeguraAsync(string contraseña);
    }
}