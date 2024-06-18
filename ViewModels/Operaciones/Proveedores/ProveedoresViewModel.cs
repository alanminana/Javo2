using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Javo2.ViewModels.Operaciones.Productos;

namespace Javo2.ViewModels.Operaciones.Proveedores
{
    public class ProveedoresViewModel
    {
        public int ProveedorID { get; set; } = 0; // Auto-incremental y no editable

        [Required]
        [StringLength(50)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Apellido { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Empresa { get; set; } = string.Empty;

        [Required]
        public int CUIT { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public int Telefono { get; set; }

        public int Celular { get; set; }

        public string CondicionesPago { get; set; } = string.Empty;

        public List<string> ProductosSeleccionados { get; set; } = [];

        public List<ProductosViewModel> ProductosDisponibles { get; set; } = [];
    }
}