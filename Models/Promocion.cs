using System;

namespace Javo2.Models
{
    public class Promocion
    {
        public int PromocionID { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;

        // Porcentaje que se aplica al producto (puede ser aumento o descuento)
        // Por ejemplo: 10 significa +10% si es aumento, o -10% si es descuento.
        public decimal Porcentaje { get; set; }

        // Indica si es aumento (true) o descuento (false)
        public bool EsAumento { get; set; }

        // Filtros opcionales: si se aplica a Rubro, Marca o SubRubro
        public int? RubroID { get; set; }
        public int? MarcaID { get; set; }
        public int? SubRubroID { get; set; }

        // Fechas de vigencia (opcional)
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }

        // Indica si la promoción está activa
        public bool Activa { get; set; } = true;
    }
}
