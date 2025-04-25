// Models/Authentication/Rol.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Javo2.Models.Authentication
{
    public class Rol
    {
        public int RolID { get; set; }

        [Required(ErrorMessage = "El nombre del rol es obligatorio")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "El nombre del rol debe tener entre 2 y 50 caracteres")]
        public string Nombre { get; set; }

        [StringLength(255, ErrorMessage = "La descripción debe tener menos de 255 caracteres")]
        public string Descripcion { get; set; }

        public bool EsSistema { get; set; } = false;

        public List<RolPermiso> Permisos { get; set; } = new List<RolPermiso>();

        public List<UsuarioRol> Usuarios { get; set; } = new List<UsuarioRol>();
    }
}