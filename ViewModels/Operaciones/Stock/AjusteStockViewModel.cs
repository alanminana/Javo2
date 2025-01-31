using System.ComponentModel.DataAnnotations;

namespace Javo2.ViewModels.Operaciones.Stock
{
    public class AjusteStockViewModel
    {
        public int ProductoID { get; set; }
        public int CantidadActual { get; set; }

        [Required]
        [Display(Name = "Nueva Cantidad")]
        public int NuevaCantidad { get; set; }

        [Required]
        [Display(Name = "Motivo")]
        public string Motivo { get; set; }
    }
}
