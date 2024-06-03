using HMZ.DTOs.Models;
using HMZ.DTOs.Queries;
using HMZ.Service.Helpers;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace HMZ.Service.MailServices
{
    public class MailService : IMailService
    {
        private readonly MailSettings _mailSettings;
        public MailService(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }
        public async Task<DataResult<bool>> SendEmailAsync(MailQuery mailRequest)
        {
            try
            {
                var emailBox = new MailboxAddress(_mailSettings.DisplayName, _mailSettings.Mail);
                var email = new MimeMessage();
                email.From.Add(emailBox);
                email.To.AddRange(mailRequest.ToEmails.Select(x => MailboxAddress.Parse(x)));
                email.Subject = mailRequest.Subject;
                var builder = new BodyBuilder();
                if (mailRequest.Attachments != null)
                {
                    byte[] fileBytes;
                    foreach (var file in mailRequest.Attachments)
                    {
                        if (file.Length > 0)
                        {
                            using (var ms = new MemoryStream())
                            {
                                file.CopyTo(ms);
                                fileBytes = ms.ToArray();
                            }
                            builder.Attachments.Add(file.FileName, fileBytes, ContentType.Parse(file.ContentType));
                        }
                    }
                }
                builder.HtmlBody = mailRequest.Body;
                email.Body = builder.ToMessageBody();
                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
                
                await smtp.AuthenticateAsync(_mailSettings.Mail, _mailSettings.Password);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
                return new DataResult<bool> { Entity = true };
            }
            catch (System.Exception ex)
            {
                return new DataResult<bool> { Entity = false, Errors = new List<string> { ex.Message } };
            }
        }
    }
}
