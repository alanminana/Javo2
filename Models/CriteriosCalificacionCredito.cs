// Models/CriteriosCalificacionCredito.cs
using System;

namespace Javo2.Models
{
    public class CriteriosCalificacionCredito
    {
        public int CriterioID { get; set; }

        // Definición
        public string ScoreCredito { get; set; } = string.Empty; // Debe coincidir con los valores en Cliente

        public string Descripcion { get; set; } = string.Empty;

        // Crédito
        public bool AptoCredito { get; set; } = true;
        public bool RequiereGarante { get; set; } = false;
        public decimal MontoMaximoSinGarante { get; set; } = 0;

        // Límites
        public decimal LimiteCreditoMaximo { get; set; } = 0;
        public decimal LimiteCreditoRecomendado { get; set; } = 0;

        // Plazos
        public int PlazoMaximo { get; set; } = 12;  // En meses
        public int PlazoRecomendado { get; set; } = 6;  // En meses

        // Datos de auditoría
        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaModificacion { get; set; }
        public string ModificadoPor { get; set; } = string.Empty;
    }
}