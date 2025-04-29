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
    public class RolService : IRolService
    {
        private readonly ILogger<RolService> _logger;
        private readonly IPermisoService _permisoService;
        private static List<Rol> _roles = new List<Rol>();
        private static int _nextRolID = 1;
        private static int _nextRolPermisoID = 1;
        private static readonly object _lock = new object();
        private readonly string _jsonFilePath = "Data/roles.json";

        public RolService(ILogger<RolService> logger, IPermisoService permisoService)
        {
            _logger = logger;
            _permisoService = permisoService;
            CargarDesdeJson();

            // Si no hay roles, crear roles por defecto
            if (_roles.Count == 0)
            {
                CrearRolesPorDefecto().GetAwaiter().GetResult();
            }
        }

        private async Task CrearRolesPorDefecto()
        {
            try
            {
                // Crear rol de administrador
                var adminExiste = _roles.Any(r => r.Nombre.Equals("Administrador", StringComparison.OrdinalIgnoreCase));
                if (!adminExiste)
                {
                    var rolAdmin = new Rol
                    {
                        Nombre = "Administrador",
                        Descripcion = "Rol con acceso completo al sistema",
                        EsSistema = true
                    };

                    var adminId = await CreateRolAsync(rolAdmin);

                    // Asignar todos los permisos al administrador
                    var permisos = await _permisoService.GetAllPermisosAsync();
                    foreach (var permiso in permisos)
                    {
                        await AsignarPermisoAsync(adminId, permiso.PermisoID);
                    }
                }

                // Crear rol de gerente
                var gerenteExiste = _roles.Any(r => r.Nombre.Equals("Gerente", StringComparison.OrdinalIgnoreCase));
                if (!gerenteExiste)
                {
                    var rolGerente = new Rol
                    {
                        Nombre = "Gerente",
                        Descripcion = "Rol con acceso a funciones gerenciales",
                        EsSistema = true
                    };

                    var gerenteId = await CreateRolAsync(rolGerente);

                    // Asignar permisos específicos al gerente
                    // Ejemplo: Permisos para ver reportes, autorizar ventas, etc.
                    var permisos = await _permisoService.GetAllPermisosAsync();
                    var permisosGerente = permisos.Where(p =>
                        p.Codigo.StartsWith("reportes.") ||
                        p.Codigo.StartsWith("ventas.autorizar") ||
                        p.Codigo.StartsWith("ventas.ver") ||
                        p.Codigo.StartsWith("productos.ver") ||
                        p.Codigo.StartsWith("clientes.ver")).ToList();

                    foreach (var permiso in permisosGerente)
                    {
                        await AsignarPermisoAsync(gerenteId, permiso.PermisoID);
                    }
                }

                // Crear rol de vendedor
                var vendedorExiste = _roles.Any(r => r.Nombre.Equals("Vendedor", StringComparison.OrdinalIgnoreCase));
                if (!vendedorExiste)
                {
                    var rolVendedor = new Rol
                    {
                        Nombre = "Vendedor",
                        Descripcion = "Rol con acceso a funciones de ventas",
                        EsSistema = true
                    };

                    var vendedorId = await CreateRolAsync(rolVendedor);

                    // Asignar permisos específicos al vendedor
                    // Ejemplo: Permisos para crear ventas, ver productos, etc.
                    var permisos = await _permisoService.GetAllPermisosAsync();
                    var permisosVendedor = permisos.Where(p =>
                        p.Codigo.StartsWith("ventas.crear") ||
                        p.Codigo.StartsWith("ventas.ver") ||
                        p.Codigo.StartsWith("productos.ver") ||
                        p.Codigo.StartsWith("clientes.ver") ||
                        p.Codigo.StartsWith("clientes.crear")).ToList();

                    foreach (var permiso in permisosVendedor)
                    {
                        await AsignarPermisoAsync(vendedorId, permiso.PermisoID);
                    }
                }

                _logger.LogInformation("Roles por defecto creados");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear roles por defecto");
            }
        }

        private void CargarDesdeJson()
        {
            try
            {
                var data = JsonFileHelper.LoadFromJsonFile<List<Rol>>(_jsonFilePath);
                lock (_lock)
                {
                    _roles = data ?? new List<Rol>();
                    if (_roles.Any())
                    {
                        _nextRolID = _roles.Max(r => r.RolID) + 1;
                        _nextRolPermisoID = _roles.SelectMany(r => r.Permisos).Any()
                            ? _roles.SelectMany(r => r.Permisos).Max(p => p.RolPermisoID) + 1
                            : 1;
                    }
                }
                _logger.LogInformation("RolService: {Count} roles cargados", _roles.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar roles desde JSON");
                _roles = new List<Rol>();
            }
        }

        private void GuardarEnJson()
        {
            try
            {
                JsonFileHelper.SaveToJsonFile(_jsonFilePath, _roles);
                _logger.LogInformation("RolService: {Count} roles guardados", _roles.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar roles en JSON");
            }
        }

        public async Task<IEnumerable<Rol>> GetAllRolesAsync()
        {
            lock (_lock)
            {
                return _roles.ToList();
            }
        }

        public async Task<Rol> GetRolByIDAsync(int id)
        {
            lock (_lock)
            {
                return _roles.FirstOrDefault(r => r.RolID == id);
            }
        }

        public async Task<Rol> GetRolByNombreAsync(string nombre)
        {
            lock (_lock)
            {
                return _roles.FirstOrDefault(r => r.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase));
            }
        }

        // RolService.cs - Método CreateRolAsync
        public async Task<int> CreateRolAsync(Rol rol)
        {
            if (string.IsNullOrWhiteSpace(rol.Nombre))
                throw new ArgumentException("El nombre del rol no puede estar vacío");

            // Verificar si ya existe un rol con el mismo nombre
            var rolExistente = await GetRolByNombreAsync(rol.Nombre);
            if (rolExistente != null)
            {
                _logger.LogWarning("Intento de crear rol con nombre duplicado: {Nombre}", rol.Nombre);
                throw new InvalidOperationException($"Ya existe un rol con el nombre '{rol.Nombre}'");
            }

            // Continuar solo si el rol no existe
            lock (_lock)
            {
                rol.RolID = _nextRolID++;
                if (rol.Permisos == null)
                    rol.Permisos = new List<RolPermiso>();

                _roles.Add(rol);
                GuardarEnJson();
            }

            _logger.LogInformation("Rol creado: {Nombre} con ID {RolID}", rol.Nombre, rol.RolID);
            return rol.RolID;
        }

        public async Task<bool> UpdateRolAsync(Rol rol)
        {
            if (string.IsNullOrWhiteSpace(rol.Nombre))
                throw new ArgumentException("El nombre del rol no puede estar vacío");

            lock (_lock)
            {
                var existingRol = _roles.FirstOrDefault(r => r.RolID == rol.RolID);
                if (existingRol == null)
                    return false;

                // Verificar que el nuevo nombre no esté en uso por otro rol
                var otroRolMismoNombre = _roles.FirstOrDefault(r =>
                    r.RolID != rol.RolID &&
                    r.Nombre.Equals(rol.Nombre, StringComparison.OrdinalIgnoreCase));

                if (otroRolMismoNombre != null)
                    throw new InvalidOperationException("Ya existe otro rol con este nombre");

                existingRol.Nombre = rol.Nombre;
                existingRol.Descripcion = rol.Descripcion;
                // No actualizar EsSistema para prevenir modificaciones no autorizadas

                GuardarEnJson();
            }

            _logger.LogInformation("Rol actualizado: {Nombre}", rol.Nombre);
            return true;
        }

        public async Task<bool> DeleteRolAsync(int id)
        {
            lock (_lock)
            {
                var rol = _roles.FirstOrDefault(r => r.RolID == id);
                if (rol == null)
                    return false;

                if (rol.EsSistema)
                    throw new InvalidOperationException("No se puede eliminar un rol del sistema");

                _roles.Remove(rol);
                GuardarEnJson();
            }

            _logger.LogInformation("Rol eliminado: {RolID}", id);
            return true;
        }

        public async Task<IEnumerable<Permiso>> GetPermisosByRolIDAsync(int rolID)
        {
            var rol = await GetRolByIDAsync(rolID);
            if (rol == null)
                return Enumerable.Empty<Permiso>();

            var permisosIDs = rol.Permisos.Select(p => p.PermisoID).ToList();
            var permisos = new List<Permiso>();

            foreach (var permisoID in permisosIDs)
            {
                var permiso = await _permisoService.GetPermisoByIDAsync(permisoID);
                if (permiso != null)
                    permisos.Add(permiso);
            }

            return permisos;
        }

        public async Task<bool> AsignarPermisoAsync(int rolID, int permisoID)
        {
            _logger.LogInformation("INICIO: AsignarPermisoAsync para Rol {RolID}, Permiso {PermisoID}", rolID, permisoID);

            try
            {
                lock (_lock)
                {
                    _logger.LogInformation("BUSCANDO: Rol con ID {RolID}", rolID);
                    var rol = _roles.FirstOrDefault(r => r.RolID == rolID);
                    if (rol == null)
                    {
                        _logger.LogError("ERROR: Rol con ID {RolID} no encontrado", rolID);
                        return false;
                    }

                    _logger.LogInformation("ROL ENCONTRADO: {Nombre}", rol.Nombre);

                    // Verificar si el permiso ya está asignado
                    if (rol.Permisos.Any(p => p.PermisoID == permisoID))
                    {
                        _logger.LogInformation("AVISO: Permiso {PermisoID} ya asignado a rol {RolID}", permisoID, rolID);
                        return true; // Ya está asignado
                    }

                    // Asignar el permiso
                    _logger.LogInformation("ASIGNANDO: Permiso {PermisoID} a rol {RolID}", permisoID, rolID);
                    rol.Permisos.Add(new RolPermiso
                    {
                        RolPermisoID = _nextRolPermisoID++,
                        RolID = rolID,
                        PermisoID = permisoID
                    });

                    _logger.LogInformation("PERMISOS TOTALES: Rol {RolID} ahora tiene {Count} permisos",
                        rolID, rol.Permisos.Count);
                }

                _logger.LogInformation("GUARDANDO: Cambios en JSON");
                GuardarEnJson();

                _logger.LogInformation("ÉXITO: Permiso {PermisoID} asignado a rol {RolID}", permisoID, rolID);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ERROR CRÍTICO: Al asignar permiso {PermisoID} a rol {RolID}", permisoID, rolID);
                throw; // Re-lanzar para manejo superior
            }
        }

        public async Task<bool> QuitarPermisoAsync(int rolID, int permisoID)
        {
            lock (_lock)
            {
                var rol = _roles.FirstOrDefault(r => r.RolID == rolID);
                if (rol == null)
                    return false;

                var rolPermiso = rol.Permisos.FirstOrDefault(p => p.PermisoID == permisoID);
                if (rolPermiso == null)
                    return true; // No estaba asignado

                rol.Permisos.Remove(rolPermiso);
                GuardarEnJson();
            }

            _logger.LogInformation("Permiso {PermisoID} removido del rol {RolID}", permisoID, rolID);
            return true;
        }

        public async Task<bool> TienePermisoAsync(int rolID, string codigoPermiso)
        {
            var permisos = await GetPermisosByRolIDAsync(rolID);
            return permisos.Any(p => p.Codigo == codigoPermiso && p.Activo);
        }
    }
}