namespace Banking_system.Models.Transactions
{
    internal class WithdrawTransaction : AbstractTransaction
    {
        private AbstractCard _sourceCard = null!;
       
        protected WithdrawTransaction() { }
        public WithdrawTransaction(AbstractCard sourceCard, decimal amount, string targetCategory)
             : base(amount, targetCategory, "Зняття коштів/Оплата")
        {
            _sourceCard = sourceCard;
        }
        public override bool Execute()
        {
            using (var db = new Banking_system.DataBase.Database())
            {
                var card = db.Cards.FirstOrDefault(c => c.CardNumber == _sourceCard.CardNumber);

                if (card != null && card.Withdraw(Amount))
                {
                    card.Operation(this);
                    db.SaveChanges();

                    var user = db.Users.FirstOrDefault(u => u.ID == card.UserId);
                    if (user != null)
                    {
                        var receiptData = new Dictionary<string, string>
                {
                    { "Amount", Amount.ToString("F2") },
                    { "Date", DateTime.Now.ToString("dd.MM.yyyy HH:mm") },
                    { "CardNumber", card.CardNumber },
                    { "Balance", card.Balance.ToString("F2") },
                    { "Purpose", string.IsNullOrEmpty(TransactionTarget) ? "Зняття коштів / Оплата" : TransactionTarget },
                    { "TransactionId", TransactionId.ToString() }
                };
                        Banking_system.Models.Logger.AppendLog(user.Email, "WithdrawalReceipt", $"Витрата: {Amount:F2} ₴", receiptData);
                    }

                    return true;
                }
                return false;
            }
        }
    }
}