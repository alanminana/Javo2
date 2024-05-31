using javo2.IServices;
using javo2.ViewModels.Operaciones.Productos;
using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace javo2.Services
{
    public class ProductoService : IProductoService
    {
        private readonly IMapper _mapper;
        private readonly ILogger<ProductoService> _logger;

        private static readonly List<ProductosViewModel> _productos = new()
        {
            new() { ProductoID = 1, Nombre = "Producto A", ProductoIDAlfa = "P001", CodBarra = "1234567890", Marca = "Marca A" },
            new() { ProductoID = 2, Nombre = "Producto B", ProductoIDAlfa = "P002", CodBarra = "0987654321", Marca = "Marca B" }
        };

        public ProductoService(IMapper mapper, ILogger<ProductoService> logger)
        {
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<ProductosViewModel>> GetAllProductosAsync()
        {
            _logger.LogInformation("GetAllProductosAsync called");
            return await Task.FromResult(_productos);
        }

        public async Task<ProductosViewModel?> GetProductoByIdAsync(int id)
        {
            _logger.LogInformation("GetProductoByIdAsync called with ID: {Id}", id);
            var producto = _productos.FirstOrDefault(p => p.ProductoID == id);
            return await Task.FromResult(producto);
        }

        public async Task CreateProductoAsync(ProductosViewModel productoViewModel)
        {
            _logger.LogInformation("CreateProductoAsync called with Producto: {Producto}", productoViewModel.Nombre);
            productoViewModel.ProductoID = _productos.Any() ? _productos.Max(p => p.ProductoID) + 1 : 1;
            productoViewModel.ProductoIDAlfa = GenerarProductoIDAlfa();
            productoViewModel.CodBarra = GenerarCodBarraProducto();
            _productos.Add(productoViewModel);
            _logger.LogInformation("Producto created with ID: {Id}", productoViewModel.ProductoID);
            await Task.CompletedTask;
        }

        public async Task UpdateProductoAsync(ProductosViewModel productoViewModel)
        {
            _logger.LogInformation("UpdateProductoAsync called with Producto: {Producto}", productoViewModel.Nombre);
            var producto = _productos.FirstOrDefault(p => p.ProductoID == productoViewModel.ProductoID);
            if (producto != null)
            {
                _mapper.Map(productoViewModel, producto);
                _logger.LogInformation("Producto updated with ID: {Id}", productoViewModel.ProductoID);
            }
            await Task.CompletedTask;
        }

        public async Task DeleteProductoAsync(int id)
        {
            _logger.LogInformation("DeleteProductoAsync called with ID: {Id}", id);
            var producto = _productos.FirstOrDefault(p => p.ProductoID == id);
            if (producto != null)
            {
                _productos.Remove(producto);
                _logger.LogInformation("Producto deleted with ID: {Id}", id);
            }
            await Task.CompletedTask;
        }

        public async Task<ProductosViewModel?> GetProductoByCodigoAsync(string codigo)
        {
            _logger.LogInformation("GetProductoByCodigoAsync called with Codigo: {Codigo}", codigo);
            var producto = _productos.FirstOrDefault(p => p.CodBarra.Contains(codigo) || p.ProductoIDAlfa.Contains(codigo));
            return await Task.FromResult(producto);
        }

        public async Task<ProductosViewModel?> GetProductoByNombreAsync(string nombre)
        {
            _logger.LogInformation("GetProductoByNombreAsync called with Nombre: {Nombre}", nombre);
            var producto = _productos.FirstOrDefault(p => p.Nombre.Contains(nombre));
            return await Task.FromResult(producto);
        }

        public async Task<IEnumerable<ProductosViewModel>> GetProductosByRubroAsync(string rubro)
        {
            _logger.LogInformation("GetProductosByRubroAsync called with Rubro: {Rubro}", rubro);
            var productos = _productos.Where(p => p.Rubros.Any(r => r.Text.Contains(rubro)));
            return await Task.FromResult(productos);
        }

        public async Task<IEnumerable<string>> GetRubrosAutocompleteAsync(string term)
        {
            _logger.LogInformation("GetRubrosAutocompleteAsync called with Term: {Term}", term);
            var rubros = _productos.SelectMany(p => p.Rubros.Select(r => r.Text))
                                   .Distinct()
                                   .Where(r => r.Contains(term));
            return await Task.FromResult(rubros);
        }

        public async Task<IEnumerable<SelectListItem>> GetRubrosAsync()
        {
            _logger.LogInformation("Getting Rubros for SelectList");
            var rubros = _productos.SelectMany(p => p.Rubros).Distinct().ToList();
            return await Task.FromResult(rubros);
        }

        public async Task<IEnumerable<SelectListItem>> GetSubRubrosAsync()
        {
            _logger.LogInformation("Getting SubRubros for SelectList");
            var subRubros = _productos.SelectMany(p => p.SubRubros).Distinct().ToList();
            return await Task.FromResult(subRubros);
        }

        public async Task<IEnumerable<SelectListItem>> GetMarcasAsync()
        {
            _logger.LogInformation("Getting Marcas for SelectList");
            var marcas = _productos.Select(p => new SelectListItem { Value = p.Marca, Text = p.Marca }).Distinct().ToList();
            return await Task.FromResult(marcas);
        }

        public async Task<IEnumerable<string>> GetMarcasAutocompleteAsync(string term)
        {
            _logger.LogInformation("GetMarcasAutocompleteAsync called with term: {Term}", term);
            var marcas = _productos
                .Where(p => p.Marca != null && p.Marca.Contains(term))
                .Select(p => p.Marca)
                .Distinct()
                .ToList();
            return await Task.FromResult(marcas);
        }

        public async Task<IEnumerable<string>> GetProductosAutocompleteAsync(string term)
        {
            _logger.LogInformation("GetProductosAutocompleteAsync called with Term: {Term}", term);
            var productos = _productos
                .Where(p => p.Nombre.Contains(term))
                .Select(p => p.Nombre)
                .Distinct()
                .ToList();
            return await Task.FromResult(productos);
        }

        public string GenerarProductoIDAlfa()
        {
            var productoIdAlfa = $"P{_productos.Max(p => p.ProductoID) + 1:000}";
            _logger.LogInformation("Generated ProductoIDAlfa: {ProductoIDAlfa}", productoIdAlfa);
            return productoIdAlfa;
        }

        public string GenerarCodBarraProducto()
        {
            var codBarraProducto = $"QR{_productos.Max(p => p.ProductoID) + 1:0000000000}";
            _logger.LogInformation("Generated CodBarraProducto: {CodBarraProducto}", codBarraProducto);
            return codBarraProducto;
        }
    }
}
