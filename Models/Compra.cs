using System;

namespace Javo2.Models
{
    public class Compra
    {
        public int CompraID { get; set; }
        public string Producto { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Total { get; set; }
        public DateTime FechaCompra { get; set; }

        // Clave Foránea
        public int ClienteID { get; set; }
        public Cliente Cliente { get; set; }
    }
}