using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Javo2.Models.Authentication
{
    public class Rol
    {
        public int RolID { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        [Display(Name = "Es sistema")]
        public bool EsSistema { get; set; }

        public List<RolPermiso> Permisos { get; set; } = new List<RolPermiso>();
    }
}