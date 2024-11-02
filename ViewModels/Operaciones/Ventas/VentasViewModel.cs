// Archivo: ViewModels/Operaciones/Ventas/VentasViewModel.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Javo2.Models;
using Javo2.ViewModels.Operaciones.Productos;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Javo2.ViewModels.Operaciones.Ventas
{
    public class VentasViewModel
    {
        public int VentaID { get; set; }

        [Required]
        public DateTime FechaVenta { get; set; }

        [Required]
        [StringLength(50)]
        public string NumeroFactura { get; set; } = string.Empty;

        [Required]
        public string Usuario { get; set; } = "cosmefulanito";

        [Required]
        public string Vendedor { get; set; } = "cosmefulanito";

        [Required]
        public int ClienteID { get; set; }

        [Required]
        [StringLength(100)]
        public string NombreCliente { get; set; } = string.Empty;

        [Required]
        [StringLength(15)]
        public string TelefonoCliente { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string DomicilioCliente { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LocalidadCliente { get; set; } = string.Empty;

        [Required]
        [StringLength(15)]
        public string CelularCliente { get; set; } = string.Empty;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal LimiteCreditoCliente { get; set; }

        public string ComentariosAutorizacion { get; set; } = "N/A";

        [Required]
        public string EstadoEntrega { get; set; } = "Pendiente";

        public DateTime FechaEstimadaEntrega { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal SaldoCliente { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal SaldoDisponibleCliente { get; set; }

        [Required]
        [StringLength(50)]
        public string FormaPago { get; set; } = string.Empty;

        public string Banco { get; set; } = string.Empty;
        public string EntidadElectronica { get; set; } = string.Empty;
        public string PlanFinanciamiento { get; set; } = string.Empty;
        public string Observaciones { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal AdelantoDinero { get; set; }

        [Range(0, double.MaxValue)]
        public decimal DineroContado { get; set; }

        [Range(0, double.MaxValue)]
        public decimal MontoCheque { get; set; }

        public int NumeroCheque { get; set; }
        public string Condiciones { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal Credito { get; set; }

        public DateTime FechaEntrega { get; set; }
        public EstadoVenta Estado { get; set; }

        [StringLength(50)]
        public string UsuarioAutorizador { get; set; } = "cosmefulanito";

        public DateTime FechaAutorizacion { get; set; }

        public bool EsCotizacion { get; set; }

        public List<ProductoPresupuestoViewModel> ProductosPresupuesto { get; set; } = new();

        [Range(0, double.MaxValue)]
        public decimal TotalProductos { get; set; }

        [Range(0, double.MaxValue)]
        public decimal PrecioTotal { get; set; }

        public IEnumerable<SelectListItem> FormasPago { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Bancos { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> TipoTarjeta { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Cuotas { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> EntidadesElectronicas { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> PlanesFinanciamiento { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> TipoEntregas { get; set; } = new List<SelectListItem>();
    }
}
