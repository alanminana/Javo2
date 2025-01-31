using Microsoft.AspNetCore.Mvc.Rendering;
using Javo2.IServices.Common;
using Javo2.ViewModels.Operaciones.Clientes;
using Javo2.ViewModels.Operaciones.Productos;

namespace Javo2.Helpers
{
    public static class DropdownHelper
    {
        public static async Task PopulateProvinciasAsync(IDropdownService dropdownService, ClientesViewModel model)
        {
            var provincias = await dropdownService.GetProvinciasAsync();
            model.Provincias = provincias.Select(p => new SelectListItem
            {
                Value = p.ProvinciaID.ToString(),
                Text = p.Nombre
            }).ToList();
        }

        public static async Task PopulateCiudadesAsync(IDropdownService dropdownService, ClientesViewModel model)
        {
            if (model.ProvinciaID > 0)
            {
                var ciudades = await dropdownService.GetCiudadesByProvinciaIDAsync(model.ProvinciaID);
                model.Ciudades = ciudades.Select(c => new SelectListItem
                {
                    Value = c.CiudadID.ToString(),
                    Text = c.Nombre
                }).ToList();
            }
            else
            {
                model.Ciudades = new List<SelectListItem>();
            }
        }

        public static async Task PopulateProductDropdownsAsync(IDropdownService dropdownService, ProductosViewModel model)
        {
            model.Rubros = await dropdownService.GetRubrosAsync();
            model.Marcas = await dropdownService.GetMarcasAsync();
            model.SubRubros = await dropdownService.GetSubRubrosAsync(model.SelectedRubroID);
        }
    }
}
