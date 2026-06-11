using System;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace Banking_system.Service
{
    public class SmtpSettings
    {
        public string Host { get; set; } = "smtp.gmail.com";
        public int Port { get; set; } = 587;
        public string User { get; set; } = "пошта@gmail.com";
        public string Password { get; set; } = "пароль додатка";
        public string SenderName { get; set; } = "Національний Банк";
        public bool UseSsl { get; set; } = true;

        private static readonly string _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "smtp_settings.json");

        public static SmtpSettings Load()
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    SmtpSettings defaultSettings = new SmtpSettings();
                    defaultSettings.Save();
                    return defaultSettings;
                }

                string json = File.ReadAllText(_filePath);

                SmtpSettings? settings = JsonSerializer.Deserialize<SmtpSettings>(json);

                return settings ?? new SmtpSettings();  
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження налаштувань smtp_settings.json: {ex.Message}", "Помилка конфігурації");
                return new SmtpSettings(); 
            }
        }

        public void Save()
        {
            try
            {

                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(this, options);
                File.WriteAllText(_filePath, jsonString);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка збереження файлу smtp_settings.json: {ex.Message}");
            }
        }
    }
}