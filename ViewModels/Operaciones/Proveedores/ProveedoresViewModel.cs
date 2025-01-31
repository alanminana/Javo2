using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Javo2.ViewModels.Operaciones.Proveedores
{
    public class ProveedoresViewModel
    {
        public ProveedoresViewModel()
        {
            ProductosDisponibles = new List<SelectListItem>();
            ProductosAsignados = new List<int>();
            ProductosAsignadosNombres = new List<string>();

        }

        public List<string> ProductosAsignadosMarcas { get; set; } = new();
        public List<string> ProductosAsignadosSubMarcas { get; set; } = new();
        public int ProveedorID { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Nombre del Proveedor")]
        public string Nombre { get; set; } = string.Empty;

        [Display(Name = "Dirección")]
        public string Direccion { get; set; } = string.Empty;

        [Display(Name = "Teléfono")]
        public string Telefono { get; set; } = string.Empty;

        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Condiciones de Pago")]
        public string CondicionesPago { get; set; } = string.Empty;

        [Display(Name = "Productos Disponibles")]
        public IEnumerable<SelectListItem> ProductosDisponibles { get; set; }

        [Display(Name = "Productos Asignados")]
        public List<int> ProductosAsignados { get; set; }
        public List<int> ProductosAsignadosStocks { get; set; } = new List<int>();

        public List<string> ProductosAsignadosNombres { get; set; }
    }
}
