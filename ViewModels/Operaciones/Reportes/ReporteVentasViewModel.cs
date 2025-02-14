// File: ViewModels/Operaciones/Reportes/ReporteVentasViewModel.cs
using System;

namespace Javo2.ViewModels.Operaciones.Reportes
{
    public class ReporteVentasViewModel
    {
        public int VentaID { get; set; }
        public DateTime FechaVenta { get; set; }
        public string NumeroFactura { get; set; } = string.Empty;
        public string NombreCliente { get; set; } = string.Empty;
        public decimal PrecioTotal { get; set; }
        public string Estado { get; set; } = string.Empty;
    }
}
