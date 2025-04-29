using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Javo2.Models.Authentication
{
    public class Rol
    {
        public int RolID { get; set; }

        [Required(ErrorMessage = "El nombre del rol es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede tener más de 100 caracteres")]
        public string Nombre { get; set; }

        [StringLength(500, ErrorMessage = "La descripción no puede tener más de 500 caracteres")]
        public string Descripcion { get; set; }

        public bool EsSistema { get; set; }

        private List<RolPermiso> _permisos;
        public List<RolPermiso> Permisos
        {
            get { return _permisos ?? (_permisos = new List<RolPermiso>()); }
            set { _permisos = value; }
        }
    }
}