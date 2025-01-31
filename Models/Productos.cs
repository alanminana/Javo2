using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Javo2.Models
{
    public class Producto
    {
        public int ProductoID { get; set; } // Auto-incremental

        [Required]
        [Display(Name = "ID Alfa del Producto")]
        public string CodigoAlfa { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Código de Barra")]
        public string CodigoBarra { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
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
        public DateTime FechaMod { get; set; } = DateTime.UtcNow;

        [Display(Name = "Fecha de Modificación de Precio")]
        public DateTime FechaModPrecio { get; set; } = DateTime.UtcNow;

        [Display(Name = "Entregable")]
        public bool Entregable { get; set; } = false; // Checkbox

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

        // Relaciones
        [Required]
        [Display(Name = "Rubro")]
        public int RubroID { get; set; }
        public Rubro Rubro { get; set; }

        [Required]
        [Display(Name = "SubRubro")]
        public int SubRubroID { get; set; }
        public SubRubro SubRubro { get; set; }

        [Required]
        [Display(Name = "Marca")]
        public int MarcaID { get; set; }
        public Marca Marca { get; set; }

        [Display(Name = "Fecha de Stock")]
        public Proveedor Proveedor { get; set; }

        public enum EstadoProducto
        {
            Activo,
            Inactivo,
            Descontinuado
        }
        public StockItem StockItem { get; set; }

        public EstadoProducto Estado { get; set; } = EstadoProducto.Activo;
    }
}
