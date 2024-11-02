using Javo2.ViewModels.Operaciones.Catalogo;

public class CatalogoIndexViewModel
{
    public IEnumerable<RubroViewModel> Rubros { get; set; } = new List<RubroViewModel>();
    public IEnumerable<MarcaViewModel> Marcas { get; set; } = new List<MarcaViewModel>();
}