namespace javo2.ViewModels.Operaciones.Productos
{
    public class ProductoPresupuestoViewModel
    {
        public ProductoPresupuestoViewModel()
        {
            Codigo = string.Empty;
            CodigoAlfa = string.Empty;
            Detalle = string.Empty;



        }

        public int ProductoID { get; set; }
        public string Codigo { get; set; }
        public string CodigoAlfa { get; set; }
        public string Detalle { get; set; }
        public int Cantidad { get; set; }
        public int Cuotas { get; set; } = 3;
        public decimal ImporteCuotaSinInteres { get; set; } = 4;
        public int MaximoCuotas { get; set; }
        public decimal PrecioLista { get; set; } = 123;
        public decimal PrecioTotal { get; set; } = 312;
        public string Marca { get; set; } = "LG";

    }
}
