// Archivo: ViewModels/Shared/ILocationViewModel.cs
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Javo2.ViewModels.Shared
{
    public interface ILocationViewModel
    {
        IEnumerable<SelectListItem> Provincias { get; set; }
        IEnumerable<SelectListItem> Ciudades { get; set; }
    }
}