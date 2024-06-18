using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
namespace Javo2.ViewModels.Operaciones.Productos
{
    public class ProductosViewModel
    {

        public int ProductoID { get; set; } = 0; // Auto-incremental y no editable


        [Display(Name = "ID Alfa del Producto")]
        public string ProductoIDAlfa { get; set; } = string.Empty; // No editable y autogenerado

        [StringLength(100)]
        [Display(Name = "Código de Barra")]
        public string CodBarra { get; set; } = string.Empty; // No editable y autogenerado

        [Required]
        [StringLength(100)]
        [Display(Name = "Nombre del Producto")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        [Display(Name = "Precio de Costo")]
        public decimal PCosto { get; set; } = 0;

        [Range(0, double.MaxValue)]
        [Display(Name = "Precio de Contado")]
        public decimal PContado { get; set; } = 0;

        [Range(0, double.MaxValue)]
        [Display(Name = "Precio de Lista")]
        public decimal PLista { get; set; } = 0;

        [Range(0, 100)]
        [Display(Name = "Porcentaje de IVA")]
        public decimal PorcentajeIva { get; set; } = 21; // Siempre es 21

        [Display(Name = "Fecha de Modificación")]
        public DateTime FechaMod { get; set; }

        [Display(Name = "Fecha de Modificación de Precio")]
        public DateTime FechaModPrecio { get; set; }

        [Display(Name = "Entregable")]
        public bool Entregable { get; set; } = false; // Checkbox

        [Range(0, int.MaxValue)]
        [Display(Name = "Cantidad en Stock")]
        public int CantidadStock { get; set; } = 0;

        [Display(Name = "No Listar")]
        public bool NoListar { get; set; } = false; // Checkbox

        [Display(Name = "Fecha de Stock")]
        public DateTime FechaStock { get; set; }

        [StringLength(100)]
        [Display(Name = "Usuario")]
        public string Usuario { get; set; } = "cosmefulanito"; // No editable

        [StringLength(500)]
        [Display(Name = "Comentario de Estado")]
        public string EstadoComentario { get; set; } = "test"; // No editable

        [Range(0, double.MaxValue)]
        [Display(Name = "Deuda Total")]
        public decimal DeudaTotal { get; set; } = 0; // No editable

        [StringLength(100)]
        [Display(Name = "Modificado Por")]
        public string ModificadoPor { get; set; } = "cosmefulanito"; // No editable

        [Display(Name = "Rubros")]
        public IEnumerable<SelectListItem> Rubros { get; set; } = new List<SelectListItem>();

        [Required]
        [Display(Name = "Rubro Seleccionado")]
        public int SelectedRubroId { get; set; }

        [Display(Name = "SubRubros")]
        public IEnumerable<SelectListItem> SubRubros { get; set; } = new List<SelectListItem>();

        [Required]
        [Display(Name = "SubRubro Seleccionado")]
        public int SelectedSubRubroId { get; set; }

        [Display(Name = "Marcas")]
        public IEnumerable<SelectListItem> Marcas { get; set; } = new List<SelectListItem>();

        [Required]
        [Display(Name = "Marca Seleccionada")]
        public int SelectedMarcaId { get; set; }

        // Propiedad para almacenar el nombre de la marca
        [StringLength(100)]
        [Display(Name = "Marca")]
        public string Marca { get; set; } = string.Empty;
    }
}