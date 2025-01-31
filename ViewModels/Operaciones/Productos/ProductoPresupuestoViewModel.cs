// Archivo: ViewModels/Operaciones/Ventas/ProductoPresupuestoViewModel.cs
namespace Javo2.ViewModels.Operaciones.Ventas
{
    public class ProductoPresupuestoViewModel
    {
        public int ProductoID { get; set; }
        public string CodigoBarra { get; set; }
        public string CodigoAlfa { get; set; }
        public string NombreProducto { get; set; }
        public string Marca { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal PrecioTotal { get; set; }
        public decimal PrecioLista { get; set; }
    }
}
