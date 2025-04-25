// Services/DevolucionGarantiaService.cs
using Javo2.IServices;
using Javo2.Models;
using Javo2.ViewModels.Operaciones.DevolucionGarantia;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Javo2.Helpers;

namespace Javo2.Services
{
    public class DevolucionGarantiaService : IDevolucionGarantiaService
    {
        private readonly ILogger<DevolucionGarantiaService> _logger;
        private readonly IStockService _stockService;
        private readonly IVentaService _ventaService;
        private readonly IProductoService _productoService;
        private readonly IAuditoriaService _auditoriaService;

        private static List<DevolucionGarantia> _devoluciones = new List<DevolucionGarantia>();
        private static int _nextDevolucionID = 1;
        private static int _nextItemID = 1;
        private static int _nextCambioID = 1;
        private static readonly object _lock = new object();
        private readonly string _jsonFilePath = "Data/devoluciones.json";

        public DevolucionGarantiaService(
            ILogger<DevolucionGarantiaService> logger,
            IStockService stockService,
            IVentaService ventaService,
            IProductoService productoService,
            IAuditoriaService auditoriaService)
        {
            _logger = logger;
            _stockService = stockService;
            _ventaService = ventaService;
            _productoService = productoService;
            _auditoriaService = auditoriaService;

            CargarDesdeJsonAsync().GetAwaiter().GetResult();
        }
        private void SeedData()
        {
            lock (_lock)
            {
                _devoluciones = new List<DevolucionGarantia>
        {
            new DevolucionGarantia
            {
                DevolucionGarantiaID = 1,
                VentaID = 1, // Referencia a una venta existente
                NombreCliente = "weqwe weqwe",
                FechaSolicitud = DateTime.Now,
                TipoCaso = TipoCaso.Devolucion,
                Motivo = "Producto defectuoso",
                Descripcion = "El producto presentaba fallas al encenderlo",
                Estado = EstadoCaso.Pendiente,
                Usuario = "Sistema",
                Items = new List<ItemDevolucionGarantia>
                {
                    new ItemDevolucionGarantia
                    {
                        ProductoID = 1,
                        NombreProducto = "Producto Inicial",
                        Cantidad = 1,
                        PrecioUnitario = 150,
                        ProductoDanado = true,
                        EstadoProducto = "Defectuoso"
                    }
                }
            },
            new DevolucionGarantia
            {
                DevolucionGarantiaID = 2,
                VentaID = 2,
                NombreCliente = "weqwe weqwe",
                FechaSolicitud = DateTime.Now.AddDays(-5),
                TipoCaso = TipoCaso.Cambio,
                Motivo = "Cambio de producto",
                Descripcion = "Cliente desea cambiar por un modelo diferente",
                Estado = EstadoCaso.Pendiente,
                Usuario = "Sistema",
                Items = new List<ItemDevolucionGarantia>
                {
                    new ItemDevolucionGarantia
                    {
                        ProductoID = 2,
                        NombreProducto = "Afeitadora",
                        Cantidad = 1,
                        PrecioUnitario = 24,
                        ProductoDanado = false,
                        EstadoProducto = "Funcional"
                    }
                },
                CambiosProducto = new List<CambioProducto>
                {
                    new CambioProducto
                    {
                        ProductoOriginalID = 2,
                        NombreProductoOriginal = "Afeitadora",
                        ProductoNuevoID = 5,
                        NombreProductoNuevo = "errewwreer1",
                        Cantidad = 1,
                        DiferenciaPrecio = 0
                    }
                }
            }
        };

                _nextDevolucionID = _devoluciones.Max(d => d.DevolucionGarantiaID) + 1;
                SaveData();
                _logger.LogInformation("DevolucionGarantiaService: Datos semilla creados con {Count} devoluciones", _devoluciones.Count);
            }
        }

