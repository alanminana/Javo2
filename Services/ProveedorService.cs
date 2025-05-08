// Services/ProveedorService.cs (parcial, con nuevos métodos)
using Javo2.IServices;
using Javo2.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Javo2.Helpers;

namespace Javo2.Services
{
    public class ProveedorService : IProveedorService
    {
        // Propiedades existentes
        private readonly ILogger<ProveedorService> _logger;
        private readonly IStockService _stockService;
        private static List<Proveedor> _proveedores = new();
        private static List<CompraProveedor> _compras = new();
        private static int _nextCompraID = 1;
        private readonly string _jsonProveedoresFilePath = "Data/proveedores.json";
        private readonly string _jsonComprasFilePath = "Data/comprasProveedores.json";
        private static readonly object _lock = new();

        // Constructor (ajustar según tu implementación actual)
        public ProveedorService(
            ILogger<ProveedorService> logger,
            IStockService stockService)
        {
            _logger = logger;
            _stockService = stockService;
            CargarDesdeJsonAsync().GetAwaiter().GetResult();
        }

        // Implementaciones de métodos para compras
        public async Task<IEnumerable<CompraProveedor>> GetComprasAsync()
        {
            lock (_lock)
            {
                return _compras.ToList();
            }
        }

        public async Task<CompraProveedor?> GetCompraByIDAsync(int id)
        {
            lock (_lock)
            {
                return _compras.FirstOrDefault(c => c.CompraID == id);
            }
        }

        public async Task<IEnumerable<CompraProveedor>> GetComprasByProveedorIDAsync(int proveedorID)
        {
            lock (_lock)
            {
                return _compras.Where(c => c.ProveedorID == proveedorID).ToList();
            }
        }

        public async Task<string> GenerarNumeroFacturaCompraAsync()
        {
            lock (_lock)
            {
                var fecha = DateTime.Now.ToString("yyyyMMdd");
                var numero = $"COM-{fecha}-{_nextCompraID:D4}";
                return numero;
            }
        }

