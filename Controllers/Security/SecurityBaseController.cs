// Controllers/Security/SecurityBaseController.cs
using Javo2.Controllers.Base;
using Javo2.IServices.Authentication;
using Javo2.Models.Authentication;
using Javo2.ViewModels.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Javo2.Controllers.Security
{
    public abstract class SecurityBaseController : BaseController
    {
        protected readonly IUsuarioService _usuarioService;
        protected readonly IRolService _rolService;
        protected readonly IPermisoService _permisoService;
        protected readonly IPermissionManagerService _permissionManager;

        public SecurityBaseController(
            IUsuarioService usuarioService,
            IRolService rolService,
            IPermisoService permisoService,
            IPermissionManagerService permissionManager,
            ILogger logger) : base(logger)
        {
            _usuarioService = usuarioService;
            _rolService = rolService;
            _permisoService = permisoService;
            _permissionManager = permissionManager;
        }

        #region Métodos comunes de verificación de permisos

        protected async Task<SecurityVerificationResult> VerificarPermisosUsuarioAsync(string username = null)
        {
            try
            {
                username ??= User.Identity?.Name;

                if (string.IsNullOrEmpty(username))
                {
                    return new SecurityVerificationResult
                    {
                        Success = false,
                        Message = "Usuario no autenticado"
                    };
                }

                var usuario = await _usuarioService.GetUsuarioByNombreUsuarioAsync(username);
                if (usuario == null)
                {
                    return new SecurityVerificationResult
                    {
                        Success = false,
                        Message = "Usuario no encontrado"
                    };
                }

                var roles = new List<string>();
                foreach (var usuarioRol in usuario.Roles)
                {
                    var rol = await _rolService.GetRolByIDAsync(usuarioRol.RolID);
                    if (rol != null)
                    {
                        roles.Add(rol.Nombre);
                    }
                }

                var permisos = await _usuarioService.GetPermisosUsuarioAsync(usuario.UsuarioID);
                var permisosLista = permisos.Select(p => p.Codigo).ToList();

                var tienePermisoDashboard = permisosLista.Contains("securitydashboard.ver");

                return new SecurityVerificationResult
                {
                    Success = true,
                    Usuario = new UsuarioSecurityInfo
                    {
                        Username = usuario.NombreUsuario,
                        Nombre = $"{usuario.Nombre} {usuario.Apellido}",
                        Roles = roles,
                        TieneRolAdmin = roles.Any(r => r.Equals("Administrador", StringComparison.OrdinalIgnoreCase)),
                        Permisos = permisosLista,
                        TienePermisoDashboard = tienePermisoDashboard,
                        UsuarioID = usuario.UsuarioID
                    }
                };
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al verificar permisos de usuario");
                return new SecurityVerificationResult
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        #endregion

        #region Métodos comunes para gestión de roles

        protected async Task<RolFormViewModel> PrepararFormularioRolAsync(Rol rol = null, bool esEdicion = false)
        {
            try
            {
                var allPermisos = await _permisoService.GetAllPermisosAsync();
                var permisosActivos = allPermisos.Where(p => p.Activo).ToList();

                // Agrupar permisos por grupo
                var gruposPermisos = permisosActivos
                    .GroupBy(p => p.Grupo ?? "General")
                    .ToDictionary(g => g.Key, g => g.ToList());

                var permisosSeleccionados = new List<int>();
                if (rol != null && esEdicion)
                {
                    permisosSeleccionados = rol.Permisos.Select(p => p.PermisoID).ToList();
                }

                return new RolFormViewModel
                {
                    Rol = rol ?? new Rol { EsSistema = false },
                    GruposPermisos = gruposPermisos,
                    PermisosSeleccionados = permisosSeleccionados,
                    EsEdicion = esEdicion
                };
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al preparar formulario de rol");
                throw;
            }
        }

        protected async Task<bool> ProcesarAsignacionPermisosRolAsync(int rolId, List<int> permisosSeleccionados)
        {
            try
            {
                LogInfo("Procesando asignación de permisos para rol {RolID}", rolId);
                LogInfo("Permisos a asignar: {Permisos}", string.Join(", ", permisosSeleccionados ?? new List<int>()));

                // Obtener rol actual
                var rol = await _rolService.GetRolByIDAsync(rolId);
                if (rol == null)
                {
                    LogWarning("Rol {RolID} no encontrado", rolId);
                    return false;
                }

                // Obtener permisos actuales
                var permisosActuales = rol.Permisos.Select(p => p.PermisoID).ToList();
                var nuevosPermisos = permisosSeleccionados ?? new List<int>();

                // Calcular diferencias
                var permisosAEliminar = permisosActuales.Except(nuevosPermisos).ToList();
                var permisosAAgregar = nuevosPermisos.Except(permisosActuales).ToList();

                LogInfo("Permisos a eliminar: {PermisosEliminar}", string.Join(", ", permisosAEliminar));
                LogInfo("Permisos a agregar: {PermisosAgregar}", string.Join(", ", permisosAAgregar));

                // Eliminar permisos
                foreach (var permisoID in permisosAEliminar)
                {
                    await _rolService.QuitarPermisoAsync(rolId, permisoID);
                    LogInfo("Permiso {PermisoID} eliminado del rol {RolID}", permisoID, rolId);
                }

                // Agregar permisos
                foreach (var permisoID in permisosAAgregar)
                {
                    await _rolService.AsignarPermisoAsync(rolId, permisoID);
                    LogInfo("Permiso {PermisoID} asignado al rol {RolID}", permisoID, rolId);
                }

                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al procesar asignación de permisos para rol {RolID}", rolId);
                return false;
            }
        }

        protected async Task<bool> ValidarRolParaEliminacionAsync(int rolId)
        {
            try
            {
                var rol = await _rolService.GetRolByIDAsync(rolId);
                if (rol == null)
                {
                    SetErrorMessage("Rol no encontrado");
                    return false;
                }

                if (rol.EsSistema)
                {
                    SetErrorMessage("No se pueden eliminar roles del sistema");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al validar rol para eliminación");
                SetErrorMessage("Error al validar el rol");
                return false;
            }
        }

        #endregion

        #region Métodos comunes para gestión de usuarios

        protected async Task<UsuarioFormViewModel> PrepararFormularioUsuarioAsync(Usuario usuario = null, bool esEdicion = false)
        {
            try
            {
                var roles = await _rolService.GetAllRolesAsync();
                var rolesItems = roles.Select(r => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = r.RolID.ToString(),
                    Text = r.Nombre
                }).ToList();

                var rolesSeleccionados = new List<int>();
                if (usuario != null && esEdicion)
                {
                    rolesSeleccionados = usuario.Roles.Select(r => r.RolID).ToList();
                }

                return new UsuarioFormViewModel
                {
                    Usuario = usuario ?? new Usuario { Activo = true },
                    RolesDisponibles = rolesItems,
                    RolesSeleccionados = rolesSeleccionados,
                    EsEdicion = esEdicion
                };
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al preparar formulario de usuario");
                throw;
            }
        }

        protected async Task<bool> ProcesarAsignacionRolesUsuarioAsync(int usuarioId, List<int> rolesSeleccionados)
        {
            try
            {
                LogInfo("Procesando asignación de roles para usuario {UsuarioID}", usuarioId);

                var usuario = await _usuarioService.GetUsuarioByIDAsync(usuarioId);
                if (usuario == null)
                {
                    LogWarning("Usuario {UsuarioID} no encontrado", usuarioId);
                    return false;
                }

                // Obtener roles actuales
                var rolesActuales = usuario.Roles.Select(r => r.RolID).ToList();
                var nuevosRoles = rolesSeleccionados ?? new List<int>();

                // Calcular diferencias
                var rolesAEliminar = rolesActuales.Except(nuevosRoles).ToList();
                var rolesAAgregar = nuevosRoles.Except(rolesActuales).ToList();

                // Eliminar roles
                foreach (var rolId in rolesAEliminar)
                {
                    await _usuarioService.QuitarRolAsync(usuarioId, rolId);
                    LogInfo("Rol {RolID} eliminado del usuario {UsuarioID}", rolId, usuarioId);
                }

                // Agregar roles
                foreach (var rolId in rolesAAgregar)
                {
                    await _usuarioService.AsignarRolAsync(usuarioId, rolId);
                    LogInfo("Rol {RolID} asignado al usuario {UsuarioID}", rolId, usuarioId);
                }

                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al procesar asignación de roles para usuario {UsuarioID}", usuarioId);
                return false;
            }
        }

        protected async Task<bool> ValidarDatosUsuarioAsync(UsuarioFormViewModel model, bool esEdicion = false)
        {
            bool isValid = true;

            if (model.Usuario == null)
            {
                ModelState.AddModelError("Usuario", "La información del usuario es obligatoria");
                isValid = false;
            }
            else
            {
                if (string.IsNullOrEmpty(model.Usuario.NombreUsuario))
                {
                    ModelState.AddModelError("Usuario.NombreUsuario", "El nombre de usuario es obligatorio");
                    isValid = false;
                }
                if (string.IsNullOrEmpty(model.Usuario.Email))
                {
                    ModelState.AddModelError("Usuario.Email", "El email es obligatorio");
                    isValid = false;
                }
            }

            // Validación de contraseña solo para creación o cuando se proporciona
            if (!esEdicion)
            {
                if (string.IsNullOrEmpty(model.Contraseña))
                {
                    ModelState.AddModelError("Contraseña", "La contraseña es obligatoria");
                    isValid = false;
                }
                else if (model.Contraseña.Length < 6)
                {
                    ModelState.AddModelError("Contraseña", "La contraseña debe tener al menos 6 caracteres");
                    isValid = false;
                }

                if (string.IsNullOrEmpty(model.ConfirmarContraseña))
                {
                    ModelState.AddModelError("ConfirmarContraseña", "La confirmación de contraseña es obligatoria");
                    isValid = false;
                }
                else if (model.Contraseña != model.ConfirmarContraseña)
                {
                    ModelState.AddModelError("ConfirmarContraseña", "Las contraseñas no coinciden");
                    isValid = false;
                }
            }
            else if (!string.IsNullOrEmpty(model.Contraseña))
            {
                // En edición, validar solo si se proporciona nueva contraseña
                if (model.Contraseña.Length < 6)
                {
                    ModelState.AddModelError("Contraseña", "La contraseña debe tener al menos 6 caracteres");
                    isValid = false;
                }

                if (model.Contraseña != model.ConfirmarContraseña)
                {
                    ModelState.AddModelError("ConfirmarContraseña", "Las contraseñas no coinciden");
                    isValid = false;
                }
            }

            return isValid;
        }

        #endregion

        #region Métodos comunes para gestión de permisos

        protected async Task<bool> ValidarPermisoParaEliminacionAsync(int permisoId)
        {
            try
            {
                var permisos = await _permisoService.GetAllPermisosAsync();
                var permiso = permisos.FirstOrDefault(p => p.PermisoID == permisoId);

                if (permiso == null)
                {
                    SetErrorMessage("Permiso no encontrado");
                    return false;
                }

                if (permiso.EsSistema)
                {
                    SetErrorMessage("No se pueden eliminar permisos del sistema");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al validar permiso para eliminación");
                SetErrorMessage("Error al validar el permiso");
                return false;
            }
        }

        protected async Task<bool> ValidarCodigoPermisoUnicoAsync(string codigo, int? permisoIdExcluir = null)
        {
            try
            {
                var permisos = await _permisoService.GetAllPermisosAsync();
                var existente = permisos.FirstOrDefault(p => p.Codigo == codigo &&
                    (permisoIdExcluir == null || p.PermisoID != permisoIdExcluir));

                if (existente != null)
                {
                    ModelState.AddModelError("Codigo", "Ya existe un permiso con este código");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al validar código de permiso único");
                return false;
            }
        }

        #endregion

        #region Métodos de reparación y mantenimiento

        protected async Task<SecurityRepairResult> RepararPermisosAdministradorAsync()
        {
            try
            {
                var result = new SecurityRepairResult();

                // 1. Encontrar el rol Administrador
                var roles = await _rolService.GetAllRolesAsync();
                var rolAdmin = roles.FirstOrDefault(r => r.Nombre.Equals("Administrador", StringComparison.OrdinalIgnoreCase));

                if (rolAdmin == null)
                {
                    result.Messages.Add("Error: No se encontró el rol Administrador");
                    result.Success = false;
                    return result;
                }

                result.Messages.Add($"Rol Administrador encontrado con ID: {rolAdmin.RolID}");

                // 2. Obtener todos los permisos disponibles
                var permisos = await _permisoService.GetAllPermisosAsync();
                result.Messages.Add($"Encontrados {permisos.Count()} permisos en total");

                // 3. Obtener permisos actuales del administrador
                var permisosAdmin = rolAdmin.Permisos?.Select(p => p.PermisoID).ToList() ?? new List<int>();
                result.Messages.Add($"El administrador tiene actualmente {permisosAdmin.Count} permisos asignados");

                // 4. Encontrar permisos faltantes
                var permisosFaltantes = permisos
                    .Where(p => !permisosAdmin.Contains(p.PermisoID))
                    .ToList();

                result.Messages.Add($"Permisos faltantes: {permisosFaltantes.Count}");

                // 5. Asignar todos los permisos faltantes
                if (permisosFaltantes.Any())
                {
                    foreach (var permiso in permisosFaltantes)
                    {
                        await _rolService.AsignarPermisoAsync(rolAdmin.RolID, permiso.PermisoID);
                    }

                    result.Messages.Add($"Se han asignado {permisosFaltantes.Count} permisos faltantes");
                }

                result.Messages.Add("IMPORTANTE: Es necesario cerrar sesión y volver a iniciar para aplicar los cambios de permisos");
                result.Success = true;
                result.PermisosReparados = permisosFaltantes.Count;

                return result;
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al reparar permisos de administrador");
                return new SecurityRepairResult
                {
                    Success = false,
                    Messages = new List<string> { $"Error: {ex.Message}" }
                };
            }
        }

        #endregion

        #region Métodos auxiliares

        protected UsuarioDetailsViewModel CrearUsuarioDetailsViewModel(Usuario usuario, List<Rol> roles)
        {
            return new UsuarioDetailsViewModel
            {
                Usuario = usuario,
                Roles = roles
            };
        }

        protected RolDetailsViewModel CrearRolDetailsViewModel(Rol rol, List<Permiso> permisos)
        {
            return new RolDetailsViewModel
            {
                Rol = rol,
                Permisos = permisos
            };
        }

        #endregion
    }

    #region Clases auxiliares

    public class SecurityVerificationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public UsuarioSecurityInfo Usuario { get; set; }
    }

    public class UsuarioSecurityInfo
    {
        public int UsuarioID { get; set; }
        public string Username { get; set; }
        public string Nombre { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public bool TieneRolAdmin { get; set; }
        public List<string> Permisos { get; set; } = new List<string>();
        public bool TienePermisoDashboard { get; set; }
    }

    public class SecurityRepairResult
    {
        public bool Success { get; set; }
        public List<string> Messages { get; set; } = new List<string>();
        public int PermisosReparados { get; set; }
    }

    #endregion
}