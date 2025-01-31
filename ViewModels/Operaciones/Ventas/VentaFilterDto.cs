// Archivo: ViewModels/Operaciones/Ventas/VentaFilterDto.cs
using System;

namespace Javo2.ViewModels.Operaciones.Ventas
{
    public class VentaFilterDto
    {
        public string? NombreCliente { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string? NumeroFactura { get; set; }
    }
}
