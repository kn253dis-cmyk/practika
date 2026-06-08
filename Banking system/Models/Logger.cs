using System;
using System.IO;

namespace Banking_system.Models
{
    internal static class Logger
    {
        private static readonly string logFilePath = "bank_log.txt";

        public static void Log(string message)
        {
            string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";

            System.Diagnostics.Debug.WriteLine(logEntry);

            try
            {
                File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Помилка запису логу: {ex.Message}");
            }
        }
    }
}