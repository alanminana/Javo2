// Archivo: IServices/ICatalogoService.cs
using Javo2.Models;
using Javo2.ViewModels.Operaciones.Catalogo;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Javo2.IServices
{
    public interface ICatalogoService
    {
        // Métodos para Rubros
        Task<IEnumerable<Rubro>> GetRubrosAsync();
        Task<Rubro?> GetRubroByIdAsync(int id);
        Task CreateRubroAsync(Rubro rubro);
        Task UpdateRubroAsync(Rubro rubro);
        Task DeleteRubroAsync(int id);

        // Métodos para Marcas
        Task<IEnumerable<Marca>> GetMarcasAsync();
        Task<Marca?> GetMarcaByIdAsync(int id);
        Task CreateMarcaAsync(Marca marca);
        Task UpdateMarcaAsync(Marca marca);
        Task DeleteMarcaAsync(int id);

        // Métodos para SubRubros
        Task<IEnumerable<SubRubro>> GetSubRubrosByRubroIdAsync(int rubroId);
        Task<SubRubro?> GetSubRubroByIdAsync(int id);
        Task CreateSubRubroAsync(SubRubro subRubro);
        Task UpdateSubRubroAsync(SubRubro subRubro);
        Task DeleteSubRubroAsync(int id);
        Task UpdateSubRubrosAsync(EditSubRubrosViewModel model);

 
        // Métodos para filtrado
        Task<IEnumerable<Rubro>> FilterRubrosAsync(CatalogoFilterDto filters);
        Task<IEnumerable<Marca>> FilterMarcasAsync(CatalogoFilterDto filters);
    }
}
