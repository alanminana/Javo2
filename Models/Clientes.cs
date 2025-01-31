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

        public string Telefono { get; set; } = string.Empty;

        [Required(ErrorMessage = "El celular es obligatorio.")]
        public string Celular { get; set; } = string.Empty;

        public string TelefonoTrabajo { get; set; } = string.Empty;

        // Datos Domiciliarios
        [Required(ErrorMessage = "La calle es obligatoria.")]
        public string Calle { get; set; } = string.Empty;

        [Required(ErrorMessage = "El número de calle es obligatorio.")]
        public string NumeroCalle { get; set; } = string.Empty;

        public decimal LimiteCreditoInicial { get; set; } = 0m;
        public string NumeroPiso { get; set; } = string.Empty;
        public string Dpto { get; set; } = string.Empty;

        [Required(ErrorMessage = "La localidad es obligatoria.")]
        public string Localidad { get; set; } = string.Empty;

        [Required(ErrorMessage = "El código postal es obligatorio.")]
        public string CodigoPostal { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción del domicilio es obligatoria.")]
        public string DescripcionDomicilio { get; set; } = string.Empty;

        // Relaciones
        [Required(ErrorMessage = "La provincia es obligatoria.")]
        public int ProvinciaID { get; set; }
        public Provincia Provincia { get; set; } // sin [Required]

        [Required(ErrorMessage = "La ciudad es obligatoria.")]
        public int CiudadID { get; set; }
        public Ciudad Ciudad { get; set; }       // sin [Required]


        // Otros campos
        public decimal SaldoInicial { get; set; }
        public decimal SaldoDisponible { get; set; }
        public decimal Saldo { get; set; }
        public string ModificadoPor { get; set; } = string.Empty;
        public decimal DeudaTotal { get; set; } = 0;
        public bool Activo { get; set; } = true;

        // Auditoría
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime? FechaModificacion { get; set; }

        // Colecciones (sin [Required], no es obligatorio tener ventas)
        public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
        public ICollection<Compra> Compras { get; set; } = new List<Compra>();

        // Calculado
        [NotMapped]
        public string Domicilio => $"{Calle} {NumeroCalle}, Piso: {NumeroPiso}, Dpto: {Dpto}";
    }
}
