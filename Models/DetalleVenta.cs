// Models/DetalleVenta.cs
namespace Javo2.Models
{
    public class DetalleVenta
    {
        public int ProductoID { get; set; }
        public string CodigoAlfa { get; set; } = string.Empty;
        public string CodigoBarra { get; set; } = string.Empty;
        public string NombreProducto { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal PrecioTotal { get; set; }
        public decimal PrecioLista { get; set; }
    }
}
