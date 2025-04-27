// Services/Authentication/PermisoService.cs
using Javo2.Helpers;
using Javo2.IServices.Authentication;
using Javo2.Models.Authentication;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Javo2.Services.Authentication
{
    public class PermisoService : IPermisoService
    {
        private readonly ILogger<PermisoService> _logger;
        private static List<Permiso> _permisos = new List<Permiso>();
        private static int _nextPermisoID = 1;
        private static readonly object _lock = new object();
        private readonly string _jsonFilePath = "Data/permisos.json";

        public PermisoService(ILogger<PermisoService> logger)
        {
            _logger = logger;
            CargarDesdeJson();

            // Si no hay permisos, crear los permisos por defecto
            if (_permisos.Count == 0)
            {
                CrearPermisosPorDefecto();
            }
        }

        private void CrearPermisosPorDefecto()
        {
            try
            {
                // Permisos para usuarios
                CrearPermisoSistema("Ver usuarios", "usuarios.ver", "Usuarios");
                CrearPermisoSistema("Crear usuarios", "usuarios.crear", "Usuarios");
                CrearPermisoSistema("Editar usuarios", "usuarios.editar", "Usuarios");
                CrearPermisoSistema("Eliminar usuarios", "usuarios.eliminar", "Usuarios");

                // Permisos para roles
                CrearPermisoSistema("Ver roles", "roles.ver", "Roles");
                CrearPermisoSistema("Crear roles", "roles.crear", "Roles");
                CrearPermisoSistema("Editar roles", "roles.editar", "Roles");
                CrearPermisoSistema("Eliminar roles", "roles.eliminar", "Roles");

                // Permisos para permisos
                CrearPermisoSistema("Ver permisos", "permisos.ver", "Permisos");
                CrearPermisoSistema("Crear permisos", "permisos.crear", "Permisos");
                CrearPermisoSistema("Editar permisos", "permisos.editar", "Permisos");
                CrearPermisoSistema("Eliminar permisos", "permisos.eliminar", "Permisos");

                // Permisos para ventas
                CrearPermisoSistema("Ver ventas", "ventas.ver", "Ventas");
                CrearPermisoSistema("Crear ventas", "ventas.crear", "Ventas");
                CrearPermisoSistema("Editar ventas", "ventas.editar", "Ventas");
                CrearPermisoSistema("Eliminar ventas", "ventas.eliminar", "Ventas");
                CrearPermisoSistema("Autorizar ventas", "ventas.autorizar", "Ventas");
                CrearPermisoSistema("Rechazar ventas", "ventas.rechazar", "Ventas");

                // Permisos para productos
                CrearPermisoSistema("Ver productos", "productos.ver", "Productos");
                CrearPermisoSistema("Crear productos", "productos.crear", "Productos");
                CrearPermisoSistema("Editar productos", "productos.editar", "Productos");
                CrearPermisoSistema("Eliminar productos", "productos.eliminar", "Productos");
                CrearPermisoSistema("Ajustar precios", "productos.ajustarprecios", "Productos");

                // Permisos para clientes
                CrearPermisoSistema("Ver clientes", "clientes.ver", "Clientes");
                CrearPermisoSistema("Crear clientes", "clientes.crear", "Clientes");
                CrearPermisoSistema("Editar clientes", "clientes.editar", "Clientes");
                CrearPermisoSistema("Eliminar clientes", "clientes.eliminar", "Clientes");

                // Permisos para reportes
                CrearPermisoSistema("Ver reportes", "reportes.ver", "Reportes");
                CrearPermisoSistema("Exportar reportes", "reportes.exportar", "Reportes");

                // Permisos para configuración
                CrearPermisoSistema("Ver configuración", "configuracion.ver", "Configuración");
                CrearPermisoSistema("Editar configuración", "configuracion.editar", "Configuración");

                GuardarEnJson();
                _logger.LogInformation("Permisos por defecto creados");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear permisos por defecto");
            }
        }

        private void CrearPermisoSistema(string nombre, string codigo, string grupo)
        {
            _permisos.Add(new Permiso
            {
                PermisoID = _nextPermisoID++,
                Nombre = nombre,
                Codigo = codigo,
                Grupo = grupo,
                Descripcion = $"Permiso para {nombre.ToLower()}",
                EsSistema = true,
                Activo = true
            });
        }

        private void CargarDesdeJson()
        {
            try
            {
                var data = JsonFileHelper.LoadFromJsonFile<List<Permiso>>(_jsonFilePath);
                lock (_lock)
                {
                    _permisos = data ?? new List<Permiso>();
                    if (_permisos.Any())
                    {
                        _nextPermisoID = _permisos.Max(p => p.PermisoID) + 1;
                    }
                }
                _logger.LogInformation("PermisoService: {Count} permisos cargados", _permisos.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar permisos desde JSON");
                _permisos = new List<Permiso>();
            }
        }

        private void GuardarEnJson()
        {
            try
            {
                JsonFileHelper.SaveToJsonFile(_jsonFilePath, _permisos);
                _logger.LogInformation("PermisoService: {Count} permisos guardados", _permisos.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar permisos en JSON");
            }
        }

        public async Task<IEnumerable<Permiso>> GetAllPermisosAsync()
        {
            lock (_lock)
            {
                return _permisos.ToList();
            }
        }

        public async Task<Permiso> GetPermisoByIDAsync(int id)
        {
            lock (_lock)
            {
                return _permisos.FirstOrDefault(p => p.PermisoID == id);
            }
        }

        public async Task<Permiso> GetPermisoByCodigo(string codigo)
        {
            lock (_lock)
            {
                return _permisos.FirstOrDefault(p => p.Codigo.Equals(codigo, StringComparison.OrdinalIgnoreCase));
            }
        }

        public async Task<bool> CreatePermisoAsync(Permiso permiso)
        {
            // Verificar si el código ya existe
            var existente = await GetPermisoByCodigo(permiso.Codigo);
            if (existente != null)
            {
                throw new InvalidOperationException("Ya existe un permiso con este código");
            }

            lock (_lock)
            {
                permiso.PermisoID = _nextPermisoID++;
                permiso.EsSistema = false; // Los permisos creados manualmente nunca son de sistema
                _permisos.Add(permiso);
                GuardarEnJson();
            }

            _logger.LogInformation("Permiso creado: {Nombre}", permiso.Nombre);
            return true;
        }

        public async Task<bool> UpdatePermisoAsync(Permiso permiso)
        {
            lock (_lock)
            {
                var existing = _permisos.FirstOrDefault(p => p.PermisoID == permiso.PermisoID);
                if (existing == null)
                {
                    return false;
                }

                // No permitir modificar permisos del sistema
                if (existing.EsSistema &&
                    (permiso.Nombre != existing.Nombre || permiso.Codigo != existing.Codigo))
                {
                    throw new InvalidOperationException("No se puede modificar el nombre o código de un permiso del sistema");
                }

                // Verificar si el nuevo código ya existe en otro permiso
                var otherWithSameCode = _permisos.FirstOrDefault(p =>
                    p.PermisoID != permiso.PermisoID &&
                    p.Codigo.Equals(permiso.Codigo, StringComparison.OrdinalIgnoreCase));

                if (otherWithSameCode != null)
                {
                    throw new InvalidOperationException("El código del permiso ya existe");
                }

                existing.Nombre = permiso.Nombre;
                existing.Codigo = permiso.Codigo;
                existing.Descripcion = permiso.Descripcion;
                existing.Grupo = permiso.Grupo;
                existing.Activo = permiso.Activo;

                // No modificar si es de sistema
                permiso.EsSistema = existing.EsSistema;

                GuardarEnJson();
            }

            _logger.LogInformation("Permiso actualizado: {PermisoID}", permiso.PermisoID);
            return true;
        }

        public async Task<bool> DeletePermisoAsync(int id)
        {
            lock (_lock)
            {
                var permiso = _permisos.FirstOrDefault(p => p.PermisoID == id);
                if (permiso == null)
                {
                    return false;
                }

                // No permitir eliminar permisos del sistema
                if (permiso.EsSistema)
                {
                    throw new InvalidOperationException("No se puede eliminar un permiso del sistema");
                }

                _permisos.Remove(permiso);
                GuardarEnJson();
            }

            _logger.LogInformation("Permiso eliminado: {PermisoID}", id);
            return true;
        }
    }
}