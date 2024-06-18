using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Javo2.ViewModels.Operaciones.Proveedores;
using Javo2.ViewModels.Operaciones.Productos;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Javo2.IServices;

namespace Javo2.Services
{
    public class ProveedorService : IProveedorService
    {
        private readonly IMapper _mapper;
        private readonly ILogger<ProveedorService> _logger;
        private readonly IProductoService _productoService;

        private static readonly List<ProveedoresViewModel> _proveedores = new()
        {
        };

        public ProveedorService(IMapper mapper, ILogger<ProveedorService> logger, IProductoService productoService)
        {
            _mapper = mapper;
            _logger = logger;
            _productoService = productoService;
        }

        public async Task<IEnumerable<ProveedoresViewModel>> GetProveedoresAsync()
        {
            _logger.LogInformation("GetProveedoresAsync called");
            return await Task.FromResult(_proveedores);
        }

        public async Task<ProveedoresViewModel?> GetProveedorByIdAsync(int id)
        {
            _logger.LogInformation("GetProveedorByIdAsync called with ID: {Id}", id);
            var proveedor = _proveedores.FirstOrDefault(p => p.ProveedorID == id);
            return await Task.FromResult(proveedor);
        }

        public async Task CreateProveedorAsync(ProveedoresViewModel proveedorViewModel)
        {
            _logger.LogInformation("CreateProveedorAsync called with Proveedor: {Proveedor}", proveedorViewModel);
            proveedorViewModel.ProveedorID = _proveedores.Count > 0 ? _proveedores.Max(p => p.ProveedorID) + 1 : 1;
            _proveedores.Add(proveedorViewModel);
            _logger.LogInformation("Proveedor created with ID: {Id}", proveedorViewModel.ProveedorID);
            await Task.CompletedTask;
        }

        public async Task UpdateProveedorAsync(ProveedoresViewModel proveedorViewModel)
        {
            _logger.LogInformation("UpdateProveedorAsync called with Proveedor: {Proveedor}", proveedorViewModel);
            var proveedor = _proveedores.FirstOrDefault(p => p.ProveedorID == proveedorViewModel.ProveedorID);
            if (proveedor != null)
            {
                _mapper.Map(proveedorViewModel, proveedor);
                _logger.LogInformation("Proveedor updated with ID: {Id}", proveedorViewModel.ProveedorID);
            }
            await Task.CompletedTask;
        }

        public async Task DeleteProveedorAsync(int id)
        {
            _logger.LogInformation("DeleteProveedorAsync called with ID: {Id}", id);
            var proveedor = _proveedores.FirstOrDefault(p => p.ProveedorID == id);
            if (proveedor != null)
            {
                _proveedores.Remove(proveedor);
                _logger.LogInformation("Proveedor deleted with ID: {Id}", id);
            }
            await Task.CompletedTask;
        }

        public async Task<IEnumerable<ProductosViewModel>> GetProductosDisponiblesAsync()
        {
            _logger.LogInformation("GetProductosDisponiblesAsync called");
            var productos = await _productoService.GetAllProductosAsync();
            return productos;
        }
    }
}