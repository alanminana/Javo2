// Models/Provincia.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Javo2.Models
{
    public class Provincia
    {
        [Key]
        public int ProvinciaID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        // Relaciones
        public ICollection<Ciudad> Ciudades { get; set; }
        public ICollection<Cliente> Clientes { get; set; }
    }
}
