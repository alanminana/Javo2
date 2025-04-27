using Javo2.Helpers;
using Javo2.IServices.Authentication;
using Javo2.Models.Authentication;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Javo2.Services.Authentication
{
    public class UsuarioService : IUsuarioService
    {
        private readonly ILogger<UsuarioService> _logger;
        private readonly IRolService _rolService;
        private static List<Usuario> _usuarios = new List<Usuario>();
        private static int _nextUsuarioID = 1;
        private static readonly object _lock = new object();
        private readonly string _jsonFilePath = "Data/usuarios.json";

        public UsuarioService(ILogger<UsuarioService> logger, IRolService rolService)
        {
            _logger = logger;
            _rolService = rolService;
            CargarDesdeJson();

            // Si no hay usuarios, crear admin por defecto
            if (_usuarios.Count == 0)
            {
                CrearUsuarioAdmin().GetAwaiter().GetResult();
            }
        }

        private async Task CrearUsuarioAdmin()
        {
            try
            {
                var adminExiste = _usuarios.Any(u => u.NombreUsuario.Equals("admin", StringComparison.OrdinalIgnoreCase));

                if (!adminExiste)
                {
                    var admin = new Usuario
                    {
                        NombreUsuario = "admin",
                        Nombre = "Administrador",
                        Apellido = "Sistema",
                        Email = "admin@sistema.com",
                        CreadoPor = "Sistema",
                        Activo = true
                    };

                    await CreateUsuarioAsync(admin, "admin");

                    // Asignar rol de administrador
                    var roles = await _rolService.GetAllRolesAsync();
                    var rolAdmin = roles.FirstOrDefault(r => r.Nombre.Equals("Administrador", StringComparison.OrdinalIgnoreCase));

                    if (rolAdmin != null)
                    {
                        await AsignarRolAsync(admin.UsuarioID, rolAdmin.RolID);
                    }

                    _logger.LogInformation("Usuario administrador creado automáticamente");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario administrador por defecto");
            }
        }

        private void CargarDesdeJson()
        {
            try
            {
                var data = JsonFileHelper.LoadFromJsonFile<List<Usuario>>(_jsonFilePath);
                lock (_lock)
                {
                    _usuarios = data ?? new List<Usuario>();
                    if (_usuarios.Any())
                    {
                        _nextUsuarioID = _usuarios.Max(u => u.UsuarioID) + 1;
                    }
                }
                _logger.LogInformation("UsuarioService: {Count} usuarios cargados", _usuarios.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar usuarios desde JSON");
                _usuarios = new List<Usuario>();
            }
        }

        private void GuardarEnJson()
        {
            try
            {
                JsonFileHelper.SaveToJsonFile(_jsonFilePath, _usuarios);
                _logger.LogInformation("UsuarioService: {Count} usuarios guardados", _usuarios.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar usuarios en JSON");
            }
        }

        private string HashContraseña(string contraseña)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(contraseña));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public async Task<IEnumerable<Usuario>> GetAllUsuariosAsync()
        {
            lock (_lock)
            {
                return _usuarios.ToList();
            }
        }

        public async Task<Usuario> GetUsuarioByIDAsync(int id)
        {
            lock (_lock)
            {
                return _usuarios.FirstOrDefault(u => u.UsuarioID == id);
            }
        }

        public async Task<Usuario> GetUsuarioByNombreUsuarioAsync(string nombreUsuario)
        {
            lock (_lock)
            {
                return _usuarios.FirstOrDefault(u => u.NombreUsuario.Equals(nombreUsuario, StringComparison.OrdinalIgnoreCase));
            }
        }

        public async Task<Usuario> GetUsuarioByEmailAsync(string email)
        {
            lock (_lock)
            {
                return _usuarios.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
            }
        }

        public async Task<bool> CreateUsuarioAsync(Usuario usuario, string contraseña)
        {
            if (string.IsNullOrWhiteSpace(contraseña))
            {
                throw new ArgumentException("La contraseña no puede estar vacía");
            }

            // Verificar si el usuario ya existe
            var usuarioExistente = await GetUsuarioByNombreUsuarioAsync(usuario.NombreUsuario);
            if (usuarioExistente != null)
            {
                throw new InvalidOperationException("El nombre de usuario ya existe");
            }

            // Verificar si el email ya existe
            var emailExistente = await GetUsuarioByEmailAsync(usuario.Email);
            if (emailExistente != null)
            {
                throw new InvalidOperationException("El email ya está en uso");
            }

            lock (_lock)
            {
                usuario.UsuarioID = _nextUsuarioID++;
                usuario.Contraseña = HashContraseña(contraseña);
                usuario.FechaCreacion = DateTime.Now;
                _usuarios.Add(usuario);
                GuardarEnJson();
            }

            _logger.LogInformation("Usuario creado: {NombreUsuario}", usuario.NombreUsuario);
            return true;
        }

        public async Task<bool> UpdateUsuarioAsync(Usuario usuario, string contraseñaNueva = null)
        {
            lock (_lock)
            {
                var existing = _usuarios.FirstOrDefault(u => u.UsuarioID == usuario.UsuarioID);
                if (existing == null)
                {
                    return false;
                }

                // Actualizar propiedades pero preservar contraseña si no se proporciona una nueva
                existing.Nombre = usuario.Nombre;
                existing.Apellido = usuario.Apellido;
                existing.Email = usuario.Email;
                existing.Activo = usuario.Activo;

                // Actualizar contraseña solo si se proporciona una nueva
                if (!string.IsNullOrWhiteSpace(contraseñaNueva))
                {
                    existing.Contraseña = HashContraseña(contraseñaNueva);
                }

                GuardarEnJson();
            }

            _logger.LogInformation("Usuario actualizado: {UsuarioID}", usuario.UsuarioID);
            return true;
        }

        public async Task<bool> DeleteUsuarioAsync(int id)
        {
            lock (_lock)
            {
                var usuario = _usuarios.FirstOrDefault(u => u.UsuarioID == id);
                if (usuario == null)
                {
                    return false;
                }

                _usuarios.Remove(usuario);
                GuardarEnJson();
            }

            _logger.LogInformation("Usuario eliminado: {UsuarioID}", id);
            return true;
        }

        public async Task<bool> CambiarContraseñaAsync(int usuarioID, string contraseñaActual, string contraseñaNueva)
        {
            lock (_lock)
            {
                var usuario = _usuarios.FirstOrDefault(u => u.UsuarioID == usuarioID);
                if (usuario == null)
                {
                    return false;
                }

                string hashActual = HashContraseña(contraseñaActual);
                if (usuario.Contraseña != hashActual)
                {
                    return false;
                }

                usuario.Contraseña = HashContraseña(contraseñaNueva);
                GuardarEnJson();
            }

            _logger.LogInformation("Contraseña cambiada para usuario: {UsuarioID}", usuarioID);
            return true;
        }

        public async Task<bool> AsignarRolAsync(int usuarioID, int rolID)
        {
            lock (_lock)
            {
                var usuario = _usuarios.FirstOrDefault(u => u.UsuarioID == usuarioID);
                if (usuario == null)
                {
                    return false;
                }

                // Verificar si el rol ya está asignado
                if (usuario.Roles.Any(r => r.RolID == rolID))
                {
                    return true; // El rol ya está asignado
                }

                // Asignar el rol
                usuario.Roles.Add(new UsuarioRol
                {
                    UsuarioID = usuarioID,
                    RolID = rolID
                });

                GuardarEnJson();
            }

            _logger.LogInformation("Rol {RolID} asignado a usuario {UsuarioID}", rolID, usuarioID);
            return true;
        }

        public async Task<bool> QuitarRolAsync(int usuarioID, int rolID)
        {
            lock (_lock)
            {
                var usuario = _usuarios.FirstOrDefault(u => u.UsuarioID == usuarioID);
                if (usuario == null)
                {
                    return false;
                }

                var rolUsuario = usuario.Roles.FirstOrDefault(r => r.RolID == rolID);
                if (rolUsuario == null)
                {
                    return true; // El rol no estaba asignado
                }

                usuario.Roles.Remove(rolUsuario);
                GuardarEnJson();
            }

            _logger.LogInformation("Rol {RolID} quitado del usuario {UsuarioID}", rolID, usuarioID);
            return true;
        }

        public async Task<bool> AutenticarAsync(string nombreUsuario, string contraseña)
        {
            var usuario = await GetUsuarioByNombreUsuarioAsync(nombreUsuario);
            if (usuario == null || !usuario.Activo)
            {
                return false;
            }

            string hash = HashContraseña(contraseña);
            bool result = hash == usuario.Contraseña;

            if (result)
            {
                // Actualizar último acceso
                lock (_lock)
                {
                    usuario.UltimoAcceso = DateTime.Now;
                    GuardarEnJson();
                }
                _logger.LogInformation("Autenticación exitosa para usuario: {NombreUsuario}", nombreUsuario);
            }
            else
            {
                _logger.LogWarning("Intento de autenticación fallido para usuario: {NombreUsuario}", nombreUsuario);
            }

            return result;
        }

        public async Task<IEnumerable<Permiso>> GetPermisosUsuarioAsync(int usuarioID)
        {
            var usuario = await GetUsuarioByIDAsync(usuarioID);
            if (usuario == null)
            {
                return Enumerable.Empty<Permiso>();
            }

            // Obtener todos los permisos de todos los roles del usuario
            var permisos = new List<Permiso>();

            foreach (var rol in usuario.Roles)
            {
                var permisosRol = await _rolService.GetPermisosByRolIDAsync(rol.RolID);
                permisos.AddRange(permisosRol);
            }

            // Eliminar duplicados
            return permisos.GroupBy(p => p.PermisoID).Select(g => g.First()).ToList();
        }

        public async Task<bool> TienePermisoAsync(int usuarioID, string codigoPermiso)
        {
            var permisos = await GetPermisosUsuarioAsync(usuarioID);
            return permisos.Any(p => p.Codigo == codigoPermiso);
        }

        public async Task<bool> ToggleEstadoAsync(int id)
        {
            lock (_lock)
            {
                var usuario = _usuarios.FirstOrDefault(u => u.UsuarioID == id);
                if (usuario == null)
                {
                    return false;
                }

                usuario.Activo = !usuario.Activo;
                GuardarEnJson();
            }

            _logger.LogInformation("Estado de usuario {UsuarioID} cambiado a {Estado}", id, _usuarios.FirstOrDefault(u => u.UsuarioID == id)?.Activo);
            return true;
        }

        public async Task<IEnumerable<Usuario>> BuscarUsuariosAsync(string termino)
        {
            if (string.IsNullOrWhiteSpace(termino))
            {
                return await GetAllUsuariosAsync();
            }

            lock (_lock)
            {
                return _usuarios.Where(u =>
                    u.NombreUsuario.Contains(termino, StringComparison.OrdinalIgnoreCase) ||
                    u.Nombre.Contains(termino, StringComparison.OrdinalIgnoreCase) ||
                    u.Apellido.Contains(termino, StringComparison.OrdinalIgnoreCase) ||
                    u.Email.Contains(termino, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
        }
    }
}