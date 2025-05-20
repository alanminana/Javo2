using Javo2.IServices;
using Javo2.Models;
using Javo2.ViewModels.Operaciones.Ventas;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Javo2.Helpers;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Javo2.Services
{
    public class VentaService : IVentaService
    {
        private readonly ILogger<VentaService> _logger;
        private readonly IAuditoriaService _auditoriaService;
        private readonly IStockService _stockService;
        private readonly ICreditoService _creditoService;
        private readonly IClienteService _clienteService;

        private static List<Venta> _ventas = new List<Venta>();
        private static int _nextVentaID = 1;
        private readonly string _jsonFilePath = "Data/ventas.json";
        private static readonly object _lock = new object();

        public VentaService(
            ILogger<VentaService> logger,
            IAuditoriaService auditoriaService,
            IStockService stockService,
            ICreditoService creditoService,
            IClienteService clienteService) // Agregar IClienteService al constructor
        {
            _logger = logger;
            _auditoriaService = auditoriaService;
            _stockService = stockService;
            _creditoService = creditoService;
            _clienteService = clienteService; // Inicializar _clienteService

            CargarDesdeJsonAsync().GetAwaiter().GetResult();
        }

        #region CRUD Operations

        public async Task<IEnumerable<Venta>> GetAllVentasAsync()
        {
            try
            {
                lock (_lock)
                {
                    _logger.LogInformation("GetAllVentasAsync => Total: {Count}", _ventas.Count);
                    return _ventas.ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todas las ventas");
                throw;
            }
        }

        public Task<Venta?> GetVentaByIDAsync(int id)
        {
            try
            {
                lock (_lock)
                {
                    var venta = _ventas.FirstOrDefault(v => v.VentaID == id);
                    _logger.LogInformation("GetVentaByIDAsync => ID: {ID}, Found: {Found}",
                        id, venta != null);
                    return Task.FromResult(venta);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener venta por ID: {ID}", id);
                throw;
            }
        }

        public async Task CreateVentaAsync(Venta venta)
        {
            try
            {
                lock (_lock)
                {
                    venta.VentaID = _nextVentaID++;
                    venta.FechaVenta = DateTime.Now;

                    // Asegurarse de que se calculen los totales
                    venta.PrecioTotal = venta.ProductosPresupuesto.Sum(p => p.PrecioTotal);
                    venta.TotalProductos = venta.ProductosPresupuesto.Sum(p => p.Cantidad);

                    _ventas.Add(venta);
                    _logger.LogInformation("Venta creada => ID: {ID}, Total: {Total}, Estado: {Estado}",
                        venta.VentaID, venta.PrecioTotal, venta.Estado);
                }

                await GuardarEnJsonAsync();

                // Registrar en auditoría
                await _auditoriaService.RegistrarCambioAsync(new AuditoriaRegistro
                {
                    FechaHora = DateTime.Now,
                    Usuario = venta.Usuario,
                    Entidad = "Venta",
                    Accion = "Create",
                    LlavePrimaria = venta.VentaID.ToString(),
                    Detalle = $"Cliente={venta.NombreCliente}, Total={venta.PrecioTotal}, Estado={venta.Estado}"
                });

                // IMPORTANTE: No se reduce el stock hasta que la venta sea autorizada
                _logger.LogInformation("Venta en estado {Estado}. Stock no modificado.", venta.Estado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear venta");
                throw;
            }
        }

        public async Task UpdateVentaAsync(Venta venta)
        {
            try
            {
                Venta? existing;
                EstadoVenta estadoAnterior;

                lock (_lock)
                {
                    existing = _ventas.FirstOrDefault(v => v.VentaID == venta.VentaID);
                    if (existing != null)
                    {
                        // Capturar estado anterior para auditoría y control de stock
                        estadoAnterior = existing.Estado;

                        // Actualizar campos
                        existing.FechaVenta = venta.FechaVenta;
                        existing.NumeroFactura = venta.NumeroFactura;
                        existing.NombreCliente = venta.NombreCliente;
                        existing.TelefonoCliente = venta.TelefonoCliente;
                        existing.DomicilioCliente = venta.DomicilioCliente;
                        existing.LocalidadCliente = venta.LocalidadCliente;
                        existing.CelularCliente = venta.CelularCliente;
                        existing.LimiteCreditoCliente = venta.LimiteCreditoCliente;
                        existing.SaldoCliente = venta.SaldoCliente;
                        existing.SaldoDisponibleCliente = venta.SaldoDisponibleCliente;

                        // Forma de pago y campos relacionados
                        existing.FormaPagoID = venta.FormaPagoID;
                        existing.BancoID = venta.BancoID;
                        existing.TipoTarjeta = venta.TipoTarjeta;
                        existing.Cuotas = venta.Cuotas;
                        existing.EntidadElectronica = venta.EntidadElectronica;
                        existing.PlanFinanciamiento = venta.PlanFinanciamiento;

                        // Otros campos
                        existing.Observaciones = venta.Observaciones;
                        existing.Condiciones = venta.Condiciones;
                        existing.Credito = venta.Credito;
                        existing.AdelantoDinero = venta.AdelantoDinero;
                        existing.DineroContado = venta.DineroContado;
                        existing.MontoCheque = venta.MontoCheque;
                        existing.NumeroCheque = venta.NumeroCheque;

                        // Productos y totales
                        existing.ProductosPresupuesto = venta.ProductosPresupuesto;
                        existing.PrecioTotal = venta.ProductosPresupuesto.Sum(p => p.PrecioTotal);
                        existing.TotalProductos = venta.ProductosPresupuesto.Sum(p => p.Cantidad);

                        // Estado
                        existing.Estado = venta.Estado;

                        _logger.LogInformation("Venta actualizada => ID: {ID}, Estado anterior: {EstadoAnterior}, Estado nuevo: {EstadoNuevo}",
                            venta.VentaID, estadoAnterior, venta.Estado);
                    }
                    else
                    {
                        _logger.LogWarning("UpdateVentaAsync => Venta no encontrada. ID: {ID}", venta.VentaID);
                        throw new KeyNotFoundException($"Venta con ID {venta.VentaID} no encontrada.");
                    }
                }

                await GuardarEnJsonAsync();

                // Gestionar stock según el cambio de estado
                await GestionarStockPorCambioEstado(existing, estadoAnterior, venta.Estado);

                // Registrar en auditoría
                await _auditoriaService.RegistrarCambioAsync(new AuditoriaRegistro
                {
                    FechaHora = DateTime.Now,
                    Usuario = venta.Usuario,
                    Entidad = "Venta",
                    Accion = "Update",
                    LlavePrimaria = venta.VentaID.ToString(),
                    Detalle = $"Cliente={venta.NombreCliente}, Total={venta.PrecioTotal}, Estado anterior={estadoAnterior}, Estado nuevo={venta.Estado}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar venta ID: {ID}", venta.VentaID);
                throw;
            }
        }

        public async Task DeleteVentaAsync(int id)
        {
            try
            {
                Venta? venta;
                lock (_lock)
                {
                    venta = _ventas.FirstOrDefault(v => v.VentaID == id);
                    if (venta != null)
                    {
                        // Si la venta estaba en un estado que afectaba el stock, revertir
                        if (venta.Estado == EstadoVenta.Autorizada ||
                            venta.Estado == EstadoVenta.PendienteDeEntrega ||
                            venta.Estado == EstadoVenta.Completada)
                        {
                            _logger.LogInformation("Revirtiendo stock para venta ID: {ID} en estado {Estado}", id, venta.Estado);
                            _ = RevertirStockAsync(venta, "Eliminación de venta");
                        }

                        _ventas.Remove(venta);
                        _logger.LogInformation("Venta eliminada => ID: {ID}", id);
                    }
                    else
                    {
                        _logger.LogWarning("DeleteVentaAsync => Venta no encontrada. ID: {ID}", id);
                        throw new KeyNotFoundException($"Venta con ID {id} no encontrada.");
                    }
                }

                await GuardarEnJsonAsync();

                // Registrar en auditoría
                await _auditoriaService.RegistrarCambioAsync(new AuditoriaRegistro
                {
                    FechaHora = DateTime.Now,
                    Usuario = "Sistema",
                    Entidad = "Venta",
                    Accion = "Delete",
                    LlavePrimaria = id.ToString(),
                    Detalle = $"Eliminada venta: Cliente={venta?.NombreCliente}, Total={venta?.PrecioTotal}, Estado={venta?.Estado}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar venta ID: {ID}", id);
                throw;
            }
        }
        // Agregar este método general para cambio de estado
        private async Task<Venta> CambiarEstadoVentaAsync(int ventaId, EstadoVenta nuevoEstado, string accion, string usuario)
        {
            _logger.LogInformation("CambiarEstadoVentaAsync => VentaID={ID}, NuevoEstado={Estado}, Usuario={Usuario}",
                ventaId, nuevoEstado, usuario);

            var venta = await GetVentaByIDAsync(ventaId);
            if (venta == null)
                throw new KeyNotFoundException($"Venta {ventaId} no encontrada");

            EstadoVenta estadoAnterior = venta.Estado;
            venta.Estado = nuevoEstado;
            venta.Usuario = usuario;

            await UpdateVentaAsync(venta);

            // Registrar en auditoría
            await _auditoriaService.RegistrarCambioAsync(new AuditoriaRegistro
            {
                FechaHora = DateTime.Now,
                Usuario = usuario,
                Entidad = "Venta",
                Accion = accion,
                LlavePrimaria = ventaId.ToString(),
                Detalle = $"Venta {accion.ToLower()}: Cliente={venta.NombreCliente}, Total={venta.PrecioTotal}, " +
                          $"Estado anterior={estadoAnterior}, Estado nuevo={nuevoEstado}"
            });

            return venta;
        }

        public async Task AutorizarVentaAsync(int ventaId, string usuario)
        {
            _logger.LogInformation("AutorizarVentaAsync => VentaID={ID}, Usuario={Usuario}", ventaId, usuario);

            var venta = await GetVentaByIDAsync(ventaId);
            if (venta == null)
                throw new KeyNotFoundException($"Venta {ventaId} no encontrada");

            if (venta.Estado != EstadoVenta.PendienteDeAutorizacion)
                throw new InvalidOperationException($"La venta {ventaId} no está pendiente de autorización");

            // Primero autorizar la venta
            await CambiarEstadoVentaAsync(ventaId, EstadoVenta.Autorizada, "Autorizar", usuario);

            // Luego cambiar a pendiente de entrega
            await CambiarEstadoVentaAsync(ventaId, EstadoVenta.PendienteDeEntrega, "Actualizar", usuario);
        }

        public async Task RechazarVentaAsync(int ventaId, string usuario)
        {
            _logger.LogInformation("RechazarVentaAsync => VentaID={ID}, Usuario={Usuario}", ventaId, usuario);

            var venta = await GetVentaByIDAsync(ventaId);
            if (venta == null)
                throw new KeyNotFoundException($"Venta {ventaId} no encontrada");

            if (venta.Estado != EstadoVenta.PendienteDeAutorizacion)
                throw new InvalidOperationException($"La venta {ventaId} no está pendiente de autorización");

            await CambiarEstadoVentaAsync(ventaId, EstadoVenta.Rechazada, "Rechazar", usuario);
        }

        public async Task MarcarVentaComoEntregadaAsync(int ventaId, string usuario)
        {
            _logger.LogInformation("MarcarVentaComoEntregadaAsync => VentaID={ID}, Usuario={Usuario}", ventaId, usuario);

            var venta = await GetVentaByIDAsync(ventaId);
            if (venta == null)
                throw new KeyNotFoundException($"Venta {ventaId} no encontrada");

            if (venta.Estado != EstadoVenta.PendienteDeEntrega)
                throw new InvalidOperationException($"La venta {ventaId} no está pendiente de entrega");

            // Actualizar stock
            foreach (var detalle in venta.ProductosPresupuesto)
            {
                var stockItem = await _stockService.GetStockItemByProductoIDAsync(detalle.ProductoID);
                if (stockItem != null)
                {
                    await _stockService.RegistrarMovimientoAsync(new MovimientoStock
                    {
                        ProductoID = detalle.ProductoID,
                        TipoMovimiento = "Salida",
                        Cantidad = detalle.Cantidad,
                        Motivo = $"Venta {venta.NumeroFactura} - Entrega"
                    });
                }
            }

            await CambiarEstadoVentaAsync(ventaId, EstadoVenta.Completada, "Entregar", usuario);
        }

        public async Task UpdateEstadoVentaAsync(int id, EstadoVenta estado)
        {
            await CambiarEstadoVentaAsync(id, estado, "UpdateEstado", "Sistema");
        }
       
     

      

        private async Task GestionarStockPorCambioEstado(Venta venta, EstadoVenta estadoAnterior, EstadoVenta estadoNuevo)
        {
            try
            {
                // De cualquier estado a Autorizada: reducir stock
                if (estadoAnterior != EstadoVenta.Autorizada && estadoNuevo == EstadoVenta.Autorizada)
                {
                    await ActualizarStockAsync(venta, "Venta autorizada");
                }
                // De Autorizada a Rechazada: revertir stock
                else if (estadoAnterior == EstadoVenta.Autorizada && estadoNuevo == EstadoVenta.Rechazada)
                {
                    await RevertirStockAsync(venta, "Venta rechazada después de autorizada");
                }
                // De estado que afecta stock a Borrador: revertir stock
                else if ((estadoAnterior == EstadoVenta.Autorizada ||
                          estadoAnterior == EstadoVenta.PendienteDeEntrega ||
                          estadoAnterior == EstadoVenta.Completada)
                         && estadoNuevo == EstadoVenta.Borrador)
                {
                    await RevertirStockAsync(venta, "Venta revertida a borrador");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al gestionar stock por cambio de estado. VentaID: {ID}, Estado anterior: {EstadoAnterior}, Estado nuevo: {EstadoNuevo}",
                    venta.VentaID, estadoAnterior, estadoNuevo);
                // No propagamos el error para no afectar el flujo principal
            }
        }

        private async Task ActualizarStockAsync(Venta venta, string motivo)
        {
            try
            {
                foreach (var detalle in venta.ProductosPresupuesto)
                {
                    if (detalle.ProductoID > 0 && detalle.Cantidad > 0)
                    {
                        var movimiento = new MovimientoStock
                        {
                            ProductoID = detalle.ProductoID,
                            Fecha = DateTime.Now,
                            TipoMovimiento = "Salida",
                            Cantidad = detalle.Cantidad,
                            Motivo = $"{motivo} - Venta #{venta.VentaID} - {venta.NumeroFactura}"
                        };

                        await _stockService.RegistrarMovimientoAsync(movimiento);
                        _logger.LogInformation("Stock reducido => ProductoID: {ProductoID}, Cantidad: -{Cantidad}, Venta: {VentaID}, Motivo: {Motivo}",
                            detalle.ProductoID, detalle.Cantidad, venta.VentaID, motivo);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar stock para venta ID: {ID}", venta.VentaID);
                throw; // Propagamos el error para que la venta no se procese si hay problemas con el stock
            }
        }

        private async Task RevertirStockAsync(Venta venta, string motivo)
        {
            try
            {
                foreach (var detalle in venta.ProductosPresupuesto)
                {
                    if (detalle.ProductoID > 0 && detalle.Cantidad > 0)
                    {
                        var movimiento = new MovimientoStock
                        {
                            ProductoID = detalle.ProductoID,
                            Fecha = DateTime.Now,
                            TipoMovimiento = "Entrada",
                            Cantidad = detalle.Cantidad,
                            Motivo = $"{motivo} - Venta #{venta.VentaID} - {venta.NumeroFactura}"
                        };

                        await _stockService.RegistrarMovimientoAsync(movimiento);
                        _logger.LogInformation("Stock revertido => ProductoID: {ProductoID}, Cantidad: +{Cantidad}, Venta: {VentaID}, Motivo: {Motivo}",
                            detalle.ProductoID, detalle.Cantidad, venta.VentaID, motivo);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al revertir stock para venta ID: {ID}", venta.VentaID);
                // No propagamos el error para no afectar el flujo principal
            }
        }

        #endregion

        #region Queries

        public Task<IEnumerable<Venta>> GetVentasByEstadoAsync(EstadoVenta estado)
        {
            lock (_lock)
            {
                var result = _ventas.Where(v => v.Estado == estado);
                _logger.LogInformation("GetVentasByEstadoAsync => Estado: {Estado}, Count: {Count}",
                    estado, result.Count());
                return Task.FromResult(result);
            }
        }

        public Task<IEnumerable<Venta>> GetVentasByFechaAsync(DateTime? fechaInicio, DateTime? fechaFin)
        {
            lock (_lock)
            {
                var query = _ventas.AsEnumerable();

                if (fechaInicio.HasValue)
                    query = query.Where(v => v.FechaVenta >= fechaInicio.Value);

                if (fechaFin.HasValue)
                    query = query.Where(v => v.FechaVenta <= fechaFin.Value);

                return Task.FromResult(query);
            }
        }

        public Task<IEnumerable<Venta>> GetVentasByClienteIDAsync(int clienteID)
        {
            lock (_lock)
            {
                var result = _ventas.Where(v => v.DniCliente == clienteID);
                return Task.FromResult(result);
            }
        }

        public Task<IEnumerable<Venta>> GetVentasFilteredAsync(VentaFilterDto filterDto)
        {
            lock (_lock)
            {
                var query = _ventas.AsEnumerable();

                if (!string.IsNullOrEmpty(filterDto.NombreCliente))
                    query = query.Where(v => v.NombreCliente.Contains(filterDto.NombreCliente, StringComparison.OrdinalIgnoreCase));

                if (filterDto.FechaInicio.HasValue)
                    query = query.Where(v => v.FechaVenta >= filterDto.FechaInicio.Value);

                if (filterDto.FechaFin.HasValue)
                    query = query.Where(v => v.FechaVenta <= filterDto.FechaFin.Value);

                if (!string.IsNullOrEmpty(filterDto.NumeroFactura))
                    query = query.Where(v => v.NumeroFactura.Contains(filterDto.NumeroFactura, StringComparison.OrdinalIgnoreCase));

                return Task.FromResult(query);
            }
        }

        public Task<IEnumerable<Venta>> GetVentasAsync(VentaFilterDto filter)
        {
            return GetVentasFilteredAsync(filter);
        }

        public Task<string> GenerarNumeroFacturaAsync()
        {
            lock (_lock)
            {
                var fecha = DateTime.Now.ToString("yyyyMMdd");
                var numero = $"FAC-{fecha}-{_nextVentaID:D4}";
                _logger.LogInformation("GenerarNumeroFacturaAsync => {Numero}", numero);
                return Task.FromResult(numero);
            }
        }

        public Task<IEnumerable<Venta>> GetVentasPendientesDeEntregaAsync()
        {
            lock (_lock)
            {
                var result = _ventas.Where(v => v.Estado == EstadoVenta.PendienteDeEntrega);
                return Task.FromResult(result);
            }
        }

        #endregion

        #region Formas de Pago y Combos

        public Task<IEnumerable<FormaPago>> GetFormasPagoAsync()
        {
            var formasPago = new List<FormaPago>
            {
                new FormaPago { FormaPagoID = 1, Nombre = "Contado" },
                new FormaPago { FormaPagoID = 2, Nombre = "Tarjeta de Crédito" },
                new FormaPago { FormaPagoID = 3, Nombre = "Tarjeta de Débito" },
                new FormaPago { FormaPagoID = 4, Nombre = "Transferencia" },
                new FormaPago { FormaPagoID = 5, Nombre = "Pago Virtual" },
                new FormaPago { FormaPagoID = 6, Nombre = "Crédito Personal" }
            };
            return Task.FromResult<IEnumerable<FormaPago>>(formasPago);
        }

        public Task<IEnumerable<Banco>> GetBancosAsync()
        {
            var bancos = new List<Banco>
            {
                new Banco { BancoID = 1, Nombre = "Banco Santander" },
                new Banco { BancoID = 2, Nombre = "BBVA" },
                new Banco { BancoID = 3, Nombre = "Banco Galicia" },
                new Banco { BancoID = 4, Nombre = "Banco Nación" },
                new Banco { BancoID = 5, Nombre = "Banco Provincia" },
                new Banco { BancoID = 6, Nombre = "Banco Ciudad" },
                new Banco { BancoID = 7, Nombre = "Banco Macro" },
                new Banco { BancoID = 8, Nombre = "HSBC" }
            };
            return Task.FromResult<IEnumerable<Banco>>(bancos);
        }

        public IEnumerable<SelectListItem> GetFormasPagoSelectList()
        {
            var formasPago = GetFormasPagoAsync().Result;
            return formasPago.Select(fp => new SelectListItem
            {
                Value = fp.FormaPagoID.ToString(),
                Text = fp.Nombre
            });
        }

        public IEnumerable<SelectListItem> GetBancosSelectList()
        {
            var bancos = GetBancosAsync().Result;
            return bancos.Select(b => new SelectListItem
            {
                Value = b.BancoID.ToString(),
                Text = b.Nombre
            });
        }

        public IEnumerable<SelectListItem> GetTipoTarjetaSelectList()
        {
            var tipos = new List<string> { "Visa", "MasterCard", "Amex", "Naranja", "Cabal" };
            return tipos.Select(t => new SelectListItem { Value = t, Text = t });
        }

        public IEnumerable<SelectListItem> GetCuotasSelectList()
        {
            var cuotas = Enumerable.Range(1, 24).Select(c => c.ToString()).ToList();
            return cuotas.Select(c => new SelectListItem
            {
                Value = c,
                Text = $"{c} cuota{(c == "1" ? "" : "s")}"
            });
        }

        public IEnumerable<SelectListItem> GetEntidadesElectronicasSelectList()
        {
            var entidades = new List<string>
            {
                "MercadoPago",
                "Modo",
                "BIMO",
                "Cuenta DNI",
                "QR"
            };
            return entidades.Select(e => new SelectListItem { Value = e, Text = e });
        }

        public IEnumerable<SelectListItem> GetPlanesFinanciamientoSelectList()
        {
            var planes = new List<string>
            {
                "Plan 6 cuotas",
                "Plan 12 cuotas",
                "Plan 18 cuotas",
                "Plan 24 cuotas",
                "Plan 36 cuotas"
            };
            return planes.Select(p => new SelectListItem { Value = p, Text = p });
        }

        #endregion

        #region Procesamiento de Venta

        public async Task ProcessVentaAsync(int ventaID)
        {
            try
            {
                Venta? venta;
                lock (_lock)
                {
                    venta = _ventas.FirstOrDefault(v => v.VentaID == ventaID);
                    if (venta == null)
                    {
                        throw new KeyNotFoundException($"Venta con ID {ventaID} no encontrada.");
                    }

                    if (venta.Estado != EstadoVenta.Autorizada)
                    {
                        throw new InvalidOperationException($"La venta debe estar autorizada para procesarla. Estado actual: {venta.Estado}");
                    }

                    // Cambiar estado a PendienteDeEntrega
                    venta.Estado = EstadoVenta.PendienteDeEntrega;
                }

                await GuardarEnJsonAsync();

                _logger.LogInformation("Venta procesada => ID: {ID}, Nuevo Estado: {Estado}",
                    ventaID, venta.Estado);

                // Registrar en auditoría
                await _auditoriaService.RegistrarCambioAsync(new AuditoriaRegistro
                {
                    FechaHora = DateTime.Now,
                    Usuario = "Sistema",
                    Entidad = "Venta",
                    Accion = "Process",
                    LlavePrimaria = ventaID.ToString(),
                    Detalle = $"Venta procesada. Estado: {venta.Estado}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar venta ID: {ID}", ventaID);
                throw;
            }
        }
        // Nuevo método para procesar venta a crédito
        public async Task<Venta> CrearVentaCreditoAsync(Venta venta, int numeroCuotas, DateTime primerVencimiento)
        {
            // Validar que el cliente pueda acceder a crédito
            var cliente = await _clienteService.GetClienteByIDAsync(venta.DniCliente);
            if (cliente == null || !cliente.AptoCredito)
            {
                throw new InvalidOperationException("El cliente no es apto para crédito");
            }

            // Validar que el cliente califique para este crédito
            bool califica = await _creditoService.ClienteCalificaPorCreditoAsync(cliente.ClienteID, venta.PrecioTotal, numeroCuotas);
            if (!califica)
            {
                throw new InvalidOperationException("El cliente no califica para este crédito");
            }

            // Calcular recargo
            decimal porcentajeRecargo = await _creditoService.CalcularRecargoAsync(cliente.ScoreCredito, venta.PrecioTotal, numeroCuotas);

            // Guardar datos originales sin recargo
            venta.TotalSinRecargo = venta.PrecioTotal;
            venta.PorcentajeRecargo = porcentajeRecargo;

            // Aplicar recargo
            decimal montoRecargo = venta.TotalSinRecargo * (porcentajeRecargo / 100);
            venta.PrecioTotal += montoRecargo;

            // Marcar como crédito
            venta.EsCredito = true;
            venta.EstadoCredito = "Activo";
            venta.SaldoPendiente = venta.PrecioTotal;

            // Guardar la venta primero
            await CreateVentaAsync(venta);

            // Generar cuotas
            var cuotas = await _creditoService.GenerarCuotasAsync(venta, numeroCuotas, primerVencimiento);
            venta.CuotasPagas = cuotas;

            // Actualizar venta con cuotas
            await UpdateVentaAsync(venta);

            return venta;
        }

        // Método para procesar pago
        public async Task<bool> ProcesarPagoCuotaAsync(int ventaId, int cuotaId, decimal monto, string formaPago, string referencia)
        {
            var venta = await GetVentaByIDAsync(ventaId);
            if (venta == null)
            {
                throw new KeyNotFoundException($"Venta con ID {ventaId} no encontrada");
            }

            // Procesamiento de pago delegado al servicio de crédito
            bool pagoProcesado = await _creditoService.RegistrarPagoCuotaAsync(cuotaId, monto, formaPago, referencia, "Sistema");

            if (pagoProcesado)
            {
                // Actualizar saldo pendiente
                venta.SaldoPendiente -= monto;

                // Verificar si todas las cuotas están pagas
                bool todasPagas = venta.CuotasPagas.All(c => c.EstadoCuota == EstadoCuota.Pagada);
                if (todasPagas)
                {
                    venta.EstadoCredito = "Cancelado";
                    venta.CreditoCancelado = true;
                    venta.FechaCancelacion = DateTime.Now;
                }

                await UpdateVentaAsync(venta);
            }

            return pagoProcesado;
        }

        #endregion

        #region Alias Methods

        public Task<Venta?> GetVentaByIdAsync(int id)
        {
            return GetVentaByIDAsync(id);
        }

        #endregion

        #region Private Methods

        private async Task CargarDesdeJsonAsync()
        {
            try
            {
                var data = await JsonFileHelper.LoadFromJsonFileAsync<List<Venta>>(_jsonFilePath);
                lock (_lock)
                {
                    _ventas = data ?? new List<Venta>();

                    if (_ventas.Any())
                    {
                        _nextVentaID = _ventas.Max(v => v.VentaID) + 1;
                    }
                    else
                    {
                        _nextVentaID = 1;
                    }

                    _logger.LogInformation("VentaService: {Count} ventas cargadas desde {File}",
                        _ventas.Count, _jsonFilePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar ventas desde JSON");
                lock (_lock)
                {
                    _ventas = new List<Venta>();
                    _nextVentaID = 1;
                }
            }
        }

        private async Task GuardarEnJsonAsync()
        {
            List<Venta> snapshot;
            lock (_lock)
            {
                snapshot = _ventas.ToList();
            }

            try
            {
                await JsonFileHelper.SaveToJsonFileAsync(_jsonFilePath, snapshot);
                _logger.LogInformation("VentaService: {Count} ventas guardadas en {File}",
                    snapshot.Count, _jsonFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar ventas en JSON");
                throw;
            }
        }

        #endregion
    }
}