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

        // Validación condicional
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            Console.WriteLine("Iniciando validación...");

            // 1) Tarjeta de Crédito => TipoTarjeta
            if (FormaPagoID == 2 && string.IsNullOrEmpty(TipoTarjeta))
            {
                Console.WriteLine("Validación fallida: TipoTarjeta es requerido para Tarjeta de Crédito.");
                yield return new ValidationResult(
                    "El campo 'TipoTarjeta' es requerido para Tarjeta de Crédito.",
                    new[] { nameof(TipoTarjeta) }
                );
            }
            else
            {
                Console.WriteLine("Validación exitosa: TipoTarjeta no es requerido o está presente.");
            }

            // 2) Pago Virtual => EntidadElectronica
            if (FormaPagoID == 5 && string.IsNullOrEmpty(EntidadElectronica))
            {
                Console.WriteLine("Validación fallida: EntidadElectronica es requerido para Pago Virtual.");
                yield return new ValidationResult(
                    "El campo 'EntidadElectronica' es requerido para Pago Virtual.",
                    new[] { nameof(EntidadElectronica) }
                );
            }
            else
            {
                Console.WriteLine("Validación exitosa: EntidadElectronica no es requerido o está presente.");
            }

            // 3) Crédito Personal => PlanFinanciamiento
            if (FormaPagoID == 6 && string.IsNullOrEmpty(PlanFinanciamiento))
            {
                Console.WriteLine("Validación fallida: PlanFinanciamiento es requerido para Crédito Personal.");
                yield return new ValidationResult(
                    "El campo 'PlanFinanciamiento' es requerido para Crédito Personal.",
                    new[] { nameof(PlanFinanciamiento) }
                );
            }
            else
            {
                Console.WriteLine("Validación exitosa: PlanFinanciamiento no es requerido o está presente.");
            }

            // 4) Al menos un producto
            if (ProductosPresupuesto == null || ProductosPresupuesto.Count == 0)
            {
                Console.WriteLine("Validación fallida: Debe agregar al menos un producto al presupuesto.");
                yield return new ValidationResult(
                    "Debe agregar al menos un producto al presupuesto.",
                    new[] { nameof(ProductosPresupuesto) }
                );
            }
            else
            {
                Console.WriteLine("Validación exitosa: Al menos un producto está presente en el presupuesto.");
            }

            Console.WriteLine("Validación completada.");
        }
    }
}
