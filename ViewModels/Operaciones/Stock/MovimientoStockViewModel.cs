using System;

namespace Javo2.ViewModels.Operaciones.Stock
{
    public class MovimientoStockViewModel
    {
        public int MovimientoID { get; set; }
        public string TipoMovimiento { get; set; }
        public int Cantidad { get; set; }
        public DateTime Fecha { get; set; }
        public string Motivo { get; set; }
    }
}
