// Models/Authentication/Permiso.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Javo2.Models.Authentication
{
    public class Permiso
    {
        public int PermisoID { get; set; }

        [Required(ErrorMessage = "El código del permiso es obligatorio")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El código del permiso debe tener entre 2 y 100 caracteres")]
        public string Codigo { get; set; }

        [Required(ErrorMessage = "El nombre del permiso es obligatorio")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre del permiso debe tener entre 2 y 100 caracteres")]
        public string Nombre { get; set; }

        [StringLength(255, ErrorMessage = "La descripción debe tener menos de 255 caracteres")]
        public string Descripcion { get; set; }

        public string Modulo { get; set; }

        public bool EsSistema { get; set; } = false;

        public List<RolPermiso> Roles { get; set; } = new List<RolPermiso>();
    }
}