using System;
using System.IO;
using System.Text.Json;

namespace Banking_system.Service
{
    public class SmtpSettings
    {
        private string _host = "smtp.gmail.com";
        private int _port = 587;

        private string _user = "mal4enko2000@gmail.com";
        private string _password = "ulgpbtpbigurdomh"; 

        private string _senderName = "Національний Банк";
        private bool _useSsl = true;

        public string Host { get => _host; set => _host = value; }
        public int Port { get => _port; set => _port = value; }
        public string User { get => _user; set => _user = value; }
        public string Password { get => _password; set => _password = value; }
        public string SenderName { get => _senderName; set => _senderName = value; }
        public bool UseSsl { get => _useSsl; set => _useSsl = value; }

        private static readonly string _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "smtp_settings.json");

        public static SmtpSettings Load()
        {
            return new SmtpSettings();
        }

        public void Save()
        {
            string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
    }
}