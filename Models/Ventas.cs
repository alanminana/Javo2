namespace Javo2.Models
{
    public enum EstadoVenta
    {
        Borrador,
        PendienteDeAutorizacion,
        PendienteDeEntrega,
        Completada,
        Finalizado,
        Cancelada
    }

    public class Venta
    {
        public int VentaID { get; set; }
        public DateTime FechaVenta { get; set; }
        public string NumeroFactura { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public string Vendedor { get; set; } = string.Empty;

        // Datos del Cliente
        public int ClienteID { get; set; }
        public string NombreCliente { get; set; } = string.Empty;
        public string TelefonoCliente { get; set; } = string.Empty;
        public string DomicilioCliente { get; set; } = string.Empty;
        public string LocalidadCliente { get; set; } = string.Empty;
        public string CelularCliente { get; set; } = string.Empty;
        public decimal LimiteCreditoCliente { get; set; }
        public decimal SaldoCliente { get; set; }
        public decimal SaldoDisponibleCliente { get; set; }

        // Forma de Pago
        public int FormaPagoID { get; set; }
        public FormaPago? FormaPago { get; set; }
        public int? BancoID { get; set; }
        public Banco? Banco { get; set; }
        public string EntidadElectronica { get; set; } = string.Empty;
        public string PlanFinanciamiento { get; set; } = string.Empty;
        public decimal AdelantoDinero { get; set; }
        public decimal DineroContado { get; set; }
        public decimal MontoCheque { get; set; }
        public string NumeroCheque { get; set; } = string.Empty;

        // Otros
        public string Observaciones { get; set; } = string.Empty;
        public string Condiciones { get; set; } = string.Empty;
        public decimal Credito { get; set; }

        // Estado de la Venta
        public EstadoVenta Estado { get; set; }
        public string EstadoEntrega { get; set; } = string.Empty;

        // Detalles de la Venta
        public List<DetalleVenta> ProductosPresupuesto { get; set; } = new List<DetalleVenta>();

        // Totales
        public decimal PrecioTotal { get; set; }

        // Auditoría
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime? FechaModificacion { get; set; }
    }
}
