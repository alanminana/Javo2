// Models/Authentication/Rol.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Javo2.Models.Authentication
{
    public class Rol
    {
        public int RolID { get; set; }

        [Required(ErrorMessage = "El nombre del rol es obligatorio")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        [Display(Name = "Es de sistema")]
        public bool EsSistema { get; set; } = false;

        // Relaciones
        public List<RolPermiso> Permisos { get; set; } = new List<RolPermiso>();
    }
}
