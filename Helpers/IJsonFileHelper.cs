using System.Threading.Tasks;

namespace Javo2.Helpers
{
    public interface IJsonFileHelper
    {
        Task<string> ReadAsync(string filePath);
        Task WriteAsync(string filePath, string content);
    }
}