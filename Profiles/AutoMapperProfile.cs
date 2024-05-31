using AutoMapper;
using javo2.ViewModels.Operaciones.Clientes;
using javo2.ViewModels.Operaciones.Productos;
using javo2.ViewModels.Operaciones.Proveedores;
using javo2.ViewModels.Operaciones.Ventas;

namespace javo2
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<ClientesViewModel, ClientesViewModel>().ReverseMap();
            CreateMap<ProductosViewModel, ProductosViewModel>().ReverseMap();
            CreateMap<VentasViewModel, VentasViewModel>().ReverseMap();
            CreateMap<ProveedoresViewModel, ProveedoresViewModel>().ReverseMap();

        }
    }
}
