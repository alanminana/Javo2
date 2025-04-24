// File: Services/CatalogoService.cs
using Javo2.IServices;
using Javo2.Models;
using Javo2.ViewModels.Operaciones.Catalogo;
using Javo2.DTOs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Javo2.Helpers;

namespace Javo2.Services
{
    public class CatalogoService : ICatalogoService
    {
        private readonly ILogger<CatalogoService> _logger;
        private readonly string _jsonFilePath = "Data/catalogo.json";
        private static readonly object _lock = new();

        private List<Rubro> _rubros = new();
        private List<Marca> _marcas = new();
        private int _nextRubroID;
        private int _nextSubRubroID;
        private int _nextMarcaID;
        private CatalogoData _catalogoData = new();

        public CatalogoService(ILogger<CatalogoService> logger)
        {
            _logger = logger;
            LoadCatalogoData();

            if (_catalogoData == null || !_catalogoData.Rubros.Any() || !_catalogoData.Marcas.Any())
            {
                SeedData();
            }
            else
            {
                _rubros = _catalogoData.Rubros;
                _marcas = _catalogoData.Marcas;
                _nextRubroID = _catalogoData.NextRubroID;
                _nextSubRubroID = _catalogoData.NextSubRubroID;
                _nextMarcaID = _catalogoData.NextMarcaID;
            }
        }

