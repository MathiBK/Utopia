using System;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

namespace utopia.Services
{
    public class EmailSender : IEmailSender
    {
        public EmailSender(IOptions<AuthMessageSenderOptions> optionsAccessor)
        {
            Options = optionsAccessor.Value;
        }
        

        public AuthMessageSenderOptions Options { get; } //set only via Secret Manager

        public Task SendEmailAsync(string email, string subject, string message)
        {
            return Execute(Options.SendGridKey, subject, message, email);
        }

        public Task Execute(string apiKey, string subject, string message, string email)
        {
            
            //Denne burde egt lagres i en form for keystore
            var apiKeyExposed = "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";
            
            var client = new SendGridClient(apiKeyExposed);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress("mail@utopiaspillet.no", "utopia"),
                Subject = subject,
                PlainTextContent = message,
                HtmlContent = message
            };
            msg.AddTo(new EmailAddress(email));

            // Disable click tracking.
            // See https://sendgrid.com/docs/User_Guide/Settings/tracking.html
            msg.SetClickTracking(false, false);
            var test = client.SendEmailAsync(msg);
            Console.WriteLine(test.Result.StatusCode);
            return test;
        }
    }
}