// Archivo: AutoMapperProfile.cs
using AutoMapper;
using Javo2.Models;
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
            CreateMap<Producto, ProductosViewModel>().ReverseMap();

            CreateMap<Cliente, ClientesViewModel>().ReverseMap();
            CreateMap<ClientesViewModel, Cliente>();
            CreateMap<Cliente, ClientesViewModel>();

            CreateMap<Provincia, ProvinciaViewModel>().ReverseMap();
            CreateMap<Ciudad, CiudadViewModel>().ReverseMap();
            CreateMap<Venta, VentasViewModel>().ReverseMap();
            CreateMap<ProductoPresupuesto, ProductoPresupuestoViewModel>().ReverseMap();
            CreateMap<Proveedor, ProveedoresViewModel>().ReverseMap();
        }
    }
}
