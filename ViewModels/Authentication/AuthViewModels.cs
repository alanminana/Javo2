// Archivo principal para todos los ViewModels relacionados con autenticación
using Javo2.Models.Authentication;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Javo2.ViewModels.Authentication
{
    #region Usuario ViewModels

    // ViewModel para el formulario de usuario (crear/editar)
    public class UsuarioFormViewModel
    {
        [Required(ErrorMessage = "El usuario es obligatorio")]
        public Usuario Usuario { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} caracteres de longitud.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Contraseña { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña")]
        [Compare("Contraseña", ErrorMessage = "La contraseña y la confirmación no coinciden.")]
        public string ConfirmarContraseña { get; set; }

        public IEnumerable<SelectListItem> RolesDisponibles { get; set; } = new List<SelectListItem>();

        public List<int> RolesSeleccionados { get; set; } = new List<int>();

        public bool EsEdicion { get; set; }

        public UsuarioFormViewModel()
        {
            // Inicializar Usuario para evitar NullReferenceException
            Usuario = new Usuario();
            RolesSeleccionados = new List<int>();
        }
    }
    public class UsuarioDetailsViewModel
    {
        public Usuario Usuario { get; set; }
        public List<Rol> Roles { get; set; } = new List<Rol>();
    }
    // ViewModel para el filtro de usuarios
    public class UsuarioFilterViewModel
    {
        [Display(Name = "Buscar")]
        public string Termino { get; set; }

        [Display(Name = "Estado")]
        public bool? Activo { get; set; }

        [Display(Name = "Rol")]
        public int RolID { get; set; }
    }

    // ViewModel simplificado para usuario (utilizado en listas)
    public class UsuarioSimpleViewModel
    {
        public int UsuarioID { get; set; }
        public string NombreUsuario { get; set; }
        public string NombreCompleto { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? UltimoAcceso { get; set; }
    }

    #endregion

    #region Auth ViewModels

    // ViewModel para el inicio de sesión
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
        [Display(Name = "Nombre de usuario")]
        public string NombreUsuario { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Contraseña { get; set; }

        [Display(Name = "Recordarme")]
        public bool RecordarMe { get; set; }
    }

    // ViewModel para el registro de usuarios
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
        [Display(Name = "Nombre de usuario")]
        public string NombreUsuario { get; set; }

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El email no es válido")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [Display(Name = "Apellido")]
        public string Apellido { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} caracteres de longitud.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Contraseña { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña")]
        [Compare("Contraseña", ErrorMessage = "La contraseña y la confirmación no coinciden.")]
        public string ConfirmarContraseña { get; set; }
    }

    // ViewModel para cambiar contraseña
    public class CambiarContraseñaViewModel
    {
        [Required(ErrorMessage = "La contraseña actual es obligatoria")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña actual")]
        public string ContraseñaActual { get; set; }

        [Required(ErrorMessage = "La nueva contraseña es obligatoria")]
        [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} caracteres de longitud.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva contraseña")]
        public string ContraseñaNueva { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar nueva contraseña")]
        [Compare("ContraseñaNueva", ErrorMessage = "La nueva contraseña y la confirmación no coinciden.")]
        public string ConfirmarContraseña { get; set; }
    }

    #endregion

    #region Password Reset ViewModels

    // ViewModel para recuperar contraseña
    public class OlvideContraseñaViewModel
    {
        [Display(Name = "Nombre de usuario")]
        public string NombreUsuario { get; set; }

        [EmailAddress(ErrorMessage = "Ingrese un email válido")]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    // ViewModel para restablecer contraseña
    public class ResetearContraseñaViewModel
    {
        [Required]
        [HiddenInput]
        public int UsuarioID { get; set; }

        [Required]
        [HiddenInput]
        public string Token { get; set; }

        [Required(ErrorMessage = "La nueva contraseña es obligatoria")]
        [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} caracteres de longitud.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva contraseña")]
        public string NuevaContraseña { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar nueva contraseña")]
        [Compare("NuevaContraseña", ErrorMessage = "La nueva contraseña y la confirmación no coinciden.")]
        public string ConfirmarContraseña { get; set; }
    }

    // Atributo para campos ocultos
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class HiddenInputAttribute : Attribute
    {
    }

    #endregion

    #region Perfil ViewModels

    // ViewModel para ver el perfil completo del usuario
    public class PerfilViewModel
    {
        public int UsuarioID { get; set; }
        public string NombreUsuario { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? UltimoAcceso { get; set; }

        public List<RolBasicoViewModel> Roles { get; set; } = new List<RolBasicoViewModel>();
        public List<PermisoBasicoViewModel> Permisos { get; set; } = new List<PermisoBasicoViewModel>();
    }

    // ViewModel para editar el perfil
    public class EditarPerfilViewModel
    {
        public int UsuarioID { get; set; }

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

        [DataType(DataType.Password)]
        [Display(Name = "Contraseña actual")]
        public string ContraseñaActual { get; set; }

        [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} caracteres de longitud.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva contraseña")]
        public string ContraseñaNueva { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar nueva contraseña")]
        [Compare("ContraseñaNueva", ErrorMessage = "La nueva contraseña y la confirmación no coinciden.")]
        public string ConfirmarContraseña { get; set; }
    }

    // ViewModels para mostrar información básica de roles y permisos
    public class RolBasicoViewModel
    {
        public int RolID { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
    }

    public class PermisoBasicoViewModel
    {
        public int PermisoID { get; set; }
        public string Nombre { get; set; }
        public string Codigo { get; set; }
        public string Grupo { get; set; }
    }

    #endregion

    #region Dashboard ViewModels

    // ViewModel para el dashboard de seguridad
    public class SecurityDashboardViewModel
    {
        public int TotalUsuarios { get; set; }
        public int UsuariosActivos { get; set; }
        public int TotalRoles { get; set; }
        public int TotalPermisos { get; set; }
        public List<UsuarioSimpleViewModel> UltimosUsuariosRegistrados { get; set; } = new List<UsuarioSimpleViewModel>();
        public List<UsuarioSimpleViewModel> UltimosAccesos { get; set; } = new List<UsuarioSimpleViewModel>();
    
    [Display(Name = "Clasificación de Crédito")]
        [Range(1, 10, ErrorMessage = "La clasificación debe estar entre 1 y 10")]
        public int ClasificacionCredito { get; set; } = 1;

        public string TextoClasificacionCredito { get; set; } = "Sin clasificar";

        // Diccionario para mostrar las opciones de clasificación en la vista
        [NotMapped]
        public Dictionary<int, string> OpcionesClasificacion => new()
    {
        { 1, "1 - No apto para crédito" },
        { 2, "2 - No apto para crédito" },
        { 3, "3 - Riesgo alto" },
        { 4, "4 - Riesgo alto" },
        { 5, "5 - Riesgo moderado" },
        { 6, "6 - Riesgo moderado" },
        { 7, "7 - Apto con garantía" },
        { 8, "8 - Apto con garantía" },
        { 9, "9 - Totalmente apto" },
        { 10, "10 - Totalmente apto" }
    };
        #endregion
    }    }