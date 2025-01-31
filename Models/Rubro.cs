// Models/Rubro.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Javo2.Models
{
    public class Rubro
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "El nombre del rubro es obligatorio.")]
        public string Nombre { get; set; } = string.Empty;

        public ICollection<SubRubro> SubRubros { get; set; } = new List<SubRubro>();
    }
}
