// Models/AjustePrecioHistorico.cs
using System;
using System.Collections.Generic;

namespace Javo2.Models
{
    /// <summary>
    /// Representa un registro histórico de un ajuste de precios, ya sea permanente o temporal.
    /// </summary>
    public class AjustePrecioHistorico
    {
        /// <summary>
        /// ID único del ajuste histórico.
        /// </summary>
        public int AjusteHistoricoID { get; set; }

        /// <summary>
        /// Fecha y hora en que se creó el ajuste.
        /// </summary>
        public DateTime FechaAjuste { get; set; }

        /// <summary>
        /// Usuario que creó el ajuste.
        /// </summary>
        public string UsuarioAjuste { get; set; } = string.Empty;

        /// <summary>
        /// Porcentaje de ajuste aplicado.
        /// </summary>
        public decimal Porcentaje { get; set; }

        /// <summary>
        /// Indica si el ajuste es un aumento (true) o un descuento (false).
        /// </summary>
        public bool EsAumento { get; set; }

        /// <summary>
        /// Descripción opcional del ajuste.
        /// </summary>
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// Detalles de los productos afectados por el ajuste.
        /// </summary>
        public ICollection<AjustePrecioDetalle> Detalles { get; set; } = new List<AjustePrecioDetalle>();

        /// <summary>
        /// Indica si el ajuste ha sido revertido.
        /// </summary>
        public bool Revertido { get; set; }

        /// <summary>
        /// Fecha y hora en que el ajuste fue revertido, si aplica.
        /// </summary>
        public DateTime? FechaReversion { get; set; }

        /// <summary>
        /// Usuario que revirtió el ajuste, si aplica.
        /// </summary>
        public string UsuarioReversion { get; set; } = string.Empty;

        // Propiedades específicas para ajustes temporales

        /// <summary>
        /// Indica si el ajuste es temporal (con fechas de inicio y fin) o permanente.
        /// </summary>
        public bool EsTemporal { get; set; }

        /// <summary>
        /// Fecha y hora de inicio del período de validez del ajuste temporal.
        /// </summary>
        public DateTime? FechaInicio { get; set; }

        /// <summary>
        /// Fecha y hora de finalización del período de validez del ajuste temporal.
        /// </summary>
        public DateTime? FechaFin { get; set; }

        /// <summary>
        /// Tipo o categoría del ajuste temporal (ej: "Promoción", "Hot Sale", "Liquidación").
        /// </summary>
        public string TipoAjusteTemporal { get; set; } = string.Empty;

        /// <summary>
        /// Estado actual del ajuste temporal.
        /// </summary>
        public EstadoAjusteTemporal EstadoTemporal { get; set; } = EstadoAjusteTemporal.NoAplica;
    }

    /// <summary>
    /// Representa los detalles de un producto específico afectado por un ajuste de precios.
    /// </summary>
    public class AjustePrecioDetalle
    {
        /// <summary>
        /// ID único del detalle de ajuste.
        /// </summary>
        public int DetalleID { get; set; }

        /// <summary>
        /// ID del ajuste histórico al que pertenece este detalle.
        /// </summary>
        public int AjusteHistoricoID { get; set; }

        /// <summary>
        /// ID del producto afectado.
        /// </summary>
        public int ProductoID { get; set; }

        /// <summary>
        /// Nombre del producto para referencia.
        /// </summary>
        public string NombreProducto { get; set; } = string.Empty;

        /// <summary>
        /// Precio de costo antes del ajuste.
        /// </summary>
        public decimal PCostoAnterior { get; set; }

        /// <summary>
        /// Precio de contado antes del ajuste.
        /// </summary>
        public decimal PContadoAnterior { get; set; }

        /// <summary>
        /// Precio de lista antes del ajuste.
        /// </summary>
        public decimal PListaAnterior { get; set; }

        /// <summary>
        /// Precio de costo después del ajuste.
        /// </summary>
        public decimal PCostoPosterior { get; set; }

        /// <summary>
        /// Precio de contado después del ajuste.
        /// </summary>
        public decimal PContadoPosterior { get; set; }

        /// <summary>
        /// Precio de lista después del ajuste.
        /// </summary>
        public decimal PListaPosterior { get; set; }
    }

    /// <summary>
    /// Define los posibles estados de un ajuste temporal.
    /// </summary>
    public enum EstadoAjusteTemporal
    {
        /// <summary>
        /// No aplica (para ajustes permanentes).
        /// </summary>
        NoAplica,

        /// <summary>
        /// El ajuste está programado pero aún no ha iniciado.
        /// </summary>
        Programado,

        /// <summary>
        /// El ajuste está actualmente vigente.
        /// </summary>
        Activo,

        /// <summary>
        /// El ajuste ha finalizado y los precios han vuelto a la normalidad.
        /// </summary>
        Finalizado
    }
}