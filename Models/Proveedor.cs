// Archivo: Models/Proveedor.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Javo2.Models
{
    public class Proveedor
    {
        public int ProveedorID { get; set; } // Clave primaria, auto-incremental

        [Required]
        [StringLength(100)]
        [Display(Name = "Nombre del Proveedor")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(200)]
        [Display(Name = "Dirección")]
        public string Direccion { get; set; } = string.Empty;

        [StringLength(20)]
        [Display(Name = "Teléfono")]
        public string Telefono { get; set; } = string.Empty;

        [StringLength(100)]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Condiciones de Pago")]
        public string CondicionesPago { get; set; } = string.Empty;

        // Relaciones: Lista de IDs de productos asignados
        public List<int> ProductosAsignados { get; set; } = new List<int>();
    }
}
