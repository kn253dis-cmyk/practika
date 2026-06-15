using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text.Json;
using System.Threading.Tasks;

namespace Banking_system.Service
{
    public class EmailService
    {
        private readonly string _templatesPath;
        private readonly SmtpSettings _smtpSettings;

        public EmailService()
        {
            _templatesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HTMLTemplates", "email_templates.json");

            
            _smtpSettings = new SmtpSettings("mal4enko2000@gmail.com", "qkzqswykrmrcjffe");
        }

        public string PrepareReceiptHtml(string templateName, Dictionary<string, string> data)
        {
            if (!File.Exists(_templatesPath))
                throw new FileNotFoundException("Файл з шаблонами квитанцій не знайдено.");

            string json = File.ReadAllText(_templatesPath);
            var templates = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

            if (templates == null || !templates.ContainsKey(templateName))
                throw new Exception($"Шаблон '{templateName}' не знайдено.");

            string html = templates[templateName];

            if (!data.ContainsKey("BankName"))
            {
                data["BankName"] = _smtpSettings.SenderName;
            }

            foreach (var item in data)
            {
                html = html.Replace($"[{item.Key}]", item.Value);
            }

            return html;
        }

        public async Task SendEmailAsync(string targetEmail, string subject, string htmlContent)
        {
            if (string.IsNullOrWhiteSpace(targetEmail))
                throw new ArgumentException("Електронна пошта отримувача не вказана.");

            if (string.IsNullOrWhiteSpace(_smtpSettings.Password))
                throw new InvalidOperationException("Пароль SMTP не вказано у конструкторі EmailService.");

            try
            {
                using (var client = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port))
                {
                    client.EnableSsl = _smtpSettings.UseSsl;
                    client.Credentials = new NetworkCredential(_smtpSettings.User, _smtpSettings.Password);

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(_smtpSettings.User, _smtpSettings.SenderName),
                        Subject = subject,
                        Body = htmlContent,
                        IsBodyHtml = true
                    };

                    mailMessage.To.Add(targetEmail);

                    await client.SendMailAsync(mailMessage);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Не вдалося надіслати лист. Деталі: {ex.Message}");
            }
        }
    }
}