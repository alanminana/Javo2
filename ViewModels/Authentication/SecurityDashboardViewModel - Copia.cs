// ViewModels/Authentication/SecurityDashboardViewModel.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Javo2.ViewModels.Authentication
{
    public class UsuarioViewModels
    {
        public int TotalUsuarios { get; set; }
        public int UsuariosActivos { get; set; }
        public int TotalRoles { get; set; }
        public int TotalPermisos { get; set; }
        public List<UsuarioSimpleViewModel> UltimosUsuariosRegistrados { get; set; } = new List<UsuarioSimpleViewModel>();
        public List<UsuarioSimpleViewModel> UltimosAccesos { get; set; } = new List<UsuarioSimpleViewModel>();

        // Propiedades para cambio de contraseña
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña actual")]
        public string ContraseñaActual { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Nueva contraseña")]
        public string ContraseñaNueva { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar nueva contraseña")]
        [Compare("ContraseñaNueva", ErrorMessage = "La nueva contraseña y la confirmación no coinciden.")]
        public string ConfirmarContraseña { get; set; }
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