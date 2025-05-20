// ViewModels/Operaciones/Ventas/VentaCreditoViewModel.cs
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Javo2.ViewModels.Operaciones.Ventas
{
    public class VentaCreditoViewModel
    {
        public int VentaID { get; set; }
        public int ClienteID { get; set; }
        public string NumeroFactura { get; set; }
        public DateTime FechaVenta { get; set; }
        public string ClienteNombre { get; set; }
        public decimal MontoTotal { get; set; }
        public string ScoreCliente { get; set; }
        public bool RequiereGarante { get; set; }
        public bool TieneGarante { get; set; }

        [Required(ErrorMessage = "El número de cuotas es obligatorio")]
        [Range(1, 36, ErrorMessage = "El número de cuotas debe estar entre 1 y 36")]
        public int NumeroCuotas { get; set; }

        [Required(ErrorMessage = "La fecha de vencimiento es obligatoria")]
        [DataType(DataType.Date)]
        public DateTime FechaVencimiento { get; set; }

        public int PlazoMaximo { get; set; }
        public IEnumerable<SelectListItem> PlazosDisponibles { get; set; }
    }

    public class PagoCuotaViewModel
    {
        public int VentaID { get; set; }
        public int CuotaID { get; set; }
        public int NumeroCuota { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public decimal MontoCuota { get; set; }
        public int? DiasAtraso { get; set; }
        public decimal? MontoMora { get; set; }

        [Required(ErrorMessage = "La fecha de pago es obligatoria")]
        [DataType(DataType.Date)]
        public DateTime FechaPago { get; set; }

        [Required(ErrorMessage = "El monto total a pagar es obligatorio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a cero")]
        public decimal MontoTotal { get; set; }

        [Required(ErrorMessage = "La forma de pago es obligatoria")]
        public string FormaPago { get; set; }

        public string Referencia { get; set; }
    }
}