        private void LoadCatalogoData()
        {
            try
            {
                _catalogoData = JsonFileHelper.LoadFromJsonFile<CatalogoData>(_jsonFilePath);
                if (_catalogoData == null)
                    _catalogoData = new CatalogoData();

                _logger.LogInformation("Catalogo data loaded from {File}", _jsonFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading catalogo data");
                _catalogoData = new CatalogoData();
            }
        }

        private void SaveCatalogoData()
        {
            lock (_lock)
            {
                try
                {
                    _catalogoData.Rubros = _rubros;
                    _catalogoData.Marcas = _marcas;
                    _catalogoData.NextRubroID = _nextRubroID;
                    _catalogoData.NextSubRubroID = _nextSubRubroID;
                    _catalogoData.NextMarcaID = _nextMarcaID;

                    JsonFileHelper.SaveToJsonFile(_jsonFilePath, _catalogoData);
                    _logger.LogInformation("Catalogo data saved to {File}", _jsonFilePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving catalogo data");
                }
            }
        }

        private void SeedData()
        {
            _rubros = new List<Rubro>();
            _marcas = new List<Marca>();
            _nextRubroID = 1;
            _nextSubRubroID = 1;
            _nextMarcaID = 1;

            var rubro1 = new Rubro { ID = _nextRubroID++, Nombre = "Electrónica" };
            var rubro2 = new Rubro { ID = _nextRubroID++, Nombre = "Hogar" };
            _rubros.AddRange(new[] { rubro1, rubro2 });

            rubro1.SubRubros.Add(new SubRubro { ID = _nextSubRubroID++, Nombre = "Televisores", RubroID = rubro1.ID });
            rubro1.SubRubros.Add(new SubRubro { ID = _nextSubRubroID++, Nombre = "Computadoras", RubroID = rubro1.ID });
            rubro2.SubRubros.Add(new SubRubro { ID = _nextSubRubroID++, Nombre = "Muebles", RubroID = rubro2.ID });
            rubro2.SubRubros.Add(new SubRubro { ID = _nextSubRubroID++, Nombre = "Electrodomésticos", RubroID = rubro2.ID });

            _marcas.Add(new Marca { ID = _nextMarcaID++, Nombre = "Marca A" });
            _marcas.Add(new Marca { ID = _nextMarcaID++, Nombre = "Marca B" });

            _catalogoData = new CatalogoData
            {
                Rubros = _rubros,
                Marcas = _marcas,
                NextRubroID = _nextRubroID,
                NextSubRubroID = _nextSubRubroID,
                NextMarcaID = _nextMarcaID
            };

            SaveCatalogoData();
            _logger.LogInformation("Seed data created for Catalogo");
        }

        // RUBRO
        public Task<IEnumerable<Rubro>> GetRubrosAsync()
        {
            lock (_lock)
            {
                return Task.FromResult<IEnumerable<Rubro>>(_rubros.AsEnumerable());
            }
        }

        public Task<Rubro?> GetRubroByIDAsync(int id)
        {
            lock (_lock)
            {
                return Task.FromResult(_rubros.FirstOrDefault(r => r.ID == id));
            }
        }

        public Task CreateRubroAsync(Rubro rubro)
        {
            if (string.IsNullOrWhiteSpace(rubro.Nombre))
                throw new ArgumentException("El nombre del rubro no puede estar vacío.");

            lock (_lock)
            {
                rubro.ID = _nextRubroID++;
                _rubros.Add(rubro);
                SaveCatalogoData();
            }

            _logger.LogInformation("Rubro creado: {Nombre}", rubro.Nombre);
            return Task.CompletedTask;
        }

        public Task UpdateRubroAsync(Rubro rubro)
        {
            if (string.IsNullOrWhiteSpace(rubro.Nombre))
                throw new ArgumentException("El nombre del rubro no puede estar vacío.");

            lock (_lock)
            {
                var existingRubro = _rubros.FirstOrDefault(r => r.ID == rubro.ID);
                if (existingRubro != null)
                {
                    existingRubro.Nombre = rubro.Nombre;
                    SaveCatalogoData();
                    _logger.LogInformation("Rubro actualizado: ID={ID}", rubro.ID);
                }
            }

            return Task.CompletedTask;
        }

        public Task DeleteRubroAsync(int id)
        {
            lock (_lock)
            {
                var rubro = _rubros.FirstOrDefault(r => r.ID == id);
                if (rubro != null)
                {
                    _rubros.Remove(rubro);
                    SaveCatalogoData();
                    _logger.LogInformation("Rubro eliminado: ID={ID}", id);
                }
            }

            return Task.CompletedTask;
        }

        // SUBRUBRO
        public Task<IEnumerable<SubRubro>> GetSubRubrosByRubroIDAsync(int rubroId)
        {
            lock (_lock)
            {
                var rubro = _rubros.FirstOrDefault(r => r.ID == rubroId);
                if (rubro == null)
                    return Task.FromResult<IEnumerable<SubRubro>>(new List<SubRubro>());

                return Task.FromResult<IEnumerable<SubRubro>>(rubro.SubRubros);
            }
        }

        public Task<SubRubro?> GetSubRubroByIDAsync(int id)
        {
            lock (_lock)
            {
                var subRubro = _rubros
                    .SelectMany(r => r.SubRubros)
                    .FirstOrDefault(sr => sr.ID == id);
                return Task.FromResult(subRubro);
            }
        }

        // In CatalogoService.cs
        public Task CreateSubRubroAsync(SubRubro subRubro)
        {
            if (string.IsNullOrWhiteSpace(subRubro.Nombre))
                throw new ArgumentException("El nombre del subrubro no puede estar vacío.");

            lock (_lock)
            {
                var rubro = _rubros.FirstOrDefault(r => r.ID == subRubro.RubroID);
                if (rubro == null)
                    throw new KeyNotFoundException($"Rubro con ID {subRubro.RubroID} no encontrado.");

                subRubro.ID = _nextSubRubroID++;
                rubro.SubRubros.Add(subRubro);
                SaveCatalogoData();
                _logger.LogInformation("SubRubro creado: {Nombre}", subRubro.Nombre);
            }

            return Task.CompletedTask;
        }

        public Task UpdateSubRubroAsync(SubRubro subRubro)
        {
            if (string.IsNullOrWhiteSpace(subRubro.Nombre))
                throw new ArgumentException("El nombre del subrubro no puede estar vacío.");

            lock (_lock)
            {
                var rubro = _rubros.FirstOrDefault(r => r.ID == subRubro.RubroID);
                if (rubro != null)
                {
                    var existingSubRubro = rubro.SubRubros.FirstOrDefault(sr => sr.ID == subRubro.ID);
                    if (existingSubRubro != null)
                    {
                        existingSubRubro.Nombre = subRubro.Nombre;
                        SaveCatalogoData();
                        _logger.LogInformation("SubRubro actualizado: ID={ID}", subRubro.ID);
                    }
                }
            }

            return Task.CompletedTask;
        }

        public Task DeleteSubRubroAsync(int id)
        {
            lock (_lock)
            {
                foreach (var rubro in _rubros)
                {
                    var subRubro = rubro.SubRubros.FirstOrDefault(sr => sr.ID == id);
                    if (subRubro != null)
                    {
                        rubro.SubRubros.Remove(subRubro);
                        SaveCatalogoData();
                        _logger.LogInformation("SubRubro eliminado: ID={ID}", id);
                        break;
                    }
                }
            }

            return Task.CompletedTask;
        }

        public Task UpdateSubRubrosAsync(EditSubRubrosViewModel model)
        {
            lock (_lock)
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
                                _logger.LogInformation("SubRubro eliminado: ID={ID}", subRubro.ID);
                            }
                            else
                            {
                                subRubro.Nombre = subRubroEdit.Nombre;
                                _logger.LogInformation("SubRubro actualizado: ID={ID}", subRubro.ID);
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
                            _logger.LogInformation("SubRubro creado: {Nombre}", newSubRubro.Nombre);
                        }
                    }
                    SaveCatalogoData();
                }
            }

