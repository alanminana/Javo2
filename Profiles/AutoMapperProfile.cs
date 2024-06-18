using AutoMapper;
using Javo2.ViewModels.Operaciones.Clientes;
using Javo2.ViewModels.Operaciones.Productos;
using Javo2.ViewModels.Operaciones.Proveedores;
using Javo2.ViewModels.Operaciones.Ventas;

namespace Javo2
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
