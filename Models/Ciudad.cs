// Models/Ciudad.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Javo2.Models
{
    public class Ciudad
    {
        [Key]
        public int CiudadID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [ForeignKey("Provincia")]
        public int ProvinciaID { get; set; }
        public Provincia Provincia { get; set; }

        // Relaciones
        public ICollection<Cliente> Clientes { get; set; }
    }
}
