// Archivo: IServices/Common/IDropdownService.cs
using Javo2.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Javo2.IServices.Common
{
    public interface IDropdownService
    {
        // Métodos para Provincias y Ciudades
        Task<List<Provincia>> GetProvinciasAsync();
        Task<List<Ciudad>> GetCiudadesByProvinciaIdAsync(int provinciaId);

        // Métodos para Rubros, SubRubros y Marcas
        Task<List<SelectListItem>> GetRubrosAsync();
        Task<List<SelectListItem>> GetSubRubrosAsync(int rubroId);
        Task<List<SelectListItem>> GetMarcasAsync();

        // Método para Productos
        Task<List<SelectListItem>> GetProductosAsync();
    }
}
