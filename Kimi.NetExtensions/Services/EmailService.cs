using Kimi.NetExtensions.Localization;
using Microsoft.IdentityModel.Tokens;
using System.Net.Mail;

public static class EmailService
{
    static EmailService()
    {
        LicenceHelper.CheckLicense();
    }

    private static string? _smtpServer;
    private static int _smtpPort = 25;

    public static void UseEmailServie(string smtpServer, int smtpPort = 25)
    {
        _smtpServer = smtpServer;
        _smtpPort = smtpPort;
    }

    /// <summary>
    /// The function is an asynchronous function used to send an email. It takes an Email object and
    /// a sendBy parameter as input. Inside the function, it first checks if EmailService.SmtpServer
    /// is null or empty, and throws an exception if it is null or empty. Then, depending on the
    /// debug mode (DEBUG), it modifies the email subject and content. Next, it creates a SmtpClient
    /// object, setting the sender and port. Then, it creates a MailMessage object, setting the
    /// sender, recipients, subject, and body of the email. If there are carbon copies (CC) and
    /// blind carbon copies (BCC) in the email, it adds them to the MailMessage object. Then, it
    /// adds a hidden text to the email body to record the sending information such as the sendBy
    /// and the current time. Finally, if there are attachments in the email, it adds them to the
    /// MailMessage object and sends the email using the SendMailAsync method of the smtp object.
    /// Use service.UseEmailService() to add the EmailService to the service collection.
    /// </summary>
    /// <param name="email">
    /// </param>
    /// <param name="sendByActualUser">
    /// </param>
    /// <returns>
    /// </returns>
    /// <exception cref="Exception">
    /// </exception>
    public static async Task Send(Email email, string sendByActualUser)
    {
        if (_smtpServer.IsNullOrEmpty())
        {
            throw new Exception("EmailService.SmtpServer is null");
        }
#if DEBUG
        email.Subject = email.Subject + " - " + L.SendEmail_AppDevTest;
        string devNotif = "<h1>" + L.SendEmail_AppDevTest + "</h1><hr />";
        email.Body = devNotif + email.Body;
#endif
        SmtpClient smtp = new SmtpClient(_smtpServer, _smtpPort);
        var emailMessage = new MailMessage(from: email.From, to: string.Join(';', email.To), subject: email.Subject, body: email.Body);
        emailMessage.IsBodyHtml = true;
        if (email.Cc?.Any() == true)
        {
            emailMessage.CC.Add(string.Join(';', email.Cc));
        }
        if (email.Bcc?.Any() == true)
        {
            emailMessage.Bcc.Add(string.Join(';', email.Bcc));
        }
        string hiddenText = $"""
                            <div style="display:none;">Sent by: {sendByActualUser} Sent on: {DateTime.UtcNow.ToIsoDateTime()} </div>
                            <div style="display:none;">Client Host: {EnvironmentExtension.ClientHost} Client IP: {EnvironmentExtension.ClientIp} </div>
                            """;
        emailMessage.Body += hiddenText;
        if (email.Attachments?.Any() == true)
        {
            foreach (var attachment in email.Attachments)
            {
                if (!string.IsNullOrEmpty(attachment.Name) && !string.IsNullOrEmpty(attachment.Base64Content))
                {
                    byte[] data = Convert.FromBase64String(attachment.Base64Content);
                    MemoryStream ms = new MemoryStream(data);
                    emailMessage.Attachments.Add(new System.Net.Mail.Attachment(ms, attachment.Name));
                }
            }
        }
        await smtp.SendMailAsync(emailMessage);
    }
}