// File: ViewModels/Operaciones/Productos/AjustePrecioViewModel.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Javo2.ViewModels.Operaciones.Productos
{
    public class ConfiguracionIndexViewModel
    {
        [Required(ErrorMessage = "Seleccione al menos un producto.")]
        public List<int> ProductoIDs { get; set; } = new List<int>();

        [Required(ErrorMessage = "Ingrese el porcentaje de ajuste.")]
        [Range(0.01, 100, ErrorMessage = "El porcentaje debe ser mayor a 0 y menor o igual a 100.")]
        public decimal Porcentaje { get; set; }

        [Display(Name = "Aumento de Precio?")]
        public bool EsAumento { get; set; } = true;
    }
}
