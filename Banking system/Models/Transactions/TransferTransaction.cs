using System;
using System.Collections.Generic;
using System.Linq;

namespace Banking_system.Models.Transactions
{
    internal class TransferTransaction : AbstractTransaction
    {
        private readonly string _sourceCardNumber;
        private readonly string _targetCardNumber;
        private readonly string _purpose;

        public TransferTransaction(string sourceCardNumber, string targetCardNumber, decimal amount, string purpose)
            : base(amount)
        {
            _sourceCardNumber = sourceCardNumber;
            _targetCardNumber = targetCardNumber;
            _purpose = purpose;
        }

        public override bool Execute()
        {
            if (Service.SessionManager.CurrentUser == null || Amount <= 0 || _sourceCardNumber == _targetCardNumber)
            {
                return false;
            }

            using (var db = new DataBase.Database())
            {
                var sourceCard = db.Cards.FirstOrDefault(c => c.CardNumber == _sourceCardNumber);
                var targetCard = db.Cards.FirstOrDefault(c => c.CardNumber == _targetCardNumber);

                var sender = sourceCard != null ? db.Users.FirstOrDefault(u => u.ID == sourceCard.UserId) : null;
                var receiver = targetCard != null ? db.Users.FirstOrDefault(u => u.ID == targetCard.UserId) : null;

                if (sourceCard == null || targetCard == null || sender == null || receiver == null)
                {
                    Logger.AppendSystemLog(Service.SessionManager.CurrentUser.Email, $"Невдала спроба переказу. Невірні реквізити картки.");
                    return false;
                }

                Logger.Log($"[Транзакція {TransactionId}] Ініційовано переказ {Amount:C} з картки {_sourceCardNumber} на {_targetCardNumber}.");

                bool isWithdrawn = sourceCard.Withdraw(Amount);

                if (isWithdrawn)
                {
                    targetCard.Deposit(Amount);
                    db.SaveChanges(); 

                    Logger.Log($"[Транзакція {TransactionId}] Переказ успішно завершено.");


                    // ЛОГУВАННЯ ДЛЯ ВІДПРАВНИКА
                    var senderReceiptData = new Dictionary<string, string>
                    {
                        { "Amount", Amount.ToString("F2") },
                        { "Date", DateTime.Now.ToString("dd.MM.yyyy HH:mm") },
                        { "Sender", $"{sender.Surname} {sender.Name}" },
                        { "SenderCard", sourceCard.CardNumber },
                        { "Receiver", $"{receiver.Surname} {receiver.Name}" },
                        { "ReceiverCard", targetCard.CardNumber },
                        { "Purpose", _purpose }
                    };

                    Logger.AppendLog(
                        userEmail: sender.Email,
                        templateName: "TransferReceipt",
                        text: $"Переказ коштів на картку {targetCard.CardNumber}: {Amount} ₴",
                        data: senderReceiptData
                    );

                    // ЛОГУВАННЯ ДЛЯ ОТРИМУВАЧА
                    var receiverReceiptData = new Dictionary<string, string>
                    {
                        { "Amount", Amount.ToString("F2") },
                        { "Date", DateTime.Now.ToString("dd.MM.yyyy HH:mm") },
                        { "Sender", $"{sender.Surname} {sender.Name}" },
                        { "ReceiverCard", targetCard.CardNumber },
                        { "Purpose", _purpose }
                    };

                    Logger.AppendLog(
                        userEmail: receiver.Email,
                        templateName: "DepositReceipt",
                        text: $"Зарахування від {sender.Surname} {sender.Name}: {Amount} ₴",
                        data: receiverReceiptData
                    );

                    return true;
                }
                else
                {
                    Logger.Log($"[Транзакція {TransactionId}] Скасовано: недостатньо коштів або перевищено ліміт.");
                    return false;
                }
            }
        }
    }
}