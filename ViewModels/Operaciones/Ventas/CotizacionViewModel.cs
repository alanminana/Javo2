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

        // Datos básicos del Cliente
        public int DniCliente { get; set; }

        [Required(ErrorMessage = "El nombre del cliente es requerido")]
        [Display(Name = "Cliente")]
        public string NombreCliente { get; set; } = string.Empty;

        public string TelefonoCliente { get; set; } = string.Empty;
        public string EmailCliente { get; set; } = string.Empty;

        // Productos
        public List<DetalleVentaViewModel> ProductosPresupuesto { get; set; } = new List<DetalleVentaViewModel>();

        // Totales
        public decimal PrecioTotal { get; set; }
        public int TotalProductos { get; set; }

        // Vigencia y observaciones
        [Display(Name = "Vigencia (días)")]
        [Range(1, 90, ErrorMessage = "La vigencia debe estar entre 1 y 90 días")]
        public int DiasVigencia { get; set; } = 15;

        [Display(Name = "Observaciones")]
        public string Observaciones { get; set; } = string.Empty;

        // Formas de pago
        public int FormaPagoID { get; set; }
        public IEnumerable<SelectListItem> FormasPago { get; set; } = new List<SelectListItem>();

        public int? BancoID { get; set; }
        public IEnumerable<SelectListItem> Bancos { get; set; } = new List<SelectListItem>();

        public string TipoTarjeta { get; set; } = string.Empty;
        public IEnumerable<SelectListItem> TipoTarjetaOptions { get; set; } = new List<SelectListItem>();

        public int? Cuotas { get; set; }
        public IEnumerable<SelectListItem> CuotasOptions { get; set; } = new List<SelectListItem>();

        public string EntidadElectronica { get; set; } = string.Empty;
        public IEnumerable<SelectListItem> EntidadesElectronicas { get; set; } = new List<SelectListItem>();

        public string PlanFinanciamiento { get; set; } = string.Empty;
        public IEnumerable<SelectListItem> PlanesFinanciamiento { get; set; } = new List<SelectListItem>();
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