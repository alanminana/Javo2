// 12. ViewModels/Authentication/PerfilViewModel.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Javo2.ViewModels.Authentication
{
    // ViewModel para ver el perfil completo del usuario
    public class PerfilViewModel
    {
        public int UsuarioID { get; set; }
        public string NombreUsuario { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? UltimoAcceso { get; set; }

        public List<RolBasicoViewModel> Roles { get; set; } = new List<RolBasicoViewModel>();
        public List<PermisoBasicoViewModel> Permisos { get; set; } = new List<PermisoBasicoViewModel>();
    }

    // ViewModel para editar el perfil
    public class EditarPerfilViewModel
    {
        public int UsuarioID { get; set; }

        [Display(Name = "Nombre de usuario")]
        public string NombreUsuario { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [Display(Name = "Apellido")]
        public string Apellido { get; set; }

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El email no es válido")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Contraseña actual")]
        public string ContraseñaActual { get; set; }

        [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} caracteres de longitud.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva contraseña")]
        public string ContraseñaNueva { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar nueva contraseña")]
        [Compare("ContraseñaNueva", ErrorMessage = "La nueva contraseña y la confirmación no coinciden.")]
        public string ConfirmarContraseña { get; set; }
    }

    // ViewModels para mostrar información básica de roles y permisos
    public class RolBasicoViewModel
    {
        public int RolID { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
    }

    public class PermisoBasicoViewModel
    {
        public int PermisoID { get; set; }
        public string Nombre { get; set; }
        public string Codigo { get; set; }
        public string Grupo { get; set; }
    }
}