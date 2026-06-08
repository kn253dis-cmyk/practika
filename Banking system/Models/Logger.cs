using System;
using System.Collections.Generic;
using System.IO;
using System.Linq; // ДОДАНО для фільтрації (.Where)
using System.Text.Json;
using Banking_system.Models;

namespace Banking_system.Helpers
{
    public static class Logger
    {
        private static readonly string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bank_logs.json");

        public static void AppendLog(string userEmail, string templateName, string text, Dictionary<string, string> data)
        {
            try
            {
                List<JsonLog.LogEntry> allLogs = ReadAllLogsFromDisk();

                JsonLog.LogEntry newLog = new JsonLog.LogEntry
                {
                    Id = "TXN-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                    UserEmail = userEmail, 
                    TemplateName = templateName,
                    Text = text,
                    Date = DateTime.Now,
                    ReceiptData = data
                };

                allLogs.Insert(0, newLog);

                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(allLogs, options);
                File.WriteAllText(LogFilePath, jsonString);
            }
            catch (Exception ex)
            {
                throw new Exception("Помилка при записі дії в файл логів: " + ex.Message);
            }
        }

      
        public static List<JsonLog.LogEntry> ReadUserLogs(string userEmail)
        {
            var allLogs = ReadAllLogsFromDisk();

        
            return allLogs.Where(log => log.UserEmail == userEmail).ToList();
        }

        private static List<JsonLog.LogEntry> ReadAllLogsFromDisk()
        {
            if (!File.Exists(LogFilePath)) return new List<JsonLog.LogEntry>();

            try
            {
                string jsonContent = File.ReadAllText(LogFilePath);
                List<JsonLog.LogEntry>? list = JsonSerializer.Deserialize<List<JsonLog.LogEntry>>(jsonContent);
                return list ?? new List<JsonLog.LogEntry>();
            }
            catch
            {
                return new List<JsonLog.LogEntry>();
            }
        }
    }
}