using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Javo2.ViewModels.Operaciones.Catalogo
{
    // Modelo para filtrado
    public class CatalogoFilterDto
    {
        public string? Nombre { get; set; }
        public string? Codigo { get; set; }
        public string? Rubro { get; set; }
        public string? SubRubro { get; set; }
        public string? Marca { get; set; }
        public decimal? PrecioMinimo { get; set; }
        public decimal? PrecioMaximo { get; set; }
    }

    // Modelo principal para vista Index
    public class CatalogoIndexViewModel
    {
        public IEnumerable<RubroViewModel> Rubros { get; set; } = new List<RubroViewModel>();
        public IEnumerable<MarcaViewModel> Marcas { get; set; } = new List<MarcaViewModel>();
    }

    // Modelo para Rubro
    public class RubroViewModel
    {
        public int ID { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public List<SubRubroViewModel> SubRubros { get; set; } = new List<SubRubroViewModel>();
        public int TotalStock { get; set; }
    }

    // Modelo para SubRubro
    public class SubRubroViewModel
    {
        public int ID { get; set; }
        [Required(ErrorMessage = "El nombre del subrubro es obligatorio.")]
        public string Nombre { get; set; } = string.Empty;
        public int RubroID { get; set; }
    }

    // Modelo para Marca
    public class MarcaViewModel
    {
        public int ID { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int TotalStock { get; set; }
    }

    // Modelo para editar SubRubros
    public class EditSubRubrosViewModel
    {
        public int RubroID { get; set; }
        public string RubroNombre { get; set; } = string.Empty;
        public List<SubRubroEditViewModel> SubRubros { get; set; } = new List<SubRubroEditViewModel>();
    }

    // Modelo para editar SubRubro individual
    public class SubRubroEditViewModel
    {
        public int ID { get; set; }
        [Required(ErrorMessage = "El nombre del subrubro es obligatorio.")]
        public string Nombre { get; set; } = string.Empty;
        public bool IsDeleted { get; set; } = false;
    }
}