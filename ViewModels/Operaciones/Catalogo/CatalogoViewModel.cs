// ViewModels/Operaciones/Catalogo/CatalogoViewModels.cs
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Javo2.ViewModels.Operaciones.Catalogo
{
    public class RubroViewModel
    {
        public int ID { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public List<SubRubroViewModel> SubRubros { get; set; } = new List<SubRubroViewModel>();
        public int TotalStock { get; set; } // Nueva propiedad
    }

    public class SubRubroViewModel
    {
        public int ID { get; set; }
        [Required(ErrorMessage = "El nombre del subrubro es obligatorio.")]
        public string Nombre { get; set; } = string.Empty;
        public int RubroID { get; set; }
    }
    public class MarcaViewModel
    {
        public int ID { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int TotalStock { get; set; } // Nueva propiedad
    }
    public class EditSubRubrosViewModel
    {
        public int RubroID { get; set; }
        public string RubroNombre { get; set; } = string.Empty;
        public List<SubRubroEditViewModel> SubRubros { get; set; } = new List<SubRubroEditViewModel>();
    }

    public class SubRubroEditViewModel
    {
        public int ID { get; set; }
        [Required(ErrorMessage = "El nombre del subrubro es obligatorio.")]
        public string Nombre { get; set; } = string.Empty;
        public bool IsDeleted { get; set; } = false;
    }
}
