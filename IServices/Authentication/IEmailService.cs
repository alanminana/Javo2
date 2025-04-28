// IServices/IEmailService.cs
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Javo2.IServices
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = false);

        Task<bool> SendEmailWithAttachmentsAsync(string to, string subject, string body,
            List<string> attachments, bool isHtml = false);

        Task<bool> SendEmailToMultipleRecipientsAsync(List<string> to, string subject, string body, bool isHtml = false);
    }
}