// Profiles/AutoMapperProfile.cs
using AutoMapper;
using Javo2.Models;
using Javo2.ViewModels.Operaciones.Catalogo;
using Javo2.ViewModels.Operaciones.Clientes;
using Javo2.ViewModels.Operaciones.Productos;
using Javo2.ViewModels.Operaciones.Proveedores;
using Javo2.ViewModels.Operaciones.Stock;
using Javo2.ViewModels.Operaciones.Promociones;
using Javo2.ViewModels.Operaciones.Ventas;
using System;

namespace Javo2
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // PRODUCTO ↔ ProductosViewModel
            CreateMap<Producto, ProductosViewModel>()
                .ForMember(dest => dest.SelectedRubroID, opt => opt.MapFrom(src => src.RubroID))
                .ForMember(dest => dest.SelectedSubRubroID, opt => opt.MapFrom(src => src.SubRubroID))
                .ForMember(dest => dest.SelectedMarcaID, opt => opt.MapFrom(src => src.MarcaID))
                .ForMember(dest => dest.CantidadDisponible, opt => opt.MapFrom(src => src.StockItem != null ? src.StockItem.CantidadDisponible : 0))
                .ForMember(dest => dest.ProductoIDAlfa, opt => opt.MapFrom(src => src.CodigoAlfa))
                .ForMember(dest => dest.CodigoBarra, opt => opt.MapFrom(src => src.CodigoBarra))
                .ForMember(dest => dest.StockInicial, opt => opt.Ignore())
                .ForMember(dest => dest.Rubros, opt => opt.Ignore())
                .ForMember(dest => dest.SubRubros, opt => opt.Ignore())
                .ForMember(dest => dest.Marcas, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(dest => dest.RubroID, opt => opt.MapFrom(src => src.SelectedRubroID))
                .ForMember(dest => dest.SubRubroID, opt => opt.MapFrom(src => src.SelectedSubRubroID))
                .ForMember(dest => dest.MarcaID, opt => opt.MapFrom(src => src.SelectedMarcaID))
                .ForMember(dest => dest.CodigoAlfa, opt => opt.MapFrom(src => src.ProductoIDAlfa))
                .ForMember(dest => dest.CodigoBarra, opt => opt.MapFrom(src => src.CodigoBarra))
                .ForMember(dest => dest.StockItem, opt => opt.Ignore())
                .ForMember(dest => dest.Rubro, opt => opt.Ignore())
                .ForMember(dest => dest.SubRubro, opt => opt.Ignore())
                .ForMember(dest => dest.Marca, opt => opt.Ignore())
                .ForMember(dest => dest.Estado, opt => opt.Ignore());

            CreateMap<Cotizacion, Venta>()
                .ForMember(dest => dest.VentaID, opt => opt.Ignore())
                .ForMember(dest => dest.FechaVenta, opt => opt.MapFrom(src => src.FechaCotizacion))
                .ForMember(dest => dest.NumeroFactura, opt => opt.Ignore())
                .ForMember(dest => dest.Vendedor, opt => opt.MapFrom(src => src.Usuario))
                .ForMember(dest => dest.DomicilioCliente, opt => opt.MapFrom(src => string.Empty))
                .ForMember(dest => dest.LocalidadCliente, opt => opt.MapFrom(src => string.Empty))
                .ForMember(dest => dest.CelularCliente, opt => opt.MapFrom(src => string.Empty))
                .ForMember(dest => dest.LimiteCreditoCliente, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.SaldoCliente, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.SaldoDisponibleCliente, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.FormaPagoID, opt => opt.MapFrom(src => 1))  // Default: Contado
                .ForMember(dest => dest.BancoID, opt => opt.MapFrom(src => (int?)null))
                .ForMember(dest => dest.TipoTarjeta, opt => opt.MapFrom(src => string.Empty))
                .ForMember(dest => dest.Cuotas, opt => opt.MapFrom(src => (int?)null))
                .ForMember(dest => dest.EntidadElectronica, opt => opt.MapFrom(src => string.Empty))
                .ForMember(dest => dest.PlanFinanciamiento, opt => opt.MapFrom(src => string.Empty))
                .ForMember(dest => dest.PromocionesAplicadas, opt => opt.MapFrom(src => new List<PromocionAplicada>()))
                .ForMember(dest => dest.Condiciones, opt => opt.MapFrom(src => string.Empty))
                .ForMember(dest => dest.Credito, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.AdelantoDinero, opt => opt.Ignore())
                .ForMember(dest => dest.DineroContado, opt => opt.Ignore())
                .ForMember(dest => dest.MontoCheque, opt => opt.Ignore())
                .ForMember(dest => dest.NumeroCheque, opt => opt.MapFrom(src => string.Empty))
                .ForMember(dest => dest.CuotasPagas, opt => opt.MapFrom(src => new List<Cuota>()))
                .ForMember(dest => dest.EstadosEntregaProductos, opt => opt.MapFrom(src => new List<EstadoEntregaProducto>()))
                .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => EstadoVenta.Borrador));


            // COTIZACION ↔ VentaFormViewModel mapping
            CreateMap<Cotizacion, VentaFormViewModel>()
                .ForMember(dest => dest.VentaID, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.FechaVenta, opt => opt.MapFrom(src => DateTime.Today))
                .ForMember(dest => dest.NumeroFactura, opt => opt.Ignore())
                .ForMember(dest => dest.Vendedor, opt => opt.MapFrom(src => src.Usuario))
                .ForMember(dest => dest.DomicilioCliente, opt => opt.MapFrom(src => string.Empty))
                .ForMember(dest => dest.LocalidadCliente, opt => opt.MapFrom(src => string.Empty))
                .ForMember(dest => dest.CelularCliente, opt => opt.MapFrom(src => string.Empty))
                .ForMember(dest => dest.LimiteCreditoCliente, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.SaldoCliente, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.SaldoDisponibleCliente, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.FormaPagoID, opt => opt.MapFrom(src => 1))  // Default: Contado
                .ForMember(dest => dest.BancoID, opt => opt.MapFrom(src => (int?)null))
                .ForMember(dest => dest.TipoTarjeta, opt => opt.MapFrom(src => string.Empty))
                .ForMember(dest => dest.Cuotas, opt => opt.MapFrom(src => (int?)null))
                .ForMember(dest => dest.EntidadElectronica, opt => opt.MapFrom(src => string.Empty))
                .ForMember(dest => dest.PlanFinanciamiento, opt => opt.MapFrom(src => string.Empty))
                .ForMember(dest => dest.Condiciones, opt => opt.MapFrom(src => $"Generado desde cotización - {src.NumeroCotizacion}"))
                .ForMember(dest => dest.Credito, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.AdelantoDinero, opt => opt.Ignore())
                .ForMember(dest => dest.DineroContado, opt => opt.Ignore())
                .ForMember(dest => dest.MontoCheque, opt => opt.Ignore())
                .ForMember(dest => dest.NumeroCheque, opt => opt.MapFrom(src => string.Empty))
                .ForMember(dest => dest.FormasPago, opt => opt.Ignore())
                .ForMember(dest => dest.Bancos, opt => opt.Ignore())
                .ForMember(dest => dest.TipoTarjetaOptions, opt => opt.Ignore())
                .ForMember(dest => dest.CuotasOptions, opt => opt.Ignore())
                .ForMember(dest => dest.EntidadesElectronicas, opt => opt.Ignore())
                .ForMember(dest => dest.PlanesFinanciamiento, opt => opt.Ignore())
                .ForMember(dest => dest.PromocionID, opt => opt.Ignore())
                .ForMember(dest => dest.Promociones, opt => opt.Ignore())
                .ForMember(dest => dest.EstadosEntregaProductos, opt => opt.Ignore())
                .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => EstadoVenta.Borrador.ToString()));
            // VENTA ↔ VentaFormViewModel
            CreateMap<Venta, VentaFormViewModel>()
                .ForMember(dest => dest.FormasPago, opt => opt.Ignore())
                .ForMember(dest => dest.Bancos, opt => opt.Ignore())
                .ForMember(dest => dest.TipoTarjetaOptions, opt => opt.Ignore())
                .ForMember(dest => dest.CuotasOptions, opt => opt.Ignore())
                .ForMember(dest => dest.EntidadesElectronicas, opt => opt.Ignore())
                .ForMember(dest => dest.PlanesFinanciamiento, opt => opt.Ignore())
                .ForMember(dest => dest.DniCliente, opt => opt.Ignore())
                .ForMember(dest => dest.TipoTarjeta, opt => opt.Ignore())
                .ForMember(dest => dest.Cuotas, opt => opt.Ignore())
                .ForMember(dest => dest.TotalProductos, opt => opt.Ignore())
                .ForMember(dest => dest.PromocionID, opt => opt.Ignore())
                .ForMember(dest => dest.Promociones, opt => opt.Ignore())
                .ForMember(dest => dest.EstadosEntregaProductos, opt => opt.Ignore())
                .ForMember(dest => dest.ProductosPresupuesto, opt => opt.MapFrom(src => src.ProductosPresupuesto))
                .ReverseMap()
                .ForMember(dest => dest.ProductosPresupuesto, opt => opt.MapFrom(src => src.ProductosPresupuesto))
                .ForMember(dest => dest.FormaPagoID, opt => opt.MapFrom(src => src.FormaPagoID))
                .ForMember(dest => dest.BancoID, opt => opt.MapFrom(src => src.BancoID))
                .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Estado));

            // VENTA ↔ VentaListViewModel
            CreateMap<Venta, VentaListViewModel>()
                .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Estado.ToString()))
                .ForMember(dest => dest.EstadoEntrega, opt => opt.Ignore())
                .ReverseMap();

            // COTIZACION ↔ CotizacionViewModel
            CreateMap<Cotizacion, CotizacionViewModel>()
                .ForMember(dest => dest.DiasVigencia, opt => opt.Ignore())
                .ForMember(dest => dest.ProductosPresupuesto, opt => opt.MapFrom(src => src.ProductosPresupuesto))
                .ReverseMap()
                .ForMember(dest => dest.ProductosPresupuesto, opt => opt.MapFrom(src => src.ProductosPresupuesto))
                .ForMember(dest => dest.FechaVencimiento, opt => opt.Ignore())
                .ForMember(dest => dest.Usuario, opt => opt.Ignore());

            // COTIZACION ↔ CotizacionListViewModel
            CreateMap<Cotizacion, CotizacionListViewModel>()
                .ReverseMap();

            // CLIENTE ↔ ClientesViewModel
            CreateMap<Cliente, ClientesViewModel>()
                .ForMember(dest => dest.Provincias, opt => opt.Ignore())
                    .ForMember(dest => dest.Garante, opt => opt.Ignore()) // Ignorar la propiedad Garante

                .ForMember(dest => dest.Ciudades, opt => opt.Ignore())
                .ForMember(dest => dest.HistorialCompras, opt => opt.MapFrom(src => src.Compras))
                .ForMember(dest => dest.Verificar, opt => opt.Ignore())
                .ForMember(dest => dest.EstadoComentario, opt => opt.Ignore())
                .ForMember(dest => dest.TipoDomicilio, opt => opt.Ignore())
                .ForMember(dest => dest.AntiguedadDomicilio, opt => opt.Ignore())
                .ForMember(dest => dest.FechaNacimiento, opt => opt.Ignore())
                .ForMember(dest => dest.EstadoCivil, opt => opt.Ignore())
                .ForMember(dest => dest.NombreConyugue, opt => opt.Ignore())
                .ForMember(dest => dest.ApellidoConyugue, opt => opt.Ignore())
                .ForMember(dest => dest.DniConyugue, opt => opt.Ignore())
                .ForMember(dest => dest.CelularConyugue, opt => opt.Ignore())
                .ForMember(dest => dest.EmailConyugue, opt => opt.Ignore())
                .ForMember(dest => dest.TrabajoConyugue, opt => opt.Ignore())
                .ForMember(dest => dest.Ocupacion, opt => opt.Ignore())
                .ForMember(dest => dest.SituacionLaboral, opt => opt.Ignore())
                .ForMember(dest => dest.LugarTrabajo, opt => opt.Ignore())
                .ForMember(dest => dest.DireccionTrabajo, opt => opt.Ignore())
                .ForMember(dest => dest.Cuit, opt => opt.Ignore())
                .ForMember(dest => dest.AntiguedadLaboral, opt => opt.Ignore())
                .ForMember(dest => dest.IngresosMensuales, opt => opt.Ignore())
                .ForMember(dest => dest.ReferenciasLaborales, opt => opt.Ignore())
                .ForMember(dest => dest.ScoreCredito, opt => opt.Ignore())
                .ForMember(dest => dest.VencimientoCuotas, opt => opt.Ignore())
                .ForMember(dest => dest.NombreGarante, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(dest => dest.Compras, opt => opt.Ignore())
                .ForMember(dest => dest.ClasificacionCredito, opt => opt.MapFrom(src => src.ClasificacionCredito))
                .ForMember(dest => dest.TextoClasificacionCredito, opt => opt.MapFrom(src => src.TextoClasificacionCredito));

            // PROVEEDOR ↔ ProveedoresViewModel
            CreateMap<Proveedor, ProveedoresViewModel>()
                .ForMember(dest => dest.ProductosAsignadosMarcas, opt => opt.Ignore())
                .ForMember(dest => dest.ProductosAsignadosSubMarcas, opt => opt.Ignore())
                .ForMember(dest => dest.ProductosDisponibles, opt => opt.Ignore())
                .ForMember(dest => dest.ProductosAsignadosStocks, opt => opt.Ignore())
                .ForMember(dest => dest.ProductosAsignadosNombres, opt => opt.Ignore())
                .ReverseMap();

            // RUBRO ↔ RubroViewModel
            CreateMap<Rubro, RubroViewModel>()
                .ForMember(dest => dest.TotalStock, opt => opt.Ignore())
                .ReverseMap();

            // MARCA ↔ MarcaViewModel
            CreateMap<Marca, MarcaViewModel>()
                .ForMember(dest => dest.TotalStock, opt => opt.Ignore())
                .ReverseMap();

            // SUBRUBRO ↔ SubRubroEditViewModel
            CreateMap<SubRubro, SubRubroEditViewModel>()
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ReverseMap();

            // SUBRUBRO ↔ SubRubroViewModel
            CreateMap<SubRubro, SubRubroViewModel>().ReverseMap();

            // STOCK ↔ StockItemViewModel
            CreateMap<StockItem, StockItemViewModel>()
                .ForMember(dest => dest.NombreProducto, opt => opt.MapFrom(src => src.Producto.Nombre))
                .ForMember(dest => dest.Movimientos, opt => opt.Ignore())
                .ReverseMap();

            // MovimientoStock ↔ MovimientoStockViewModel
            CreateMap<MovimientoStock, MovimientoStockViewModel>()
                .ForMember(dest => dest.MovimientoID, opt => opt.Ignore())
                .ReverseMap();

            // PROMOCION ↔ PromocionViewModel
            CreateMap<Promocion, PromocionViewModel>().ReverseMap();

            // CompraProveedor ↔ CompraProveedorViewModel
            CreateMap<CompraProveedor, CompraProveedorViewModel>()
                .ForMember(dest => dest.NombreProveedor, opt => opt.MapFrom(src => src.Proveedor.Nombre))
                .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Estado.ToString()))
                .ForMember(dest => dest.FormasPago, opt => opt.Ignore())
                .ForMember(dest => dest.Bancos, opt => opt.Ignore())
                .ForMember(dest => dest.TipoTarjetaOptions, opt => opt.Ignore())
                .ForMember(dest => dest.CuotasOptions, opt => opt.Ignore())
                .ForMember(dest => dest.EntidadesElectronicas, opt => opt.Ignore())
                .ForMember(dest => dest.Proveedores, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => Enum.Parse<EstadoCompra>(src.Estado)));

            // DetalleCompraProveedor ↔ DetalleCompraProveedorViewModel
            CreateMap<DetalleCompraProveedor, DetalleCompraProveedorViewModel>().ReverseMap();

            // Otros mapeos
            CreateMap<Compra, HistorialCompraViewModel>().ReverseMap();
            CreateMap<DetalleVenta, DetalleVentaViewModel>().ReverseMap();

            // AjustePrecio mapeos
            CreateMap<AjustePrecioHistorico, AjustePrecioHistoricoViewModel>().ReverseMap();
            CreateMap<AjustePrecioDetalle, AjustePrecioDetalleViewModel>().ReverseMap();

            CreateMap<AjustePrecioHistorico, AjusteTemporalViewModel>()
                .ForMember(dest => dest.EstadoTemporal, opt => opt.MapFrom(src => src.EstadoTemporal.ToString()))
                .ForMember(dest => dest.Detalles, opt => opt.MapFrom(src => src.Detalles))
                .ReverseMap()
                .ForMember(dest => dest.EstadoTemporal, opt => opt.MapFrom(src => Enum.Parse<EstadoAjusteTemporal>(src.EstadoTemporal)));
        }
    }
}