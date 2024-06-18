using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Javo2.ViewModels.Operaciones.Catalogo
{
    public class RubroViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; } = string.Empty;

        public ICollection<SubRubroViewModel> SubRubros { get; set; } = new List<SubRubroViewModel>();
    }

    public class SubRubroViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        public string RubroNombre { get; set; } = string.Empty;
        public IEnumerable<SelectListItem> Rubros { get; set; } = new List<SelectListItem>();
    }

    public class MarcaViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; } = string.Empty;
    }

    public class EditSubRubrosViewModel
    {
        public int RubroId { get; set; }
        public string RubroNombre { get; set; } = string.Empty;
        public List<SubRubroEditViewModel> SubRubros { get; set; } = new List<SubRubroEditViewModel>();
        public string NewSubRubroNombre { get; set; } = string.Empty;
    }

    public class SubRubroEditViewModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public bool IsDeleted { get; set; } = false;
    }
}
