namespace Javo2.ViewModels.Operaciones.Productos
{
    public class ProductoFilterDto
    {
        public string? Nombre { get; set; }
        public string? Categoria { get; set; }
        public decimal? PrecioMinimo { get; set; }
        public decimal? PrecioMaximo { get; set; }
        public string? Codigo { get; set; }
        public string? Rubro { get; set; }
        public string? SubRubro { get; set; }
        public string? Marca { get; internal set; }
    }
}
