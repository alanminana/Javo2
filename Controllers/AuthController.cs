using Javo2.Controllers.Base;
using Javo2.IServices.Authentication;
using Javo2.IServices;
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

namespace Javo2.Controllers
{
    public class AuthController : BaseController
    {
        private readonly IUsuarioService _usuarioService;
        private readonly IRolService _rolService;
        private readonly IResetPasswordService _resetPasswordService;
        private readonly IEmailService _emailService;

        public AuthController(
            IUsuarioService usuarioService,
            IRolService rolService,
            IResetPasswordService resetPasswordService,
            IEmailService emailService,
            ILogger<AuthController> logger)
            : base(logger)
        {
            _usuarioService = usuarioService;
            _rolService = rolService;
            _resetPasswordService = resetPasswordService;
            _emailService = emailService;
        }

        #region Login/Logout

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
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
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
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
            return RedirectToAction("Login");
        }

        #endregion

        #region Password Management

        [HttpGet]
        public IActionResult CambiarContraseña()
        {
            return View(new CambiarContraseñaViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarContraseña(CambiarContraseñaViewModel model)
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

        [HttpGet]
        public IActionResult OlvideContraseña()
        {
            return View(new OlvideContraseñaViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OlvideContraseña(OlvideContraseñaViewModel model)
        {
            try
            {
                // Verificar si se proporcionó al menos un campo
                if (string.IsNullOrWhiteSpace(model.NombreUsuario) && string.IsNullOrWhiteSpace(model.Email))
                {
                    ModelState.AddModelError(string.Empty, "Debe proporcionar un nombre de usuario o email");
                    return View(model);
                }

                // Buscar usuario por nombre de usuario o email
                var usuario = !string.IsNullOrWhiteSpace(model.NombreUsuario)
                    ? await _usuarioService.GetUsuarioByNombreUsuarioAsync(model.NombreUsuario)
                    : await _usuarioService.GetUsuarioByEmailAsync(model.Email);

                // Por razones de seguridad, no revelamos si el usuario existe o no
                ViewBag.ConfirmacionEnviada = true;

                // Si el usuario existe y está activo, enviar email de recuperación
                if (usuario != null && usuario.Activo)
                {
                    // Generar token de restablecimiento
                    await _resetPasswordService.GenerateResetTokenAsync(usuario.UsuarioID);
                }

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar solicitud de olvidé contraseña");
                ModelState.AddModelError(string.Empty, "Ocurrió un error al procesar la solicitud");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ResetearContraseña(int userId, string token)
        {
            try
            {
                // Validar token
                bool tokenValido = await _resetPasswordService.ValidateResetTokenAsync(userId, token);
                if (!tokenValido)
                {
                    return View("TokenInvalido");
                }

                var model = new ResetearContraseñaViewModel
                {
                    UsuarioID = userId,
                    Token = token
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar token de restablecimiento");
                return View("TokenInvalido");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetearContraseña(ResetearContraseñaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Resetear contraseña
                bool result = await _resetPasswordService.ResetPasswordAsync(
                    model.UsuarioID, model.Token, model.NuevaContraseña);

                if (!result)
                {
                    return View("TokenInvalido");
                }

                return View("ResetExitoso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al restablecer contraseña");
                ModelState.AddModelError(string.Empty, "Ocurrió un error al restablecer la contraseña");
                return View(model);
            }
        }

        #endregion

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}