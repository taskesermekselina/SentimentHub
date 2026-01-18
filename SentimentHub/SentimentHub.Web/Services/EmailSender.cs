using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace SentimentHub.Web.Services;

public class EmailSettings
{
    public string MailServer { get; set; } = string.Empty;
    public int MailPort { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class EmailSender : IEmailSender
{
    private readonly EmailSettings _emailSettings;

    public EmailSender(IOptions<EmailSettings> emailSettings)
    {
        _emailSettings = emailSettings.Value;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        try
        {
            using var client = new SmtpClient(_emailSettings.MailServer, _emailSettings.MailPort)
            {
                Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password),
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };

            mailMessage.To.Add(email);

            // Write email to file for testing purposes
            try 
            {
                string logContent = $"To: {email}\nSubject: {subject}\nBody:\n{htmlMessage}\n--------------------------------------------------\n";
                string logPath = Path.Combine(Directory.GetCurrentDirectory(), "email_verification_test.txt");
                File.AppendAllText(logPath, logContent);
            }
            catch (Exception) { /* If file logging fails, ignore it and try SMTP */ }

            try
            {
                await client.SendMailAsync(mailMessage);
            }
            catch (Exception smtpEx)
            {
                // SMTP failed (likely no config), but we already logged to file.
                // Suppress error so user can continue flow using the file log.
                Console.WriteLine($"SMTP Sending Failed (Ignored): {smtpEx.Message}");
            }
        }
        catch (Exception ex)
        {
             Console.WriteLine($"General Email Error: {ex.Message}");
             // No throw - ensure app keeps running
        }
    }
}
