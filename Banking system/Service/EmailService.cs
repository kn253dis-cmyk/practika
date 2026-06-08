using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text.Json;
using System.Threading.Tasks;

namespace Banking_system.Service // Виправлено старий простір імен
{
    public class EmailService
    {
        private readonly string _templatesPath;

        public EmailService()
        {
            _templatesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HTMLTemplates", "email_templates.json");
        }

        public string PrepareReceiptHtml(string templateName, Dictionary<string, string> data)
        {
            if (!File.Exists(_templatesPath))
            {
                throw new FileNotFoundException("Файл з шаблонами квитанцій (email_templates.json) не знайдено.");
            }

            string json = File.ReadAllText(_templatesPath);
            var templates = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

            if (templates == null || !templates.ContainsKey(templateName))
            {
                throw new Exception($"Шаблон з назвою '{templateName}' не знайдено у файлі JSON.");
            }

            string html = templates[templateName];

            foreach (var item in data)
            {
                html = html.Replace($"[{item.Key}]", item.Value);
            }

            return html;
        }

        public async Task SendEmailAsync(string targetEmail, string subject, string htmlContent)
        {
            if (string.IsNullOrWhiteSpace(targetEmail))
            {
                throw new ArgumentException("Електронна пошта отримувача не вказана.");
            }

            var settings = SmtpSettings.Load();

            if (string.IsNullOrWhiteSpace(settings.User) || string.IsNullOrWhiteSpace(settings.Password))
            {
                throw new InvalidOperationException("Налаштування SMTP не заповнені. Вкажіть пошту та пароль.");
            }

            try
            {
                using (var client = new SmtpClient(settings.Host, settings.Port))
                {
                    client.EnableSsl = settings.UseSsl;
                    client.Credentials = new NetworkCredential(settings.User, settings.Password);

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(settings.User, settings.SenderName),
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