// Archivo: ViewModels/Operaciones/Ventas/DetalleVentaViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace Javo2.ViewModels.Operaciones.Ventas
{
    public class DetalleVentaViewModel
    {
        public int ProductoID { get; set; } = 0;  // Evitamos '' en lugar de 0

        public string CodigoAlfa { get; set; } = string.Empty;
        public string CodigoBarra { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string NombreProducto { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser al menos 1.")]
        public int Cantidad { get; set; } = 1;

        [Range(0, double.MaxValue, ErrorMessage = "El precio unitario no puede ser negativo.")]
        public decimal PrecioUnitario { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El precio de lista no puede ser negativo.")]
        public decimal PrecioLista { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El precio total no puede ser negativo.")]
        public decimal PrecioTotal { get; set; }
    }
}
