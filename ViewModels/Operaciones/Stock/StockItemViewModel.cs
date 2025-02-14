// File: ViewModels/Operaciones/Stock/StockItemViewModel.cs
using System.Collections.Generic;

namespace Javo2.ViewModels.Operaciones.Stock
{
    public class StockItemViewModel
    {
        public int StockItemID { get; set; }
        public int ProductoID { get; set; }
        public string NombreProducto { get; set; } = string.Empty; // inicializado para evitar null
        public int CantidadDisponible { get; set; }
        public IEnumerable<MovimientoStockViewModel> Movimientos { get; set; } = new List<MovimientoStockViewModel>();
    }
}
