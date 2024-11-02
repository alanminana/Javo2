// Archivo: Services/CatalogoService.cs
using Javo2.IServices;
using Javo2.Models;
using Javo2.ViewModels.Operaciones.Catalogo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Javo2.Services
{
    public class CatalogoService : ICatalogoService
    {
        private readonly List<Rubro> _rubros;
        private readonly List<Marca> _marcas;

        public CatalogoService()
        {
            _rubros = new List<Rubro>();
            _marcas = new List<Marca>();
            SeedData();
        }

        private void SeedData()
        {
            var rubro1 = new Rubro { Id = 1, Nombre = "Electrodomésticos", SubRubros = new List<SubRubro>() };
            var rubro2 = new Rubro { Id = 2, Nombre = "Electrónica", SubRubros = new List<SubRubro>() };

            rubro1.SubRubros.Add(new SubRubro { Id = 1, Nombre = "Refrigeradores", RubroId = 1 });
            rubro1.SubRubros.Add(new SubRubro { Id = 2, Nombre = "Lavadoras", RubroId = 1 });

            rubro2.SubRubros.Add(new SubRubro { Id = 3, Nombre = "Televisores", RubroId = 2 });
            rubro2.SubRubros.Add(new SubRubro { Id = 4, Nombre = "Audio", RubroId = 2 });

            _rubros.Add(rubro1);
            _rubros.Add(rubro2);

            _marcas.Add(new Marca { Id = 1, Nombre = "Samsung" });
            _marcas.Add(new Marca { Id = 2, Nombre = "LG" });
        }

        // Implementación de métodos para Rubros

        public Task<IEnumerable<Rubro>> GetRubrosAsync()
        {
            return Task.FromResult(_rubros.AsEnumerable());
        }

        public Task<Rubro?> GetRubroByIdAsync(int id)
        {
            var rubro = _rubros.FirstOrDefault(r => r.Id == id);
            return Task.FromResult(rubro);
        }

        public Task CreateRubroAsync(Rubro rubro)
        {
            rubro.Id = _rubros.Any() ? _rubros.Max(r => r.Id) + 1 : 1;
            rubro.SubRubros = new List<SubRubro>();
            _rubros.Add(rubro);
            return Task.CompletedTask;
        }

        public Task UpdateRubroAsync(Rubro rubro)
        {
            var existingRubro = _rubros.FirstOrDefault(r => r.Id == rubro.Id);
            if (existingRubro == null)
                throw new KeyNotFoundException($"Rubro con ID {rubro.Id} no encontrado.");

            existingRubro.Nombre = rubro.Nombre;
            // Actualizar otras propiedades si es necesario

            return Task.CompletedTask;
        }

        public Task DeleteRubroAsync(int id)
        {
            var rubro = _rubros.FirstOrDefault(r => r.Id == id);
            if (rubro == null)
                throw new KeyNotFoundException($"Rubro con ID {id} no encontrado.");

            _rubros.Remove(rubro);
            return Task.CompletedTask;
        }

        // Implementación de métodos para Marcas

        public Task<IEnumerable<Marca>> GetMarcasAsync()
        {
            return Task.FromResult(_marcas.AsEnumerable());
        }

        public Task<Marca?> GetMarcaByIdAsync(int id)
        {
            var marca = _marcas.FirstOrDefault(m => m.Id == id);
            return Task.FromResult(marca);
        }

        public Task CreateMarcaAsync(Marca marca)
        {
            marca.Id = _marcas.Any() ? _marcas.Max(m => m.Id) + 1 : 1;
            _marcas.Add(marca);
            return Task.CompletedTask;
        }

        public Task UpdateMarcaAsync(Marca marca)
        {
            var existingMarca = _marcas.FirstOrDefault(m => m.Id == marca.Id);
            if (existingMarca == null)
                throw new KeyNotFoundException($"Marca con ID {marca.Id} no encontrada.");

            existingMarca.Nombre = marca.Nombre;
            // Actualizar otras propiedades si es necesario

            return Task.CompletedTask;
        }

        public Task DeleteMarcaAsync(int id)
        {
            var marca = _marcas.FirstOrDefault(m => m.Id == id);
            if (marca == null)
                throw new KeyNotFoundException($"Marca con ID {id} no encontrada.");

            _marcas.Remove(marca);
            return Task.CompletedTask;
        }

        // Implementación de métodos para SubRubros

        public Task<IEnumerable<SubRubro>> GetSubRubrosByRubroIdAsync(int rubroId)
        {
            var rubro = _rubros.FirstOrDefault(r => r.Id == rubroId);
            if (rubro == null)
                throw new KeyNotFoundException($"Rubro con ID {rubroId} no encontrado.");

            return Task.FromResult(rubro.SubRubros.AsEnumerable());
        }

        public Task<SubRubro?> GetSubRubroByIdAsync(int id)
        {
            var subRubro = _rubros.SelectMany(r => r.SubRubros).FirstOrDefault(sr => sr.Id == id);
            return Task.FromResult(subRubro);
        }

        public Task CreateSubRubroAsync(SubRubro subRubro)
        {
            var rubro = _rubros.FirstOrDefault(r => r.Id == subRubro.RubroId);
            if (rubro == null)
                throw new KeyNotFoundException($"Rubro con ID {subRubro.RubroId} no encontrado.");

            subRubro.Id = rubro.SubRubros.Any() ? rubro.SubRubros.Max(sr => sr.Id) + 1 : 1;
            rubro.SubRubros.Add(subRubro);
            return Task.CompletedTask;
        }

        public Task UpdateSubRubroAsync(SubRubro subRubro)
        {
            var rubro = _rubros.FirstOrDefault(r => r.Id == subRubro.RubroId);
            if (rubro == null)
                throw new KeyNotFoundException($"Rubro con ID {subRubro.RubroId} no encontrado.");

            var existingSubRubro = rubro.SubRubros.FirstOrDefault(sr => sr.Id == subRubro.Id);
            if (existingSubRubro == null)
                throw new KeyNotFoundException($"SubRubro con ID {subRubro.Id} no encontrado.");

            existingSubRubro.Nombre = subRubro.Nombre;
            return Task.CompletedTask;
        }

        public Task DeleteSubRubroAsync(int id)
        {
            foreach (var rubro in _rubros)
            {
                var subRubro = rubro.SubRubros.FirstOrDefault(sr => sr.Id == id);
                if (subRubro != null)
                {
                    rubro.SubRubros.Remove(subRubro);
                    return Task.CompletedTask;
                }
            }
            throw new KeyNotFoundException($"SubRubro con ID {id} no encontrado.");
        }

        public Task UpdateSubRubrosAsync(EditSubRubrosViewModel model)
        {
            var existingRubro = _rubros.FirstOrDefault(r => r.Id == model.RubroId);
            if (existingRubro == null)
                throw new KeyNotFoundException($"Rubro con ID {model.RubroId} no encontrado.");

            // Actualizar SubRubros
            foreach (var subRubroModel in model.SubRubros)
            {
                var existingSubRubro = existingRubro.SubRubros.FirstOrDefault(sr => sr.Id == subRubroModel.Id);

                if (subRubroModel.IsDeleted)
                {
                    if (existingSubRubro != null)
                    {
                        existingRubro.SubRubros.Remove(existingSubRubro);
                    }
                }
                else
                {
                    if (existingSubRubro != null)
                    {
                        existingSubRubro.Nombre = subRubroModel.Nombre;
                    }
                    else
                    {
                        var newSubRubro = new SubRubro
                        {
                            Id = existingRubro.SubRubros.Any() ? existingRubro.SubRubros.Max(sr => sr.Id) + 1 : 1,
                            Nombre = subRubroModel.Nombre,
                            RubroId = existingRubro.Id
                        };
                        existingRubro.SubRubros.Add(newSubRubro);
                    }
                }
            }

            return Task.CompletedTask;
        }

        // Implementación de métodos para filtrado

        public Task<IEnumerable<Rubro>> FilterRubrosAsync(CatalogoFilterDto filters)
        {
            var query = _rubros.AsQueryable();

            if (!string.IsNullOrEmpty(filters.Nombre))
            {
                query = query.Where(r => r.Nombre.Contains(filters.Nombre, StringComparison.OrdinalIgnoreCase));
            }

            // Implementar otros filtros según sea necesario

            return Task.FromResult(query.AsEnumerable());
        }

        public Task<IEnumerable<Marca>> FilterMarcasAsync(CatalogoFilterDto filters)
        {
            var query = _marcas.AsQueryable();

            if (!string.IsNullOrEmpty(filters.Nombre))
            {
                query = query.Where(m => m.Nombre.Contains(filters.Nombre, StringComparison.OrdinalIgnoreCase));
            }

            // Implementar otros filtros según sea necesario

            return Task.FromResult(query.AsEnumerable());
        }
    }
}
