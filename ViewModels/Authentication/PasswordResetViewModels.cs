// ViewModels/Authentication/PasswordResetViewModels.cs
using System.ComponentModel.DataAnnotations;

namespace Javo2.ViewModels.Authentication
{
    public class OlvideContraseñaViewModel
    {
        [Display(Name = "Nombre de usuario")]
        public string NombreUsuario { get; set; }

        [EmailAddress(ErrorMessage = "Ingrese un email válido")]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class ResetearContraseñaViewModel
    {
        [Required]
        [HiddenInput]
        public int UsuarioID { get; set; }

        [Required]
        [HiddenInput]
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
}

// HTMLHelpers
namespace System.ComponentModel.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class HiddenInputAttribute : Attribute
    {
    }
}