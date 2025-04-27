// ViewModels/Authentication/UsuarioFormViewModel.cs
using Javo2.Models.Authentication;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Javo2.ViewModels.Authentication
{
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
}
