// Services/Authentication/ResetPasswordService.cs
using Javo2.Helpers;
using Javo2.IServices;
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
    public class ResetPasswordService : IResetPasswordService
    {
        private readonly IUsuarioService _usuarioService;
        private readonly IEmailService _emailService;
        private readonly ILogger<ResetPasswordService> _logger;
        private static List<PasswordResetToken> _tokens = new List<PasswordResetToken>();
        private static readonly object _lock = new object();
        private readonly string _jsonFilePath = "Data/passwordResetTokens.json";

        // Token expiration time in hours
        private const int TOKEN_EXPIRATION_HOURS = 24;

        public ResetPasswordService(
            IUsuarioService usuarioService,
            IEmailService emailService,
            ILogger<ResetPasswordService> logger)
        {
            _usuarioService = usuarioService;
            _emailService = emailService;
            _logger = logger;
            CargarTokensDesdeJson();
        }

        private void CargarTokensDesdeJson()
        {
            try
            {
                var data = JsonFileHelper.LoadFromJsonFile<List<PasswordResetToken>>(_jsonFilePath);
                lock (_lock)
                {
                    _tokens = data ?? new List<PasswordResetToken>();
                }
                _logger.LogInformation("ResetPasswordService: {Count} tokens cargados", _tokens.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar tokens de restablecimiento");
                _tokens = new List<PasswordResetToken>();
            }
        }

        private void GuardarTokensEnJson()
        {
            try
            {
                // Limpiar tokens expirados antes de guardar
                LimpiarTokensExpirados();

                lock (_lock)
                {
                    JsonFileHelper.SaveToJsonFile(_jsonFilePath, _tokens);
                }
                _logger.LogInformation("ResetPasswordService: {Count} tokens guardados", _tokens.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar tokens de restablecimiento");
            }
        }

        private void LimpiarTokensExpirados()
        {
            lock (_lock)
            {
                _tokens = _tokens.Where(t => t.Expiracion > DateTime.Now).ToList();
            }
        }

        private string GenerateRandomToken()
        {
            var randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes);
        }

        public async Task<string> GenerateResetTokenAsync(int usuarioID)
        {
            // Verificar que el usuario existe
            var usuario = await _usuarioService.GetUsuarioByIDAsync(usuarioID);
            if (usuario == null)
            {
                throw new ArgumentException($"Usuario con ID {usuarioID} no encontrado");
            }

            // Invalidar tokens anteriores
            await InvalidateTokensAsync(usuarioID);

            // Generar nuevo token
            var token = GenerateRandomToken();
            var resetToken = new PasswordResetToken
            {
                UsuarioID = usuarioID,
                Token = token,
                Creacion = DateTime.Now,
                Expiracion = DateTime.Now.AddHours(TOKEN_EXPIRATION_HOURS),
                EsValido = true
            };

            lock (_lock)
            {
                _tokens.Add(resetToken);
            }

            GuardarTokensEnJson();

            // Enviar email con el token
            await EnviarEmailRestablecimientoAsync(usuario, token);

            return token;
        }

        public async Task<bool> ValidateResetTokenAsync(int usuarioID, string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            lock (_lock)
            {
                var resetToken = _tokens.FirstOrDefault(t =>
                    t.UsuarioID == usuarioID &&
                    t.Token == token &&
                    t.EsValido &&
                    t.Expiracion > DateTime.Now);

                return resetToken != null;
            }
        }

        public async Task<bool> ResetPasswordAsync(int usuarioID, string token, string nuevaContraseña)
        {
            if (!await ValidateResetTokenAsync(usuarioID, token))
            {
                return false;
            }

            try
            {
                var usuario = await _usuarioService.GetUsuarioByIDAsync(usuarioID);
                if (usuario == null)
                {
                    return false;
                }

                // Cambiar contraseña
                var result = await _usuarioService.UpdateUsuarioAsync(usuario, nuevaContraseña);
                if (!result)
                {
                    return false;
                }

                // Invalidar todos los tokens del usuario
                await InvalidateTokensAsync(usuarioID);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al restablecer contraseña para usuario {UsuarioID}", usuarioID);
                return false;
            }
        }

        public async Task<bool> HasActiveTokenAsync(int usuarioID)
        {
            lock (_lock)
            {
                return _tokens.Any(t =>
                    t.UsuarioID == usuarioID &&
                    t.EsValido &&
                    t.Expiracion > DateTime.Now);
            }
        }

        public async Task InvalidateTokensAsync(int usuarioID)
        {
            lock (_lock)
            {
                foreach (var token in _tokens.Where(t => t.UsuarioID == usuarioID && t.EsValido))
                {
                    token.EsValido = false;
                }
            }

            GuardarTokensEnJson();
        }

        private async Task EnviarEmailRestablecimientoAsync(Usuario usuario, string token)
        {
            try
            {
                if (string.IsNullOrEmpty(usuario.Email))
                {
                    _logger.LogWarning("No se pudo enviar email de restablecimiento a usuario {UsuarioID}: email no disponible", usuario.UsuarioID);
                    return;
                }

                var resetUrl = $"/ResetPassword/ResetearContraseña?userId={usuario.UsuarioID}&token={token}";

                var subject = "Restablecimiento de contraseña - Sistema Javo2";
                var body = $@"
                    <html>
                    <body>
                        <h2>Restablecimiento de contraseña</h2>
                        <p>Estimado/a {usuario.Nombre} {usuario.Apellido},</p>
                        <p>Hemos recibido una solicitud para restablecer su contraseña. Si usted no realizó esta solicitud, ignore este correo.</p>
                        <p>Para restablecer su contraseña, haga clic en el siguiente enlace:</p>
                        <p><a href='{resetUrl}'>Restablecer contraseña</a></p>
                        <p>Este enlace expirará en {TOKEN_EXPIRATION_HOURS} horas.</p>
                        <p>Saludos,<br>Equipo de Javo2</p>
                    </body>
                    </html>";

                await _emailService.SendEmailAsync(usuario.Email, subject, body, isHtml: true);
                _logger.LogInformation("Email de restablecimiento enviado a usuario {UsuarioID}", usuario.UsuarioID);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar email de restablecimiento a usuario {UsuarioID}", usuario.UsuarioID);
            }
        }
    }

    // Clase para almacenar tokens de restablecimiento
    public class PasswordResetToken
    {
        public int UsuarioID { get; set; }
        public string Token { get; set; }
        public DateTime Creacion { get; set; }
        public DateTime Expiracion { get; set; }
        public bool EsValido { get; set; }
    }
}