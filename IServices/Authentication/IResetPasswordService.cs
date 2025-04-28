using System.Threading.Tasks;

namespace Javo2.IServices.Authentication
{
    public interface IResetPasswordService
    {
        /// <summary>
        /// Genera un token de restablecimiento de contraseña para un usuario
        /// </summary>
        /// <param name="usuarioID">ID del usuario</param>
        /// <returns>Token generado</returns>
        Task<string> GenerateResetTokenAsync(int usuarioID);

        /// <summary>
        /// Valida un token de restablecimiento de contraseña
        /// </summary>
        /// <param name="usuarioID">ID del usuario</param>
        /// <param name="token">Token a validar</param>
        /// <returns>True si el token es válido</returns>
        Task<bool> ValidateResetTokenAsync(int usuarioID, string token);

        /// <summary>
        /// Restablece la contraseña de un usuario utilizando un token
        /// </summary>
        /// <param name="usuarioID">ID del usuario</param>
        /// <param name="token">Token de restablecimiento</param>
        /// <param name="nuevaContraseña">Nueva contraseña</param>
        /// <returns>True si el restablecimiento fue exitoso</returns>
        Task<bool> ResetPasswordAsync(int usuarioID, string token, string nuevaContraseña);

        /// <summary>
        /// Verifica si un usuario tiene tokens activos
        /// </summary>
        /// <param name="usuarioID">ID del usuario</param>
        /// <returns>True si el usuario tiene al menos un token activo</returns>
        Task<bool> HasActiveTokenAsync(int usuarioID);

        /// <summary>
        /// Invalida todos los tokens de un usuario
        /// </summary>
        /// <param name="usuarioID">ID del usuario</param>
        Task InvalidateTokensAsync(int usuarioID);
    }
}