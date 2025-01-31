// Archivo: IServices/ICatalogoService.cs
using Javo2.Models;
using Javo2.ViewModels.Operaciones.Catalogo;
using Javo2.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Javo2.IServices
{
    public interface ICatalogoService
    {
        Task<IEnumerable<Rubro>> GetRubrosAsync();
        Task<Rubro?> GetRubroByIDAsync(int id);
        Task CreateRubroAsync(Rubro rubro);
        Task UpdateRubroAsync(Rubro rubro);
        Task DeleteRubroAsync(int id);
        Task<IEnumerable<SubRubro>> GetSubRubrosByRubroIDAsync(int rubroId);
        Task<SubRubro?> GetSubRubroByIDAsync(int id);
        Task CreateSubRubroAsync(SubRubro subRubro);
        Task UpdateSubRubroAsync(SubRubro subRubro);
        Task DeleteSubRubroAsync(int id);
        Task UpdateSubRubrosAsync(EditSubRubrosViewModel model);

        Task<IEnumerable<Marca>> GetMarcasAsync();
        Task<Marca?> GetMarcaByIDAsync(int id);
        Task CreateMarcaAsync(Marca marca);
        Task UpdateMarcaAsync(Marca marca);
        Task DeleteMarcaAsync(int id);

        Task<IEnumerable<Rubro>> FilterRubrosAsync(CatalogoFilterDto filters);
        Task<IEnumerable<Marca>> FilterMarcasAsync(CatalogoFilterDto filters);
    }
}
