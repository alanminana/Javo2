// Models/Authentication/RolPermiso.cs - Tabla de relación muchos a muchos
namespace Javo2.Models.Authentication
{
    public class RolPermiso
    {
        public int RolPermisoID { get; set; }

        public int RolID { get; set; }
        public Rol Rol { get; set; }

        public int PermisoID { get; set; }
        public Permiso Permiso { get; set; }
    }
}