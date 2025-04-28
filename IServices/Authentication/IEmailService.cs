// IServices/IEmailService.cs
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Javo2.IServices
{
    public interface IEmailService
    {
        /// <summary>
        /// Envía un correo electrónico
        /// </summary>
        /// <param name="to">Dirección de correo electrónico del destinatario</param>
        /// <param name="subject">Asunto del correo</param>
        /// <param name="body">Cuerpo del correo</param>
        /// <param name="isHtml">Indica si el cuerpo del correo es HTML</param>
        /// <returns>True si el correo se envió correctamente, false en caso contrario</returns>
        Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = false);

        /// <summary>
        /// Envía un correo electrónico con archivos adjuntos
        /// </summary>
        /// <param name="to">Dirección de correo electrónico del destinatario</param>
        /// <param name="subject">Asunto del correo</param>
        /// <param name="body">Cuerpo del correo</param>
        /// <param name="attachments">Lista de rutas de archivos adjuntos</param>
        /// <param name="isHtml">Indica si el cuerpo del correo es HTML</param>
        /// <returns>True si el correo se envió correctamente, false en caso contrario</returns>
        Task<bool> SendEmailWithAttachmentsAsync(string to, string subject, string body,
            List<string> attachments, bool isHtml = false);

        /// <summary>
        /// Envía un correo electrónico a múltiples destinatarios
        /// </summary>
        /// <param name="to">Lista de direcciones de correo electrónico de los destinatarios</param>
        /// <param name="subject">Asunto del correo</param>
        /// <param name="body">Cuerpo del correo</param>
        /// <param name="isHtml">Indica si el cuerpo del correo es HTML</param>
        /// <returns>True si el correo se envió correctamente, false en caso contrario</returns>
        Task<bool> SendEmailToMultipleRecipientsAsync(List<string> to, string subject, string body, bool isHtml = false);
    }
}