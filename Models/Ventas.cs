// File: Models/Ventas.cs
using System;
using System.Collections.Generic;

namespace Javo2.Models
{
    public enum EstadoVenta
    {
        Borrador,
        PendienteDeAutorizacion,
        Autorizada,
        Rechazada,
        PendienteDeEntrega,
        Completada
    }

    public class Venta
    {
        public int VentaID { get; set; }
        public DateTime FechaVenta { get; set; }
        public string NumeroFactura { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public string Vendedor { get; set; } = string.Empty;

        // Datos del Cliente (se usan propiedades existentes)
        public int DniCliente { get; set; }
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
        // Para promociones, se usa un listado (ya definido en PromocionAplicada)
        public IEnumerable<PromocionAplicada> PromocionesAplicadas { get; set; } = new List<PromocionAplicada>();

        public int? BancoID { get; set; }
        public string TipoTarjeta { get; set; } = string.Empty;
        public int? Cuotas { get; set; }
        public string EntidadElectronica { get; set; } = string.Empty;
        public string PlanFinanciamiento { get; set; } = string.Empty;

        // Otros datos
        public string Observaciones { get; set; } = string.Empty;
        public string Condiciones { get; set; } = string.Empty;
        public decimal Credito { get; set; }

        // Productos de la venta (o presupuesto)
        public List<DetalleVenta> ProductosPresupuesto { get; set; } = new List<DetalleVenta>();

        // Totales
        public decimal PrecioTotal { get; set; }
        public int TotalProductos { get; set; }

        // Anticipo / Cheques
        public decimal? AdelantoDinero { get; set; }
        public decimal? DineroContado { get; set; }
        public decimal? MontoCheque { get; set; }
        public string NumeroCheque { get; set; } = string.Empty;

        // Estado
        public EstadoVenta Estado { get; set; } = EstadoVenta.Borrador;

        // Para ventas a crédito
        public List<Cuota> CuotasPagas { get; set; } = new List<Cuota>();

        // Estados de entrega por producto (opcional, si se usa)
        public List<EstadoEntregaProducto> EstadosEntregaProductos { get; set; } = new List<EstadoEntregaProducto>();
    }

    public class PromocionAplicada
    {
        public int PromocionID { get; set; }
        public string NombrePromocion { get; set; } = string.Empty;
        public decimal Porcentaje { get; set; }
        public bool EsAumento { get; set; }
    }

    public class EstadoEntregaProducto
    {
        public int ProductoID { get; set; }
        public EstadoFlujo EstadoFlujo { get; set; }
        public DateTime FechaEstado { get; set; }
    }

    public enum EstadoFlujo
    {
        Reservado,
        Embalaje,
        Enviado,
        Entregado
    }

    public class Cuota
    {
        public int CuotaID { get; set; }
        public int VentaID { get; set; }
        public int NumeroCuota { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public decimal ImporteCuota { get; set; }
        public DateTime? FechaPago { get; set; }
        public decimal ImportePagado { get; set; }
        public EstadoCuota EstadoCuota { get; set; }
    }

    public enum EstadoCuota
    {
        Pendiente,
        Pagada,
        Vencida
    }
}
