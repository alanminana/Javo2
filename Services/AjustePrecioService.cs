// Services/AjustePrecioService.cs (Implementación ampliada)
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
    public class JsonDataService : IAjustePrecioService
    {
        private readonly IProductoService _productoService;
        private readonly ILogger<JsonDataService> _logger;
        private readonly string _jsonFilePath = "Data/ajustesPrecios.json";
        private static List<AjustePrecioHistorico> _historialAjustes = new List<AjustePrecioHistorico>();
        private static int _nextAjusteID = 1;
        private static readonly object _lock = new object();

        public JsonDataService(IProductoService productoService, ILogger<JsonDataService> logger)
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
                Descripcion = descripcion,
                EsTemporal = false
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

                // Si es un ajuste temporal, actualizar su estado
                if (ajuste.EsTemporal)
                {
                    ajuste.EstadoTemporal = EstadoAjusteTemporal.Finalizado;
                }
            }

            await GuardarHistorialAsync();
        }

        // Nuevos métodos para ajustes temporales
        public async Task<int> CrearAjusteTemporalAsync(
    IEnumerable<int> productoIDs,
    decimal porcentaje,
    bool esAumento,
    DateTime fechaInicio,
    DateTime fechaFin,
    string tipoAjuste,
    string descripcion = "")
        {
            if (fechaInicio >= fechaFin)
            {
                throw new ArgumentException("La fecha de inicio debe ser anterior a la fecha de fin.");
            }

            // Truncar fechas a minutos (sin segundos ni milisegundos)
            fechaInicio = new DateTime(
                fechaInicio.Year,
                fechaInicio.Month,
                fechaInicio.Day,
                fechaInicio.Hour,
                fechaInicio.Minute,
                0
            );

            fechaFin = new DateTime(
                fechaFin.Year,
                fechaFin.Month,
                fechaFin.Day,
                fechaFin.Hour,
                fechaFin.Minute,
                0
            );

            // Verificar si hay conflictos con otros ajustes temporales
            await VerificarConflictosAjustesTemporalesAsync(productoIDs, fechaInicio, fechaFin);

            var ajusteHistorico = new AjustePrecioHistorico
            {
                FechaAjuste = DateTime.Now,
                UsuarioAjuste = System.Threading.Thread.CurrentPrincipal?.Identity?.Name ?? "Sistema",
                Porcentaje = porcentaje,
                EsAumento = esAumento,
                Descripcion = descripcion,
                // Campos específicos para ajustes temporales
                EsTemporal = true,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                TipoAjusteTemporal = tipoAjuste,
                // IMPORTANTE: Siempre configurar como Programado inicialmente
                EstadoTemporal = EstadoAjusteTemporal.Programado
            };

            var detalles = new List<AjustePrecioDetalle>();

            // Guardar los detalles originales de los productos
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
                    PListaAnterior = producto.PLista,
                    // Estos valores se actualizarán si el ajuste se activa ahora
                    PCostoPosterior = producto.PCosto,
                    PContadoPosterior = producto.PContado,
                    PListaPosterior = producto.PLista
                };

                detalles.Add(detalle);
            }

            ajusteHistorico.Detalles = detalles;

            // Guardar en historial
            lock (_lock)
            {
                ajusteHistorico.AjusteHistoricoID = _nextAjusteID++;
                _historialAjustes.Add(ajusteHistorico);
            }

            await GuardarHistorialAsync();

            // Si debe activarse inmediatamente (la fecha de inicio es ahora o anterior)
            if (fechaInicio <= DateTime.Now)
            {
                await ActivarAjusteTemporalAsync(ajusteHistorico.AjusteHistoricoID);
            }

            return ajusteHistorico.AjusteHistoricoID;
        }
        private async Task VerificarConflictosAjustesTemporalesAsync(IEnumerable<int> productoIDs, DateTime fechaInicio, DateTime fechaFin)
        {
            var ajustesActivos = await ObtenerAjustesTemporalesActivosAsync();
            var productosConConflictos = new Dictionary<int, List<int>>(); // ProductoID -> Lista de AjusteIDs

            foreach (var ajuste in ajustesActivos)
            {
                // Verificar si hay solapamiento de fechas
                if (fechaInicio <= ajuste.FechaFin && fechaFin >= ajuste.FechaInicio)
                {
                    // Buscar productos en común
                    foreach (var id in productoIDs)
                    {
                        if (ajuste.Detalles.Any(d => d.ProductoID == id))
                        {
                            if (!productosConConflictos.ContainsKey(id))
                            {
                                productosConConflictos[id] = new List<int>();
                            }
                            productosConConflictos[id].Add(ajuste.AjusteHistoricoID);
                        }
                    }
                }
            }

            if (productosConConflictos.Any())
            {
                var mensaje = "Existen conflictos con ajustes temporales ya programados para los siguientes productos: ";
                foreach (var kvp in productosConConflictos)
                {
                    var producto = await _productoService.GetProductoByIDAsync(kvp.Key);
                    mensaje += $"\nProducto {producto?.Nombre ?? kvp.Key.ToString()} (ID: {kvp.Key}) tiene conflictos con ajustes: {string.Join(", ", kvp.Value)}";
                }
                throw new InvalidOperationException(mensaje);
            }
        }

        public async Task<IEnumerable<AjustePrecioHistorico>> ObtenerAjustesTemporalesActivosAsync()
        {
            await Task.CompletedTask;
            lock (_lock)
            {
                return _historialAjustes
                    .Where(a => a.EsTemporal &&
                          (a.EstadoTemporal == EstadoAjusteTemporal.Activo ||
                           a.EstadoTemporal == EstadoAjusteTemporal.Programado) &&
                           !a.Revertido)
                    .OrderBy(a => a.FechaInicio)
                    .ToList();
            }
        }

        public async Task<IEnumerable<AjustePrecioHistorico>> ObtenerAjustesTemporalesPorEstadoAsync(EstadoAjusteTemporal estado)
        {
            await Task.CompletedTask;
            lock (_lock)
            {
                return _historialAjustes
                    .Where(a => a.EsTemporal && a.EstadoTemporal == estado)
                    .OrderByDescending(a => a.FechaInicio)
                    .ToList();
            }
        }

        public async Task ActivarAjusteTemporalAsync(int ajusteHistoricoID)
        {
            AjustePrecioHistorico ajuste;

            lock (_lock)
            {
                ajuste = _historialAjustes.FirstOrDefault(h => h.AjusteHistoricoID == ajusteHistoricoID);

                if (ajuste == null)
                {
                    throw new KeyNotFoundException($"Ajuste histórico con ID {ajusteHistoricoID} no encontrado.");
                }

                if (!ajuste.EsTemporal)
                {
                    throw new InvalidOperationException("Este ajuste no es temporal.");
                }

                if (ajuste.EstadoTemporal != EstadoAjusteTemporal.Programado)
                {
                    throw new InvalidOperationException($"El ajuste no está en estado Programado. Estado actual: {ajuste.EstadoTemporal}");
                }
            }

            // Aplicar el ajuste a los productos
            var factor = ajuste.EsAumento ? (1 + ajuste.Porcentaje / 100m) : (1 - ajuste.Porcentaje / 100m);

            foreach (var detalle in ajuste.Detalles)
            {
                var producto = await _productoService.GetProductoByIDAsync(detalle.ProductoID);
                if (producto == null)
                {
                    _logger.LogWarning("Producto con ID {ProductoID} no encontrado. Se omite.", detalle.ProductoID);
                    continue;
                }

                // Calcular nuevos precios
                decimal nuevoPCosto = producto.PCosto * factor;
                decimal nuevoPContado = producto.PContado * factor;
                decimal nuevoPLista = producto.PLista * factor;

                // Actualizar producto
                producto.PCosto = nuevoPCosto;
                producto.PContado = nuevoPContado;
                producto.PLista = nuevoPLista;
                producto.FechaModPrecio = DateTime.Now;

                // Actualizar detalles
                detalle.PCostoPosterior = nuevoPCosto;
                detalle.PContadoPosterior = nuevoPContado;
                detalle.PListaPosterior = nuevoPLista;

                await _productoService.UpdateProductoAsync(producto);
                _logger.LogInformation("Ajuste temporal activado - ProductoID={ProductoID}: PCosto de {AnteriorCosto} a {NuevoCosto}",
                    detalle.ProductoID, detalle.PCostoAnterior, detalle.PCostoPosterior);
            }

            // Actualizar estado
            lock (_lock)
            {
                ajuste.EstadoTemporal = EstadoAjusteTemporal.Activo;
            }

            await GuardarHistorialAsync();
        }

        public async Task FinalizarAjusteTemporalAsync(int ajusteHistoricoID, string usuario)
        {
            AjustePrecioHistorico ajuste;

            lock (_lock)
            {
                ajuste = _historialAjustes.FirstOrDefault(h => h.AjusteHistoricoID == ajusteHistoricoID);

                if (ajuste == null)
                {
                    throw new KeyNotFoundException($"Ajuste histórico con ID {ajusteHistoricoID} no encontrado.");
                }

                if (!ajuste.EsTemporal)
                {
                    throw new InvalidOperationException("Este ajuste no es temporal.");
                }

                if (ajuste.EstadoTemporal != EstadoAjusteTemporal.Activo)
                {
                    throw new InvalidOperationException($"El ajuste no está en estado Activo. Estado actual: {ajuste.EstadoTemporal}");
                }
            }

            // Revertir el ajuste usando el método existente
            await RevertirAjusteAsync(ajusteHistoricoID, usuario);
        }

        public async Task VerificarYActualizarAjustesTemporalesAsync()
        {
            var ahora = DateTime.Now;
            List<AjustePrecioHistorico> ajustesParaActivar = new List<AjustePrecioHistorico>();
            List<AjustePrecioHistorico> ajustesParaFinalizar = new List<AjustePrecioHistorico>();

            lock (_lock)
            {
                // Encontrar ajustes programados que deben activarse
                ajustesParaActivar = _historialAjustes
                    .Where(a => a.EsTemporal &&
                           a.EstadoTemporal == EstadoAjusteTemporal.Programado &&
                           a.FechaInicio <= ahora)
                    .ToList();

                // Encontrar ajustes activos que deben finalizarse
                ajustesParaFinalizar = _historialAjustes
                    .Where(a => a.EsTemporal &&
                           a.EstadoTemporal == EstadoAjusteTemporal.Activo &&
                           a.FechaFin <= ahora)
                    .ToList();
            }

            // Activar ajustes programados
            foreach (var ajuste in ajustesParaActivar)
            {
                _logger.LogInformation("Activando automáticamente ajuste temporal ID={ID}", ajuste.AjusteHistoricoID);
                try
                {
                    await ActivarAjusteTemporalAsync(ajuste.AjusteHistoricoID);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al activar automáticamente ajuste temporal ID={ID}", ajuste.AjusteHistoricoID);
                }
            }

            // Finalizar ajustes vencidos
            foreach (var ajuste in ajustesParaFinalizar)
            {
                _logger.LogInformation("Finalizando automáticamente ajuste temporal ID={ID}", ajuste.AjusteHistoricoID);
                try
                {
                    await FinalizarAjusteTemporalAsync(ajuste.AjusteHistoricoID, "Sistema");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al finalizar automáticamente ajuste temporal ID={ID}", ajuste.AjusteHistoricoID);
                }
            }
        }
    }
}