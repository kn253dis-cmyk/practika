namespace Banking_system.Service
{
    public class SmtpSettings
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string SenderName { get; set; }
        public bool UseSsl { get; set; }

        public SmtpSettings(string user, string password, string senderName = "Національний Банк", string host = "smtp.gmail.com", int port = 587, bool useSsl = true)
        {
            User = user;
            Password = password;
            SenderName = senderName;
            Host = host;
            Port = port;
            UseSsl = useSsl;
        }
    }
}