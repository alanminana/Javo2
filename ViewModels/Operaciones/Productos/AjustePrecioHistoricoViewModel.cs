using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

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
    public string TipoAjuste { get; set; }

    [Required(ErrorMessage = "La fecha de inicio es obligatoria")]
    [Display(Name = "Fecha de Inicio")]
    public DateTime? FechaInicio { get; set; }

    [Required(ErrorMessage = "La fecha de finalización es obligatoria")]
    [Display(Name = "Fecha de Finalización")]
    public DateTime? FechaFin { get; set; }

    [Display(Name = "Descripción")]
    [StringLength(250, ErrorMessage = "La descripción no puede exceder los 250 caracteres")]
    public string Descripcion { get; set; }

    public List<ProductoAjusteViewModel> Productos { get; set; } = new List<ProductoAjusteViewModel>();
    public List<SelectListItem> TiposDeAjuste { get; set; } = new List<SelectListItem>();
}

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
    public string Descripcion { get; set; }

    public List<ProductoAjusteViewModel> Productos { get; set; } = new List<ProductoAjusteViewModel>();
}

public class ProductoAjusteViewModel
{
    public int ProductoID { get; set; }
    public string Nombre { get; set; }
    public decimal PCosto { get; set; }
    public decimal PContado { get; set; }
    public decimal PLista { get; set; }
    public bool Seleccionado { get; set; }
}

public class SimulacionAjusteTemporalViewModel
{
    public List<int> ProductoIDs { get; set; }
    public decimal Porcentaje { get; set; }
    public bool EsAumento { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public string TipoAjuste { get; set; }
}

public class SimulacionAjusteViewModel
{
    public List<int> ProductoIDs { get; set; }
    public decimal Porcentaje { get; set; }
    public bool EsAumento { get; set; }
}

public class AjusteTemporalViewModel
{
    public int AjusteHistoricoID { get; set; }
    public DateTime FechaAjuste { get; set; }
    public string UsuarioAjuste { get; set; }
    public decimal Porcentaje { get; set; }
    public bool EsAumento { get; set; }
    public string Descripcion { get; set; }
    public List<AjustePrecioDetalleViewModel> Detalles { get; set; } = new List<AjustePrecioDetalleViewModel>();
    public bool Revertido { get; set; }
    public DateTime? FechaReversion { get; set; }
    public string UsuarioReversion { get; set; }

    // Propiedades específicas para ajustes temporales
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public string TipoAjusteTemporal { get; set; }
    public string EstadoTemporal { get; set; }

    public bool PuedeActivar => EstadoTemporal == "Programado" && !Revertido;
    public bool PuedeFinalizar => EstadoTemporal == "Activo" && !Revertido;
    public TimeSpan DuracionTotal => FechaFin.HasValue && FechaInicio.HasValue ? FechaFin.Value - FechaInicio.Value : TimeSpan.Zero;
    public int DiasRestantes => FechaFin.HasValue && DateTime.Now < FechaFin.Value ? (FechaFin.Value - DateTime.Now).Days : 0;
}

public class AjustesTemporalesIndexViewModel
{
    public List<AjusteTemporalViewModel> AjustesActivos { get; set; } = new List<AjusteTemporalViewModel>();
    public List<AjusteTemporalViewModel> AjustesProgramados { get; set; } = new List<AjusteTemporalViewModel>();
    public List<AjusteTemporalViewModel> AjustesFinalizados { get; set; } = new List<AjusteTemporalViewModel>();
}

public class ProductoAjusteViewModel
{
    public int ProductoID { get; set; }
    public string Nombre { get; set; }
    public decimal PCosto { get; set; }
    public decimal PContado { get; set; }
    public decimal PLista { get; set; }
    public bool Seleccionado { get; set; }
}