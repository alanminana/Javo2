// ViewModels/Operaciones/Clientes/ClienteFilterDtoViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace Javo2.ViewModels.Operaciones.Clientes
{
    public class ClienteFilterDtoViewModel
    {
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
        public string? Nombre { get; set; }

        [StringLength(100, ErrorMessage = "El apellido no puede superar los 100 caracteres.")]
        public string? Apellido { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "El DNI debe ser un número positivo.")]
        public int? Dni { get; set; }

        [EmailAddress(ErrorMessage = "Formato de email inválido.")]
        public string? Email { get; set; }
    }
}
