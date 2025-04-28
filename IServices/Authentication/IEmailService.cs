using System.Collections.Generic;
using System.Threading.Tasks;

namespace Javo2.IServices
{
    public interface IEmailService
    {
        /// <summary>
        /// Envía un email
        /// </summary>
        /// <param name="to">Dirección de correo del destinatario</param>
        /// <param name="subject">Asunto del correo</param>
        /// <param name="body">Cuerpo del correo</param>
        /// <param name="isHtml">Indica si el cuerpo es HTML</param>
        /// <returns>True si el envío fue exitoso</returns>
        Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = false);

        /// <summary>
        /// Envía un email a múltiples destinatarios
        /// </summary>
        /// <param name="to">Lista de direcciones de correo de los destinatarios</param>
        /// <param name="subject">Asunto del correo</param>
        /// <param name="body">Cuerpo del correo</param>
        /// <param name="isHtml">Indica si el cuerpo es HTML</param>
        /// <returns>True si el envío fue exitoso a todos los destinatarios</returns>
        Task<bool> SendEmailAsync(IEnumerable<string> to, string subject, string body, bool isHtml = false);

        /// <summary>
        /// Envía un email con adjuntos
        /// </summary>
        /// <param name="to">Dirección de correo del destinatario</param>
        /// <param name="subject">Asunto del correo</param>
        /// <param name="body">Cuerpo del correo</param>
        /// <param name="attachments">Lista de rutas a los archivos adjuntos</param>
        /// <param name="isHtml">Indica si el cuerpo es HTML</param>
        /// <returns>True si el envío fue exitoso</returns>
        Task<bool> SendEmailWithAttachmentsAsync(string to, string subject, string body, IEnumerable<string> attachments, bool isHtml = false);
    }
}