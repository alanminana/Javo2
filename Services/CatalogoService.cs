using Javo2.IServices;
using Javo2.Models;
using Javo2.ViewModels.Operaciones.Catalogo;
using Javo2.DTOs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Javo2.Services
{
    public class CatalogoService : ICatalogoService
    {
        private readonly List<Rubro> _rubros = new();
        private readonly List<Marca> _marcas = new();
        private int _nextRubroID = 1;
        private int _nextSubRubroID = 1;
        private int _nextMarcaID = 1;
        private readonly ILogger<CatalogoService> _logger;

        public CatalogoService(ILogger<CatalogoService> logger)
        {
            _logger = logger;
            SeedData();
        }

        private void SeedData()
        {
            var rubro1 = new Rubro { ID = _nextRubroID++, Nombre = "Electrónica" };
            var rubro2 = new Rubro { ID = _nextRubroID++, Nombre = "Hogar" };
            _rubros.AddRange(new[] { rubro1, rubro2 });

            rubro1.SubRubros.Add(new SubRubro { ID = _nextSubRubroID++, Nombre = "Televisores", RubroID = rubro1.ID });
            rubro1.SubRubros.Add(new SubRubro { ID = _nextSubRubroID++, Nombre = "Computadoras", RubroID = rubro1.ID });

            rubro2.SubRubros.Add(new SubRubro { ID = _nextSubRubroID++, Nombre = "Muebles", RubroID = rubro2.ID });
            rubro2.SubRubros.Add(new SubRubro { ID = _nextSubRubroID++, Nombre = "Electrodomésticos", RubroID = rubro2.ID });

            _marcas.Add(new Marca { ID = _nextMarcaID++, Nombre = "Marca A" });
            _marcas.Add(new Marca { ID = _nextMarcaID++, Nombre = "Marca B" });

            _logger.LogInformation("SeedData completed in CatalogoService");
        }

        public Task<IEnumerable<Rubro>> GetRubrosAsync() => Task.FromResult<IEnumerable<Rubro>>(_rubros);

        public Task<Rubro?> GetRubroByIDAsync(int id) =>
            Task.FromResult(_rubros.FirstOrDefault(r => r.ID == id));

        public Task CreateRubroAsync(Rubro rubro)
        {
            if (string.IsNullOrWhiteSpace(rubro.Nombre))
                throw new ArgumentException("El nombre del rubro no puede estar vacío.");

            rubro.ID = _nextRubroID++;
            _rubros.Add(rubro);
            _logger.LogInformation("Rubro creado: {@Rubro}", rubro);
            return Task.CompletedTask;
        }

        public Task UpdateRubroAsync(Rubro rubro)
        {
            var existingRubro = _rubros.FirstOrDefault(r => r.ID == rubro.ID);
            if (existingRubro != null)
            {
                if (string.IsNullOrWhiteSpace(rubro.Nombre))
                    throw new ArgumentException("El nombre del rubro no puede estar vacío.");

                existingRubro.Nombre = rubro.Nombre;
                _logger.LogInformation("Rubro actualizado: {@Rubro}", rubro);
            }
            return Task.CompletedTask;
        }

        public Task DeleteRubroAsync(int id)
        {
            var rubro = _rubros.FirstOrDefault(r => r.ID == id);
            if (rubro != null)
            {
                _rubros.Remove(rubro);
                _logger.LogInformation("Rubro eliminado: {@Rubro}", rubro);
            }
            return Task.CompletedTask;
        }

        public Task<IEnumerable<SubRubro>> GetSubRubrosByRubroIDAsync(int rubroId)
        {
            _logger.LogInformation("GetSubRubrosByRubroIDAsync called with RubroID: {RubroID}", rubroId);
            var rubro = _rubros.FirstOrDefault(r => r.ID == rubroId);
            if (rubro == null)
            {
                _logger.LogWarning("Rubro with ID {RubroID} not found", rubroId);
                return Task.FromResult<IEnumerable<SubRubro>>(new List<SubRubro>());
            }

            var subRubros = rubro.SubRubros;
            _logger.LogInformation("SubRubros found for RubroID {RubroID}: {Count}", rubroId, subRubros.Count);
            return Task.FromResult<IEnumerable<SubRubro>>(subRubros);
        }

        public Task<SubRubro?> GetSubRubroByIDAsync(int id)
        {
            var subRubro = _rubros.SelectMany(r => r.SubRubros).FirstOrDefault(sr => sr.ID == id);
            return Task.FromResult(subRubro);
        }

        public Task CreateSubRubroAsync(SubRubro subRubro)
        {
            if (string.IsNullOrWhiteSpace(subRubro.Nombre))
                throw new ArgumentException("El nombre del subrubro no puede estar vacío.");

            subRubro.ID = _nextSubRubroID++;
            var rubro = _rubros.FirstOrDefault(r => r.ID == subRubro.RubroID);
            if (rubro != null)
            {
                rubro.SubRubros.Add(subRubro);
                _logger.LogInformation("SubRubro creado: {@SubRubro}", subRubro);
            }
            return Task.CompletedTask;
        }

        public Task UpdateSubRubroAsync(SubRubro subRubro)
        {
            if (string.IsNullOrWhiteSpace(subRubro.Nombre))
                throw new ArgumentException("El nombre del subrubro no puede estar vacío.");

            var rubro = _rubros.FirstOrDefault(r => r.ID == subRubro.RubroID);
            if (rubro != null)
            {
                var existingSubRubro = rubro.SubRubros.FirstOrDefault(sr => sr.ID == subRubro.ID);
                if (existingSubRubro != null)
                {
                    existingSubRubro.Nombre = subRubro.Nombre;
                    _logger.LogInformation("SubRubro actualizado: {@SubRubro}", subRubro);
                }
            }
            return Task.CompletedTask;
        }

        public Task DeleteSubRubroAsync(int id)
        {
            foreach (var rubro in _rubros)
            {
                var subRubro = rubro.SubRubros.FirstOrDefault(sr => sr.ID == id);
                if (subRubro != null)
                {
                    rubro.SubRubros.Remove(subRubro);
                    _logger.LogInformation("SubRubro eliminado: {@SubRubro}", subRubro);
                    break;
                }
            }
            return Task.CompletedTask;
        }

        public Task UpdateSubRubrosAsync(EditSubRubrosViewModel model)
        {
            var rubro = _rubros.FirstOrDefault(r => r.ID == model.RubroID);
            if (rubro != null)
            {
                foreach (var subRubroEdit in model.SubRubros)
                {
                    if (string.IsNullOrWhiteSpace(subRubroEdit.Nombre) && !subRubroEdit.IsDeleted)
                        throw new ArgumentException("El nombre del subrubro no puede estar vacío.");

                    var subRubro = rubro.SubRubros.FirstOrDefault(sr => sr.ID == subRubroEdit.ID);
                    if (subRubro != null)
                    {
                        if (subRubroEdit.IsDeleted)
                        {
                            rubro.SubRubros.Remove(subRubro);
                            _logger.LogInformation("SubRubro eliminado: {@SubRubro}", subRubro);
                        }
                        else
                        {
                            subRubro.Nombre = subRubroEdit.Nombre;
                            _logger.LogInformation("SubRubro actualizado: {@SubRubro}", subRubro);
                        }
                    }
                    else if (!subRubroEdit.IsDeleted)
                    {
                        var newSubRubro = new SubRubro
                        {
                            ID = _nextSubRubroID++,
                            Nombre = subRubroEdit.Nombre,
                            RubroID = model.RubroID
                        };
                        rubro.SubRubros.Add(newSubRubro);
                        _logger.LogInformation("SubRubro creado: {@SubRubro}", newSubRubro);
                    }
                }
            }

            return Task.CompletedTask;
        }

        public Task<IEnumerable<Marca>> GetMarcasAsync()
        {
            return Task.FromResult<IEnumerable<Marca>>(_marcas);
        }

        public Task<Marca?> GetMarcaByIDAsync(int id)
        {
            var marca = _marcas.FirstOrDefault(m => m.ID == id);
            return Task.FromResult(marca);
        }

        public Task CreateMarcaAsync(Marca marca)
        {
            if (string.IsNullOrWhiteSpace(marca.Nombre))
                throw new ArgumentException("El nombre de la marca no puede estar vacío.");

            marca.ID = _nextMarcaID++;
            _marcas.Add(marca);
            _logger.LogInformation("Marca creada: {@Marca}", marca);
            return Task.CompletedTask;
        }

        public Task UpdateMarcaAsync(Marca marca)
        {
            var existingMarca = _marcas.FirstOrDefault(m => m.ID == marca.ID);
            if (existingMarca != null)
            {
                if (string.IsNullOrWhiteSpace(marca.Nombre))
                    throw new ArgumentException("El nombre de la marca no puede estar vacío.");

                existingMarca.Nombre = marca.Nombre;
                _logger.LogInformation("Marca actualizada: {@Marca}", marca);
            }
            return Task.CompletedTask;
        }

        public Task DeleteMarcaAsync(int id)
        {
            var marca = _marcas.FirstOrDefault(m => m.ID == id);
            if (marca != null)
            {
                _marcas.Remove(marca);
                _logger.LogInformation("Marca eliminada: {@Marca}", marca);
            }
            return Task.CompletedTask;
        }

        public Task<IEnumerable<Rubro>> FilterRubrosAsync(CatalogoFilterDto filters)
        {
            _logger.LogInformation("FilterRubrosAsync called with filters: {@Filters}", filters);
            var query = _rubros.AsQueryable();
            if (!string.IsNullOrEmpty(filters.Nombre))
            {
                query = query.Where(r => r.Nombre.Contains(filters.Nombre, StringComparison.OrdinalIgnoreCase));
            }
            return Task.FromResult<IEnumerable<Rubro>>(query.ToList());
        }

        public Task<IEnumerable<Marca>> FilterMarcasAsync(CatalogoFilterDto filters)
        {
            _logger.LogInformation("FilterMarcasAsync called with filters: {@Filters}", filters);
            var query = _marcas.AsQueryable();
            if (!string.IsNullOrEmpty(filters.Nombre))
            {
                query = query.Where(m => m.Nombre.Contains(filters.Nombre, StringComparison.OrdinalIgnoreCase));
            }
            return Task.FromResult<IEnumerable<Marca>>(query.ToList());
        }
    }
}
