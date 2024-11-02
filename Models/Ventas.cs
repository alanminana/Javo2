// Archivo: Models/Venta.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Javo2.Models
{
    public enum EstadoVenta
    {
        PendienteDeAutorizacion,
        PendienteDeEntrega,
        Entregado,
        Cancelado,
        Rechazada
    }

    public class Venta
    {
        public int VentaID { get; set; }

        [Required]
        public DateTime FechaVenta { get; set; }

        [Required]
        [StringLength(50)]
        public string NumeroFactura { get; set; } = string.Empty;

        [Required]
        public string Usuario { get; set; } = string.Empty;

        [Required]
        public string Vendedor { get; set; } = string.Empty;

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
        public decimal LimiteCreditoCliente { get; set; }

        public string ComentariosAutorizacion { get; set; } = "N/A";

        [Required]
        public string EstadoEntrega { get; set; } = "Pendiente";

        public DateTime FechaEstimadaEntrega { get; set; }

        [Required]
        public decimal SaldoCliente { get; set; }

        [Required]
        public decimal SaldoDisponibleCliente { get; set; }

        [Required]
        [StringLength(50)]
        public string FormaPago { get; set; } = string.Empty;

        public string Banco { get; set; } = string.Empty;
        public string EntidadElectronica { get; set; } = string.Empty;
        public string PlanFinanciamiento { get; set; } = string.Empty;
        public string Observaciones { get; set; } = string.Empty;

        public decimal AdelantoDinero { get; set; }

        public decimal DineroContado { get; set; }

        public decimal MontoCheque { get; set; }

        public int NumeroCheque { get; set; }
        public string Condiciones { get; set; } = string.Empty;

        public decimal Credito { get; set; }

        public DateTime FechaEntrega { get; set; }
        public EstadoVenta Estado { get; set; }

        [StringLength(50)]
        public string UsuarioAutorizador { get; set; } = string.Empty;

        public DateTime FechaAutorizacion { get; set; }

        public bool EsCotizacion { get; set; }

        public List<ProductoPresupuesto> ProductosPresupuesto { get; set; } = new();

        public decimal TotalProductos { get; set; }

        public decimal PrecioTotal { get; set; }
    }
}
