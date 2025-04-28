// ViewModels/Authentication/AuthViewModels.cs
using Javo2.Models.Authentication;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Javo2.ViewModels.Authentication
{
    // ViewModel para el formulario de usuario (crear/editar)
    public class UsuarioFormViewModel
    {
        public Usuario Usuario { get; set; }

        [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} caracteres de longitud.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Contraseña { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña")]
        [Compare("Contraseña", ErrorMessage = "La contraseña y la confirmación no coinciden.")]
        public string ConfirmarContraseña { get; set; }

        public IEnumerable<SelectListItem> RolesDisponibles { get; set; }
        public List<int> RolesSeleccionados { get; set; }
        public bool EsEdicion { get; set; }
    }

    // ViewModel para los detalles del usuario
    public class UsuarioDetailsViewModel
    {
        public Usuario Usuario { get; set; }
        public List<Rol> Roles { get; set; }
    }

    // ViewModel para el filtro de usuarios
    public class UsuarioFilterViewModel
    {
        [Display(Name = "Buscar")]
        public string Termino { get; set; }

        [Display(Name = "Estado")]
        public bool? Activo { get; set; }

        [Display(Name = "Rol")]
        public int RolID { get; set; }
    }

    // ViewModel para el inicio de sesión
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
        [Display(Name = "Nombre de usuario")]
        public string NombreUsuario { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Contraseña { get; set; }

        [Display(Name = "Recordarme")]
        public bool RecordarMe { get; set; }
    }

    // ViewModel para el registro de usuarios
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
        [Display(Name = "Nombre de usuario")]
        public string NombreUsuario { get; set; }

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El email no es válido")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [Display(Name = "Apellido")]
        public string Apellido { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} caracteres de longitud.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Contraseña { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña")]
        [Compare("Contraseña", ErrorMessage = "La contraseña y la confirmación no coinciden.")]
        public string ConfirmarContraseña { get; set; }
    }

    // ViewModel para cambiar contraseña
    public class CambiarContraseñaViewModel
    {
        [Required(ErrorMessage = "La contraseña actual es obligatoria")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña actual")]
        public string ContraseñaActual { get; set; }

        [Required(ErrorMessage = "La nueva contraseña es obligatoria")]
        [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} caracteres de longitud.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva contraseña")]
        public string ContraseñaNueva { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar nueva contraseña")]
        [Compare("ContraseñaNueva", ErrorMessage = "La nueva contraseña y la confirmación no coinciden.")]
        public string ConfirmarContraseña { get; set; }
    }

    // ViewModel para recuperar contraseña
    public class OlvideContraseñaViewModel
    {
        [Display(Name = "Nombre de usuario")]
        public string NombreUsuario { get; set; }

        [EmailAddress(ErrorMessage = "Ingrese un email válido")]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    // ViewModel para restablecer contraseña
    public class ResetearContraseñaViewModel
    {
        [Required]
        public int UsuarioID { get; set; }

        [Required]
        public string Token { get; set; }

        [Required(ErrorMessage = "La nueva contraseña es obligatoria")]
        [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} caracteres de longitud.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva contraseña")]
        public string NuevaContraseña { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar nueva contraseña")]
        [Compare("NuevaContraseña", ErrorMessage = "La nueva contraseña y la confirmación no coinciden.")]
        public string ConfirmarContraseña { get; set; }
    }

    // ViewModel para el dashboard de seguridad
    public class SecurityDashboardViewModel
    {
        public int TotalUsuarios { get; set; }
        public int UsuariosActivos { get; set; }
        public int TotalRoles { get; set; }
        public int TotalPermisos { get; set; }
        public List<UsuarioSimpleViewModel> UltimosUsuariosRegistrados { get; set; } = new List<UsuarioSimpleViewModel>();
        public List<UsuarioSimpleViewModel> UltimosAccesos { get; set; } = new List<UsuarioSimpleViewModel>();
    }

    // ViewModel simplificado para usuario
    public class UsuarioSimpleViewModel
    {
        public int UsuarioID { get; set; }
        public string NombreUsuario { get; set; }
        public string NombreCompleto { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? UltimoAcceso { get; set; }
    }
}