// Este es un archivo que representa cómo debería verse Models/Authentication/RolPermiso.cs
// para asegurar que no haya conflictos con otras definiciones

using System;

namespace Javo2.Models.Authentication
{
    public class RolPermiso
    {
        public int RolPermisoID { get; set; }
        public int RolID { get; set; }
        public int PermisoID { get; set; }

        // Referencias a entidades relacionadas (opcional)
        public virtual Rol Rol { get; set; }
        public virtual Permiso Permiso { get; set; }
    }
}