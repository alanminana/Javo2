// File: ViewModels/Operaciones/Promociones/PromocionViewModel.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace Javo2.ViewModels.Operaciones.Promociones
{
    public class PromocionViewModel
    {
        public int PromocionID { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        [Range(0, 100, ErrorMessage = "El porcentaje debe estar entre 0 y 100")]
        public decimal Porcentaje { get; set; }

        public bool EsAumento { get; set; }

        public int? RubroID { get; set; }
        public int? MarcaID { get; set; }
        public int? SubRubroID { get; set; }

        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }

        public bool Activa { get; set; } = true;
    }
}
