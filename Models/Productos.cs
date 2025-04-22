// Models/Producto.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Javo2.Models
{
    public class Producto
    {
        public int ProductoID { get; set; }

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

        public int RubroID { get; set; }
        public Rubro Rubro { get; set; } = new Rubro();
        public int SubRubroID { get; set; }
        public SubRubro SubRubro { get; set; } = new SubRubro();
        public int MarcaID { get; set; }
        public Marca? Marca { get; set; }

        [Range(0, 100)]
        [Display(Name = "Porcentaje de IVA")]
        public decimal PorcentajeIva { get; set; } = 21;

        [Display(Name = "Fecha de Modificación")]
        public DateTime FechaMod { get; set; } = DateTime.UtcNow;

        [Display(Name = "Fecha de Modificación de Precio")]
        public DateTime FechaModPrecio { get; set; } = DateTime.UtcNow;

        [Display(Name = "Entregable")]
        public bool Entregable { get; set; } = false;

        [Display(Name = "No Listar")]
        public bool NoListar { get; set; } = false;

        [Display(Name = "Proveedor")]
        public Proveedor Proveedor { get; set; } = new Proveedor();

        // Nuevas propiedades
        [Display(Name = "Usuario")]
        public string Usuario { get; set; } = string.Empty;

        [Display(Name = "Modificado Por")]
        public string ModificadoPor { get; set; } = string.Empty;

        [Display(Name = "Estado Comentario")]
        public string EstadoComentario { get; set; } = string.Empty;

        [Display(Name = "Deuda Total")]
        public decimal DeudaTotal { get; set; } = 0;

        public enum EstadoProducto
        {
            Activo,
            Inactivo,
            Descontinuado
        }
        public EstadoProducto Estado { get; set; } = EstadoProducto.Activo;

        public StockItem? StockItem { get; set; }
    }
}