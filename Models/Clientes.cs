using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Javo2.Models
{
    public class Cliente
    {
        [Key]
        public int ClienteID { get; set; }

        // Datos Personales
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [MaxLength(50, ErrorMessage = "El nombre no puede superar los 50 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [MaxLength(50, ErrorMessage = "El apellido no puede superar los 50 caracteres.")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El DNI es obligatorio.")]
        [Range(1000000, 99999999, ErrorMessage = "DNI debe ser un número válido.")]
        public int DNI { get; set; }

        [Required(ErrorMessage = "El email es obligatorio.")]
        [EmailAddress(ErrorMessage = "Formato de email inválido.")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Formato de teléfono inválido.")]
        public string Telefono { get; set; } = string.Empty;

        [Required(ErrorMessage = "El celular es obligatorio.")]
        [Phone(ErrorMessage = "Formato de celular inválido.")]
        public string Celular { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Formato de teléfono de trabajo inválido.")]
        public string TelefonoTrabajo { get; set; } = string.Empty;

        // Datos Domiciliarios
        [Required(ErrorMessage = "La calle es obligatoria.")]
        public string Calle { get; set; } = string.Empty;

        [Required(ErrorMessage = "El número de calle es obligatorio.")]
        public string NumeroCalle { get; set; } = string.Empty;

        public string NumeroPiso { get; set; } = string.Empty;
        public string Dpto { get; set; } = string.Empty;

        [Required(ErrorMessage = "La localidad es obligatoria.")]
        public string Localidad { get; set; } = string.Empty;

        [Required(ErrorMessage = "El código postal es obligatorio.")]
        public string CodigoPostal { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción del domicilio es obligatoria.")]
        public string DescripcionDomicilio { get; set; } = string.Empty;

        // Relaciones
        [ForeignKey("Provincia")]
        public int ProvinciaID { get; set; }
        public Provincia Provincia { get; set; }

        [ForeignKey("Ciudad")]
        public int CiudadID { get; set; }
        public Ciudad Ciudad { get; set; }

        // Otros campos
        public string ModificadoPor { get; set; } = string.Empty;
        public decimal DeudaTotal { get; set; } = 0;
        public decimal Saldo { get; set; } = 0;
        public bool Activo { get; set; } = true;

        // Campos de auditoría (opcional)
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime? FechaModificacion { get; set; }
    }
}