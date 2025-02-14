// File: ViewModels/Operaciones/Reportes/ReporteStockViewModel.cs
namespace Javo2.ViewModels.Operaciones.Reportes
{
    public class ReporteStockViewModel
    {
        public int ProductoID { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int StockDisponible { get; set; }
    }
}
