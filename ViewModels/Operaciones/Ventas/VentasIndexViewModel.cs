// ViewModels/Operaciones/Ventas/VentasIndexViewModel.cs
using System.Collections.Generic;

namespace Javo2.ViewModels.Operaciones.Ventas
{
    public class VentasIndexViewModel
    {
        public IEnumerable<VentaListViewModel> Ventas { get; set; } = new List<VentaListViewModel>();
        public IEnumerable<CotizacionListViewModel> Cotizaciones { get; set; } = new List<CotizacionListViewModel>();
    }
}