// IServices/Authentication/IResetPasswordService.cs
using System.Threading.Tasks;

namespace Javo2.IServices.Authentication
{
    public interface IResetPasswordService
    {
        Task<string> GenerateResetTokenAsync(int usuarioID);
        Task<bool> ValidateResetTokenAsync(int usuarioID, string token);
        Task<bool> ResetPasswordAsync(int usuarioID, string token, string nuevaContraseña);
        Task<bool> HasActiveTokenAsync(int usuarioID);
        Task InvalidateTokensAsync(int usuarioID);
    }
}