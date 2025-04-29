using System.Collections.Generic;
using System.Threading.Tasks;

namespace Javo2.IServices
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = false);
        Task<bool> SendEmailAsync(IEnumerable<string> to, string subject, string body, bool isHtml = false);
        Task<bool> SendEmailWithAttachmentsAsync(string to, string subject, string body, IEnumerable<string> attachments, bool isHtml = false);
    }
}