using System;

namespace Javo2.Models
{
    public class MovimientoStock
    {
        public int MovimientoStockID { get; set; }
        public int ProductoID { get; set; }
        public DateTime Fecha { get; set; }
        public string TipoMovimiento { get; set; } = string.Empty; // "Entrada", "Salida", "Ajuste"
        public int Cantidad { get; set; }
        public string Motivo { get; set; } = string.Empty;
    }
}
