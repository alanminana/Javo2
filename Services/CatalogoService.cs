using Javo2.IServices;
using Javo2.ViewModels.Operaciones.Catalogo;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Javo2.Services
{
    public class CatalogoService : ICatalogoService
    {
        private readonly List<RubroViewModel> _rubros;
        private readonly List<SubRubroViewModel> _subRubros;
        private readonly List<MarcaViewModel> _marcas;

        public CatalogoService()
        {
            _rubros = new List<RubroViewModel>
            {
                new RubroViewModel { Id = 1, Nombre = "Electrodomésticos", SubRubros = new List<SubRubroViewModel> {
                    new SubRubroViewModel { Id = 1, Nombre = "Pequeños Electrodomésticos" },
                    new SubRubroViewModel { Id = 2, Nombre = "Grandes Electrodomésticos" }
                }},
                new RubroViewModel { Id = 2, Nombre = "Electrónica", SubRubros = new List<SubRubroViewModel> {
                    new SubRubroViewModel { Id = 3, Nombre = "Televisión" },
                    new SubRubroViewModel { Id = 4, Nombre = "Audio" }
                }}
            };
            _subRubros = new List<SubRubroViewModel>
            {
                new SubRubroViewModel { Id = 1, Nombre = "Pequeños Electrodomésticos", RubroNombre = "Electrodomésticos" },
                new SubRubroViewModel { Id = 2, Nombre = "Grandes Electrodomésticos", RubroNombre = "Electrodomésticos" },
                new SubRubroViewModel { Id = 3, Nombre = "Televisión", RubroNombre = "Electrónica" },
                new SubRubroViewModel { Id = 4, Nombre = "Audio", RubroNombre = "Electrónica" }
            };
            _marcas = new List<MarcaViewModel>
            {
                new MarcaViewModel { Id = 1, Nombre = "Samsung" },
                new MarcaViewModel { Id = 2, Nombre = "LG" }
            };
        }

        public Task<IEnumerable<RubroViewModel>> GetRubroViewModelsAsync()
        {
            return Task.FromResult<IEnumerable<RubroViewModel>>(_rubros);
        }

        public Task<IEnumerable<MarcaViewModel>> GetMarcaViewModelsAsync()
        {
            return Task.FromResult<IEnumerable<MarcaViewModel>>(_marcas);
        }

        public Task<IEnumerable<SelectListItem>> GetRubrosAsync()
        {
            var rubros = _rubros.Select(r => new SelectListItem { Value = r.Id.ToString(), Text = r.Nombre });
            return Task.FromResult(rubros);
        }

        public Task<IEnumerable<SelectListItem>> GetSubRubrosAsync()
        {
            var subRubros = _subRubros.Select(sr => new SelectListItem { Value = sr.Id.ToString(), Text = sr.Nombre });
            return Task.FromResult(subRubros);
        }

        public Task<IEnumerable<SelectListItem>> GetMarcasAsync()
        {
            var marcas = _marcas.Select(m => new SelectListItem { Value = m.Id.ToString(), Text = m.Nombre });
            return Task.FromResult(marcas);
        }

        public Task<IEnumerable<SelectListItem>> GetSubRubrosByRubroAsync(string rubroNombre)
        {
            var subRubros = _subRubros
                .Where(sr => sr.RubroNombre == rubroNombre)
                .Select(sr => new SelectListItem { Value = sr.Id.ToString(), Text = sr.Nombre });
            return Task.FromResult(subRubros);
        }

        public Task CreateRubroAsync(RubroViewModel model)
        {
            model.Id = _rubros.Any() ? _rubros.Max(r => r.Id) + 1 : 1;
            _rubros.Add(model);
            return Task.CompletedTask;
        }

        public Task CreateSubRubroAsync(SubRubroViewModel model)
        {
            model.Id = _subRubros.Any() ? _subRubros.Max(sr => sr.Id) + 1 : 1;
            var rubro = _rubros.FirstOrDefault(r => r.Nombre == model.RubroNombre);
            if (rubro != null)
            {
                rubro.SubRubros.Add(model);
            }
            _subRubros.Add(model);
            return Task.CompletedTask;
        }

        public Task CreateMarcaAsync(MarcaViewModel model)
        {
            model.Id = _marcas.Any() ? _marcas.Max(m => m.Id) + 1 : 1;
            _marcas.Add(model);
            return Task.CompletedTask;
        }

        public Task<RubroViewModel?> GetRubroByIdAsync(int id)
        {
            var rubro = _rubros.FirstOrDefault(r => r.Id == id);
            return Task.FromResult<RubroViewModel?>(rubro);
        }

        public Task<SubRubroViewModel?> GetSubRubroByIdAsync(int id)
        {
            var subRubro = _subRubros.FirstOrDefault(sr => sr.Id == id);
            return Task.FromResult<SubRubroViewModel?>(subRubro);
        }

        public Task<MarcaViewModel?> GetMarcaByIdAsync(int id)
        {
            var marca = _marcas.FirstOrDefault(m => m.Id == id);
            return Task.FromResult<MarcaViewModel?>(marca);
        }

        public Task UpdateRubroAsync(RubroViewModel model)
        {
            var rubro = _rubros.FirstOrDefault(r => r.Id == model.Id);
            if (rubro != null)
            {
                rubro.Nombre = model.Nombre;
            }
            return Task.CompletedTask;
        }

        public Task UpdateSubRubroAsync(SubRubroViewModel model)
        {
            var subRubro = _subRubros.FirstOrDefault(sr => sr.Id == model.Id);
            if (subRubro != null)
            {
                subRubro.Nombre = model.Nombre;
                UpdateSubRubroInRubro(subRubro);
            }
            return Task.CompletedTask;
        }

        private void UpdateSubRubroInRubro(SubRubroViewModel subRubro)
        {
            var rubro = _rubros.FirstOrDefault(r => r.Nombre == subRubro.RubroNombre);
            if (rubro != null)
            {
                var rubroSubRubro = rubro.SubRubros.FirstOrDefault(sr => sr.Id == subRubro.Id);
                if (rubroSubRubro != null)
                {
                    rubroSubRubro.Nombre = subRubro.Nombre;
                }
            }
        }

        public Task UpdateMarcaAsync(MarcaViewModel model)
        {
            var marca = _marcas.FirstOrDefault(m => m.Id == model.Id);
            if (marca != null)
            {
                marca.Nombre = model.Nombre;
            }
            return Task.CompletedTask;
        }

        public Task DeleteRubroAsync(int id)
        {
            var rubro = _rubros.FirstOrDefault(r => r.Id == id);
            if (rubro != null)
            {
                _rubros.Remove(rubro);
            }
            return Task.CompletedTask;
        }

        public Task DeleteMarcaAsync(int id)
        {
            var marca = _marcas.FirstOrDefault(m => m.Id == id);
            if (marca != null)
            {
                _marcas.Remove(marca);
            }
            return Task.CompletedTask;
        }

        public Task DeleteSubRubroAsync(int id)
        {
            var subRubro = _subRubros.FirstOrDefault(sr => sr.Id == id);
            if (subRubro != null)
            {
                _subRubros.Remove(subRubro);
                RemoveSubRubroFromRubro(subRubro);
            }
            return Task.CompletedTask;
        }

        private void RemoveSubRubroFromRubro(SubRubroViewModel subRubro)
        {
            var rubro = _rubros.FirstOrDefault(r => r.Nombre == subRubro.RubroNombre);
            if (rubro != null)
            {
                var rubroSubRubro = rubro.SubRubros.FirstOrDefault(sr => sr.Id == subRubro.Id);
                if (rubroSubRubro != null)
                {
                    rubro.SubRubros.Remove(rubroSubRubro);
                }
            }
        }

        public Task<IEnumerable<RubroViewModel>> FilterRubrosAsync(CatalogoFilterDto filters)
        {

            var rubros = _rubros.AsQueryable();

            if (!string.IsNullOrEmpty(filters.Rubro))
            {
                rubros = rubros.Where(r => r.Nombre.Contains(filters.Rubro, StringComparison.OrdinalIgnoreCase));
            }

            return Task.FromResult(rubros.ToList().AsEnumerable());
        }

        public Task<IEnumerable<MarcaViewModel>> FilterMarcasAsync(CatalogoFilterDto filters)
        {

            var marcas = _marcas.AsQueryable();

            if (!string.IsNullOrEmpty(filters.Marca))
            {
                marcas = marcas.Where(m => m.Nombre.Contains(filters.Marca, StringComparison.OrdinalIgnoreCase));
            }

            return Task.FromResult(marcas.ToList().AsEnumerable());
        }

    }
}
