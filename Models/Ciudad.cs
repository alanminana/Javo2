using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Javo2.Models
{
    public class Ciudad
    {
        [Key]
        public int CiudadID { get; set; }

        [Required(ErrorMessage = "El nombre de la ciudad es obligatorio.")]
        [MaxLength(100, ErrorMessage = "El nombre de la ciudad no puede superar los 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [ForeignKey("Provincia")]
        [Required(ErrorMessage = "La provincia es obligatoria.")]
        public int ProvinciaID { get; set; }
        public Provincia Provincia { get; set; }

        // Relaciones
        public ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();
    }
}
