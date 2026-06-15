using System;
using System.Collections.Generic;
using System.Linq;

namespace Banking_system.Models.Transactions
{
    internal class TransferTransaction : AbstractTransaction
    {
        // Залишаємо лише один набір полів з ініціалізацією
        private readonly string _sourceCardNumber = string.Empty;
        private readonly string _targetCardNumber = string.Empty;
        private readonly string _purpose = string.Empty;
        private readonly string _comment = string.Empty;

        public TransferTransaction(string sourceCardNumber, string targetCardNumber, decimal amount, string purpose, string comment)
            : base(amount)
        {
            _sourceCardNumber = sourceCardNumber;
            _targetCardNumber = targetCardNumber;
            _purpose = purpose;
            _comment = comment;
        }

        public TransferTransaction(
            string sourceCardNumber,
            string targetCardNumber,
            decimal amount,
            string purpose,
            string transTarget,
            string description)
        : base(amount, transTarget, description)
        {
            _sourceCardNumber = sourceCardNumber;
            _targetCardNumber = targetCardNumber;
            _purpose = purpose;
            _comment = string.Empty; // Ініціалізуємо, щоб уникнути помилки Nullable Reference
        }

        protected TransferTransaction() { }

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
                    Logger.AppendSystemLog(Service.SessionManager.CurrentUser?.Email ?? "Unknown", "Невдала спроба переказу. Невірні реквізити картки.");
                    return false;
                }

                // Виконуємо логіку зняття та поповнення
                if (sourceCard.Withdraw(Amount))
                {
                    targetCard.Deposit(Amount);
                    sourceCard.Operation(this);
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
                        { "Purpose", _purpose },
                        { "Comment", string.IsNullOrWhiteSpace(_comment) ? "Без коментаря" : _comment }
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
                        { "Purpose", _purpose },
                        { "Comment", string.IsNullOrWhiteSpace(_comment) ? "Без коментаря" : _comment }
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