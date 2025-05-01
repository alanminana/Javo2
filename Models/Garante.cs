using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Javo2.Models
{
    public class Garante
    {
        [Key]
        public int GaranteID { get; set; }

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

        public string Telefono { get; set; } = string.Empty;

        [Required(ErrorMessage = "El celular es obligatorio.")]
        public string Celular { get; set; } = string.Empty;

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

        // Relaciones
        [Required(ErrorMessage = "La provincia es obligatoria.")]
        public int ProvinciaID { get; set; }
        public Provincia? Provincia { get; set; }

        [Required(ErrorMessage = "La ciudad es obligatoria.")]
        public int CiudadID { get; set; }
        public Ciudad? Ciudad { get; set; }

        // Relación con Cliente
        public int ClienteID { get; set; }
        public Cliente? Cliente { get; set; }

        // Información laboral
        [Required(ErrorMessage = "El lugar de trabajo es obligatorio.")]
        public string LugarTrabajo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los ingresos mensuales son obligatorios.")]
        [Range(1, double.MaxValue, ErrorMessage = "Los ingresos deben ser mayores a 0.")]
        public decimal IngresosMensuales { get; set; }

        // Información adicional
        public string RelacionCliente { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        [NotMapped]
        public string NombreCompleto => $"{Nombre} {Apellido}";

        [NotMapped]
        public string Domicilio => $"{Calle} {NumeroCalle}, {Localidad}";
    }
}