using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Javo2.ViewModels.Operaciones.Clientes
{
    public class ClientesViewModel
    {
        public int ClienteID { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Apellido { get; set; } = string.Empty;

        [Required]
        public int DNI { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string Telefono { get; set; } = string.Empty;
        [Required]
        public string Celular { get; set; } = string.Empty;
        public string TelefonoTrabajo { get; set; } = string.Empty;

        [Required]
        public string Calle { get; set; } = string.Empty;

        [Required]
        public string NumeroCalle { get; set; } = string.Empty;

        public decimal LimiteCreditoInicial { get; set; }
        public string NumeroPiso { get; set; } = string.Empty;
        public string Dpto { get; set; } = string.Empty;

        [Required]
        public string Localidad { get; set; } = string.Empty;

        [Required]
        public string CodigoPostal { get; set; } = string.Empty;

        [Required]
        public string DescripcionDomicilio { get; set; } = string.Empty;

        [Required]
        public int ProvinciaID { get; set; }

        [Required]
        public int CiudadID { get; set; }

        public decimal SaldoInicial { get; set; }
        public decimal SaldoDisponible { get; set; }
        public decimal Saldo { get; set; }
        public string ModificadoPor { get; set; } = string.Empty;
        public decimal DeudaTotal { get; set; } = 0;
        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime? FechaModificacion { get; set; }

        public IEnumerable<SelectListItem> Provincias { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Ciudades { get; set; } = new List<SelectListItem>();

        public bool Verificar { get; set; } = false;
        public string EstadoComentario { get; set; } = string.Empty;

        // Historial
        public List<HistorialCompraViewModel> HistorialCompras { get; set; } = new();
    }
}
