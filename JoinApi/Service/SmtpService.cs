using JoinApi.Settings;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;


namespace JoinApi.Service
{
    public class SmtpService
    {
        private readonly SmtpSettings _smtpSettings;

        public SmtpService(IOptions<SmtpSettings> smtpSettingsOptions)
        {
            _smtpSettings = smtpSettingsOptions.Value;
        }

        public async Task<bool> SendAsync(string recipientName, string recipientEmail, string subject, string body, IEnumerable<IFormFile>? attachments = null)
        {
            var senderMailboxAddress = new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.SenderEmail);

            var bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = body;
            if (attachments != null)
            {
                byte[] fileBytes;
                foreach (var file in attachments)
                {
                    if (file.Length > 0)
                    {
                        using (var ms = new MemoryStream())
                        {
                            file.CopyTo(ms);
                            fileBytes = ms.ToArray();
                        }
                        bodyBuilder.Attachments.Add(file.FileName, fileBytes, ContentType.Parse(file.ContentType));
                    }
                }
            }

            var message = new MimeMessage(new List<MailboxAddress> { senderMailboxAddress }, new List<MailboxAddress> { new MailboxAddress(recipientName, recipientEmail) }, subject, bodyBuilder.ToMessageBody())
            {
                Sender = senderMailboxAddress
            };

            using var smtp = new SmtpClient();

            try
            {
                await smtp.ConnectAsync(_smtpSettings.Host, _smtpSettings.Port.Value, _smtpSettings.Security.Value);
            }
            catch (Exception)
            {
                return false;
            }
            try
            {
                await smtp.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);
            }
            catch (Exception)
            {
                return false;
            }
            try
            {
                await smtp.SendAsync(message);
            }
            catch (Exception)
            {
                return false;
            }
            try
            {
                await smtp.DisconnectAsync(true);
            }
            catch (Exception)
            {
            }

            await Task.CompletedTask;

            return true;
        }
    }
}