        public async Task CreateCompraAsync(CompraProveedor compra)
        {
            try
            {
                lock (_lock)
                {
                    compra.CompraID = _nextCompraID++;
                    compra.FechaCompra = DateTime.Now;

                    // Calculamos totales
                    compra.TotalProductos = compra.ProductosCompra.Sum(p => p.Cantidad);
                    compra.PrecioTotal = compra.ProductosCompra.Sum(p => p.PrecioTotal);

                    _compras.Add(compra);
                    _logger.LogInformation("Compra creada => ID: {ID}, Total: {Total}, Estado: {Estado}",
                        compra.CompraID, compra.PrecioTotal, compra.Estado);
                }

                await GuardarComprasEnJsonAsync();

                // Ajustar stock si la compra está completada
                if (compra.Estado == EstadoCompra.Completada)
                {
                    await AjustarStockAsync(compra);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear compra");
                throw;
            }
        }

        public async Task UpdateCompraAsync(CompraProveedor compra)
        {
            try
            {
                EstadoCompra estadoAnterior;
                lock (_lock)
                {
                    var existingCompra = _compras.FirstOrDefault(c => c.CompraID == compra.CompraID);
                    if (existingCompra == null)
                    {
                        throw new KeyNotFoundException($"Compra con ID {compra.CompraID} no encontrada.");
                    }

                    estadoAnterior = existingCompra.Estado;

                    // Actualizar campos
                    existingCompra.NumeroFactura = compra.NumeroFactura;
                    existingCompra.FormaPagoID = compra.FormaPagoID;
                    existingCompra.BancoID = compra.BancoID;
                    existingCompra.TipoTarjeta = compra.TipoTarjeta;
                    existingCompra.Cuotas = compra.Cuotas;
                    existingCompra.EntidadElectronica = compra.EntidadElectronica;
                    existingCompra.FechaVencimiento = compra.FechaVencimiento;
                    existingCompra.MontoCheque = compra.MontoCheque;
                    existingCompra.NumeroCheque = compra.NumeroCheque;
                    existingCompra.Observaciones = compra.Observaciones;
                    existingCompra.Estado = compra.Estado;
                    existingCompra.ProductosCompra = compra.ProductosCompra;
                    existingCompra.TotalProductos = compra.ProductosCompra.Sum(p => p.Cantidad);
                    existingCompra.PrecioTotal = compra.ProductosCompra.Sum(p => p.PrecioTotal);
                }

                await GuardarComprasEnJsonAsync();

                // Si cambió a completada, ajustar stock
                if (estadoAnterior != EstadoCompra.Completada && compra.Estado == EstadoCompra.Completada)
                {
                    await AjustarStockAsync(compra);
                }
                // Si cambió de completada a otro estado, revertir stock
                else if (estadoAnterior == EstadoCompra.Completada && compra.Estado != EstadoCompra.Completada)
                {
                    await RevertirStockAsync(compra);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar compra");
                throw;
            }
        }

        public async Task DeleteCompraAsync(int id)
        {
            try
            {
                CompraProveedor? compra;
                lock (_lock)
                {
                    compra = _compras.FirstOrDefault(c => c.CompraID == id);
                    if (compra == null)
                    {
                        throw new KeyNotFoundException($"Compra con ID {id} no encontrada.");
                    }

                    // Si la compra ya fue completada, revertir stock
                    if (compra.Estado == EstadoCompra.Completada)
                    {
                        RevertirStockAsync(compra).GetAwaiter().GetResult();
                    }

                    _compras.Remove(compra);
                }

                await GuardarComprasEnJsonAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar compra");
                throw;
            }
        }

        public async Task ProcesarCompraAsync(int compraID)
        {
            try
            {
                lock (_lock)
                {
                    var compra = _compras.FirstOrDefault(c => c.CompraID == compraID);
                    if (compra == null)
                    {
                        throw new KeyNotFoundException($"Compra con ID {compraID} no encontrada.");
                    }

                    if (compra.Estado != EstadoCompra.Pendiente)
                    {
                        throw new InvalidOperationException($"La compra debe estar en estado Pendiente para procesarla.");
                    }

                    compra.Estado = EstadoCompra.Procesada;
                }

                await GuardarComprasEnJsonAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar compra");
                throw;
            }
        }

        public async Task CompletarCompraAsync(int compraID)
        {
            try
            {
                CompraProveedor? compra;
                lock (_lock)
                {
                    compra = _compras.FirstOrDefault(c => c.CompraID == compraID);
                    if (compra == null)
                    {
                        throw new KeyNotFoundException($"Compra con ID {compraID} no encontrada.");
                    }

                    if (compra.Estado != EstadoCompra.Procesada)
                    {
                        throw new InvalidOperationException($"La compra debe estar en estado Procesada para completarla.");
                    }

                    compra.Estado = EstadoCompra.Completada;
                }

                await GuardarComprasEnJsonAsync();

                // Ajustar stock
                await AjustarStockAsync(compra);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al completar compra");
                throw;
            }
        }

        public async Task CancelarCompraAsync(int compraID)
        {
            try
            {
                CompraProveedor? compra;
                lock (_lock)
                {
                    compra = _compras.FirstOrDefault(c => c.CompraID == compraID);
                    if (compra == null)
                    {
                        throw new KeyNotFoundException($"Compra con ID {compraID} no encontrada.");
                    }

                    // Si la compra estaba completada, revertir stock
                    if (compra.Estado == EstadoCompra.Completada)
                    {
                        RevertirStockAsync(compra).GetAwaiter().GetResult();
                    }

                    compra.Estado = EstadoCompra.Cancelada;
                }

                await GuardarComprasEnJsonAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cancelar compra");
                throw;
            }
        }

        // Métodos auxiliares para stock
        private async Task AjustarStockAsync(CompraProveedor compra)
        {
            foreach (var detalle in compra.ProductosCompra)
            {
                // Registrar entrada de stock
                await _stockService.RegistrarMovimientoAsync(new MovimientoStock
                {
                    ProductoID = detalle.ProductoID,
                    TipoMovimiento = "Entrada",
                    Cantidad = detalle.Cantidad,
                    Motivo = $"Compra a proveedor #{compra.CompraID}"
                });
            }
        }

        private async Task RevertirStockAsync(CompraProveedor compra)
        {
            foreach (var detalle in compra.ProductosCompra)
            {
                // Registrar salida de stock
                await _stockService.RegistrarMovimientoAsync(new MovimientoStock
                {
                    ProductoID = detalle.ProductoID,
                    TipoMovimiento = "Salida",
                    Cantidad = detalle.Cantidad,
                    Motivo = $"Reversión de compra a proveedor #{compra.CompraID}"
                });
            }
        }

        // Métodos para cargar/guardar JSON
        private async Task GuardarComprasEnJsonAsync()
        {
            try
            {
                List<CompraProveedor> comprasParaGuardar;
                lock (_lock)
                {
                    comprasParaGuardar = _compras.ToList();
                }
                await JsonFileHelper.SaveToJsonFileAsync(_jsonComprasFilePath, comprasParaGuardar);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar compras en JSON");
                throw;
            }
        }

        private async Task CargarComprasDesdeJsonAsync()
        {
            try
            {
                var data = await JsonFileHelper.LoadFromJsonFileAsync<List<CompraProveedor>>(_jsonComprasFilePath);
                lock (_lock)
                {
                    _compras = data ?? new List<CompraProveedor>();
                    if (_compras.Any())
                    {
                        _nextCompraID = _compras.Max(c => c.CompraID) + 1;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar compras desde JSON");
                _compras = new List<CompraProveedor>();
                _nextCompraID = 1;
            }
        }

        // Puedes agregar aquí el resto de los métodos que ya tengas en tu servicio
    }
}