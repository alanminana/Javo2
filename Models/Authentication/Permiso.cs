using System.ComponentModel.DataAnnotations;

namespace Javo2.Models.Authentication
{
    public class Permiso
    {
        public int PermisoID { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El código es obligatorio")]
        [Display(Name = "Código")]
        public string Codigo { get; set; }

        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        [Display(Name = "Grupo")]
        public string Grupo { get; set; }

        [Display(Name = "Es sistema")]
        public bool EsSistema { get; set; }

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;
    }
}