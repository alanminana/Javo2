// ViewModels/Operaciones/Catalogo/CatalogoFilterDto.cs
namespace Javo2.ViewModels.Operaciones.Catalogo
{
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
}
