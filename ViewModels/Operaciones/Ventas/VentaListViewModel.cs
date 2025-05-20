// ViewModels/Operaciones/Ventas/VentaListViewModel.cs
using System;

namespace Javo2.ViewModels.Operaciones.Ventas
{
    public class VentaListViewModel
    {
        public int VentaID { get; set; }
        public DateTime FechaVenta { get; set; }
        public string NumeroFactura { get; set; } = string.Empty;
        public string NombreCliente { get; set; } = string.Empty;
        public decimal PrecioTotal { get; set; }
        public string Estado { get; set; } = string.Empty;
        public decimal TotalProductos { get; set; }
        public string EstadoEntrega { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;

        // Agregamos propiedades del cliente que faltan
        public string DomicilioCliente { get; set; } = string.Empty;
        public string TelefonoCliente { get; set; } = string.Empty;
        public string LocalidadCliente { get; set; } = string.Empty;
        public string CelularCliente { get; set; } = string.Empty;
        public bool EsCredito { get; set; } = false;
        public string EstadoCredito { get; set; } = "Sin iniciar";

    }
}