        private void SaveData()
        {
            try
            {
                JsonFileHelper.SaveToJsonFile(_jsonFilePath, _devoluciones);
                _logger.LogInformation("DevolucionGarantiaService: Datos guardados en {File}", _jsonFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar datos de devoluciones");
                throw;
            }
        }

        private async Task CargarDesdeJsonAsync()
        {
            try
            {
                var data = await JsonFileHelper.LoadFromJsonFileAsync<List<DevolucionGarantia>>(_jsonFilePath);
                lock (_lock)
                {
                    _devoluciones = data ?? new List<DevolucionGarantia>();
                    if (_devoluciones.Any())
                    {
                        _nextDevolucionID = _devoluciones.Max(d => d.DevolucionGarantiaID) + 1;
                    }
                    else
                    {
                        SeedData(); // Crear datos iniciales si está vacío
                    }
                }
                _logger.LogInformation("DevolucionGarantiaService: {Count} devoluciones cargadas", _devoluciones.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar devoluciones desde JSON");
                _devoluciones = new List<DevolucionGarantia>();
                SeedData();
            }
        }
        private async Task GuardarEnJsonAsync()
        {
            try
            {
                List<DevolucionGarantia> devoluciones;
                lock (_lock)
                {
                    devoluciones = _devoluciones.ToList();
                }
                await JsonFileHelper.SaveToJsonFileAsync(_jsonFilePath, devoluciones);
                _logger.LogInformation("Devoluciones guardadas: {Count}", devoluciones.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar devoluciones en JSON");
            }
        }

        public Task<IEnumerable<DevolucionGarantia>> GetAllAsync()
        {
            lock (_lock)
            {
                return Task.FromResult(_devoluciones.AsEnumerable());
            }
        }

        public Task<DevolucionGarantia> GetByIDAsync(int id)
        {
            lock (_lock)
            {
                var devolucion = _devoluciones.FirstOrDefault(d => d.DevolucionGarantiaID == id);
                return Task.FromResult(devolucion);
            }
        }

        public Task<IEnumerable<DevolucionGarantia>> GetByVentaIDAsync(int ventaID)
        {
            lock (_lock)
            {
                var devoluciones = _devoluciones.Where(d => d.VentaID == ventaID);
                return Task.FromResult(devoluciones);
            }
        }

        public Task<IEnumerable<DevolucionGarantia>> GetByEstadoAsync(EstadoCaso estado)
        {
            lock (_lock)
            {
                var devoluciones = _devoluciones.Where(d => d.Estado == estado);
                return Task.FromResult(devoluciones);
            }
        }

        public async Task<int> CreateAsync(DevolucionGarantia devolucion)
        {
            lock (_lock)
            {
                devolucion.DevolucionGarantiaID = _nextDevolucionID++;
                devolucion.FechaSolicitud = DateTime.Now;

                // Asignar IDs a los items
                foreach (var item in devolucion.Items)
                {
                    item.ItemDevolucionGarantiaID = _nextItemID++;
                    item.DevolucionGarantiaID = devolucion.DevolucionGarantiaID;
                }

                // Asignar IDs a los cambios
                foreach (var cambio in devolucion.CambiosProducto)
                {
                    cambio.CambioProductoID = _nextCambioID++;
                    cambio.DevolucionGarantiaID = devolucion.DevolucionGarantiaID;
                }

                _devoluciones.Add(devolucion);
            }

            await GuardarEnJsonAsync();

            // Registrar en auditoría
            await _auditoriaService.RegistrarCambioAsync(new AuditoriaRegistro
            {
                FechaHora = DateTime.Now,
                Usuario = devolucion.Usuario,
                Entidad = "DevolucionGarantia",
                Accion = "Create",
                LlavePrimaria = devolucion.DevolucionGarantiaID.ToString(),
                Detalle = $"Tipo: {devolucion.TipoCaso}, Venta: {devolucion.VentaID}"
            });

            return devolucion.DevolucionGarantiaID;
        }

        public async Task UpdateAsync(DevolucionGarantia devolucion)
        {
            lock (_lock)
            {
                var existing = _devoluciones.FirstOrDefault(d => d.DevolucionGarantiaID == devolucion.DevolucionGarantiaID);
                if (existing == null)
                {
                    throw new KeyNotFoundException($"Devolución con ID {devolucion.DevolucionGarantiaID} no encontrada");
                }

                // Actualizar información general
                existing.TipoCaso = devolucion.TipoCaso;
                existing.Motivo = devolucion.Motivo;
                existing.Descripcion = devolucion.Descripcion;
                existing.Estado = devolucion.Estado;
                existing.Comentarios = devolucion.Comentarios;
                existing.FechaResolucion = devolucion.FechaResolucion;

                // Actualizar items existentes y agregar nuevos
                foreach (var item in devolucion.Items)
                {
                    var existingItem = existing.Items.FirstOrDefault(i => i.ItemDevolucionGarantiaID == item.ItemDevolucionGarantiaID);
                    if (existingItem != null)
                    {
                        existingItem.Cantidad = item.Cantidad;
                        existingItem.ProductoDanado = item.ProductoDanado;
                        existingItem.EstadoProducto = item.EstadoProducto;
                    }
                    else
                    {
                        item.ItemDevolucionGarantiaID = _nextItemID++;
                        item.DevolucionGarantiaID = devolucion.DevolucionGarantiaID;
                        existing.Items.Add(item);
                    }
                }

                // Actualizar cambios existentes y agregar nuevos
                foreach (var cambio in devolucion.CambiosProducto)
                {
                    var existingCambio = existing.CambiosProducto.FirstOrDefault(c => c.CambioProductoID == cambio.CambioProductoID);
                    if (existingCambio != null)
                    {
                        existingCambio.ProductoNuevoID = cambio.ProductoNuevoID;
                        existingCambio.NombreProductoNuevo = cambio.NombreProductoNuevo;
                        existingCambio.Cantidad = cambio.Cantidad;
                        existingCambio.DiferenciaPrecio = cambio.DiferenciaPrecio;
                    }
                    else
                    {
                        cambio.CambioProductoID = _nextCambioID++;
                        cambio.DevolucionGarantiaID = devolucion.DevolucionGarantiaID;
                        existing.CambiosProducto.Add(cambio);
                    }
                }
            }

            await GuardarEnJsonAsync();

            // Registrar en auditoría
            await _auditoriaService.RegistrarCambioAsync(new AuditoriaRegistro
            {
                FechaHora = DateTime.Now,
                Usuario = devolucion.Usuario,
                Entidad = "DevolucionGarantia",
                Accion = "Update",
                LlavePrimaria = devolucion.DevolucionGarantiaID.ToString(),
                Detalle = $"Tipo: {devolucion.TipoCaso}, Estado: {devolucion.Estado}"
            });
        }

        public async Task DeleteAsync(int id)
        {
            DevolucionGarantia devolucion;
            lock (_lock)
            {
                devolucion = _devoluciones.FirstOrDefault(d => d.DevolucionGarantiaID == id);
                if (devolucion == null)
                {
                    throw new KeyNotFoundException($"Devolución con ID {id} no encontrada");
                }

                _devoluciones.Remove(devolucion);
            }

            await GuardarEnJsonAsync();

            // Registrar en auditoría
            await _auditoriaService.RegistrarCambioAsync(new AuditoriaRegistro
            {
                FechaHora = DateTime.Now,
                Usuario = "Sistema",
                Entidad = "DevolucionGarantia",
                Accion = "Delete",
                LlavePrimaria = id.ToString(),
                Detalle = $"Devolución eliminada: Tipo {devolucion.TipoCaso}, Venta {devolucion.VentaID}"
            });
        }

        public async Task ProcesarDevolucionAsync(int devolucionID)
        {
            DevolucionGarantia devolucion;
            lock (_lock)
            {
                devolucion = _devoluciones.FirstOrDefault(d => d.DevolucionGarantiaID == devolucionID);
                if (devolucion == null)
                {
                    throw new KeyNotFoundException($"Devolución con ID {devolucionID} no encontrada");
                }

                devolucion.Estado = EstadoCaso.Completado;
                devolucion.FechaResolucion = DateTime.Now;
            }

            // Actualizar stock
            await ActualizarStockDevolucionAsync(devolucionID);

            await GuardarEnJsonAsync();

            // Registrar en auditoría
            await _auditoriaService.RegistrarCambioAsync(new AuditoriaRegistro
            {
                FechaHora = DateTime.Now,
                Usuario = devolucion.Usuario,
                Entidad = "DevolucionGarantia",
                Accion = "ProcesarDevolucion",
                LlavePrimaria = devolucionID.ToString(),
                Detalle = $"Devolución procesada: Venta {devolucion.VentaID}"
            });
        }

        public async Task ProcesarCambioAsync(int devolucionID)
        {
            DevolucionGarantia devolucion;
            lock (_lock)
            {
                devolucion = _devoluciones.FirstOrDefault(d => d.DevolucionGarantiaID == devolucionID);
                if (devolucion == null)
                {
                    throw new KeyNotFoundException($"Devolución con ID {devolucionID} no encontrada");
                }

                devolucion.Estado = EstadoCaso.Completado;
                devolucion.FechaResolucion = DateTime.Now;
            }

            // Para cada cambio de producto:
            // 1. Decrementar stock del producto nuevo
            // 2. Incrementar stock del producto original si no está dañado
            foreach (var cambio in devolucion.CambiosProducto)
            {
                // Reducir stock del producto nuevo
                await _stockService.RegistrarMovimientoAsync(new MovimientoStock
                {
                    ProductoID = cambio.ProductoNuevoID,
                    Fecha = DateTime.Now,
                    TipoMovimiento = "Salida",
                    Cantidad = cambio.Cantidad,
                    Motivo = $"Cambio por devolución #{devolucionID}"
                });

                // Verificar si el producto original está dañado
                var itemOriginal = devolucion.Items.FirstOrDefault(i => i.ProductoID == cambio.ProductoOriginalID);
                if (itemOriginal != null && !itemOriginal.ProductoDanado)
                {
                    // Si no está dañado, reingresarlo al stock
                    await _stockService.RegistrarMovimientoAsync(new MovimientoStock
                    {
                        ProductoID = cambio.ProductoOriginalID,
                        Fecha = DateTime.Now,
                        TipoMovimiento = "Entrada",
                        Cantidad = cambio.Cantidad,
                        Motivo = $"Reingreso por cambio, devolución #{devolucionID}"
                    });
                }
            }

            await GuardarEnJsonAsync();

            // Registrar en auditoría
            await _auditoriaService.RegistrarCambioAsync(new AuditoriaRegistro
            {
                FechaHora = DateTime.Now,
                Usuario = devolucion.Usuario,
                Entidad = "DevolucionGarantia",
                Accion = "ProcesarCambio",
                LlavePrimaria = devolucionID.ToString(),
                Detalle = $"Cambio procesado: Venta {devolucion.VentaID}"
            });
        }

        public async Task EnviarGarantiaAsync(int devolucionID, string destinatario, string trackingNumber)
        {
            DevolucionGarantia devolucion;
            lock (_lock)
            {
                devolucion = _devoluciones.FirstOrDefault(d => d.DevolucionGarantiaID == devolucionID);
                if (devolucion == null)
                {
                    throw new KeyNotFoundException($"Devolución con ID {devolucionID} no encontrada");
                }

                devolucion.Estado = EstadoCaso.EnProceso;
                devolucion.Comentarios += $"\nEnviado a garantía: {destinatario}, Tracking: {trackingNumber}, Fecha: {DateTime.Now}";
            }

            await GuardarEnJsonAsync();

            // Registrar en auditoría
            await _auditoriaService.RegistrarCambioAsync(new AuditoriaRegistro
            {
                FechaHora = DateTime.Now,
                Usuario = devolucion.Usuario,
                Entidad = "DevolucionGarantia",
                Accion = "EnviarGarantia",
                LlavePrimaria = devolucionID.ToString(),
                Detalle = $"Enviado a garantía: {destinatario}, Tracking: {trackingNumber}"
            });
        }

        public async Task CompletarGarantiaAsync(int devolucionID, bool exitoso, string resultado)
        {
            DevolucionGarantia devolucion;
            lock (_lock)
            {
                devolucion = _devoluciones.FirstOrDefault(d => d.DevolucionGarantiaID == devolucionID);
                if (devolucion == null)
                {
                    throw new KeyNotFoundException($"Devolución con ID {devolucionID} no encontrada");
                }

                devolucion.Estado = exitoso ? EstadoCaso.Completado : EstadoCaso.Rechazado;
                devolucion.FechaResolucion = DateTime.Now;
                devolucion.Comentarios += $"\nResultado de garantía: {resultado}, Fecha: {DateTime.Now}";
            }

            // Si la garantía fue exitosa y se devolvió un producto reparado o nuevo
            if (exitoso)
            {
                // Lógica para manejar el reingreso del producto al inventario o la entrega al cliente
            }

            await GuardarEnJsonAsync();

            // Registrar en auditoría
            await _auditoriaService.RegistrarCambioAsync(new AuditoriaRegistro
            {
                FechaHora = DateTime.Now,
                Usuario = devolucion.Usuario,
                Entidad = "DevolucionGarantia",
                Accion = "CompletarGarantia",
                LlavePrimaria = devolucionID.ToString(),
                Detalle = $"Garantía {(exitoso ? "aprobada" : "rechazada")}: {resultado}"
            });
        }

        public async Task ActualizarStockDevolucionAsync(int devolucionID)
        {
            DevolucionGarantia devolucion;
            lock (_lock)
            {
                devolucion = _devoluciones.FirstOrDefault(d => d.DevolucionGarantiaID == devolucionID);
                if (devolucion == null)
                {
                    throw new KeyNotFoundException($"Devolución con ID {devolucionID} no encontrada");
                }
            }

            // Para cada item devuelto, incrementar el stock si no está dañado
            foreach (var item in devolucion.Items)
            {
                if (!item.ProductoDanado)
                {
                    await _stockService.RegistrarMovimientoAsync(new MovimientoStock
                    {
                        ProductoID = item.ProductoID,
                        Fecha = DateTime.Now,
                        TipoMovimiento = "Entrada",
                        Cantidad = item.Cantidad,
                        Motivo = $"Devolución #{devolucionID}"
                    });
                }
                else
                {
                    // Si está dañado, registrarlo como pérdida o para reparación según corresponda
                    _logger.LogInformation($"Producto dañado en devolución #{devolucionID}: ProductoID {item.ProductoID}, {item.NombreProducto}");
                }
            }

            // Registrar en auditoría
            await _auditoriaService.RegistrarCambioAsync(new AuditoriaRegistro
            {
                FechaHora = DateTime.Now,
                Usuario = devolucion.Usuario ?? "Sistema",
                Entidad = "DevolucionGarantia",
                Accion = "ActualizarStock",
                LlavePrimaria = devolucionID.ToString(),
                Detalle = $"Stock actualizado para devolución: {devolucionID}"
            });
        }
    }
}