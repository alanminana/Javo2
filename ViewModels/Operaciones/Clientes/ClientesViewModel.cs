using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace javo2.ViewModels.Operaciones.Clientes
{
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
        public int Telefono { get; set; }

        [Display(Name = "Celular")]
        public int Celular { get; set; }

        [Display(Name = "Teléfono del Trabajo")]
        public int TelefonoTrabajo { get; set; }

        [Display(Name = "Calle")]
        public string Calle { get; set; } = string.Empty;

        [Display(Name = "Número de Calle")]
        public int NumeroCalle { get; set; }

        [Display(Name = "Número de Piso")]
        public int? NumeroPiso { get; set; }

        [Display(Name = "Departamento")]
        public string Dpto { get; set; } = string.Empty;

        [Display(Name = "Localidad")]
        public string Localidad { get; set; } = string.Empty;

        [Display(Name = "Código Postal")]
        public int? CodigoPostal { get; set; }

        [Display(Name = "Descripción del Domicilio")]
        public string DescripcionDomicilio { get; set; } = string.Empty;

        [Display(Name = "Nombre del Garante")]
        public string NombreGarante { get; set; } = string.Empty;

        [Display(Name = "Apellido del Garante")]
        public string ApellidoGarante { get; set; } = string.Empty;

        [Range(1000000, 99999999, ErrorMessage = "DNI del Garante debe ser un número válido")]
        [Display(Name = "DNI del Garante")]
        public int? DNIGarante { get; set; }

        [Display(Name = "Teléfono del Garante")]
        public int TelefonoGarante { get; set; }

        [Range(1000000, 99999999, ErrorMessage = "DNI del Cónyuge debe ser un número válido")]
        [Display(Name = "DNI del Cónyuge")]
        public int? NumeroDNIConyugue { get; set; }

        [Display(Name = "Nombre del Cónyuge")]
        public string NombreConyugue { get; set; } = string.Empty;

        [Display(Name = "Apellido del Cónyuge")]
        public string ApellidoConyugue { get; set; } = string.Empty;

        [Display(Name = "Autoriza")]
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
        public IEnumerable<SelectListItem> TiposDeCliente { get; set; } = [];

        [Display(Name = "Ciudad")]
        public IEnumerable<SelectListItem> Ciudades { get; set; } = [];

        [Display(Name = "Provincia")]
        public IEnumerable<SelectListItem> Provincias { get; set; } = [];

        [Display(Name = "Tipo de Consumidor")]
        public IEnumerable<SelectListItem> TiposConsumidor { get; set; } = [];

        [Display(Name = "Tipo de Cliente")]
        public int TipoCliente { get; set; }

        [Display(Name = "Ciudad")]
        public int Ciudad { get; set; }

        [Display(Name = "Provincia")]
        public int Provincia { get; set; }

        [Display(Name = "Código de Nivel de Crédito")]
        public int CodigoNivelCredito { get; set; }

        [Display(Name = "Tipo de Consumidor")]
        public int TipoConsumidor { get; set; }
    }
}
