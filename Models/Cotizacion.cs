// Models/Cotizacion.cs
using System;
using System.Collections.Generic;

namespace Javo2.Models
{
    public class Cotizacion
    {
        public int CotizacionID { get; set; }
        public DateTime FechaCotizacion { get; set; }
        public int DiasVigencia { get; set; } = 15;
        public DateTime FechaVencimiento { get; set; }
        public string NumeroCotizacion { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public string EstadoCotizacion { get; set; } = "Borrador";

        // Datos del Cliente
        public int DniCliente { get; set; }
        public string NombreCliente { get; set; } = string.Empty;
        public string TelefonoCliente { get; set; } = string.Empty;
        public string EmailCliente { get; set; } = string.Empty;

        // Propiedades necesarias para mapeo con Venta
        public string DomicilioCliente { get; set; } = string.Empty;
        public string LocalidadCliente { get; set; } = string.Empty;
        public string CelularCliente { get; set; } = string.Empty;

        // Para compatibilidad con el mapeo
        public string NumeroFactura => NumeroCotizacion;
        public int VentaID => CotizacionID;

        // Productos cotizados
        public List<DetalleVenta> ProductosPresupuesto { get; set; } = new List<DetalleVenta>();

        // Totales
        public decimal PrecioTotal { get; set; }
        public int TotalProductos { get; set; }

        // Otros campos
        public string Observaciones { get; set; } = string.Empty;
        public bool Vigente => DateTime.Now <= FechaVencimiento;
    }
}