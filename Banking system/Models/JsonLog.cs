using System;
using System.Collections.Generic;

namespace Banking_system.Models
{
    public class JsonLog
    {
        public class LogEntry
        {
            public string Id { get; set; } = "";
            public string UserEmail { get; set; } = "";
            public string TemplateName { get; set; } = "";
            public string Text { get; set; } = "";
            public DateTime Date { get; set; } = DateTime.Now;
            public Dictionary<string, string> ReceiptData { get; set; } = new Dictionary<string, string>();
        }
    }
}