            return Task.CompletedTask;
        }

        // MARCA
        public Task<IEnumerable<Marca>> GetMarcasAsync()
        {
            lock (_lock)
            {
                return Task.FromResult<IEnumerable<Marca>>(_marcas.AsEnumerable());
            }
        }

        public Task<Marca?> GetMarcaByIDAsync(int id)
        {
            lock (_lock)
            {
                return Task.FromResult(_marcas.FirstOrDefault(m => m.ID == id));
            }
        }

        public Task CreateMarcaAsync(Marca marca)
        {
            if (string.IsNullOrWhiteSpace(marca.Nombre))
                throw new ArgumentException("El nombre de la marca no puede estar vacío.");

            lock (_lock)
            {
                marca.ID = _nextMarcaID++;
                _marcas.Add(marca);
                SaveCatalogoData();
            }

            _logger.LogInformation("Marca creada: {Nombre}", marca.Nombre);
            return Task.CompletedTask;
        }

        public Task UpdateMarcaAsync(Marca marca)
        {
            if (string.IsNullOrWhiteSpace(marca.Nombre))
                throw new ArgumentException("El nombre de la marca no puede estar vacío.");

            lock (_lock)
            {
                var existingMarca = _marcas.FirstOrDefault(m => m.ID == marca.ID);
                if (existingMarca != null)
                {
                    existingMarca.Nombre = marca.Nombre;
                    SaveCatalogoData();
                    _logger.LogInformation("Marca actualizada: ID={ID}", marca.ID);
                }
            }

            return Task.CompletedTask;
        }

        public Task DeleteMarcaAsync(int id)
        {
            lock (_lock)
            {
                var marca = _marcas.FirstOrDefault(m => m.ID == id);
                if (marca != null)
                {
                    _marcas.Remove(marca);
                    SaveCatalogoData();
                    _logger.LogInformation("Marca eliminada: ID={ID}", id);
                }
            }

            return Task.CompletedTask;
        }

        // FILTROS
        public Task<IEnumerable<Rubro>> FilterRubrosAsync(CatalogoFilterDto filters)
        {
            lock (_lock)
            {
                var query = _rubros.AsQueryable();

                if (!string.IsNullOrEmpty(filters.Nombre))
                    query = query.Where(r => r.Nombre.Contains(filters.Nombre, StringComparison.OrdinalIgnoreCase));

                return Task.FromResult<IEnumerable<Rubro>>(query.ToList());
            }
        }

        public Task<IEnumerable<Marca>> FilterMarcasAsync(CatalogoFilterDto filters)
        {
            lock (_lock)
            {
                var query = _marcas.AsQueryable();

                if (!string.IsNullOrEmpty(filters.Nombre))
                    query = query.Where(m => m.Nombre.Contains(filters.Nombre, StringComparison.OrdinalIgnoreCase));

                return Task.FromResult<IEnumerable<Marca>>(query.ToList());
            }
        }
    }
}