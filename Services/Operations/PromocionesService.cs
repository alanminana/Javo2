using Javo2.IServices;
using Javo2.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Javo2.Services.Operations
{
    public class PromocionesService : IPromocionesService
    {
        private readonly ILogger<PromocionesService> _logger;
        private static readonly List<Promocion> _promociones = new();
        private static int _nextPromocionID = 1;
        private static readonly object _lock = new();

        public PromocionesService(ILogger<PromocionesService> logger)
        {
            _logger = logger;
            if (!_promociones.Any())
            {
                SeedData();
            }
        }

        private void SeedData()
        {
            _promociones.Add(new Promocion
            {
                PromocionID = _nextPromocionID++,
                Nombre = "Descuento Rubro Electrónica",
                Descripcion = "10% de descuento en todos los productos del rubro Electrónica",
                Porcentaje = 10,
                EsAumento = false,
                RubroID = 1,
                Activa = true
            });
            _logger.LogInformation("Promociones seed data added");
        }

        public Task<IEnumerable<Promocion>> GetPromocionesAsync()
        {
            lock (_lock)
            {
                return Task.FromResult<IEnumerable<Promocion>>(_promociones.ToList());
            }
        }

        public Task<Promocion?> GetPromocionByIDAsync(int id)
        {
            lock (_lock)
            {
                var promo = _promociones.FirstOrDefault(p => p.PromocionID == id);
                return Task.FromResult(promo);
            }
        }

        public Task CreatePromocionAsync(Promocion promocion)
        {
            if (string.IsNullOrWhiteSpace(promocion.Nombre))
                throw new ArgumentException("El nombre de la promoción no puede estar vacío.");

            lock (_lock)
            {
                promocion.PromocionID = _nextPromocionID++;
                _promociones.Add(promocion);
            }
            _logger.LogInformation("Promoción creada: {Nombre}", promocion.Nombre);
            return Task.CompletedTask;
        }

        public Task UpdatePromocionAsync(Promocion promocion)
        {
            if (string.IsNullOrWhiteSpace(promocion.Nombre))
                throw new ArgumentException("El nombre de la promoción no puede estar vacío.");

            lock (_lock)
            {
                var existing = _promociones.FirstOrDefault(p => p.PromocionID == promocion.PromocionID);
                if (existing != null)
                {
                    existing.Nombre = promocion.Nombre;
                    existing.Descripcion = promocion.Descripcion;
                    existing.Porcentaje = promocion.Porcentaje;
                    existing.EsAumento = promocion.EsAumento;
                    existing.RubroID = promocion.RubroID;
                    existing.MarcaID = promocion.MarcaID;
                    existing.SubRubroID = promocion.SubRubroID;
                    existing.FechaInicio = promocion.FechaInicio;
                    existing.FechaFin = promocion.FechaFin;
                    existing.Activa = promocion.Activa;
                }
            }
            _logger.LogInformation("Promoción actualizada: {Nombre}", promocion.Nombre);
            return Task.CompletedTask;
        }

        public Task DeletePromocionAsync(int id)
        {
            lock (_lock)
            {
                var promo = _promociones.FirstOrDefault(p => p.PromocionID == id);
                if (promo != null) _promociones.Remove(promo);
            }
            _logger.LogInformation("Promoción eliminada con ID: {ID}", id);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<Promocion>> GetPromocionesAplicablesAsync(Producto producto)
        {
            var now = DateTime.UtcNow;
            var aplicables = _promociones.Where(p =>
                p.Activa &&
                (p.RubroID == null || p.RubroID == producto.RubroID) &&
                (p.MarcaID == null || p.MarcaID == producto.MarcaID) &&
                (p.SubRubroID == null || p.SubRubroID == producto.SubRubroID) &&
                (p.FechaInicio == null || p.FechaInicio <= now) &&
                (p.FechaFin == null || p.FechaFin >= now)
            );
            return Task.FromResult(aplicables);
        }
    }
}
