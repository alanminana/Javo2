// Models/Marca.cs
using System.ComponentModel.DataAnnotations;

namespace Javo2.Models
{
    public class Marca
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "El nombre de la marca es obligatorio.")]
        public string Nombre { get; set; } = string.Empty;
    }
}
