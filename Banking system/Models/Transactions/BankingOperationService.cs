using System;
using System.Collections.Generic;
using System.Linq;
using Banking_system.Database;
using Banking_system.Entity;
using Banking_system.Models;

namespace Banking_system.Service
{
    public static class BankingOperationService
    {
        public static bool ExecuteTransfer(string sourceCardNumber, string targetCardNumber, decimal amount, string purposeInput)
        {
            if (SessionManager.CurrentUser == null || amount <= 0 || sourceCardNumber == targetCardNumber)
            {
                return false;
            }

            using (var db = new Database.Database())
            {

                var sourceCard = db.Cards.FirstOrDefault(c => c.CardNumber == sourceCardNumber);
                var targetCard = db.Cards.FirstOrDefault(c => c.CardNumber == targetCardNumber);
                var sender = sourceCard != null ? db.Users.FirstOrDefault(u => u.ID == sourceCard.UserId) : null;
                var receiver = targetCard != null ? db.Users.FirstOrDefault(u => u.ID == targetCard.UserId) : null;

                if (sourceCard == null || targetCard == null || sender == null || receiver == null || sourceCard.Balance < amount)
                {
                    Logger.AppendSystemLog(SessionManager.CurrentUser.Email, $"Невдала спроба переказу на картку {targetCardNumber}. Перевірте реквізити та баланс.");
                    return false; 
                }


                sourceCard.Balance -= amount;
                targetCard.Balance += amount;
                db.SaveChanges(); 

                // ЛОГУВАННЯ ДЛЯ ВІДПРАВНИКА
                var senderReceiptData = new Dictionary<string, string>
                {
                    { "Amount", amount.ToString("F2") },
                    { "Date", DateTime.Now.ToString("dd.MM.yyyy HH:mm") },
                    { "Sender", $"{sender.Surname} {sender.Name}" },
                    { "SenderCard", sourceCard.CardNumber },
                    { "Receiver", $"{receiver.Surname} {receiver.Name}" },
                    { "ReceiverCard", targetCard.CardNumber },
                    { "Purpose", purposeInput }
                };

                Logger.AppendLog(
                    userEmail: sender.Email,
                    templateName: "TransferReceipt",
                    text: $"Переказ коштів на картку {targetCard.CardNumber}: {amount} ₴",
                    data: senderReceiptData
                );

                // ЛОГУВАННЯ ДЛЯ ОТРИМУВАЧА
                var receiverReceiptData = new Dictionary<string, string>
                {
                    { "Amount", amount.ToString("F2") },
                    { "Date", DateTime.Now.ToString("dd.MM.yyyy HH:mm") },
                    { "Sender", $"{sender.Surname} {sender.Name}" },
                    { "ReceiverCard", targetCard.CardNumber },
                    { "Purpose", purposeInput }
                };

                Logger.AppendLog(
                    userEmail: receiver.Email,
                    templateName: "DepositReceipt",
                    text: $"Зарахування від {sender.Surname} {sender.Name}: {amount} ₴",
                    data: receiverReceiptData
                );

                return true;
            }
        }
    }
}