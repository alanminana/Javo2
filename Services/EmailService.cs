using Javo2.IServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Javo2.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;
        private readonly bool _isDevelopment;
        private readonly string _emailLogPath;

        public EmailService(
            ILogger<EmailService> logger,
            IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
            _emailLogPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "emails.log");

            // Asegurar que exista el directorio de logs
            Directory.CreateDirectory(Path.GetDirectoryName(_emailLogPath));
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = false)
        {
            try
            {
                if (_isDevelopment)
                {
                    return await LogEmailForDevelopmentAsync(to, subject, body, isHtml);
                }
                else
                {
                    // En un entorno de producción, aquí se implementaría el envío real
                    // utilizando un servicio SMTP o un proveedor como SendGrid, MailChimp, etc.
                    _logger.LogWarning("Envío de email en modo producción no está implementado");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar email a {To}, Asunto: {Subject}", to, subject);
                return false;
            }
        }

        public async Task<bool> SendEmailAsync(IEnumerable<string> to, string subject, string body, bool isHtml = false)
        {
            if (to == null || !to.Any())
            {
                _logger.LogWarning("No se especificaron destinatarios para el email");
                return false;
            }

            var allSuccess = true;
            foreach (var recipient in to)
            {
                var success = await SendEmailAsync(recipient, subject, body, isHtml);
                allSuccess = allSuccess && success;
            }

            return allSuccess;
        }

        public async Task<bool> SendEmailWithAttachmentsAsync(string to, string subject, string body, IEnumerable<string> attachments, bool isHtml = false)
        {
            try
            {
                if (_isDevelopment)
                {
                    var success = await LogEmailForDevelopmentAsync(to, subject, body, isHtml);
                    if (success && attachments != null && attachments.Any())
                    {
                        // Loguear información sobre los adjuntos
                        var attachmentPaths = string.Join(", ", attachments);
                        await File.AppendAllTextAsync(_emailLogPath, $"[{DateTime.Now}] ATTACHMENTS: {attachmentPaths}{Environment.NewLine}");
                    }
                    return success;
                }
                else
                {
                    // En un entorno de producción, aquí se implementaría el envío real
                    // utilizando un servicio SMTP o un proveedor como SendGrid, MailChimp, etc.
                    _logger.LogWarning("Envío de email con adjuntos en modo producción no está implementado");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar email con adjuntos a {To}, Asunto: {Subject}", to, subject);
                return false;
            }
        }

        private async Task<bool> LogEmailForDevelopmentAsync(string to, string subject, string body, bool isHtml)
        {
            try
            {
                _logger.LogInformation("SIMULANDO ENVÍO DE EMAIL - To: {To}, Subject: {Subject}, IsHtml: {IsHtml}",
                    to, subject, isHtml);

                // Guardar el email en un archivo de logs para desarrollo
                var emailContent = $@"
[{DateTime.Now}] EMAIL SENT
TO: {to}
SUBJECT: {subject}
IS_HTML: {isHtml}
BODY:
{body}
---------------------------------------------
";
                await File.AppendAllTextAsync(_emailLogPath, emailContent);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al simular envío de email en desarrollo");
                return false;
            }
        }
    }
}
