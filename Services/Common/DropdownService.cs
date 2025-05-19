// Archivo: Services/Common/DropdownService.cs
using Javo2.IServices;
using Javo2.IServices.Common;
using Javo2.Models;
using Javo2.ViewModels.Operaciones.Clientes;
using Javo2.ViewModels.Operaciones.Productos;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Javo2.Services.Common
{
    public class DropdownService : IDropdownService
    {
        private readonly IProductoService _productoService;
        private readonly ICatalogoService _catalogoService;
        private readonly IProvinciaService _provinciaService;
        private readonly IProveedorService _proveedorService;
        private readonly ILogger<DropdownService> _logger;

        public DropdownService(
            IProductoService productoService,
            ICatalogoService catalogoService,
            IProvinciaService provinciaService,
            IProveedorService proveedorService,
            ILogger<DropdownService> logger)
        {
            _productoService = productoService;
            _catalogoService = catalogoService;
            _provinciaService = provinciaService;
            _proveedorService = proveedorService;
            _logger = logger;
        }

        // Métodos para Provincias y Ciudades
        public async Task<List<Provincia>> GetProvinciasAsync()
        {
            try
            {
                var provincias = await _provinciaService.GetAllProvinciasAsync();
                _logger.LogInformation("Provincias obtenidas: {Count}", provincias.Count());
                return provincias.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las provincias");
                return new List<Provincia>();
            }
        }

        public async Task<List<Ciudad>> GetCiudadesByProvinciaIDAsync(int provinciaID)
        {
            try
            {
                _logger.LogInformation("Obteniendo ciudades para la provincia ID: {ProvinciaID}", provinciaID);
                var ciudades = await _provinciaService.GetCiudadesByProvinciaIDAsync(provinciaID);
                var ciudadList = ciudades.ToList();
                _logger.LogInformation("Ciudades encontradas: {Count}", ciudadList.Count);
                return ciudadList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ciudades para la provincia ID {ProvinciaID}", provinciaID);
                return new List<Ciudad>();
            }
        }

        // Métodos para Rubros, SubRubros y Marcas
        public async Task<List<SelectListItem>> GetRubrosAsync()
        {
            try
            {
                var rubros = await _catalogoService.GetRubrosAsync();
                var result = rubros.Select(r => new SelectListItem
                {
                    Value = r.ID.ToString(),
                    Text = r.Nombre
                }).ToList();

                _logger.LogInformation("Rubros obtenidos: {Count}", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener rubros");
                return new List<SelectListItem>();
            }
        }

        public async Task<List<SelectListItem>> GetSubRubrosAsync(int rubroId)
        {
            _logger.LogInformation("GetSubRubrosAsync called with RubroID: {RubroID}", rubroId);
            var subRubros = await _catalogoService.GetSubRubrosByRubroIDAsync(rubroId);
            var result = subRubros.Select(sr => new SelectListItem
            {
                Value = sr.ID.ToString(),
                Text = sr.Nombre
            }).ToList();

            _logger.LogInformation("SubRubros obtenidos para RubroID {RubroID}: {Count}", rubroId, result.Count);
            return result;
        }

        public async Task<List<SelectListItem>> GetMarcasAsync()
        {
            try
            {
                var marcas = await _catalogoService.GetMarcasAsync();
                var result = marcas.Select(m => new SelectListItem
                {
                    Value = m.ID.ToString(),
                    Text = m.Nombre
                }).ToList();

                _logger.LogInformation("Marcas obtenidas: {Count}", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener marcas");
                return new List<SelectListItem>();
            }
        }

        // Método para Productos
        public async Task<List<SelectListItem>> GetProductosAsync()
        {
            try
            {
                var productos = await _productoService.GetAllProductosAsync();
                var result = productos.Select(p => new SelectListItem
                {
                    Value = p.ProductoID.ToString(),
                    Text = p.Nombre
                }).ToList();

                _logger.LogInformation("Productos obtenidos: {Count}", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos");
                return new List<SelectListItem>();
            }
        }

        // Nuevos métodos para gestión de Proveedores y Compras
        public Task<List<SelectListItem>> GetFormasPagoAsync()
        {
            var formasPago = new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "Contado" },
                new SelectListItem { Value = "2", Text = "Tarjeta de Crédito" },
                new SelectListItem { Value = "3", Text = "Tarjeta de Débito" },
                new SelectListItem { Value = "4", Text = "Transferencia" },
                new SelectListItem { Value = "5", Text = "Pago Virtual" },
                new SelectListItem { Value = "6", Text = "Crédito Personal" },
                new SelectListItem { Value = "7", Text = "Cheque" }
            };

            return Task.FromResult(formasPago);
        }

        public Task<List<SelectListItem>> GetBancosAsync()
        {
            var bancos = new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "Banco Nación" },
                new SelectListItem { Value = "2", Text = "Banco Provincia" },
                new SelectListItem { Value = "3", Text = "Banco Santander" },
                new SelectListItem { Value = "4", Text = "Banco Galicia" },
                new SelectListItem { Value = "5", Text = "BBVA" },
                new SelectListItem { Value = "6", Text = "HSBC" }
            };

            return Task.FromResult(bancos);
        }

        public Task<List<SelectListItem>> GetTiposTarjetaAsync()
        {
            var tiposTarjeta = new List<SelectListItem>
            {
                new SelectListItem { Value = "Visa", Text = "Visa" },
                new SelectListItem { Value = "MasterCard", Text = "MasterCard" },
                new SelectListItem { Value = "American Express", Text = "American Express" },
                new SelectListItem { Value = "Naranja", Text = "Naranja" }
            };

            return Task.FromResult(tiposTarjeta);
        }

        public Task<List<SelectListItem>> GetCuotasAsync()
        {
            var cuotas = new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "1 cuota" },
                new SelectListItem { Value = "3", Text = "3 cuotas" },
                new SelectListItem { Value = "6", Text = "6 cuotas" },
                new SelectListItem { Value = "12", Text = "12 cuotas" }
            };

            return Task.FromResult(cuotas);
        }

        public Task<List<SelectListItem>> GetEntidadesElectronicasAsync()
        {
            var entidades = new List<SelectListItem>
            {
                new SelectListItem { Value = "MercadoPago", Text = "MercadoPago" },
                new SelectListItem { Value = "Todo Pago", Text = "Todo Pago" },
                new SelectListItem { Value = "PayPal", Text = "PayPal" }
            };

            return Task.FromResult(entidades);
        }

        public async Task<List<SelectListItem>> GetProveedoresAsync()
        {
            try
            {
                var proveedores = await _proveedorService.GetProveedoresAsync();
                var result = proveedores.Select(p => new SelectListItem
                {
                    Value = p.ProveedorID.ToString(),
                    Text = p.Nombre
                }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener lista de proveedores");
                return new List<SelectListItem>();
            }
        }

        // Métodos migrados desde DropdownHelper
        public async Task PopulateProvinciasAsync(ClientesViewModel model)
        {
            var provincias = await GetProvinciasAsync();
            model.Provincias = provincias.Select(p => new SelectListItem
            {
                Value = p.ProvinciaID.ToString(),
                Text = p.Nombre
            }).ToList();
        }

        public async Task PopulateCiudadesAsync(ClientesViewModel model)
        {
            if (model.ProvinciaID > 0)
            {
                var ciudades = await GetCiudadesByProvinciaIDAsync(model.ProvinciaID);
                model.Ciudades = ciudades.Select(c => new SelectListItem
                {
                    Value = c.CiudadID.ToString(),
                    Text = c.Nombre
                }).ToList();
            }
            else
            {
                model.Ciudades = new List<SelectListItem>();
            }
        }

        public async Task PopulateProductDropdownsAsync(ProductosViewModel model)
        {
            model.Rubros = await GetRubrosAsync();
            model.Marcas = await GetMarcasAsync();
            model.SubRubros = await GetSubRubrosAsync(model.SelectedRubroID);
        }
    }
}