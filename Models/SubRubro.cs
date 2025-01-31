// Models/SubRubro.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Javo2.Models
{
    public class SubRubro
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "El nombre del subrubro es obligatorio.")]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [ForeignKey("Rubro")]
        public int RubroID { get; set; }
        public Rubro Rubro { get; set; }
    }
}
