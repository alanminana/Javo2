



// ViewModels/Authentication/UsuarioDetailsViewModel.cs
using Javo2.Models.Authentication;
using System.Collections.Generic;

namespace Javo2.ViewModels.Authentication
{
    public class UsuarioDetailsViewModel
    {
        public Usuario Usuario { get; set; }
        public List<Rol> Roles { get; set; }
    }
}