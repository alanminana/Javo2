// ViewModels/Operaciones/Ventas/CotizacionViewModel.cs
using Javo2.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Javo2.ViewModels.Operaciones.Ventas
{
    public class CotizacionViewModel
    {
        public int CotizacionID { get; set; }

        [Display(Name = "Fecha de Cotización")]
        [DataType(DataType.Date)]
        public DateTime FechaCotizacion { get; set; } = DateTime.Today;

        [Display(Name = "Número")]
        public string NumeroCotizacion { get; set; } = string.Empty;

        // Datos básicos del Cliente (reducidos)
        public int DniCliente { get; set; }

        [Required(ErrorMessage = "El nombre del cliente es requerido")]
        [Display(Name = "Cliente")]
        public string NombreCliente { get; set; } = string.Empty;

        public string TelefonoCliente { get; set; } = string.Empty;
        public string EmailCliente { get; set; } = string.Empty;

        // Productos (igual a ventas, necesitamos el detalle)
        public List<DetalleVentaViewModel> ProductosPresupuesto { get; set; } = new List<DetalleVentaViewModel>();

        // Totales
        public decimal PrecioTotal { get; set; }
        public int TotalProductos { get; set; }

        // Vigencia y observaciones para cotización
        [Display(Name = "Vigencia (días)")]
        [Range(1, 90, ErrorMessage = "La vigencia debe estar entre 1 y 90 días")]
        public int DiasVigencia { get; set; } = 15;

        [Display(Name = "Observaciones")]
        public string Observaciones { get; set; } = string.Empty;

        // Validación
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ProductosPresupuesto == null || ProductosPresupuesto.Count == 0)
            {
                yield return new ValidationResult(
                    "Debe agregar al menos un producto a la cotización.",
                    new[] { nameof(ProductosPresupuesto) }
                );
            }
        }
    }

    public class CotizacionListViewModel
    {
        public int CotizacionID { get; set; }
        public DateTime FechaCotizacion { get; set; }
        public string NumeroCotizacion { get; set; } = string.Empty;
        public string NombreCliente { get; set; } = string.Empty;
        public decimal PrecioTotal { get; set; }
        public int TotalProductos { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public bool Vigente => DateTime.Now <= FechaVencimiento;
    }
}