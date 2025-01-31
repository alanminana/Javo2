// ViewModels/Operaciones/Ventas/VentaFormViewModel.cs
using Javo2.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Javo2.ViewModels.Operaciones.Ventas
{
    public class VentaFormViewModel : IValidatableObject
    {
        public int VentaID { get; set; }

        [Display(Name = "Fecha de Venta")]
        [DataType(DataType.Date)]
        public DateTime FechaVenta { get; set; } = DateTime.Today;

        [Display(Name = "Número de Factura")]
        public string NumeroFactura { get; set; } = string.Empty;

        public string Usuario { get; set; } = string.Empty;
        public string Vendedor { get; set; } = string.Empty;

        // Datos del Cliente
        public int DniCliente { get; set; }
        public string NombreCliente { get; set; } = string.Empty;
        public string TelefonoCliente { get; set; } = string.Empty;
        public string DomicilioCliente { get; set; } = string.Empty;
        public string LocalidadCliente { get; set; } = string.Empty;
        public string CelularCliente { get; set; } = string.Empty;
        public decimal LimiteCreditoCliente { get; set; }
        public decimal SaldoCliente { get; set; }
        public decimal SaldoDisponibleCliente { get; set; }

        // Forma de Pago
        public int FormaPagoID { get; set; }
        public IEnumerable<SelectListItem> FormasPago { get; set; } = new List<SelectListItem>();

        public int? BancoID { get; set; }
        public IEnumerable<SelectListItem> Bancos { get; set; } = new List<SelectListItem>();

        // Validación condicional
        public string TipoTarjeta { get; set; } = string.Empty;
        public IEnumerable<SelectListItem> TipoTarjetaOptions { get; set; } = new List<SelectListItem>();

        public int? Cuotas { get; set; }
        public IEnumerable<SelectListItem> CuotasOptions { get; set; } = new List<SelectListItem>();

        public string EntidadElectronica { get; set; } = string.Empty;
        public IEnumerable<SelectListItem> EntidadesElectronicas { get; set; } = new List<SelectListItem>();

        public string PlanFinanciamiento { get; set; } = string.Empty;
        public IEnumerable<SelectListItem> PlanesFinanciamiento { get; set; } = new List<SelectListItem>();

        // Otros
        public string Observaciones { get; set; } = string.Empty;
        public string Condiciones { get; set; } = string.Empty;
        public decimal Credito { get; set; }

        // Productos
        public List<DetalleVentaViewModel> ProductosPresupuesto { get; set; } = new List<DetalleVentaViewModel>();

        // Totales
        public decimal PrecioTotal { get; set; }
        public int TotalProductos { get; set; }

        // Anticipo / Cheques
        public decimal? AdelantoDinero { get; set; }
        public decimal? DineroContado { get; set; }
        public decimal? MontoCheque { get; set; }
        public string NumeroCheque { get; set; } = string.Empty;

        // Estado
        public string Estado { get; set; } = string.Empty;

        // Promociones
        public int? PromocionID { get; set; }
        public IEnumerable<SelectListItem> Promociones { get; set; } = new List<SelectListItem>();

        // Relación con Estados de Entrega
        public List<EstadoEntregaProductoViewModel> EstadosEntregaProductos { get; set; } = new List<EstadoEntregaProductoViewModel>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Validaciones existentes
            if (FormaPagoID == 2 && string.IsNullOrEmpty(TipoTarjeta))
            {
                yield return new ValidationResult(
                    "El campo 'TipoTarjeta' es requerido para Tarjeta de Crédito.",
                    new[] { nameof(TipoTarjeta) }
                );
            }

            if (FormaPagoID == 5 && string.IsNullOrEmpty(EntidadElectronica))
            {
                yield return new ValidationResult(
                    "El campo 'EntidadElectronica' es requerido para Pago Virtual.",
                    new[] { nameof(EntidadElectronica) }
                );
            }

            if (FormaPagoID == 6 && string.IsNullOrEmpty(PlanFinanciamiento))
            {
                yield return new ValidationResult(
                    "El campo 'PlanFinanciamiento' es requerido para Crédito Personal.",
                    new[] { nameof(PlanFinanciamiento) }
                );
            }

            if (ProductosPresupuesto == null || ProductosPresupuesto.Count == 0)
            {
                yield return new ValidationResult(
                    "Debe agregar al menos un producto al presupuesto.",
                    new[] { nameof(ProductosPresupuesto) }
                );
            }

            // Validaciones adicionales
            if (FormaPagoID == 6 && (Cuotas == null || Cuotas <= 0))
            {
                yield return new ValidationResult(
                    "Debe especificar una cantidad válida de cuotas para compras a crédito.",
                    new[] { nameof(Cuotas) }
                );
            }
        }
    }

    public class EstadoEntregaProductoViewModel
    {
        public int ProductoID { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public EstadoFlujo EstadoFlujo { get; set; }
    }
}
