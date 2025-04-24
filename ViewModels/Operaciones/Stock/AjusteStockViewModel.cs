using System.ComponentModel.DataAnnotations;

namespace Javo2.ViewModels.Operaciones.Stock
{
    public class AjusteStockViewModel
    {
        public int ProductoID { get; set; }

        [Display(Name = "Cantidad Actual")]
        public int CantidadActual { get; set; }

        [Required(ErrorMessage = "La nueva cantidad es obligatoria")]
        [Display(Name = "Nueva Cantidad")]
        public int NuevaCantidad { get; set; }

        [Required(ErrorMessage = "El motivo del ajuste es obligatorio")]
        [Display(Name = "Motivo del Ajuste")]
        [StringLength(500, ErrorMessage = "El motivo no puede superar los 500 caracteres")]
        public string Motivo { get; set; } = string.Empty;
    }
}