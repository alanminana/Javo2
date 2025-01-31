// Archivo: IServices/IProvinciaService.cs
using Javo2.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Javo2.IServices
{
    public interface IProvinciaService
    {
        Task<IEnumerable<Provincia>> GetAllProvinciasAsync();
        Task<IEnumerable<Ciudad>> GetCiudadesByProvinciaIDAsync(int provinciaID);
    }
}
