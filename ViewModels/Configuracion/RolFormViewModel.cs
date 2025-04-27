// ViewModels/Authentication/RolFormViewModel.cs
using Javo2.Models.Authentication;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Javo2.ViewModels.Authentication
{
    public class RolFormViewModel
    {
        public Rol Rol { get; set; }
        public IEnumerable<SelectListItem> PermisosDisponibles { get; set; }
        public List<int> PermisosSeleccionados { get; set; }
        public Dictionary<string, List<Permiso>> GruposPermisos { get; set; }
        public bool EsEdicion { get; set; }
    }
}