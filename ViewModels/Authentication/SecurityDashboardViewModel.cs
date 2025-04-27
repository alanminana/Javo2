// 2. ViewModels/Authentication/SecurityDashboardViewModel.cs
using System;
using System.Collections.Generic;

namespace Javo2.ViewModels.Authentication
{
    public class SecurityDashboardViewModel
    {
        public int TotalUsuarios { get; set; }
        public int UsuariosActivos { get; set; }
        public int TotalRoles { get; set; }
        public int TotalPermisos { get; set; }
        public List<UsuarioSimpleViewModel> UltimosUsuariosRegistrados { get; set; }
        public List<UsuarioSimpleViewModel> UltimosAccesos { get; set; }
    }

    public class UsuarioSimpleViewModel
    {
        public int UsuarioID { get; set; }
        public string NombreUsuario { get; set; }
        public string NombreCompleto { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? UltimoAcceso { get; set; }
    }
}
