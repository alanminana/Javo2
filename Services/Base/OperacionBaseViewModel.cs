using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Javo2.ViewModels.Operaciones.Base
{
    public abstract class OperacionBaseViewModel
    {
        public int ID { get; set; }

        [Display(Name = "Fecha")]
        [DataType(DataType.Date)]
        public DateTime Fecha { get; set; } = DateTime.Today;

        [Display(Name = "Número")]
        public string Numero { get; set; } = string.Empty;

        public string Usuario { get; set; } = string.Empty;
        public string Vendedor { get; set; } = string.Empty;

        // Datos del Cliente
        public int? DniCliente { get; set; }

        [Required(ErrorMessage = "El nombre del cliente es requerido")]
        [Display(Name = "Cliente")]
        public string NombreCliente { get; set; } = string.Empty;

        public string TelefonoCliente { get; set; } = string.Empty;
        public string DomicilioCliente { get; set; } = string.Empty;
        public string LocalidadCliente { get; set; } = string.Empty;
        public string CelularCliente { get; set; } = string.Empty;

        // Observaciones
        [Display(Name = "Observaciones")]
        public string Observaciones { get; set; } = string.Empty;

        // Totales
        public decimal PrecioTotal { get; set; }
        public int TotalProductos { get; set; }
    }
}