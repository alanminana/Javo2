// Archivo: ViewModels/ProductosViewModel.cs
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Javo2.ViewModels.Operaciones.Productos
{
    public class ProductosViewModel
    {
        public ProductosViewModel()
        {
            Rubros = new List<SelectListItem>();
            SubRubros = new List<SelectListItem>();
            Marcas = new List<SelectListItem>();
        }

        public int ProductoID { get; set; } = 0; // Auto-incremental y no editable

        [Display(Name = "ID Alfa del Producto")]
        public string ProductoIDAlfa { get; set; } = string.Empty; // No editable y autogenerado

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

        [Display(Name = "Rubros")]
        public IEnumerable<SelectListItem> Rubros { get; set; }

        [Required]
        [Display(Name = "Rubro Seleccionado")]
        public int SelectedRubroId { get; set; }

        [Display(Name = "SubRubros")]
        public IEnumerable<SelectListItem> SubRubros { get; set; }

        [Required]
        [Display(Name = "SubRubro Seleccionado")]
        public int SelectedSubRubroId { get; set; }

        [Display(Name = "Marcas")]
        public IEnumerable<SelectListItem> Marcas { get; set; }

        [Required]
        [Display(Name = "Marca Seleccionada")]
        public int SelectedMarcaId { get; set; }
    }
}
