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
                List<Usuario> usuarios;
                lock (_lock)
                {
                    usuarios = _usuarios.ToList();
                }
                JsonFileHelper.SaveToJsonFile(_jsonFilePath, usuarios);
                _logger.LogInformation("UsuarioService: {Count} usuarios guardados", usuarios.Count);
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
            try
            {
                if (string.IsNullOrWhiteSpace(contraseña))
                {
                    _logger.LogError("Error al crear usuario: La contraseña no puede estar vacía");
                    throw new ArgumentException("La contraseña no puede estar vacía");
                }

                // Verificar si el usuario ya existe
                var usuarioExistente = await GetUsuarioByNombreUsuarioAsync(usuario.NombreUsuario);
                if (usuarioExistente != null)
                {
                    _logger.LogError("Error al crear usuario: El nombre de usuario ya existe");
                    throw new InvalidOperationException("El nombre de usuario ya existe");
                }

                // Verificar si el email ya existe
                var emailExistente = await GetUsuarioByEmailAsync(usuario.Email);
                if (emailExistente != null)
                {
                    _logger.LogError("Error al crear usuario: El email ya está en uso");
                    throw new InvalidOperationException("El email ya está en uso");
                }

                _logger.LogInformation("Creando usuario en base de datos: {NombreUsuario}", usuario.NombreUsuario);

                lock (_lock)
                {
                    usuario.UsuarioID = _nextUsuarioID++;
                    usuario.Contraseña = HashContraseña(contraseña);
                    usuario.FechaCreacion = DateTime.Now;

                    // Asegurar que todos los campos requeridos tengan valor
                    usuario.NombreUsuario = usuario.NombreUsuario ?? throw new ArgumentException("El nombre de usuario es obligatorio");
                    usuario.Nombre = usuario.Nombre ?? string.Empty;
                    usuario.Apellido = usuario.Apellido ?? string.Empty;
                    usuario.Email = usuario.Email ?? throw new ArgumentException("El email es obligatorio");

                    // Inicializar colecciones si son nulas
                    usuario.Roles = usuario.Roles ?? new List<UsuarioRol>();

                    _usuarios.Add(usuario);
                }

                GuardarEnJson();

                _logger.LogInformation("Usuario creado exitosamente: {NombreUsuario} (ID: {UsuarioID})", usuario.NombreUsuario, usuario.UsuarioID);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico al crear usuario: {NombreUsuario}", usuario?.NombreUsuario);
                throw; // Re-lanzar la excepción para que se pueda manejar en el controlador
            }
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

                // Prevenir cambio de nombre de usuario si ya existe otro con ese nombre
                if (!string.Equals(existing.NombreUsuario, usuario.NombreUsuario, StringComparison.OrdinalIgnoreCase))
                {
                    var usuarioConMismoNombre = _usuarios.FirstOrDefault(u =>
                        u.UsuarioID != usuario.UsuarioID &&
                        u.NombreUsuario.Equals(usuario.NombreUsuario, StringComparison.OrdinalIgnoreCase));

                    if (usuarioConMismoNombre != null)
                    {
                        throw new InvalidOperationException("El nombre de usuario ya está en uso por otro usuario");
                    }
                }

                // Prevenir cambio de email si ya existe otro con ese email
                if (!string.Equals(existing.Email, usuario.Email, StringComparison.OrdinalIgnoreCase))
                {
                    var usuarioConMismoEmail = _usuarios.FirstOrDefault(u =>
                        u.UsuarioID != usuario.UsuarioID &&
                        u.Email.Equals(usuario.Email, StringComparison.OrdinalIgnoreCase));

                    if (usuarioConMismoEmail != null)
                    {
                        throw new InvalidOperationException("El email ya está en uso por otro usuario");
                    }
                }

                // Actualizar propiedades pero preservar contraseña si no se proporciona una nueva
                existing.NombreUsuario = usuario.NombreUsuario;
                existing.Nombre = usuario.Nombre;
                existing.Apellido = usuario.Apellido;
                existing.Email = usuario.Email;
                existing.Activo = usuario.Activo;

                // Actualizar contraseña solo si se proporciona una nueva
                if (!string.IsNullOrWhiteSpace(contraseñaNueva))
                {
                    existing.Contraseña = HashContraseña(contraseñaNueva);
                }
            }

            GuardarEnJson();

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
            }

            GuardarEnJson();

            _logger.LogInformation("Usuario eliminado: {UsuarioID}", id);
            return true;
        }

        public async Task<bool> CambiarContraseñaAsync(int usuarioID, string contraseñaActual, string contraseñaNueva)
        {
            if (string.IsNullOrWhiteSpace(contraseñaNueva))
            {
                throw new ArgumentException("La nueva contraseña no puede estar vacía");
            }

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

                // Verificar que la nueva contraseña sea diferente a la actual
                string hashNueva = HashContraseña(contraseñaNueva);
                if (hashNueva == hashActual)
                {
                    throw new InvalidOperationException("La nueva contraseña debe ser diferente a la actual");
                }

                usuario.Contraseña = hashNueva;
            }

            GuardarEnJson();

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

                // Inicializar la colección de roles si es nula
                usuario.Roles ??= new List<UsuarioRol>();

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
            }

            GuardarEnJson();

            _logger.LogInformation("Rol {RolID} asignado a usuario {UsuarioID}", rolID, usuarioID);
            return true;
        }

        public async Task<bool> QuitarRolAsync(int usuarioID, int rolID)
        {
            lock (_lock)
            {
                var usuario = _usuarios.FirstOrDefault(u => u.UsuarioID == usuarioID);
                if (usuario == null || usuario.Roles == null)
                {
                    return false;
                }

                var rolUsuario = usuario.Roles.FirstOrDefault(r => r.RolID == rolID);
                if (rolUsuario == null)
                {
                    return true; // El rol no estaba asignado
                }

                usuario.Roles.Remove(rolUsuario);
            }

            GuardarEnJson();

            _logger.LogInformation("Rol {RolID} quitado del usuario {UsuarioID}", rolID, usuarioID);
            return true;
        }

        public async Task<bool> AutenticarAsync(string nombreUsuario, string contraseña)
        {
            var usuario = await GetUsuarioByNombreUsuarioAsync(nombreUsuario);
            if (usuario == null || !usuario.Activo)
            {
                _logger.LogWarning("Intento de autenticación fallido para usuario inexistente o inactivo: {NombreUsuario}", nombreUsuario);
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
                }
                GuardarEnJson();
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
            if (usuario == null || usuario.Roles == null)
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
            return permisos.Any(p => p.Codigo == codigoPermiso && p.Activo);
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
            }

            GuardarEnJson();

            _logger.LogInformation("Estado de usuario {UsuarioID} cambiado a {Estado}",
                id, _usuarios.FirstOrDefault(u => u.UsuarioID == id)?.Activo);
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

        public async Task<IEnumerable<Usuario>> GetUsuariosByRolIDAsync(int rolID)
        {
            lock (_lock)
            {
                return _usuarios.Where(u => u.Roles != null && u.Roles.Any(r => r.RolID == rolID)).ToList();
            }
        }

        public async Task<bool> ResetearContrasenaAsync(int usuarioID, string nuevaContraseña)
        {
            if (string.IsNullOrWhiteSpace(nuevaContraseña))
            {
                throw new ArgumentException("La nueva contraseña no puede estar vacía");
            }

            lock (_lock)
            {
                var usuario = _usuarios.FirstOrDefault(u => u.UsuarioID == usuarioID);
                if (usuario == null)
                {
                    return false;
                }

                usuario.Contraseña = HashContraseña(nuevaContraseña);
            }

            GuardarEnJson();

            _logger.LogInformation("Contraseña reseteada para usuario: {UsuarioID}", usuarioID);
            return true;
        }

        public async Task<bool> IsUsernameUniqueAsync(string nombreUsuario)
        {
            var usuario = await GetUsuarioByNombreUsuarioAsync(nombreUsuario);
            return usuario == null;
        }

        public async Task<bool> IsEmailUniqueAsync(string email)
        {
            var usuario = await GetUsuarioByEmailAsync(email);
            return usuario == null;
        }

        public async Task<IEnumerable<Usuario>> GetUsuariosActivosAsync()
        {
            lock (_lock)
            {
                return _usuarios.Where(u => u.Activo).ToList();
            }
        }

        public async Task<IEnumerable<Usuario>> GetUsuariosRecentesAsync(int count)
        {
            lock (_lock)
            {
                return _usuarios.OrderByDescending(u => u.FechaCreacion).Take(count).ToList();
            }
        }

        public async Task<IEnumerable<Usuario>> GetUltimosAccesosAsync(int count)
        {
            lock (_lock)
            {
                return _usuarios.Where(u => u.UltimoAcceso.HasValue)
                                .OrderByDescending(u => u.UltimoAcceso)
                                .Take(count)
                                .ToList();
            }
        }

        public async Task<bool> ValidarContraseñaSeguraAsync(string contraseña)
        {
            if (string.IsNullOrWhiteSpace(contraseña))
                return false;

            // Longitud mínima
            if (contraseña.Length < 6)
                return false;

            // Debe contener letras y números
            bool tieneLetras = contraseña.Any(char.IsLetter);
            bool tieneNumeros = contraseña.Any(char.IsDigit);
            if (!tieneLetras || !tieneNumeros)
                return false;

            // Debe contener al menos un carácter especial
            bool tieneCaracterEspecial = contraseña.Any(c => !char.IsLetterOrDigit(c));
            if (!tieneCaracterEspecial)
                return false;

            return true;
        }
    }
}