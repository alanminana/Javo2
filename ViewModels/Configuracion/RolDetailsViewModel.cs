// ViewModels/Authentication/RolDetailsViewModel.cs
using Javo2.Models.Authentication;
using System.Collections.Generic;

namespace Javo2.ViewModels.Authentication
{
    public class RolDetailsViewModel
    {
        public Rol Rol { get; set; }
        public List<Permiso> Permisos { get; set; }
    }
}