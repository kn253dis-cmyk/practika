using Banking_system.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;

namespace Banking_system.Models
{
    public static class Logger
    {
        private static readonly string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bank_logs.json");


        public static void Log(string message)
        {

            System.Diagnostics.Debug.WriteLine($"[Card System]: {message}");
        }
        public static void LogUserAction(string userEmail, string message)
        {
            AppendLog(userEmail, "General", message, new Dictionary<string, string>());
        }
        //  основний метод для логування дій конкретного користувача в JSON
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

        public static void AppendSystemLog(string userEmail, string text)
        {
        
            AppendLog(userEmail, "SystemLog", text, new Dictionary<string, string>());
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

        /*
        ЧАСТИНА ДЛЯ ВІДПРАВНИКА (Списання)
            var senderReceiptData = new Dictionary<string, string>
            {
                { "Amount", amount.ToString("F2") }, // amount - сума переказу (decimal)
                { "Date", DateTime.Now.ToString("dd.MM.yyyy HH:mm") },
                { "Sender", $"{sender.Surname} {sender.Name}" }, // sender - об'єкт User з БД
                { "SenderCard", sender.CardNumber },
                { "Receiver", $"{receiver.Surname} {receiver.Name}" }, // receiver - об'єкт User з БД
                { "ReceiverCard", receiver.CardNumber },
                { "Purpose", purposeInput } // purposeInput - рядок з текстового поля (наприклад "Повернення боргу")
            };

            Logger.AppendLog(
                userEmail: sender.Email,
                templateName: "TransferReceipt",
                text: $"Переказ коштів на картку {receiver.CardNumber}: {amount} ₴",
                data: senderReceiptData
            );

         ЧАСТИНА ДЛЯ ОТРИМУВАЧА (Зарахування) 
            var receiverReceiptData = new Dictionary<string, string>
            {
                { "Amount", amount.ToString("F2") },
                { "Date", DateTime.Now.ToString("dd.MM.yyyy HH:mm") },
                { "Sender", $"{sender.Surname} {sender.Name}" },
                { "ReceiverCard", receiver.CardNumber },
                { "Purpose", purposeInput }
            };

            Logger.AppendLog(
                userEmail: receiver.Email, 
                templateName: "DepositReceipt",
                text: $"Зарахування від {sender.Surname} {sender.Name}: {amount} ₴",
                data: receiverReceiptData
            );
            
        
        ДЛЯ ЗНЯТТЯ ГОТІВКИ 
            var withdrawalData = new Dictionary<string, string>
            {
                { "Amount", amount.ToString("F2") }, // amount - сума зняття
                { "Date", DateTime.Now.ToString("dd.MM.yyyy HH:mm") },
                { "CardNumber", user.CardNumber }, // user - поточний об'єкт User
                { "Balance", currentBalance.ToString("F2") }, // currentBalance - залишок після зняття (decimal)
                { "Purpose", "Зняття готівки" }
            };

            Logger.AppendLog(
                userEmail: user.Email,
                templateName: "WithdrawalReceipt",
                text: $"Зняття готівки: {amount} ₴",
                data: withdrawalData
            );

        ДЛЯ ОФОРМЛЕННЯ КРЕДИТУ
            var loanData = new Dictionary<string, string>
            {
                { "Amount", loanAmount.ToString("F2") }, // loanAmount - сума кредиту
                { "Date", DateTime.Now.ToString("dd.MM.yyyy HH:mm") },
                { "CardNumber", user.CardNumber },
                { "Percentage", "3.5" }, // Відсоток (можна брати з властивості CreditCard.Percentage)
                { "CreditEndDate", DateTime.Now.AddYears(1).ToString("dd.MM.yyyy") }, // Дата закінчення дії (або з CreditCard.CreditEndDate)
                { "CreditLimit", creditLimit.ToString("F2") } // Встановлений ліміт (з CreditCard.CreditLimit)
            };

            Logger.AppendLog(
                userEmail: user.Email,
                templateName: "LoanReceipt",
                text: $"Оформлено кредит: {loanAmount} ₴",
                data: loanData
            );
         */


    }
}