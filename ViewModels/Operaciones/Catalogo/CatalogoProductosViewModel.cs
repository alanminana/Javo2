// ViewModels/CatalogoProductosViewModel.cs
using Javo2.ViewModels.Operaciones.Catalogo;
using Javo2.ViewModels.Operaciones.Productos;
using System.Collections.Generic;

namespace Javo2.ViewModels
{
    public class CatalogoProductosViewModel
    {
        public IEnumerable<ProductosViewModel> Productos { get; set; }
        public IEnumerable<RubroViewModel> Rubros { get; set; }
        public IEnumerable<MarcaViewModel> Marcas { get; set; }
    }
}