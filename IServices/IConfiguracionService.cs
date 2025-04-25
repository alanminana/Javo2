// IServices/IConfiguracionService.cs
using Javo2.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Javo2.IServices
{
    public interface IConfiguracionService
    {
        Task<IEnumerable<ConfiguracionSistema>> GetAllAsync();
        Task<IEnumerable<ConfiguracionSistema>> GetByModuloAsync(string modulo);
        Task<ConfiguracionSistema> GetByClaveAsync(string modulo, string clave);
        Task<T> GetValorAsync<T>(string modulo, string clave, T valorPorDefecto);
        Task SaveAsync(ConfiguracionSistema configuracion);
        Task SaveValorAsync(string modulo, string clave, string valor, string descripcion = "", string tipoDato = "string");
    }
}