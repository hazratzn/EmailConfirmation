using eShopDemo.Services.Interfaces;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

namespace eShopDemo.Services
{
    public class SendEmailService: ISendEmail
    {
        public async Task SendEmail(string toEmail, string url)
        {
            var apiKey = "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";
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("tahmin.fatiyev@gmail.com", "Tahmin Fatiyev");
            var subject = "Verify Your account";
            var to = new EmailAddress(toEmail, "Somebody");
            var plainTextContent = "and easy to do anywhere, even with C#";
            var htmlContent = $"<a href={url} >Click here to Verify Your Account</a>";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);
        }
    }
}
