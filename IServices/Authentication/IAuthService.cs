using System.Threading.Tasks;

namespace Javo2.IServices.Authentication
{
    public interface IAuthService
    {
        Task<bool> ValidateUserAsync(string username, string password);
        Task<string> GenerateJwtTokenAsync(string username);
        Task<bool> RevokeTokenAsync(string username);
    }
}