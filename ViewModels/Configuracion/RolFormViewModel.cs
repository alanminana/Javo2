using Javo2.Models.Authentication;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Javo2.ViewModels.Authentication
{
    public class RolFormViewModel
    {
        [Required(ErrorMessage = "El rol es obligatorio")]
        public Rol Rol { get; set; }

        // Usamos un atributo diferente ya que NotMapped puede no estar disponible
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public Dictionary<string, List<Permiso>> GruposPermisos { get; set; }

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public List<int> PermisosSeleccionados { get; set; } = new List<int>();

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public bool EsEdicion { get; set; }
    }
}