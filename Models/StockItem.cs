namespace Javo2.Models
{
    public class StockItem
    {
        public int StockItemID { get; set; }
        public int ProductoID { get; set; }
        public int CantidadDisponible { get; set; }
        public Producto Producto { get; set; }
    }
}
