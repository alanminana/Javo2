// File: Models/StockItem.cs
namespace Javo2.Models
{
    public class StockItem
    {
        public int StockItemID { get; set; }
        public int ProductoID { get; set; }
        public int CantidadDisponible { get; set; }
        // Se asume que se establecerá la referencia a Producto desde el servicio
        public Producto Producto { get; set; } = new Producto();
    }
}
