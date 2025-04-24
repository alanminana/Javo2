// ViewModels/Operaciones/DevolucionGarantia/DevolucionGarantiaViewModel.cs
using Javo2.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Javo2.ViewModels.Operaciones.DevolucionGarantia
{
    public class DevolucionGarantiaViewModel
    {
        public int DevolucionGarantiaID { get; set; }

        [Display(Name = "Venta")]
        public int VentaID { get; set; }

        public string NumeroFactura { get; set; } = string.Empty;

        [Display(Name = "Cliente")]
        public string NombreCliente { get; set; } = string.Empty;

        [Display(Name = "Fecha de Venta")]
        public DateTime FechaVenta { get; set; }

        [Display(Name = "Fecha de Solicitud")]
        public DateTime FechaSolicitud { get; set; } = DateTime.Now;

        [Display(Name = "Tipo de Caso")]
        public TipoCaso TipoCaso { get; set; }

        public List<SelectListItem> TiposCaso { get; set; } = new List<SelectListItem>();

        [Display(Name = "Motivo")]
        [Required(ErrorMessage = "El motivo es obligatorio")]
        public string Motivo { get; set; } = string.Empty;

        public List<SelectListItem> Motivos { get; set; } = new List<SelectListItem>();

        [Display(Name = "Descripción")]
        [Required(ErrorMessage = "La descripción es obligatoria")]
        public string Descripcion { get; set; } = string.Empty;

        [Display(Name = "Estado")]
        public EstadoCaso Estado { get; set; } = EstadoCaso.Pendiente;

        [Display(Name = "Comentarios")]
        public string Comentarios { get; set; } = string.Empty;

        [Display(Name = "Fecha Resolución")]
        public DateTime? FechaResolucion { get; set; }

        // Items para devolver o enviar a garantía
        public List<ItemDevolucionGarantiaViewModel> Items { get; set; } = new List<ItemDevolucionGarantiaViewModel>();

        // Para el caso de cambio de producto
        public List<CambioProductoViewModel> CambiosProducto { get; set; } = new List<CambioProductoViewModel>();

        // Para buscar la venta
        [Display(Name = "Buscar Venta")]
        public string BuscarVenta { get; set; } = string.Empty;
    }

    public class ItemDevolucionGarantiaViewModel
    {
        public int ItemDevolucionGarantiaID { get; set; }
        public int ProductoID { get; set; }
        public string CodigoAlfa { get; set; } = string.Empty;
        public string NombreProducto { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
        public bool Seleccionado { get; set; }
        public bool ProductoDanado { get; set; }

        [Display(Name = "Estado del Producto")]
        public string EstadoProducto { get; set; } = string.Empty;

        public List<SelectListItem> EstadosProducto { get; set; } = new List<SelectListItem>();
    }

    public class CambioProductoViewModel
    {
        public int CambioProductoID { get; set; }

        [Display(Name = "Producto Original")]
        public int ProductoOriginalID { get; set; }
        public string NombreProductoOriginal { get; set; } = string.Empty;

        [Display(Name = "Producto Nuevo")]
        [Required(ErrorMessage = "Debe seleccionar un producto nuevo")]
        public int ProductoNuevoID { get; set; }
        public string NombreProductoNuevo { get; set; } = string.Empty;

        [Display(Name = "Cantidad")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public int Cantidad { get; set; }

        [Display(Name = "Diferencia de Precio")]
        public decimal DiferenciaPrecio { get; set; }

        public List<SelectListItem> ProductosDisponibles { get; set; } = new List<SelectListItem>();
    }

    public class DevolucionGarantiaListViewModel
    {
        public int DevolucionGarantiaID { get; set; }
        public int VentaID { get; set; }
        public string NumeroFactura { get; set; } = string.Empty;
        public string NombreCliente { get; set; } = string.Empty;
        public DateTime FechaSolicitud { get; set; }
        public TipoCaso TipoCaso { get; set; }
        public EstadoCaso Estado { get; set; }
        public int CantidadProductos { get; set; }
        public DateTime? FechaResolucion { get; set; }
    }
}