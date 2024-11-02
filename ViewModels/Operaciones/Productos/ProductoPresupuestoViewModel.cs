// Archivo: ViewModels/Operaciones/Ventas/ProductoPresupuestoViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace Javo2.ViewModels.Operaciones.Ventas
{
    public class ProductoPresupuestoViewModel
    {
        public int ProductoPresupuestoID { get; set; }

        public int VentaID { get; set; }

        public int ProductoID { get; set; }

        [Required]
        [StringLength(100)]
        public string NombreProducto { get; set; } = string.Empty;

        public string DescripcionProducto { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue)]
        public int Cantidad { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal PrecioUnitario { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal SubTotal { get; set; }
    }
}
