// IServices/Authentication/IResetPasswordService.cs
using System;
using System.Threading.Tasks;

namespace Javo2.IServices.Authentication
{
    public interface IResetPasswordService
    {
        /// <summary>
        /// Genera un token de restablecimiento de contraseña para el usuario especificado
        /// </summary>
        /// <param name="usuarioID">ID del usuario</param>
        /// <returns>El token generado</returns>
        Task<string> GenerateResetTokenAsync(int usuarioID);

        /// <summary>
        /// Valida si un token de restablecimiento es válido para el usuario especificado
        /// </summary>
        /// <param name="usuarioID">ID del usuario</param>
        /// <param name="token">Token de restablecimiento</param>
        /// <returns>True si el token es válido, false en caso contrario</returns>
        Task<bool> ValidateResetTokenAsync(int usuarioID, string token);

        /// <summary>
        /// Restablece la contraseña del usuario utilizando el token proporcionado
        /// </summary>
        /// <param name="usuarioID">ID del usuario</param>
        /// <param name="token">Token de restablecimiento</param>
        /// <param name="nuevaContraseña">Nueva contraseña</param>
        /// <returns>True si la contraseña se restableció correctamente, false en caso contrario</returns>
        Task<bool> ResetPasswordAsync(int usuarioID, string token, string nuevaContraseña);

        /// <summary>
        /// Verifica si un usuario tiene un token de restablecimiento activo
        /// </summary>
        /// <param name="usuarioID">ID del usuario</param>
        /// <returns>True si el usuario tiene un token activo, false en caso contrario</returns>
        Task<bool> HasActiveTokenAsync(int usuarioID);

        /// <summary>
        /// Invalida todos los tokens de restablecimiento para el usuario especificado
        /// </summary>
        /// <param name="usuarioID">ID del usuario</param>
        Task InvalidateTokensAsync(int usuarioID);
    }
}