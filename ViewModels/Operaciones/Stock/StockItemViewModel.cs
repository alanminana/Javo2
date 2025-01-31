using System.Collections.Generic;

namespace Javo2.ViewModels.Operaciones.Stock
{
    public class StockItemViewModel
    {
        public int StockItemID { get; set; }
        public int ProductoID { get; set; }
        public string NombreProducto { get; set; }
        public int CantidadDisponible { get; set; }
        public IEnumerable<MovimientoStockViewModel> Movimientos { get; set; }
    }
}
