using javo2.ViewModels.Operaciones.Productos;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace javo2.IServices
{
    public interface ICatalogoService
    {
        Task<IEnumerable<RubroViewModel>> GetRubroViewModelsAsync();
        Task<IEnumerable<MarcaViewModel>> GetMarcaViewModelsAsync();
        Task<IEnumerable<SelectListItem>> GetRubrosAsync();
        Task<IEnumerable<SelectListItem>> GetSubRubrosAsync();
        Task<IEnumerable<SelectListItem>> GetMarcasAsync();
        Task<IEnumerable<SelectListItem>> GetSubRubrosByRubroAsync(string rubroNombre);
        Task CreateRubroAsync(RubroViewModel model);
        Task CreateSubRubroAsync(SubRubroViewModel model);
        Task CreateMarcaAsync(MarcaViewModel model);
        Task UpdateRubroAsync(RubroViewModel model);
        Task UpdateSubRubroAsync(SubRubroViewModel model);
        Task UpdateMarcaAsync(MarcaViewModel model);
        Task DeleteRubroAsync(int id);
        Task DeleteSubRubroAsync(int id);
        Task DeleteMarcaAsync(int id);
        Task<RubroViewModel?> GetRubroByIdAsync(int id);
        Task<SubRubroViewModel?> GetSubRubroByIdAsync(int id);
        Task<MarcaViewModel?> GetMarcaByIdAsync(int id);
    }
}
