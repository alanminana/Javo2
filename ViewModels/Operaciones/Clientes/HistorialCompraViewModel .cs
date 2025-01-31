// Archivo: ViewModels/Operaciones/Clientes/HistorialCompraViewModel.cs
namespace Javo2.ViewModels.Operaciones.Clientes
{
    public class HistorialCompraViewModel
    {
        public int CompraID { get; set; }
        public string Producto { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Total { get; set; }
        public string FechaCompra { get; set; }
    }
}
