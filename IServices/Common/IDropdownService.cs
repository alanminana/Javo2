// Archivo: IServices/Common/IDropdownService.cs
using Javo2.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Javo2.IServices.Common
{
    public interface IDropdownService
    {
        Task<List<SelectListItem>> GetRubrosAsync();
        Task<List<SelectListItem>> GetSubRubrosAsync(int rubroId);
        Task<List<SelectListItem>> GetMarcasAsync();
        Task<List<Provincia>> GetProvinciasAsync();
        Task<List<Ciudad>> GetCiudadesByProvinciaIDAsync(int provinciaID);
        Task<List<SelectListItem>> GetProductosAsync();
    }
}
