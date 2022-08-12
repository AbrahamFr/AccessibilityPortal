using ComplianceSheriff.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;

namespace ComplianceSheriff.Email
{
    public class EmailService : IEmailService
    {
        private string _host;
        private string _from;
        private int _port;
        private readonly ILogger<EmailService> _logger;
        private readonly ConfigurationOptions _configOptions;

        public EmailService(IOptions<ConfigurationOptions> configOptions, ILogger<EmailService> logger)
        {
            _configOptions = configOptions.Value;
            _logger = logger;
        }

        public string GenerateEmailMessage(string templateName, params object[] args)
        {
            var message = String.Empty;
            var emailTemplate = GetEmailTemplate(templateName);

            using (var sourceReader = System.IO.File.OpenText(emailTemplate))
            {
                message = sourceReader.ReadToEnd();
            }

            string messageBody = string.Format(message, args);
            return messageBody;
        }

        private string GetEmailTemplate(string templateName)
        {
            var templatePage = String.Empty;
            var templatePath = "./EmailTemplates/";

            switch (templateName)
            {
                case "PasswordReset":
                    templatePage = templatePath + "passwordReset.html";
                    break;

            }

            return templatePage;
        }

        public void SendEmail(EmailModel emailModel, CancellationToken cancellationToken)
        {
            try
            {
                _host = _configOptions.Smtp.Host;
                _port = _configOptions.Smtp.Port;
                _from = _configOptions.Smtp.From;

                using (SmtpClient client = new SmtpClient(_host, _port))
                {
                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(_from),
                        BodyEncoding = Encoding.UTF8
                    };

                    mailMessage.To.Add(emailModel.To);
                    mailMessage.Body = emailModel.Message;
                    mailMessage.Subject = emailModel.Subject;
                    mailMessage.IsBodyHtml = emailModel.IsBodyHtml;

                    if (!String.IsNullOrWhiteSpace(_configOptions.Smtp.UserName) && !String.IsNullOrWhiteSpace(_configOptions.Smtp.Password))
                    {
                        var networkCredentials = new NetworkCredential(_configOptions.Smtp.UserName, _configOptions.Smtp.Password);
                        client.Credentials = networkCredentials;
                    } else
                    {
                        _logger.LogInformation("SMTP UserName and/or SMTP Password is not set.");
                    }

                    client.Send(mailMessage);
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message + " " + ex.StackTrace);
                throw;
            }
        }
    }
}
