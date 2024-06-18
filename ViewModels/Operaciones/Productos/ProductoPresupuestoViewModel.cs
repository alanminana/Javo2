using System.ComponentModel.DataAnnotations;

namespace Javo2.ViewModels.Operaciones.Productos
{
    public class ProductoPresupuestoViewModel
    {
        [Required]
        public string Codigo { get; set; } = string.Empty;

        [Required]
        public string CodigoAlfa { get; set; } = string.Empty;

        [Required]
        public string Detalle { get; set; } = string.Empty;

        public int Cantidad { get; set; }

        public int Cuotas { get; set; }

        public decimal ImporteCuotaSinInteres { get; set; }

        public string Marca { get; set; } = string.Empty;

        public decimal PrecioLista { get; set; }

        public decimal PrecioTotal { get; set; }
    }
}
