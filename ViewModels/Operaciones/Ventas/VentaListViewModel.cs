// ViewModels/Operaciones/Ventas/VentaListViewModel.cs
using System;

namespace Javo2.ViewModels.Operaciones.Ventas
{
    public class VentaListViewModel
    {
        public int VentaID { get; set; }
        public DateTime FechaVenta { get; set; }
        public string NumeroFactura { get; set; }
        public string NombreCliente { get; set; }
        public decimal PrecioTotal { get; set; }
        public string Estado { get; set; }
        public decimal TotalProductos { get; set; }
        public string EstadoEntrega { get; set; }
        public string Usuario { get; set; }
        public string DomicilioCliente { get; set; }



    }
}
