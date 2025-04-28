// Controllers/ResetPasswordController.cs
using Javo2.Controllers.Base;
using Javo2.IServices;
using Javo2.IServices.Authentication;
using Javo2.ViewModels.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Javo2.Controllers
{
    public class ResetPasswordController : BaseController
    {
        private readonly IUsuarioService _usuarioService;
        private readonly IResetPasswordService _resetPasswordService;
        private readonly IEmailService _emailService;

        public ResetPasswordController(
            IUsuarioService usuarioService,
            IResetPasswordService resetPasswordService,
            IEmailService emailService,
            ILogger<ResetPasswordController> logger)
            : base(logger)
        {
            _usuarioService = usuarioService;
            _resetPasswordService = resetPasswordService;
            _emailService = emailService;
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
    }
}