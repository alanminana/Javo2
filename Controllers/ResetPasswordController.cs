// Services/EmailService.cs
using Javo2.IServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Javo2.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly bool _enableSsl;

        public EmailService(
            IConfiguration configuration,
            ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            // Cargar configuración de SMTP
            _smtpServer = _configuration["Email:SmtpServer"] ?? "localhost";
            _smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "25");
            _smtpUsername = _configuration["Email:Username"] ?? "";
            _smtpPassword = _configuration["Email:Password"] ?? "";
            _fromEmail = _configuration["Email:FromEmail"] ?? "noreply@javo2.com";
            _fromName = _configuration["Email:FromName"] ?? "Sistema Javo2";
            _enableSsl = bool.Parse(_configuration["Email:EnableSsl"] ?? "false");
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = false)
        {
            try
            {
                if (string.IsNullOrEmpty(to))
                {
                    _logger.LogWarning("No se puede enviar correo: la dirección de destino está vacía");
                    return false;
                }

                using (var client = new SmtpClient(_smtpServer, _smtpPort))
                {
                    if (!string.IsNullOrEmpty(_smtpUsername) && !string.IsNullOrEmpty(_smtpPassword))
                    {
                        client.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);
                    }

                    client.EnableSsl = _enableSsl;

                    using (var mailMessage = new MailMessage())
                    {
                        mailMessage.From = new MailAddress(_fromEmail, _fromName);
                        mailMessage.To.Add(new MailAddress(to));
                        mailMessage.Subject = subject;
                        mailMessage.Body = body;
                        mailMessage.IsBodyHtml = isHtml;

                        await client.SendMailAsync(mailMessage);
                        _logger.LogInformation("Correo enviado a {To} con asunto: {Subject}", to, subject);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar correo a {To}", to);
                return false;
            }
        }

        public async Task<bool> SendEmailWithAttachmentsAsync(string to, string subject, string body,
            List<string> attachments, bool isHtml = false)
        {
            try
            {
                if (string.IsNullOrEmpty(to))
                {
                    _logger.LogWarning("No se puede enviar correo: la dirección de destino está vacía");
                    return false;
                }

                using (var client = new SmtpClient(_smtpServer, _smtpPort))
                {
                    if (!string.IsNullOrEmpty(_smtpUsername) && !string.IsNullOrEmpty(_smtpPassword))
                    {
                        client.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);
                    }

                    client.EnableSsl = _enableSsl;

                    using (var mailMessage = new MailMessage())
                    {
                        mailMessage.From = new MailAddress(_fromEmail, _fromName);
                        mailMessage.To.Add(new MailAddress(to));
                        mailMessage.Subject = subject;
                        mailMessage.Body = body;
                        mailMessage.IsBodyHtml = isHtml;

                        // Añadir archivos adjuntos
                        if (attachments != null && attachments.Count > 0)
                        {
                            foreach (var attachment in attachments)
                            {
                                if (System.IO.File.Exists(attachment))
                                {
                                    mailMessage.Attachments.Add(new Attachment(attachment));
                                }
                                else
                                {
                                    _logger.LogWarning("No se pudo adjuntar el archivo {Attachment}: archivo no encontrado", attachment);
                                }
                            }
                        }

                        await client.SendMailAsync(mailMessage);
                        _logger.LogInformation("Correo con adjuntos enviado a {To} con asunto: {Subject}", to, subject);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar correo con adjuntos a {To}", to);
                return false;
            }
        }

        public async Task<bool> SendEmailToMultipleRecipientsAsync(List<string> to, string subject, string body, bool isHtml = false)
        {
            try
            {
                if (to == null || to.Count == 0)
                {
                    _logger.LogWarning("No se puede enviar correo: la lista de destinatarios está vacía");
                    return false;
                }

                using (var client = new SmtpClient(_smtpServer, _smtpPort))
                {
                    if (!string.IsNullOrEmpty(_smtpUsername) && !string.IsNullOrEmpty(_smtpPassword))
                    {
                        client.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);
                    }

                    client.EnableSsl = _enableSsl;

                    using (var mailMessage = new MailMessage())
                    {
                        mailMessage.From = new MailAddress(_fromEmail, _fromName);

                        // Añadir todos los destinatarios
                        foreach (var recipient in to)
                        {
                            if (!string.IsNullOrEmpty(recipient))
                            {
                                mailMessage.To.Add(new MailAddress(recipient));
                            }
                        }

                        if (mailMessage.To.Count == 0)
                        {
                            _logger.LogWarning("No se puede enviar correo: no hay destinatarios válidos");
                            return false;
                        }

                        mailMessage.Subject = subject;
                        mailMessage.Body = body;
                        mailMessage.IsBodyHtml = isHtml;

                        await client.SendMailAsync(mailMessage);
                        _logger.LogInformation("Correo enviado a {RecipientCount} destinatarios con asunto: {Subject}",
                            mailMessage.To.Count, subject);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar correo a múltiples destinatarios");
                return false;
            }
        }
    }
}