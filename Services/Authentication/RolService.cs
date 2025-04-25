
// Services/Authentication/RolService.cs
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
        private static readonly object _lock = new object();
        private readonly string _jsonFilePath = "Data/roles.json";

        public RolService(ILogger<RolService> logger, IPermisoService permisoService)
        {
            _logger = logger;
            _permisoService = permisoService;
            CargarDesdeJson();

            // Si no hay roles, crear los roles por defecto
            if (_roles.Count == 0)
            {
                CrearRolesPorDefecto();
            }
        }

        private void CrearRolesPorDefecto()
        {
            try
            {
                // Crear roles por defecto
                _roles.Add(new Rol
                {
                    RolID = _nextRolID++,
                    Nombre = "Administrador",
                    Descripcion = "Acceso completo al sistema",
                    EsSistema = true
                });

                _roles.Add(new Rol
                {
                    RolID = _nextRolID++,
                    Nombre = "Gerente",
                    Descripcion = "Acceso a reportes y aprobaciones",
                    EsSistema = true
                });

                _roles.Add(new Rol
                {
                    RolID = _nextRolID++,
                    Nombre = "Vendedor",
                    Descripcion = "Acceso a ventas y consultas",
                    EsSistema = true
                });

                GuardarEnJson();
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

        public async Task<bool> CreateRolAsync(Rol rol)
        {
            // Verificar si el rol ya existe
            var rolExistente = await GetRolByNombreAsync(rol.Nombre);
            if (rolExistente != null)
            {
                throw new InvalidOperationException("El nombre del rol ya existe");
            }

            lock (_lock)
            {
                rol.RolID = _nextRolID++;
                _roles.Add(rol);
                GuardarEnJson();
            }

            _logger.LogInformation("Rol creado: {Nombre}", rol.Nombre);
            return true;
        }

        public async Task<bool> UpdateRolAsync(Rol rol)
        {
            lock (_lock)
            {
                var existing = _roles.FirstOrDefault(r => r.RolID == rol.RolID);
                if (existing == null)
                {
                    return false;
                }

                // No permitir modificar roles del sistema
                if (existing.EsSistema && rol.Nombre != existing.Nombre)
                {
                    throw new InvalidOperationException("No se puede modificar el nombre de un rol del sistema");
                }

                // Verificar si el nuevo nombre ya existe en otro rol
                var otherWithSameName = _roles.FirstOrDefault(r =>
                    r.RolID != rol.RolID &&
                    r.Nombre.Equals(rol.Nombre, StringComparison.OrdinalIgnoreCase));

                if (otherWithSameName != null)
                {
                    throw new InvalidOperationException("El nombre del rol ya existe");
                }

                existing.Nombre = rol.Nombre;
                existing.Descripcion = rol.Descripcion;

                GuardarEnJson();
            }

            _logger.LogInformation("Rol actualizado: {RolID}", rol.RolID);
            return true;
        }

        public async Task<bool> DeleteRolAsync(int id)
        {
            lock (_lock)
            {
                var rol = _roles.FirstOrDefault(r => r.RolID == id);
                if (rol == null)
                {
                    return false;
                }

                // No permitir eliminar roles del sistema
                if (rol.EsSistema)
                {
                    throw new InvalidOperationException("No se puede eliminar un rol del sistema");
                }

                _roles.Remove(rol);
                GuardarEnJson();
            }

            _logger.LogInformation("Rol eliminado: {RolID}", id);
            return true;
        }

        public async Task<bool> AsignarPermisoAsync(int rolID, int permisoID)
        {
            lock (_lock)
            {
                var rol = _roles.FirstOrDefault(r => r.RolID == rolID);
                if (rol == null)
                {
                    return false;
                }

                // Verificar si el permiso ya está asignado
                if (rol.Permisos.Any(p => p.PermisoID == permisoID))
                {
                    return true; // El permiso ya está asignado
                }

                // Asignar el permiso
                rol.Permisos.Add(new RolPermiso
                {
                    RolID = rolID,
                    PermisoID = permisoID
                });

                GuardarEnJson();
            }

            _logger.LogInformation("Permiso {PermisoID} asignado a rol {RolID}", permisoID, rolID);
            return true;
        }

        public async Task<bool> QuitarPermisoAsync(int rolID, int permisoID)
        {
            lock (_lock)
            {
                var rol = _roles.FirstOrDefault(r => r.RolID == rolID);
                if (rol == null)
                {
                    return false;
                }

                var rolPermiso = rol.Permisos.FirstOrDefault(p => p.PermisoID == permisoID);
                if (rolPermiso == null)
                {
                    return true; // El permiso no estaba asignado
                }

                rol.Permisos.Remove(rolPermiso);
                GuardarEnJson();
            }

            _logger.LogInformation("Permiso {PermisoID} quitado del rol {RolID}", permisoID, rolID);
            return true;
        