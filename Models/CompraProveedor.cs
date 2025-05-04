// Models/CompraProveedor.cs
using System;
using System.Collections.Generic;

namespace Javo2.Models
{
    public class CompraProveedor
    {
        public int CompraID { get; set; }
        public int ProveedorID { get; set; }
        public Proveedor Proveedor { get; set; } = new Proveedor();
        public DateTime FechaCompra { get; set; } = DateTime.Now;
        public string NumeroFactura { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;

        // Detalles de los productos
        public List<DetalleCompraProveedor> ProductosCompra { get; set; } = new List<DetalleCompraProveedor>();

        // Información de pago
        public int FormaPagoID { get; set; }
        public int? BancoID { get; set; }
        public string? TipoTarjeta { get; set; }
        public int? Cuotas { get; set; }
        public string? EntidadElectronica { get; set; }
        public DateTime? FechaVencimiento { get; set; }

        // Datos de cheque
        public decimal? MontoCheque { get; set; }
        public string? NumeroCheque { get; set; }

        // Totales
        public decimal PrecioTotal { get; set; }
        public int TotalProductos { get; set; }

        // Observaciones
        public string Observaciones { get; set; } = string.Empty;

        // Estados de la compra
        public EstadoCompra Estado { get; set; } = EstadoCompra.Pendiente;
    }

    public class DetalleCompraProveedor
    {
        public int DetalleCompraID { get; set; }
        public int CompraID { get; set; }
        public int ProductoID { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal PrecioTotal { get; set; }
    }

    public enum EstadoCompra
    {
        Pendiente,
        Procesada,
        Completada,
        Cancelada
    }
}