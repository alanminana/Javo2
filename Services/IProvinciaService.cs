using Javo2.ViewModels.Operaciones.Clientes;

namespace Javo2.Services
{
    public interface IProvinciaService
    {
        IEnumerable<ProvinciaViewModel> GetAllProvincias();
        IEnumerable<CiudadViewModel> GetCiudadesByProvinciaId(int provinciaId);
    }
}