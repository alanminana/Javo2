// Archivo: IServices/Common/IDropdownService.cs
using Javo2.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Javo2.IServices.Common
{
    public interface IDropdownService
    {
        // Métodos existentes
        Task<List<SelectListItem>> GetRubrosAsync();
        Task<List<SelectListItem>> GetSubRubrosAsync(int rubroId);
        Task<List<SelectListItem>> GetMarcasAsync();
        Task<List<Provincia>> GetProvinciasAsync();
        Task<List<Ciudad>> GetCiudadesByProvinciaIDAsync(int provinciaID);
        Task<List<SelectListItem>> GetProductosAsync();

        // Nuevos métodos para manejo de proveedores y compras
        Task<List<SelectListItem>> GetFormasPagoAsync();
        Task<List<SelectListItem>> GetBancosAsync();
        Task<List<SelectListItem>> GetTiposTarjetaAsync();
        Task<List<SelectListItem>> GetCuotasAsync();
        Task<List<SelectListItem>> GetEntidadesElectronicasAsync();
        Task<List<SelectListItem>> GetProveedoresAsync();
    }
}