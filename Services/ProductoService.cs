// Archivo: Services/ProductoService.cs
using Javo2.DTOs;
using Javo2.IServices;
using Javo2.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Javo2.Services
{
    public class ProductoService : IProductoService
    {
        private readonly ILogger<ProductoService> _logger;
        private readonly ICatalogoService _catalogoService;

        private static readonly List<Producto> _productos = new();

        public ProductoService(ILogger<ProductoService> logger, ICatalogoService catalogoService)
        {
            _logger = logger;
            _catalogoService = catalogoService;
            SeedData();
        }

        private void SeedData()
        {
            _productos.Add(new Producto
            {
                ProductoID = 1,
                ProductoIDAlfa = "P001",
                CodBarra = "1234567890",
                Nombre = "Producto A",
                Descripcion = "Descripción del Producto A",
                PCosto = 100,
                PContado = 150,
                PLista = 200,
                PorcentajeIva = 21,
                RubroId = 1,
                SubRubroId = 1,
                MarcaId = 1,
                FechaMod = DateTime.Now,
                FechaModPrecio = DateTime.Now,
                Usuario = "cosmefulanito",
                ModificadoPor = "cosmefulanito",
                EstadoComentario = "Activo",
                CantidadStock = 10,
                DeudaTotal = 0
                // Inicializa otras propiedades según sea necesario
            });
            // Agrega más productos si es necesario
        }

        public Task<IEnumerable<Producto>> GetAllProductosAsync()
        {
            _logger.LogInformation("GetAllProductosAsync called");
            return Task.FromResult(_productos.AsEnumerable());
        }

        public Task<Producto?> GetProductoByIdAsync(int id)
        {
            _logger.LogInformation("GetProductoByIdAsync called with ID: {Id}", id);
            var producto = _productos.FirstOrDefault(p => p.ProductoID == id);
            return Task.FromResult(producto);
        }

        public Task CreateProductoAsync(Producto producto)
        {
            _logger.LogInformation("CreateProductoAsync called with Producto: {Producto}", producto.Nombre);
            producto.ProductoID = _productos.Any() ? _productos.Max(p => p.ProductoID) + 1 : 1;
            producto.ProductoIDAlfa = GenerarProductoIDAlfa();
            producto.CodBarra = GenerarCodBarraProducto();
            producto.FechaMod = DateTime.Now;
            producto.FechaModPrecio = DateTime.Now;
            producto.Usuario = "cosmefulanito";
            producto.ModificadoPor = "cosmefulanito";
            producto.EstadoComentario = "Activo";

            _productos.Add(producto);
            _logger.LogInformation("Producto created with ID: {Id}", producto.ProductoID);
            return Task.CompletedTask;
        }

        public Task UpdateProductoAsync(Producto producto)
        {
            _logger.LogInformation("UpdateProductoAsync called with Producto: {Producto}", producto.Nombre);
            var existingProducto = _productos.FirstOrDefault(p => p.ProductoID == producto.ProductoID);
            if (existingProducto == null)
            {
                throw new KeyNotFoundException($"Producto con ID {producto.ProductoID} no encontrado.");
            }

            // Actualizar las propiedades necesarias
            existingProducto.Nombre = producto.Nombre;
            existingProducto.Descripcion = producto.Descripcion;
            existingProducto.PCosto = producto.PCosto;
            existingProducto.PContado = producto.PContado;
            existingProducto.PLista = producto.PLista;
            existingProducto.PorcentajeIva = producto.PorcentajeIva;
            existingProducto.RubroId = producto.RubroId;
            existingProducto.SubRubroId = producto.SubRubroId;
            existingProducto.MarcaId = producto.MarcaId;
            existingProducto.FechaMod = DateTime.Now;
            existingProducto.ModificadoPor = "cosmefulanito"; // Actualizar según el usuario actual

            _logger.LogInformation("Producto updated with ID: {Id}", producto.ProductoID);
            return Task.CompletedTask;
        }

        public Task DeleteProductoAsync(int id)
        {
            _logger.LogInformation("DeleteProductoAsync called with ID: {Id}", id);
            var producto = _productos.FirstOrDefault(p => p.ProductoID == id);
            if (producto == null)
            {
                throw new KeyNotFoundException($"Producto con ID {id} no encontrado.");
            }

            _productos.Remove(producto);
            _logger.LogInformation("Producto deleted with ID: {Id}", id);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<Producto>> FilterProductosAsync(ProductoFilterDto filters)
        {
            _logger.LogInformation("FilterProductosAsync called with filters: {@Filters}", filters);
            var query = _productos.AsQueryable();

            if (!string.IsNullOrEmpty(filters.Nombre))
            {
                query = query.Where(p => p.Nombre.Contains(filters.Nombre, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(filters.Codigo))
            {
                query = query.Where(p => p.CodBarra.Contains(filters.Codigo) || p.ProductoIDAlfa.Contains(filters.Codigo));
            }

            if (!string.IsNullOrEmpty(filters.Marca))
            {
                if (int.TryParse(filters.Marca, out int marcaId))
                {
                    query = query.Where(p => p.MarcaId == marcaId);
                }
            }

            if (!string.IsNullOrEmpty(filters.Rubro))
            {
                if (int.TryParse(filters.Rubro, out int rubroId))
                {
                    query = query.Where(p => p.RubroId == rubroId);
                }
            }

            if (!string.IsNullOrEmpty(filters.SubRubro))
            {
                if (int.TryParse(filters.SubRubro, out int subRubroId))
                {
                    query = query.Where(p => p.SubRubroId == subRubroId);
                }
            }

            return Task.FromResult(query.AsEnumerable());
        }

        public Task<Producto?> GetProductoByCodigoAsync(string codigo)
        {
            _logger.LogInformation("GetProductoByCodigoAsync called with Codigo: {Codigo}", codigo);
            var producto = _productos.FirstOrDefault(p => p.CodBarra.Contains(codigo) || p.ProductoIDAlfa.Contains(codigo));
            return Task.FromResult(producto);
        }

        public Task<Producto?> GetProductoByNombreAsync(string nombre)
        {
            _logger.LogInformation("GetProductoByNombreAsync called with Nombre: {Nombre}", nombre);
            var producto = _productos.FirstOrDefault(p => p.Nombre.Contains(nombre, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(producto);
        }

        public Task<IEnumerable<Producto>> GetProductosByRubroAsync(string rubro)
        {
            _logger.LogInformation("GetProductosByRubroAsync called with Rubro: {Rubro}", rubro);
            // Implementar lógica para obtener productos por rubro
            return Task.FromResult(Enumerable.Empty<Producto>());
        }

        public Task<IEnumerable<string>> GetRubrosAutocompleteAsync(string term)
        {
            _logger.LogInformation("GetRubrosAutocompleteAsync called with Term: {Term}", term);
            // Implementar lógica para autocompletar rubros
            return Task.FromResult(Enumerable.Empty<string>());
        }

        public Task<IEnumerable<string>> GetMarcasAutocompleteAsync(string term)
        {
            _logger.LogInformation("GetMarcasAutocompleteAsync called with Term: {Term}", term);
            // Implementar lógica para autocompletar marcas
            return Task.FromResult(Enumerable.Empty<string>());
        }

        public Task<IEnumerable<string>> GetProductosAutocompleteAsync(string term)
        {
            _logger.LogInformation("GetProductosAutocompleteAsync called with Term: {Term}", term);
            var productos = _productos
                .Where(p => p.Nombre.Contains(term, StringComparison.OrdinalIgnoreCase))
                .Select(p => p.Nombre)
                .Distinct()
                .ToList();

            return Task.FromResult(productos.AsEnumerable());
        }

        public string GenerarProductoIDAlfa()
        {
            var productoIdAlfa = $"P{(_productos.Any() ? _productos.Max(p => p.ProductoID) + 1 : 1):D3}";
            _logger.LogInformation("Generated ProductoIDAlfa: {ProductoIDAlfa}", productoIdAlfa);
            return productoIdAlfa;
        }

        public string GenerarCodBarraProducto()
        {
            var codBarraProducto = $"QR{(_productos.Any() ? _productos.Max(p => p.ProductoID) + 1 : 1):D10}";
            _logger.LogInformation("Generated CodBarraProducto: {CodBarraProducto}", codBarraProducto);
            return codBarraProducto;
        }
    }
}
