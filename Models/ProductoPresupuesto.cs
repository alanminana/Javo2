using System.ComponentModel.DataAnnotations;

namespace Javo2.Models
{
    public class ProductoPresupuesto
    {
        public int ProductoPresupuestoID { get; set; }
        public int VentaID { get; set; }

        public int ProductoID { get; set; }

        [StringLength(100)]
        public string NombreProducto { get; set; } = string.Empty;

        public string DescripcionProducto { get; set; } = string.Empty;

        public int Cantidad { get; set; }

        public decimal PrecioUnitario { get; set; }
        public decimal SubTotal { get; set; }
    }
}
