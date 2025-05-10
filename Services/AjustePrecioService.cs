// Services/AjustePrecioService.cs
using Javo2.Helpers;
using Javo2.IServices;
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
        private readonly string _jsonFilePath = "Data/ajustesPrecios.json";
        private static List<AjustePrecioHistorico> _historialAjustes = new List<AjustePrecioHistorico>();
        private static int _nextAjusteID = 1;
        private static readonly object _lock = new object();

        public AjustePrecioService(IProductoService productoService, ILogger<AjustePrecioService> logger)
        {
            _productoService = productoService;
            _logger = logger;
            CargarHistorialAsync().GetAwaiter().GetResult();
        }

        private async Task CargarHistorialAsync()
        {
            try
            {
                var historial = await JsonFileHelper.LoadFromJsonFileAsync<List<AjustePrecioHistorico>>(_jsonFilePath);
                lock (_lock)
                {
                    _historialAjustes = historial ?? new List<AjustePrecioHistorico>();
                    if (_historialAjustes.Any())
                    {
                        _nextAjusteID = _historialAjustes.Max(h => h.AjusteHistoricoID) + 1;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar historial de ajustes de precios");
                lock (_lock)
                {
                    _historialAjustes = new List<AjustePrecioHistorico>();
                    _nextAjusteID = 1;
                }
            }
        }

        private async Task GuardarHistorialAsync()
        {
            try
            {
                List<AjustePrecioHistorico> snapshot;
                lock (_lock)
                {
                    snapshot = _historialAjustes.ToList();
                }
                await JsonFileHelper.SaveToJsonFileAsync(_jsonFilePath, snapshot);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar historial de ajustes de precios");
            }
        }

        public async Task<int> AjustarPreciosAsync(IEnumerable<int> productoIDs, decimal porcentaje, bool esAumento, string descripcion = "")
        {
            var ajusteHistorico = new AjustePrecioHistorico
            {
                FechaAjuste = DateTime.Now,
                UsuarioAjuste = System.Threading.Thread.CurrentPrincipal?.Identity?.Name ?? "Sistema",
                Porcentaje = porcentaje,
                EsAumento = esAumento,
                Descripcion = descripcion
            };

            var detalles = new List<AjustePrecioDetalle>();
            var factor = esAumento ? (1 + porcentaje / 100m) : (1 - porcentaje / 100m);

            foreach (var id in productoIDs)
            {
                var producto = await _productoService.GetProductoByIDAsync(id);
                if (producto == null)
                {
                    _logger.LogWarning("Producto con ID {ProductoID} no encontrado. Se omite.", id);
                    continue;
                }

                // Guardar estado previo
                var detalle = new AjustePrecioDetalle
                {
                    ProductoID = id,
                    NombreProducto = producto.Nombre,
                    PCostoAnterior = producto.PCosto,
                    PContadoAnterior = producto.PContado,
                    PListaAnterior = producto.PLista
                };

                // Calcular nuevos precios
                decimal nuevoPCosto = producto.PCosto * factor;
                decimal nuevoPContado = producto.PContado * factor;
                decimal nuevoPLista = producto.PLista * factor;

                // Actualizar producto
                producto.PCosto = nuevoPCosto;
                producto.PContado = nuevoPContado;
                producto.PLista = nuevoPLista;
                producto.FechaModPrecio = DateTime.Now;

                // Guardar estado posterior
                detalle.PCostoPosterior = nuevoPCosto;
                detalle.PContadoPosterior = nuevoPContado;
                detalle.PListaPosterior = nuevoPLista;

                detalles.Add(detalle);

                await _productoService.UpdateProductoAsync(producto);
                _logger.LogInformation("ProductoID={ProductoID} ajustado: PCosto de {AnteriorCosto} a {NuevoCosto}",
                    id, detalle.PCostoAnterior, detalle.PCostoPosterior);
            }

            ajusteHistorico.Detalles = detalles;

            // Guardar en historial
            lock (_lock)
            {
                ajusteHistorico.AjusteHistoricoID = _nextAjusteID++;
                _historialAjustes.Add(ajusteHistorico);
            }

            await GuardarHistorialAsync();
            return ajusteHistorico.AjusteHistoricoID;
        }

        public async Task<IEnumerable<AjustePrecioHistorico>> ObtenerHistorialAjustesAsync()
        {
            await Task.CompletedTask; // Para mantener la asincronía
            lock (_lock)
            {
                return _historialAjustes.OrderByDescending(h => h.FechaAjuste).ToList();
            }
        }

        public async Task<AjustePrecioHistorico> ObtenerAjusteHistoricoAsync(int ajusteHistoricoID)
        {
            await Task.CompletedTask; // Para mantener la asincronía
            lock (_lock)
            {
                return _historialAjustes.FirstOrDefault(h => h.AjusteHistoricoID == ajusteHistoricoID);
            }
        }

        public async Task RevertirAjusteAsync(int ajusteHistoricoID, string usuario)
        {
            AjustePrecioHistorico ajuste;

            lock (_lock)
            {
                ajuste = _historialAjustes.FirstOrDefault(h => h.AjusteHistoricoID == ajusteHistoricoID);

                if (ajuste == null)
                {
                    throw new KeyNotFoundException($"Ajuste histórico con ID {ajusteHistoricoID} no encontrado.");
                }

                if (ajuste.Revertido)
                {
                    throw new InvalidOperationException("Este ajuste ya ha sido revertido anteriormente.");
                }
            }

            // Revertir cada detalle
            foreach (var detalle in ajuste.Detalles)
            {
                var producto = await _productoService.GetProductoByIDAsync(detalle.ProductoID);
                if (producto == null)
                {
                    _logger.LogWarning("Producto con ID {ProductoID} no encontrado para revertir. Se omite.", detalle.ProductoID);
                    continue;
                }

                producto.PCosto = detalle.PCostoAnterior;
                producto.PContado = detalle.PContadoAnterior;
                producto.PLista = detalle.PListaAnterior;
                producto.FechaModPrecio = DateTime.Now;

                await _productoService.UpdateProductoAsync(producto);
                _logger.LogInformation("Revertido ajuste de precio para ProductoID={ProductoID}: PCosto de {ActualCosto} a {AnteriorCosto}",
                    detalle.ProductoID, detalle.PCostoPosterior, detalle.PCostoAnterior);
            }

            // Marcar como revertido
            lock (_lock)
            {
                ajuste.Revertido = true;
                ajuste.FechaReversion = DateTime.Now;
                ajuste.UsuarioReversion = usuario;
            }

            await GuardarHistorialAsync();
        }
    }
}