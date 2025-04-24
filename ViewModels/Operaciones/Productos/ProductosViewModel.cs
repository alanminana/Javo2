// ViewModels/Operaciones/Productos/ProductosViewModel.cs
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
            ModificadoPor = string.Empty;
        }

        public int ProductoID { get; set; }

        [Display(Name = "ID Alfa del Producto")]
        public string ProductoIDAlfa { get; set; } = string.Empty;

        [Display(Name = "Código de Barra")]
        public string CodigoBarra { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
        [StringLength(100)]
        [Display(Name = "Nombre del Producto")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "El precio de costo debe ser mayor o igual a cero.")]
        [Display(Name = "Precio de Costo")]
        public decimal PCosto { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El precio de contado debe ser mayor o igual a cero.")]
        [Display(Name = "Precio de Contado")]
        public decimal PContado { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El precio de lista debe ser mayor o igual a cero.")]
        [Display(Name = "Precio de Lista")]
        public decimal PLista { get; set; }

        [Range(0, 100, ErrorMessage = "El porcentaje de IVA debe estar entre 0 y 100.")]
        [Display(Name = "Porcentaje de IVA")]
        public decimal PorcentajeIva { get; set; } = 21;

        [Display(Name = "Fecha de Modificación")]
        public DateTime FechaMod { get; set; } = DateTime.Now;

        [Display(Name = "Fecha de Modificación de Precio")]
        public DateTime FechaModPrecio { get; set; } = DateTime.Now;

        [Display(Name = "Modificado Por")]
        public string ModificadoPor { get; set; } = string.Empty;

        // Rubros
        [Required(ErrorMessage = "El rubro es obligatorio.")]
        [Display(Name = "Rubro")]
        public int SelectedRubroID { get; set; }
        public IEnumerable<SelectListItem> Rubros { get; set; }

        // SubRubros
        [Required(ErrorMessage = "El subrubro es obligatorio.")]
        [Display(Name = "SubRubro")]
        public int SelectedSubRubroID { get; set; }
        public IEnumerable<SelectListItem> SubRubros { get; set; }

        // Marcas
        [Required(ErrorMessage = "La marca es obligatoria.")]
        [Display(Name = "Marca")]
        public int SelectedMarcaID { get; set; }
        public IEnumerable<SelectListItem> Marcas { get; set; }

        [Display(Name = "Stock Disponible")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock debe ser un valor positivo")]
        public int CantidadDisponible { get; set; }

        // Nuevo campo para editar el stock directamente
        [Display(Name = "Stock Inicial")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock inicial debe ser un valor positivo")]
        public int StockInicial { get; set; }
    }
}