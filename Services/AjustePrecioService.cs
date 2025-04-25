// File: Services/AjustePrecioService.cs
using Javo2.IServices;
using Javo2.IServices.Authentication;
using Javo2.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Javo2.Services
{
    public class AjustePrecioService : IAjustePrecioService
    {
        private readonly IProductoService _productoService;
        private readonly ILogger<AjustePrecioService> _logger;

        public AjustePrecioService(IProductoService productoService, ILogger<AjustePrecioService> logger)
        {
            _productoService = productoService;
            _logger = logger;
        }

        public async Task AjustarPreciosAsync(IEnumerable<int> productoIDs, decimal porcentaje, bool esAumento)
        {
            foreach (var id in productoIDs)
            {
                var producto = await _productoService.GetProductoByIDAsync(id);
                if (producto == null)
                {
                    _logger.LogWarning("Producto con ID {ProductoID} no encontrado. Se omite.", id);
                    continue;
                }

                decimal factor = esAumento ? (1 + porcentaje / 100m) : (1 - porcentaje / 100m);

                _logger.LogInformation("Ajustando precios para ProductoID={ProductoID} con factor={Factor}", id, factor);

                // Guardamos precios anteriores para posible auditoría
                decimal precioAnteriorCosto = producto.PCosto;
                decimal precioAnteriorContado = producto.PContado;
                decimal precioAnteriorLista = producto.PLista;

                producto.PCosto *= factor;
                producto.PContado *= factor;
                producto.PLista *= factor;
                producto.FechaModPrecio = DateTime.Now;

                await _productoService.UpdateProductoAsync(producto);
                _logger.LogInformation("ProductoID={ProductoID} ajustado: PCosto de {AnteriorCosto} a {NuevoCosto}", id, precioAnteriorCosto, producto.PCosto);
            }
        }
    }
}
