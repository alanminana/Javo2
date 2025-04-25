// Models/Authentication/UsuarioRol.cs - Tabla de relación muchos a muchos
namespace Javo2.Models.Authentication
{
    public class UsuarioRol
    {
        public int UsuarioRolID { get; set; }

        public int UsuarioID { get; set; }
        public Usuario Usuario { get; set; }

        public int RolID { get; set; }
        public Rol Rol { get; set; }
    }
}