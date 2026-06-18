using System;

namespace Banking_system.Models.Transactions
{
    internal class DepositTransaction : AbstractTransaction
    {
        private AbstractCard _targetCard = null!;
        protected DepositTransaction() { }
        public DepositTransaction(AbstractCard targetCard, decimal amount) : base(amount)
        {
            _targetCard = targetCard;
        }

        public override bool Execute()
        {
            using (var db = new Banking_system.DataBase.Database())
            {
                var card = db.Cards.FirstOrDefault(c => c.CardNumber == _targetCard.CardNumber);
                if (card != null)
                {
                    card.Deposit(Amount);
                    card.Operation(this);
                    db.SaveChanges();

                    // ДОДАНО: Логування для історії транзакцій
                    var user = db.Users.FirstOrDefault(u => u.ID == card.UserId);
                    if (user != null)
                    {
                        var receiptData = new Dictionary<string, string>
                {
                    { "Amount", Amount.ToString("F2") },
                    { "Date", DateTime.Now.ToString("dd.MM.yyyy HH:mm") },
                    { "Sender", "Термінал / Каса" },
                    { "ReceiverCard", card.CardNumber },
                    { "Purpose", string.IsNullOrEmpty(TransactionTarget) ? "Поповнення рахунку" : TransactionTarget },
                    { "TransactionId", TransactionId.ToString() }
                };
                        Banking_system.Models.Logger.AppendLog(user.Email, "DepositReceipt", $"Зарахування: {Amount:F2} ₴", receiptData);
                    }

                    return true;
                }
                return false;
            }
        }
    }
}