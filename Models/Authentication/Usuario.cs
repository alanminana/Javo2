// Models/Authentication/Usuario.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Javo2.Models.Authentication
{
    public class Usuario
    {
        public int UsuarioID { get; set; }

        [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
        [Display(Name = "Nombre de usuario")]
        public string NombreUsuario { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [Display(Name = "Apellido")]
        public string Apellido { get; set; }

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El email no es válido")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        // La contraseña no se expone en el modelo
        public string Contraseña { get; set; }

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;

        [Display(Name = "Fecha de creación")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [Display(Name = "Último acceso")]
        public DateTime? UltimoAcceso { get; set; }

        [Display(Name = "Creado por")]
        public string CreadoPor { get; set; }

        // Relaciones
        public List<UsuarioRol> Roles { get; set; } = new List<UsuarioRol>();
    }
}