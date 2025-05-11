// Models/Cotizacion.cs
using System;
using System.Collections.Generic;

namespace Javo2.Models
{
    public class Cotizacion
    {
        public int CotizacionID { get; set; }
        public DateTime FechaCotizacion { get; set; }

        // Nuevo: días de vigencia desde la fecha de cotización
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

        // Si necesitas añadir dirección:
        // public string DireccionCliente { get; set; } = string.Empty;

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