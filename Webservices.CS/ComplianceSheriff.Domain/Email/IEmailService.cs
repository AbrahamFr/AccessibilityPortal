using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ComplianceSheriff.Email
{
    public interface IEmailService
    {
        void SendEmail(EmailModel emailModel, CancellationToken cancellationToken);

        string GenerateEmailMessage(string templateName, params object[] args);
    }
}
