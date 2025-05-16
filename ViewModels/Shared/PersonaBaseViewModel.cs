// Archivo: ViewModels/Shared/PersonaBaseViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace Javo2.ViewModels.Shared
{
    public abstract class PersonaBaseViewModel
    {


        [Required(ErrorMessage = "El apellido es obligatorio")]
        [StringLength(50, ErrorMessage = "El apellido no puede superar los 50 caracteres")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El DNI es obligatorio")]
        [Range(1000000, 99999999, ErrorMessage = "DNI debe ser un número válido")]
        public int DNI { get; set; }

        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string Email { get; set; } = string.Empty;

        public string Telefono { get; set; } = string.Empty;
        public string Celular { get; set; } = string.Empty;

        // Datos de dirección
        [Required(ErrorMessage = "La calle es obligatoria")]
        public string Calle { get; set; } = string.Empty;

        [Required(ErrorMessage = "El número de calle es obligatorio")]
        public string NumeroCalle { get; set; } = string.Empty;

        public string NumeroPiso { get; set; } = string.Empty;
        public string Dpto { get; set; } = string.Empty;
        public string Localidad { get; set; } = string.Empty;
        public string CodigoPostal { get; set; } = string.Empty;

        // Relaciones ubicación
        [Required(ErrorMessage = "La provincia es obligatoria")]
        public int ProvinciaID { get; set; }

        [Required(ErrorMessage = "La ciudad es obligatoria")]
        public int CiudadID { get; set; }
    }
}