// Models/ConfiguracionCredito.cs
using System;
using System.Collections.Generic;

namespace Javo2.Models
{
    public class ConfiguracionCredito
    {
        public int ConfiguracionCreditoID { get; set; }

        // Parámetros generales
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;

        // Parámetros de recargo
        public decimal PorcentajeRecargoBase { get; set; } = 0;  // Recargo base para todos los créditos

        // Recargos por calificación
        public decimal RecargoScoreA { get; set; } = 0;  // Excelente
        public decimal RecargoScoreB { get; set; } = 0;  // Bueno
        public decimal RecargoScoreC { get; set; } = 0;  // Regular
        public decimal RecargoScoreD { get; set; } = 0;  // Malo

        // Interés por mora
        public decimal PorcentajeMoraDiaria { get; set; } = 0;
        public decimal PorcentajeMoraMensual { get; set; } = 0;

        // Plazos máximos permitidos
        public int PlazoMaximoScoreA { get; set; } = 24;  // Meses máximos para Score A
        public int PlazoMaximoScoreB { get; set; } = 18;  // Meses máximos para Score B
        public int PlazoMaximoScoreC { get; set; } = 12;  // Meses máximos para Score C
        public int PlazoMaximoScoreD { get; set; } = 6;   // Meses máximos para Score D

        // Días de gracia antes de incurrir en mora
        public int DiasGracia { get; set; } = 5;

        // Días antes de trasladar responsabilidad al garante
        public int DiasTraspasarGarante { get; set; } = 30;

        // Datos de auditoría
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaModificacion { get; set; }
        public string ModificadoPor { get; set; } = string.Empty;
    }
}