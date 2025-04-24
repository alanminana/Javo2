// Models/DevolucionGarantia.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Javo2.Models
{
    public enum TipoCaso
    {
        Devolucion,
        Cambio,
        Garantia,
        Reparacion
    }

    public enum EstadoCaso
    {
        Pendiente,
        EnProceso,
        Completado,
        Rechazado
    }

    public class DevolucionGarantia
    {
        public int DevolucionGarantiaID { get; set; }

        [Display(Name = "Venta")]
        public int VentaID { get; set; }
        public Venta Venta { get; set; }

        [Display(Name = "Cliente")]
        public string NombreCliente { get; set; } = string.Empty;

        [Display(Name = "Fecha de Solicitud")]
        public DateTime FechaSolicitud { get; set; } = DateTime.Now;

        [Display(Name = "Tipo de Caso")]
        public TipoCaso TipoCaso { get; set; }

        [Display(Name = "Motivo")]
        public string Motivo { get; set; } = string.Empty;

        [Display(Name = "Descripción")]
        public string Descripcion { get; set; } = string.Empty;

        [Display(Name = "Estado")]
        public EstadoCaso Estado { get; set; } = EstadoCaso.Pendiente;

        [Display(Name = "Usuario")]
        public string Usuario { get; set; } = string.Empty;

        [Display(Name = "Comentarios")]
        public string Comentarios { get; set; } = string.Empty;

        [Display(Name = "Fecha Resolución")]
        public DateTime? FechaResolucion { get; set; }

        // Items de devolución/garantía
        public List<ItemDevolucionGarantia> Items { get; set; } = new List<ItemDevolucionGarantia>();

        // Para el caso de cambio de producto
        public List<CambioProducto> CambiosProducto { get; set; } = new List<CambioProducto>();
    }

    public class ItemDevolucionGarantia
    {
        public int ItemDevolucionGarantiaID { get; set; }
        public int DevolucionGarantiaID { get; set; }
        public DevolucionGarantia DevolucionGarantia { get; set; }

        public int ProductoID { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public bool ProductoDanado { get; set; }

        [Display(Name = "Estado del Producto")]
        public string EstadoProducto { get; set; } = string.Empty;
    }

    public class CambioProducto
    {
        public int CambioProductoID { get; set; }
        public int DevolucionGarantiaID { get; set; }
        public DevolucionGarantia DevolucionGarantia { get; set; }

        public int ProductoOriginalID { get; set; }
        public string NombreProductoOriginal { get; set; } = string.Empty;

        public int ProductoNuevoID { get; set; }
        public string NombreProductoNuevo { get; set; } = string.Empty;

        public int Cantidad { get; set; }
        public decimal DiferenciaPrecio { get; set; } // Positivo si el cliente paga, negativo si se reembolsa
    }
}