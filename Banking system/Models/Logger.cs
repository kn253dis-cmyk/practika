using Banking_system.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Banking_system.Models
{
    public static class Logger
    {
        private static readonly string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bank_logs.json");
        private static List<JsonLog.LogEntry>? _cachedLogs = null;
        private static readonly object _lock = new object();

        public static void Log(string message)
        {
            System.Diagnostics.Debug.WriteLine($"[Card System]: {message}");
        }

        public static void LogUserAction(string userEmail, string message)
        {
            AppendLog(userEmail, "General", message, new Dictionary<string, string>());
        }

        private static void EnsureLogsLoaded()
        {
            if (_cachedLogs == null)
            {
                if (File.Exists(LogFilePath))
                {
                    try
                    {
                        string jsonContent = File.ReadAllText(LogFilePath);
                        _cachedLogs = JsonSerializer.Deserialize<List<JsonLog.LogEntry>>(jsonContent) ?? new List<JsonLog.LogEntry>();
                    }
                    catch
                    {
                        _cachedLogs = new List<JsonLog.LogEntry>();
                    }
                }
                else
                {
                    _cachedLogs = new List<JsonLog.LogEntry>();
                }
            }
        }

        public static void AppendLog(string userEmail, string templateName, string text, Dictionary<string, string> data)
        {
            try
            {
                lock (_lock)
                {
                    EnsureLogsLoaded(); 

                    string transactionId = "TXN-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();

                    if (data != null && !data.ContainsKey("TransactionId"))
                    {
                        data["TransactionId"] = transactionId;
                    }

                    JsonLog.LogEntry newLog = new JsonLog.LogEntry
                    {
                        Id = transactionId,
                        UserEmail = userEmail,
                        TemplateName = templateName,
                        Text = text,
                        Date = DateTime.Now,
                        ReceiptData = data ?? new Dictionary<string, string>()
                    };

                    _cachedLogs!.Insert(0, newLog);

                    var options = new JsonSerializerOptions { WriteIndented = true };
                    string jsonString = JsonSerializer.Serialize(_cachedLogs, options);
                    File.WriteAllText(LogFilePath, jsonString);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Помилка при записі дії в файл логів: " + ex.Message);
            }
        }

        public static void AppendSystemLog(string userEmail, string text)
        {
            AppendLog(userEmail, "SystemLog", text, new Dictionary<string, string>());
        }

        public static List<JsonLog.LogEntry> ReadUserLogs(string userEmail)
        {
            lock (_lock)
            {
                EnsureLogsLoaded(); 
                return _cachedLogs!.Where(log => log.UserEmail == userEmail).ToList();
            }
        }
    }
}