using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Javo2.ViewModels.Operaciones.Productos
{
    #region Product Models

    /// <summary>
    /// Representa un producto para selección en ajustes de precios
    /// </summary>
    public class ProductoAjusteViewModel
    {
        public int ProductoID { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal PCosto { get; set; }
        public decimal PContado { get; set; }
        public decimal PLista { get; set; }
        public bool Seleccionado { get; set; }
    }

    #endregion

    #region Permanent Price Adjustment

    /// <summary>
    /// Modelo para el formulario de ajuste de precios permanente
    /// </summary>
    public class AjustePrecioFormViewModel
    {
        [Required(ErrorMessage = "El porcentaje es obligatorio")]
        [Range(0.01, 100, ErrorMessage = "El porcentaje debe estar entre 0.01 y 100")]
        [Display(Name = "Porcentaje")]
        public decimal Porcentaje { get; set; }

        [Display(Name = "Tipo de Ajuste")]
        public bool EsAumento { get; set; } = true;

        [Display(Name = "Descripción")]
        [StringLength(250, ErrorMessage = "La descripción no puede exceder los 250 caracteres")]
        public string Descripcion { get; set; } = string.Empty;

        public List<ProductoAjusteViewModel> Productos { get; set; } = new List<ProductoAjusteViewModel>();
    }

    #endregion

    #region Temporary Price Adjustment

    /// <summary>
    /// Modelo para el formulario de ajuste de precios temporal
    /// </summary>
    public class AjusteTemporalFormViewModel
    {
        [Required(ErrorMessage = "El porcentaje es obligatorio")]
        [Range(0.01, 100, ErrorMessage = "El porcentaje debe estar entre 0.01 y 100")]
        [Display(Name = "Porcentaje")]
        public decimal Porcentaje { get; set; }

        [Display(Name = "Tipo de Ajuste")]
        public bool EsAumento { get; set; } = false;

        [Required(ErrorMessage = "El tipo de ajuste es obligatorio")]
        [Display(Name = "Motivo del Ajuste")]
        public string TipoAjuste { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de inicio es obligatoria")]
        [Display(Name = "Fecha de Inicio")]
        public DateTime? FechaInicio { get; set; }

        [Required(ErrorMessage = "La fecha de finalización es obligatoria")]
        [Display(Name = "Fecha de Finalización")]
        public DateTime? FechaFin { get; set; }

        [Display(Name = "Descripción")]
        [StringLength(250, ErrorMessage = "La descripción no puede exceder los 250 caracteres")]
        public string Descripcion { get; set; } = string.Empty;

        public List<ProductoAjusteViewModel> Productos { get; set; } = new List<ProductoAjusteViewModel>();
        public List<SelectListItem> TiposDeAjuste { get; set; } = new List<SelectListItem>();
        public bool ForzarEstadoProgramado { get; set; } = true;
    }

    #endregion

    #region Detail Models

    /// <summary>
    /// Detalle de un ajuste de precio para un producto específico
    /// </summary>
    public class AjustePrecioDetalleViewModel
    {
        public int DetalleID { get; set; }
        public int AjusteHistoricoID { get; set; }
        public int ProductoID { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public decimal PCostoAnterior { get; set; }
        public decimal PContadoAnterior { get; set; }
        public decimal PListaAnterior { get; set; }
        public decimal PCostoPosterior { get; set; }
        public decimal PContadoPosterior { get; set; }
        public decimal PListaPosterior { get; set; }
        public decimal DiferenciaPCosto => PCostoPosterior - PCostoAnterior;
        public decimal PorcentajeCambio => PCostoAnterior > 0 ? (PCostoPosterior / PCostoAnterior) - 1 : 0;
    }

    /// <summary>
    /// Registro histórico de un ajuste de precios permanente
    /// </summary>
    public class AjustePrecioHistoricoViewModel
    {
        public int AjusteHistoricoID { get; set; }
        public DateTime FechaAjuste { get; set; }
        public string UsuarioAjuste { get; set; } = string.Empty;
        public decimal Porcentaje { get; set; }
        public bool EsAumento { get; set; }
        public string TipoAjuste => EsAumento ? "Aumento" : "Descuento";
        public string Descripcion { get; set; } = string.Empty;
        public List<AjustePrecioDetalleViewModel> Detalles { get; set; } = new List<AjustePrecioDetalleViewModel>();
        public int CantidadProductos => Detalles?.Count ?? 0;
        public bool Revertido { get; set; }
        public DateTime? FechaReversion { get; set; }
        public string UsuarioReversion { get; set; } = string.Empty;
    }

    /// <summary>
    /// Modelo para ver los detalles de un ajuste temporal
    /// </summary>
    public class AjusteTemporalViewModel
    {
        public int AjusteHistoricoID { get; set; }
        public DateTime FechaAjuste { get; set; }
        public string UsuarioAjuste { get; set; } = string.Empty;
        public decimal Porcentaje { get; set; }
        public bool EsAumento { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public List<AjustePrecioDetalleViewModel> Detalles { get; set; } = new List<AjustePrecioDetalleViewModel>();
        public bool Revertido { get; set; }
        public DateTime? FechaReversion { get; set; }
        public string UsuarioReversion { get; set; } = string.Empty;

        // Propiedades específicas para ajustes temporales
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string TipoAjusteTemporal { get; set; } = string.Empty;
        public string EstadoTemporal { get; set; } = string.Empty;

        // Propiedades calculadas
        public bool PuedeActivar => EstadoTemporal == "Programado" && !Revertido;
        public bool PuedeFinalizar => EstadoTemporal == "Activo" && !Revertido;
        public TimeSpan DuracionTotal => FechaFin.HasValue && FechaInicio.HasValue ? FechaFin.Value - FechaInicio.Value : TimeSpan.Zero;
        public int DiasRestantes => FechaFin.HasValue && DateTime.Now < FechaFin.Value ? (FechaFin.Value - DateTime.Now).Days : 0;
    }

    /// <summary>
    /// Modelo para la lista de ajustes temporales
    /// </summary>
    public class AjustesTemporalesIndexViewModel
    {
        public List<AjusteTemporalViewModel> AjustesActivos { get; set; } = new List<AjusteTemporalViewModel>();
        public List<AjusteTemporalViewModel> AjustesProgramados { get; set; } = new List<AjusteTemporalViewModel>();
        public List<AjusteTemporalViewModel> AjustesFinalizados { get; set; } = new List<AjusteTemporalViewModel>();
    }

    #endregion

    #region Simulation Models

    /// <summary>
    /// Modelo para simulación de ajuste de precios permanente
    /// </summary>
    public class SimulacionAjusteViewModel
    {
        public List<int> ProductoIDs { get; set; } = new List<int>();
        public decimal Porcentaje { get; set; }
        public bool EsAumento { get; set; }
    }

    /// <summary>
    /// Modelo para simulación de ajuste de precios temporal
    /// </summary>
    public class SimulacionAjusteTemporalViewModel : SimulacionAjusteViewModel
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string TipoAjuste { get; set; } = string.Empty;
    }

    #endregion
}