// Controllers/Authentication/AuthController.cs
using Javo2.Controllers.Base;
using Javo2.IServices.Authentication;
using Javo2.Models.Authentication;
using Javo2.ViewModels.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Javo2.Controllers.Authentication
{
    public class AuthController : BaseController
    {
        private readonly IUsuarioService _usuarioService;
        private readonly IRolService _rolService;

        public AuthController(
            IUsuarioService usuarioService,
            IRolService rolService,
            ILogger<AuthController> logger)
            : base(logger)
        {
            _usuarioService = usuarioService;
            _rolService = rolService;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                bool isAuthenticated = await _usuarioService.AutenticarAsync(model.NombreUsuario, model.Contraseña);

                if (!isAuthenticated)
                {
                    ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos");
                    return View(model);
                }

                var usuario = await _usuarioService.GetUsuarioByNombreUsuarioAsync(model.NombreUsuario);

                if (usuario == null)
                {
                    ModelState.AddModelError(string.Empty, "Error al obtener información del usuario");
                    return View(model);
                }

                if (!usuario.Activo)
                {
                    ModelState.AddModelError(string.Empty, "Su cuenta está desactivada. Contacte al administrador.");
                    return View(model);
                }

                // Obtener permisos del usuario
                var permisos = await _usuarioService.GetPermisosUsuarioAsync(usuario.UsuarioID);

                // Crear claims para el usuario
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
                    claims.Add(new Claim("Permission", permiso.Codigo));
                }

                var claimsIdentity = new ClaimsIdentity(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = model.RecordarMe,
                    ExpiresUtc = DateTime.UtcNow.AddHours(3) // La sesión expira después de 3 horas
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                _logger.LogInformation("Usuario {NombreUsuario} ha iniciado sesión", usuario.NombreUsuario);

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
                ModelState.AddModelError(string.Empty, "Ocurrió un error al iniciar sesión");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public IActionResult CambiarContraseña()
        {
            return View(new SecurityDashboardViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarContraseña(SecurityDashboardViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Obtener ID del usuario actual
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int usuarioID))
                {
                    ModelState.AddModelError(string.Empty, "No se pudo identificar al usuario");
                    return View(model);
                }

                var result = await _usuarioService.CambiarContraseñaAsync(
                    usuarioID, model.ContraseñaActual, model.ContraseñaNueva);

                if (!result)
                {
                    ModelState.AddModelError(string.Empty, "La contraseña actual es incorrecta");
                    return View(model);
                }

                TempData["Success"] = "Contraseña cambiada correctamente";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar contraseña");
                ModelState.AddModelError(string.Empty, "Ocurrió un error al cambiar la contraseña");
                return View(model);
            }
        }
    }
}