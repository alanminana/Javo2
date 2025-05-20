// ViewModels/Operaciones/Ventas/VentasIndexViewModel.cs
using Javo2.ViewModels.Operaciones.DevolucionGarantia;
using Javo2.ViewModels.Operaciones.Ventas;
using Javo2.ViewModels.Operaciones.DevolucionGarantia;
using Javo2.ViewModels.Operaciones.Ventas;


namespace Javo2.ViewModels.Operaciones.Ventas
{
    public class VentasIndexViewModel
    {
        public IEnumerable<VentaListViewModel> Ventas { get; set; } = new List<VentaListViewModel>();
        public IEnumerable<CotizacionListViewModel> Cotizaciones { get; set; } = new List<CotizacionListViewModel>();
        public IEnumerable<DevolucionGarantiaListViewModel> Devoluciones { get; set; } = new List<DevolucionGarantiaListViewModel>();
    }
}


