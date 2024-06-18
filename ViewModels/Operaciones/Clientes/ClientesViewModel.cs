using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Javo2.ViewModels.Operaciones.Clientes
{
    public class CiudadViewModel
    {
        public int CiudadID { get; set; }
        public string Nombre { get; set; }
        public int ProvinciaID { get; set; }
    }

    public class ClientesViewModel
    {
        public int ClienteID { get; set; } = 0; // Auto-incremental y no editable

        [Required]
        [StringLength(50)]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Apellido")]
        public string Apellido { get; set; } = string.Empty;

        [Required]
        [Range(1000000, 99999999, ErrorMessage = "DNI debe ser un número válido")]
        [Display(Name = "DNI")]
        public int DNI { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        [Display(Name = "Celular")]
        public string Celular { get; set; } = string.Empty;

        [Display(Name = "Teléfono del Trabajo")]
        public string? TelefonoTrabajo { get; set; }

        [Display(Name = "Calle")]
        public string Calle { get; set; } = string.Empty;

        [Display(Name = "Número de Calle")]
        public string NumeroCalle { get; set; }

        [Display(Name = "Número de Piso")]
        public string? NumeroPiso { get; set; }

        [Display(Name = "Departamento")]
        public string? Dpto { get; set; } = string.Empty;

        [Display(Name = "Localidad")]
        public string Localidad { get; set; } = string.Empty;

        [Display(Name = "Código Postal")]
        public string CodigoPostal { get; set; }

        [Display(Name = "Descripción del Domicilio")]
        public string DescripcionDomicilio { get; set; } = string.Empty;

        [Display(Name = "Nombre del Garante")]
        public string? NombreGarante { get; set; } = string.Empty;

        [Display(Name = "Apellido del Garante")]
        public string? ApellidoGarante { get; set; } = string.Empty;

        [Range(1000000, 99999999, ErrorMessage = "DNI del Garante debe ser un número válido")]
        [Display(Name = "DNI del Garante")]
        public string? DNIGarante { get; set; }

        [Display(Name = "Teléfono del Garante")]
        public string? TelefonoGarante { get; set; }

        [Range(1000000, 99999999, ErrorMessage = "DNI del Cónyuge debe ser un número válido")]
        [Display(Name = "DNI del Cónyuge")]
        public string? NumeroDNIConyugue { get; set; }

        [Display(Name = "Nombre del Cónyuge")]
        public string? NombreConyugue { get; set; } 

        [Display(Name = "Apellido del Cónyuge")]
        public string? ApellidoConyugue { get; set; }

        public bool Autoriza { get; set; } = false;

        [Display(Name = "Verificar")]
        public bool Verificar { get; set; } = false;

        [Display(Name = "Estado del Comentario")]
        public string EstadoComentario { get; set; } = "Sin comentarios"; // No editable

        [Display(Name = "Deuda Total")]
        public decimal DeudaTotal { get; set; } = 0; // No editable

        [Display(Name = "Saldo")]
        public decimal Saldo { get; set; } = 0; // No editable

        [Display(Name = "Modificado Por")]
        public string ModificadoPor { get; set; } = "No modificado"; // No editable

        [Display(Name = "Importe del Crédito")]
        public decimal ImporteCredito { get; set; } = 0; // No editable

        [Display(Name = "Comentarios")]
        public string Comentarios { get; set; } = string.Empty;

        // Propiedades para listas desplegables
        [Display(Name = "Tipo de Cliente")]
        public IEnumerable<SelectListItem> TiposDeCliente { get; set; } = new List<SelectListItem>();

        [Display(Name = "Tipo de Consumidor")]
        public IEnumerable<SelectListItem> TiposConsumidor { get; set; } = new List<SelectListItem>();

        [Display(Name = "Tipo de Cliente")]
        public int TipoCliente { get; set; }

        [Display(Name = "Ciudad")]
        public int Ciudad { get; set; }

        [Display(Name = "Código de Nivel de Crédito")]
        public int CodigoNivelCredito { get; set; }

        [Display(Name = "Tipo de Consumidor")]
        public int TipoConsumidor { get; set; }

        [Display(Name = "Provincia")]
        [Required(ErrorMessage = "Debe seleccionar una provincia")]
        public int ProvinciaID { get; set; }

        [Display(Name = "Ciudad")]
        [Required(ErrorMessage = "Debe seleccionar una ciudad")]
        public int CiudadID { get; set; }

        public IEnumerable<SelectListItem> Provincias { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Ciudades { get; set; } = new List<SelectListItem>();

        [Display(Name = "Cliente Autorizado?")]
        public bool ClienteAutorizado { get; set; } = false;

    }

    public class ProvinciaViewModel
    {
        public int ProvinciaID { get; set; }
        public string? Nombre { get; set; }
    }
}
