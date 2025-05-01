// Archivo: Models/Garante.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace Javo2.Models
{
    public class Garante
    {
        public int GaranteID { get; set; }

        [Required]
        [StringLength(50)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Apellido { get; set; } = string.Empty;

        [Required]
        [Range(1000000, 99999999)]
        public int DNI { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string Telefono { get; set; } = string.Empty;

        [Required]
        public string Celular { get; set; } = string.Empty;

        [Required]
        public string Calle { get; set; } = string.Empty;

        [Required]
        public string NumeroCalle { get; set; } = string.Empty;

        public string NumeroPiso { get; set; } = string.Empty;
        public string Dpto { get; set; } = string.Empty;

        [Required]
        public string Localidad { get; set; } = string.Empty;

        [Required]
        public string CodigoPostal { get; set; } = string.Empty;

        [Required]
        public int ProvinciaID { get; set; }
        public Provincia? Provincia { get; set; }

        [Required]
        public int CiudadID { get; set; }
        public Ciudad? Ciudad { get; set; }

        [Required]
        public string LugarTrabajo { get; set; } = string.Empty;

        [Required]
        [Range(1, double.MaxValue)]
        public decimal IngresosMensuales { get; set; }

        [Required]
        public string RelacionCliente { get; set; } = string.Empty;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }
}