// ViewModels/Operaciones/Proveedores/CompraProveedorViewModel.cs
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Javo2.Models;

namespace Javo2.ViewModels.Operaciones.Proveedores
{
    public class CompraProveedorViewModel
    {
        public CompraProveedorViewModel()
        {
            ProductosCompra = new List<DetalleCompraProveedorViewModel>();
            FormasPago = new List<SelectListItem>();
            Bancos = new List<SelectListItem>();
            TipoTarjetaOptions = new List<SelectListItem>();
            CuotasOptions = new List<SelectListItem>();
            EntidadesElectronicas = new List<SelectListItem>();
        }

        public int CompraID { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un proveedor")]
        [Display(Name = "Proveedor")]
        public int ProveedorID { get; set; }
        public string NombreProveedor { get; set; } = string.Empty;

        [Display(Name = "Fecha de Compra")]
        public DateTime FechaCompra { get; set; } = DateTime.Now;

        [Display(Name = "Número de Factura")]
        public string NumeroFactura { get; set; } = string.Empty;

        public string Usuario { get; set; } = string.Empty;

        [Display(Name = "Productos")]
        public List<DetalleCompraProveedorViewModel> ProductosCompra { get; set; }

        [Display(Name = "Forma de Pago")]
        [Required(ErrorMessage = "Debe seleccionar una forma de pago")]
        public int FormaPagoID { get; set; }

        [Display(Name = "Banco")]
        public int? BancoID { get; set; }

        [Display(Name = "Tipo de Tarjeta")]
        public string? TipoTarjeta { get; set; }

        [Display(Name = "Cuotas")]
        public int? Cuotas { get; set; }

        [Display(Name = "Entidad Electrónica")]
        public string? EntidadElectronica { get; set; }

        [Display(Name = "Fecha de Vencimiento")]
        public DateTime? FechaVencimiento { get; set; }

        [Display(Name = "Monto del Cheque")]
        public decimal? MontoCheque { get; set; }

        [Display(Name = "Número de Cheque")]
        public string? NumeroCheque { get; set; }

        [Display(Name = "Precio Total")]
        public decimal PrecioTotal { get; set; }

        [Display(Name = "Total Productos")]
        public int TotalProductos { get; set; }

        [Display(Name = "Observaciones")]
        public string Observaciones { get; set; } = string.Empty;

        [Display(Name = "Estado")]
        public string Estado { get; set; } = EstadoCompra.Pendiente.ToString();

        // Listas para dropdowns
        public IEnumerable<SelectListItem> FormasPago { get; set; }
        public IEnumerable<SelectListItem> Bancos { get; set; }
        public IEnumerable<SelectListItem> TipoTarjetaOptions { get; set; }
        public IEnumerable<SelectListItem> CuotasOptions { get; set; }
        public IEnumerable<SelectListItem> EntidadesElectronicas { get; set; }
        public IEnumerable<SelectListItem> Proveedores { get; set; } = new List<SelectListItem>();
    }

    public class DetalleCompraProveedorViewModel
    {
        public int DetalleCompraID { get; set; }
        public int CompraID { get; set; }

        [Required]
        [Display(Name = "Producto")]
        public int ProductoID { get; set; }

        [Display(Name = "Nombre del Producto")]
        public string NombreProducto { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a cero")]
        [Display(Name = "Cantidad")]
        public int Cantidad { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a cero")]
        [Display(Name = "Precio Unitario")]
        public decimal PrecioUnitario { get; set; }

        [Display(Name = "Precio Total")]
        public decimal PrecioTotal { get; set; }
    }
}