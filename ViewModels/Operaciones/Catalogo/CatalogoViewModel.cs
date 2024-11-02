// ViewModels/Operaciones/Catalogo/CatalogoViewModels.cs
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Javo2.ViewModels.Operaciones.Catalogo
{
    public class RubroViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El nombre del rubro es obligatorio.")]
        public string Nombre { get; set; } = string.Empty;
        public List<SubRubroViewModel> SubRubros { get; set; } = new List<SubRubroViewModel>();
    }
    public class SubRubroViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El nombre del subrubro es obligatorio.")]
        public string Nombre { get; set; } = string.Empty;
        public int RubroId { get; set; }
    }


    public class MarcaViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El nombre de la marca es obligatorio.")]
        public string Nombre { get; set; } = string.Empty;
    }

    public class EditSubRubrosViewModel
    {
        public int RubroId { get; set; }
        public string RubroNombre { get; set; } = string.Empty;
        public List<SubRubroEditViewModel> SubRubros { get; set; } = new List<SubRubroEditViewModel>();
    }

    public class SubRubroEditViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El nombre del subrubro es obligatorio.")]
        public string Nombre { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
    }
}
