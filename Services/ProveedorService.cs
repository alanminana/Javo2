// Services/ProveedorService.cs
using Javo2.IServices;
using Javo2.Models;
using Microsoft.Extensions.Logging;
using Javo2.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Javo2.Services
{
    public class ProveedorService : IProveedorService
    {
        private readonly ILogger<ProveedorService> _logger;
        private readonly IStockService _stockService;

        // Rutas de JSON
        private readonly string _jsonProveedoresFilePath = "Data/proveedores.json";
        private readonly string _jsonComprasFilePath = "Data/comprasProveedores.json";

        // En memoria
        private List<Proveedor> _proveedores = new();
        private List<CompraProveedor> _compras = new();
        private int _nextCompraID = 1;

        private static readonly object _lock = new();

        public ProveedorService(
            ILogger<ProveedorService> logger,
            IStockService stockService)
        {
            _logger = logger;
            _stockService = stockService;

            // Cargo ambos conjuntos de datos
            LoadProveedoresFromJson();
            LoadComprasFromJson().GetAwaiter().GetResult();
        }

        #region --- Proveedores CRUD + persistencia ---

        private void LoadProveedoresFromJson()
        {
            try
            {
                var data = JsonFileHelper.LoadFromJsonFile<ProveedorData>(_jsonProveedoresFilePath)
                           ?? new ProveedorData();
                _proveedores = data.Proveedores ?? new List<Proveedor>();

                if (!_proveedores.Any())
                    SeedProveedores();

                _logger.LogInformation("Proveedores cargados: {0}", _proveedores.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cargando proveedores, inicializando vacío");
                _proveedores = new List<Proveedor>();
                SeedProveedores();
            }
        }

        private void SeedProveedores()
        {
            _proveedores = new List<Proveedor>
            {
                new Proveedor { ProveedorID = 1, Nombre="Proveedor Ejemplo 1", Direccion="Calle Falsa 123", Telefono="123456789", Email="p1@ej.com", CondicionesPago="30 días", ProductosAsignados = new List<int>{1,2} },
                new Proveedor { ProveedorID = 2, Nombre="Proveedor Ejemplo 2", Direccion="Av Siempre Viva 742", Telefono="987654321", Email="p2@ej.com", CondicionesPago="15 días", ProductosAsignados = new List<int>{3} }
            };
            SaveProveedoresToJson();
        }

        private void SaveProveedoresToJson()
        {
            try
            {
                var wrapper = new ProveedorData { Proveedores = _proveedores };
                JsonFileHelper.SaveToJsonFile(_jsonProveedoresFilePath, wrapper);
                _logger.LogInformation("Proveedores guardados en JSON");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error guardando proveedores");
                throw;
            }
        }

        public async Task<IEnumerable<Proveedor>> GetProveedoresAsync()
            => await Task.FromResult(_proveedores.ToList());

        public async Task<Proveedor?> GetProveedorByIDAsync(int id)
            => await Task.FromResult(_proveedores.FirstOrDefault(p => p.ProveedorID == id));

        public async Task CreateProveedorAsync(Proveedor proveedor)
        {
            if (string.IsNullOrWhiteSpace(proveedor.Nombre))
                throw new ArgumentException("El nombre no puede estar vacío.");

            lock (_lock)
            {
                proveedor.ProveedorID = _proveedores.Any() ? _proveedores.Max(p => p.ProveedorID) + 1 : 1;
                _proveedores.Add(proveedor);
                SaveProveedoresToJson();
            }
            _logger.LogInformation("Proveedor creado ID={0}", proveedor.ProveedorID);
            await Task.CompletedTask;
        }

        public async Task UpdateProveedorAsync(Proveedor proveedor)
        {
            if (string.IsNullOrWhiteSpace(proveedor.Nombre))
                throw new ArgumentException("El nombre no puede estar vacío.");

            lock (_lock)
            {
                var ex = _proveedores.FirstOrDefault(p => p.ProveedorID == proveedor.ProveedorID);
                if (ex == null) throw new KeyNotFoundException($"ID {proveedor.ProveedorID} no existe.");
                ex.Nombre = proveedor.Nombre;
                ex.Direccion = proveedor.Direccion;
                ex.Telefono = proveedor.Telefono;
                ex.Email = proveedor.Email;
                ex.CondicionesPago = proveedor.CondicionesPago;
                ex.ProductosAsignados = proveedor.ProductosAsignados;
                SaveProveedoresToJson();
            }
            _logger.LogInformation("Proveedor actualizado ID={0}", proveedor.ProveedorID);
            await Task.CompletedTask;
        }

        public async Task DeleteProveedorAsync(int id)
        {
            lock (_lock)
            {
                var ex = _proveedores.FirstOrDefault(p => p.ProveedorID == id);
                if (ex == null) throw new KeyNotFoundException($"ID {id} no existe.");
                _proveedores.Remove(ex);
                SaveProveedoresToJson();
            }
            _logger.LogInformation("Proveedor eliminado ID={0}", id);
            await Task.CompletedTask;
        }

        #endregion

        #region --- Compras CRUD + persistencia + stock ---

        private async Task LoadComprasFromJson()
        {
            try
            {
                var data = await JsonFileHelper.LoadFromJsonFileAsync<List<CompraProveedor>>(_jsonComprasFilePath);
                lock (_lock)
                {
                    _compras = data ?? new List<CompraProveedor>();
                    if (_compras.Any())
                        _nextCompraID = _compras.Max(c => c.CompraID) + 1;
                }
                _logger.LogInformation("Compras cargadas: {0}", _compras.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cargando compras, inicializando vacío");
                _compras = new List<CompraProveedor>();
                _nextCompraID = 1;
            }
        }

        private async Task SaveComprasToJsonAsync()
        {
            try
            {
                List<CompraProveedor> snapshot;
                lock (_lock) { snapshot = _compras.ToList(); }
                await JsonFileHelper.SaveToJsonFileAsync(_jsonComprasFilePath, snapshot);
                _logger.LogInformation("Compras guardadas en JSON");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error guardando compras");
                throw;
            }
        }

        public async Task<IEnumerable<CompraProveedor>> GetComprasAsync()
            => await Task.FromResult(_compras.ToList());

        public async Task<CompraProveedor?> GetCompraByIDAsync(int id)
            => await Task.FromResult(_compras.FirstOrDefault(c => c.CompraID == id));

        public async Task<IEnumerable<CompraProveedor>> GetComprasByProveedorIDAsync(int proveedorID)
            => await Task.FromResult(_compras.Where(c => c.ProveedorID == proveedorID).ToList());

        public async Task<string> GenerarNumeroFacturaCompraAsync()
        {
            var fecha = DateTime.Now.ToString("yyyyMMdd");
            lock (_lock)
            {
                return $"COM-{fecha}-{_nextCompraID:D4}";
            }
        }

        public async Task CreateCompraAsync(CompraProveedor compra)
        {
            lock (_lock)
            {
                compra.CompraID = _nextCompraID++;
                compra.FechaCompra = DateTime.Now;
                compra.TotalProductos = compra.ProductosCompra.Sum(p => p.Cantidad);
                compra.PrecioTotal = compra.ProductosCompra.Sum(p => p.PrecioTotal);
                _compras.Add(compra);
            }
            await SaveComprasToJsonAsync();

            if (compra.Estado == EstadoCompra.Completada)
                await AjustarStockAsync(compra);
        }

        public async Task UpdateCompraAsync(CompraProveedor compra)
        {
            EstadoCompra prevEstado;
            lock (_lock)
            {
                var ex = _compras.FirstOrDefault(c => c.CompraID == compra.CompraID);
                if (ex == null) throw new KeyNotFoundException($"Compra {compra.CompraID} no encontrada.");
                prevEstado = ex.Estado;

                // Actualizo todos los campos relevantes
                ex.NumeroFactura = compra.NumeroFactura;
                ex.FormaPagoID = compra.FormaPagoID;
                ex.BancoID = compra.BancoID;
                ex.TipoTarjeta = compra.TipoTarjeta;
                ex.Cuotas = compra.Cuotas;
                ex.EntidadElectronica = compra.EntidadElectronica;
                ex.FechaVencimiento = compra.FechaVencimiento;
                ex.MontoCheque = compra.MontoCheque;
                ex.NumeroCheque = compra.NumeroCheque;
                ex.Observaciones = compra.Observaciones;
                ex.Estado = compra.Estado;
                ex.ProductosCompra = compra.ProductosCompra;
                ex.TotalProductos = compra.ProductosCompra.Sum(p => p.Cantidad);
                ex.PrecioTotal = compra.ProductosCompra.Sum(p => p.PrecioTotal);
            }
            await SaveComprasToJsonAsync();

            // Ajustes de stock según cambio de estado
            if (prevEstado != EstadoCompra.Completada && compra.Estado == EstadoCompra.Completada)
                await AjustarStockAsync(compra);
            else if (prevEstado == EstadoCompra.Completada && compra.Estado != EstadoCompra.Completada)
                await RevertirStockAsync(compra);
        }

        public async Task DeleteCompraAsync(int id)
        {
            CompraProveedor? target;
            lock (_lock)
            {
                target = _compras.FirstOrDefault(c => c.CompraID == id);
                if (target == null) throw new KeyNotFoundException($"Compra {id} no encontrada.");
                if (target.Estado == EstadoCompra.Completada)
                    RevertirStockAsync(target).GetAwaiter().GetResult();
                _compras.Remove(target);
            }
            await SaveComprasToJsonAsync();
        }

        public async Task ProcesarCompraAsync(int compraID)
        {
            lock (_lock)
            {
                var c = _compras.FirstOrDefault(x => x.CompraID == compraID)
                        ?? throw new KeyNotFoundException($"Compra {compraID} no encontrada.");
                if (c.Estado != EstadoCompra.Pendiente)
                    throw new InvalidOperationException("Solo Pendiente puede procesarse.");
                c.Estado = EstadoCompra.Procesada;
            }
            await SaveComprasToJsonAsync();
        }

        public async Task CompletarCompraAsync(int compraID)
        {
            CompraProveedor c;
            lock (_lock)
            {
                c = _compras.FirstOrDefault(x => x.CompraID == compraID)
                    ?? throw new KeyNotFoundException($"Compra {compraID} no encontrada.");
                if (c.Estado != EstadoCompra.Procesada)
                    throw new InvalidOperationException("Solo Procesada puede completarse.");
                c.Estado = EstadoCompra.Completada;
            }
            await SaveComprasToJsonAsync();
            await AjustarStockAsync(c);
        }

        public async Task CancelarCompraAsync(int compraID)
        {
            CompraProveedor c;
            lock (_lock)
            {
                c = _compras.FirstOrDefault(x => x.CompraID == compraID)
                    ?? throw new KeyNotFoundException($"Compra {compraID} no encontrada.");
                if (c.Estado == EstadoCompra.Completada)
                    RevertirStockAsync(c).GetAwaiter().GetResult();
                c.Estado = EstadoCompra.Cancelada;
            }
            await SaveComprasToJsonAsync();
        }

        private async Task AjustarStockAsync(CompraProveedor compra)
        {
            foreach (var d in compra.ProductosCompra)
            {
                await _stockService.RegistrarMovimientoAsync(new MovimientoStock
                {
                    ProductoID = d.ProductoID,
                    TipoMovimiento = "Entrada",
                    Cantidad = d.Cantidad,
                    Motivo = $"Compra #{compra.CompraID}"
                });
            }
        }

        private async Task RevertirStockAsync(CompraProveedor compra)
        {
            foreach (var d in compra.ProductosCompra)
            {
                await _stockService.RegistrarMovimientoAsync(new MovimientoStock
                {
                    ProductoID = d.ProductoID,
                    TipoMovimiento = "Salida",
                    Cantidad = d.Cantidad,
                    Motivo = $"Reversión compra #{compra.CompraID}"
                });
            }
        }

        #endregion
    }
}
