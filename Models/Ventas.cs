// File: Models/Ventas.cs
using DocumentFormat.OpenXml.Bibliography;
using System;
using System.Collections.Generic;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

        // Agregar nuevas propiedades
        public bool EsCredito { get; set; } = false;
        public decimal TotalSinRecargo { get; set; }
        public decimal PorcentajeRecargo { get; set; } = 0;
        // Ya existe: public decimal PrecioTotal { get; set; }
        public decimal MontoRecargo => PrecioTotal - TotalSinRecargo;
        public string EstadoCredito { get; set; } = "Sin iniciar";
        public decimal SaldoPendiente { get; set; }
        public bool CreditoCancelado { get; set; } = false;
        public DateTime? FechaCancelacion { get; set; }
        public decimal TotalCapitalPagado { get; set; } = 0;
        public decimal TotalInteresPagado { get; set; } = 0;
        public decimal TotalPagado => TotalCapitalPagado + TotalInteresPagado;
        public decimal PorcentajePagado => TotalSinRecargo > 0 ? (TotalCapitalPagado / TotalSinRecargo) * 100 : 0;
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

        public int ClienteID { get; set; }
        public int? GaranteID { get; set; }
        public decimal MontoCapital { get; set; }
        public decimal MontoInteres { get; set; }
        public int? DiasAtraso { get; set; }
        public decimal? MontoMora { get; set; }
        public decimal? MontoTotalConMora => ImporteCuota + (MontoMora ?? 0);
        public string FormaPago { get; set; } = string.Empty;
        public string ReferenciaPago { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaModificacion { get; set; }
        public string ModificadoPor { get; set; } = string.Empty;
        public string Comentarios { get; set; } = string.Empty;
        public bool Pagada => EstadoCuota == EstadoCuota.Pagada;

        public EstadoCuota EstadoCuota { get; set; }

    }

    public enum EstadoCuota
    {
        Pendiente,
        Pagada,
        Vencida,
        EnMora,           // Añadido
        AsignadaGarante,  // Añadido
        PagadaPorGarante  // Añadido
    }
}
