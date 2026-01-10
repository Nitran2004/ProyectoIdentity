using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;

namespace ProyectoIdentity.Servicios
{
    public class ServicioEmail : IEmailSender
    {
        private readonly IConfiguration _config;

        public ServicioEmail(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var smtpHost = _config["SmtpSettings:Server"];
            var smtpPort = int.Parse(_config["SmtpSettings:Port"]);
            var smtpUser = _config["SmtpSettings:Username"];
            var smtpPass = _config["SmtpSettings:Password"];
            var senderEmail = _config["SmtpSettings:SenderEmail"];
            var senderName = _config["SmtpSettings:SenderName"];

            using (var client = new SmtpClient(smtpHost, smtpPort))
            {
                client.Credentials = new NetworkCredential(smtpUser, smtpPass);
                client.EnableSsl = true;

                // --- ESTA ES LA LÍNEA QUE DEBES AGREGAR ---
                // Le dice a .NET que acepte el certificado de Brevo aunque crea que hay un error de nombre
                ServicePointManager.ServerCertificateValidationCallback =
                    delegate { return true; };
                // ------------------------------------------

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, senderName),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(email);

                await client.SendMailAsync(mailMessage);
            }
        }
    }
}