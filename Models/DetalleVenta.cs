namespace Javo2.Models
{
    public class DetalleVenta
    {
        public int DetalleVentaID { get; set; }
        public int VentaID { get; set; }
        public Venta Venta { get; set; }

        public int ProductoID { get; set; }
        public Producto Producto { get; set; }
        public string CodigoAlfa { get; set; }
        public string CodigoBarra { get; set; }

        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal PrecioTotal { get; set; }
    }
}
