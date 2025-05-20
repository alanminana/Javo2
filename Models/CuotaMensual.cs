// Models/CuotaMensual.cs
using System;

namespace Javo2.Models
{
    public enum EstadoCuotaMensual
    {
        Pendiente,
        Pagada,
        Vencida,
        EnMora,
        AsignadaGarante,
        PagadaPorGarante
    }

    public class CuotaMensual
    {
        public int CuotaID { get; set; }

        // Relaciones
        public int VentaID { get; set; }
        public Venta Venta { get; set; }
        public int ClienteID { get; set; }
        public Cliente Cliente { get; set; }
        public int? GaranteID { get; set; }
        public Garante Garante { get; set; }

        // Datos de la cuota
        public int NumeroCuota { get; set; }  // 1, 2, 3, etc.
        public decimal MontoCapital { get; set; }  // Porción del capital
        public decimal MontoInteres { get; set; }  // Interés calculado
        public decimal MontoTotal { get; set; }    // Capital + Interés

        // Fechas
        public DateTime FechaVencimiento { get; set; }
        public DateTime? FechaPago { get; set; }

        // Estado
        public EstadoCuota Estado { get; set; } = EstadoCuota.Pendiente;

        // Mora
        public int? DiasAtraso { get; set; }
        public decimal? MontoMora { get; set; }
        public decimal? MontoTotalConMora => MontoTotal + (MontoMora ?? 0);

        // Pago
        public bool Pagada => Estado == EstadoCuota.Pagada || Estado == EstadoCuota.PagadaPorGarante;
        public decimal? MontoPagado { get; set; }
        public string FormaPago { get; set; } = string.Empty;
        public string ReferenciaPago { get; set; } = string.Empty;

        // Datos de auditoría
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaModificacion { get; set; }
        public string ModificadoPor { get; set; } = string.Empty;
        public string Comentarios { get; set; } = string.Empty;
    }
}