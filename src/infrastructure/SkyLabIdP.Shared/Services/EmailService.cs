using System.Net.Security;
using SkyLabIdP.Application.Common.Exceptions;
using SkyLabIdP.Application.Common.Interfaces;
using SkyLabIdP.Application.Dtos.Email;
using SkyLabIdP.Domain.Settings;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace SkyLabIdP.Shared.Services
{
    public class EmailService : IEmailService
    {
        private MailSettings MailSettings { get; }
        private ILogger<EmailService> Logger { get; }
        private bool IsDevelopment { get; }
        public EmailService(IOptions<MailSettings> mailSettings, ILogger<EmailService> logger, IHostEnvironment env)
        {
            MailSettings = mailSettings.Value;
            Logger = logger;
            IsDevelopment = env.IsDevelopment();
        }

        public async Task SendAsync(EmailDto emailRequest)
        {
            try
            {
                // create message

                var email = new MimeMessage();
                email.From.Add(new MailboxAddress("SkyLab系統", MailSettings.EmailFrom));
                foreach (var recipient in emailRequest.To)
                {
                    email.To.Add(MailboxAddress.Parse(recipient));
                }
                email.Subject = emailRequest.Subject;
                var builder = new BodyBuilder { HtmlBody = emailRequest.Body };
                email.Body = builder.ToMessageBody();
                var _SecureSocketOptions = MailSettings.SmtpUseSSL ? MailKit.Security.SecureSocketOptions.Auto : MailKit.Security.SecureSocketOptions.None;
                using var smtp = new SmtpClient();
                smtp.ServerCertificateValidationCallback +=
                (sender, certificate, chain, errors) =>
                {
                    // local dev, just approve all certs
                    if (IsDevelopment) return true;
                    return errors == SslPolicyErrors.None;
                };
                smtp.Connect(MailSettings.SmtpHost, MailSettings.SmtpPort, _SecureSocketOptions);
                if (!String.IsNullOrWhiteSpace(MailSettings.SmtpUser) && !String.IsNullOrWhiteSpace(MailSettings.SmtpPass))
                {
                    smtp.Authenticate(MailSettings.SmtpUser, MailSettings.SmtpPass);
                }
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);

            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to send email");
                throw new ApiException("Failed to send email");
            }
        }
        public async Task SendErrorEmailAsync(Exception ex, string subjectPrefix = "[SkyLabDoc]")
        {
            var emailDto = new EmailDto
            {
                To = [MailSettings.PgEmail ?? "skyhsieh@skylab.com.tw"],
                From = MailSettings.EmailFrom ?? "skyhsieh@skylab.com.tw",
                Subject = $" 在{subjectPrefix}發生例外錯誤",
                Body = $"在{subjectPrefix}生例外錯誤.<br />錯誤訊息：{ex}"
            };

            await SendAsync(emailDto);
        }
    }
}
