// 4. Controllers/Auth/LoginController.cs
// Controlador para autenticación más completo
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Office2013.Drawing.ChartStyle;
using DocumentFormat.OpenXml.Wordprocessing;
using Javo2.IServices.Authentication;
using Javo2.Models.Authentication;
using Javo2.ViewModels.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Javo2.Controllers.Auth
{
    public class LoginController : Controller
    {
        private readonly IUsuarioService _usuarioService;
        private readonly IRolService _rolService;
        private readonly ILogger<LoginController> _logger;

        public LoginController(
            IUsuarioService usuarioService,
            IRolService rolService,
            ILogger<LoginController> logger)
        {
            _usuarioService = usuarioService;
            _rolService = rolService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index(string returnUrl = null)
        {
            // Si ya está autenticado, redirigir a home
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LoginViewModel model, string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var isValid = await _usuarioService.AutenticarAsync(model.NombreUsuario, model.Contraseña);
                if (!isValid)
                {
                    ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos");
                    return View(model);
                }

                var usuario = await _usuarioService.GetUsuarioByNombreUsuarioAsync(model.NombreUsuario);
                if (usuario == null)
                {
                    ModelState.AddModelError(string.Empty, "Usuario no encontrado");
                    return View(model);
                }

                if (!usuario.Activo)
                {
                    ModelState.AddModelError(string.Empty, "Su cuenta está desactivada. Contacte al administrador");
                    return View(model);
                }

                // Obtener permisos del usuario
                var permisos = await _usuarioService.GetPermisosUsuarioAsync(usuario.UsuarioID);

                // Crear claims para la identidad
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.UsuarioID.ToString()),
                    new Claim(ClaimTypes.Name, usuario.NombreUsuario),
                    new Claim(ClaimTypes.GivenName, usuario.Nombre),
                    new Claim(ClaimTypes.Surname, usuario.Apellido),
                    new Claim(ClaimTypes.Email, usuario.Email)
                };

                // Agregar roles como claims
                foreach (var usuarioRol in usuario.Roles)
                {
                    var rol = await _rolService.GetRolByIDAsync(usuarioRol.RolID);
                    if (rol != null)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, rol.Nombre));
                    }
                }

                // Agregar permisos como claims
                foreach (var permiso in permisos)
                {
                    if (permiso.Activo)
                    {
                        claims.Add(new Claim("Permission", permiso.Codigo));
                    }
                }

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = model.RecordarMe,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(3)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                _logger.LogInformation("Usuario {NombreUsuario} ha iniciado sesión", usuario.NombreUsuario);

                // Actualizar último acceso
                usuario.UltimoAcceso = DateTime.Now;
                await _usuarioService.UpdateUsuarioAsync(usuario);

                // Redirigir a la URL solicitada o a la página principal
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el proceso de login");
                ModelState.AddModelError(string.Empty, "Ocurrió un error al iniciar sesión. Por favor, inténtelo de nuevo.");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Login");
        }
    }
}


// 6. IServices/Authentication/IPermisoService.cs
// Interfaz más completa para el servicio de permisos
using Javo2.Models.Authentication;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Javo2.IServices.Authentication
{
    public interface IPermisoService
    {
        Task<IEnumerable<Permiso>> GetAllPermisosAsync();
        Task<Permiso> GetPermisoByIDAsync(int id);
        Task<Permiso> GetPermisoByCodigo(string codigo);
        Task<bool> CreatePermisoAsync(Permiso permiso);
        Task<bool> UpdatePermisoAsync(Permiso permiso);
        Task<bool> DeletePermisoAsync(int id);
        Task<IEnumerable<Permiso>> GetPermisosByModuloAsync(string modulo);
        Task<bool> ToggleEstadoAsync(int id);
    }
}

// 7. IServices/Authentication/IRolService.cs
// Interfaz más completa para el servicio de roles
using Javo2.Models.Authentication;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Javo2.IServices.Authentication
{
    public interface IRolService
    {
        Task<IEnumerable<Rol>> GetAllRolesAsync();
        Task<Rol> GetRolByIDAsync(int id);
        Task<Rol> GetRolByNombreAsync(string nombre);
        Task<int> CreateRolAsync(Rol rol);
        Task<bool> UpdateRolAsync(Rol rol);
        Task<bool> DeleteRolAsync(int id);
        Task<IEnumerable<Permiso>> GetPermisosByRolIDAsync(int rolID);
        Task<bool> AsignarPermisoAsync(int rolID, int permisoID);
        Task<bool> QuitarPermisoAsync(int rolID, int permisoID);
        Task<bool> TienePermisoAsync(int rolID, string codigoPermiso);
    }
}

// 8. IServices/Authentication/IUsuarioService.cs
// Interfaz más completa para el servicio de usuarios
using Javo2.Models.Authentication;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Javo2.IServices.Authentication
{
    public interface IUsuarioService
    {
        Task<IEnumerable<Usuario>> GetAllUsuariosAsync();
        Task<Usuario> GetUsuarioByIDAsync(int id);
        Task<Usuario> GetUsuarioByNombreUsuarioAsync(string nombreUsuario);
        Task<Usuario> GetUsuarioByEmailAsync(string email);
        Task<bool> CreateUsuarioAsync(Usuario usuario, string contraseña);
        Task<bool> UpdateUsuarioAsync(Usuario usuario, string contraseñaNueva = null);
        Task<bool> DeleteUsuarioAsync(int id);
        Task<bool> CambiarContraseñaAsync(int usuarioID, string contraseñaActual, string contraseñaNueva);
        Task<bool> AsignarRolAsync(int usuarioID, int rolID);
        Task<bool> QuitarRolAsync(int usuarioID, int rolID);
        Task<bool> AutenticarAsync(string nombreUsuario, string contraseña);
        Task<IEnumerable<Permiso>> GetPermisosUsuarioAsync(int usuarioID);
        Task<bool> TienePermisoAsync(int usuarioID, string codigoPermiso);
        Task<bool> ToggleEstadoAsync(int id);
        Task<IEnumerable<Usuario>> BuscarUsuariosAsync(string termino);
    }
}
