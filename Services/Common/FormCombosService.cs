// Services/Common/FormCombosService.cs
using Javo2.IServices;
using Javo2.ViewModels.Operaciones.Ventas;
using System.Threading.Tasks;

namespace Javo2.Services.Common
{
    public class FormCombosService : IFormCombosService
    {
        private readonly IVentaService _ventaService;

        public FormCombosService(IVentaService ventaService)
        {
            _ventaService = ventaService;
        }

        public async Task CargarCombosVentaAsync(VentaFormViewModel model)
        {
            model.FormasPago = _ventaService.GetFormasPagoSelectList();
            model.Bancos = _ventaService.GetBancosSelectList();
            model.TipoTarjetaOptions = _ventaService.GetTipoTarjetaSelectList();
            model.CuotasOptions = _ventaService.GetCuotasSelectList();
            model.EntidadesElectronicas = _ventaService.GetEntidadesElectronicasSelectList();
            model.PlanesFinanciamiento = _ventaService.GetPlanesFinanciamientoSelectList();
        }

        public async Task CargarCombosCotizacionAsync(CotizacionViewModel model)
        {
            model.FormasPago = _ventaService.GetFormasPagoSelectList();
            model.Bancos = _ventaService.GetBancosSelectList();
            model.TipoTarjetaOptions = _ventaService.GetTipoTarjetaSelectList();
            model.CuotasOptions = _ventaService.GetCuotasSelectList();
            model.EntidadesElectronicas = _ventaService.GetEntidadesElectronicasSelectList();
            model.PlanesFinanciamiento = _ventaService.GetPlanesFinanciamientoSelectList();
        }
    }

    public interface IFormCombosService
    {
        Task CargarCombosVentaAsync(VentaFormViewModel model);
        Task CargarCombosCotizacionAsync(CotizacionViewModel model);
    }
}