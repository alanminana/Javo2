﻿// File: Profiles/AutoMapperProfile.cs
using AutoMapper;
using Javo2.Models;
using Javo2.ViewModels.Operaciones.Catalogo;
using Javo2.ViewModels.Operaciones.Clientes;
using Javo2.ViewModels.Operaciones.Productos;
using Javo2.ViewModels.Operaciones.Proveedores;
using Javo2.ViewModels.Operaciones.Stock;
using Javo2.ViewModels.Operaciones.Promociones;
using Javo2.ViewModels.Operaciones.Ventas;

namespace Javo2
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // -----------------------
            // PRODUCTO ↔ ProductosViewModel
            // -----------------------
            CreateMap<Producto, ProductosViewModel>()
                .ForMember(dest => dest.SelectedRubroID, opt => opt.MapFrom(src => src.RubroID))
                .ForMember(dest => dest.SelectedSubRubroID, opt => opt.MapFrom(src => src.SubRubroID))
                .ForMember(dest => dest.SelectedMarcaID, opt => opt.MapFrom(src => src.MarcaID))
                .ForMember(dest => dest.CantidadDisponible, opt => opt.MapFrom(src => src.StockItem != null ? src.StockItem.CantidadDisponible : 0))
                .ForMember(dest => dest.ProductoIDAlfa, opt => opt.Ignore())
                .ForMember(dest => dest.CodigoBarra, opt => opt.Ignore())
                .ForMember(dest => dest.CantidadStock, opt => opt.Ignore())
                .ForMember(dest => dest.Rubros, opt => opt.Ignore())
                .ForMember(dest => dest.SubRubros, opt => opt.Ignore())
                .ForMember(dest => dest.Marcas, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(dest => dest.RubroID, opt => opt.MapFrom(src => src.SelectedRubroID))
                .ForMember(dest => dest.SubRubroID, opt => opt.MapFrom(src => src.SelectedSubRubroID))
                .ForMember(dest => dest.MarcaID, opt => opt.MapFrom(src => src.SelectedMarcaID))
                .ForMember(dest => dest.CodigoAlfa, opt => opt.Ignore())
                .ForMember(dest => dest.CodigoBarra, opt => opt.Ignore())
                .ForMember(dest => dest.StockItem, opt => opt.Ignore());

            // -----------------------
            // CLIENTE ↔ ClientesViewModel
            // -----------------------
            CreateMap<Cliente, ClientesViewModel>()
                .ForMember(dest => dest.Provincias, opt => opt.Ignore())
                .ForMember(dest => dest.Ciudades, opt => opt.Ignore())
                .ForMember(dest => dest.HistorialCompras, opt => opt.MapFrom(src => src.Compras))
                .ForMember(dest => dest.Verificar, opt => opt.Ignore())
                .ForMember(dest => dest.EstadoComentario, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(dest => dest.Compras, opt => opt.Ignore());

            // -----------------------
            // PROVEEDOR ↔ ProveedoresViewModel
            // -----------------------
            CreateMap<Proveedor, ProveedoresViewModel>()
                .ForMember(dest => dest.ProductosAsignadosMarcas, opt => opt.Ignore())
                .ForMember(dest => dest.ProductosAsignadosSubMarcas, opt => opt.Ignore())
                .ForMember(dest => dest.ProductosDisponibles, opt => opt.Ignore())
                .ForMember(dest => dest.ProductosAsignadosStocks, opt => opt.Ignore())
                .ForMember(dest => dest.ProductosAsignadosNombres, opt => opt.Ignore())
                .ReverseMap();

            // -----------------------
            // RUBRO ↔ RubroViewModel
            // -----------------------
            CreateMap<Rubro, RubroViewModel>()
                .ForMember(dest => dest.TotalStock, opt => opt.Ignore())
                .ReverseMap();

            // -----------------------
            // MARCA ↔ MarcaViewModel
            // -----------------------
            CreateMap<Marca, MarcaViewModel>()
                .ForMember(dest => dest.TotalStock, opt => opt.Ignore())
                .ReverseMap();

            // -----------------------
            // SUBRUBRO ↔ SubRubroEditViewModel
            // -----------------------
            CreateMap<SubRubro, SubRubroEditViewModel>()
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ReverseMap();

            // -----------------------
            // SUBRUBRO ↔ SubRubroViewModel
            // -----------------------
            CreateMap<SubRubro, SubRubroViewModel>().ReverseMap();

            // -----------------------
            // STOCK ↔ StockItemViewModel
            // -----------------------
            CreateMap<StockItem, StockItemViewModel>()
                .ForMember(dest => dest.NombreProducto, opt => opt.MapFrom(src => src.Producto.Nombre))
                .ForMember(dest => dest.Movimientos, opt => opt.Ignore())
                .ReverseMap();

            // -----------------------
            // MovimientoStock ↔ MovimientoStockViewModel
            // -----------------------
            CreateMap<MovimientoStock, MovimientoStockViewModel>()
                .ForMember(dest => dest.MovimientoID, opt => opt.Ignore())
                .ReverseMap();

            // -----------------------
            // AjusteStockViewModel ↔ MovimientoStock
            // -----------------------
            CreateMap<AjusteStockViewModel, MovimientoStock>()
                .ForMember(dest => dest.MovimientoStockID, opt => opt.Ignore())
                .ForMember(dest => dest.Fecha, opt => opt.Ignore())
                .ForMember(dest => dest.TipoMovimiento, opt => opt.Ignore())
                .ForMember(dest => dest.Cantidad, opt => opt.Ignore())
                .ReverseMap();

            // -----------------------
            // VENTA ↔ VentaListViewModel
            // -----------------------
            CreateMap<Venta, VentaListViewModel>()
                .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Estado.ToString()))
                // Se omite el mapeo de EstadoEntrega porque no existe en el modelo Venta.
                .ReverseMap();

            // -----------------------
            // VENTA ↔ VentaFormViewModel
            // -----------------------
            CreateMap<Venta, VentaFormViewModel>()
                .ForMember(dest => dest.FormasPago, opt => opt.Ignore())
                .ForMember(dest => dest.Bancos, opt => opt.Ignore())
                .ForMember(dest => dest.TipoTarjetaOptions, opt => opt.Ignore())
                .ForMember(dest => dest.CuotasOptions, opt => opt.Ignore())
                .ForMember(dest => dest.EntidadesElectronicas, opt => opt.Ignore())
                .ForMember(dest => dest.PlanesFinanciamiento, opt => opt.Ignore())
                // Ignorar propiedades que no existen en Venta
                .ForMember(dest => dest.DniCliente, opt => opt.Ignore())
                .ForMember(dest => dest.TipoTarjeta, opt => opt.Ignore())
                .ForMember(dest => dest.Cuotas, opt => opt.Ignore())
                .ForMember(dest => dest.TotalProductos, opt => opt.Ignore())
                .ForMember(dest => dest.ProductosPresupuesto, opt => opt.MapFrom(src => src.ProductosPresupuesto))
                .ReverseMap()
                .ForMember(dest => dest.ProductosPresupuesto, opt => opt.MapFrom(src => src.ProductosPresupuesto))
                .ForMember(dest => dest.FormaPagoID, opt => opt.MapFrom(src => src.FormaPagoID))
                .ForMember(dest => dest.BancoID, opt => opt.MapFrom(src => src.BancoID))
                .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Estado));
                // Se eliminan mappings a ClienteID, FormaPago, Banco, EstadoEntrega, FechaCreacion, FechaModificacion.
        }
    }
}
