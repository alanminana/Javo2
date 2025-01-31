using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Javo2.Models
{
    public class Provincia
    {
        [Key]
        public int ProvinciaID { get; set; }

        [Required(ErrorMessage = "El nombre de la provincia es obligatorio.")]
        [MaxLength(100, ErrorMessage = "El nombre de la provincia no puede superar los 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        // Relaciones
        public ICollection<Ciudad> Ciudades { get; set; } = new List<Ciudad>();
        public ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();
    }
}
