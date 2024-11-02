// Archivo: Services/ProveedorService.cs
using Javo2.IServices;
using Javo2.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Javo2.Services
{
    /// <summary>
    /// Servicio para manejar operaciones relacionadas con proveedores.
    /// </summary>
    public class ProveedorService : IProveedorService
    {
        private readonly ILogger<ProveedorService> _logger;

        // Almacenamiento en memoria para proveedores.
        private static readonly List<Proveedor> _proveedores = new();
        private static readonly object _lock = new(); // Para garantizar la seguridad de hilos.

        /// <summary>
        /// Constructor del servicio de proveedores.
        /// </summary>
        /// <param name="logger">Instancia de ILogger para registrar información.</param>
        public ProveedorService(ILogger<ProveedorService> logger)
        {
            _logger = logger;

            // Inicializar datos de ejemplo si la lista está vacía.
            if (!_proveedores.Any())
            {
                SeedData();
            }
        }

        /// <summary>
        /// Inicializa datos de ejemplo en la lista de proveedores.
        /// </summary>
        private void SeedData()
        {
            lock (_lock)
            {
                _proveedores.AddRange(new List<Proveedor>
                {
                    new Proveedor
                    {
                        ProveedorID = 1,
                        Nombre = "Proveedor Ejemplo 1",
                        Direccion = "Calle Falsa 123",
                        Telefono = "123456789",
                        Email = "proveedor1@example.com",
                        CondicionesPago = "30 días",
                        ProductosAsignados = new List<int> { 1, 2 }
                    },
                    new Proveedor
                    {
                        ProveedorID = 2,
                        Nombre = "Proveedor Ejemplo 2",
                        Direccion = "Avenida Siempre Viva 742",
                        Telefono = "987654321",
                        Email = "proveedor2@example.com",
                        CondicionesPago = "15 días",
                        ProductosAsignados = new List<int> { 3 }
                    }
                });
            }
        }

        /// <summary>
        /// Obtiene todos los proveedores.
        /// </summary>
        /// <returns>Una lista de proveedores.</returns>
        public async Task<IEnumerable<Proveedor>> GetProveedoresAsync()
        {
            _logger.LogInformation("GetProveedoresAsync llamado");
            List<Proveedor> proveedoresCopy;
            lock (_lock)
            {
                proveedoresCopy = _proveedores.ToList();
            }
            return await Task.FromResult(proveedoresCopy);
        }

        /// <summary>
        /// Obtiene un proveedor por su ID.
        /// </summary>
        /// <param name="id">ID del proveedor.</param>
        /// <returns>El proveedor si se encuentra, de lo contrario null.</returns>
        public async Task<Proveedor?> GetProveedorByIdAsync(int id)
        {
            _logger.LogInformation("GetProveedorByIdAsync llamado con ID: {Id}", id);
            Proveedor? proveedor;
            lock (_lock)
            {
                proveedor = _proveedores.FirstOrDefault(p => p.ProveedorID == id);
            }
            return await Task.FromResult(proveedor);
        }

        /// <summary>
        /// Crea un nuevo proveedor.
        /// </summary>
        /// <param name="proveedor">El proveedor a crear.</param>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        public async Task CreateProveedorAsync(Proveedor proveedor)
        {
            _logger.LogInformation("CreateProveedorAsync llamado con Proveedor: {Proveedor}", proveedor.Nombre);
            lock (_lock)
            {
                proveedor.ProveedorID = _proveedores.Any() ? _proveedores.Max(p => p.ProveedorID) + 1 : 1;
                _proveedores.Add(proveedor);
                _logger.LogInformation("Proveedor creado con ID: {Id}", proveedor.ProveedorID);
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// Actualiza un proveedor existente.
        /// </summary>
        /// <param name="proveedor">El proveedor con los datos actualizados.</param>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        public async Task UpdateProveedorAsync(Proveedor proveedor)
        {
            _logger.LogInformation("UpdateProveedorAsync llamado con Proveedor: {Proveedor}", proveedor.Nombre);
            lock (_lock)
            {
                var existingProveedor = _proveedores.FirstOrDefault(p => p.ProveedorID == proveedor.ProveedorID);
                if (existingProveedor == null)
                {
                    throw new KeyNotFoundException($"Proveedor con ID {proveedor.ProveedorID} no encontrado.");
                }

                // Actualizar las propiedades necesarias
                existingProveedor.Nombre = proveedor.Nombre;
                existingProveedor.Direccion = proveedor.Direccion;
                existingProveedor.Telefono = proveedor.Telefono;
                existingProveedor.Email = proveedor.Email;
                existingProveedor.CondicionesPago = proveedor.CondicionesPago;
                existingProveedor.ProductosAsignados = proveedor.ProductosAsignados;
                _logger.LogInformation("Proveedor actualizado con ID: {Id}", proveedor.ProveedorID);
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// Elimina un proveedor por su ID.
        /// </summary>
        /// <param name="id">ID del proveedor a eliminar.</param>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        public async Task DeleteProveedorAsync(int id)
        {
            _logger.LogInformation("DeleteProveedorAsync llamado con ID: {Id}", id);
            lock (_lock)
            {
                var proveedor = _proveedores.FirstOrDefault(p => p.ProveedorID == id);
                if (proveedor == null)
                {
                    throw new KeyNotFoundException($"Proveedor con ID {id} no encontrado.");
                }

                _proveedores.Remove(proveedor);
                _logger.LogInformation("Proveedor eliminado con ID: {Id}", id);
            }
            await Task.CompletedTask;
        }
    }
}
